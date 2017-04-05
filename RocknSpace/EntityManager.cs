using System;
using System.Collections.Generic;
using RocknSpace.Collision;
using RocknSpace.Entities;
using SharpDX;
using RocknSpace.Utils;

namespace RocknSpace
{
    class EntityManager
    {
        public static List<Entity> Entities = new List<Entity>();
        private static List<Entity> addedEntities = new List<Entity>();

        static bool isUpdating = false;
        public static int Count { get { return Entities.Count; } }

        public static void Add(Entity entity)
        {
            if (isUpdating)
                addedEntities.Add(entity);
            else
                Entities.Add(entity);      
        }

        public static void Update()
        {
            isUpdating = true;

            foreach (Entity entity in Entities)
                entity.PreUpdate();

            IEnumerable<CollisionData> Collisions = GetProbableCollisions();

            foreach (CollisionData data in Collisions)
            {
                bool collision = data.Check();
                if (collision)
                {
                    Random rand = new Random();
                    data.CalculateN();
                    data.CalculateJ();

                    ((PhysicsEntity)data.Object1).Collision(data);
                    data.Swap();
                    ((PhysicsEntity)data.Object1).Collision(data);

                    float hue1 = 2;
                    float hue2 = (hue1 + rand.NextFloat(0, 2)) % 6f;
                    Color4 color1 = ColorUtil.HSVToColor(hue1, 0.5f, 1);
                    Color4 color2 = ColorUtil.HSVToColor(hue2, 0.5f, 1);

                    for (int i = 0; i < 150; i++)
                    {
                        float speed = 12.0f * (1.0f - 1 / rand.NextFloat(1, 3));

                        Color4 color = Color4.Lerp(color1, color2, rand.NextFloat(0, 1));
                        Vector2 dir = rand.NextVector2(1, 1);
                        ParticleManager.CreateParticle((data.Point1 + data.Point2) / 2, color, 190, Vector2.One, dir * speed, 0.2f);
                    }
                }
            }

            foreach (Entity entity in Entities)
                entity.Update();

            foreach (Entity entity in Entities)
                entity.PostUpdate();

            isUpdating = false;

            for (int i = Entities.Count - 1; i >= 0; i--)
            {
                Entity ent = Entities[i];
                if (ent.isExpired)
                {
                    Disposer.RemoveAndDispose(ref ent);
                    Entities.RemoveAt(i);
                }
            }

            Entities.AddRange(addedEntities);

            addedEntities.Clear();
        }

        private static IEnumerable<CollisionData> GetProbableCollisions()
        {
            for (int i = 0; i < Entities.Count; i++)
                for (int j = i + 1; j < Entities.Count; j++)
                    if ((Entities[i].Position - Entities[j].Position).LengthSquared() < Math.Pow(Entities[i].Shape.Radius + Entities[j].Shape.Radius, 2))
                        yield return new CollisionData(Entities[i], Entities[j]);

            Entity e = PlayerShip.Instance;
            if (Math.Abs(e.Position.X) + e.Shape.Radius > GameRoot.Width)
            {
                float maxX = 0;
                Vector2 maxPX = Vector2.Zero;
                float dist;

                foreach (Vector2 v in e.Shape.Vertices)
                {
                    Vector2 o = v.Transform(e.Orientation, e.Position);

                    if ((dist = Math.Abs(o.X) - GameRoot.Width) > maxX)
                    {
                        maxX = dist;
                        maxPX = o;
                    }
                }

                if (maxX > 0)
                    yield return new CollisionData(e, new Vector2(maxPX.X < 0 ? -1 : 1, 0), maxX, maxPX);
            }
        }
    }
}
