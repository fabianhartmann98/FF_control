using System;
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

namespace FF_control.Bluetooth
{
    public class BT_connection
    {

        private void Logger(String lines)
        {
            // Write the string to a file.append mode is enabled so that the log
            // lines get appended to  test.txt than wiping content and writing the log
            System.IO.StreamWriter file = new System.IO.StreamWriter("Log.txt", true);
            file.Write(DateTime.Now.ToString() + ": ");
            file.WriteLine(lines);
            file.Close();
        }

        private void DeletLogger()
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter("Log.txt", false);
            file.Write("");
            file.Close();

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
                SendMotorAdjusting(Convert.ToInt32(aim_position * 100));
            }
        }
        
        public double Lastupdated_position {get; private set;}      //saves the gap in mm
        public byte Lastupdated_status{get; private set;}
        public double Maxgap { get; private set; }                                       //saves the max gap in mm

        public BluetoothClient bc {get; private set;}                 //the client which it is going to be connected to 
        public BluetoothDeviceInfo[] infos { get; private set; }        //all available DeviceInfos 
        public BluetoothDeviceInfo ConnectedDevice { get; private set; }         //the deviceInfo of the connected device
        Stream s;                           //the stream to write and read on   
        const int buf_len = 2048;            //the buffer Length of the RX
        byte[] RX_buf = new byte[buf_len];  //the buffer for RX 
        int rx_head = 0;                    //what we have already read (should always be 0)
        int rx_tail = 0;                    //the length in the array thath is filled but not read yet 

        static byte[] crc_table = new byte[256];        //the table to compute te crc8
         // x8 + x7 + x6 + x4 + x2 + 1
        const byte crc_poly = 0xd5;                     //the key for the crc8 = 111010101=0x1d5

        
        private string pin = "2017";                    //the pin to connect to the BT-Modul
        public string Pin
        {
            get { return pin; }
            set { pin = value; }
        }

        #region staying alive
        Timer staying_alive_timer;
        int count_missing_StayingAlive;
        const int max_count_missing_StayingAlive = 20;
        const double staying_alive_timer_interval = 1000;

        public void startStayingAliveTimer()
        {
            staying_alive_timer = new Timer(staying_alive_timer_interval);
            staying_alive_timer.Elapsed += staying_alive_timer_Elapsed;
            staying_alive_timer.Start();
        }

        void staying_alive_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            count_missing_StayingAlive++;
            if (count_missing_StayingAlive > max_count_missing_StayingAlive)
            {
                OnDeviceDisconnected();
            }
            SendStayingAlive();
        }
        #endregion

        public BT_connection()
        {
            DeletLogger();
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
        public string[] GetAvailableDevices()
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
                infos = bc.EndDiscoverDevices(ar);
                OnDiscoverDevicesEnded();
            }
        }
        //event in in region Event
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
            rx_tail += s.EndRead(ar);
            //ArrayList al = new ArrayList();
            //for (int i = 0; i+1< rx_tail-rx_head; i+=2)
            //{
            //    int high = AccessRXBuf(i + rx_head);
            //    int low = AccessRXBuf(i + rx_head + 1);
            //    int x = (high << 8) + low;
            //    al.Add(x);
            //}
            DataManager();

            s.BeginRead(RX_buf, rx_tail, buf_len-rx_tail, beginRead_cal, s);            
            //s.BeginRead(RX_buf, rx_tail, buf_len, beginRead_cal, s);

        }

        /// <summary>
        /// modules the input to the buf_len so no out of Range can appear
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
                        Logger("Received something which shouldn't be here; after or before packet arrived");
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
                        Logger("Received something which shouldn't be here; in the packet");
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
                            count_missing_StayingAlive = 0;
                            Logger("received StayingAliveAnswer");
                            break;                                               
                        case (BT_Protocoll.InitAnswer):
                            Logger("received InitAnswer");
                            break;
                        case (BT_Protocoll.MeasuredDataCommand):
                            int number = Convert.ToInt32((AccessRXBuf(rx_head + 4) << 8 )+ AccessRXBuf(rx_head + 5));
                            int time = Convert.ToInt32((AccessRXBuf(rx_head + 6) << 8) + AccessRXBuf(rx_head + 7));
                            int data = Convert.ToInt32((AccessRXBuf(rx_head + 8) << 8) + AccessRXBuf(rx_head + 9));
                            byte followup = AccessRXBuf(rx_head + 10);
                            OnMeasuredDataReceived(number, time,data);
                            SendMeasuredDataAnswer(followup);
                            Logger("received MeasuredDataCommand");
                            break;                                               
                        case (BT_Protocoll.MotorAdjustingAnswer):
                            int m_act_pos = Convert.ToInt32(Lastupdated_position * BT_Protocoll.ConvertFromMM);                 //using the last position (new position doesn't get sent)
                            int m_aim_pos = Convert.ToInt32((AccessRXBuf(rx_head + 4) << 8) + AccessRXBuf(rx_head + 5));        //get the real aim_position
                            OnPositionReceived(m_act_pos,m_aim_pos);
                            Logger("received MotorAdjustingAnswer");
                            break;                        
                        case (BT_Protocoll.StatusRequestAnswer):
                            OnStatusReceived(AccessRXBuf(rx_head + 4));
                            Logger("received StatusRequestAnswer");
                            break;                        
                        case (BT_Protocoll.PositionRequestAnswer):
                            int act_pos = Convert.ToInt32((AccessRXBuf(rx_head + 4) << 8) + AccessRXBuf(rx_head + 5));
                            int aim_pos = Convert.ToInt32((AccessRXBuf(rx_head + 6) << 8) + AccessRXBuf(rx_head + 7));
                            OnPositionReceived(act_pos,aim_pos);
                            Logger("received PositionRequestAnswer");
                            break;
                        case (BT_Protocoll.MaxGapRequestAnswer):
                            int i_maxGap = Convert.ToInt32((AccessRXBuf(rx_head + 4) << 8) + AccessRXBuf(rx_head + 5));
                            OnMaxGapReceived(i_maxGap);
                            Logger("received MaxGapRequestAnswer");
                            break;
                        case (BT_Protocoll.ReferenzPlacementAnswer):
                            OnReferenzPlacementReceived();
                            Logger("received ReferenzPlacementAnswer");
                            break;
                        case (BT_Protocoll.RunAnswer):
                            OnRunReceived();
                            Logger("received RunAnswer");
                            break;
                        case (BT_Protocoll.StopAnswer):
                            OnStopReceived();
                            Logger("received StopAnswer");
                            break;
                        #region shouldn't receive any of this
                        case (BT_Protocoll.InitCommand):
                            Logger("received InitCommand\t ERROR");
                            break;
                        case (BT_Protocoll.StayingAliveCommand):
                            Logger("received StayingAliveCommand\t ERROR");
                            break;
                        case (BT_Protocoll.MeasuredDataAnswer):
                            Logger("received MeasuredDataAnswer\t ERROR");
                            break;
                        case (BT_Protocoll.MotorAdjustingCommand):
                            Logger("received MotorAdjustingCommand\t ERROR");
                            break;
                        case (BT_Protocoll.StatusRequestCommand):
                            Logger("received StatusRequestCommand\t ERROR");
                            break;
                        case (BT_Protocoll.PositionRequestCommand):
                            Logger("received PositionRequestCommand\t ERROR");
                            break;
                        case (BT_Protocoll.MaxGapRequestCommand):
                            Logger("received MaxGapRequestCommand\t ERROR");
                            break;
                        case (BT_Protocoll.ReferenzPlacementCommand):
                            Logger("received ReferenzPlacementCommand");
                            break;
                        case (BT_Protocoll.StopCommand):
                            Logger("recieved StopCommand\t ERROR");
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
                if (bc.Connected)
                {
                    Logger("connected to Device: " + ConnectedDevice.DeviceName + " with Address " + ConnectedDevice.DeviceAddress);
                    if (s != null)
                        s.Close();
                    s = bc.GetStream(); //get the Stream to read and write on
                    s.BeginRead(RX_buf, rx_tail, buf_len - rx_tail, beginRead_cal, s);  //start reading    
                    OnDeviceConnected(); //calling the Event DeviceConnected
                    SendInit();
                    //startStayingAliveTimer();
                }
            }
        }

        /// <summary>
        /// pair and connect to the device using the Pin
        /// </summary>
        /// <param name="DevName">Device name </param>
        public void ConnectToDevice(string DevName)
        {
            foreach (var item in infos)
            {
                if (item.DeviceName == DevName)  //if the name of the device has the name we search for
                {
                    ConnectedDevice = item;      //set the deviceinfo of the connected device
                    BluetoothSecurity.PairRequest(ConnectedDevice.DeviceAddress, pin);   //pairing with the given pin

                    if (ConnectedDevice.Authenticated)
                    {
                        bc.BeginConnect(ConnectedDevice.DeviceAddress, BluetoothService.SerialPort, new AsyncCallback(Connect_ac),ConnectedDevice); //connect 
                    }
                    break;  //stop looking for other fitting devices 
                }
            }
        }

        public void ConnectToDevice(BluetoothDeviceInfo DevName)
        {

            ConnectedDevice = DevName;      //set the deviceinfo of the connected device
            BluetoothSecurity.PairRequest(ConnectedDevice.DeviceAddress, pin);   //pairing with the given pin

            if (ConnectedDevice.Authenticated)
            {
                bc.BeginConnect(ConnectedDevice.DeviceAddress, BluetoothService.SerialPort, new AsyncCallback(Connect_ac), ConnectedDevice); //connect 
            }
        }

        public void DisconnectFromDevice()
        {
            try
            {
                staying_alive_timer.Stop();
                s.Close();
                bc.Close();                
            }
            catch { }
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
            b[2] = (byte)BT_Protocoll.InitLength;   //set the length
            b[3] = BT_Protocoll.InitCommand;        //set the command

            b[packetlength - 2] = crc_ComputeChecksum(b); //set the checksum
            b[packetlength - 1] = BT_Protocoll.CarriageReturn;  //set the cr
            Send(b);
            Logger("sending Init");
        }

        public void SendStayingAlive()
        {
            int packetlength = BT_Protocoll.StayingAliveLength + BT_Protocoll.FrameLengthOverhead;
            byte[] b= new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.StayingAliveLength;
            b[3] = BT_Protocoll.StayingAliveCommand;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending StayingAlive");
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
            b[2] = (byte)BT_Protocoll.MeasuredDataAnswerLength;
            b[3] = BT_Protocoll.MeasuredDataAnswer;

            b[4] = notlastData;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending MeasuredDataAnswer");
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
            b[2] = (byte)BT_Protocoll.MotorAdjustingLength;
            b[3] = BT_Protocoll.MotorAdjustingCommand;

            b[4] = (byte)(i_gap >> 8);
            b[5] = (byte)(i_gap&0xFF);

            b[packetlength - 2] = crc_ComputeChecksum(b);
            b[packetlength - 1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending MotorAdjusting");
        }

        public void SendStatusRequest()
        {
            int packetlength = BT_Protocoll.StatusRequestLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.StatusRequestLength;
            b[3] = BT_Protocoll.StatusRequestCommand;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending StatusRequest");
        }

        public void SendPositionRequest()
        {
            int packetlength = BT_Protocoll.PositionRequestLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.PositionRequestLength;
            b[3] = BT_Protocoll.PositionRequestCommand;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending PositionRequest");
        }

        public void SendMaxGapRequest()
        {
            int packetlength = BT_Protocoll.MaxGapRequestLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.MaxGapRequestLength;

            b[3] = BT_Protocoll.MaxGapRequestCommand;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending MaxGapRequest");
        }

        public void SendReferenzPlacement()
        {
            int packetlength = BT_Protocoll.ReferenzPlacementLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.ReferenzPlacementLength;

            b[3] = BT_Protocoll.ReferenzPlacementCommand;

            b[packetlength - 2] = crc_ComputeChecksum(b);
            b[packetlength - 1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending ReferenzPlacement");
        }

        public void SendRun()
        {
            int packetlength = BT_Protocoll.RunLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.RunLength;

            b[3] = BT_Protocoll.RunCommand;

            b[packetlength - 2] = crc_ComputeChecksum(b);
            b[packetlength - 1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending Stop");
        }

        public void SendStop()
        {
            int packetlength = BT_Protocoll.StopLenght + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.StopLenght;

            b[3] = BT_Protocoll.StopCommand;

            b[packetlength - 2] = crc_ComputeChecksum(b);
            b[packetlength - 1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending Stop");
        }

        public void Send(byte[] b)
        {
            if (s != null)
                s.Write(b, 0, b.Length);
            //s.Write(new byte[] {0x01}, 0, 1);
        }
        #endregion

        #region Events
        public event EventHandler DeviceConnected;

        protected virtual void OnDeviceConnected()
        {
            if (DeviceConnected != null)
                DeviceConnected(this, new EventArgs());
        }

        public event EventHandler DeviceDisconnected;

        protected virtual void OnDeviceDisconnected()
        {
            DisconnectFromDevice(); 
            if (DeviceDisconnected != null)
                DeviceDisconnected(this, new EventArgs());
        }

        public event EventHandler DiscoverDevicesEnded;

        protected virtual void OnDiscoverDevicesEnded()
        {
            if (DiscoverDevicesEnded != null)
                DiscoverDevicesEnded(this, new EventArgs());
        }

        public event EventHandler<ReceivedData_EventArgs> MeasuredDataReceived;

        protected virtual void OnMeasuredDataReceived(int number, int time, int act_value)
        {
            if (MeasuredDataReceived != null)
                MeasuredDataReceived(this, new ReceivedData_EventArgs(number,time,act_value));
        }

        public event EventHandler StatusReceived;

        protected virtual void OnStatusReceived(byte status)
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
