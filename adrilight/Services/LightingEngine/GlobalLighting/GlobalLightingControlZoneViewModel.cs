using adrilight_shared.Helpers;
using adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues;
using adrilight_shared.Models.Device.Zone.Spot;
using adrilight_shared.Models.Drawable;
using HandyControl.Tools.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace adrilight.Services.LightingEngine.GlobalLighting
{
    public class GlobalLightingControlZoneViewModel
    {
        #region Construct

        #endregion
        public GlobalLightingControlZoneViewModel(GlobalLightingControlZone controlZone)
        {
                ControlZone = controlZone;
        }

        #region Properties
        public GlobalLightingControlZone ControlZone { get; set; }

        private List<DeviceSpot> _spots;
        #endregion
        #region Methods
        #region Graphic Related Method
        public void ResetVIDStage()
        {
            foreach (var spot in _spots)
            {
                spot.HasVID = false;
                spot.SetVID(0);
            }
        }

        /// <summary>
        /// generate VID based on position
        /// </summary>
        /// <param name="startVID"></param>
        /// <param name="value"></param>
        /// <param name="intensity"></param>
        /// <param name="brushSize"></param>
        public void GenerateVID(int startVID, IParameterValue value, int intensity, int brushSize)
        {
           
        }
        /// <summary>
        /// apply VID preset
        /// </summary>
        /// <param name="parameterValue"></param>
        /// <param name="offSet"></param>
        public void ApplyPredefinedVID(IParameterValue parameterValue, int offSet)
        {
            var currentVIDData = parameterValue as VIDDataModel;
            if (currentVIDData == null)
                return;
            if (currentVIDData.DrawingPath == null)
                return;
            ResetVIDStage();

            for (var i = 0; i < currentVIDData.DrawingPath.Count(); i++)
            {
                var vid = currentVIDData.DrawingPath[i].ID;
                var brush = currentVIDData.DrawingPath[i].Brush;
                if (Rect.Intersect(ControlZone.GetRect, brush).IsEmpty)
                    continue;
                var intersectRect = Rect.Intersect(ControlZone.GetRect, brush);
                intersectRect.Offset(0 - ControlZone.GetRect.Left, 0 - ControlZone.GetRect.Top);
                foreach (var spot in _spots)
                {
                    spot.GetVIDIfNeeded(vid - offSet, intersectRect, 0);
                }
            }

        }
  
        #endregion
        #endregion
    }
}
