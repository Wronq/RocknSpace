using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace RocknSpace.Utils
{
    static class Extensions
    {
        /*public static float NextFloat(this Random rand, float maxValue = 1.0f, float minValue=0.0f)
        {
            return (float)rand.NextDouble() * (maxValue - minValue) + minValue;
        }*/

        public static Vector2 NextVector2(this Random rand, float maxLength = 1.0f, float minLength = 1.0f)
        {
            double theta = rand.NextDouble() * 2 * Math.PI;
            float length = rand.NextFloat(minLength, maxLength);
            
            return new Vector2(length * (float)Math.Cos(theta), length * (float)Math.Sin(theta));
        }

        public static Vector2 Normal(this Vector2 v)
        {
            return v / v.Length();
        }

        public static float ToAngle(this Vector2 v)
        {
            return (float)Math.Atan2(v.Y, v.X);
        }

        public static float Dot(this Vector2 vector1, Vector2 vector2)
        {
            return vector1.X * vector2.X + vector1.Y * vector2.Y;
        }

        public static float Cross(this Vector2 vector1, Vector2 vector2)
        {
            return vector1.X * vector2.Y - vector1.Y * vector2.X;
        }

        public static Vector2 Perpendicular(this Vector2 v)
        {
            return new Vector2(-v.Y, v.X);
        }

        public static Vector2 Transform(this Vector2 v, Matrix3x3 transform)
        {
            return new Vector2(v.X * transform.M11 + v.Y * transform.M12 + transform.M13, v.X * transform.M21 + v.Y * transform.M22 + transform.M23);
        }

        public static Vector2 Transform(this Vector2 v, float rotation, Vector2 translation)
        {
            rotation = -rotation;
            return new Vector2((float)(v.X * Math.Cos(rotation) + v.Y * Math.Sin(rotation) + translation.X), (float)(-v.X * Math.Sin(rotation) + v.Y * Math.Cos(rotation) + translation.Y));
        }
    }
}
