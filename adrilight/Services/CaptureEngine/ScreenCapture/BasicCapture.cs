using adrilight_shared.Models.FrameData;
using Serilog;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.UI.Composition;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace adrilight.Services.CaptureEngine.ScreenCapture
{
    public class BasicCapture : IDisposable
    {
        private GraphicsCaptureItem item;
        private Direct3D11CaptureFramePool framePool;
        private GraphicsCaptureSession session;
        private SizeInt32 lastSize;
        private Texture2D _stagingTexture;
        private Texture2D _smallerTexture;
        private ShaderResourceView _smallerTextureView;
        private IDirect3DDevice device;
        private SharpDX.Direct3D11.Device d3dDevice;
        private SwapChain1 swapChain;
        private bool _shouldBeRuning = false;
        private const int mipMapLevel = 3;
        private const int scalingFactor = 1 << mipMapLevel;
        public object Lock { get; } = new object();
        public BasicCapture(IDirect3DDevice d, GraphicsCaptureItem i)
        {
            item = i;
            device = d;
            d3dDevice = Direct3D11Helper.CreateSharpDXDevice(device);
            _shouldBeRuning = true;
            var dxgiFactory = new Factory2();
            var description = new SwapChainDescription1() {
                Width = item.Size.Width,
                Height = item.Size.Height,
                Format = Format.B8G8R8A8_UNorm,
                Stereo = false,
                SampleDescription = new SampleDescription() {
                    Count = 1,
                    Quality = 0
                },
                Usage = Usage.RenderTargetOutput,
                BufferCount = 2,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipSequential,
                AlphaMode = AlphaMode.Premultiplied,
                Flags = SwapChainFlags.None
            };
            swapChain = new SwapChain1(dxgiFactory, d3dDevice, ref description);

            framePool = Direct3D11CaptureFramePool.CreateFreeThreaded(
                device,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2,
                i.Size);
            session = framePool.CreateCaptureSession(i);
            lastSize = i.Size;

            framePool.FrameArrived += OnFrameArrived;
        }

        public void Dispose()
        {
            session?.Dispose();
            framePool?.Dispose();
            swapChain?.Dispose();
            d3dDevice?.Dispose();
        }
        public void StopProcessing()
        {
            _shouldBeRuning = false;
        }
        public void ResumeProcessing()
        {
            _shouldBeRuning = true;
        }
        public void StartCaptureWithBorder()
        {
            session.IsCursorCaptureEnabled = false;
            session.StartCapture();
        }
        public async Task StartCaptureBorderless()
        {
            await GraphicsCaptureAccess.RequestAccessAsync(GraphicsCaptureAccessKind.Borderless);
            session.IsBorderRequired = false;
            session.IsCursorCaptureEnabled = false;
            session.StartCapture();
        }
        public ICompositionSurface CreateSurface(Compositor compositor)
        {
            return compositor.CreateCompositionSurfaceForSwapChain(swapChain);
        }
        public ByteFrame CurrentFrame { get; set; }
        private void CopyTexture(Texture2D texture)
        {
            // Create a CPU-accessible staging texture and copy the captured frame to it
            if (_stagingTexture == null)
            {
                try
                {
                    _stagingTexture = new Texture2D(d3dDevice, new Texture2DDescription() {
                        CpuAccessFlags = CpuAccessFlags.Read,
                        BindFlags = BindFlags.None,
                        Format = Format.B8G8R8A8_UNorm,
                        Width = lastSize.Width / scalingFactor,
                        Height = lastSize.Height / scalingFactor,
                        OptionFlags = ResourceOptionFlags.None,
                        MipLevels = 1,
                        ArraySize = 1,
                        SampleDescription = { Count = 1, Quality = 0 },
                        Usage = ResourceUsage.Staging // << can be read by CPU
                    });
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString() + "Staging Texture");
                    return;
                }

            }
            try
            {
                if (_smallerTexture == null)
                {
                    _smallerTexture = new Texture2D(d3dDevice, new Texture2DDescription {
                        CpuAccessFlags = CpuAccessFlags.None,
                        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                        Format = Format.B8G8R8A8_UNorm,
                        Width = lastSize.Width,
                        Height = lastSize.Height,
                        OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                        MipLevels = mipMapLevel + 1,
                        ArraySize = 1,
                        SampleDescription = { Count = 1, Quality = 0 },
                        Usage = ResourceUsage.Default
                    });
                    _smallerTextureView = new ShaderResourceView(d3dDevice, _smallerTexture);
                }
            }

            catch (Exception ex)
            {
                Log.Error(ex.ToString() + "SmallerTexture Texture");
                return;
            }


            d3dDevice.ImmediateContext.CopySubresourceRegion(texture, 0, null, _smallerTexture, 0);
            // Generates the mipmap of the screen
            d3dDevice.ImmediateContext.GenerateMips(_smallerTextureView);
            // Copy the mipmap 1 of smallerTexture (size/2) to the staging texture
            d3dDevice.ImmediateContext.CopySubresourceRegion(_smallerTexture, mipMapLevel, null, _stagingTexture, 0);
            // Map the resource using 'MapFlags.None' -> this call waits until it is completed and the data is accessible
            // This takes up the majority of the time and CPU usage
        }
        private ByteFrame ProcessFrame()
        {
            // Get the desktop capture texture
            var mapSource = d3dDevice.ImmediateContext.MapSubresource(_stagingTexture, 0, MapMode.Read, MapFlags.None);
            //Rectangle frame;
            var height = lastSize.Height / 8;
            var width = lastSize.Width / 8;

            // Copy pixels from screen capture Texture to GDI bitmap
            var sourcePtr = mapSource.DataPointer;
            var bitsPerPixel = ((int)PixelFormat.Format32bppRgb & 0xff00) >> 8;
            var bytesPerPixel = (bitsPerPixel + 7) / 8;
            var stride = 4 * ((width * bytesPerPixel + 3) / 4);
            var bytes = Math.Abs(stride) * height;
            var rgbValues = new byte[bytes];
            if (mapSource.RowPitch == stride)
            {
                //fast copy
                System.Runtime.InteropServices.Marshal.Copy(sourcePtr, rgbValues, 0, bytes);
            }
            else
            {
                //safe copy
                for (var y = 0; y < height; y++)
                {
                    // Copy a single line 
                    System.Runtime.InteropServices.Marshal.Copy(sourcePtr, rgbValues, y * stride, width * 4);
                    sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);

                }
            }
            try
            {
                d3dDevice.ImmediateContext.UnmapSubresource(_stagingTexture, 0);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString() + "Staging Texture");
                return null;
            }
            var frame = new ByteFrame() {
                Frame = rgbValues,
                FrameWidth = width,
                FrameHeight = height
            };
            return frame;
        }
        private void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
        {

            var newSize = false;

            using (var frame = framePool.TryGetNextFrame())
            {
                if (frame.ContentSize.Width != lastSize.Width ||
                    frame.ContentSize.Height != lastSize.Height)
                {
                    // The thing we have been capturing has changed size.
                    // We need to resize the swap chain first, then blit the pixels.
                    // After we do that, retire the frame and then recreate the frame pool.
                    newSize = true;
                    lastSize = frame.ContentSize;
                    swapChain.ResizeBuffers(
                        2,
                        lastSize.Width,
                        lastSize.Height,
                        Format.B8G8R8A8_UNorm,
                        SwapChainFlags.None);
                }

                //  using (var backBuffer = swapChain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0))
                using (var bitmap = Direct3D11Helper.CreateSharpDXTexture2D(frame.Surface))
                {
                    //d3dDevice.ImmediateContext.CopyResource(bitmap, backBuffer);
                    if (newSize)
                    {
                        _smallerTexture?.Dispose();
                        _smallerTextureView?.Dispose();
                        _stagingTexture?.Dispose();
                        _smallerTexture = null;
                        _smallerTextureView = null;
                        _stagingTexture = null;
                    }
                    CopyTexture(bitmap);
                }
                //process frame
            } // Retire the frame.

            //swapChain.Present(0, SharpDX.DXGI.PresentFlags.None);

            if (newSize)
            {
                framePool.Recreate(
                    device,
                    DirectXPixelFormat.B8G8R8A8UIntNormalized,
                    2,
                    lastSize);
            }
            if (_shouldBeRuning)
            {
                lock (Lock)
                {
                    CurrentFrame = ProcessFrame();
                }
            }


        }

    }
}
