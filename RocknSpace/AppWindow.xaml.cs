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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using RocknSpace.DirectWpf;
using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device;
using RocknSpace.Menu;

namespace RocknSpace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        public bool Fullscreen
        {
            get { return WindowState == WindowState.Maximized && WindowStyle == WindowStyle.None; }
            set
            {
                if (value)
                {
                    WindowStyle = WindowStyle.None;
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    WindowState = WindowState.Normal;
                    WindowStyle = WindowStyle.SingleBorderWindow;
                }
            }
        }

        private GameRoot Root;
        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            
            Root = new GameRoot();
            this.Canvas1.Scene = Root;

            Application.Current.Exit += Current_Exit;
            MenuManager.CurrentChanged += MenuManager_CurrentChanged;

            MenuManager.AddInstance(MenuType.Profiles, ScreenProfiles);
            MenuManager.AddInstance(MenuType.Main, ScreenMain);
            MenuManager.AddInstance(MenuType.Controls, ScreenControls);
            MenuManager.AddInstance(MenuType.Highscores, ScreenHighscores);
            MenuManager.AddInstance(MenuType.Options, ScreenOptions);
            MenuManager.AddInstance(MenuType.Help, ScreenHelp);
            MenuManager.AddInstance(MenuType.Profile, ScreenProfile);
            MenuManager.AddInstance(MenuType.HUD, ScreenHUD);
            MenuManager.AddInstance(MenuType.Pause, ScreenPause);

            MenuManager.Add(MenuType.Profiles);

            ProfilesDummy.Instance.CurrentChanged += Instance_CurrentChanged;
        }

        private void Instance_CurrentChanged(object sender, EventArgs e)
        {
            Fullscreen = Profile.Current.Fullscreen;
        }

        private void MenuManager_CurrentChanged(object sender, EventArgs e)
        {
            UserControl current = sender as UserControl;

            if (current == null)
            {
                MenuManager.Add(MenuType.HUD);
                Root.isRunning = true;
            }
            else
                Root.isRunning = false;
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            Profile.Save();
        }
    }
}
