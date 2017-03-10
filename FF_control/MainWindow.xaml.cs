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

using FF_control.Bluetooth;
using FF_control.Measure;
using FF_control.Visual; 

namespace FF_control
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BT_connection bt_connection { get; set; }
        public GraphCollection gcollection { get; set; }
        public Connection v_connection;
        public FF_control.Visual.Control v_control;
        public Plot v_plot;
        public FF_control.Visual.Table v_table;

        public MainWindow()
        {            
            InitializeComponent();

            bt_connection = new BT_connection(); 

            gcollection = new GraphCollection();
            gcollection.addGraph(GraphCollection.createTestingPlot(gcollection));
            Graph g = new Graph(gcollection);
            g.AddPoint(new MeasurementPoint(new Point(-5, 2)));
            g.AddPoint(new MeasurementPoint(new Point(-2, 4)));
            g.AddPoint(new MeasurementPoint(new Point(2, -2)));
            g.AddPoint(new MeasurementPoint(new Point(5, 4)));
            gcollection.addGraph(g);

            gcollection.Graphs[1].PlotColor = Brushes.Blue;
            gcollection.Graphs[0].PlotColor = Brushes.Green;

            v_connection = new Connection(this);
            Connection_grid.Children.Add(v_connection);
            v_control = new FF_control.Visual.Control(this);
            Control_grid.Children.Add(v_control);
            v_plot = new Plot(this);
            Plot_grid.Children.Add(v_plot);
            v_table = new FF_control.Visual.Table(this);
            Table_grid.Children.Add(v_table);
        }

        private void TabControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double maintabcontrolwidth = (double)FindResource("d_MainTabControlWidth");
            Connection_grid.Width = dockpanel.ActualWidth - maintabcontrolwidth;
            Control_grid.Width = dockpanel.ActualWidth - maintabcontrolwidth;
            Plot_grid.Width = dockpanel.ActualWidth - maintabcontrolwidth;
            Table_grid.Width = dockpanel.ActualWidth - maintabcontrolwidth;

            Connection_grid.Height = dockpanel.ActualHeight;
            Control_grid.Height = dockpanel.ActualHeight;
            Plot_grid.Height = dockpanel.ActualHeight;
            Table_grid.Height = dockpanel.ActualHeight;
        }

        private void menu_SaveCollection(object sender, RoutedEventArgs e)
        {
            gcollection.Save_diagram_xml();
        }

        private void menu_SaveSingleGraph(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("not implemented yet");
        }

        private void menu_Open(object sender, RoutedEventArgs e)
        {
            GraphCollection temp = GraphCollection.Open_diagram_xml();
            if (temp == null)
                return;
            gcollection.Clone(temp, v_plot.can);
        }

        private void menu_Include(object sender, RoutedEventArgs e)
        {
            Graph[] g = GraphCollection.Open_graph_xml();             //show the Open File dialog an other stuff
            if (g != null)
                foreach (var item in g)
                {
                    gcollection.addGraph(item);
                }
        }

        private void menu_New(object sender, RoutedEventArgs e)
        {
            gcollection.Clone(new GraphCollection(v_plot.can));
        }

        private void menu_Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void menu_ScaleAuto(object sender, RoutedEventArgs e)
        {
            gcollection.setScalingAuto();
        }

        private void menu_RemoveGraphs(object sender, RoutedEventArgs e)
        {
            while (gcollection.Graphs.Count!=0)
            {
                gcollection.removeGraph(0);
            }
        }

        private void menu_HighliteAll(object sender, RoutedEventArgs e)
        {
            gcollection.higliteallgraphs(((MenuItem)sender).IsChecked);
        }

        private void menu_DiagramEdit(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("not implemented yet");
        }

        private void menu_GraphEdit(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("not implemented yet");
        }

        private void menu_SaveClipboard(object sender, RoutedEventArgs e)
        {
            gcollection.save_to_clipboard();
        }

        private void menu_SavePng(object sender, RoutedEventArgs e)
        {
            gcollection.save_as_png();
        }

        private void menu_SaveCSV(object sender, RoutedEventArgs e)
        {
            gcollection.Export_as_CSV();
        }

        private void menu_Help(object sender, RoutedEventArgs e)
        {
            mainTabControl.SelectedIndex = mainTabControl.Items.Count - 1;
        }

        private void menu_lightTheme(object sender, RoutedEventArgs e)
        {

        }

        private void menu_darkTheme(object sender, RoutedEventArgs e)
        {             
            ChangeToDarkTheme();
        }
    }
}
