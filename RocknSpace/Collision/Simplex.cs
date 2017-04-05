using SharpDX;
using RocknSpace.Utils;

namespace RocknSpace.Collision
{
    class Simplex
    {
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
                Vector2 n = points[i].S * e.Dot(e) - e * e.Dot(points[i].S);
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

        public bool ContainsOrigin(ref Vector2 d)
        {
            Vector2 a = points[2].S;
            Vector2 b = points[1].S;
            Vector2 c = points[0].S;

            Vector2 ab = b - a;
            Vector2 ac = c - a;

            Vector2 abPerp = ab * ab.Dot(ac) - ac * ab.Dot(ab);
            Vector2 acPerp = ac * ac.Dot(ab) - ab * ac.Dot(ac);

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

        public void Add(MinkowskiDiff value)
        {
            points[count++] = value;
        }

        public void Add(MinkowskiDiff value, int index)
        {
            index = (count + index) % count;
            for (int i = count - 1; i >= index; i--)
                points[i + 1] = points[i];

            points[index] = value;
            count++;
        }

        public void Remove(int index)
        {
            for (int i = index; i < count; i++)
                points[i] = points[i + 1];

            count--;
        }
    }
}
