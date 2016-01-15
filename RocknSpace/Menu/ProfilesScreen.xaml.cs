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
    /// Interaction logic for ProfilesScreen.xaml
    /// </summary>
    public partial class ProfilesScreen : UserControl
    {
        public ProfilesScreen()
        {
            InitializeComponent();
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            Profiles.Current = Profiles.GetByName(((Button)sender).Content.ToString());
            MenuManager.Add(MenuType.Main);
        }
    
        private void AddNew_Click(object sender, RoutedEventArgs e)
        {
            Profiles.Create(txtName.Text);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
