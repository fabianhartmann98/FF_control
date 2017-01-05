using FF_control.Bluetooth;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for Table.xaml
    /// </summary>
    public partial class Table : UserControl
    {
        public MainWindow parent { get; set; }
        public TabControl SideTabControl { get; set; }

        public Table(MainWindow p, TabControl sidetab)
        {
            InitializeComponent();
            parent = p;
            SideTabControl = sidetab;

            parent.bt_connection.MeasuredDataReceived += bt_connection_MeasuredDataReceived;
        }

        void bt_connection_MeasuredDataReceived(object sender, EventArgs e)
        {
            ReceivedData_EventArgs args = (ReceivedData_EventArgs)e;
            double number = (double)args.ArrayL[0];
            double time = (double)args.ArrayL[1];
            double actvalue = (double)args.ArrayL[2];

            int index = parent.diagram.Grpahs.Count - 1;

            if(number==0)
            {
                parent.diagram.Grpahs.Add(new Measure.Graph());
                index = parent.diagram.Grpahs.Count - 1;
                parent.diagram.Grpahs[index].MeasurementTime=DateTime.Now;
            }
            parent.diagram.Grpahs[index].mps.Add(new Measure.MeasurementPoint(actvalue, time, Convert.ToInt32(number)));
            parent.v_plot.DrawDiagram();
        }
    }
}
