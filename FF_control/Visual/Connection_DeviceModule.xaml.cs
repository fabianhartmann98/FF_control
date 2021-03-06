﻿using System;
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
using BluetoothUtilities;
using System.Windows.Media.Animation;


namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for Connection_DeviceModule.xaml
    /// </summary>
    public partial class Connection_DeviceModule : UserControl
    {
        public Connection_DeviceModule(BluetoothDeviceInfo device)
        {
            InitializeComponent();
            Device = device;
            button.Tag = this;
            DeviceName = device.DeviceName;
            DeviceAddress = device.DeviceAddress.ToString();
            ClassOfDevice = device.ClassOfDevice.Device.ToString();
            Connected = "not connected";
            if (device.Connected)
            {
                Connected = "Connected";
                button.Style = (Style)FindResource("Style_DisconnectButton");   //overwrite the default Style
                button.Content = (String)FindResource("s_DisconnectDevice");
            }
            wp.DataContext = this;      //set the data context (Layout uses Props (DeviceName, DeviceAddress, ClassOfDevice, Connected
        }

        public Connection_DeviceModule(BluetoothDeviceInfo device, bool connected) : this(device)
        {
            Connected = "not connected";
            if (connected)
            {
                Connected = "Connected";
                button.Style = (Style)FindResource("Style_DisconnectButton");   //overwrite the default Style
                button.Content = (String)FindResource("s_DisconnectDevice");
            }
        }

        public string DeviceName { get;private set; }
        public string DeviceAddress { get; private set; }
        public string ClassOfDevice { get; private set; }
        public string Connected { get; private set; }
        public BluetoothDeviceInfo Device { get; private set; }

        public event RoutedEventHandler Dis_ConnectDevice       //redirect button event
        {
            add { button.Click += value; }
            remove { button.Click -= value; }
        }

         
       
    }
}
