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
            parent.bt_connection.MaxGapRecieved += bt_connection_MaxGapRecieved;
            parent.bt_connection.PositionReceived += bt_connection_PositionReceived;
            parent.bt_connection.StatusReceived += bt_connection_StatusReceived;

            this.IsVisibleChanged += Control_IsVisibleChanged;
        }

        void Control_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
                setUpSideTabControl();
        }

        private void setUpSideTabControl()
        {
            SideTabControl.Items.Clear();

            TabItem ti = new TabItem();
            ti.Header = "1st Control Header";
            ti.Style = (Style)FindResource("Style_SideTabItem");

            WrapPanel wp = new WrapPanel();
            Label l = new Label();
            l.Content = "1st TabItem Content from Control";
            wp.Children.Add(l);

            ti.Content = wp;
            SideTabControl.Items.Add(ti);


            ti = new TabItem();
            ti.Header = "2nd Control Header";
            ti.Style = (Style)FindResource("Style_SideTabItem");

            wp = new WrapPanel();
            l = new Label();
            l.Content = "2nd TabItem Content from Control";
            wp.Children.Add(l);

            ti.Content = wp;

            SideTabControl.Items.Add(ti);
        }

        void bt_connection_StatusReceived(object sender, EventArgs e)
        {
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
            LabelUpdate(l_laststatus, ((int)parent.bt_connection.Lastupdated_status).ToString());
        }

        void bt_connection_PositionReceived(object sender, EventArgs e)
        {
            LabelUpdate(l_lasposition, parent.bt_connection.Lastupdated_position.ToString());
        }

        void bt_connection_MaxGapRecieved(object sender, EventArgs e)
        {
            LabelUpdate(l_maxgap, parent.bt_connection.Maxgap.ToString());
        }

        private void b_gap_approve_Click(object sender, RoutedEventArgs e)
        {
            parent.bt_connection.SendMotorAdjusting(Math.Round(slider_gap.Value, 2));
        }

        private void b_refernez_Click(object sender, RoutedEventArgs e)
        {
            //todo: implement Referenz 
        }

        private void b_status_Click(object sender, RoutedEventArgs e)
        {
            parent.bt_connection.SendStatusRequest();
            parent.bt_connection.SendPositionRequest();
        }

        private void b_maxgap_Click(object sender, RoutedEventArgs e)
        {
            parent.bt_connection.SendMaxGapRequest();
        }

        private void LabelUpdate(Label l, string text)
        {
            if (!l.Dispatcher.CheckAccess())
            {
                l.Dispatcher.Invoke((Action<Label, string>)LabelUpdate, l, text);
            }
            else
                l.Content = text;
        }
    }
}
