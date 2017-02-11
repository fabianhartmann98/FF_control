using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FF_control.Measure
{
    public class MeasurementPoint
    {
        #region full prop
        private double i_value;             //whats the Current (Strom) value
        private double time;                //when was this recorded
        private int measurementNumber;      //whats the measurement number (not really needed) 
        private bool highlited;

        public bool Highlited
        {
            get { return highlited; }
            set { highlited = value; }
        }
        public int MeasurementNumber
        {
            get { return measurementNumber; }
            set { measurementNumber = value; }
        }
        public double Time
        {
            get { return time; }
            set { time = value; }
        }
        public double I_Value
        {
            get { return i_value; }
            set { i_value = value; }
        }
        #endregion

        #region constructors
        public MeasurementPoint()
        {
            i_value = 0;
            time = 0;
            measurementNumber = 0;
            highlited = false;
        }
        public MeasurementPoint(double value, double time = 0, int measurementNumber = 0) :this()
        {
            this.measurementNumber = measurementNumber;
            this.i_value = value;
            this.time = time; 
        }
        public MeasurementPoint(Point p, int number = 0) :this()
        {
            i_value = p.Y;
            time = p.X;
            measurementNumber = number; 
        }
        #endregion

        #region public methods
        public Point getPoint()
        {
            return new Point(time, i_value); 
        }       
        #endregion
    }
}
