using Microsoft.Win32;
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

namespace MeasureUtilities
{
    public class Graph
    {
        [XmlIgnore]
        public GraphCollection parent;  //the parent of this element (in which Collection is it stored in) (used for InformParent())
        public Guid uid;        //is a uniq ID to identifie it by
        #region Prop
        private ObservableCollection<MeasurementPoint> mps; //saves all Measurement Points

        public ObservableCollection<MeasurementPoint> Mps
        {
            get { return mps; }
            set
            {
                mps = value;
                InformParent();
            }
        }
        public string SaveLocation { get; set; } //the Location where this is saved at
        
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
            set
            {
                highlitedPointCircleRadius = value;
                InformParent();
            }
        }

        public double MeasurementGap
        {
            get { return gap; }
            set
            {
                gap = value;
                InformParent();
            }
        }        

        public DateTime MeasurementTime
        {
            get { return measurementTime; }
            set
            {
                measurementTime = value;
                InformParent();
            }
        }

        public double PlotStrokeThickness
        {
            get { return plotStrokeThickness; }
            set
            {
                plotStrokeThickness = value;
                InformParent();
            }
        }

        public string PlotColor_hex //only realy used for savin in xml
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
            set
            {
                plotColor = value;
                InformParent();
            }
        }

        private void InformParent()
        {
            if(parent!=null)
                parent.OnGraphCollectionPropertiesChanged(GraphCollectionChange.Graph, this);
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                InformParent();
            }
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
        public Graph(GraphCollection p)
        {
            mps = new ObservableCollection<MeasurementPoint>();
            plotStrokeThickness = 3;
            PlotColor = Brushes.Black;
            SaveLocation="";
            name = "";
            parent = p;
            uid = Guid.NewGuid();         
        }

        private Graph()
        {
            mps = new ObservableCollection<MeasurementPoint>();
            plotStrokeThickness = 3;
            PlotColor = Brushes.Black;
            SaveLocation = "";
            name = "";
            parent = null;
            uid = Guid.NewGuid();
        }

        /// <summary>
        /// creat a new graph with a measurement row
        /// </summary>
        /// <param name="mp">the row of measurement points</param>
        /// <param name="name">the name of the graph</param>
        public Graph(ObservableCollection<MeasurementPoint> mp, GraphCollection p,  string name = ""):this(p)
        {
            mps = mp;
            Name = name;
        }
        #endregion

        public void AddPoint(MeasurementPoint mp)
        {
            mps.Add(mp);
            InformParent();
        }

        /// <summary>
        /// draw on the canvas and use the scale of the given values
        /// </summary>
        /// <param name="can">The canvas, where to draw the things on</param>
        /// <param name="offsetX">the offset on X axis</param>
        /// <param name="offsetY">the offset on Y axis</param>
        /// <param name="ScaleX">the scale used for X Axis</param>
        /// <param name="ScaleY">the scale used for Y Axis</param>
        public void draw(Canvas can, double offsetX, double offsetY, double ScaleX, double ScaleY, double plotheight)
        {

            Polyline pl = new Polyline();       //defining new Polyline (Thickness = 3, Color = Black) 
            pl.StrokeThickness = PlotStrokeThickness;
            pl.Stroke = PlotColor;
            foreach (MeasurementPoint item in mps)       //adding the Point in the list
            {
                pl.Points.Add(GraphCollection.scalingPoint(item.getPoint(), offsetX,offsetY,ScaleX,ScaleY,plotheight));   //editing the points to fit to Canvas an plot 
                if (item.Highlited)
                { 
                    Ellipse e = new Ellipse();
                    e.Fill=PlotColor;
                    e.Height=highlitedPointCircleRadius;
                    e.Width=highlitedPointCircleRadius;
                    Canvas.SetTop(e, GraphCollection.scalingPoint(item.getPoint(),offsetX,offsetY,ScaleX,ScaleY,plotheight).Y-highlitedPointCircleRadius/2);
                    Canvas.SetLeft(e, GraphCollection.scalingPoint(item.getPoint(),offsetX,offsetY,ScaleX,ScaleY,plotheight).X-highlitedPointCircleRadius/2);
                    can.Children.Add(e);
                }
            }
            can.Children.Add(pl);
        }        

        internal void highliteallpoints(bool v)
        {
            foreach (var item in mps)
            {
                item.Highlited = v;
            }
            InformParent();
        }

        /// <summary>
        /// highlite a Point in the List
        /// use this instead of [].Highlited = xxx, because of InformParent
        /// </summary>
        /// <param name="index">which point to highlite</param>
        /// <param name="v">the value to whicht it should be set</param>
        public void highlitepoint(int index, bool v)
        {
            mps[index].Highlited = v;
            InformParent();
        }

        /// <summary>
        /// get the nearest point in the collection
        /// </summary>
        /// <param name="p">the point on Canvas</param>
        /// <param name="pointindex">which mps has the lowest one</param>
        /// <param name="scalex">scale of X Axis</param>
        /// <param name="scaley">scale of Y Axis</param>
        /// <param name="offsetX">offset used for X Axis</param>
        /// <param name="offsetY">offset used for Y Axis</param>
        /// <param name="plotheight">the hight of the Plotcan</param>
        /// <returns>the distance between mousepoint and neares point ^2</returns>
        public double get_nearest_point(Point p, ref int pointindex, double scalex, double scaley, double offsetX, double offsetY, double plotheight)
        {
            pointindex = -1; //set for default (if none existing)
            if (mps.Count==0)
                return -1;
            double nearest = mps[0].getDistance(p,scalex,scaley, offsetX, offsetY, plotheight); //start with the first one
            pointindex = 0; //set the pointindex on 0 (because its the first one, if none are smalle, this is the smallest)
            double temp; 
            for (int i = 1; i < mps.Count; i++)
            {
                temp = mps[i].getDistance(p,scalex,scaley, offsetX, offsetY, plotheight);
                if (temp < nearest)//only set if closer
                {
                    pointindex = i; 
                    nearest = temp;
                }
            }
            return nearest;
        }
    }
}
