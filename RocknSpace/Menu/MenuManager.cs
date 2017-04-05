using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RocknSpace.Menu
{
    public enum MenuType { Profiles, Main, Options, Controls, Help, Highscores,Pause, GameOver, HUD};

    public static class MenuManager
    {
        private static UserControl Current
        {
            get
            {
                return MenuStack.Count == 0 ? null : MenuStack.Peek();
            }
        }

        private static Stack<UserControl> MenuStack;
        private static Dictionary<MenuType, UserControl> Menus;

        public static event EventHandler CurrentChanged;

        static MenuManager()
        {
            MenuStack = new Stack<UserControl>();
            Menus = new Dictionary<MenuType, UserControl>();
        }

        public static void Register(MenuType type, UserControl menu)
        {
            Menus.Add(type, menu);
        }

        public static void Add(MenuType type)
        {
            if (Current != null)
                Current.Visibility = Visibility.Hidden;

            MenuStack.Push(Menus[type]);
            currentChanged();
        }

        public static void Back()
        {
            if (Current != null)
                Current.Visibility = Visibility.Hidden;

            if (MenuStack.Count > 0)
                MenuStack.Pop();

            currentChanged();
        }

        public static void Clear()
        {
            if (Current != null)
                Current.Visibility = Visibility.Hidden;

            MenuStack.Clear();

            currentChanged();
        }

        private static void currentChanged()
        {
            if (Current != null)
                Current.Visibility = Visibility.Visible;

            CurrentChanged?.Invoke(Current, EventArgs.Empty);
        }
    }
}
