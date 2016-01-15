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
    /// Interaction logic for GameOverScreen.xaml
    /// </summary>
    public partial class GameOverScreen : UserControl
    {
        public GameOverScreen()
        {
            InitializeComponent();
        }

        private void Highscores_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Add(MenuType.Highscores);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Back();
            MenuManager.Add(MenuType.Main);
        }
    }
}
