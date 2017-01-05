﻿using System;
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
        public Diagram diagram { get; set; }
        public Connection v_connection;
        public FF_control.Visual.Control v_control;
        public Plot v_plot;
        public FF_control.Visual.Table v_table;

        public MainWindow()
        {            
            InitializeComponent();

            bt_connection = new BT_connection(); 

            diagram = new Diagram();
            diagram.addGraph(Diagram.createTestingPlot());
            Graph g = new Graph();
            g.addPoint(new MeasurementPoint(new Point(-5, 2)));
            g.addPoint(new MeasurementPoint(new Point(-2, 4)));
            g.addPoint(new MeasurementPoint(new Point(2, -2)));
            g.addPoint(new MeasurementPoint(new Point(5, 4)));
            diagram.addGraph(g);

            diagram.Grpahs[1].PlotColor = Brushes.Blue;
            diagram.Grpahs[0].PlotColor = Brushes.Green;

            v_connection = new Connection(this, SideTabControl);
            Connection_grid.Children.Add(v_connection);
            v_control = new FF_control.Visual.Control(this, SideTabControl);
            Control_grid.Children.Add(v_control);
            v_plot = new Plot(this, SideTabControl);
            Plot_grid.Children.Add(v_plot);
            v_table = new FF_control.Visual.Table(this, SideTabControl);
            Table_grid.Children.Add(v_table);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var app = App.Current as App;
            app.ChangeToDarkTheme();
        }

        private void TabControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Connection_grid.Width = dockpanel.ActualWidth - 250 - 150;
            Control_grid.Width = dockpanel.ActualWidth - 250 - 150;

        }
    }
}
