using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RocknSpace.Entities;
using SharpDX;

namespace RocknSpace.Collision
{
    interface IShape
    {
        uint MortonCode { get; }

        //void CalculateMorton();
        IShape Cover(IShape shape);
        bool Overlap(IShape shape);
        bool Overlap(Vector2 point);
        IShape CreateFromEntity(Entity Entity);
    }
}
