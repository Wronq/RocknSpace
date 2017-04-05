using System;
using SharpDX;
using RocknSpace.Utils;

namespace RocknSpace.Entities
{
    public class Rock : PhysicsEntity
    {
        public Rock() { }

        public Rock(GameShape Shape)
            : base(Shape)
        {
            Color = new Color4(1, 1, 1, 1);
        }

        public static Rock Create(float maxSize = 280.0f)
        {
            bool isCollision = false;
            float radius = rand.NextFloat(60, maxSize);
            Vector2 position = Vector2.Zero;

            do
            {
                isCollision = false;
                switch (rand.Next(3))
                {
                    case 0: position = new Vector2(rand.NextFloat(-GameRoot.Width, GameRoot.Width), GameRoot.Height + PlayerShip.Instance.Position.Y + radius * 4); break;
                    case 1: position = new Vector2(GameRoot.Width + radius * 4, rand.NextFloat(0, GameRoot.Height) + PlayerShip.Instance.Position.Y); break;
                    case 2: position = new Vector2(-GameRoot.Width - radius * 4, rand.NextFloat(0, GameRoot.Height) + PlayerShip.Instance.Position.Y); break;
                }

                foreach (Entity e in EntityManager.Entities)
                {
                    if ((e.Position - position).LengthSquared() < (e.Shape.Radius + radius) * (e.Shape.Radius + radius))
                    {
                        isCollision = true;
                        break;
                    }
                }
            }
            while (isCollision);

            Rock rock = Create(position, radius);

            rock.Velocity = (PlayerShip.Instance.Position - position + rand.NextVector2(400, 0)).Normal() * rand.NextFloat(60, 190);
            rock.Omega = rand.NextFloat(-5, 5);

            return rock;
        }

        public static Rock Create(Vector2 position, float radius)
        {
            int numVertices = rand.Next(5, 8);
            float angleSpace = (float)Math.PI * 2 / numVertices;
            Vector2[] points = new Vector2[numVertices];

            for (int i = 0; i < numVertices; i++)
            {
                float angle = i * angleSpace;
                angle = rand.NextFloat(angle - angleSpace / 2 * 0.9f, angle + angleSpace / 2 * 0.9f);

                points[i] = new Vector2(radius * (float)Math.Cos(angle), radius * (float)Math.Sin(angle));
            }

            Rock r = new Rock(new GameShape(points));
            r.Position = position;

            return r;
        }

        public override void Update()
        {
            if (Math.Abs(Position.X) > GameRoot.Width + Shape.Radius * 4 + 100|| Math.Abs(this.Position.Y - PlayerShip.Instance.Position.Y) > GameRoot.Height + Shape.Radius * 4 + 100)
               isExpired = true;

            float dist = Position.Y - Profiles.Current.State.LinesPosition;
            if (dist < 300.0f)
            {
                if (dist < -300.0f)
                    isExpired = true;
                else
                {
                    dist = (dist + 300.0f) / 600.0f;

                    Color.Green = Color.Blue = dist;
                }
            }
            base.Update();
        }
    }
}
