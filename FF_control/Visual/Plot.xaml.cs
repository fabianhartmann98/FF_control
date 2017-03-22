using System;
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

using FF_control.Measure;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for Plot.xaml
    /// </summary>
    public partial class Plot : UserControl
    {
        public MainWindow parent { get; set; }

        private const double MaxDistanceToleranz = 200; 

        //this is displayed in the ADD Tab
        private Button b_add;                       //host an OnClick 
        private DiagramProperties dp;
        private List<GraphProperties> gplist; 

        private int selected_tabindex;

        Point prevmousePosition;

        private Label DisplayingValueLabel;

        public Plot(MainWindow p)
        {
            InitializeComponent();
            parent = p;
            selected_tabindex = 0;          //set the default selected_tabindex

            parent.gcollection.Can = can;
            parent.gcollection.setScalingAuto();
            parent.gcollection.GraphCollectionPropertiesChanged += Gcollection_GraphCollectionPropertiesChanged;
            DrawDiagram();
            setUpSideTabControl();

            DisplayingValueLabel = new Label();
            DisplayingValueLabel.Background = parent.gcollection.BackgroundColor;
            DisplayingValueLabel.Foreground = parent.gcollection.AxisColor;
        }

        private void Gcollection_GraphCollectionPropertiesChanged(object sender, GraphCollectionChanged_EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke((Action<object, GraphCollectionChanged_EventArgs>)Gcollection_GraphCollectionPropertiesChanged, sender, e);
            }
            else
            {
                DrawDiagram();
                if (e.change == GraphCollectionChange.MinMax)
                    dp.update_minmax();
                if (e.change == GraphCollectionChange.Collection || e.change == GraphCollectionChange.everything)
                    setUpSideTabControl();
                if (e.change == GraphCollectionChange.Graph)
                    gplist[(int)e.Data].UpdateProperties();
            }
        }

        private void open_diagram(object sender, RoutedEventArgs e)
        {            
            GraphCollection temp = GraphCollection.Open_diagram_xml();
            if (temp == null)
                return;
            parent.gcollection.Clone(temp, can);
        }

        private void save_diagram(object sender, RoutedEventArgs e)
        {
            parent.gcollection.Save_diagram_xml();
        }

        private void save_to_clipboard(object sender, RoutedEventArgs e)
        {
            parent.gcollection.save_to_clipboard();
        }

        private void save_canvas_png(object sender, RoutedEventArgs e)
        {
            parent.gcollection.save_as_png();
        }

        private void setUpSideTabControl()
        {
            selected_tabindex = SideTabControl.SelectedIndex;
            gplist = new List<GraphProperties>();

            SideTabControl.Items.Clear();       //remove old TabItems, redo new one

            #region Tab Diagram
            TabItem ti = new TabItem();
            ti.Header = FindResource("s_DiagramSideTab").ToString();
            ti.Style = (Style)FindResource("Style_SideTabItem");

            dp = new DiagramProperties(parent.gcollection);
            ti.Content = dp;
            SideTabControl.Items.Add(ti);
            #endregion

            #region  Plot Tab
            for (int i = 0; i < parent.gcollection.Graphs.Count; i++)
			{			
                ti = new TabItem();
                ti.Header = FindResource("s_PlotSideTab").ToString()+i.ToString();
                ti.Style = (Style)FindResource("Style_SideTabItem");               

                GraphProperties gp = new GraphProperties(parent.gcollection.Graphs[i],parent.gcollection);
                gplist.Add(gp);
                ti.Content = gp;
                SideTabControl.Items.Add(ti);

            }
            #endregion

            #region Add Plot    
            ti = new TabItem();
            ti.Header = FindResource("s_AddPlotSideTab");
            ti.Style = (Style)FindResource("Style_SideTabItem");
            StackPanel mainstack = new StackPanel();

            WrapPanel wp = new WrapPanel();
            b_add = new Button();
            b_add.Content = FindResource("s_AddPlotSideTab");
            b_add.Click += b_add_Click;
            wp.Children.Add(b_add);
            mainstack.Children.Add(wp);
            ti.Content = mainstack;

            SideTabControl.Items.Add(ti);

            #endregion

            if (selected_tabindex < SideTabControl.Items.Count)
                SideTabControl.SelectedIndex = selected_tabindex;
           
        }

        void b_add_Click(object sender, RoutedEventArgs e)
        {            
            Graph[] g = GraphCollection.Open_graph_xml();             //show the Open File dialog an other stuff
            if(g!=null)
                foreach (var item in g)
                {
                    parent.gcollection.addGraph(item);
                }
        }

        public void DrawDiagram()
        {
            // der Textbox einen Text hinzufügen
            if (!can.Dispatcher.CheckAccess())
            {
                can.Dispatcher.Invoke((Action<string>)DrawDiagram, "0");
            }
            else
            {
                can.Children.Clear();           //clear the canvas to redraw axis and plots
                parent.gcollection.DrawAxis2dot0();
                can = parent.gcollection.draw();
                //if (tb_xmin != null)
                //{
                //    tb_xmin.Text = parent.diagram.AxisXmin.ToString("F2");      //"F2" used for #,##
                //    tb_xmax.Text = parent.diagram.AxisXmax.ToString("F2");
                //    tb_ymin.Text = parent.diagram.AxisYmin.ToString("F2");
                //    tb_ymax.Text = parent.diagram.AxisYmax.ToString("F2");
                //}
            }
            
        }
        public void DrawDiagram(string x)
        {
            DrawDiagram();
        }

        private void can_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            parent.gcollection.Scrole(e, e.Delta);         //scroll
        }

        private void can_MouseMove(object sender, MouseEventArgs e)
        {
            int graphindex = 0;
            int pointindex = 0;


            if (e.LeftButton == MouseButtonState.Pressed && prevmousePosition.X != -100)
            {

                double dx = e.GetPosition(can).X - prevmousePosition.X;   //get the differenz to the previous position
                double dy = e.GetPosition(can).Y - prevmousePosition.Y;

                prevmousePosition = e.GetPosition(can);
                parent.gcollection.Shift(-dx, dy);                          //shift the diagram
            }
            else
            {
                prevmousePosition.X = -100;
                if (parent.gcollection.Graphs.Count != 0 && parent.gcollection.get_nearest_point(e, ref graphindex, ref pointindex) < MaxDistanceToleranz)
                {
                    if (graphindex != -1 && pointindex != -1)
                        DisplayPoint(graphindex,  pointindex);
                }
            }
        }

        private void DisplayPoint( int graphindex,  int pointindex)
        {
            MeasurementPoint mp = parent.gcollection.Graphs[graphindex].Mps[pointindex];
            DisplayingValueLabel.Content = mp.ToString();
            Canvas.SetLeft(DisplayingValueLabel, parent.gcollection.scalingPoint(mp.getPoint()).X+GraphCollection.AxisMargin);
            Canvas.SetTop(DisplayingValueLabel, parent.gcollection.scalingPoint(mp.getPoint()).Y);
            can.Children.Remove(DisplayingValueLabel);
            can.Children.Add(DisplayingValueLabel);      

        }

        private void can_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            prevmousePosition = e.GetPosition(can);                 //get the first prevmouse as a reference
            if (e.ClickCount == 2)      //if double click on pont highlite the specific point (if is in range)
            {
                int graphindex = 0;
                int pointindex = 0;
                if(parent.gcollection.get_nearest_point(e as MouseEventArgs, ref graphindex, ref pointindex)< MaxDistanceToleranz) //get neares point and if it isn't to far away -> toggle highlite
                    parent.gcollection.Graphs[graphindex].highlitepoint(pointindex, !parent.gcollection.Graphs[graphindex].Mps[pointindex].Highlited);
            }
        }

        private void can_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            parent.gcollection.Can = can;                               //setting the can new, will change the scales and offset automatically
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            double columnwidth = (double)FindResource("d_SideTabControlWidth");
            if (plot_grid.ColumnDefinitions[plot_grid.ColumnDefinitions.Count - 1].Width.Value >= columnwidth)
                columnwidth = 10;
            plot_grid.ColumnDefinitions.RemoveAt(plot_grid.ColumnDefinitions.Count - 1);
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = new GridLength(columnwidth);
            cd.MinWidth = columnwidth;
            plot_grid.ColumnDefinitions.Add(cd);
        }
    }
}
