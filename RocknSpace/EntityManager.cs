using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
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
        public static BVHTree<CircleShape> bvhTree = new BVHTree<CircleShape>();

        static bool isUpdating = false;
        public static int Count { get { return Entities.Count; } }

        public static void Add(Entity Entity)
        {
            if (isUpdating)
                addedEntities.Add(Entity);
            else
                Entities.Add(Entity);      
        }

        public static void Update()
        {
            isUpdating = true;

            foreach (Entity entity in Entities)
                entity.PreUpdate();

            bvhTree.Recalculate();
            IEnumerable<CollisionData> Collisions = bvhTree.GetProbableCollisions();

            foreach (CollisionData data in Collisions)
            {
                bool collision = data.Check();
                if (collision)
                {
                    Random rand = new Random();
                    data.CalculateN();

                    ((PhysicsEntity)data.Object1).Collision(data);
                    data.Swap();
                    ((PhysicsEntity)data.Object1).Collision(data);
                    
                    for (int i = 0; i < 5; i++)
                    {
                        float speed = 1.0f * (1.0f - 1 / rand.NextFloat(1, 3));

                        ParticleManager.CreateParticle(data.Point1, new Color4(1, 1, 1, 1), 190, Vector2.One, data.N * speed, 1.3f);
                        ParticleManager.CreateParticle(data.Point2, new Color4(0, 1, 1, 1), 190, Vector2.One, data.N * speed, 1.3f);
                    }
                    PlayerShip.Instance.Color = new SharpDX.Color4(0, 1, 0, 1);
                }
                else
                    PlayerShip.Instance.Color = new SharpDX.Color4(1, 0, 0, 1);
            }

            foreach (Entity entity in Entities)
                entity.Update();

            foreach (Entity entity in Entities)
                entity.PostUpdate();

            isUpdating = false;

            Entities.RemoveAll(e => e.isExpired);
            Entities.AddRange(addedEntities);

            addedEntities.Clear();
        }
    }
}
