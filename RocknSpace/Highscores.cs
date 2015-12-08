using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace RocknSpace
{
    public class HighscoresDummy : INotifyPropertyChanged
    {
        public ObservableCollection<Highscores.Entry> Entries { get; set; }

        public static HighscoresDummy Instance { get; private set; }

        static HighscoresDummy()
        {
            Instance = new HighscoresDummy();
            Instance.Entries = Highscores.Entries;
        }

        private HighscoresDummy() { }
        public void EntriesChanged()
        {
            Entries = Highscores.Entries;

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Entries"));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Highscores
    {
        [Serializable]
        public class Entry : INotifyPropertyChanged
        {
            private string playername;
            private int score;

            public string PlayerName
            {
                get { return playername; }
                set { playername = value; propertyChanged("PlayerName"); }
            }

            public int Score
            {
                get { return score; }
                set { score = value;  propertyChanged("Score"); }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public Entry() : this("", 0) { }

            public Entry(string Name, int Score)
            {
                this.PlayerName = Name;
                this.Score = Score;
            }

            private void propertyChanged(string Name)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(Name));
            }
        }

        public static ObservableCollection<Entry> Entries;
        
        static Highscores()
        {
            Entries = new ObservableCollection<Entry>();
            Load();
        }

        public static void Load()
        {
            if (!File.Exists(Settings.ProfilesPath)) return;

            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Entry>));
            TextReader reader = new StreamReader(Settings.HighscoresPath);
            Entries = (ObservableCollection<Entry>)serializer.Deserialize(reader);
            reader.Close();

            HighscoresDummy.Instance.EntriesChanged();
        }

        public static void Add(int Score)
        {
            Add(Profile.Current.Name, Score);
        }

        public static void Add(string Name, int Score)
        {
            Entries.Add(new Entry(Name, Score));
            HighscoresDummy.Instance.EntriesChanged();
        }

        public static void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Entry>));
            TextWriter writer = new StreamWriter(Settings.HighscoresPath);
            serializer.Serialize(writer, Entries);
            writer.Close();
        }
    }
}
