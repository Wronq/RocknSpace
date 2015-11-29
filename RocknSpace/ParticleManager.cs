using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using SharpDX;
using RocknSpace.Utils;

namespace RocknSpace
{
    static class ParticleManager
    {
        public class Particle
        {
            public Vector2 Position, Velocity;
            public float Orientation;

            public Vector2 Scale = Vector2.One;

            public Color4 Color;
            public float Duration;
            public float PercentLife = 1f;
            public float LengthMultiplier;

            public Particle()
            {
                Orientation = 0;
                Position = Vector2.One * 50;
            }

            public void Update()
            {
                Position += Velocity;
                Orientation = Velocity.ToAngle();

                float speed = Velocity.Length();
                float alpha = Math.Min(1, Math.Min(PercentLife * 2, speed * 1f));
                alpha *= alpha;

                Color.Alpha = (byte)(255 * alpha);

                Scale.X = LengthMultiplier * Math.Min(Math.Min(1f, 0.2f * speed + 0.1f), alpha);

                var pos = Position;
                int width = (int)500;
                int height = (int)500;

                // collide with the edges of the screen
                if (pos.X < -500) Velocity.X = Math.Abs(Velocity.X);
                else if (pos.X > width)
                    Velocity.X = -Math.Abs(Velocity.X);
                if (pos.Y < -500) Velocity.Y = Math.Abs(Velocity.Y);
                else if (pos.Y > height)
                    Velocity.Y = -Math.Abs(Velocity.Y);
                /*
                foreach (var Object in ObjectManager.GetProbableCollisions(particle.Position))
                {
                    Vector2 mtv = Collision.Check(Object, particle.Position);

                    if (mtv != Vector2.Zero)
                    {
                        mtv.Normalize();
                        vel = vel + (2 * vel.Dot(-mtv)) * mtv;
                    }
                }*/
                // denormalized floats cause significant performance issues
                if (Math.Abs(Velocity.X) + Math.Abs(Velocity.Y) < 0.00000000001f)
                    Velocity = Vector2.Zero;

                Velocity *= 0.97f;       // particles gradually slow down
            }
        }

        public class CircularParticleArray
        {
            private int start;
            public int Start
            {
                get { return start; }
                set { start = value % list.Length; }
            }

            public int Count { get; set; }
            public int Capacity { get { return list.Length; } }
            private Particle[] list;

            public CircularParticleArray(int capacity)
            {
                list = new Particle[capacity];
            }

            public Particle this[int i]
            {
                get { return list[(start + i) % list.Length]; }
                set { list[(start + i) % list.Length] = value; }
            }
        }

        private const int capacity = 20000;// * 1024;
        public static CircularParticleArray particleList = new CircularParticleArray(capacity);

        

        static ParticleManager()
        {
            for (int i = 0; i < capacity; i++)
                particleList[i] = new Particle();
        }

        public static void CreateParticle(Vector2 position, Color4 tint, float duration, Vector2 scale, Vector2 velocity, float lengthMultiplier, float theta = 0)
        {
            Particle particle;
            if (particleList.Count == particleList.Capacity)
            {
                particle = particleList[0];
                particleList.Start++;
            }
            else
            {
                particle = particleList[particleList.Count];
                particleList.Count++;
            }

            particle.Position = position;
            particle.Color = tint;

            particle.Duration = duration;
            particle.PercentLife = 1f;
            particle.Scale = scale;
            particle.Orientation = theta;

            particle.Velocity = velocity;
            particle.LengthMultiplier = lengthMultiplier;
        }

        public static void Update()
        {
            int removalCount = 0;
            for (int i = 0; i < particleList.Count; i++)
            {
                var particle = particleList[i];
                particle.Update();
                particle.PercentLife -= 1f / particle.Duration;

                // sift deleted particles to the end of the list
                Swap(particleList, i - removalCount, i);

                // if the particle has expired, delete this particle
                if (particle.PercentLife < 0)
                    removalCount++;
            }
            particleList.Count -= removalCount;
        }

        private static void Swap(CircularParticleArray list, int index1, int index2)
        {
            var temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }
    }

}
