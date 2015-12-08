using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using RocknSpace.Utils;
using RocknSpace.Entities;

namespace RocknSpace.Entities
{
    class Rock : PhysicsEntity
    {
        public float HP;
        public Rock(GameShape Shape)
            : base(Shape)
        {
            Color = new Color4(1, 1, 1, 1);
        }

        public static Rock Create(Vector2 Position, float Mass)
        {
            int numVertices = rand.Next(4, 8);
            float angleSpace = (float)Math.PI * 2 / numVertices;
            Vector2[] points = new Vector2[numVertices];

            for (int i = 0; i < numVertices; i++)
            {
                float angle = i * angleSpace;
                angle = rand.NextFloat(angle - angleSpace / 2 * 0.9f, angle + angleSpace / 2 * 0.9f);
                float dist = (float)(Math.Sqrt(Mass) / Math.PI);

                points[i] = new Vector2(dist * (float)Math.Cos(angle), dist * (float)Math.Sin(angle));
            }

            //points = new Vector2[] { new Vector2(-100, -100), new Vector2(100, -100), new Vector2(100, 100), new Vector2(-100, 100) };
            
            Rock r = new Rock(new GameShape(points));

            return r;
        }

        public override void Update()
        {
            if (this.Position.X < -1000 || this.Position.X > 1000 || this.Position.Y < -1000 || this.Position.Y > 1000)
            {
               // isExpired = true;
            }
            if (HP < 0)
            {
                this.isExpired = true;

                if (this.Mass > 10000.0f)
                {
                    int newPieces = rand.Next(2, 5);
                    float newMass = this.Mass / newPieces * 0.9f;

                    for (int i = 0; i < newPieces; i++)
                        EntityManager.Add(Rock.Create(this.Position, newMass));
                }

                return;
            }

            base.Update();
        }
    }
}
