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
    /// Interaction logic for MainScreen.xaml
    /// </summary>
    public partial class MainScreen : UserControl
    {
        public MainScreen()
        {
            InitializeComponent();
        }


        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Clear();
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Clear();
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Add(MenuType.Options);
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Add(MenuType.Profile);
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Add(MenuType.Help);
        }

        private void Highscores_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Add(MenuType.Highscores);
        }

        private void ChangeProfile_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Back();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
