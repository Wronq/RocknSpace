using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace RocknSpace.Sounds
{
    static class Jet
    {
        private static List<MediaPlayer> players;
        static Jet()
        {
            Sound.SoundVolumeChanged += SoundVolumeChanged;
            players = new List<MediaPlayer>();
            for (int i = 0; i <= 4; i++)
            {
                MediaPlayer player = new MediaPlayer();
                player.Volume = Sound.SoundVolume;
                player.MediaEnded += Player_MediaEnded;
                player.Open(new Uri(string.Format("Sounds/jet0{0}.wav", i), UriKind.Relative));
                players.Add(player);
            }
        }

        public static void Start()
        {
            foreach (MediaPlayer p in players)
                p.Play();
        }

        public static void Stop()
        {
            foreach (MediaPlayer p in players)
                p.Stop();
        }

        private static void Player_MediaEnded(object sender, EventArgs e)
        {
            MediaPlayer p = (MediaPlayer)sender;
            p.Position = TimeSpan.Zero;
        }

        private static void SoundVolumeChanged(object sender, EventArgs e)
        {
            foreach (MediaPlayer p in players)
                p.Volume = Sound.SoundVolume;
        }
    }
}
