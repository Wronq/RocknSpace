using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices; 

namespace RocknSpace
{
    public class Collision
    {
        /*protected class MinkowskiDiff
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

            public static MinkowskiDiff Support(GameObject Object1, GameObject Object2, Vector2 axis)
            {
                float max1 = axis.Dot(Object1.Shape.Vertices[0].Rotate(Object1.Orientation) + Object1.Position);
                Vector2 maxp1 = Object1.Shape.Vertices[0].Rotate(Object1.Orientation) + Object1.Position;

                float max2 = axis.Dot(Object2.Shape.Vertices[0].Rotate(Object2.Orientation) + Object2.Position);
                Vector2 maxp2 = Object2.Shape.Vertices[0].Rotate(Object2.Orientation) + Object2.Position;

                for (int i = 1; i < Object1.Shape.Vertices.Length; i++)
                {
                    float dot = axis.Dot(Object1.Shape.Vertices[i].Rotate(Object1.Orientation) + Object1.Position);
                    if (max1 < dot)
                    {
                        max1 = dot;
                        maxp1 = Object1.Shape.Vertices[i].Rotate(Object1.Orientation) + Object1.Position;
                    }
                }

                for (int i = 1; i < Object2.Shape.Vertices.Length; i++)
                {
                    float dot = axis.Dot(Object2.Shape.Vertices[i].Rotate(Object2.Orientation) + Object2.Position);
                    if (max2 > dot)
                    {
                        max2 = dot;
                        maxp2 = Object2.Shape.Vertices[i].Rotate(Object2.Orientation) + Object2.Position;
                    }
                }

                return new MinkowskiDiff(maxp1, maxp2);
            }
        }

        protected class Simplex
        {
            public string Matlab
            {
                get
                {
                    string ret = "X = [";
                    for (int i = 0; i < count; i++)
                        ret += points[i].S.X + " ";

                    ret += points[0].S.X;
                    ret += "]; Y = [";

                    for (int i = 0; i < count; i++)
                        ret += points[i].S.Y + " ";
                    ret += points[0].S.Y;
                    ret += "];";


                    return ret.Replace(",", ".");
                }
            }
                
            public class Edge
            {
                public float Distance;
                public Vector2 Normal;
                public int Index;

                public Edge()
                {
                    Distance = float.MaxValue;
                }
            }

            private MinkowskiDiff[] points;
            private int count;

            public Simplex(int Size)
            {
                Size = 200;
                points = new MinkowskiDiff[Size];
                count = 0;
            }

            public MinkowskiDiff this[int i]
            {
                get { return points[i]; }
                set { points[i] = value; }
            }

            public Edge FindClosestEdge()
            {
                Edge closest = new Edge();
                for (int i = 0; i < count; i++)
                {
                    int k = i == count - 1 ? 0 : i + 1;

                    Vector2 e = points[k].S - points[i].S;
                    Vector2 n = e * points[i].S.Dot(e);
                    n.Normalize();

                    float d = n.Dot(points[i].S);
                    if (d < closest.Distance)
                    {
                        closest.Distance = d;
                        closest.Normal = n;
                        closest.Index = i;
                    }
                }
                return closest;
            }

            public bool containsOrigin(ref Vector2 d)
            {
                Vector2 a = points[2].S;
                Vector2 b = points[1].S;
                Vector2 c = points[0].S;

                Vector2 ab = b - a;
                Vector2 ac = c - a;

                Vector2 abPerp = ac * ab.Dot(ab);
                Vector2 acPerp = ab * ac.Dot(ac);

                if (abPerp.Dot(-a) > 0)
                {
                    Remove(0);
                    d = abPerp;
                }
                else
                {
                    if (acPerp.Dot(-a) > 0)
                    {
                        Remove(1);
                        d = acPerp;
                    }
                    else return true;
                }

                return false;
            }

            public void Add(MinkowskiDiff Value)
            {
                points[count++] = Value;
            }

            public void Add(MinkowskiDiff Value, int index)
            {
                for (int i = count - 1; i >= index; i--)
                    points[i + 1] = points[i];

                points[index] = Value;
                count++;
            }

            public void Remove(int Index)
            {
                for (int i = Index; i < count; i++)
                    points[i] = points[i + 1];

                count--;
            }
        }

        public PhysicsObject Object1;
        public PhysicsObject Object2;

        public Vector2 Point1;
        public Vector2 Point2;

        public Vector2 MTV;
        public Vector2 N;
        public float Depth;

        public bool IsCollision;
        private Simplex simplex;

        public Collision(PhysicsObject Object1, PhysicsObject Object2)
        {
            this.Object1 = Object1;
            this.Object2 = Object2;

            this.IsCollision = false;
            simplex = new Simplex(Object1.Shape.Vertices.Count() + Object2.Shape.Vertices.Count());
        }

        public void Swap()
        {
            PhysicsObject TmpObj = Object1;
            Object1 = Object2;
            Object2 = TmpObj;

            Vector2 TmpPt = Point1;
            Point1 = Point2;
            Point2 = TmpPt;
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
            axis = (simplex[1].S - simplex[0].S).GetPerpendicular();
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
                /*
                if (IsOrigInside(simplex[0].S, simplex[1].S, simplex[2].S))
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

                float d1 = GetDistanceFromOrig(simplex[0].S, simplex[1].S);
                float d2 = GetDistanceFromOrig(simplex[1].S, simplex[2].S);
                float d3 = GetDistanceFromOrig(simplex[2].S, simplex[0].S);

                if (float.IsNaN(d1) || float.IsNaN(d2) || float.IsNaN(d3))
                    return false;
                if (d1 < d2 && d1 < d3)
                    simplex.Remove(2);
                else if (d2 < d1 && d2 < d3)
                    simplex.Remove(0);
                else
                    simplex.Remove(1);

                //axis = GetClosestPoint(simplex[0].S, simplex[1].S);
                axis = (simplex[1].S - simplex[0].S).GetPerpendicular();

                if (Orient(simplex[0].S, simplex[1].S) < 0)
                    axis = -axis;*/
            /*}

            return false;
        }

        public void CalculateN()
        {
            if (!IsCollision) return;

            float tolerance = 0.0001f;

            while (true)
            {
                Simplex.Edge e = simplex.FindClosestEdge();
                if (e.Index > 10)
                {

                }
                MinkowskiDiff p = MinkowskiDiff.Support(Object1, Object2, e.Normal);
                float d = p.S.Dot(e.Normal);

                if (d - e.Distance < tolerance)
                {
                    N = e.Normal;
                    Depth = d;
                    return;
                }
                simplex.Add(p, e.Index);
            }
        }

        public static Vector2 Check(GameObject Object, Vector2 Point)
        {
            float overlap = float.MaxValue;
            Vector2 direction = Vector2.Zero;

            for (int j = 0; j < Object.Shape.Axes.Length; j++)
            {
                Vector2 axis = Object.Shape.Axes[j];

                Vector2 rotatedAxis = axis.Rotate(Object.Orientation).GetNormal();

                float min1 = rotatedAxis.Dot(Object.Shape.Vertices[0].Rotate(Object.Orientation) + Object.Position);
                float max1 = min1;
                float pt2 = rotatedAxis.Dot(Point);

                for (int i = 1; i < Object.Shape.Vertices.Length; i++)
                {
                    float dot = rotatedAxis.Dot(Object.Shape.Vertices[i].Rotate(Object.Orientation) + Object.Position);
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

            return direction.ScaleTo(overlap);
        }*/
    }
}
