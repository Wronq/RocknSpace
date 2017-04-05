using RocknSpace.Entities;
using SharpDX;

namespace RocknSpace.Collision
{
    interface IShape
    {
        uint MortonCode { get; }
        IShape Cover(IShape shape);
        bool Overlap(IShape shape);
        bool Overlap(Vector2 point);
        IShape CreateFromEntity(Entity entity);
    }
}
