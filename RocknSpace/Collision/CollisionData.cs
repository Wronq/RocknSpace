using System;
using System.Linq;
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

        public Vector2 rAP, rBP;
        public Vector2 N;
        public float J;
        public float Depth;

        public bool IsCollision;
        private Simplex simplex;

        private CollisionData()
        {
            N = Vector2.Zero;
            IsCollision = false;
        }

        public CollisionData(Entity object1, Entity object2) : this()
        {
            Object1 = object1;
            Object2 = object2;

            simplex = new Simplex(object1.Shape.Vertices.Count() + object2.Shape.Vertices.Count());
        }

        public CollisionData(Entity object1, Vector2 normalVector, float depth, Vector2 point) : this()
        {
            Object1 = object1;
            N = normalVector;
            Depth = depth * 2;

            Point1 = Point2 = point;
            Object2 = Wall.Instance;
            IsCollision = true;
        }

        public void Swap()
        {
            Entity TmpObj = Object1;
            Object1 = Object2;
            Object2 = TmpObj;

            Vector2 TmpPt = Point1;
            Point1 = Point2;
            Point2 = TmpPt;

            TmpPt = rAP;
            rAP = rBP;
            rBP = TmpPt;

            J = -J;
            Depth = -Depth;
        }

        private static float Orient(Vector2 a, Vector2 b)
        {
            return (b.X - a.X) * (-a.Y) - (-a.X) * (b.Y - a.Y);
        }

        public bool Check()
        {
            if (IsCollision) return true;
            if (Object1 == Object2)
                return false;

            Vector2 axis = new Vector2(1, 1);

            simplex.Add(MinkowskiDiff.Support(Object1, Object2, axis));
            simplex.Add(MinkowskiDiff.Support(Object1, Object2, -axis));

            axis = (simplex[1].S - simplex[0].S).Perpendicular();
            if (Orient(simplex[0].S, simplex[1].S) < 0)
                axis = -axis;

            for (int b = 0; b < 20; b++)
            {
                simplex.Add(MinkowskiDiff.Support(Object1, Object2, axis));

                if (simplex[2].S.Dot(axis) <= 0)
                {
                    return false;
                }
                else if (simplex.ContainsOrigin(ref axis))
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
                        Point1 = B.S1;
                        Point2 = B.S2;
                        return true;
                    }
                    if (l2 < 0)
                    {
                        Point1 = A.S1;
                        Point2 = A.S2;
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
            if (!IsCollision || N != Vector2.Zero) return;

            float tolerance = 3f;

            float lastDist = float.MaxValue;

            while (true)
            {
                Simplex.Edge e = simplex.FindClosestEdge();
                MinkowskiDiff p = MinkowskiDiff.Support(Object1, Object2, e.Normal);
                float d = p.S.Dot(e.Normal);
                
                if (d - e.Distance < tolerance || lastDist - d + e.Distance < 1f)
                {
                    N = e.Normal;
                    Depth = d;
                    return;
                }
                simplex.Add(p, e.Index + 1);

                lastDist = d - e.Distance;
            }
        }

        public void CalculateJ()
        {
            PhysicsEntity p1 = (PhysicsEntity)Object1;
            PhysicsEntity p2 = (PhysicsEntity)Object2;
            Vector2 p = (Point1 + Point2) / 2;

            float e = 0.1f; //Restitution

            rAP = (p - p1.Position).Perpendicular();
            rBP = (p - p2.Position).Perpendicular();

            Vector2 vAP = p1.Velocity + p1.Omega * rAP;
            Vector2 vBP = p2.Velocity + p2.Omega * rBP;

            Vector2 vAB = vAP - vBP;

            float j = -(1 + e) * vAB.Dot(N);
            float j1 = N.Dot(N) * (p1.MassInv + p2.MassInv);
            float j2 = (float)Math.Pow(rAP.Dot(N), 2) * p1.InertiaInv;
            float j3 = (float)Math.Pow(rBP.Dot(N), 2) * p2.InertiaInv;
            
            J = j / (j1 + j2 + j3);
        }
    }
}
