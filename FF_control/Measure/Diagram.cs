using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FF_control.Measure
{
    public class Diagram
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
        private double PlottingMargin = 0.1;       //used to  set a small marging (top, bottom, right and left)    

        private double DefaultPlotHeightWidth = 100; 
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
        private List<Graph> graphs;

        public List<Graph> Grpahs
        {
            get { return graphs; }
            set { graphs = value; }
        }


        public int YAxisLabelCount
        {
            get { return yAxisLabelCount; }
            set { yAxisLabelCount = value; }
        }
        public int XAxisLabelCount
        {
            get { return xAxisLabelCount; }
            set { xAxisLabelCount = value; }
        }


        /// <summary>
        /// drasw on given can
        /// set: addapts scale 
        /// </summary>
        public Canvas Can
        {
            get { return can; }
            set
            {
                var plotheight_old = plotheight;
                var plotwidth_old = plotwidth;
                can = value;
                if (can != null)
                {
                    if(can.ActualHeight!=0)
                        plotheight = can.ActualHeight;                                //setting height and Width
                    if(can.ActualWidth!=0)
                        plotwidth = can.ActualWidth;
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
        public Brush AxisColor { get; set; }            //whats the color of the Axis
        public Brush AxisLabelColor { get; set; }       //whats the color of the Axis Labels and Markers
        public double DiffPerScrolePercent { get; set; }       //what does the window (min and max of Axis) change per each scroll
        #endregion

        #region constructors
        /// <summary>
        /// creats default plot 
        /// hight = 100, width = 100 
        /// Plotcolor = Blue; AxisColor = green; AxisLabelColor = Black
        /// DiffPerScrolePercent = 1
        /// gets allways called 
        /// </summary>
        public Diagram()
        {
            Grpahs = new List<Graph>();
            plotheight = DefaultPlotHeightWidth;
            plotwidth = DefaultPlotHeightWidth;
            AxisColor = Brushes.Green;
            AxisLabelColor = Brushes.Black;
            DiffPerScrolePercent = 2;
        }
        public Diagram(List<Point> Points) : this()         //calls Plot() first
        {
            Graph g = new Graph();
            foreach (var item in Points)
            {
                g.addPoint(new MeasurementPoint(item));
            }
            graphs.Add(g);
        }
        public Diagram(Canvas ca) : this()                 //calls Plot() first
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
            if (can != null && graphs != null)
            {
                foreach (var item in graphs)
                {
                    item.draw(can, offsetX, offsetY, scaleX, scaleY,plotheight);
                }
            }
            return can;
        }

        /// <summary>
        /// sets offset and scale depending on the plotwidth and height and the points 
        /// doesn't need to be called if resized (need to set new Canvas) 
        /// get's min and max values and calls OffsetScaleCalcualtion
        /// </summary>
        public void setScalingAuto()
        {
            if (graphs.Count == 0)
                return;
            xmin = graphs[0].getXmin;
            xmax = graphs[0].getXmax;
            ymin = graphs[0].getYmin;
            ymax = graphs[0].getYmax;
            for (int i = 1; i < graphs.Count; i++)
            {
                if (xmin > graphs[i].getXmin)
                    xmin = graphs[i].getXmin;
                if (xmax < graphs[i].getXmax)
                    xmax = graphs[i].getXmax;
                if (ymin > graphs[i].getYmin)
                    ymin = graphs[i].getYmin;
                if (ymax < graphs[i].getYmax)
                    ymax = graphs[i].getYmax;
            }

            OffsetScaleCalculation();

        }

        /// <summary>
        /// Adds point to the List of MeasurementPoints 
        /// </summary>
        /// <param name="mp"></param>
        public void addPoint(MeasurementPoint mp, int Plotindex)
        {
            if (Plotindex > graphs.Count)
                return;
            graphs[Plotindex].addPoint(mp);
        }

        #region addGraph
        public void addGraph()
        {
            graphs.Add(new Graph());            
        }

        public void addGraph(Graph g)
        {
            graphs.Add(g);
        }

        public void addGraph(List<MeasurementPoint> mp, string graphname = "")
        {
            graphs.Add(new Graph(mp, graphname));
        }
        #endregion
        /// <summary>
        /// just for debugging purpose
        /// creating a small sample to test plotting
        /// </summary>
        /// <returns></returns>
        public static Graph createTestingPlot()
        {
            Graph p = new Graph();
            for (int i = 0; i < 20; i++)        //20 points
            {
                p.addPoint(new MeasurementPoint(new Point(i-5, 5-i%10), i));        //function for the points generated
            }
            return p; 
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
            pX.Points.Add(new Point(plotwidth -arrowlength, xAxis.Y1 + arrowwidth));
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

        /// <summary>
        /// sets offset and Sclae for given min and max value 
        /// </summary>
        public void OffsetScaleCalculation()
        {
            //give them some margin 20% of the canvas is margin (10% top and bottom)
            offsetX = xmin - (xmax - xmin) * PlottingMargin;     //xmin - Margin (Margin is not a pixel value) 
            offsetY = ymin - (ymax - ymin) * PlottingMargin;
            scaleX = plotwidth / (xmax + (xmax - xmin) * PlottingMargin - offsetX);     //*1 because of 2 Margins (on is already in offset); Pixel/Range displayed(=xmax+margin-offset)  
            scaleY = plotheight / (ymax + (ymax - ymin) * PlottingMargin - offsetY);
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
            double LeftToAll = MousePoint.X / plotwidth; //how much canvas is on the left of the pointer/max (=plotwidht)
            double TopToAll = MousePoint.Y / plotheight;     //how much canvas is on the top of the pointer/max (=plotwidht)

            xmin += DiffPerScrolePercent/100 * (xmax - xmin) * LeftToAll *delta; //adapt xmin, add value acording to sensetifity, and proportion of mouse Pointer and delta
            xmax -= DiffPerScrolePercent / 100 * (xmax - xmin) * (1 - LeftToAll) * delta;

            ymin += DiffPerScrolePercent / 100 * (ymax - ymin) * (1 - TopToAll) * delta;
            ymax -= DiffPerScrolePercent / 100 * (ymax - ymin) * TopToAll * delta;
            OffsetScaleCalculation();  //scale new offset and scale
        }

        public void Shift(double dx, double dy)
        {
            xmin += dx / scaleX;
            xmax += dx / scaleX;
            ymax += dy / scaleY;
            ymin += dy / scaleY;
            OffsetScaleCalculation();  //scale new offset and scale
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

    }
}
