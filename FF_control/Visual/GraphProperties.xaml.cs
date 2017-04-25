using MeasureUtilities;
using Microsoft.Win32;
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

namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for GraphProperties.xaml
    /// </summary>
    public partial class GraphProperties : UserControl
    {
        Graph graph;
        GraphCollection gcollection;
        public GraphProperties(Graph g, GraphCollection d)
        {
            InitializeComponent();
            graph = g;
            gcollection = d;
            tb_name.Text = g.Name;
            tb_time.Text = g.MeasurementTime.ToString();
            string[] splited = g.SaveLocation.Split('\\');
            l_saveloc.Content = splited[splited.Length-1];
            if (g.SaveLocation == null || g.SaveLocation == "")
                l_saveloc.Content = FindResource("s_NotSavedGraphProp"); 
            border_StrokeColor.Background = g.PlotColor;
        }

        private void tb_name_LostFocus(object sender, RoutedEventArgs e)
        {
            if(graph.Name!=tb_name.Text)
                graph.Name = tb_name.Text;
            if (b_remove.IsMouseOver)           //if b_remove was clicked it loses fokus but doesn't call b_remove_Click
                b_remove_Click(b_remove, e);
        }

        private void border_StrokeColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.SolidColorOnly = true;

            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color c = cd.Color;
                Brush b = new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
                border_StrokeColor.Background = b;
                graph.PlotColor = b;
            }
        }

        private void b_remove_Click(object sender, RoutedEventArgs e)
        {
            gcollection.removeGraph(graph);
        }

        public void UpdateProperties()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke((Action)UpdateProperties);
            }
            else
            {
                tb_name.Text = graph.Name;
                tb_time.Text = graph.MeasurementTime.ToString();
                border_StrokeColor.Background = graph.PlotColor;
            }
        }
    }
}
