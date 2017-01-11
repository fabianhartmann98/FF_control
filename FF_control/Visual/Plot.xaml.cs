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
        private TextBox tb_xmin;
        private TextBox tb_xmax;
        private TextBox tb_ymin;
        private TextBox tb_ymax;
        private Border border_background;
        private Border border_axis;

        private List<TextBox> tb_name;
        private List<Label> l_time;
        private List<Label> l_saveloc;
        private List<Border> border_plot;
        private List<Button> b_plot_remove;

        private Button b_add;

        private int selected_tabindex;

        Point prevmousePosition;

        public Plot(MainWindow p)
        {
            InitializeComponent();
            parent = p;
            selected_tabindex = 0;

            InitializeComponent();
            this.IsVisibleChanged += Plot_IsVisibleChanged;                     

            parent.diagram.Can = can;
            parent.diagram.setScalingAuto();
            DrawDiagram();
        }

        private void setUpSideTabControl()
        {
            selected_tabindex = SideTabControl.SelectedIndex;
            SideTabControl.Items.Clear();
            tb_name = new List<TextBox>();
            l_time = new List<Label>();
            l_saveloc = new List<Label>();
            border_plot = new List<Border>();
            b_plot_remove = new List<Button>();

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
            l_Background.Content = "Background Color:";
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
            l_Axis.Content = "Axis Color:";
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

            #region  Plot Tab
            for (int i = 0; i < parent.diagram.Grpahs.Count; i++)
			{			
                ti = new TabItem();
                ti.Header = "Plot"+i.ToString();
                ti.Style = (Style)FindResource("Style_SideTabItem");

                mainstack = new StackPanel();

                wp = new WrapPanel();
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
                l_time[i].Content=parent.diagram.Grpahs[i].MeasurementTime.ToString();
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
                Label ll_plot0 = new Label();
                ll_plot0.Content = "Color:";
                border_plot.Add(new Border());
                border_plot[i].Width = 50;
                border_plot[i].Height = 20;
                border_plot[i].BorderBrush = Brushes.Black;
                border_plot[i].BorderThickness = new Thickness(1);
                border_plot[i].Background = parent.diagram.Grpahs[i].PlotColor;
                border_plot[i].MouseUp += border_MouseUp;
                border_plot[i].Tag = "plot:"+i.ToString();
                wp.Children.Add(ll_plot0);
                wp.Children.Add(border_plot[i]);
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
            #endregion

            #region Add Plot    
            ti = new TabItem();
            ti.Header = "Add Plot";
            ti.Style = (Style)FindResource("Style_SideTabItem");
            mainstack = new StackPanel();

            wp = new WrapPanel();
            b_add = new Button();
            b_add.Content = "Hinzufügen";
            b_add.Click += b_add_Click;
            wp.Children.Add(b_add);
            mainstack.Children.Add(wp);
            ti.Content = mainstack;

            SideTabControl.Items.Add(ti);

            #endregion

            if (selected_tabindex < SideTabControl.Items.Count)
                SideTabControl.SelectedIndex = selected_tabindex;
           
        }

        void b_add_Click(object sender, RoutedEventArgs e)
        {
            Graph g = Graph.Open();
            if(g!=null)
                parent.diagram.Grpahs.Add(g);
        }

        private void b_plot_remove_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            parent.diagram.Grpahs.RemoveAt((int)b.Tag);
            setUpSideTabControl();
            DrawDiagram(); 
        }

        private void l_saveloc_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Label l = (Label)sender;
            

            parent.diagram.Grpahs[(int)l.Tag].Save();
            l.Content = parent.diagram.Grpahs[(int)l.Tag].SaveLocation;
        }

        public void DrawDiagram()
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
            if (this.IsVisible)
            {
                setUpSideTabControl();
                DrawDiagram();
            }
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
                String s = ((Border)sender).Tag.ToString();
                switch (s.Split(':')[0])
                {
                    case "background":
                        can.Background = b;
                        break;
                    case "axis":
                        parent.diagram.AxisColor = b;
                        parent.diagram.AxisLabelColor = b;
                        DrawDiagram();
                        break;
                    case "plot":
                        parent.diagram.Grpahs[Convert.ToInt32(s.Split(':')[1])].PlotColor = b;
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

        private void can_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            parent.diagram.Can = can;
            DrawDiagram();
        }
    }
}
