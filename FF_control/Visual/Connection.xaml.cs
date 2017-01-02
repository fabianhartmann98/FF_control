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
using Bluetooth;

namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for Connection.xaml
    /// </summary>
    public partial class Connection : UserControl
    {
        BT_connection bt;

        public Connection()
        {
            InitializeComponent();

            bt = new BT_connection();
            bt.DeviceConnected += bt_DeviceConnected;

            Devices.ItemsSource = bt.GetAvailableDevices();
            Devices.SelectedIndex = 0;
        }

        private void bt_DeviceConnected(object sender, EventArgs e)
        {
            TextBoxUpdate(Receive_tb, "connected to Device");
        }

        private void TextBoxUpdate(TextBox textBox, string v)
        {
            if (!textBox.Dispatcher.CheckAccess())
            {
                textBox.Dispatcher.Invoke(
                     (Action<TextBox, string>)TextBoxUpdate, textBox, v);
            }
            else
            {
                textBox.Text += v;
            }
        }


        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            bt.ConnectToDevice(Devices.SelectedItem.ToString());
        }

        private void Send_b_Click(object sender, RoutedEventArgs e)
        {

            bt.SendStayingAlive();
            bt.SendPositionRequest();
            //bt.SendInit();
            //bt.SendMotorAdjusting(300);
        }
    }
}
