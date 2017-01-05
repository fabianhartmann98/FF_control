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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;

using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using FF_control.Bluetooth;

namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for Connection.xaml
    /// </summary>
    public partial class Connection : UserControl
    {
        public MainWindow parent { get; set; }
        public TabControl SideTabControl { get; set; }

        public Connection(MainWindow p, TabControl sidetabcontrol)
        {
            InitializeComponent();
            parent = p;
            SideTabControl = sidetabcontrol;

            parent.bt_connection.DeviceConnected += bt_DeviceConnected;
            parent.bt_connection.DiscoverDevicesEnded += bt_DiscoverDevicesEnded;
            parent.bt_connection.DeviceDisconnected += bt_DeviceDisconnected;

            this.IsVisibleChanged += Connection_IsVisibleChanged;

            parent.bt_connection.GetAvailableDevicesAsync();
        }

        void Connection_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
                setUpSideTabControl();
        }

        private void setUpSideTabControl()
        {
            SideTabControl.Items.Clear();

            TabItem ti = new TabItem();
            ti.Header = "1st Header";
            ti.Style = (Style)FindResource("Style_SideTabItem");

            WrapPanel wp = new WrapPanel();
            Label l = new Label();
            l.Content = "1st TabItem Content from Connection";
            wp.Children.Add(l);

            ti.Content = wp;
            SideTabControl.Items.Add(ti);


            ti = new TabItem();
            ti.Header = "2nd Header";
            ti.Style = (Style)FindResource("Style_SideTabItem");

            wp = new WrapPanel();
            l = new Label();
            l.Content = "2nd TabItem Content from Connection";
            wp.Children.Add(l);

            ti.Content = wp;

            SideTabControl.Items.Add(ti);
        }

        void bt_DiscoverDevicesEnded(object sender, EventArgs e)
        {
            if (!stackpanel.Dispatcher.CheckAccess())
            {
                stackpanel.Dispatcher.Invoke(
                    (Action<object, EventArgs>)bt_DiscoverDevicesEnded, sender, e);
            }
            else
            {
                stackpanel.Children.Clear();

                foreach (var item in parent.bt_connection.infos)
                {
                    Connection_DeviceModule cdm = new Connection_DeviceModule(item);
                    cdm.Dis_ConnectDevice += cdm_Dis_ConnectDevice;
                    stackpanel.Children.Add(cdm);
                }
            }
        }

        void cdm_Dis_ConnectDevice(object sender, RoutedEventArgs e)
        {
            Connection_DeviceModule cdm = (Connection_DeviceModule)((Button)sender).Tag;
            if (cdm.Device.Connected)
                parent.bt_connection.DisconnectFromDevice();
            else
                parent.bt_connection.ConnectToDevice(cdm.Device);           
        }

        void bt_DeviceConnected(object sender, EventArgs e)
        {
            stackpanel.Children.Clear();
            stackpanel.Children.Add(new Connection_DeviceModule(parent.bt_connection.ConnectedDevice));
        }

        void bt_DeviceDisconnected(object sender, EventArgs e)
        {
            parent.bt_connection.GetAvailableDevicesAsync();
        }

        private void button_refresh_Click(object sender, RoutedEventArgs e)
        {
            parent.bt_connection.GetAvailableDevicesAsync();
        }

        private void StackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            StackPanel sp = (StackPanel)sender;
            if (sp.ActualWidth < 600)
                sp.Orientation = Orientation.Vertical;
            else
                sp.Orientation = Orientation.Horizontal;
        }
    }
}
