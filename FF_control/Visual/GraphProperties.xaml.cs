using FF_control.Measure;
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
        Diagram diagram;
        public GraphProperties(Graph g, Diagram d)
        {
            InitializeComponent();
            graph = g;
            diagram=d;
            tb_name.Text = g.Name;
            tb_time.Text = g.MeasurementTime.ToString();
            border_StrokeColor.Background = g.PlotColor;
        }

        private void tb_name_LostFocus(object sender, RoutedEventArgs e)
        {
            graph.Name = tb_name.Text;
            OnGraphPropertiesChanged();
        }

        private void b_saveloc_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (!(bool)sfd.ShowDialog())
                return;
            Diagram.Save_graph_xaml(graph, sfd.FileName);
            l_saveloc.Content = graph.SaveLocation;
            OnGraphPropertiesChanged();
        }

        private void border_StrokeColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.SolidColorOnly=true;

            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color c = cd.Color;
                Brush b = new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
                border_StrokeColor.Background = b;
                graph.PlotColor = b;
            }
            OnGraphPropertiesChanged();
        }

        private void b_remove_Click(object sender, RoutedEventArgs e)
        {
            diagram.Grpahs.Remove(graph);
            OnGraphPropertiesChanged();
        }

        public event EventHandler GraphPropertiesChanged;

        protected virtual void OnGraphPropertiesChanged()
        {
            if (GraphPropertiesChanged != null)
                GraphPropertiesChanged(this, new EventArgs());
        }
    }
}
