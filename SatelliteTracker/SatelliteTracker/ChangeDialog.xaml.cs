using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace SatelliteTracker
{
    /// <summary>
    /// Логика взаимодействия для ChangeDialog.xaml
    /// </summary>
    public partial class ChangeDialog : Window
    {
        public TLESett tleSett = new();

        public ChangeDialog(string oldName, string oldColor, string oldTLE1, string oldTLE2)
        {
            InitializeComponent();
            TLEnameTB.Text = oldName;

            string[] colorParts = oldColor.Split(',');

            if (colorParts.Length == 3)
            {
                byte r = byte.Parse(colorParts[0].Trim());
                byte g = byte.Parse(colorParts[1].Trim());
                byte b = byte.Parse(colorParts[2].Trim());

                ColorPicker.SelectedColor = Color.FromRgb(r, g, b);
            }

            Tle1TB.Text = oldTLE1;
            Tle2TB.Text = oldTLE2;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void OKbutton_Click(object sender, RoutedEventArgs e)
        {
            tleSett.TLEname = TLEnameTB.Text;

            System.Windows.Media.Color selectedColor = Colors.White;

            if (ColorPicker.SelectedColor != null)
            {
                selectedColor =
                    (Color)ColorPicker.SelectedColor;
            }

            tleSett.TLEcolor = $"{selectedColor.R}, {selectedColor.G}, {selectedColor.B}"; 
            tleSett.TLE1 = Tle1TB.Text;
            tleSett.TLE2 = Tle2TB.Text;

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
