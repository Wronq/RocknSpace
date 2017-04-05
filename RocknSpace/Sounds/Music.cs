using System;
using System.Windows.Media;

namespace RocknSpace.Sounds
{
    static class Music
    {
        private static MediaPlayer player;
        static Music()
        {
            Sound.MusicVolumeChanged += Sound_MusicVolumeChanged;
            player = new MediaPlayer();
            player.Volume = Sound.MusicVolume;
            player.MediaEnded += Player_MediaEnded;
            player.Open(new Uri(Sound.BackgroundPath, UriKind.Relative));
        }

        public static void Play()
        {
            player.Play();
        }

        public static void Stop()
        {
            player.Stop();
        }

        private static void Player_MediaEnded(object sender, EventArgs e)
        {
            player.Position = TimeSpan.Zero;
        }

        private static void Sound_MusicVolumeChanged(object sender, EventArgs e)
        {
            player.Volume = Sound.MusicVolume;
        }
    }
}
