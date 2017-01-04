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
        private Rectangle rec_background;
        private Rectangle rec_axis;
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

        void Plot_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SideTabControl.SelectedIndex = 0;
            if (this.IsVisible)
                setUpSideTabControl();
        }

        private void setUpSideTabControl()
        {
            selected_tabindex = SideTabControl.SelectedIndex;
            SideTabControl.Items.Clear();

            TabItem ti = new TabItem();
            ti.Header = "Diagramm";
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


            ti.Content = mainstack;
            SideTabControl.Items.Add(ti);

            if (selected_tabindex < SideTabControl.Items.Count)
                SideTabControl.SelectedIndex = selected_tabindex;
           
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
