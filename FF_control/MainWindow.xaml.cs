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
    }
}
