using System.Collections.ObjectModel;
using System;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Xml.Serialization;

namespace RocknSpace
{
    [Serializable]
    public class Highscores : INotifyPropertyChanged
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

            public Entry(string name, int score)
            {
                PlayerName = name;
                Score = score;
            }

            private void propertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        [XmlIgnore]
        private int lastScore = 0;

        public ObservableCollection<Entry> entries { get; private set; }
        
        public int LastScore
        {
            get { return lastScore; }
            set { lastScore = value; propertyChanged("LastScore"); }
        }

        public static ObservableCollection<Entry> Entries { get { return Instance.entries; } }
        public static Highscores Instance;

        public event PropertyChangedEventHandler PropertyChanged;

        static Highscores()
        {
            Instance = new Highscores();
        }

        public Highscores()
        {
            entries = new ObservableCollection<Entry>();
            Load();
        }
 
        public static void Add()
        {
            Add(Profiles.Current.Name, Profiles.Current.State.Score);
        }

        public static void Add(int score)
        {
            Add(Profiles.Current.Name, score);
        }

        public static void Add(string name, int score)
        {
            Instance.LastScore = score;

            int i = 0;
            for (; i < Entries.Count; i++)
                if (Entries[i].Score < score)
                    break;

            Entries.Insert(i, new Entry(name, score));

            while (Entries.Count > 9)
                Entries.Remove(Entries.FirstOrDefault(e1 => e1.Score == Entries.Min(e2 => e2.Score)));
        }

        private void Load()
        {
            if (!File.Exists(Settings.HighscoresPath)) return;

            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Entry>));
            TextReader reader = new StreamReader(Settings.HighscoresPath);
            entries = (ObservableCollection<Entry>)serializer.Deserialize(reader);
            reader.Close();

            propertyChanged("entries");
        }

        public static void Save()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Entry>));
            TextWriter writer = new StreamWriter(Settings.HighscoresPath);
            serializer.Serialize(writer, Entries);
            writer.Close();
        }

        private void propertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
