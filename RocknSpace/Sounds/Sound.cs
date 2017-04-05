using System;
using System.Windows.Media;

namespace RocknSpace.Sounds
{
    static class Sound
    {
        public const string BackgroundPath = "Sounds/background.wav";

        public static event EventHandler SoundVolumeChanged, MusicVolumeChanged;

        private static float soundVolume, musicVolume;
        public static float SoundVolume
        {
            get
            {
                return soundVolume;
            }
            private set
            {
                soundVolume = value;
                SoundVolumeChanged?.Invoke(SoundVolume, EventArgs.Empty);
            }
        }
        public static float MusicVolume
        {
            get
            {
                return musicVolume;
            }
            private set
            {
                musicVolume = value;
                MusicVolumeChanged?.Invoke(MusicVolume, EventArgs.Empty);
            }
        }

        static Sound()
        {
            Profiles.CurrentChanged += Profiles_CurrentChanged;
        }

        private static void Profiles_CurrentChanged(object sender, EventArgs e)
        {
            Profiles.Current.PropertyChanged += Current_PropertyChanged;

            if (Profiles.Current == null)
            {
                SoundVolume = 1.0f;
                MusicVolume = 1.0f;
            }
            else
            {
                SoundVolume = Profiles.Current.Sound;
                MusicVolume = Profiles.Current.Music;
            }
        }

        private static void Current_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Sound":
                    SoundVolume = Profiles.Current.Sound;
                    break;
                case "Music":
                    MusicVolume = Profiles.Current.Music;
                    break;
            }
        }

        public static void Play(string Path)
        {

        }

        private static MediaPlayer player;
        public static void PlayMusic(string Path)
        {
            player = new MediaPlayer();
            
            player.Open(new Uri(Path, UriKind.Relative));
            player.MediaEnded += Music_MediaEnded;
            
            player.Play();
        }

        private static void Music_MediaEnded(object sender, EventArgs e)
        {
            MediaPlayer player = (MediaPlayer)sender;
            player.Position = TimeSpan.Zero;
        }
    }
}
