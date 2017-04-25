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
using BluetoothUtilities;
using System.Diagnostics;

namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for Connection.xaml
    /// </summary>
    public partial class Connection : UserControl
    {
        public MainWindow parent { get; set; }
        public string MyGif { get; set; }
        public Connection(MainWindow p)
        {
            InitializeComponent();
            parent = p;


            parent.bt_connection.DeviceConnected += bt_DeviceConnected;     //to react if the divice is connected
            parent.bt_connection.DiscoverDevicesEnded += bt_DiscoverDevicesEnded; //to get all available devices in range
            parent.bt_connection.DeviceDisconnected += bt_DeviceDisconnected; //if it wasn't able to reach ist
            parent.bt_connection.DeviceConnectedFailed += bt_connection_DeviceConnectedFailed; //if it wasn't able to connect to the device

            infinitygif.Visibility = Visibility.Visible; //set it to visible (because starting to search)
            parent.bt_connection.GetAvailableDevicesAsync();            //get de AvailableDevices Async 
        }

        void bt_connection_DeviceConnectedFailed(object sender, EventArgs e)
        {
            if (!infinitygif.Dispatcher.CheckAccess()) //setting the loading gif as collapsed (not visible)
            {
                infinitygif.Dispatcher.Invoke((Action<object, EventArgs>)bt_connection_DeviceConnectedFailed, sender, e);
            }
            else
            {
                infinitygif.Visibility = Visibility.Collapsed;
            }
        }        

        void bt_DiscoverDevicesEnded(object sender, EventArgs e)        //now able to get the devices 
        {
            if (!stackpanel.Dispatcher.CheckAccess())                   //if invoke needed
            {
                stackpanel.Dispatcher.Invoke(
                    (Action<object, EventArgs>)bt_DiscoverDevicesEnded, sender, e);
            }
            else
            {
                infinitygif.Visibility = Visibility.Collapsed;
                stackpanel.Children.Clear();        //creat new list with all the found devices

                foreach (var item in parent.bt_connection.infos)        //for each available Device, set up a new connection_DeviceModul
                {
                    Connection_DeviceModule cdm = new Connection_DeviceModule(item);
                    cdm.Dis_ConnectDevice += cdm_Dis_ConnectDevice;         //get whenn button to connect or disconnect is pressed
                    stackpanel.Children.Add(cdm);                           //add them to the stackpanel
                }
            }
        }

        void cdm_Dis_ConnectDevice(object sender, RoutedEventArgs e)        //connecting or disconnecting from device
        {
            infinitygif.Visibility = Visibility.Visible;
            Connection_DeviceModule cdm = (Connection_DeviceModule)((Button)sender).Tag;
            if (cdm.Device == parent.bt_connection.ConnectedDevice && cdm.Device.Connected)        //if it is the same device which is connected to the bt_connection  => disconnect
                parent.bt_connection.DisconnectFromDevice();
            if(!cdm.Device.Connected)                                                             // if the device is not connected
                parent.bt_connection.ConnectToDevice(cdm.Device.DeviceName);                      //connect to the Device if possible  
        }

        void bt_DeviceConnected(object sender, EventArgs e)
        {
            if (!stackpanel.Dispatcher.CheckAccess())       //if invoke needed
            {
                stackpanel.Dispatcher.Invoke((Action<object, EventArgs>)bt_DeviceConnected, null, new EventArgs());
            }
            else
            {
                infinitygif.Visibility = Visibility.Collapsed;
                stackpanel.Children.Clear();        //delete all Moduls, add the one which it is connected now
                stackpanel.Children.Add(new Connection_DeviceModule(parent.bt_connection.ConnectedDevice));
            }
 
        }

        void bt_DeviceDisconnected(object sender, EventArgs e)          //the connection is lost
        {
            infinitygif.Visibility = Visibility.Visible;
            scrollviewer.ScrollToBottom();
            parent.bt_connection.GetAvailableDevicesAsync();        //search for available devices
        }

        private void button_refresh_Click(object sender, RoutedEventArgs e)
        {
            infinitygif.Visibility = Visibility.Visible;
            scrollviewer.ScrollToBottom();
            parent.bt_connection.GetAvailableDevicesAsync();  //search for available devices
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            double columnwidth = (double)FindResource("d_SideTabControlWidth");
            if (connection_grid.ColumnDefinitions[connection_grid.ColumnDefinitions.Count - 1].Width.Value >= columnwidth)
                columnwidth = 10;
            connection_grid.ColumnDefinitions.RemoveAt(connection_grid.ColumnDefinitions.Count - 1);
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = new GridLength(columnwidth);
            cd.MinWidth = columnwidth;
            connection_grid.ColumnDefinitions.Add(cd);
        }
    }
}
