﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;

namespace FF_control.Measure
{
    public class GraphCollection
    {

        /*
* Working flow: 
* 1. setup Plot (change Plotting color and shit) 
* 2. add Graph (add points and chang color and shit) 
* 3. add canvas 
* 4. if wanted (setAutoScaling) 
* 5. drawAxis if wanted
* 6. draw plot (need to clear can) 
*/

        #region private variables, not accesable form outside
        private double scaleX;              //by which scale do i have to multiplie to use canvas properly (set in setAxisAuto and set Can)
        private double offsetX;             //don't start at the left end (set in setAxisAuto) (not pixel value, instead its a value where the scale needs to be multiplied with) 
        private double scaleY;              //by wich scale do i have to multiplie to use canvas properly (set in setAxisAuto and set Can) 
        private double offsetY;             //don't start at the bottom end (set in setAxisAuto) 
        private double plotheight;          //whats the canvas hight (default = 100)
        private double plotwidth;           //whats the canvas width (default = 100) 

        #region constants                           
        //constants that are used to do Stuff but are here so it is easy to change
        private double arrowwidth =2.5; //what percentage is the arrow wide  
        private double arrowlength = 10; //how many times longer than wide 
        private double LableMarkerLenght = 10; //how long ist the Marker for a lable at a Axis 
        private int AxisStrokeThickness = 1;    //default StrokeThickness for the Axis
        private int LabelMarginTopX = 5;         //whats the Margin to the Label Marker X Axis
        private int LabelMarginLeftX = -10;        //Margin to the Label Marker XAxis
        private int LabelMarginTopY = -10;         //whats the Margin to the Label Marker YAxis
        private int LabelMarginLeftY = -25;        //Margin to the Label Marker YAxis
        private double PlottingMargin = 0.05;       //used to  set a small marging (top, bottom, right and left)  
        private double AxisMargin = 40;
        private double LabelRounding = 5;
        private double Rounding2dot0 = 5;
        private const int LabelMinAverage = 2;
        private const int LabelMaxAverage = 2000;


        private double DefaultPlotHeightWidth = 100;
        public static string FileFilter = "H2B2 (*.h2b2)|*.h2b2|All Files (*.*)|*.*";

        #endregion

        #endregion

        #region full prop variables
        private double xmin;                        //min x (time) value
        private double xmax;                        //maximum x (time) value 
        private double ymin;                        //minimum y (i_value) value
        private double ymax;                        //maximum y (i_value) value
        private Canvas can;                         //the canvas to draw on 
        private int xAxisLabelCount = 5;                //how many labels should be placed on the x Axis (default = 5)
        private int yAxisLabelCount = 5;                //how many labels should be placed on the y Axis  (default = 5)
        private Canvas plotcan;
        private string axisColor_hex;
        private string axisLabelColor_hex;
        private string backgroundColor_hex;
        private Brush backgroundColor;
        private Brush axisColor;
        private Brush axisLabelColor;
        private List<Graph> graphs;
        private double xDiffPerLabel;
        private double yDiffPerLabel;

        public double YDiffPerLabel
        {
            get { return yDiffPerLabel; }
            set { yDiffPerLabel = value; }
        }


        public double XDiffPerLabel
        {
            get { return xDiffPerLabel; }
            set { xDiffPerLabel = value; }
        }
        

        public List<Graph> Graphs
        {
            get { return graphs; }
            set
            {
                graphs = value;
                OnGraphCollectionPropertiesChanged(GraphCollectionChange.Collection);
            }
        }

        [XmlIgnore]
        public Brush AxisColor
        {
            get
            { return axisColor; }
            set
            {
                axisColor = value;
                OnGraphCollectionPropertiesChanged(GraphCollectionChange.Color);
            }
        }            //whats the color of the Axis
        [XmlIgnore]
        public Brush AxisLabelColor
        {
            get { return axisLabelColor; }
            set
            {
                axisLabelColor = value;
                OnGraphCollectionPropertiesChanged(GraphCollectionChange.Color);
            }
        }       //whats the color of the Axis Labels and Markers
        [XmlIgnore]
        public Brush BackgroundColor 
        {
            get { return backgroundColor; }
            set
            {
                backgroundColor = value;
                OnGraphCollectionPropertiesChanged(GraphCollectionChange.Color);
                if (can!=null)
                    can.Background = backgroundColor;
            } 
        }     

        public string BackgroundColor_hex
        {
            get { return ((SolidColorBrush)BackgroundColor).Color.ToString(); }
            set
            {
                backgroundColor_hex = value;
                var converter = new System.Windows.Media.BrushConverter();
                BackgroundColor = (Brush)converter.ConvertFromString(backgroundColor_hex);
            }
        }

        public string AxisLabelColor_hex
        {
            get { return ((SolidColorBrush)AxisLabelColor).Color.ToString(); }
            set
            {
                axisLabelColor_hex = value;
                var converter = new System.Windows.Media.BrushConverter();
                AxisLabelColor = (Brush)converter.ConvertFromString(axisLabelColor_hex);
            }
        }

        public string AxisColor_hex
        {
            get { return ((SolidColorBrush)AxisColor).Color.ToString(); }
            set 
            { 
                axisColor_hex = value;
                var converter = new System.Windows.Media.BrushConverter();
                AxisColor = (Brush)converter.ConvertFromString(axisColor_hex);
            }
        }        

        [XmlIgnore]
        public Canvas PlotCan
        {
            get { return plotcan; }
            set { plotcan = value; }
        }

        public int YAxisLabelCount
        {
            get { return yAxisLabelCount; }
            set
            {
                yAxisLabelCount = value;
                OnGraphCollectionPropertiesChanged(GraphCollectionChange.LabelCount);
            }
        }
        public int XAxisLabelCount
        {
            get { return xAxisLabelCount; }
            set
            {
                xAxisLabelCount = value;
                OnGraphCollectionPropertiesChanged(GraphCollectionChange.LabelCount);
            }
        }


        /// <summary>
        /// drasw on given can
        /// set: addapts scale 
        /// </summary>
        [XmlIgnore]
        public Canvas Can
        {
            get { return can; }
            set
            {
                can = value;
                can.Background = backgroundColor;
                if (can != null)
                {
                    if(can.ActualHeight!=0)
                        plotheight = can.ActualHeight - AxisMargin;                                //setting height and Width
                    if(can.ActualWidth!=0)
                        plotwidth = can.ActualWidth - AxisMargin;
                }
                else
                {
                    plotheight = DefaultPlotHeightWidth;
                    plotwidth = DefaultPlotHeightWidth;
                }
                OffsetScaleCalculation();
            }
        }

        public double AxisYmax
        {
            get { return ymax; }
            set
            {
                ymax = value;
                OffsetScaleCalculation();
            }
        }                             //setting scale and offset
        public double AxisYmin
        {
            get { return ymin; }
            set
            {
                ymin = value;
                OffsetScaleCalculation();
            }
        }
        public double AxisXmax
        {
            get { return xmax; }
            set
            {
                xmax = value;
                OffsetScaleCalculation();
            }
        }
        public double AxisXmin
        {
            get { return xmin; }
            set
            {
                xmin = value;
                OffsetScaleCalculation();
            }
        }


        #endregion

        #region prop vaiables 
        public double DiffPerScrolePercent { get; set; }       //what does the window (min and max of Axis) change per each scroll
        public int XDiffAccuracy { get; private set; }
        public int YDiffAccuracy { get; private set; }
        public int XLabelPow { get; private set; }
        public int YLabelPow { get; private set; }
        #endregion

        #region constructors
        /// <summary>
        /// creats default plot 
        /// hight = 100, width = 100 
        /// Plotcolor = Blue; AxisColor = green; AxisLabelColor = Black
        /// DiffPerScrolePercent = 1
        /// gets allways called 
        /// </summary>
        public GraphCollection()
        {
            graphs = new List<Graph>();
            plotheight = DefaultPlotHeightWidth;
            plotwidth = DefaultPlotHeightWidth;
            AxisColor = Brushes.Green;
            AxisLabelColor = Brushes.Black;
            backgroundColor = Brushes.AliceBlue;
            DiffPerScrolePercent = 2;
            plotcan = new Canvas();
            plotcan.ClipToBounds = true;
        }
        public GraphCollection(List<Point> Points) : this()         //calls Plot() first
        {
            Graph g = new Graph(this);
            foreach (var item in Points)
            {
                g.AddPoint(new MeasurementPoint(item));
            }
            this.addGraph(g);
        }
        public GraphCollection(Canvas ca) : this()                 //calls Plot() first
        {
            can = ca;
            if (can != null)
            {
                if (can.ActualHeight != 0)
                    plotheight = can.ActualHeight;                                //setting height and Width
                if (can.ActualWidth != 0)
                    plotwidth = can.ActualWidth;
            }
            else
            {
                plotheight = DefaultPlotHeightWidth;
                plotwidth = DefaultPlotHeightWidth;
            }
        }
        #endregion

        #region public methods
        #region diagram save open
        public void Save_diagram_xml()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = FileFilter;
            if (!(bool)sfd.ShowDialog())
                return;
            Save_diagram_xml(sfd.FileName);
        }

        /// <summary>
        /// saves this diagram as xml-file ont the location of filename
        /// </summary>
        /// <param name="filename">location of file</param>
        public void Save_diagram_xml(string filename)
        {
            System.IO.StreamWriter sww = new System.IO.StreamWriter(filename);
            XmlWriter writer = XmlWriter.Create(sww);
            try
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(GraphCollection));//create an xmlSerializer
                
                xsSubmit.Serialize(writer, this);//write to the file
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            writer.Close();
            sww.Close();
        }

        static public GraphCollection Open_diagram_xml()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = FileFilter;
            if (!(bool)ofd.ShowDialog())
                return null;
            return Open_diagram_xml(ofd.FileName);
        }

        /// <summary>
        /// opens a diagram at the location of filename and returns it
        /// </summary>
        /// <param name="filename">location of file</param>
        /// <returns>the diagram which was opended</returns>
        static public GraphCollection Open_diagram_xml(string filename)
        {
            StreamReader sr = new StreamReader(filename);
            try
            {                
                XmlSerializer xsSubmit = new XmlSerializer(typeof(GraphCollection));

                GraphCollection d =  (GraphCollection) xsSubmit.Deserialize(sr);
                sr.Close();
                return d;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            sr.Close();
            return null;
        }
        #endregion

        #region graph save and open
        static public void Save_graph_xml(Graph g)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (!(bool)sfd.ShowDialog())
                return;
            Save_graph_xml(g, sfd.FileName);
        }

        /// <summary>
        /// create a single graph in a new diagram at the given location
        /// </summary>
        /// <param name="g">the graph to save</param>
        /// <param name="filename">the location to save the file</param>
        static public void Save_graph_xml(Graph g, string filename)
        {
            GraphCollection d = new GraphCollection();
            d.addGraph(g);
            d.setScalingAuto();
            g.SaveLocation = filename;
            d.Save_diagram_xml(filename);
        }

        public void Save_graph_xml(int index)
        {
            Save_graph_xml(Graphs[index]);
        }

        /// <summary>
        /// saves a single graph (index) at a given location
        /// </summary>
        /// <param name="index">index of the graph</param>
        /// <param name="filename">the location to save the file</param>
        public void Save_graph_xml(int index, string filename)
        {
            GraphCollection.Save_graph_xml(Graphs[index],filename);
        }

        public static Graph[] Open_graph_xml()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (!(bool)ofd.ShowDialog())
                return null;
            return Open_graph_xml(ofd.FileName);
        }
        /// <summary>
        /// gets only the graphs of the file (ingnores the min and max)
        /// </summary>
        /// <param name="filename">the location of the file</param>
        /// <returns>all the saved graphs</returns>
        public static Graph[] Open_graph_xml(string filename)
        {
            GraphCollection d = GraphCollection.Open_diagram_xml(filename);
            if (d == null || d.Graphs.Count == 0)
                return null;
            Graph[] col = new Graph[d.Graphs.Count];
            for (int i = 0; i < d.Graphs.Count; i++)
            {
                col[i] = d.Graphs[i];
            }
            return col;
        }
        #endregion

        #region appearence 
        public void higliteallgraphs(bool highlite)
        {
            foreach (var item in Graphs)
            {
                item.highliteallpoints(highlite);
            }
        }

        /// <summary>
        /// drawing the Points in a Polyline (thickness = 3, color = Black) 
        /// need to clear can if needed 
        /// need to set points and can before caling
        /// need to call autoScaling 
        /// need to call AddAxis for axis
        /// </summary>
        /// <returns>Canvas with Polyline and Axis as Child</returns>
        public Canvas draw()
        {
            plotcan.Children.Clear();
            if (can != null && this != null)
            {
                foreach (var item in Graphs)
                {
                    item.draw(plotcan, offsetX, offsetY, scaleX, scaleY,plotheight);
                }
            }
            Canvas.SetLeft(plotcan,AxisMargin);
            Canvas.SetTop(plotcan, 0);
            can.Children.Add(plotcan);
            return can;
        }

        /// <summary>
        /// adding the axis with the labels and label marker 
        /// depending on min and max Values 
        /// </summary>
        public void DrawAxis()
        {
            #region xAxis
            //########Line################
            Line xAxis = new Line();
            xAxis.Stroke = AxisColor;
            xAxis.StrokeThickness = AxisStrokeThickness;
            xAxis.X1 = 0;                   //is starting left
            xAxis.X2 = plotwidth;           //end at the right end
            if (ymin <= 0 && ymax > 0)      //if Y=0 is displayed 
            {
                xAxis.Y1 = scalingPoint(new Point(0, 0)).Y; //set Y to 0 value (!= pixel value => needs scaling) 
                xAxis.Y2 = xAxis.Y1;
            }
            else
            {
                xAxis.Y1 = scalingPoint(new Point(0, ymin)).Y; //else use ymin as value to go through Y Axis (also needs scaling) 
                xAxis.Y2 = xAxis.Y1;
            }
            can.Children.Add(xAxis);

            //#########Arrow################
            Polygon pX = new Polygon();     //Arrow = Filled Polygon with 3 Points 
            pX.Fill = AxisColor;
            pX.Stroke = AxisColor;
            pX.StrokeThickness = AxisStrokeThickness;       //not really needed
            pX.Points.Add(new Point(plotwidth, xAxis.Y1));  //Spike point (at the end and on the level of xAxis) 
            pX.Points.Add(new Point(plotwidth - arrowlength, xAxis.Y1 - arrowwidth));
            //x = Width*(1-Arrowlengthpercentage); y = Y level of x axis - height*arrowwithpercentage
            pX.Points.Add(new Point(plotwidth - arrowlength, xAxis.Y1 + arrowwidth));
            can.Children.Add(pX);

            //#########Labels##############
            for (int i = 0; i < xAxisLabelCount; i++)   //for every Label
            {
                double x;
                if (xmin <= 0 && xmax > 0)     //if x = 0 is displayed 
                {
                    // q    =   count - how many labels do i have to place in negative(xmin/(dif per Label))
                    double q = (i + Math.Ceiling(xmin / (xmax - xmin) * (xAxisLabelCount)));        //uses Ceiling to round up (-1,2->-1) 
                    x = q * (xmax - xmin) / (xAxisLabelCount);         //multiplies it with the dif per Label
                }
                else
                {
                    x = i * (xmax - xmin) / (xAxisLabelCount) + xmin; //not displayed, so we start with xmin -> add up dif per Labe each time
                }

                Line l = new Line();        //setting up Label Marker Line
                l.Stroke = AxisLabelColor;
                l.StrokeThickness = AxisStrokeThickness;
                Point p = new Point(x, 0);
                p = scalingPoint(p);
                l.X1 = p.X;         //x is on the level of the value (is scaled) 
                l.X2 = p.X;
                l.Y1 = xAxis.Y1 - LableMarkerLenght / 2;        //on level of XAxis +- the LabelMarkerLength/2 so the Label marker has a Length 
                l.Y2 = xAxis.Y1 + LableMarkerLenght / 2;
                can.Children.Add(l);                            //adding Label marker Line to the Canvas

                TextBlock tb = new TextBlock();             //textblock with Value 
                tb.Foreground = AxisLabelColor;
                tb.Text = String.Format("{0:f2}", x);         //Floating 
                Canvas.SetLeft(tb, l.X1 + LabelMarginLeftX);    //Sets it to the Marker Line plus a little Margin (=constants =  around -10) 
                Canvas.SetTop(tb, l.Y2 + LabelMarginTopX);
                can.Children.Add(tb);                       //adds Label Value to the canvas
            }
            #endregion

            #region yAxis
            //########Line################
            Line yAxis = new Line();                //setting up yAxis ine 
            yAxis.Stroke = AxisColor;
            yAxis.StrokeThickness = AxisStrokeThickness;
            yAxis.Y1 = 0;                           //starting at the Top
            yAxis.Y2 = plotheight;                  //ending at the Bottom 
            if (xmin <= 0 && xmax > 0)              //if x = 0 is displayed
            {
                yAxis.X1 = scalingPoint(new Point(0, 0)).X;     //use x = 0 as crossing point with XAxis 
                yAxis.X2 = yAxis.X1;
            }
            else
            {
                yAxis.X1 = scalingPoint(new Point(xmin, 0)).X;  //else use smin as crossing point with XAxis
                yAxis.X2 = yAxis.X1;
            }
            can.Children.Add(yAxis);

            //#########Arrow################
            Polygon pY = new Polygon();     //Setting up Arrow = filled Polygon
            pY.Fill = AxisColor;
            pY.Stroke = AxisColor;
            pY.StrokeThickness = AxisStrokeThickness;
            pY.Points.Add(new Point(yAxis.X1, 0));   //is at the top and on the level of the yAxis 
            pY.Points.Add(new Point(arrowwidth + yAxis.X1, arrowlength));
            //x = yAxis.X +- Arrowwidth; y = Arrowlength 
            pY.Points.Add(new Point(-arrowwidth + yAxis.X1, arrowlength));
            can.Children.Add(pY);

            //#########Labels##############
            for (int i = 0; i < yAxisLabelCount; i++)
            {
                double y;

                if (ymin <= 0 && ymax > 0)  //if y = 0 is displayed
                {
                    // q    =   count - how many labels do i have to set in negative (xmin/(dif per label)) 
                    double q = (i + Math.Ceiling(ymin / (ymax - ymin) * (yAxisLabelCount)));
                    y = q * (ymax - ymin) / (yAxisLabelCount);
                }
                else
                {
                    y = i * (ymax - ymin) / (yAxisLabelCount) + ymin;    //start at ymin and add dif per label each time
                }
                Line l = new Line();            //setting up Label Marker Line
                l.Stroke = AxisLabelColor;
                l.StrokeThickness = AxisStrokeThickness;
                Point p = new Point(0, y);
                p = scalingPoint(p);
                l.X1 = yAxis.X1 - LableMarkerLenght / 2;    //is on the yAxis.X +-the Marker length
                l.X2 = yAxis.X1 + LableMarkerLenght / 2;
                l.Y1 = p.Y;                                 //is on the value (is scaled) 
                l.Y2 = p.Y;
                can.Children.Add(l);                        //add Label Marker Line to the canvas 

                TextBlock tb = new TextBlock();
                tb.Foreground = AxisLabelColor;
                tb.Text = String.Format("{0:f2}", y);
                Canvas.SetLeft(tb, l.X1 + LabelMarginLeftY);    //set it to the label + Margin
                Canvas.SetTop(tb, l.Y2 + LabelMarginTopY);
                can.Children.Add(tb);                           //add Label Value to the canvas 
            }
            #endregion
        }

        public void DrawAxis2dot0()
        {
            #region xAxis
            //########Line################
            Line xAxis = new Line();        //the straight line of the axis
            xAxis.Stroke = AxisColor;
            xAxis.StrokeThickness = AxisStrokeThickness;
            xAxis.X1 = 0;                   //is starting left
            xAxis.X2 = plotwidth + AxisMargin;           //end at the right end

            xAxis.Y1 = plotheight;              //setting it to the edge of plotcan (=can-Axismargin)
            xAxis.Y2 = xAxis.Y1;

            can.Children.Add(xAxis);

            //#########Arrow################
            Polygon pX = new Polygon();     //Arrow = Filled Polygon with 3 Points 
            pX.Fill = AxisColor;
            pX.Stroke = AxisColor;
            pX.StrokeThickness = AxisStrokeThickness;       //not really needed
            pX.Points.Add(new Point(xAxis.X2, xAxis.Y1));  //Spike point (at the end and on the level of xAxis) 
            pX.Points.Add(new Point(xAxis.X2 - arrowlength, xAxis.Y1 - arrowwidth));
            pX.Points.Add(new Point(xAxis.X2 - arrowlength, xAxis.Y1 + arrowwidth));
            can.Children.Add(pX);

            //#########Labels##############
            xDiffPerLabel = (xmax - xmin) / xAxisLabelCount;  //get the diff per label (=range displayed / number)
            XDiffAccuracy = 0;
            while (Math.Round(xDiffPerLabel) > LabelRounding*0.1)
            {
                xDiffPerLabel /= 10;
                XDiffAccuracy--;
            }
            while (Math.Round(xDiffPerLabel) < LabelRounding)  //get how much after the comma the first digit of diffperlabel is
            {
                xDiffPerLabel *= 10;                 //multiply it by the
                XDiffAccuracy++;
            }
            
            //diffperlabel=Math.Round(diffperlabel/Rounding2dot0)*Rounding2dot0;      //round it
            xDiffPerLabel = Math.Round(xDiffPerLabel);      //round it
            XDiffPerLabel /= Math.Pow(10, XDiffAccuracy);            //divide by the first multiplied potenz; using property so it eventually gets updatet in UI
            //now we have a rounded diffperlabel and how many commas did we need to get it (used to display only the necessary digits after comma)

            double xminrounded = xmin;          //round the xmin up (so it's not on the left of the Y-Axis) (only needed if 0 is not in range)
            xminrounded *= Math.Pow(10, XDiffAccuracy);
            //xminrounded = Math.Ceiling(xminrounded/Rounding2dot0)*Rounding2dot0;
            xminrounded = Math.Ceiling(xminrounded);
            xminrounded /= Math.Pow(10, XDiffAccuracy);

            XLabelPow = 0;                  //get the potency we need to multiply by to get floatingpoint number
            while ((Math.Abs(xmin) + Math.Abs(xmax)) * Math.Pow(10, -XLabelPow) / 2 < LabelMinAverage)
                XLabelPow-=3;
            while ((Math.Abs(xmin) + Math.Abs(xmax)) * Math.Pow(10, -XLabelPow) / 2 > LabelMaxAverage)
                XLabelPow+=3;


            for (int i = 0; i < (xmax-xmin)/xDiffPerLabel; i++)   //for every Label
            {
                double x;             //which x is the label going to be placed

                if (xmin <= 0 && xmax > 0)     //if x = 0 is displayed 
                {
                    // q    =   count - how many labels do i have to place in negative(xmin/(dif per Label))
                    double pos = (i + Math.Ceiling(xmin / xDiffPerLabel));        //uses Ceiling to round up (-1,2->-1) 
                    x = pos * xDiffPerLabel;         //multiplies it with the dif per Label
                }
                else
                {
                    x = i * xDiffPerLabel + xminrounded; //not displayed, so we start with xmin -> add up dif per Labe each time
                }

                Line label_marker_line = new Line();        //setting up Label Marker Line
                label_marker_line.Stroke = AxisLabelColor;
                label_marker_line.StrokeThickness = AxisStrokeThickness;
                Point p = new Point(x, 0);
                p = scalingPoint(p);
                label_marker_line.X1 = p.X + AxisMargin;         //x is on the level of the value (is scaled) (need to add AxisMargin (scaling only works inside plotcan (need to add the diff))
                label_marker_line.X2 = p.X + AxisMargin;
                label_marker_line.Y1 = xAxis.Y1 - LableMarkerLenght / 2;        //on level of XAxis +- the LabelMarkerLength/2 so the Label marker has a Length 
                label_marker_line.Y2 = xAxis.Y1 + LableMarkerLenght / 2;
                can.Children.Add(label_marker_line);                            //adding Label marker Line to the Canvas

                TextBlock tb = new TextBlock();             //textblock with Value 
                tb.Foreground = AxisLabelColor;
                int floatcomma = XDiffAccuracy + XLabelPow;
                if (floatcomma < 0)
                    floatcomma = 0;
                tb.Text = String.Format("{0:f" + Convert.ToString(floatcomma) + "}", x * Math.Pow(10, -XLabelPow));         //Floating, number of decimals = (numberofdecimalsindiffperlabel) - powermultipliedwith                
                Canvas.SetLeft(tb, label_marker_line.X1 + LabelMarginLeftX);    //set it to the label + Margin
                Canvas.SetTop(tb, label_marker_line.Y2 + LabelMarginTopX);
                can.Children.Add(tb);                       //adds Label Value to the canvas
            }

            TextBlock tb_multiplier = new TextBlock();      //displays the power used to multiply the labels-Text with
            tb_multiplier.Foreground = AxisLabelColor;
            tb_multiplier.Text = "10^" + Convert.ToString(XLabelPow);
            Canvas.SetLeft(tb_multiplier, 0);
            Canvas.SetTop(tb_multiplier, xAxis.Y1);
            can.Children.Add(tb_multiplier);
            #endregion

            #region yAxis
            //########Line################
            Line yAxis = new Line();                //setting up yAxis ine 
            yAxis.Stroke = AxisColor;
            yAxis.StrokeThickness = AxisStrokeThickness;
            yAxis.Y1 = 0;                           //starting at the Top
            yAxis.Y2 = plotheight + AxisMargin;                  //ending at the Bottom 

            //yAxis.X1 = scalingPoint(new Point(xmin, 0)).X + AxisMargin;  // use smin as crossing point with XAxis
            yAxis.X1 = AxisMargin;      //set the line on the AxisMargin (is directly to the Plotcan)
            yAxis.X2 = yAxis.X1;

            can.Children.Add(yAxis);

            //#########Arrow################
            Polygon pY = new Polygon();     //Setting up Arrow = filled Polygon
            pY.Fill = AxisColor;
            pY.Stroke = AxisColor;
            pY.StrokeThickness = AxisStrokeThickness;
            pY.Points.Add(new Point(yAxis.X1, 0));   //is at the top and on the level of the yAxis 
            pY.Points.Add(new Point(arrowwidth + yAxis.X1, arrowlength));
            //x = yAxis.X +- Arrowwidth; y = Arrowlength 
            pY.Points.Add(new Point(-arrowwidth + yAxis.X1, arrowlength));
            can.Children.Add(pY);

            //#########Labels##############
            yDiffPerLabel = (ymax - ymin) / yAxisLabelCount;     //diff per label = range displayed/ number of labels
            YDiffAccuracy = 0;
            while (Math.Round(yDiffPerLabel) > LabelRounding*0.1)      
            {
                yDiffPerLabel /= 10;
                YDiffAccuracy--;                                    //store the decimals, which is needed to display to get diffperlabel
            }
            while (Math.Round(yDiffPerLabel) < LabelRounding)      //multiply by 10, till there are more than decimals
            {
                yDiffPerLabel *= 10;
                YDiffAccuracy++;                                    //store the decimals, which is needed to display to get diffperlabel
            }
            
            yDiffPerLabel = Math.Round(yDiffPerLabel);
            YDiffPerLabel /= Math.Pow(10, YDiffAccuracy);    //using prop to potentioly inform UI

            double yminrounded = ymin;
            yminrounded *= Math.Pow(10, YDiffAccuracy);             //round ymin to the same level as diffperlabel
            yminrounded = Math.Ceiling(yminrounded);
            yminrounded /= Math.Pow(10, YDiffAccuracy);

            YLabelPow = 0;                                  //the power which is possible to devide the labeltext by 
            while ((Math.Abs(ymin) + Math.Abs(ymax)) * Math.Pow(10, -YLabelPow) / 2 < LabelMinAverage)
                YLabelPow-=3;
            while ((Math.Abs(ymin) + Math.Abs(ymax)) * Math.Pow(10, -YLabelPow) / 2 > LabelMaxAverage)
                YLabelPow+=3;

            for (int i = 0; i < (ymax - ymin) / yDiffPerLabel; i++)       //for every label
            {
                double y;

                if (ymin <= 0 && ymax > 0)  //if y = 0 is displayed
                {
                    // q    =   count - how many labels do i have to set in negative (xmin/(dif per label)) 
                    double pos = (i + Math.Ceiling(ymin / yDiffPerLabel));
                    y = pos * yDiffPerLabel;
                }
                else
                {
                    y = i * yDiffPerLabel + yminrounded;    //start at ymin and add dif per label each time
                }
                Line label_marker_line = new Line();            //setting up Label Marker Line
                label_marker_line.Stroke = AxisLabelColor;
                label_marker_line.StrokeThickness = AxisStrokeThickness;
                Point p = new Point(0, y);
                p = scalingPoint(p);
                label_marker_line.X1 = yAxis.X1 - LableMarkerLenght / 2;    //is on the yAxis.X +-the Marker length
                label_marker_line.X2 = yAxis.X1 + LableMarkerLenght / 2;
                label_marker_line.Y1 = p.Y;                                 //is on the value (is scaled) (don't need to add Axismargin, because plotcan is on top)
                label_marker_line.Y2 = p.Y;
                can.Children.Add(label_marker_line);                        //add Label Marker Line to the canvas 

                TextBlock tb = new TextBlock();
                tb.Foreground = AxisLabelColor;
                int floatcomma = YDiffAccuracy + YLabelPow;     //the decimals which needed to be displayed (to get diffperlabel visible)
                if (floatcomma < 0)
                    floatcomma = 0;
                tb.Text = String.Format("{0:f" + Convert.ToString(floatcomma) + "}", y * Math.Pow(10, -YLabelPow));         //Floating 
                Canvas.SetLeft(tb, label_marker_line.X1 + LabelMarginLeftY);    //set it to the label + Margin
                Canvas.SetTop(tb, label_marker_line.Y2 + LabelMarginTopY);
                can.Children.Add(tb);                           //add Label Value to the canvas 
            }
            tb_multiplier = new TextBlock();            //displays the power used to multiply the labels-Text with
            tb_multiplier.Foreground = AxisLabelColor;
            tb_multiplier.Text = "10^" + Convert.ToString(YLabelPow);
            Canvas.SetLeft(tb_multiplier, 0);
            Canvas.SetTop(tb_multiplier, 0);
            can.Children.Add(tb_multiplier);
            #endregion
        }
        #endregion

        /// <summary>
        /// sets offset and scale depending on the plotwidth and height and the points 
        /// doesn't need to be called if resized (need to set new Canvas) 
        /// get's min and max values and calls OffsetScaleCalcualtion
        /// </summary>
        public void setScalingAuto()
        {
            if (Graphs.Count == 0)
                return;
            xmin = Graphs[0].getXmin;
            xmax = Graphs[0].getXmax;
            ymin = Graphs[0].getYmin;
            ymax = Graphs[0].getYmax;
            for (int i = 1; i < Graphs.Count; i++)
            {
                if (xmin > Graphs[i].getXmin)
                    xmin = Graphs[i].getXmin;
                if (xmax < Graphs[i].getXmax)
                    xmax = Graphs[i].getXmax;
                if (ymin > Graphs[i].getYmin)
                    ymin = Graphs[i].getYmin;
                if (ymax < Graphs[i].getYmax)
                    ymax = Graphs[i].getYmax;
            }

            OffsetScaleCalculation();

        }

        #region addGraph removeGraph
        public void addGraph()
        {
            addGraph(new Graph(this));            
        }
        public void addGraph(ObservableCollection<MeasurementPoint> list,  string graphname = "")
        {
            addGraph(new Graph(list,this,graphname));
        }
        public void addGraph(Graph g)
        {
            Graphs.Add(g);
            OnGraphCollectionPropertiesChanged(GraphCollectionChange.Collection);
        }

        public void removeGraph(int index)
        {
            removeGraph(Graphs[index]);
        }
        public void removeGraph(Graph g)
        {
            Graphs.Remove(g);
            OnGraphCollectionPropertiesChanged(GraphCollectionChange.Collection);
        }

        public void Clone(GraphCollection g)
        {
            axisColor = g.AxisColor;
            axisLabelColor = g.AxisLabelColor;
            xmax = g.AxisXmax;
            xmin =  AxisXmin;
            ymax = g.AxisYmax;
            ymin = g.AxisYmin;
            backgroundColor = g.BackgroundColor;
            can = g.Can;
            DiffPerScrolePercent = g.DiffPerScrolePercent;
            graphs = g.Graphs;
            foreach (var item in graphs)
            {
                item.parent = this;
            }
            plotcan = g.PlotCan;
            xAxisLabelCount = g.XAxisLabelCount;
            yAxisLabelCount = g.YAxisLabelCount;
            OffsetScaleCalculation();
            OnGraphCollectionPropertiesChanged(GraphCollectionChange.everything);       
        }

        public void Clone(GraphCollection g, Canvas can)
        {
            axisColor = g.AxisColor;
            axisLabelColor = g.AxisLabelColor;
            xmax = g.AxisXmax;
            xmin = AxisXmin;
            ymax = g.AxisYmax;
            ymin = g.AxisYmin;
            backgroundColor = g.BackgroundColor;
            this.can = can;
            DiffPerScrolePercent = g.DiffPerScrolePercent;
            graphs = g.Graphs;
            foreach (var item in graphs)
            {
                item.parent = this;
            }
            plotcan = g.PlotCan;
            xAxisLabelCount = g.XAxisLabelCount;
            yAxisLabelCount = g.YAxisLabelCount;
            OffsetScaleCalculation();
            OnGraphCollectionPropertiesChanged(GraphCollectionChange.everything);
        }
        #endregion

        /// <summary>
        /// just for debugging purpose
        /// creating a small sample to test plotting
        /// </summary>
        /// <returns></returns>
        public static Graph createTestingPlot(GraphCollection parent)
        {
            Graph p = new Graph(parent);
            for (int i = 0; i < 20; i++)        //20 points
            {
                //p.addPoint(new MeasurementPoint(new Point(i-5, 5-i%10), i));        //function for the points generated
                p.AddPoint(new MeasurementPoint(new Point(i,0),i));
            }
            return p; 
        }

        /// <summary>
        /// sets offset and Sclae for given min and max value 
        /// </summary>
        public void OffsetScaleCalculation()
        {
            can_set_heigt_width(plotcan, plotheight, plotwidth);
            check_max_min();
            //give them some margin of the canvas is margin (margin/2 top and bottom)
            offsetX = xmin - (xmax - xmin) * PlottingMargin;     //xmin - Margin (Margin is not a pixel value; is percentage) 
            offsetY = ymin - (ymax - ymin) * PlottingMargin;
            scaleX = plotwidth / (xmax + (xmax - xmin) * PlottingMargin - offsetX);     //*1 because of 2 Margins (on is already in offset); Pixel/Range displayed(=xmax+margin-offset)  
            scaleY = plotheight / (ymax + (ymax - ymin) * PlottingMargin - offsetY);
            OnGraphCollectionPropertiesChanged(GraphCollectionChange.MinMax);

        }

        private void check_max_min()
        {
            if (xmin == xmax)
            {
                xmin--;
                xmax++;
            }
            if (ymin == ymax)
            {
                ymin--;
                ymax++;
            }
            if (xmax < xmin)
            {
                double temp = xmin;
                xmin = xmax;
                xmax = temp;
            }
            if (ymax < ymin)
            {
                double temp = ymin;
                ymin = ymax;
                ymax = temp;
            }
        }

        private void can_set_heigt_width(Canvas c, double height, double width)
        {
            // der Textbox einen Text hinzufügen
            if (!c.Dispatcher.CheckAccess())
            {
                c.Dispatcher.Invoke((Action<Canvas, double,double>)can_set_heigt_width, c,height, width);
            }
            else
            {
                c.Height = height;
                c.Width = width;
            }
        }

        /// <summary>
        /// scrolling with a sensetifity set in DiffPerScrolePersen [0,100]
        /// needs to be called by every wheel event
        /// calls offsetScaleCalculation
        /// needs to draw once again if it should be displayed
        /// </summary>
        /// <param name="MousePoint">point proportional to canvas origin</param>
        /// <param name="delta">how much was the wheel turned (/120)</param>
        public void Scrole(Point MousePoint, double delta)
        {
            delta /= 120;
            //used to zoom to the place where the mouse Pointer is
            double LeftToAll = (MousePoint.X) / plotwidth; //how much canvas is on the left of the pointer/max (=plotwidht)
            double TopToAll = MousePoint.Y / plotheight;     //how much canvas is on the top of the pointer/max (=plotwidht)

            xmin += DiffPerScrolePercent/100 * (xmax - xmin) * LeftToAll *delta; //adapt xmin, add value acording to sensetifity, and proportion of mouse Pointer and delta
            xmax -= DiffPerScrolePercent / 100 * (xmax - xmin) * (1 - LeftToAll) * delta;

            ymin += DiffPerScrolePercent / 100 * (ymax - ymin) * (1 - TopToAll) * delta;
            ymax -= DiffPerScrolePercent / 100 * (ymax - ymin) * TopToAll * delta;
            OffsetScaleCalculation();  //scale new offset and scale
        }

        /// <summary>
        /// get's the position in reference to plotcan
        /// calls Scrole(Point,double)
        /// </summary>
        /// <param name="e"></param>
        /// <param name="delta"></param>
        public void Scrole(MouseWheelEventArgs e, double delta)
        {
            Scrole(e.GetPosition(plotcan),delta);
        }

        /// <summary>
        /// shifts min and max according to dx and dy and the sclae used 
        /// gets new offset (also scale is computed)
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public void Shift(double dx, double dy)
        {
            xmin += dx / scaleX;
            xmax += dx / scaleX;
            ymax += dy / scaleY;
            ymin += dy / scaleY;
            OffsetScaleCalculation();  //scale new offset and scale
        }

        /// <summary>
        /// saves the Plot in a png (right now it's saved as logo.png in Debug)
        /// </summary>
        public void save_as_png()
        {
            Rect rect = new Rect(can.RenderSize);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right,
              (int)rect.Bottom, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(can);        //create screenshot
            //endcode as PNG
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));     //add it to the encoder

            //save to memory stream
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            pngEncoder.Save(ms);
            ms.Close();
            System.IO.File.WriteAllBytes("logo.png", ms.ToArray()); //save the png
        }

        /// <summary>
        /// save plot and some text to the clipboard as rtf
        /// </summary>
        public void save_to_clipboard()
        {
            Rect rect = new Rect(can.RenderSize);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)rect.Right,
              (int)rect.Bottom, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(can);        //create screenshot

            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb)); //add it to the encoder

            //save to memory stream
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            pngEncoder.Save(stream);            //add it to the stream
            byte[] bytes = stream.ToArray();    //create byte-Array

            string str = BitConverter.ToString(bytes, 0).Replace("-", string.Empty);
            int width = (int)rtb.Width;
            int height = (int)rtb.Height;
            StringBuilder sb = new StringBuilder();
            sb.Append(@"{\rtf1");           //add start of rtf
            sb.Append("text prev pic");     //add some text
            sb.Append(@"{\pict\pngblip\picw");  //add the header for the pic
            sb.Append(rtb.Width.ToString());    //add the widht
            sb.Append(@"\pich");                //add the height
            sb.Append(rtb.Height.ToString());
            sb.Append(@"\bin ");                //add the binary information of the png
            sb.Append(str);                     //byte array
            sb.Append("}");                     //end the pic
            sb.Append("text after pic");        //add some text after pic
            sb.Append("}");                     //close the rtf
            Clipboard.Clear();
            Clipboard.SetData(DataFormats.Rtf, sb.ToString());      //add it ot the clipboard (rtf)
        }
        #endregion
        
        #region private methods 
        /// <summary>
        /// scales the given point with the scale and offset (also plotheight) 
        /// </summary>
        /// <param name="p">edits this point</param>
        /// <returns>scaled point with fitting values for plot and canvas</returns>
        private Point scalingPoint(Point p)
        {
            Point q = new Point();
            q.X = (p.X - offsetX) * scaleX;
            q.Y = plotheight - (p.Y - offsetY) * scaleY; //start at the top -> height - YValue
            return q; 
        }
        #endregion

        #region Events
        public event EventHandler<GraphCollectionChanged_EventArgs> GraphCollectionPropertiesChanged;

        public void OnGraphCollectionPropertiesChanged(GraphCollectionChange gcc)
        {
            if (GraphCollectionPropertiesChanged != null)
                GraphCollectionPropertiesChanged(this, new GraphCollectionChanged_EventArgs(gcc));
        }
        public void OnGraphCollectionPropertiesChanged(GraphCollectionChange gcc, Graph g)
        {
            if (GraphCollectionPropertiesChanged != null)
                GraphCollectionPropertiesChanged(this, new GraphCollectionChanged_EventArgs(gcc, Graphs.FindIndex(item =>item.uid==g.uid)));
        }
        #endregion

    }
}
