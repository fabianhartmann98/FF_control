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
    /// Interaction logic for Control.xaml
    /// </summary>
    public partial class Control : UserControl
    {
        public MainWindow parent { get; set; }

        public Control(MainWindow p)
        {
            parent = p;
            InitializeComponent();
            parent.bt_connection.DeviceConnected += bt_connection_DeviceConnected;
            parent.bt_connection.DeviceDisconnected += Bt_connection_DeviceDisconnected;
            parent.bt_connection.MaxGapReceived += bt_connection_MaxGapReceived;
            parent.bt_connection.PositionReceived += bt_connection_PositionReceived;
            parent.bt_connection.StatusReceived += bt_connection_StatusReceived;
            parent.bt_connection.ReferenzPlacementReceived += Bt_connection_ReferenzPlacementReceived;
            parent.bt_connection.StopReceived += Bt_connection_StopReceived;
            parent.bt_connection.RunReceived += Bt_connection_RunReceived;

            control_grid.IsEnabled = false; 
        }

        private void Bt_connection_DeviceDisconnected(object sender, EventArgs e)
        {
            control_grid.IsEnabled = false;
        }

        private void bt_connection_DeviceConnected(object sender, EventArgs e)
        {
            control_grid.IsEnabled = true;
        }

        private void Bt_connection_RunReceived(object sender, EventArgs e)
        {
            //todo: implement RunReceived
        }

        private void Bt_connection_StopReceived(object sender, EventArgs e)
        {
            //todo: implement StopReceived
        }

        private void Bt_connection_ReferenzPlacementReceived(object sender, EventArgs e)
        {
            //todo: implement ReferenzPlacementReceived
        }

        void bt_connection_StatusReceived(object sender, EventArgs e)           //a new Status was received -> update UI
        {
            //todo:
            //switch (parent.bt_connection.Lastupdated_status)
            //{
            //    case 0x01:
            //        break;
            //    case 0x02:
            //        break;
            //    case 0x03:
            //        break;
            //    default:
            //        break;
            //}
            LabelUpdate(l_laststatus, ((int)parent.bt_connection.Lastupdated_status).ToString());       //call invoker to set toe Status
        }

        void bt_connection_PositionReceived(object sender, EventArgs e)
        {
            LabelUpdate(l_lasposition, parent.bt_connection.Lastupdated_position.ToString());       //update Lastposition (use Invoker)
        }

        void bt_connection_MaxGapReceived(object sender, EventArgs e)       //the Answer to a MaxGapRequest was received -> update UI
        {
            LabelUpdate(l_maxgap, parent.bt_connection.Maxgap.ToString());         //update Maxgap (use Invoker)
        }

        private void b_gap_approve_Click(object sender, RoutedEventArgs e)
        {
            parent.bt_connection.SendMotorAdjusting(Math.Round(slider_gap.Value, 2)); //round to 2, because thats the most accurate which is going to be sent
        }

        private void b_refernez_Click(object sender, RoutedEventArgs e)
        {
            parent.bt_connection.SendReferenzPlacement();
        }

        private void b_status_Click(object sender, RoutedEventArgs e)
        {
            parent.bt_connection.SendStatusRequest();           //ask for a new Status
            parent.bt_connection.SendPositionRequest();         //ask for the Position (aim and actual)
        }

        private void b_maxgap_Click(object sender, RoutedEventArgs e)
        {
            parent.bt_connection.SendMaxGapRequest();           //ask for the maxgap which is able to set
        }

        private void LabelUpdate(Label l, string text)      //uses Invoke to get Access to the Label (sets the text, removes the old one)
        {
            if (!l.Dispatcher.CheckAccess())        //call invoke if needed
            {
                l.Dispatcher.Invoke((Action<Label, string>)LabelUpdate, l, text);
            }
            else
                l.Content = text;                   //else, set text
        }

        private void button_stop_Click(object sender, RoutedEventArgs e)
        {
            parent.bt_connection.SendStop();
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            double columnwidth = (double)FindResource("d_SideTabControlWidth");
            if (control_grid.ColumnDefinitions[control_grid.ColumnDefinitions.Count - 1].Width.Value >= columnwidth)
                columnwidth = 10;
            control_grid.ColumnDefinitions.RemoveAt(control_grid.ColumnDefinitions.Count - 1);
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = new GridLength(columnwidth);
            cd.MinWidth = columnwidth;
            control_grid.ColumnDefinitions.Add(cd);
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    slider_gap.Value = Convert.ToDouble(((TextBox)sender).Text.Replace('.',',')); //use , as Komma (seperator)to Convert
                }
                catch (Exception)
                {
                    ((TextBox)sender).Text = slider_gap.Value.ToString("F2").Replace(',','.');  //always display . as seperator (komma)
                }
            }
        }
    }
}
