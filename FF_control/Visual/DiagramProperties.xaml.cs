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

namespace FF_control.Visual
{
    /// <summary>
    /// Interaction logic for DiagramProperties.xaml
    /// </summary>
    public partial class DiagramProperties : UserControl
    {
        Diagram diagram;
        public DiagramProperties(Diagram d )
        {
            InitializeComponent();
            diagram = d;
            if (tb_xmin != null)
            {
                tb_xmin.Text = diagram.AxisXmin.ToString("F2");      //"F2" used for #,##
                tb_xmax.Text = diagram.AxisXmax.ToString("F2");
                tb_ymin.Text = diagram.AxisYmin.ToString("F2");
                tb_ymax.Text = diagram.AxisYmax.ToString("F2");
            }
            border_AxisColor.Background = d.AxisColor;
            border_BackgroundColor.Background = d.BackgroundColor;
        }

        private void tb_minmax_LostFocus(object sender, RoutedEventArgs e)
        {
            switch (((TextBox)sender).Tag.ToString())       //tag specifies which item was changed
            {
                case "xmin":
                    diagram.AxisXmin = Convert.ToDouble(((TextBox)sender).Text);
                    break;
                case "xmax":
                    diagram.AxisXmax = Convert.ToDouble(((TextBox)sender).Text);
                    break;
                case "ymin":
                    diagram.AxisYmin = Convert.ToDouble(((TextBox)sender).Text);
                    break;
                case "ymax":
                    diagram.AxisYmax = Convert.ToDouble(((TextBox)sender).Text);
                    break;
                default:
                    break;
            }
            OnDiagramPropertiesChanged();
        }

        private void tb_minmax_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tb = sender as TextBox;
                tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void border_BackgroundColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.SolidColorOnly = true;

            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color c = cd.Color;
                Brush b = new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
                border_BackgroundColor.Background = b;
                diagram.BackgroundColor = b;
                OnDiagramPropertiesChanged();
            }
        }

        private void border_AxisColor_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.SolidColorOnly = true;

            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color c = cd.Color;
                Brush b = new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
                border_AxisColor.Background = b;
                diagram.AxisColor = b;
                diagram.AxisLabelColor = b;
                OnDiagramPropertiesChanged();
            }
        }

        public void update_minmax()
        { 
        
        }

        public event EventHandler DiagramPropertiesChanged;

        protected virtual void OnDiagramPropertiesChanged()
        {
            if (DiagramPropertiesChanged != null)
                DiagramPropertiesChanged(this, new EventArgs());
        }
    }
}
