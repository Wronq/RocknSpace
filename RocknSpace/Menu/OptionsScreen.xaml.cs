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

namespace RocknSpace.Menu
{
    /// <summary>
    /// Interaction logic for OptionsScreen.xaml
    /// </summary>
    public partial class OptionsScreen : UserControl
    {
        public OptionsScreen()
        {
            InitializeComponent();
        }

        private void Controls_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Add(MenuType.Controls);
        }

        private void Blur_Click(object sender, RoutedEventArgs e)
        {
            Profile.Current.Blur = !Profile.Current.Blur;
        }

        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            Profile.Current.Fullscreen = !Profile.Current.Fullscreen;
            MainWindow.Instance.Fullscreen = Profile.Current.Fullscreen;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Back();
        }
    }
}
