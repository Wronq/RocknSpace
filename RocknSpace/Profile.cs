using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace RocknSpace
{
    public class ProfilesDummy : INotifyPropertyChanged
    {
        private ObservableCollection<Profile> list;
        private Profile current;

        public ObservableCollection<Profile> List
        {
            get { return list; }
            set { list = value; propertyChanged("List"); }
        }

        public Profile Current
        {
            get { return current; }
            set
            {
                current = value; propertyChanged("Current");
                if(CurrentChanged != null)
                    CurrentChanged(Current, EventArgs.Empty);
            }
        }

        public static ProfilesDummy Instance { get; private set; }

        static ProfilesDummy()
        {
            Instance = new ProfilesDummy();
        }

        private ProfilesDummy() { }

        private void propertyChanged(string Name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CurrentChanged;
    }

    [Serializable]
    public class Profile : INotifyPropertyChanged
    {
        public static Profile Current
        {
            get { return ProfilesDummy.Instance.Current; }
            set { ProfilesDummy.Instance.Current = value; }
        }
        public static ObservableCollection<Profile> Profiles
        {
            get { return ProfilesDummy.Instance.List; }
            set { ProfilesDummy.Instance.List = value; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        static Profile()
        {
            Profiles = new ObservableCollection<Profile>();
            Load();
        }

        private string name;
        private Key keyup, keyleft, keyright, keyshoot, keyshield, keyrocket, keymine;
        private int sound, music;
        private bool blur, fullscreen;

        public string Name
        {
            get { return name; }
            set { name = value; propertyChanged("Name"); }
        }

        public Key KeyUp
        {
            get { return keyup; }
            set { keyup = value; propertyChanged("KeyUp"); }
        }
        public Key KeyLeft
        {
            get { return keyleft; }
            set { keyleft = value; propertyChanged("Key"); }
        }
        public Key KeyRight
        {
            get { return keyright; }
            set { keyright = value; propertyChanged("KeyRight"); }
        }
        public Key KeyShoot
        {
            get { return keyshoot; }
            set { keyshoot = value; propertyChanged("KeyShoot"); }
        }
        public Key KeyShield
        {
            get { return keyshield; }
            set { keyshield = value; propertyChanged("KeyShield"); }
        }
        public Key KeyRocket
        {
            get { return keyrocket; }
            set { keyrocket = value; propertyChanged("KeyRocket"); }
        }
        public Key KeyMine
        {
            get { return keymine; }
            set { keymine = value; propertyChanged("KeyMine"); }
        }

        public int Sound
        {
            get { return sound; }
            set { sound = value; propertyChanged("Sound"); }
        }
        public int Music
        {
            get { return music; }
            set { music = value; propertyChanged("Music"); }
        }

        public bool Blur
        {
            get { return blur; }
            set { blur = value; propertyChanged("Blur"); }
        }
        public bool Fullscreen
        {
            get { return fullscreen; }
            set { fullscreen = value; propertyChanged("Fullscreen"); }
        }

        public Profile() : this(string.Empty) { }
        public Profile(string Name)
        {
            this.Name = Name;
            KeyUp = Key.W;
            KeyLeft = Key.A;
            KeyRight = Key.D;
            KeyShoot = Key.Space;
            KeyShield = Key.LeftShift;
            KeyRocket = Key.LeftCtrl;
            KeyMine = Key.LeftAlt;

            Sound = 100;
            Music = 100;
            Blur = true;
            Fullscreen = false;
        }

        private void propertyChanged(string Name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(Name));
        }

        public static Profile Create(string Name)
        {
            Profile p = new Profile(Name);
            Profiles.Add(p);
            return p;
        }

        public static Profile GetByName(string Name)
        {
            return Profiles.FirstOrDefault(p => p.Name == Name);
        }
        
        public static void Load()
        {
            if (!File.Exists(Settings.ProfilesPath)) return;

            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Profile>));
            TextReader reader = new StreamReader(Settings.ProfilesPath);
            Profiles = (ObservableCollection<Profile>)serializer.Deserialize(reader);
            reader.Close();
        }

        public static void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Profile>));
            TextWriter writer = new StreamWriter(Settings.ProfilesPath);
            serializer.Serialize(writer, Profiles);
            writer.Close();
        }
    }
}
