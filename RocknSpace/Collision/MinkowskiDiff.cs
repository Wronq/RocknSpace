using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static MinkowskiDiff Support(Entity Object1, Entity Object2, Vector2 axis)
        {
            Matrix3x3 transform1, transform2;
            {
                Matrix3x3 rotation1 = Matrix3x3.RotationZ(-Object1.Orientation);
                Matrix3x3 translation1 = new Matrix3x3(1, 0, Object1.Position.X, 0, 1, Object1.Position.Y, 0, 0, 1);
                transform1 = Matrix3x3.Multiply(translation1, rotation1);

                Matrix3x3 rotation2 = Matrix3x3.RotationZ(-Object2.Orientation);
                Matrix3x3 translation2 = new Matrix3x3(1, 0, Object2.Position.X, 0, 1, Object2.Position.Y, 0, 0, 1);
                transform2 = Matrix3x3.Multiply(translation2, rotation2);
            }
            
            float max1 = axis.Dot(Object1.Shape.Vertices[0].Transform(transform1));
            Vector2 maxp1 = Object1.Shape.Vertices[0].Transform(transform1);

            float max2 = axis.Dot(Object2.Shape.Vertices[0].Transform(transform2));
            Vector2 maxp2 = Object2.Shape.Vertices[0].Transform(transform2);

            for (int i = 1; i < Object1.Shape.Vertices.Length; i++)
            {
                float dot = axis.Dot(Object1.Shape.Vertices[i].Transform(transform1));
                if (max1 < dot)
                {
                    max1 = dot;
                    maxp1 = Object1.Shape.Vertices[i].Transform(transform1);
                }
            }

            for (int i = 1; i < Object2.Shape.Vertices.Length; i++)
            {
                float dot = axis.Dot(Object2.Shape.Vertices[i].Transform(transform2));
                if (max2 > dot)
                {
                    max2 = dot;
                    maxp2 = Object2.Shape.Vertices[i].Transform(transform2);
                }
            }

            return new MinkowskiDiff(maxp1, maxp2);
        }
    }
}
