using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using RocknSpace;
using SharpDX;
using RocknSpace.Utils;
using RocknSpace.Entities;

namespace RocknSpace.Collision
{
    public class CollisionData
    {
        public Entity Object1;
        public Entity Object2;

        public Vector2 Point1;
        public Vector2 Point2;

        public Vector2 MTV;
        public Vector2 N;
        public float Depth;

        public bool IsCollision;
        private Simplex simplex;

        public CollisionData(Entity Object1, Entity Object2)
        {
            this.Object1 = Object1;
            this.Object2 = Object2;

            this.IsCollision = false;
            simplex = new Simplex(Object1.Shape.Vertices.Count() + Object2.Shape.Vertices.Count());
        }

        public void Swap()
        {
            Entity TmpObj = Object1;
            Object1 = Object2;
            Object2 = TmpObj;

            Vector2 TmpPt = Point1;
            Point1 = Point2;
            Point2 = TmpPt;

            N = -N;
        }

        private static Vector2 GetClosestPoint(Vector2 v0, Vector2 v1)
        {
            float C = v0.Dot(v1 - v0);

            Vector2 v2 = (v1 - v0) * C / (v1 - v0).LengthSquared();

            return v2;
        }

        private static float GetDistanceFromOrig(Vector2 p1, Vector2 p2)
        {
            return Math.Abs(p1.Cross(p2)) / (float)Math.Sqrt((p1.Y - p2.Y) * (p1.Y - p2.Y) + (p2.X - p1.X) * (p2.X - p1.X));
        }

        private static bool IsOrigInside(Vector2 A, Vector2 B, Vector2 C)
        {      
            Vector2 v0 = C - A;
            Vector2 v1 = B - A;
            Vector2 v2 = -A;

            float dot00 = Vector2.Dot(v0, v0);
            float dot01 = Vector2.Dot(v0, v1);
            float dot02 = Vector2.Dot(v0, v2);
            float dot11 = Vector2.Dot(v1, v1);
            float dot12 = Vector2.Dot(v1, v2);

            float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            return u >= 0 && v >= 0 && u + v < 1;
        }

        private static float Orient(Vector2 A, Vector2 B)
        {
            return (B.X - A.X) * (-A.Y) - (-A.X) * (B.Y - A.Y);
        }

        public bool Check()
        {
            if (Object1 == Object2)
                return false;

            Vector2 axis = new Vector2(1, 1);

            simplex.Add(MinkowskiDiff.Support(Object1, Object2, axis));
            simplex.Add(MinkowskiDiff.Support(Object1, Object2, -axis));

            //axis = GetClosestPoint(simplex[0].S, simplex[1].S);
            axis = (simplex[1].S - simplex[0].S).Perpendicular();
            if (Orient(simplex[0].S, simplex[1].S) < 0)
                axis = -axis;

            for (int b = 0; b < 10; b++)
            {
                simplex.Add(MinkowskiDiff.Support(Object1, Object2, axis));

                if (simplex[2].S.Dot(axis) <= 0)
                {
                    return false;
                }
                else if (simplex.containsOrigin(ref axis))
                {
                    float do1 = simplex[0].S.LengthSquared();
                    float do2 = simplex[1].S.LengthSquared();
                    float do3 = simplex[2].S.LengthSquared();

                    MinkowskiDiff A, B;
                    if (do1 > do2 && do1 > do3)
                    {
                        A = simplex[1];
                        B = simplex[2];
                    }
                    else if (do2 > do1 && do2 > do3)
                    {
                        A = simplex[0];
                        B = simplex[2];
                    }
                    else
                    {
                        A = simplex[0];
                        B = simplex[1];
                    }

                    IsCollision = true;

                    if (A.S == B.S)
                    {
                        Point1 = A.S1;
                        Point2 = A.S2;
                    }

                    Vector2 L = B.S - A.S;

                    float l2 = -L.Dot(A.S) / L.Dot(L);
                    float l1 = 1 - l2;

                    if (l1 < 0)
                    {
                        Point1 = B.S2;
                        Point2 = B.S2;
                        return true;
                    }
                    if (l2 < 0)
                    {
                        Point1 = A.S1;
                        Point2 = A.S1;
                        return true;
                    }

                    Point1 = l1 * A.S1 + l2 * B.S1;
                    Point2 = l1 * A.S2 + l2 * B.S2;
                    return true;
                }
            }

            return false;
        }

        public void CalculateN()
        {
            if (!IsCollision) return;

            float tolerance = 0.1f;

            while (true)
            {
                Simplex.Edge e = simplex.FindClosestEdge();
                MinkowskiDiff p = MinkowskiDiff.Support(Object1, Object2, e.Normal);
                float d = p.S.Dot(e.Normal);

                if (d - e.Distance < tolerance)
                {
                    N = e.Normal;
                    Depth = d;
                    return;
                }
                simplex.Add(p, e.Index + 1);
            }
        }

        public static Vector2 Check(Entity Object, Vector2 Point)
        {
            float overlap = float.MaxValue;
            Vector2 direction = Vector2.Zero;

            for (int j = 0; j < Object.Shape.Axes.Length; j++)
            {
                Vector2 axis = Object.Shape.Axes[j];

                Vector2 rotatedAxis = axis.Transform(Object.Orientation, Vector2.Zero).Normal();

                float min1 = rotatedAxis.Dot(Object.Shape.Vertices[0].Transform(Object.Orientation, Object.Position));
                float max1 = min1;
                float pt2 = rotatedAxis.Dot(Point);

                for (int i = 1; i < Object.Shape.Vertices.Length; i++)
                {
                    float dot = rotatedAxis.Dot(Object.Shape.Vertices[i].Transform(Object.Orientation, Object.Position));
                    if (min1 > dot) min1 = dot;
                    else if (max1 < dot) max1 = dot;
                }

                if (min1 > pt2 || max1 < pt2)
                    return Vector2.Zero;
                else
                {
                    float o = Math.Min(Math.Abs(pt2 - min1), Math.Abs(pt2 - max1));

                    if (o < overlap)
                    {
                        overlap = o;

                        direction = rotatedAxis;
                        if (Math.Abs(pt2 - min1) > Math.Abs(pt2 - max1))
                            direction = -direction;
                    }
                }
            }

            return direction.Normal() * overlap;
        }
    }
}
