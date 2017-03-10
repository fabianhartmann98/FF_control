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
        private bool highlited;             //if it should draw a circle arount it 

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

        public override string ToString()
        {
            return "Time:"+time.ToString()+"\nValue:"+i_value.ToString();
        }

        /// <summary>
        /// gets the distance to this point 
        /// </summary>
        /// <returns>distance ^2</returns>
        internal double getDistance(Point p, double scalex, double scaley, double offsetX, double offsetY, double plotheight)
        {
            Point scaledpoint = GraphCollection.scalingPoint(this.getPoint(), offsetX, offsetY, scalex, scaley, plotheight);
            double dx = scaledpoint.X - p.X;
            double dy = scaledpoint.Y - p.Y;
            return Math.Pow(dx, 2) + Math.Pow(dy, 2);
        }
        #endregion
    }
}
