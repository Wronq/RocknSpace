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

            Root = GameRoot.Instance;
            this.Canvas1.Scene = Root;

            Application.Current.Exit += Current_Exit;
            //MenuManager.CurrentChanged += MenuManager_CurrentChanged;

            MenuManager.Register(MenuType.Profiles, ScreenProfiles);
            MenuManager.Register(MenuType.Main, ScreenMain);
            MenuManager.Register(MenuType.Controls, ScreenControls);
            MenuManager.Register(MenuType.Highscores, ScreenHighscores);
            MenuManager.Register(MenuType.Options, ScreenOptions);
            MenuManager.Register(MenuType.Help, ScreenHelp);
            MenuManager.Register(MenuType.HUD, ScreenHUD);
            MenuManager.Register(MenuType.Pause, ScreenPause);
            MenuManager.Register(MenuType.GameOver, ScreenGameOver);

            MenuManager.Add(MenuType.Profiles);

            Profiles.CurrentChanged += Instance_CurrentChanged;

            Sounds.Music.Play();
        }

        private void Instance_CurrentChanged(object sender, EventArgs e)
        {
            Fullscreen = Profiles.Current.Fullscreen;
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            Sounds.Music.Stop();
            Profiles.Save();
            Highscores.Save();
        }
    }
}
