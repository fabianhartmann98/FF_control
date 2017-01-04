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

namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for Plot.xaml
    /// </summary>
    public partial class Plot : UserControl
    {
        public MainWindow parent { get; set; }
        public TabControl SideTabControl { get; set; }
        private TextBox tb_xmin;
        private TextBox tb_xmax;
        private TextBox tb_ymin;
        private TextBox tb_ymax;
        private Border border_background;
        private Border border_axis;

        private TextBox tb_name0;
        private Label l_time0;
        private Label l_saveloc0;
        private Border border_plot0;
        private Button b_plot0_remove;

        private TextBox tb_name1;
        private Label l_time1;
        private Label l_saveloc1;
        private Border border_plot1;
        private Button b_plot1_remove;



        private int selected_tabindex;

        Point prevmousePosition;

        public Plot(MainWindow p , TabControl sidetap)
        {
            InitializeComponent();
            parent = p;
            SideTabControl = sidetap;
            selected_tabindex = 0;

            InitializeComponent();
            this.IsVisibleChanged += Plot_IsVisibleChanged;

            parent.diagram = new Diagram();
            parent.diagram.addGraph(Diagram.createTestingPlot());
            Graph g = new Graph();
            g.addPoint(new MeasurementPoint(new Point(-5, 2)));
            g.addPoint(new MeasurementPoint(new Point(-2, 4)));
            g.addPoint(new MeasurementPoint(new Point(2, -2)));
            g.addPoint(new MeasurementPoint(new Point(5, 4)));           
            parent.diagram.addGraph(g);

            parent.diagram.Grpahs[1].PlotColor = Brushes.Blue;
            parent.diagram.Grpahs[0].PlotColor = Brushes.Green;

            parent.diagram.Can = can;
            parent.diagram.setScalingAuto();
            DrawDiagram();
        }

        private void setUpSideTabControl()
        {
            selected_tabindex = SideTabControl.SelectedIndex;
            SideTabControl.Items.Clear();

            #region Tab Diagram
            TabItem ti = new TabItem();
            ti.Header = "Diagram";
            ti.Style = (Style)FindResource("Style_SideTabItem");

            StackPanel mainstack = new StackPanel(); 

            WrapPanel wp = new WrapPanel();
            Label l_xmin = new Label();
            l_xmin.Content = "Xmin:";
            tb_xmin = new TextBox();
            tb_xmin.Text = parent.diagram.AxisXmin.ToString("F2");
            tb_xmin.Tag = "xmin";
            tb_xmin.LostFocus += tb_LostFocus;
            wp.Children.Add(l_xmin);
            wp.Children.Add(tb_xmin);
            mainstack.Children.Add(wp);

            wp = new WrapPanel();
            Label l_xmax = new Label();
            l_xmax.Content = "Xmax:";
            tb_xmax = new TextBox();
            tb_xmax.Text = parent.diagram.AxisXmax.ToString("F2");
            tb_xmax.LostFocus += tb_LostFocus;
            tb_xmax.Tag ="xmax";
            wp.Children.Add(l_xmax);
            wp.Children.Add(tb_xmax);
            mainstack.Children.Add(wp);


            wp = new WrapPanel();
            Label l_ymin = new Label();
            l_ymin.Content = "Ymin:";
            tb_ymin = new TextBox();
            tb_ymin.Text = parent.diagram.AxisYmin.ToString("F2");
            tb_ymin.LostFocus += tb_LostFocus;
            tb_ymin.Tag = "ymin";
            wp.Children.Add(l_ymin);
            wp.Children.Add(tb_ymin);
            mainstack.Children.Add(wp);


            wp = new WrapPanel();
            Label l_ymax = new Label();
            l_ymax.Content = "Ymax:";
            tb_ymax = new TextBox();
            tb_ymax.Text = parent.diagram.AxisYmax.ToString("F2"); ;
            tb_ymax.LostFocus += tb_LostFocus;
            tb_ymax.Tag = "ymax";
            wp.Children.Add(l_ymax);
            wp.Children.Add(tb_ymax);
            mainstack.Children.Add(wp);

            wp = new WrapPanel();
            Label l_Background = new Label();
            l_Background.Content = "BackgroundColor:";
            border_background = new Border();
            border_background.Width = 50;
            border_background.Height = 20;
            border_background.BorderBrush = Brushes.Black;
            border_background.BorderThickness = new Thickness(1);
            border_background.Background = can.Background;
            border_background.MouseUp += border_MouseUp;
            border_background.Tag = "background";
            wp.Children.Add(l_Background);
            wp.Children.Add(border_background);
            mainstack.Children.Add(wp);

            wp = new WrapPanel();
            Label l_Axis = new Label();
            l_Axis.Content = "BackgroundColor:";
            border_axis = new Border();
            border_axis.Width = 50;
            border_axis.Height = 20;
            border_axis.BorderBrush = Brushes.Black;
            border_axis.BorderThickness = new Thickness(1);
            border_axis.Background = parent.diagram.AxisLabelColor;
            border_axis.MouseUp += border_MouseUp;
            border_axis.Tag = "axis";
            wp.Children.Add(l_Axis);
            wp.Children.Add(border_axis);
            mainstack.Children.Add(wp);

            ti.Content = mainstack;
            SideTabControl.Items.Add(ti);
            #endregion

            #region 1. Plot Tab
            if (parent.diagram.Grpahs.Count >= 1)
            {
                ti = new TabItem();
                ti.Header = "Plot1";
                ti.Style = (Style)FindResource("Style_SideTabItem");

                mainstack = new StackPanel();

                wp = new WrapPanel();
                Label ll_name0 = new Label();
                ll_name0.Content = "Name:";
                tb_name0 = new TextBox();
                tb_name0.Text = parent.diagram.Grpahs[0].Name;
                tb_name0.LostFocus += tb_name_LostFocus;
                tb_name0.Tag = 0;
                wp.Children.Add(ll_name0);
                wp.Children.Add(tb_name0);
                mainstack.Children.Add(wp);

                wp = new WrapPanel();
                Label ll_time0 = new Label();
                ll_time0.Content = "Time:";
                l_time0 = new Label(); 
                l_time0.Content=parent.diagram.Grpahs[0].MeasurementTime.ToString();
                wp.Children.Add(ll_time0);
                wp.Children.Add(l_time0);
                mainstack.Children.Add(wp);

                wp = new WrapPanel();
                Label ll_saveloc0 = new Label();
                ll_saveloc0.Content = "Save Location:";
                l_saveloc0 = new Label();
                l_saveloc0.Content = parent.diagram.Grpahs[0].SaveLocation;
                l_saveloc0.MouseUp += l_saveloc_MouseUp;
                wp.Children.Add(ll_saveloc0);
                wp.Children.Add(l_saveloc0);
                mainstack.Children.Add(wp);

                wp = new WrapPanel();
                Label ll_plot0 = new Label();
                ll_plot0.Content = "Color:";
                border_plot0 = new Border();
                border_plot0.Width = 50;
                border_plot0.Height = 20;
                border_plot0.BorderBrush = Brushes.Black;
                border_plot0.BorderThickness = new Thickness(1);
                border_plot0.Background = parent.diagram.Grpahs[0].PlotColor;
                border_plot0.MouseUp += border_MouseUp;
                border_plot0.Tag = "plot:0";
                wp.Children.Add(ll_plot0);
                wp.Children.Add(border_plot0);
                mainstack.Children.Add(wp);

                ti.Content = mainstack;
                SideTabControl.Items.Add(ti);

            }
            #endregion

            #region Tab Plot2
            if (parent.diagram.Grpahs.Count >= 2)
            {
                ti = new TabItem();
                ti.Header = "Plot2";
                ti.Style = (Style)FindResource("Style_SideTabItem");

                mainstack = new StackPanel();

                wp = new WrapPanel();
                Label ll_name1 = new Label();
                ll_name1.Content = "Name:";
                tb_name1 = new TextBox();
                tb_name1.Text = parent.diagram.Grpahs[1].Name;
                tb_name1.LostFocus += tb_name_LostFocus;
                tb_name1.Tag = 1;
                wp.Children.Add(ll_name1);
                wp.Children.Add(tb_name1);
                mainstack.Children.Add(wp);

                wp = new WrapPanel();
                Label ll_time1 = new Label();
                ll_time1.Content = "Time:";
                l_time1 = new Label();
                l_time1.Content = parent.diagram.Grpahs[1].MeasurementTime.ToString();
                wp.Children.Add(ll_time1);
                wp.Children.Add(l_time1);
                mainstack.Children.Add(wp);

                wp = new WrapPanel();
                Label ll_saveloc1 = new Label();
                ll_saveloc1.Content = "Save Location:";
                l_saveloc1 = new Label();
                l_saveloc1.Content = parent.diagram.Grpahs[1].SaveLocation;
                l_saveloc1.MouseUp += l_saveloc_MouseUp;
                wp.Children.Add(ll_saveloc1);
                wp.Children.Add(l_saveloc1);
                mainstack.Children.Add(wp);

                wp = new WrapPanel();
                Label ll_plot1 = new Label();
                ll_plot1.Content = "Color:";
                border_plot1 = new Border();
                border_plot1.Width = 50;
                border_plot1.Height = 20;
                border_plot1.BorderBrush = Brushes.Black;
                border_plot1.BorderThickness = new Thickness(1);
                border_plot1.Background = parent.diagram.Grpahs[1].PlotColor;
                border_plot1.MouseUp += border_MouseUp;
                border_plot1.Tag = "plot:1";
                wp.Children.Add(ll_plot1);
                wp.Children.Add(border_plot1);
                mainstack.Children.Add(wp);

                ti.Content = mainstack;
                SideTabControl.Items.Add(ti);

            }
            #endregion

            if (selected_tabindex < SideTabControl.Items.Count)
                SideTabControl.SelectedIndex = selected_tabindex;
           
        }

        private void l_saveloc_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Label l = (Label)sender;
            SaveFileDialog sfd = new SaveFileDialog();
            if (l != null && "" != l.Content.ToString())
                sfd.InitialDirectory = l.Content.ToString();
            sfd.Filter = Graph.FileFilter;
            if ((bool)sfd.ShowDialog())
            {
                l.Content = sfd.FileName;
                //tode add save
            }
        }

        private void DrawDiagram()
        {
            can.Children.Clear();
            parent.diagram.DrawAxis();
            can = parent.diagram.draw();
            if (tb_xmin != null)
            {
                tb_xmin.Text = parent.diagram.AxisXmin.ToString("F2");
                tb_xmax.Text = parent.diagram.AxisXmax.ToString("F2");
                tb_ymin.Text = parent.diagram.AxisYmin.ToString("F2");
                tb_ymax.Text = parent.diagram.AxisYmax.ToString("F2");
            }
        }

        void Plot_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SideTabControl.SelectedIndex = 0;
            if (this.IsVisible)
                setUpSideTabControl();
        }

        private void tb_name_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            parent.diagram.Grpahs[(int)tb.Tag].Name = tb.Text;
        }

        void border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.SolidColorOnly=true;
            
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color c= cd.Color;
                Brush b = new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
                ((Border)sender).Background=b;

                switch (((Border)sender).Tag.ToString().Split(':')[0])
                {
                    case "background":
                        can.Background = b;
                        break;
                    case "axis":
                        parent.diagram.AxisColor = b;
                        parent.diagram.AxisLabelColor = b;
                        DrawDiagram();
                        break;
                    default:
                        break;
                }
            }
        }

        void tb_LostFocus(object sender, RoutedEventArgs e)
        {
            switch (((TextBox)sender).Tag.ToString())
            {
                case "xmin":
                    parent.diagram.AxisXmin = Convert.ToDouble(((TextBox)sender).Text);
                    break;
                case "xmax":
                    parent.diagram.AxisXmax = Convert.ToDouble(((TextBox)sender).Text);
                    break;
                case "ymin":
                    parent.diagram.AxisYmin = Convert.ToDouble(((TextBox)sender).Text);
                    break;
                case "ymax":
                    parent.diagram.AxisYmax = Convert.ToDouble(((TextBox)sender).Text);
                    break;
                default:
                    break;
            }
            DrawDiagram();
        }

        private void can_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            parent.diagram.Scrole(e.GetPosition(can), e.Delta);
            DrawDiagram();

        }

        private void can_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            { 
                double dx = e.GetPosition(can).X-prevmousePosition.X;
                double dy = e.GetPosition(can).Y-prevmousePosition.Y;

                prevmousePosition=e.GetPosition(can);
                parent.diagram.Shift(-dx, dy);
                DrawDiagram();

            }
        }

        private void can_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            prevmousePosition = e.GetPosition(can);
        }
    }
}
