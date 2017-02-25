﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace FF_control.Measure
{
    public class Graph
    {

        #region Prop
        public ObservableCollection<MeasurementPoint> mps { get; set; }
        public string SaveLocation { get; set; }
        public static string FileFilter ="H2B2 (*.b2h2)|*.b2h2|All Files (*.*)|*.*";

        private string name;            //name of the measurementrow
        [XmlIgnore]private Brush plotColor;        //the color which is going to be displayed
        private string plotColor_hex;

        
        private double plotStrokeThickness; //how thick is the grpah going to be
        private DateTime measurementTime;   //when was the row measured (start Time) 
        private double gap;                 //the gap in mm
        private double highlitedPointCircleRadius = 10;

        public double HighlitedPointCircleRadius
        {
            get { return highlitedPointCircleRadius ; }
            set { highlitedPointCircleRadius = value; }
        }

        public double MeasurementGap
        {
            get { return gap; }
            set { gap = value; }
        }
        

        public DateTime MeasurementTime
        {
            get { return measurementTime; }
            set { measurementTime = value; }
        }

        public double PlotStrokeThickness
        {
            get { return plotStrokeThickness; }
            set { plotStrokeThickness = value; }
        }

        public string PlotColor_hex
        {
            get { return ((SolidColorBrush)PlotColor).Color.ToString(); }
            set 
            { 
                plotColor_hex = value;
                var converter = new System.Windows.Media.BrushConverter();
                PlotColor = (Brush)converter.ConvertFromString(plotColor_hex);
            }
        }

        [XmlIgnore]
        public Brush PlotColor
        {
            get { return plotColor; }
            set { plotColor = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        #endregion

        #region field
        /// <summary>
        /// get the minimum time value in the graph
        /// </summary>
        public double getXmin
        {
            get {
                if (mps.Count == 0)
                    return 0;
                double min = mps[0].Time;
                foreach (var item in mps)
                {
                    if (item.Time < min)
                        min = item.Time;
                }
                return min; 
            }
        }

        /// <summary>
        /// get the maximum time value in the measurement row
        /// </summary>
        public double getXmax
        {
            get
            {
                if (mps.Count == 0)
                    return 0;
                double max = mps[0].Time;
                foreach (var item in mps)
                {
                    if (item.Time > max)
                        max = item.Time;
                }
                return max;
            }
        }

        /// <summary>
        /// get the minimum Current value in the measurement row
        /// </summary>
        public double getYmin
        {
            get
            {
                if (mps.Count == 0)
                    return 0;
                double min = mps[0].I_Value;
                foreach (var item in mps)
                {
                    if (item.I_Value < min)
                        min = item.I_Value;
                }
                return min;
            }
        }

        /// <summary>
        /// get the maximum Current value in the measurement row
        /// </summary>
        public double getYmax
        {
            get
            {
                if (mps.Count == 0)
                    return 0;
                double max = mps[0].I_Value;
                foreach (var item in mps)
                {
                    if (item.I_Value > max)
                        max = item.I_Value;
                }
                return max;
            }
        }
        #endregion

        #region constructor
        /// <summary>
        /// creat a new graph with a measurement row
        /// </summary>
        public Graph()
        {
            mps = new ObservableCollection<MeasurementPoint>();
            plotStrokeThickness = 3;
            PlotColor = Brushes.Black;
            SaveLocation="";
            name = "";
        }

        /// <summary>
        /// creat a new graph with a measurement row
        /// </summary>
        /// <param name="mp">the row of measurement points</param>
        /// <param name="name">the name of the graph</param>
        public Graph(ObservableCollection<MeasurementPoint> mp, string name = "") : this()
        {
            mps = mp;
            Name = name;
        }
        #endregion

        public void addPoint(MeasurementPoint mp)
        {
            mps.Add(mp);
        }

        /// <summary>
        /// draw on the canvas and use the scale of the given values
        /// </summary>
        /// <param name="can"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="ScaleX"></param>
        /// <param name="ScaleY"></param>
        public void draw(Canvas can, double offsetX, double offsetY, double ScaleX, double ScaleY, double plotheight)
        {

            Polyline pl = new Polyline();       //defining new Polyline (Thickness = 3, Color = Black) 
            pl.StrokeThickness = PlotStrokeThickness;
            pl.Stroke = PlotColor;
            foreach (MeasurementPoint item in mps)       //adding the Point in the list
            {
                pl.Points.Add(scalingPoint(item.getPoint(), offsetX,offsetY,ScaleX,ScaleY,plotheight));   //editing the points to fit to Canvas an plot 
                if (item.Highlited)
                { 
                    Ellipse e = new Ellipse();
                    e.Fill=PlotColor;
                    e.Height=highlitedPointCircleRadius;
                    e.Width=highlitedPointCircleRadius;
                    Canvas.SetTop(e,scalingPoint(item.getPoint(),offsetX,offsetY,ScaleX,ScaleY,plotheight).Y-highlitedPointCircleRadius/2);
                    Canvas.SetLeft(e,scalingPoint(item.getPoint(),offsetX,offsetY,ScaleX,ScaleY,plotheight).X-highlitedPointCircleRadius/2);
                    can.Children.Add(e);
                }
            }
            can.Children.Add(pl);
        }        

        internal void dehighliteallpoints()
        {
            foreach (var item in mps)
            {
                item.Highlited = false;
            }
        }

        private Point scalingPoint(Point p, double offsetX,double offsetY, double scaleX, double scaleY, double plotheight)
        {
            Point q = new Point();
            q.X = (p.X - offsetX) * scaleX;
            q.Y = plotheight - (p.Y - offsetY) * scaleY; //start at the top -> height - YValue
            return q;
        }        
    }
}
