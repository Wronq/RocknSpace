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
    /// Interaction logic for ControlsScreen.xaml
    /// </summary>
    public partial class ControlsScreen : UserControl
    {
        private string state = null;
        private List<Key> forbiddenKeys = new List<Key>() { Key.Escape };

        public ControlsScreen()
        {
            InitializeComponent();
        }

        private void Key_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            state = btn.ToolTip.ToString();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            MenuManager.Back();
        }

        
        private bool isKeyOk(Key key)
        {
            List<Key> keys = new List<Key>();
            if (forbiddenKeys.Contains(key)) return false;

            if (state != "Up") keys.Add(Profiles.Current.KeyUp);
            if (state != "Left") keys.Add(Profiles.Current.KeyLeft);
            if (state != "Right") keys.Add(Profiles.Current.KeyRight);
            if (state != "Stop") keys.Add(Profiles.Current.KeyStop);

            return !keys.Contains(key);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (state == null) return;
            if (!isKeyOk(e.Key))
            {
                state = null;
                return;
            }

            switch (state)
            {
                case "Up": Profiles.Current.KeyUp = e.Key; break;
                case "Left": Profiles.Current.KeyLeft = e.Key; break;
                case "Right": Profiles.Current.KeyRight = e.Key; break;
                case "Stop": Profiles.Current.KeyStop = e.Key; break;
            }

            state = null;
        }
    }
}
