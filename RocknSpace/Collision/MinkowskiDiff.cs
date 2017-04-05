using SharpDX;
using RocknSpace.Utils;
using RocknSpace.Entities;

namespace RocknSpace.Collision
{
    class MinkowskiDiff
    {
        public Vector2 S1;
        public Vector2 S2;
        public Vector2 S;

        public MinkowskiDiff(Vector2 S1, Vector2 S2)
        {
            this.S1 = S1;
            this.S2 = S2;
            S = S1 - S2;
        }

        public static MinkowskiDiff Support(Entity object1, Entity object2, Vector2 axis)
        {
            Matrix3x3 transform1, transform2;
            {
                Matrix3x3 rotation1 = Matrix3x3.RotationZ(-object1.Orientation);
                Matrix3x3 translation1 = new Matrix3x3(1, 0, object1.Position.X, 0, 1, object1.Position.Y, 0, 0, 1);
                transform1 = Matrix3x3.Multiply(translation1, rotation1);

                Matrix3x3 rotation2 = Matrix3x3.RotationZ(-object2.Orientation);
                Matrix3x3 translation2 = new Matrix3x3(1, 0, object2.Position.X, 0, 1, object2.Position.Y, 0, 0, 1);
                transform2 = Matrix3x3.Multiply(translation2, rotation2);
            }
            
            float max1 = axis.Dot(object1.Shape.Vertices[0].Transform(transform1));
            Vector2 maxp1 = object1.Shape.Vertices[0].Transform(transform1);

            float max2 = axis.Dot(object2.Shape.Vertices[0].Transform(transform2));
            Vector2 maxp2 = object2.Shape.Vertices[0].Transform(transform2);

            for (int i = 1; i < object1.Shape.Vertices.Length; i++)
            {
                float dot = axis.Dot(object1.Shape.Vertices[i].Transform(transform1));
                if (max1 < dot)
                {
                    max1 = dot;
                    maxp1 = object1.Shape.Vertices[i].Transform(transform1);
                }
            }

            for (int i = 1; i < object2.Shape.Vertices.Length; i++)
            {
                float dot = axis.Dot(object2.Shape.Vertices[i].Transform(transform2));
                if (max2 > dot)
                {
                    max2 = dot;
                    maxp2 = object2.Shape.Vertices[i].Transform(transform2);
                }
            }

            return new MinkowskiDiff(maxp1, maxp2);
        }
    }
}
