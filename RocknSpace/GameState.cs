using SharpDX;
using System;
using System.Collections.Generic;
using RocknSpace.Entities;
using System.Xml.Serialization;
using System.ComponentModel;

namespace RocknSpace
{
    public class GameState : INotifyPropertyChanged
    {
        [XmlIgnore]
        private int score;

        public List<Rock> Rocks;
        public PlayerShip Player;

        public float LinesPosition;
        public int Score
        {
            get { return score; }
            set { score = value; propertyChanged("Score"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public GameState()
        {
            Score = 0;
            LinesPosition = -800.0f;

            EntityManager.Entities.Clear();
            if (GameRoot.Initialized)
            {
                PlayerShip.Instance.Position = Vector2.Zero;
                PlayerShip.Instance.Velocity = Vector2.Zero;
                PlayerShip.Instance.Orientation = (float)Math.PI / 2;
                PlayerShip.Instance.Omega = 0.0f;

                EntityManager.Add(PlayerShip.Instance);
            }
        }

        private void propertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void OnSave()
        {
            Rocks = new List<Rock>();

            foreach (Entity e in EntityManager.Entities)
            {
                if (e is PlayerShip)
                    Player = (PlayerShip)e;
                else
                    Rocks.Add((Rock)e);
            }
        }

        public void OnLoad()
        {
            EntityManager.Entities.Clear();

            Player.OnDeserialized();
            PlayerShip.Instance = Player;
            EntityManager.Add(PlayerShip.Instance);

            foreach (Rock rock in Rocks)
            {
                rock.OnDeserialized();
                EntityManager.Add(rock);
            }
        }
    }
}
