using System.Windows;

namespace adrilight_shared.Models.ControlMode.ModeParameters.ParameterValues
{
    public class BrushData
    {
        public BrushData(int id, Rect brush)
        {
            ID = id;
            Brush = brush;
        }
        public BrushData()
        {

        }
        public int ID { get; set; }
        public Rect Brush { get; set; }
    }
}