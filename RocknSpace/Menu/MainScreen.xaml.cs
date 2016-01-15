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
            GameRoot.Instance.Start();
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            GameRoot.Instance.Resume();
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Add(MenuType.Options);
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
            MenuManager.Clear();
            MenuManager.Add(MenuType.Profiles);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
