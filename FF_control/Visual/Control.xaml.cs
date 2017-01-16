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
            parent.bt_connection.MaxGapReceived += bt_connection_MaxGapReceived;
            parent.bt_connection.PositionReceived += bt_connection_PositionReceived;
            parent.bt_connection.StatusReceived += bt_connection_StatusReceived;
            parent.bt_connection.ReferenzPlacementReceived += Bt_connection_ReferenzPlacementReceived;
            parent.bt_connection.StopReceived += Bt_connection_StopReceived;
            parent.bt_connection.RunReceived += Bt_connection_RunReceived;

            this.IsVisibleChanged += Control_IsVisibleChanged;      //needed to set up the SideTabcontrol
        }

       

        void Control_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
                setUpSideTabControl();
        }

        private void setUpSideTabControl()
        {
            SideTabControl.Items.Clear();       //redo all of the TabItems

            TabItem ti = new TabItem();         //first Tabitem
            ti.Header = "1st Control Header";
            ti.Style = (Style)FindResource("Style_SideTabItem");

            WrapPanel wp = new WrapPanel();
            Label l = new Label();
            l.Content = "1st TabItem Content from Control";
            wp.Children.Add(l);

            ti.Content = wp;
            SideTabControl.Items.Add(ti);


            ti = new TabItem();                 //secont TabItem
            ti.Header = "2nd Control Header";
            ti.Style = (Style)FindResource("Style_SideTabItem");

            wp = new WrapPanel();
            l = new Label();
            l.Content = "2nd TabItem Content from Control";
            wp.Children.Add(l);

            ti.Content = wp;

            SideTabControl.Items.Add(ti);
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
    }
}
