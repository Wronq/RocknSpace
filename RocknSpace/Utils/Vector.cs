using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RocknSpace.Utils
{
    /*struct Vector
    {
        public float X;
        public float Y;

        public static Vector Zero = new Vector(0, 0);
        public static Vector One = new Vector(1, 1);

        public Vector(float X, float Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public Vector(double X, double Y)
        {
            this.X = (float)X;
            this.Y = (float)Y;
        }

        static public Vector operator *(Vector v, float m)
        {
            return new Vector(v.X * m, v.Y * m);
        }

        static public Vector operator /(Vector v, float m)
        {
            if (m == 0)
                throw new DivideByZeroException();
            return new Vector(v.X / m, v.Y / m);
        }

        static public Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }

        static public Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }

        public float LengthSquared()
        {
            return X * X + Y * Y;
        }

        public Vector Perpendicular()
        {
            return new Vector(-Y, X);
        }

        public Vector Normal()
        {
            return this / this.Length();
        }

        public float ToAngle()
        {
            return (float)Math.Atan2(Y, X);
        }
        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public float Dot(Vector v)
        {
            return X * v.X + Y * v.Y;
        }

        public float Cross(Vector v)
        {
            return X * v.Y - Y * v.X;
        }

        static public implicit operator Point(Vector v)
        {
            return new Point(v.X, v.Y);
        }
    }*/
}
