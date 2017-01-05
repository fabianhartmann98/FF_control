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


namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for Table.xaml
    /// </summary>
    public partial class Table : UserControl
    {
        public MainWindow parent { get; set; }
        public TabControl SideTabControl { get; set; }

        private int selected_tabindex;

        private List<TextBox> tb_name;
        private List<Label> l_time;
        private List<Label> l_gap;
        private List<Label> l_saveloc;
        private List<Button> b_plot_remove;

        public Table(MainWindow p, TabControl sidetab)
        {
            InitializeComponent();
            parent = p;
            SideTabControl = sidetab;
            selected_tabindex = 0;
            this.IsVisibleChanged += Table_IsVisibleChanged;

            CreateTable();
            parent.bt_connection.MeasuredDataReceived += bt_connection_MeasuredDataReceived;
        }

        private void Table_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
                setUpSideTabControl();
        }

        private void setUpSideTabControl()
        {
            selected_tabindex = SideTabControl.SelectedIndex;
            SideTabControl.Items.Clear();
            tb_name = new List<TextBox>();
            l_time = new List<Label>();
            l_saveloc = new List<Label>();
            b_plot_remove = new List<Button>();

            for (int i = 0; i < parent.diagram.Grpahs.Count; i++)
            {
                TabItem ti = new TabItem();
                ti.Header = "Plot" + i.ToString();
                ti.Style = (Style)FindResource("Style_SideTabItem");

                StackPanel mainstack = new StackPanel();

                WrapPanel wp = new WrapPanel();
                Label ll_name0 = new Label();
                ll_name0.Content = "Name:";
                tb_name.Add(new TextBox());
                tb_name[i].Text = parent.diagram.Grpahs[i].Name;
                tb_name[i].LostFocus += tb_name_LostFocus;
                tb_name[i].Tag = i;
                wp.Children.Add(ll_name0);
                wp.Children.Add(tb_name[i]);
                mainstack.Children.Add(wp);

                wp = new WrapPanel();
                Label ll_time0 = new Label();
                ll_time0.Content = "Time:";
                l_time.Add(new Label());
                l_time[i].Content = parent.diagram.Grpahs[i].MeasurementTime.ToString();
                wp.Children.Add(ll_time0);
                wp.Children.Add(l_time[i]);
                mainstack.Children.Add(wp);

                wp = new WrapPanel();
                Label ll_saveloc0 = new Label();
                ll_saveloc0.Content = "Save Location:";
                l_saveloc.Add(new Label());
                l_saveloc[i].Content = parent.diagram.Grpahs[i].SaveLocation;
                l_saveloc[i].MouseUp += l_saveloc_MouseUp;
                l_saveloc[i].Tag = i;
                wp.Children.Add(ll_saveloc0);
                wp.Children.Add(l_saveloc[i]);
                mainstack.Children.Add(wp);



                wp = new WrapPanel();
                b_plot_remove.Add(new Button());
                b_plot_remove[i].Content = "Remove";
                b_plot_remove[i].Click += b_plot_remove_Click;
                b_plot_remove[i].Tag = i;
                wp.Children.Add(b_plot_remove[i]);
                mainstack.Children.Add(wp);

                ti.Content = mainstack;
                SideTabControl.Items.Add(ti);

            }
            
        }

        private void b_plot_remove_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            parent.diagram.Grpahs.RemoveAt((int)b.Tag);
            setUpSideTabControl();
        }

        private void l_saveloc_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Button b = (Button)sender;
            parent.diagram.Grpahs.RemoveAt((int)b.Tag);
            setUpSideTabControl();
        }

        private void tb_name_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            parent.diagram.Grpahs[(int)tb.Tag].Name = tb.Text;
        }

        void bt_connection_MeasuredDataReceived(object sender, EventArgs e)
        {
            ReceivedData_EventArgs args = (ReceivedData_EventArgs)e;
            double number = (double)args.ArrayL[0];
            double time = (double)args.ArrayL[1];
            double actvalue = (double)args.ArrayL[2];

            int index = parent.diagram.Grpahs.Count - 1;

            if(number==0)
            {
                parent.diagram.Grpahs.Add(new Measure.Graph());
                index = parent.diagram.Grpahs.Count - 1;
                parent.diagram.Grpahs[index].MeasurementTime=DateTime.Now;
                parent.diagram.Grpahs[index].MeasurementGap = parent.bt_connection.Lastupdated_position;
            }
            parent.diagram.Grpahs[index].mps.Add(new Measure.MeasurementPoint(actvalue, time, Convert.ToInt32(number)));
            parent.v_plot.DrawDiagram();
        }


        public void CreateTable()
        {
            dg.DataContext = parent.diagram;
            for (int i = 0; i < parent.diagram.Grpahs.Count; i++)
            {
                DataGridTextColumn dgtc = new DataGridTextColumn();
                dgtc.Header = "Nr" + i.ToString();
                dgtc.Binding = new Binding("Graphs[" + i.ToString() + "].mps");
                dg.Items.Add(dgtc);
            }

        }
    }
}
