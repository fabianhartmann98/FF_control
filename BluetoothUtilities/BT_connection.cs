﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.IO;
using System.Timers;
using System.Collections;
using System.Windows;

namespace BluetoothUtilities
{
    public class BT_connection
    {
        const string Logger_Error = "\t ||Error";
        const string Logger_DeviceDisconnected = "\t ||DeviceDisconnected";
        const string Logger_DeviceConnected = "\t ||DeviceConnected";
        const string Logger_DeviceConnectedFailed = "\t ||DeviceConnectedFailed";
        const string Logger_ReceiveError = "\t ||ReceiveError";
        const string Logger_ReceiveData = "\t ||ReceiveData";
        const string Logger_SendData = "\t ||SendData";


        private void Logger(String lines)
        {
            string applicationdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // Write the string to a file.append mode is enabled so that the log
            // lines get appended to  test.txt than wiping content and writing the log
            System.IO.StreamWriter file = new System.IO.StreamWriter(applicationdata + @"\FFControl\Log.txt", true);
            file.Write(DateTime.Now.ToString() + ": ");
            file.WriteLine(lines);
            file.Close();
        }

        private void DeletLogger()
        {
            string applicationdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            Directory.CreateDirectory(applicationdata + @"\FFControl");
            System.IO.StreamWriter file = new System.IO.StreamWriter(applicationdata+@"\FFControl\Log.txt",false);
            file.Write("");
            file.Close();
        }
        
        public double Lastupdated_position {get; private set;}      //saves the gap in mm
        public byte Lastupdated_status{get; private set;}
        public double Maxgap { get; private set; }                                       //saves the max gap in mm
        public long BytesSend { get; set; }
        public long BytesReceived { get; set; }
        public bool Stopped_Movement { get; set; } = false;


        public BluetoothClient bc {get; private set;}                 //the client which it is going to be connected to 
        public BluetoothDeviceInfo[] infos { get; private set; }        //all available DeviceInfos 
        public BluetoothDeviceInfo ConnectedDevice { get; private set; }         //the deviceInfo of the connected device
        Stream s;                           //the stream to write and read on   
        const int buf_len = 2048;            //the buffer Length of the RX
        byte[] RX_buf = new byte[buf_len];  //the buffer for RX 
        int rx_head = 0;                    //what we have already read (should always be 0)
        int rx_tail = 0;                    //the length in the array thath is filled but not read yet 

        const int MaxMinutesAsAvailable = 2;

        private System.Threading.Thread ConnectThread;

        static byte[] crc_table = new byte[256];        //the table to compute te crc8
         // x8 + x7 + x6 + x4 + x2 + 1
        const byte crc_poly = 0xd5;                     //the key for the crc8 = 111010101=0x1d5

        
        private string pin = "2017";                    //the pin to connect to the BT-Modul
        public string Pin
        {
            get { return pin; }
            set { pin = value; }
        }

        private double aim_position;        //saves the gap in mm
        /// <summary>
        /// aim position in mm
        /// </summary>
        public double Aim_position
        {
            get { return aim_position; }
            set
            {
                aim_position = value;
                SendMotorAdjusting(Convert.ToInt32(aim_position * BT_Protocoll.MMtoSendFormat));
            }
        }

        #region staying alive
        Timer staying_alive_timer;
        int count_missing_StayingAlive;
        const int max_count_missing_StayingAlive = 20;
        const double staying_alive_timer_interval = 1000;

        /// <summary>
        /// is starting a new timer to send staying alive every staying_alive_timer_interval
        /// </summary>
        public void startStayingAliveTimer()
        {
            staying_alive_timer = new Timer(staying_alive_timer_interval);
            staying_alive_timer.Elapsed += staying_alive_timer_Elapsed;
            staying_alive_timer.Start();
        }

        /// <summary>
        /// Sends a staying alive
        /// when more than max_count_missing_StayingAlive send and didn't receive anything
        /// ->OnDiviceDisconnected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void staying_alive_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            count_missing_StayingAlive++;                                       //to determin when the last stayingAliveAnswer was received
            if (count_missing_StayingAlive > max_count_missing_StayingAlive)    //to long since one received -> Disconnect
            {
                Logger("Too many missed StayingAlive -> disconnect"+Logger_DeviceDisconnected);
                DisconnectFromDevice();
            }
            else
                SendStayingAlive();
        }
        #endregion

        public BT_connection()
        {
            DeletLogger();      //clear Log 
            crc_CreateTable(); 
            //todo: activate StayinAliveTimer

            aim_position = -1;
            Lastupdated_position = -1;
            Lastupdated_status = 0xFF;
            Maxgap = -1; 
        }

        #region DiscoverDeviced
        /// <summary>
        /// getting al the available Devices in the area 
        /// uses DiscoverDevices
        /// </summary>
        /// <returns>string array with every name of available devices</returns>
        public string[] GetAvailableDeviceNames()
        {
            bc = new BluetoothClient();
            infos = null;
            infos = bc.DiscoverDevices(255, false, true, true);
            string[] names = new string[infos.Length];
            for (int i = 0; i < infos.Length; i++)
            {
                names[i] = infos[i].DeviceName;
            }
            return names;
        }

        /// <summary>
        /// getting all the available Devices in the area
        /// need to subscribe to DiscoverDevicesEnded
        /// get info out of Infos
        /// </summary>
        public void GetAvailableDevicesAsync()
        {
            bc = new BluetoothClient();
            infos = null;
            bc.BeginDiscoverDevices(255, false, true, true,false, getavailabledevicesasync_calback, bc);
        }

        private void getavailabledevicesasync_calback(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                var infos_ = bc.EndDiscoverDevices(ar);
                int i = 0; 
                foreach (var item in infos_)
                {
                    if ((DateTime.Now.ToUniversalTime()-item.LastSeen.ToUniversalTime()).TotalMinutes< MaxMinutesAsAvailable||item.Connected)
                        i++;
                }
                infos = new BluetoothDeviceInfo[i];
                i = 0; 
                foreach (var item in infos_)
                {
                    if ((DateTime.Now.ToUniversalTime() - item.LastSeen.ToUniversalTime()).TotalMinutes < MaxMinutesAsAvailable||item.Connected)
                    {
                        infos[i] = item;
                        i++;
                    }
                }
                OnDiscoverDevicesEnded(); //call event
            }
        }
        #endregion

        #region Reading
        /// <summary>
        /// end the reading and adds up to the rx_tail 
        /// calls DataManager 
        /// calls BeginRead again
        /// </summary>
        /// <param name="ar"></param>
        private void beginRead_cal(IAsyncResult ar)
        {
            if (!s.CanRead)
                return;
            int read = s.EndRead(ar);
            rx_tail += read;
            BytesReceived += read;
            DataManager();

            s.BeginRead(RX_buf, rx_tail, buf_len-rx_tail, beginRead_cal, s);            
            //starting at rx_tail (doesn't have to be 0)
            //buf_len -rx-tail is the remaining free space 
            //call this again if received something
        }

        /// <summary>
        /// the input % buf_len so no out of Range can appear
        /// </summary>
        /// <param name="i">index of where data should come from</param>
        /// <returns>value in the RX_Buf at the index</returns>
        private byte AccessRXBuf(int i)
        { 
            return RX_buf[i%buf_len];
        }

        /// <summary>
        /// shifting every bit behind and including legnth to the front
        /// rest gets overwriten 
        /// decreases rx_tail 
        /// </summary>
        /// <param name="length"></param>
        private void shiftingRXBuf(int length)
        {
            //starts at lenght by the RX buf and copys it to 0
            //does this until end of RX buf reached
            Array.Copy(RX_buf, length, RX_buf, 0, buf_len - length);
            if(rx_tail!=0)      //not able to get negative tail 
                rx_tail -= length;  //decreases rx_tail about length to get the new tail 
            
        }

        /// <summary>
        /// checks if the message is completed 
        /// checks the checksum 
        /// does stuff according to the command
        /// </summary>
        private void DataManager()
        {
            try
            {
                while (rx_tail != 0)  //as long as there is some data in it (or the checksum does not fit or the full packet hasn't arrived yet)
                {

                    //correcting 
                    //if rx_tail is greater 2: it is going to overwrite it anyway
                    //if one of the präambles is wrong
                    //both combinded: shift(1) because didn't receive correct präamble
                    while (rx_tail > 2 && (AccessRXBuf(0) != BT_Protocoll.PräambleBytes[0] || AccessRXBuf(rx_head + 1) != BT_Protocoll.PräambleBytes[1]))
                    {
                        Logger("Received something which shouldn't be here; after or before packet arrived"+Logger_ReceiveError);
                        shiftingRXBuf(1);
                    }

                    if (rx_tail < 6) //if the minimum frame lenght has not arrived yet
                        return;
                    //now etleast there is a correct präamble and at least 6 recieved bytes 

                    int framelength = AccessRXBuf(rx_head + 2); //get the framelength (is the 3. Byte)

                    if (rx_tail < framelength + BT_Protocoll.FrameLengthOverhead)
                        return; //not full packege arrived yet

                    //if the last Byte is not the CarriageReturn the full packet has arrived but is not correct
                    if (AccessRXBuf(rx_head + BT_Protocoll.FrameLengthOverhead + framelength - 1) != BT_Protocoll.CarriageReturn)
                    {
                        Logger("Received something which shouldn't be here; in the packet"+Logger_ReceiveError);
                        shiftingRXBuf(1); 
                        break;
                    }

                    byte[] crcpacket = new byte[framelength];   //is because the präamble and the CR is not included (in the framelenght the cr and crc is included but in the crc the cr is not included instead the framelength)
                    Array.Copy(RX_buf, rx_head + 2, crcpacket, 0, crcpacket.Length); //copy the for the crc intresting byte in the crcpacket
                    //todo: undo if crc implemented on MCU
                    //if (crc_CheckWithCRC(crcpacket) != 0)   //if the crc is not fitting
                    //{
                    //    Logger("didn't pass Checksum");     //log it and end Datamanager
                    //    return;
                    //}
                    switch (AccessRXBuf(rx_head + 3))       //which Command is it
                    {
                        case (BT_Protocoll.StayingAliveAnswer):
                            count_missing_StayingAlive = 0;         //set counter of time till last StayingAlive Answer received (don't call OnDeviceDisconnected)
                            Logger("received StayingAliveAnswer"+Logger_ReceiveData);
                            break;                                               
                        case (BT_Protocoll.InitAnswer):
                            Logger("received InitAnswer"+Logger_ReceiveData);
                            break;
                        case (BT_Protocoll.MeasuredDataCommand):
                            int number = Convert.ToInt32((AccessRXBuf(rx_head + 4) << 8 )+ AccessRXBuf(rx_head + 5));
                            int time = Convert.ToInt32((AccessRXBuf(rx_head + 6) << 8) + AccessRXBuf(rx_head + 7));
                            int data = Convert.ToInt32((AccessRXBuf(rx_head + 8) << 8) + AccessRXBuf(rx_head + 9));
                            byte followup = AccessRXBuf(rx_head + 10);
                            OnMeasuredDataReceived(number, time,data);
                            //SendMeasuredDataAnswer(followup); //not doing it to reduce traffic on MCU while sending
                            Logger("received MeasuredDataCommand\t ReceivedData");
                            break;                                               
                        case (BT_Protocoll.MotorAdjustingAnswer):
                            int m_act_pos = Convert.ToInt32(Lastupdated_position * BT_Protocoll.ConvertFromMM);                 //using the last position (new position doesn't get sent)
                            int m_aim_pos = Convert.ToInt32((AccessRXBuf(rx_head + 4) << 8) + AccessRXBuf(rx_head + 5));        //get the real aim_position
                            OnPositionReceived(m_act_pos,m_aim_pos);
                            Logger("received MotorAdjustingAnswer" + Logger_ReceiveData);
                            break;                        
                        case (BT_Protocoll.StatusRequestAnswer):
                            OnStatusReceived(AccessRXBuf(rx_head + 4));
                            Logger("received StatusRequestAnswer" + Logger_ReceiveData);
                            break;                        
                        case (BT_Protocoll.PositionRequestAnswer):
                            int act_pos = Convert.ToInt32((AccessRXBuf(rx_head + 4) << 8) + AccessRXBuf(rx_head + 5));
                            int aim_pos = Convert.ToInt32((AccessRXBuf(rx_head + 6) << 8) + AccessRXBuf(rx_head + 7));
                            OnPositionReceived(act_pos,aim_pos);
                            Logger("received PositionRequestAnswer" + Logger_ReceiveData);
                            break;
                        case (BT_Protocoll.MaxGapRequestAnswer):
                            int i_maxGap = Convert.ToInt32((AccessRXBuf(rx_head + 4) << 8) + AccessRXBuf(rx_head + 5));
                            OnMaxGapReceived(i_maxGap);
                            Logger("received MaxGapRequestAnswer" + Logger_ReceiveData);
                            break;
                        case (BT_Protocoll.ReferenzPlacementAnswer):
                            OnReferenzPlacementReceived();
                            Logger("received ReferenzPlacementAnswer" + Logger_ReceiveData);
                            break;
                        case (BT_Protocoll.RunAnswer):
                            OnRunReceived();
                            Stopped_Movement = false;
                            Logger("received RunAnswer" + Logger_ReceiveData);
                            break;
                        case (BT_Protocoll.StopAnswer):
                            OnStopReceived();
                            Stopped_Movement = true;
                            Logger("received StopAnswer" + Logger_ReceiveData);
                            break;
                        #region shouldn't receive any of this
                        case (BT_Protocoll.InitCommand):
                            Logger("received InitCommand" + Logger_Error);
                            break;
                        case (BT_Protocoll.StayingAliveCommand):
                            Logger("received StayingAliveCommand" + Logger_Error);
                            break;
                        case (BT_Protocoll.MeasuredDataAnswer):
                            Logger("received MeasuredDataAnswer" + Logger_Error);
                            break;
                        case (BT_Protocoll.MotorAdjustingCommand):
                            Logger("received MotorAdjustingCommand" + Logger_Error);
                            break;
                        case (BT_Protocoll.StatusRequestCommand):
                            Logger("received StatusRequestCommand" + Logger_Error);
                            break;
                        case (BT_Protocoll.PositionRequestCommand):
                            Logger("received PositionRequestCommand" + Logger_Error);
                            break;
                        case (BT_Protocoll.MaxGapRequestCommand):
                            Logger("received MaxGapRequestCommand" + Logger_Error);
                            break;
                        case (BT_Protocoll.ReferenzPlacementCommand):
                            Logger("received ReferenzPlacementCommand" + Logger_Error);
                            break;
                        case (BT_Protocoll.StopCommand):
                            Logger("recieved StopCommand" + Logger_Error);
                            break;
                        #endregion

                        default:
                            break;
                    }

                    shiftingRXBuf(framelength + BT_Protocoll.FrameLengthOverhead); //remove packet out of buffer
                }
	        }
	        catch (Exception e)
	        {
                MessageBox.Show(e.ToString());
	        }                      
        }
        #endregion

        #region Connecting
        private void Connect_ac(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                if (bc.Connected)   //if it was able to connect to the device
                {
                    Logger("connected to Device: " + ConnectedDevice.DeviceName + " with Address " + ConnectedDevice.DeviceAddress +Logger_DeviceConnected);
                    if (s != null)
                        s.Close();      //close the old one
                    s = bc.GetStream(); //get the Stream to read and write on
                    s.BeginRead(RX_buf, rx_tail, buf_len - rx_tail, beginRead_cal, s);  //start reading    
                    OnDeviceConnected(); //calling the Event DeviceConnected
                    SendInit();
                    //startStayingAliveTimer();
                }
            }
            if (!bc.Connected)
            {
                Logger("Unable to connect to Device (AsyncCallback): " + ConnectedDevice.DeviceName + Logger_DeviceConnectedFailed);
                ConnectedDevice = null;
                OnDeviceConnectedFailed();                
            }
        }

        /// <summary>
        /// pair and connect to the device using the Pin
        /// </summary>
        /// <param name="DevName">Device name </param>
        public void ConnectToDevice(string DevName)
        {
            if (infos == null)
            {
                Logger("No devices in Info"+Logger_DeviceConnectedFailed);
                OnDeviceConnectedFailed();
                return;
            }
            foreach (var item in infos)
            {
                if (item.DeviceName == DevName)  //if the name of the device has the name we search for
                {
                    ConnectToDevice(item);
                    break;  //stop looking for other fitting devices 
                }
            }
        }

        /// <summary>
        /// pairs to the device (using pin); does it async
        /// trys to connect
        /// </summary>
        /// <param name="DevName"></param>
        public void ConnectToDevice(BluetoothDeviceInfo DevName)
        {
            if (ConnectThread==null||!ConnectThread.IsAlive)    //look if it is running (not allowed to run multiple times)
            {                
                ConnectThread = new System.Threading.Thread(() => ConnectToDeviceAsync(DevName));
                ConnectThread.Start();
            }
        }

        private void ConnectToDeviceAsync(BluetoothDeviceInfo DevName)
        {
            ConnectedDevice = DevName;      //set the deviceinfo of the connected device
            //if ((DateTime.Now - ConnectedDevice.LastSeen).TotalMinutes > MaxMinutesAsAvailable)    not needed, because they don't show up in UI
            //{
            //    Logger("Tried to connect to "+ ConnectedDevice.DeviceName + ", but is not in range");
            //    MessageBox.Show(ConnectedDevice.DeviceName + " not in range (is remebered)");
            //    OnDeviceConnectedFailed();
            //    return;
            //}
            try
            {
                BluetoothSecurity.PairRequest(ConnectedDevice.DeviceAddress, pin);   //pairing with the given pin

                if (ConnectedDevice.Authenticated)
                {
                    bc.BeginConnect(ConnectedDevice.DeviceAddress, BluetoothService.SerialPort, new AsyncCallback(Connect_ac), ConnectedDevice); //connect 
                }
                else
                {
                    Logger("Device not Authenticated" + Logger_DeviceConnectedFailed);
                    OnDeviceConnectedFailed();
                }
            }
            catch (Exception)
            {
                Logger("Throw while trying to pair and connect"+ Logger_DeviceConnectedFailed);
                OnDeviceConnectedFailed();
            }

        }

        /// <summary>
        /// closing all streams
        /// stop Staying alive
        /// </summary>
        public void DisconnectFromDevice()
        {
            try
            {
                s.Close();
                bc.Close();
                staying_alive_timer.Stop();
            }
            catch { }
            finally
            {
                s = null;
                bc.Dispose();
                bc = new BluetoothClient();
                ConnectedDevice = null;
                Logger("Disconnected from Device" + Logger_DeviceDisconnected);
                OnDeviceDisconnected();
            }
        }
        #endregion

        #region CRC
        /// <summary>
        /// creating the table to do crc8
        /// </summary>
        private void crc_CreateTable()
        {
            for (int i = 0; i < 256; ++i)
            {
                int temp = i;
                for (int j = 0; j < 8; ++j)
                {
                    if ((temp & 0x80) != 0)
                    {
                        temp = (temp << 1) ^ crc_poly;
                    }
                    else
                    {
                        temp <<= 1;
                    }
                }
                crc_table[i] = (byte)temp;
            }
        }

        /// <summary>
        /// used only for a whole packet (with präamble, framelength, command, data, crc, cr)
        /// doesn't use every bit (starts at the second one ends at the n-3)
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static byte crc_ComputeChecksum(params byte[] bytes)
        {
            byte crc = 0;
            if (bytes != null && bytes.Length > 0)
            {        
                //starting at 2 because the Präamble is not included in crc
                for (int i = 2; i < bytes.Length-2; i++) //-2 because the last 2 (CRC and CR) shouldn't be used to calculate crc
                    //this works only, when the first one is only done whith the values 
                {
                    crc = crc_table[crc ^ bytes[i]];
                }
            }
            return crc;
        }

        /// <summary>
        /// use only for the CRC with only the stuff intresting for the crc (framelenght, command, data, and sometimes crc (check if correct transmitted))
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static byte crc_CheckWithCRC(params byte[] bytes)
        {
            byte crc = 0;
            if (bytes != null && bytes.Length > 0)
            {
                foreach (byte b in bytes)
                {
                    crc = crc_table[crc ^ b];                    
                }
            }
            return crc;
        }
        #endregion

        /// <summary>
        /// setting the präamble in the first two bytes
        /// </summary>
        /// <param name="b"></param>
        private void SettingPräamble(ref byte[] b)
        {
            b[0] = BT_Protocoll.PräambleBytes[0];
            b[1] = BT_Protocoll.PräambleBytes[1]; 
        }

        #region Sending
        /// <summary>
        /// sends an Init to the Device with everything (präamble and crc and cr) 
        /// </summary>
        public void SendInit()
        {
            int packetlength = BT_Protocoll.InitLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);         //set the präamble 
            b[2] = BT_Protocoll.InitLength;   //set the length
            b[3] = BT_Protocoll.InitCommand;        //set the command

            b[packetlength - 2] = crc_ComputeChecksum(b); //set the checksum
            b[packetlength - 1] = BT_Protocoll.CarriageReturn;  //set the cr
            if(Send(b))
                Logger("sending Init" + Logger_SendData);
        }

        public void SendStayingAlive()
        {
            int packetlength = BT_Protocoll.StayingAliveLength + BT_Protocoll.FrameLengthOverhead;
            byte[] b= new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = BT_Protocoll.StayingAliveLength;
            b[3] = BT_Protocoll.StayingAliveCommand;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            if(Send(b))
                Logger("sending StayingAlive" + Logger_SendData);
        }

        /// <summary>
        /// sends an answer to MeasuredDataCommand
        /// </summary>
        /// <param name="notlastData">if there is a point comming after the last one</param>
        public void SendMeasuredDataAnswer(byte notlastData)
        {
            int packetlength = BT_Protocoll.MeasuredDataAnswerLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = BT_Protocoll.MeasuredDataAnswerLength;
            b[3] = BT_Protocoll.MeasuredDataAnswer;

            b[4] = notlastData;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            if(Send(b))
                Logger("sending MeasuredDataAnswer" + Logger_SendData);
        }

        /// <summary>
        /// sends a MotorAdustingCommand
        /// </summary>
        /// <param name="gap">the gap in  mm</param>
        public void SendMotorAdjusting(double gap)
        {
            int packetlength = BT_Protocoll.MotorAdjustingLength + BT_Protocoll.FrameLengthOverhead;
            int i_gap = Convert.ToInt32(Math.Round(gap * 100));
            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = BT_Protocoll.MotorAdjustingLength;
            b[3] = BT_Protocoll.MotorAdjustingCommand;

            b[4] = (byte)(i_gap >> 8);
            b[5] = (byte)(i_gap&0xFF);

            b[packetlength - 2] = crc_ComputeChecksum(b);
            b[packetlength - 1] = BT_Protocoll.CarriageReturn;
            if(Send(b))
                Logger("sending MotorAdjusting: Gap="+gap.ToString() + Logger_SendData);
        }

        public void SendStatusRequest()
        {
            int packetlength = BT_Protocoll.StatusRequestLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = BT_Protocoll.StatusRequestLength;
            b[3] = BT_Protocoll.StatusRequestCommand;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            if(Send(b))
                Logger("sending StatusRequest" + Logger_SendData);
        }

        public void SendPositionRequest()
        {
            int packetlength = BT_Protocoll.PositionRequestLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = BT_Protocoll.PositionRequestLength;
            b[3] = BT_Protocoll.PositionRequestCommand;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            if(Send(b))
                Logger("sending PositionRequest" + Logger_SendData);
        }

        public void SendMaxGapRequest()
        {
            int packetlength = BT_Protocoll.MaxGapRequestLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = BT_Protocoll.MaxGapRequestLength;

            b[3] = BT_Protocoll.MaxGapRequestCommand;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            if(Send(b))
                Logger("sending MaxGapRequest" + Logger_SendData);
        }

        public void SendReferenzPlacement()
        {
            int packetlength = BT_Protocoll.ReferenzPlacementLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = BT_Protocoll.ReferenzPlacementLength;

            b[3] = BT_Protocoll.ReferenzPlacementCommand;

            b[packetlength - 2] = crc_ComputeChecksum(b);
            b[packetlength - 1] = BT_Protocoll.CarriageReturn;
            if(Send(b))
                Logger("sending ReferenzPlacement" + Logger_SendData);
        }

        public void SendRun()
        {
            int packetlength = BT_Protocoll.RunLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = BT_Protocoll.RunLength;

            b[3] = BT_Protocoll.RunCommand;

            b[packetlength - 2] = crc_ComputeChecksum(b);
            b[packetlength - 1] = BT_Protocoll.CarriageReturn;
            if(Send(b))
                Logger("sending Run" + Logger_SendData);
        }

        public void SendStop()
        {
            int packetlength = BT_Protocoll.StopLenght + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = BT_Protocoll.StopLenght;

            b[3] = BT_Protocoll.StopCommand;

            b[packetlength - 2] = crc_ComputeChecksum(b);
            b[packetlength - 1] = BT_Protocoll.CarriageReturn;
            if(Send(b))
                Logger("sending Stop" + Logger_SendData);
        }

        public bool Send(byte[] b)
        {
            if (s != null)
            {
                s.Write(b, 0, b.Length);
                BytesSend += b.Length;
                return true; 
            }
            Logger("Trying to Send, not Connected"+ Logger_Error);
            return false;
        }
        #endregion

        #region Events
        public event EventHandler DeviceConnected;

        protected virtual void OnDeviceConnected()  //device connected
        {
            if (DeviceConnected != null)
                DeviceConnected(this, new EventArgs());
        }

        public event EventHandler DeviceConnectedFailed;

        protected virtual void OnDeviceConnectedFailed()    //not able to connect to device
        {
            ConnectedDevice = null;
            if (DeviceConnectedFailed != null)
                DeviceConnectedFailed(this, new EventArgs());
        }

        public event EventHandler DeviceDisconnected;

        protected virtual void OnDeviceDisconnected()   //device not not reachable (stayingAlive)
        {
            if (DeviceDisconnected != null)
                DeviceDisconnected(this, new EventArgs());
        }

        public event EventHandler DiscoverDevicesEnded;

        protected virtual void OnDiscoverDevicesEnded() //finished searching for devices
        {
            if (DiscoverDevicesEnded != null)
                DiscoverDevicesEnded(this, new EventArgs());
        }

        public event EventHandler<ReceivedData_EventArgs> MeasuredDataReceived;

        protected virtual void OnMeasuredDataReceived(int number, int time, int act_value) //received new MeasurementData
        {
            if (MeasuredDataReceived != null)
                MeasuredDataReceived(this, new ReceivedData_EventArgs(number,time,act_value));
        }

        public event EventHandler StatusReceived;

        protected virtual void OnStatusReceived(byte status) //Received new Status
        {
            Lastupdated_status = status;
            if (StatusReceived != null)
                StatusReceived(this, new ReceivedData_EventArgs(status));
        }

        public event EventHandler PositionReceived;

        protected virtual void OnPositionReceived(int current_position,  int aim_position)
        {
            Lastupdated_position = Convert.ToDouble(current_position) / BT_Protocoll.ConvertFromMM;
            this.aim_position = Convert.ToDouble(aim_position) / BT_Protocoll.ConvertFromMM;             //not using Property, because it will send new MotorAdjusting
            if (PositionReceived != null)
                PositionReceived(this, new ReceivedData_EventArgs(current_position, aim_position));
        }

        public event EventHandler MaxGapReceived;

        protected virtual void OnMaxGapReceived(int maxgap)
        {
            Maxgap = Convert.ToDouble(maxgap) / BT_Protocoll.ConvertFromMM;
            if (MaxGapReceived != null)
                MaxGapReceived(this, new ReceivedData_EventArgs(maxgap));
        }

        public event EventHandler ReferenzPlacementReceived;

        protected virtual void OnReferenzPlacementReceived()
        {
            if (ReferenzPlacementReceived != null)
                ReferenzPlacementReceived(this, new EventArgs());
        }

        public event EventHandler StopReceived;

        protected virtual void OnStopReceived()
        {
            if (StopReceived != null)
                StopReceived(this, new EventArgs());
        }

        public event EventHandler RunReceived;

        protected virtual void OnRunReceived()
        {
            if (RunReceived != null)
                RunReceived(this, new EventArgs());
        }
        #endregion

    }
}
