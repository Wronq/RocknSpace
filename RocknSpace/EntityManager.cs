using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RocknSpace
{
    class EntityManager
    {
        static List<Entity> Entities = new List<Entity>();
        static List<Entity> addedEntities = new List<Entity>();

        static bool isUpdating = false;
        public static int Count { get { return Entities.Count; } }

        public static IEnumerable<GameShape> GetShapes()
        {
            return from e in Entities
                   select e.Shape;
        }

        public static void Add(Entity Entity)
        {
            if (isUpdating)
                addedEntities.Add(Entity);
            else
            {
                Entities.Add(Entity);      
            }
        }

        public static void Update()
        {
            isUpdating = true;

            foreach (Entity entity in Entities)
                entity.PreUpdate();

            foreach (Entity entity in Entities)
                entity.Update();

            foreach (Entity entity in Entities)
                entity.PostUpdate();

            isUpdating = false;

            Entities.AddRange(addedEntities);
            Entities.RemoveAll(e => e.isExpired);

            addedEntities.Clear();
        }
    }
}
