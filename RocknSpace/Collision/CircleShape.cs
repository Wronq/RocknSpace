using System;
using SharpDX;
using RocknSpace.Entities;

namespace RocknSpace.Collision
{
    class CircleShape : IShape
    {
        private Vector2 center;
        private float radius;

        uint IShape.MortonCode
        {
            get
            {
                uint xx = (uint)(center.X * 0xFFFF / 1000);
                uint yy = (uint)(center.Y * 0xFFFF / 1000);

                xx = (xx | xx << 8) & 0x00FF00FF;
                xx = (xx | xx << 4) & 0x0F0F0F0F;
                xx = (xx | xx << 2) & 0x33333333;
                xx = (xx | xx << 1) & 0x55555555;

                yy = (yy | yy << 8) & 0x00FF00FF;
                yy = (yy | yy << 4) & 0x0F0F0F0F;
                yy = (yy | yy << 2) & 0x33333333;
                yy = (yy | yy << 1) & 0x55555555;

                return xx << 1 | yy;
            }
        }

        public CircleShape()
        { }

        public CircleShape(float x, float y, float radius) : this(new Vector2(x, y), radius)
        { }

        public CircleShape(Vector2 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public IShape CreateFromEntity(Entity entity)
        {
            center = entity.Position;
            radius = entity.Shape.Radius;
            return this;
        }

        public IShape Cover(IShape shape)
        {
            CircleShape circle2 = (CircleShape)shape;
            float diffx = center.X - circle2.center.X;
            float diffy = center.Y - circle2.center.Y;
            float dist = (float)Math.Sqrt(diffx * diffx + diffy * diffy);
            float m = (circle2.radius - radius) / 2 / dist + 0.5f;

            return new CircleShape((circle2.center.X - center.X) * m + center.X, (circle2.center.Y - center.Y) * m + center.Y, (dist + radius + circle2.radius) / 2);
        }

        public bool Overlap(IShape shape)
        {
            CircleShape circle2 = (CircleShape)shape;
            return (center - circle2.center).LengthSquared() < (radius + circle2.radius) * (radius + circle2.radius);
        }

        public bool Overlap(Vector2 point)
        {
            return (center - point).LengthSquared() < radius * radius;
        }

        public override string ToString()
        {
            return center.X + ", " + center.Y + ", " + radius;
        }
    }
}
