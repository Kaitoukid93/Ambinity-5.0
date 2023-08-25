using adrilight.Helpers;
using adrilight.ViewModel;
using HandyControl.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TimeLineTool;

namespace adrilight.View
{
    /// <summary>
    /// Interaction logic for PaletteEditWindow.xaml
    /// </summary>
    public partial class CompositionEditWindow
    {
        ObservableCollection<ITimeLineDataItem> data = new ObservableCollection<ITimeLineDataItem>();
        public ObservableCollection<ITimeLineDataItem> t2Data = new ObservableCollection<ITimeLineDataItem>();
        public ObservableCollection<ITimeLineDataItem> t3Data = new ObservableCollection<ITimeLineDataItem>();
        public ObservableCollection<ITimeLineDataItem> t4Data = new ObservableCollection<ITimeLineDataItem>();
        public ObservableCollection<ITimeLineDataItem> t5Data = new ObservableCollection<ITimeLineDataItem>();
        ObservableCollection<ITimeLineDataItem> listboxData = new ObservableCollection<ITimeLineDataItem>();
        public CompositionEditWindow()
        {
            InitializeComponent();

        //    var tmp1 = new TempDataType() {
        //        StartFrame = 0.0,
        //        EndFrame = 120.0,
        //        OriginalDuration = 120.0,
        //        Name = "Temp 1"
        //    };
        //    var tmp2 = new TempDataType() {
        //        StartFrame = 0.0,
        //        EndFrame = 120.0,
        //        OriginalDuration = 120.0,
        //        Name = "Temp 2"
        //    };
        //    var temp3 = new TempDataType() {
        //        StartFrame = 0.0,
        //        EndFrame = 120.0,
        //        OriginalDuration = 120.0,
        //        Name = "Temp 3"
        //    };
        //    var temp4 = new TempDataType() {
        //        StartFrame = 120.0,
        //        EndFrame = 150.0,
        //        OriginalDuration = 30.0,
        //        Name = "Temp 4"
        //    };

        //    data.Add(tmp1);
        //    data.Add(tmp2);
        //    data.Add(temp3);
        //    data.Add(temp4);

        //    t2Data.Add(tmp1);


        //    //TimeLine2.Items = data;
        //    TimeLine2.StartFrame = 0.0;
        //    TimeLine3.StartFrame = 0.0;
        //    TimeLine4.StartFrame = 0.0;
        //    TimeLine5.StartFrame = 0.0;
        //    TimeLine2.Items = t2Data;
        //    TimeLine3.Items = t3Data;
        //    TimeLine4.Items = t4Data;
        //    TimeLine5.Items = t5Data;


        //    var lb1 = new TempDataType() {
        //        Name = "ListBox 1"
        //    };
        //    var lb2 = new TempDataType() {
        //        Name = "ListBox 2"
        //    };
        //    var lb3 = new TempDataType() {
        //        Name = "ListBox 3"
        //    };
        //    var lb4 = new TempDataType() {
        //        Name = "ListBox 4"
        //    };
        //    listboxData.Add(lb1);
        //    listboxData.Add(lb2);
        //    listboxData.Add(lb3);
        //    listboxData.Add(lb4);
        //    ListSrc.ItemsSource = listboxData;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*var tli = TimeLine1.Items[TimeLine1.Items.Count - 1] as TimeLineItemControl;
			var adder = new TimeLineItemControl()
			{
				StartTime = tli.EndTime.AddHours(15),
				EndTime = tli.EndTime.AddHours(30),
				ViewLevel = TimeLine1.ViewLevel,
				Content = new Button(){Content=(TimeLine1.Items.Count+1).ToString()}
			};
			ctrls.Add(adder);*/
            /*if (TimeLine1.ViewLevel == TimeLineViewLevel.Hours)
			{
				TimeLine1.ViewLevel = TimeLineViewLevel.Minutes;
			}
			else
			{
				TimeLine1.ViewLevel = TimeLineViewLevel.Hours;
			}*/
        }


        private void Slider_Scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //TimeLine3.UnitSize = Slider_Scale.Value;
            //TimeLine2.UnitSize = Slider_Scale.Value;
            //TimeLine4.UnitSize = Slider_Scale.Value;
            //TimeLine5.UnitSize = Slider_Scale.Value;
        }
    

}
}
