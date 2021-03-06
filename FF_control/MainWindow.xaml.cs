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

using BluetoothUtilities;
using MeasureUtilities;
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

        private List<MenuItem> SaveGraphMenuItemCollection; 

        public MainWindow()
        {            
            InitializeComponent();

            bt_connection = new BT_connection(); 

            gcollection = new GraphCollection();
            gcollection.addGraph(GraphCollection.createTestingGraph(gcollection));
            Graph g = new Graph(gcollection);
            g.AddPoint(new MeasurementPoint(new Point(-5, 2)));
            g.AddPoint(new MeasurementPoint(new Point(-2, 4)));
            g.AddPoint(new MeasurementPoint(new Point(2, -2)));
            g.AddPoint(new MeasurementPoint(new Point(5, 4)));
            g.Name = "Second TestGraph";
            gcollection.addGraph(g);

            gcollection.Graphs[1].GraphColor = Brushes.Blue;
            gcollection.Graphs[0].GraphColor = Brushes.Green;

            v_connection = new Connection(this);
            Connection_grid.Children.Add(v_connection);
            v_control = new FF_control.Visual.Control(this);
            Control_grid.Children.Add(v_control);
            v_plot = new Plot(this);
            Plot_grid.Children.Add(v_plot);
            v_table = new FF_control.Visual.Table(this);
            Table_grid.Children.Add(v_table);

            SaveGraphMenuItemCollection = new List<MenuItem>();
            gcollection.GraphCollectionPropertiesChanged += Gcollection_GraphCollectionPropertiesChanged;
            SetUpSaveSingleGraphMenuItems();

        }

        private void Gcollection_GraphCollectionPropertiesChanged(object sender, GraphCollectionChanged_EventArgs e)
        {
            if (e.change == GraphCollectionChange.Graph || e.change == GraphCollectionChange.Collection)
            {
                SetUpSaveSingleGraphMenuItems();
            }
        }

        private void SetUpSaveSingleGraphMenuItems()
        {            
            menuitem_SaveSingleGraph.Items.Clear();        

            int i = 0;
            SaveGraphMenuItemCollection.Clear();
            foreach (var item in gcollection.Graphs)
            {
                MenuItem mi = new MenuItem();
                mi.Header = item.Name;
                if (item.Name == "")
                    mi.Header = "Graph"+i.ToString();
                mi.Tag = i;
                mi.Click += menuitem_SaveSingleGraph_Click;
                SaveGraphMenuItemCollection.Add(mi);
                menuitem_SaveSingleGraph.Items.Add(mi);
                i++;
            }
        }

        private void menuitem_SaveSingleGraph_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            gcollection.Save_graph_xml((int)mi.Tag);
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
            Window window = new Window //disply it in a window
            {
                Title = "Edit Diagram",
                Content = new Visual.DiagramProperties(gcollection,true),  //content is the user-control used in sidetab
                Height = 550, 
                Width = 450
            };
            window.ShowDialog();
        }

        private void menu_GraphEdit(object sender, RoutedEventArgs e)
        {
            TabControl tc = new TabControl();
            tc.Style = (Style)FindResource("Style_SideTabControl");
            foreach (var item in gcollection.Graphs)
            {
                TabItem ti = new TabItem();
                ti.Style = (Style)FindResource("Style_SideTabItem");

                ti.Header = item.Name;
                ti.Content = new Visual.GraphProperties(item, gcollection);
                tc.Items.Add(ti);
            }            

            Window window = new Window //disply it in a window
            {
                Title = "Edit Diagram",
                Content = tc,  //content is the user-control used in sidetab
                Height = 500,
                Width = 500
            };
            window.ShowDialog();
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
            var app = App.Current as App;
            app.Theme_used = App.Themes.Light_Theme;
            app.ChangeDynamicResources();
        }

        private void menu_darkTheme(object sender, RoutedEventArgs e)
        {
            var app = App.Current as App;
            app.Theme_used = App.Themes.Dark_Theme;
            app.ChangeDynamicResources();
        }

        private void menu_German(object sender, RoutedEventArgs e)
        {
            var app = App.Current as App;
            app.Language_used = App.Languages.German;
            app.ChangeDynamicResources();
        }

        private void menu_English(object sender, RoutedEventArgs e)
        {
            var app = App.Current as App;
            app.Language_used = App.Languages.English;
            app.ChangeDynamicResources();
        }
    }
}
