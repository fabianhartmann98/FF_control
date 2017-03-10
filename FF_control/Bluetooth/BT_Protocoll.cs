using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF_control.Bluetooth
{
    public class BT_Protocoll
    {
        public const int FrameLengthOverhead = 3;
        public const int PräambleLength = 2;
        public const int FrameLengthLength = 1;
        public const int CommandLength = 1;

        public const int ConvertFromMM = 100; 

        public static readonly byte[] PräambleBytes = new byte[]{0xCA, 0xFE};
        public const byte CarriageReturn = 0x0D;

        public const int InitLength = 3;
        public const int InitAnswerLength = 3; 
        public const int StayingAliveLength = 3;
        public const int StayingAliveAnswerLength = 3;
        public const int MeasuredDataLength = 10;            //not fixed yet
        public const int MeasuredDataAnswerLength = 4;
        public const int MotorAdjustingLength = 5;
        public const int MotorAdjustingAnswerLength = 5;
        public const int StatusRequestLength = 3;
        public const int StatusRequestAnswerLength = 4;
        public const int PositionRequestLength = 3;
        public const int PositionRequestAnswerLength = 7;
        public const int MaxGapRequestLength = 3;
        public const int MaxGapRequestAnswerLength = 5;
        public const int ReferenzPlacementLength = 3;
        public const int RefernezPlacementAnswerLength = 3;
        public const int RunLength = 3;
        public const int RunAnswerLength = 3; 
        public const int StopLenght = 3;
        public const int StopAnswerLength = 3; 

        public const byte InitCommand = 0x01;
        public const byte InitAnswer = 0x81;
        public const byte StayingAliveCommand = 0x02;
        public const byte StayingAliveAnswer = 0x82;
        public const byte MeasuredDataCommand = 0x04;
        public const byte MeasuredDataAnswer = 0x84;
        public const byte MotorAdjustingCommand = 0x08;
        public const byte MotorAdjustingAnswer = 0x88;
        public const byte StatusRequestCommand = 0x11;
        public const byte StatusRequestAnswer = 0x91;
        public const byte PositionRequestCommand = 0x12;
        public const byte PositionRequestAnswer = 0x92;
        public const byte MaxGapRequestCommand = 0x13;
        public const byte MaxGapRequestAnswer = 0x93;
        public const byte ReferenzPlacementCommand = 0x14;
        public const byte ReferenzPlacementAnswer = 0x94;
        public const byte RunCommand = 0x7E;
        public const byte RunAnswer = 0xFE; 
        public const byte StopCommand = 0x7F;
        public const byte StopAnswer = 0xFF;

        public const int MMtoSendFormat = 100; 
    }
}
