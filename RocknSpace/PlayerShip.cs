using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using SharpDX;

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

        private Random rand = new Random();

        public PlayerShip()
        {
            float s = 4.0f;
            float dens = 1.0f;

            Shape = new GameShape(new Vector[] { new Vector(0, -5) * s, new Vector(10, -10) * s, new Vector(25, 0) * s, new Vector(10, 10) * s, new Vector(0, 5) * s });

            Mass = Shape.GetMass(dens);
            MassInv = 1 / Mass;

            Inertia = Shape.GetInertia(dens);
            InertiaInv = 1 / Inertia;

            Radius = Shape.GetRadius();
        }
        public override void Update()
        {
            base.Update();

            if (Keyboard.IsKeyDown(Key.W))
            {
                Force += new Vector(Math.Cos(-Orientation), Math.Sin(-Orientation)) * 5000000.0f;
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
                    ParticleManager.CreateParticle(PlayerShip.Instance.Position, color, 190, Vector.One, rand.NextVector(speed, speed), 0.3f);
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
                Vector baseVel = new Vector((float)Math.Cos(-Orientation), (float)Math.Sin(-Orientation)).Normal() * -3;
                // Calculate the sideways velocity for the two side streams. The direction is perpendicular to the ship's velocity and the
                // magnitude varies sinusoidally.
                Vector perpVel = new Vector(baseVel.Y, -baseVel.X) * (0.6f * (float)Math.Sin(t * 10));
                Color4 sideColor = new Color4(0.78f, 0.14f, 0.03f, 1);    // deep red
                Color4 midColor = new Color4(1.0f, 0.73f, 0.11f, 1);   // orange-yellow
                Vector pos = Position /*+ Vector.Transform(new Vector(-25, 0), rot)*/;   // position of the ship's exhaust pipe.

                // middle particle stream
                Vector velMid = baseVel + rand.NextVector(0, 1);
                ParticleManager.CreateParticle(pos, midColor, 60f, new Vector(0.5f, 1), velMid*5, 0.3f);

                // side particle streams
                Vector vel1 = baseVel + perpVel + rand.NextVector(0, 0.3f);
                Vector vel2 = baseVel - perpVel + rand.NextVector(0, 0.3f);
                ParticleManager.CreateParticle(pos, sideColor, 60f, new Vector(0.5f, 1), vel1*5, 0.3f);
                ParticleManager.CreateParticle(pos, sideColor, 60f, new Vector(0.5f, 1), vel2*5, 0.3f);
            }
        }
    }
}
