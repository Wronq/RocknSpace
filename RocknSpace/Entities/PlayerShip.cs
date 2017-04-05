using System;
using System.Windows.Input;
using SharpDX;
using RocknSpace.Utils;
using RocknSpace.Entities;

namespace RocknSpace
{
    public class PlayerShip : PhysicsEntity
    {
        private static PlayerShip instance;
        public static PlayerShip Instance
        {
            get
            {
                if (instance == null) instance = new PlayerShip();
                return instance;
            }
            set
            {
                Disposer.RemoveAndDispose(ref instance);
                instance = value;
            }
        }

        private bool isThrusting = false;

        public PlayerShip() :
            base(new GameShape(new Vector2[] { new Vector2(0, -5) * 4, new Vector2(10, -10) * 4, new Vector2(25, 0) * 4, new Vector2(10, 10) * 4, new Vector2(0, 5) * 4 }))
        {
            Color = new Color4(0, 1, 1, 1);
            Position = new Vector2(0, 0);
            Orientation = (float)Math.PI / 2.0f;
        }

        public override void Update()
        {
            base.Update();

            if (Keyboard.IsKeyDown(Profiles.Current.KeyUp))
            {
                Force += new Vector2((float)Math.Cos(Orientation), (float)Math.Sin(Orientation)) * 5000000.0f;
                if (!isThrusting)
                {
                    Sounds.Jet.Start();
                    isThrusting = true;
                }
                MakeExhaustFire();
            }
            else if (isThrusting)
            {
                Sounds.Jet.Stop();
                isThrusting = false;
            }
            if (Keyboard.IsKeyDown(Profiles.Current.KeyLeft))
            {
                Torque += 50000000.0f;
            }
            if (Keyboard.IsKeyDown(Profiles.Current.KeyRight))
            {
                Torque -= 50000000.0f;
            }
            if (Keyboard.IsKeyDown(Key.Escape))
            {
                if (GameRoot.Instance.isRunning)
                    GameRoot.Instance.Stop();
            }
            if (Keyboard.IsKeyDown(Profiles.Current.KeyStop))
            {
                Velocity *= 0.97f;
                Omega *= 0.97f;

                float hue1 = 6;
                float hue2 = (hue1 + rand.NextFloat(0, 2)) % 6f;
                Color4 color1 = ColorUtil.HSVToColor(hue1, 0.5f, 1);
                Color4 color2 = ColorUtil.HSVToColor(hue2, 0.5f, 1);

                for (int i = 0; i < 30; i++)
                {
                    float speed = 1.0f * (1.0f - 1 / rand.NextFloat(1, 5));

                    Color4 color = Color4.Lerp(color1, color2, rand.NextFloat(0, 1));
                    Vector2 dir = rand.NextVector2(1, 1);
                    ParticleManager.CreateParticle(Instance.Position + dir * 40, color, 190, Vector2.One, dir * speed, 0.2f);
                }
            }
        }

        private void MakeExhaustFire()
        {
            double t = (double)DateTime.Now.Ticks / 1000000.0f;

            Vector2 baseVel = new Vector2(1, 0).Transform(Orientation, Vector2.Zero).Normal() * -3;

            Vector2 perpVel = new Vector2(baseVel.Y, -baseVel.X) * (0.4f * (float)Math.Sin(t));
            Color4 sideColor = new Color4(1.0f, 0.73f, 0.11f, 1);   // orange-yellow
            Color4 midColor = new Color4(0.78f, 0.14f, 0.03f, 1);    // deep red
            Vector2 pos = new Vector2(-40, 0).Transform(Orientation, Position);

            for (int i = 0; i < 3; i++)
            {
                // middle particle stream
                Vector2 velMid = baseVel + rand.NextVector2(0, 0.5f);
                ParticleManager.CreateParticle(pos, midColor, 60f, new Vector2(0.5f, 1), velMid * 5, 0.3f);

                // side particle streams
                Vector2 vel1 = baseVel + perpVel + rand.NextVector2(0, 0.2f);
                Vector2 vel2 = baseVel - perpVel + rand.NextVector2(0, 0.2f);
                ParticleManager.CreateParticle(pos, sideColor, 60f, new Vector2(0.5f, 1), vel1 * 5, 0.3f);
                ParticleManager.CreateParticle(pos, sideColor, 60f, new Vector2(0.5f, 1), vel2 * 5, 0.3f);
            }
        }
    }
}
