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
    /// Interaction logic for PauseScreen.xaml
    /// </summary>
    public partial class PauseScreen : UserControl
    {
        public PauseScreen()
        {
            InitializeComponent();
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
        
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Back();
            MenuManager.Add(MenuType.Main);
        }
    }
}
