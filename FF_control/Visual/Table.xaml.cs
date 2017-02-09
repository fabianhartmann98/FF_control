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

namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for Table.xaml
    /// </summary>
    public partial class Table : UserControl
    {
        public MainWindow parent { get; set; }              //saves the parent MainWindow (used to access bt_connection and graph) 

        private int selected_tabindex;                      //what tabindex was selected, before redoing SideTabItems

        private List<TextBox> tb_name;                      //saves all Tb where the names of each graph can be modivied
        private List<Label> l_time;                         //saves all Lables where the time of recording gets displayed
        private List<Label> l_gap;                          //saves all Labels where the gap gets displayed
        private List<Label> l_saveloc;                      //saves all Labels where the savelocation is displayed, there is a click event on them right now
        private List<Button> b_plot_remove;                 //saves all Buttons where the Plot can be removed, tag is the index

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
                Stopwatch sw = new Stopwatch();
                sw.Start();
                setUpSideTabControl(); //redo or do the TabItems 
                long x = sw.ElapsedMilliseconds;
                CreateTable();         //redo or do the Tables
                long y = sw.ElapsedMilliseconds;
                sw.Stop();
            }
        }

        private void setUpSideTabControl()
        {
            selected_tabindex = SideTabControl.SelectedIndex; //get the last index, set back to it after redoing them
            SideTabControl.Items.Clear();       //delete all Items 
            tb_name = new List<TextBox>();      //make an instanze
            l_time = new List<Label>();
            l_gap = new List<Label>();
            l_saveloc = new List<Label>();
            b_plot_remove = new List<Button>();

            for (int i = 0; i < parent.diagram.Grpahs.Count; i++)//for each graph, creat an own tab
            {
                TabItem ti = new TabItem(); //creat Tab
                ti.Header = "Plot" + i.ToString(); //set Header to Plot0 for Graphs[0]
                ti.Style = (Style)FindResource("Style_SideTabItem");//set the Style

                StackPanel mainstack = new StackPanel();

                WrapPanel wp = new WrapPanel();     //creat Wrap Panel for the Name of the Plot
                Label ll_name = new Label();
                ll_name.Content = "Name:";
                tb_name.Add(new TextBox());
                tb_name[i].Text = parent.diagram.Grpahs[i].Name; //set the text
                tb_name[i].LostFocus += tb_name_LostFocus;      //using Lost_focus so it is not changing when eddeting
                tb_name[i].Tag = i;                             //Tag is the index (for later use)
                wp.Children.Add(ll_name);                      //add those to the Wrappenel
                wp.Children.Add(tb_name[i]);
                mainstack.Children.Add(wp);                     //add them to the mainstack

                wp = new WrapPanel();               //creat WrapPanel for the time of measurement
                Label ll_time = new Label();
                ll_time.Content = "Time:";
                l_time.Add(new Label());
                l_time[i].Content = parent.diagram.Grpahs[i].MeasurementTime.ToString();    //set the Text
                wp.Children.Add(ll_time);
                wp.Children.Add(l_time[i]);
                mainstack.Children.Add(wp);

                wp = new WrapPanel();               //creat WrapPanel for the gap 
                Label ll_gap = new Label();
                ll_gap.Content = "Gap:";
                l_gap.Add(new Label());
                l_gap[i].Content = parent.diagram.Grpahs[i].MeasurementGap.ToString();
                wp.Children.Add(ll_gap);
                wp.Children.Add(l_gap[i]);
                mainstack.Children.Add(wp);

                wp = new WrapPanel();               //creat WrapPanel for the Save Location
                Label ll_saveloc = new Label();
                ll_saveloc.Content = "Save Location:";
                l_saveloc.Add(new Label());
                l_saveloc[i].Content = parent.diagram.Grpahs[i].SaveLocation;
                l_saveloc[i].MouseUp += l_saveloc_MouseUp;          //open a saveFileDialog if clicked on it
                l_saveloc[i].Tag = i;                               //Tag is the index (for later use)
                wp.Children.Add(ll_saveloc);
                wp.Children.Add(l_saveloc[i]);
                mainstack.Children.Add(wp);

                wp = new WrapPanel();           //creat WrapPanel for removing graph
                b_plot_remove.Add(new Button());
                b_plot_remove[i].Content = "Remove";
                b_plot_remove[i].Click += b_plot_remove_Click;  //if click remove plot and redo tab and creatTable
                b_plot_remove[i].Tag = i;
                wp.Children.Add(b_plot_remove[i]);
                mainstack.Children.Add(wp);

                ti.Content = mainstack;
                SideTabControl.Items.Add(ti);

            }
            
        }

        private void b_plot_remove_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;              //get the button
            parent.diagram.Grpahs.RemoveAt((int)b.Tag); //get the index for the Graphs in the Tag (need casting)
            setUpSideTabControl();                  //redo SideTab
            CreateTable();                          //redo Table
        }

        private void l_saveloc_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Label b = (Label)sender;
            //todo: open SavefileDialog
            parent.diagram.Grpahs[(int)b.Tag].Save(); 
            setUpSideTabControl();
        }

        private void tb_name_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender; 
            parent.diagram.Grpahs[(int)tb.Tag].Name = tb.Text; //reset the name
            setUpSideTabControl();
        }

        void bt_connection_MeasuredDataReceived(object sender, EventArgs e) //received a new Data
        {
            ReceivedData_EventArgs args = (ReceivedData_EventArgs)e;
            double number = (double)args.ArrayL[0];//get number (are saved in ArrayList) (order is importent)
            double time = (double)args.ArrayL[1];//get time
            double actvalue = (double)args.ArrayL[2]; //get actual value

            int index = parent.diagram.Grpahs.Count - 1; //get the index of the graph to be added

            if(number==0) //if number is 0 => new Graph
            {
                parent.diagram.Grpahs.Add(new Measure.Graph()); //add Graph
                index = parent.diagram.Grpahs.Count - 1;        //set index new
                parent.diagram.Grpahs[index].MeasurementTime=DateTime.Now;  //set the MeasurementTime
                parent.diagram.Grpahs[index].MeasurementGap = parent.bt_connection.Lastupdated_position; 
            }
            parent.diagram.Grpahs[index].mps.Add(new Measure.MeasurementPoint(actvalue, time, Convert.ToInt32(number))); //add the point
            parent.v_plot.DrawDiagram();
        }


        public void CreateTable()
        {
            stackpanel_dg.Children.Clear(); //delete all Grids
            for (int i = 0; i < parent.diagram.Grpahs.Count; i++)
            {
                WrapPanel wp = new WrapPanel(); 
                DataGrid dg = new DataGrid();
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
    }
}
