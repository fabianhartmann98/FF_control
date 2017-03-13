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
        GraphCollection diagram;
        public DiagramProperties(GraphCollection d )
        {
            InitializeComponent();
            diagram = d;
            update_minmax();
            border_AxisColor.Background = d.AxisColor;
            border_BackgroundColor.Background = d.BackgroundColor;
        }

        private void tb_minmax_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (((TextBox)sender).Tag.ToString())       //tag specifies which item was changed
                {
                    case "xmin":
                        diagram.AxisXmin = Convert.ToDouble(((TextBox)sender).Text.Replace('.', ','));
                        break;
                    case "xmax":
                        diagram.AxisXmax = Convert.ToDouble(((TextBox)sender).Text.Replace('.', ','));
                        break;
                    case "ymin":
                        diagram.AxisYmin = Convert.ToDouble(((TextBox)sender).Text.Replace('.', ','));
                        break;
                    case "ymax":
                        diagram.AxisYmax = Convert.ToDouble(((TextBox)sender).Text.Replace('.', ','));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
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
            }
        }

        public void update_minmax()
        {
            if (!tb_xmin.Dispatcher.CheckAccess())
            {
                tb_xmin.Dispatcher.Invoke((Action)update_minmax);
            }
            else
            {
                if (tb_xmin != null)
                {
                    int xdecimals = diagram.XDiffAccuracy + diagram.XLabelPow;
                    if (xdecimals < 0)
                        xdecimals = 0;
                    int ydecimals = diagram.YDiffAccuracy + diagram.YLabelPow;
                    if (ydecimals < 0)
                        ydecimals = 0;
                    tb_xmin.Text = (diagram.AxisXmin / Math.Pow(10, diagram.XLabelPow)).ToString("F" + (xdecimals + 1).ToString()) + "E" + diagram.XLabelPow;
                    tb_xmax.Text = (diagram.AxisXmax / Math.Pow(10, diagram.XLabelPow)).ToString("F" + (xdecimals + 1).ToString()) + "E" + diagram.XLabelPow;
                    tb_ymin.Text = (diagram.AxisYmin / Math.Pow(10, diagram.YLabelPow)).ToString("F" + (ydecimals + 1).ToString()) + "E" + diagram.YLabelPow;
                    tb_ymax.Text = (diagram.AxisYmax / Math.Pow(10, diagram.YLabelPow)).ToString("F" + (ydecimals + 1).ToString()) + "E" + diagram.YLabelPow;
                }
            }
        }
    }
}
