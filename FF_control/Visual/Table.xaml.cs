using FF_control.Bluetooth;
using FF_control.Measure;
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Win32;

namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for Table.xaml
    /// </summary>
    public partial class Table : UserControl
    {
        public MainWindow parent { get; set; }              //saves the parent MainWindow (used to access bt_connection and graph) 

        private int selected_tabindex;                      //what tabindex was selected, before redoing SideTabItems 
        private List<GraphProperties> gplist;

        public Table(MainWindow p)
        {
            InitializeComponent();
            parent = p;
            selected_tabindex = 0;
            this.IsVisibleChanged += Table_IsVisibleChanged; //needed to set up the SideTabControl

            setUpSideTabControl(); //redo or do the TabItems 
            CreateTable();                                  //creats tables (for each graph one)
            parent.bt_connection.MeasuredDataReceived += bt_connection_MeasuredDataReceived;
        }

        private void Table_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible) //if it is set on visible 
            {                
                setUpSideTabControl(); //redo or do the TabItems 
                CreateTable();         //redo or do the Tables
            }
        }

        private void setUpSideTabControl()
        {
            selected_tabindex = SideTabControl.SelectedIndex; //get the last index, set back to it after redoing them
            SideTabControl.Items.Clear();       //delete all Items 
            gplist = new List<GraphProperties>();

            for (int i = 0; i < parent.diagram.Grpahs.Count; i++)//for each graph, creat an own tab
            {
                TabItem ti = new TabItem(); //creat Tab
                ti.Header = "Plot" + i.ToString(); //set Header to Plot0 for Graphs[0]
                ti.Style = (Style)FindResource("Style_SideTabItem");//set the Style

                GraphProperties gp = new GraphProperties(parent.diagram.Grpahs[i], parent.diagram);
                gp.GraphPropertiesChanged += gp_GraphPropertiesChanged;
                gplist.Add(gp);

                ti.Content = gp;
                SideTabControl.Items.Add(ti);
            }            
        }

        void gp_GraphPropertiesChanged(object sender, EventArgs e)
        {
            setUpSideTabControl();
            CreateTable();
        }

        void bt_connection_MeasuredDataReceived(object sender, ReceivedData_EventArgs e) //received a new Data
        {
            ReceivedData_EventArgs args = e;
            int number = (int)args.ArrayL[0];//get number (are saved in ArrayList) (order is importent)
            int time = (int)args.ArrayL[1];//get time
            int actvalue = (int)args.ArrayL[2]; //get actual value

            int index = parent.diagram.Grpahs.Count - 1; //get the index of the graph to be added

            if(number==0) //if number is 0 => new Graph
            {
                parent.diagram.Grpahs.Add(new Measure.Graph()); //add Graph
                index = parent.diagram.Grpahs.Count - 1;        //set index new
                parent.diagram.Grpahs[index].MeasurementTime=DateTime.Now;  //set the MeasurementTime
                parent.diagram.Grpahs[index].MeasurementGap = parent.bt_connection.Lastupdated_position; 
            }
            parent.diagram.Grpahs[index].mps.Add(new Measure.MeasurementPoint(actvalue, time, Convert.ToInt32(number))); //add the point
            parent.diagram.setScalingAuto();
            parent.v_plot.DrawDiagram();
            
        }


        public void CreateTable()
        {
            stackpanel_dg.Children.Clear(); //delete all Grids
            for (int i = 0; i < parent.diagram.Grpahs.Count; i++)
            {
                WrapPanel wp = new WrapPanel(); 
                DataGrid dg = new DataGrid();
                dg.MouseDoubleClick += dg_MouseDoubleClick;
                dg.Tag = i;
                //dg.Height = this.Height;                //setting height to NaN (this.Height is never set) decreases lag
                dg.AutoGenerateColumns = false;
                DataGridTextColumn dgtc = new DataGridTextColumn();
                dgtc.Header = "Number";
                dgtc.Binding = new Binding("MeasurementNumber");
                dg.Columns.Add(dgtc);
                dgtc = new DataGridTextColumn();
                dgtc.Header = "Time";
                dgtc.Binding = new Binding("Time");
                dg.Columns.Add(dgtc);
                dgtc = new DataGridTextColumn();
                dgtc.Header = "Value";
                dgtc.Binding = new Binding("I_Value");
                dg.Columns.Add(dgtc);
                dg.ItemsSource = parent.diagram.Grpahs[i].mps;
                wp.Children.Add(dg);
                stackpanel_dg.Children.Add(wp);
            }

        }

        void dg_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            parent.diagram.Grpahs[(int)(dg.Tag)].mps[dg.SelectedIndex].Highlited = true;
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            double columnwidth = (double)FindResource("d_SideTabControlWidth");
            if (table_grid.ColumnDefinitions[table_grid.ColumnDefinitions.Count-1].Width.Value >=columnwidth)
                columnwidth = 10;
            table_grid.ColumnDefinitions.RemoveAt(table_grid.ColumnDefinitions.Count-1);
            ColumnDefinition cd = new ColumnDefinition();
            cd.Width = new GridLength(columnwidth);
            cd.MinWidth = columnwidth;
            table_grid.ColumnDefinitions.Add(cd);
        }
    }
}
