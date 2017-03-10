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
        private List<DataGrid> dglist; 

        public Table(MainWindow p)
        {
            InitializeComponent();
            parent = p;

            gplist = new List<GraphProperties>();
            dglist = new List<DataGrid>();

            selected_tabindex = 0;
            setUpSideTabControl(); //redo or do the TabItems 
            CreateTable();                                  //creats tables (for each graph one)
            parent.bt_connection.MeasuredDataReceived += bt_connection_MeasuredDataReceived;
            parent.gcollection.GraphCollectionPropertiesChanged += Diagram_GraphCollectionPropertiesChanged;
        }

        private void Diagram_GraphCollectionPropertiesChanged(object sender, GraphCollectionChanged_EventArgs e)
        {
            if (e.change == GraphCollectionChange.Collection || e.change == GraphCollectionChange.everything) //if the whole collection or everything changed
            {
                setUpSideTabControl();
                CreateTable();
            }
            if (e.change == GraphCollectionChange.Graph) //if only one graph changed 
            {
                gplist[(int)e.Data].UpdateProperties();
                CreateTable();
            }

        }

        private void setUpSideTabControl()
        {
            if (!SideTabControl.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke((Action)setUpSideTabControl);
            }
            else
            {
                selected_tabindex = SideTabControl.SelectedIndex; //get the last index, set back to it after redoing them
                SideTabControl.Items.Clear();       //delete all Items 

                gplist.Clear();

                for (int i = 0; i < parent.gcollection.Graphs.Count; i++)//for each graph, creat an own tab
                {
                    TabItem ti = new TabItem(); //creat Tab
                    ti.Header = "Plot" + i.ToString(); //set Header to Plot0 for Graphs[0]
                    ti.Style = (Style)FindResource("Style_SideTabItem");//set the Style

                    GraphProperties gp = new GraphProperties(parent.gcollection.Graphs[i], parent.gcollection);
                    gplist.Add(gp);

                    ti.Content = gp;
                    SideTabControl.Items.Add(ti);
                }
            }      
        }

        void bt_connection_MeasuredDataReceived(object sender, ReceivedData_EventArgs e) //received a new Data
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke((Action<object,ReceivedData_EventArgs>)bt_connection_MeasuredDataReceived,sender,e);
            }
            else
            {
                ReceivedData_EventArgs args = e;
                int number = (int)args.ArrayL[0];//get number (are saved in ArrayList) (order is importent)
                int time = (int)args.ArrayL[1];//get time
                int actvalue = (int)args.ArrayL[2]; //get actual value

                int index = parent.gcollection.Graphs.Count - 1; //get the index of the graph to be added

                if (number == 0) //if number is 0 => new Graph
                {
                    parent.gcollection.addGraph(new Measure.Graph(parent.gcollection)); //add Graph
                    index++;        //increase the index to adapt to new last element
                    parent.gcollection.Graphs[index].MeasurementTime = DateTime.Now;  //set the MeasurementTime
                    parent.gcollection.Graphs[index].MeasurementGap = parent.bt_connection.Lastupdated_position;
                }
                parent.gcollection.Graphs[index].AddPoint(new Measure.MeasurementPoint(actvalue, time, Convert.ToInt32(number))); //add the point
                parent.gcollection.setScalingAuto();
                parent.v_plot.DrawDiagram();
            }         
        }


        public void CreateTable()
        {
            if (!stackpanel_dg.Dispatcher.CheckAccess())
            {
                stackpanel_dg.Dispatcher.Invoke((Action)CreateTable);
            }
            else
            {
                stackpanel_dg.Children.Clear(); //delete all Grids
                dglist.Clear();
                for (int i = 0; i < parent.gcollection.Graphs.Count; i++)
                {
                    DataGrid dg = new DataGrid();
                    dg.MouseDoubleClick += dg_MouseDoubleClick;
                    dg.Tag = i;
                    //dg.Height = this.Height;                //setting height to NaN (this.Height is never set) decreases lag
                    dg.AutoGenerateColumns = false;
                    //dg.EnableColumnVirtualization = true;
                    //dg.EnableRowVirtualization = true;
                    dg.MaxHeight = this.ActualHeight;


                    DataGridTextColumn dgtc = new DataGridTextColumn();
                    dgtc.Header = "Number";
                    Binding bindingmeasurementnumber = new Binding();
                    bindingmeasurementnumber.IsAsync = true;
                    bindingmeasurementnumber.Path = new PropertyPath("MeasurementNumber");
                    dgtc.Binding = bindingmeasurementnumber;
                    dg.Columns.Add(dgtc);

                    dgtc = new DataGridTextColumn();
                    dgtc.Header = "Time";
                    Binding bindingtime = new Binding();
                    bindingtime.IsAsync = true;
                    bindingtime.Path = new PropertyPath("Time");
                    dgtc.Binding = bindingtime;
                    dg.Columns.Add(dgtc);

                    dgtc = new DataGridTextColumn();
                    dgtc.Header = "Value";
                    Binding bindingvalue = new Binding();
                    bindingvalue.IsAsync = true;
                    bindingvalue.Path = new PropertyPath("I_Value");
                    dgtc.Binding = bindingvalue;
                    dg.Columns.Add(dgtc);

                    dg.ItemsSource = parent.gcollection.Graphs[i].Mps;
                    stackpanel_dg.Children.Add(dg);
                    dglist.Add(dg);
                }
            }

        }

        void dg_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            if(dg.SelectedIndex!=-1)    //if any was selected
                parent.gcollection.Graphs[(int)(dg.Tag)].highlitepoint(dg.SelectedIndex, !parent.gcollection.Graphs[(int)(dg.Tag)].Mps[dg.SelectedIndex].Highlited); //togle the highlite of the element
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

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double actheight = this.ActualHeight;
            foreach (var item in dglist)
            {
                item.MaxHeight = actheight; //adapt new maxheight
            }
        }
    }
}
