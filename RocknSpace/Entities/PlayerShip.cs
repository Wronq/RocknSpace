using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using SharpDX;
using RocknSpace.Utils;

namespace RocknSpace
{
    class PlayerShip : PhysicsEntity
    {
        private static PlayerShip instance;
        public static PlayerShip Instance
        {
            get
            {
                if (instance == null) instance = new PlayerShip();
                return instance;
            }
        }

        public PlayerShip() :
            base(new GameShape(new Vector2[] { new Vector2(0, -5) * 4, new Vector2(10, -10) * 4, new Vector2(25, 0) * 4, new Vector2(10, 10) * 4, new Vector2(0, 5) * 4 }))
        {
            Color = new Color4(1, 0, 0, 1);
        }

        public override void Update()
        {
            base.Update();

            if (Keyboard.IsKeyDown(Key.W))
            {
                Force += new Vector2((float)Math.Cos(-Orientation), (float)Math.Sin(-Orientation)) * 5000000.0f;
                MakeExhaustFire();
            }
            if (Keyboard.IsKeyDown(Key.A))
            {
                Torque -= 50000000.0f;
            }
            if (Keyboard.IsKeyDown(Key.D))
            {
                Torque += 50000000.0f;
            }
            if (Keyboard.IsKeyDown(Key.Space))
            {
                float hue1 = rand.NextFloat(0, 6);
                float hue2 = (hue1 + rand.NextFloat(0, 2)) % 6f;
                Color4 color1 = ColorUtil.HSVToColor(hue1, 0.5f, 1);
                Color4 color2 = ColorUtil.HSVToColor(hue2, 0.5f, 1);

                for (int i = 0; i < 120; i++)
                {
                    float speed = 12.0f * (1.0f - 1 / rand.NextFloat(1, 10));

                    Color4 color = Color4.Lerp(color1, color2, rand.NextFloat(0, 1));
                    ParticleManager.CreateParticle(PlayerShip.Instance.Position, color, 190, Vector2.One, rand.NextVector2(speed, speed), 0.3f);
                }
            }
        }

        private void MakeExhaustFire()
        {
            //if (Velocity.LengthSquared() > 0.1f)
            {
                // set up some variables
                //Orientation = Velocity.ToAngle();
                //Quaternion rot = Quaternion.CreateFromYawPitchRoll(0f, 0f, Orientation);

                double t = Time.TotalSeconds;
                // The primary velocity of the particles is 3 pixels/frame in the direction opposite to which the ship is travelling.
                Vector2 baseVel = new Vector2((float)Math.Cos(-Orientation), (float)Math.Sin(-Orientation)).Normal() * -3;
                // Calculate the sideways velocity for the two side streams. The direction is perpendicular to the ship's velocity and the
                // magnitude varies sinusoidally.
                Vector2 perpVel = new Vector2(baseVel.Y, -baseVel.X) * (0.6f * (float)Math.Sin(t * 10));
                Color4 sideColor = new Color4(0.78f, 0.14f, 0.03f, 1);    // deep red
                Color4 midColor = new Color4(1.0f, 0.73f, 0.11f, 1);   // orange-yellow
                Vector2 pos = Position /*+ Vector.Transform(new Vector(-25, 0), rot)*/;   // position of the ship's exhaust pipe.

                // middle particle stream
                Vector2 velMid = baseVel + rand.NextVector2(0, 1);
                ParticleManager.CreateParticle(pos, midColor, 60f, new Vector2(0.5f, 1), velMid*5, 0.3f);

                // side particle streams
                Vector2 vel1 = baseVel + perpVel + rand.NextVector2(0, 0.3f);
                Vector2 vel2 = baseVel - perpVel + rand.NextVector2(0, 0.3f);
                ParticleManager.CreateParticle(pos, sideColor, 60f, new Vector2(0.5f, 1), vel1*5, 0.3f);
                ParticleManager.CreateParticle(pos, sideColor, 60f, new Vector2(0.5f, 1), vel2*5, 0.3f);
            }
        }
    }
}
