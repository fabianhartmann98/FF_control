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
        public BT_connection bt;

        public Connection()
        {
            InitializeComponent();
            bt = new BT_connection();
            bt.DeviceConnected += bt_DeviceConnected;
            bt.DiscoverDevicesEnded += bt_DiscoverDevicesEnded;
            bt.DeviceDisconnected += bt_DeviceDisconnected;

            bt.GetAvailableDevicesAsync();
        }

        void bt_DeviceDisconnected(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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

                foreach (var item in bt.infos)
                {
                    Grid g = new Grid();
                    Border b = new Border();
                    //b.Style = (Style)FindResource("Style_ModulBorder");
                    WrapPanel wp = new WrapPanel();
                    Label l_name = new Label();
                    l_name.Content = item.DeviceName;
                    Label l_address = new Label();
                    l_address.Content = item.DeviceAddress;
                    Label l_devicetype = new Label();
                    l_address.Content = item.ClassOfDevice.Device.ToString();
                    Label l_connected = new Label();
                    l_connected.Content = "not connected";
                    if (item.Connected)
                        l_connected.Content = "Connected";
                    wp.Children.Add(l_name);
                    wp.Children.Add(l_address);
                    wp.Children.Add(l_devicetype);
                    wp.Children.Add(l_connected);
                    wp.Margin = new Thickness(5);

                    g.Children.Add(b);
                    g.Children.Add(wp);

                    stackpanel.Children.Add(g);
                }
            }
        }

        void bt_DeviceConnected(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void button_refresh_Click(object sender, RoutedEventArgs e)
        {
            bt.GetAvailableDevicesAsync();
        }
    }
}
