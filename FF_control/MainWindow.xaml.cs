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

using FF_control.Bluetooth;
using FF_control.Measure;
using FF_control.Visual; 

namespace FF_control
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BT_connection bt_connection { get; set; }
        public Diagram diagram { get; set; }

        public MainWindow()
        {            
            InitializeComponent();
            Connection_grid.Children.Add(new Connection(this, SideTabControl));
            Control_grid.Children.Add(new FF_control.Visual.Control(this, SideTabControl));
            Plot_grid.Children.Add(new Plot(this, SideTabControl));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var app = App.Current as App;
            app.ChangeToDarkTheme();
        }

        private void TabControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Connection_grid.Width = dockpanel.ActualWidth - 250 - 150;
            Control_grid.Width = dockpanel.ActualWidth - 250 - 150;

        }
    }
}
