using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Web.WebView2.Core;

namespace SatelliteTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitMap();
            this.WindowState = WindowState.Maximized;
        }

        private async void InitMap()
        {
            await MapView.EnsureCoreWebView2Async();

            string htmlPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "map.html");
            MapView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
        }
    }
}