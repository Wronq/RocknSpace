using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocknSpace.Entities
{
    class Wall : PhysicsEntity
    {
        public static Wall Instance;

        static Wall()
        {
            Instance = new Wall();
        }

        public Wall()
        {
            
        }
    }
}
