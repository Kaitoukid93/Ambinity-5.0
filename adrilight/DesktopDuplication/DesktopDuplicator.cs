using adrilight.Extensions;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Drawing.Imaging;
using Windows.Graphics;
using Device = SharpDX.Direct3D11.Device;
using Format = SharpDX.DXGI.Format;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace adrilight.DesktopDuplication
{
    /// <summary>
    /// Provides access to frame-by-frame updates of a particular desktop (i.e. one monitor), with image and cursor information.
    /// </summary>
    public class DesktopDuplicator : IDisposable
    {
        private readonly Device _device;
        private OutputDescription _outputDescription;
        private readonly OutputDuplication _outputDuplication;

        private Texture2D _stagingTexture;
        private Texture2D _smallerTexture;
        private ShaderResourceView _smallerTextureView;
        private int _blackFrameCounter = 0;
        /// <summary>
        /// Duplicates the output of the specified monitor on the specified graphics adapter.
        /// </summary>
        /// <param name="whichGraphicsCardAdapter">The adapter which contains the desired outputs.</param>
        /// <param name="whichOutputDevice">The output device to duplicate (i.e. monitor). Begins with zero, which seems to correspond to the primary monitor.</param>
        public DesktopDuplicator(int whichGraphicsCardAdapter, int whichOutputDevice)
        {

            Adapter1 adapter;
            try
            {
                adapter = new Factory1().GetAdapter1(whichGraphicsCardAdapter);
            }
            catch (SharpDXException ex)
            {
                // throw new DesktopDuplicationException("Could not find the specified graphics card adapter.", ex);
                return;
            }
            _device = new Device(adapter);
            Output output;
            try
            {
                output = adapter.GetOutput(whichOutputDevice);
            }
            catch (SharpDXException ex)
            {
                //if (ex.ResultCode == SharpDX.DXGI.ResultCode.NotFound)
                //{

                //    // HandyControl.Controls.MessageBox.Show(" Không thể capture màn hình " + (whichOutputDevice + 1).ToString(), "Screen Capture", MessageBoxButton.OK, MessageBoxImage.Warning,);

                //    output = adapter.GetOutput(0);

                //}
                //else
                //{
                // throw new DesktopDuplicationException("Unknown Device Error", ex);
                // }
                return;



            }
            var output1 = output.QueryInterface<Output1>();
            _outputDescription = output.Description;
            lastSize.Width = _outputDescription.DesktopBounds.GetWidth();
            lastSize.Height = _outputDescription.DesktopBounds.GetHeight();

            try
            {
                _outputDuplication = output1.DuplicateOutput(_device);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.NotCurrentlyAvailable.Result.Code)
                {
                    throw new DesktopDuplicationException(
                        "There is already the maximum number of applications using the Desktop Duplication API running, please close one of the applications and try again.");
                }
                else if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.AccessDenied.Result.Code)
                {
                    //Dispose();
                    //throw new DesktopDuplicationException("Access Denied");
                }
                else
                {
                    Dispose();
                    GC.Collect();
                    //retry right here??
                    //throw new Exception("Unknown, just retry");



                }


            }


        }

        //  private static readonly FpsLogger _desktopFrameLogger = new FpsLogger("DesktopDuplication");


        /// <summary>
        /// Retrieves the latest desktop image and associated metadata.
        /// </summary>
        public ByteFrame GetLatestFrame()
        {
            // Try to get the latest frame; this may timeout
            var succeeded = RetrieveFrame();
            if (!succeeded)
                return null;

            // _desktopFrameLogger.TrackSingleFrame();

            return ProcessFrame();

        }

        private const int mipMapLevel = 3;
        private const int scalingFactor = 1 << mipMapLevel;
        private SizeInt32 lastSize;
        private bool RetrieveFrame()
        {

            int desktopWidth;
            int desktopHeight;
            var newSize = false;
            if (_outputDescription.DesktopBounds.GetWidth() != lastSize.Width ||
                    _outputDescription.DesktopBounds.GetHeight() != lastSize.Height)
            {
                // The thing we have been capturing has changed size.
                // We need to resize the swap chain first, then blit the pixels.
                // After we do that, retire the frame and then recreate the frame pool.
                newSize = true;
                lastSize.Width = _outputDescription.DesktopBounds.GetWidth();
                lastSize.Height = _outputDescription.DesktopBounds.GetHeight();
            }

            if (_outputDescription.Rotation == DisplayModeRotation.Rotate90 || _outputDescription.Rotation == DisplayModeRotation.Rotate270)
            {

                desktopWidth = _outputDescription.DesktopBounds.GetHeight();
                desktopHeight = _outputDescription.DesktopBounds.GetWidth();
            }
            else  //landscape mode
            {

                desktopWidth = _outputDescription.DesktopBounds.GetWidth();
                desktopHeight = _outputDescription.DesktopBounds.GetHeight();
            }
            if (newSize)
            {
                _smallerTexture?.Dispose();
                _smallerTextureView?.Dispose();
                _stagingTexture?.Dispose();
                _smallerTexture = null;
                _smallerTextureView = null;
                _stagingTexture = null;
            }
            if (_stagingTexture == null)
            {
                try
                {
                    _stagingTexture = new Texture2D(_device, new Texture2DDescription() {
                        CpuAccessFlags = CpuAccessFlags.Read,
                        BindFlags = BindFlags.None,
                        Format = Format.B8G8R8A8_UNorm,
                        Width = desktopWidth / scalingFactor,
                        Height = desktopHeight / scalingFactor,
                        OptionFlags = ResourceOptionFlags.None,
                        MipLevels = 1,
                        ArraySize = 1,
                        SampleDescription = { Count = 1, Quality = 0 },
                        Usage = ResourceUsage.Staging // << can be read by CPU
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception("_stagingTextureProblem");

                }

            }
            SharpDX.DXGI.Resource desktopResource;
            try
            {
                if (_outputDuplication == null) throw new Exception("_outputDuplication is null");
                _outputDuplication.TryAcquireNextFrame(500, out var frameInformation, out desktopResource);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    return false;
                }

                throw new DesktopDuplicationException("Failed to acquire next frame.", ex);
            }
            // if (desktopResource == null) throw new Exception("desktopResource is null");
            try
            {
                if (_smallerTexture == null)
                {
                    _smallerTexture = new Texture2D(_device, new Texture2DDescription {
                        CpuAccessFlags = CpuAccessFlags.None,
                        BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                        Format = Format.B8G8R8A8_UNorm,
                        Width = desktopWidth,
                        Height = desktopHeight,
                        OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                        MipLevels = mipMapLevel + 1,
                        ArraySize = 1,
                        SampleDescription = { Count = 1, Quality = 0 },
                        Usage = ResourceUsage.Default
                    });
                    _smallerTextureView = new ShaderResourceView(_device, _smallerTexture);
                }
            }

            catch (Exception ex)
            {
                throw new Exception("_smallerTextureProblem");
            }
            try
            {
                using (var tempTexture = desktopResource?.QueryInterface<Texture2D>())
                {
                    if (_device == null) throw new Exception("_device is null");
                    if (_device.ImmediateContext == null) throw new Exception("_device.ImmediateContext is null");

                    _device.ImmediateContext.CopySubresourceRegion(tempTexture, 0, null, _smallerTexture, 0);
                }

                _outputDuplication.ReleaseFrame();

                // Generates the mipmap of the screen
                _device.ImmediateContext.GenerateMips(_smallerTextureView);

                // Copy the mipmap 1 of smallerTexture (size/2) to the staging texture
                _device.ImmediateContext.CopySubresourceRegion(_smallerTexture, mipMapLevel, null, _stagingTexture, 0);

                desktopResource?.Dispose(); //perf?
            }
            catch (Exception ex)
            {
                _outputDuplication?.Dispose();

                return false;
            }
            _blackFrameCounter = 0;
            return true;
        }

        private ByteFrame ProcessFrame()
        {
            // Get the desktop capture texture
            var mapSource = _device.ImmediateContext.MapSubresource(_stagingTexture, 0, MapMode.Read, MapFlags.None);
            //Rectangle frame;
            int height;
            int width;
            if (_outputDescription.Rotation == DisplayModeRotation.Rotate90 || _outputDescription.Rotation == DisplayModeRotation.Rotate270)
            {

                width = _outputDescription.DesktopBounds.GetHeight() / scalingFactor;
                height = _outputDescription.DesktopBounds.GetWidth() / scalingFactor;
            }
            else  //landscape mode
            {

                width = _outputDescription.DesktopBounds.GetWidth() / scalingFactor;
                height = _outputDescription.DesktopBounds.GetHeight() / scalingFactor;
            }
            // Copy pixels from screen capture Texture to GDI bitmap
            var sourcePtr = mapSource.DataPointer;
            int bitsPerPixel = ((int)PixelFormat.Format32bppRgb & 0xff00) >> 8;
            int bytesPerPixel = (bitsPerPixel + 7) / 8;
            int stride = 4 * ((width * bytesPerPixel + 3) / 4);
            int bytes = Math.Abs(stride) * height;
            byte[] rgbValues = new byte[bytes];

            for (int y = 0; y < height; y++)
            {
                // Copy a single line 
                System.Runtime.InteropServices.Marshal.Copy(sourcePtr, rgbValues, y * stride, width * 4);
                sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);

            }

            try
            {
                _device.ImmediateContext.UnmapSubresource(_stagingTexture, 0);
            }
            catch (Exception ex)
            {
                return null;
            }
            var frame = new ByteFrame() {
                Frame = rgbValues,
                FrameWidth = width,
                FrameHeight = height
            };
            return frame;
        }

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
            _smallerTexture?.Dispose();
            _smallerTextureView?.Dispose();
            _stagingTexture?.Dispose();
            _outputDuplication?.Dispose();
            _device?.Dispose();

            GC.Collect();
        }
    }
}