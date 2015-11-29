using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device;
using RocknSpace.Utils;

namespace RocknSpace
{
    class GameShape
    {
        public Vector2[] Vertices;
        public Vector2[] Axes;

        public DataStream data;

        public float GetInertia(float density = 1.0f)
        {
            float inertia = 0;
            for (int i = 0; i < Vertices.Length; i++)
            {
                int k = i == Vertices.Length - 1 ? 0 : i + 1;
                
                inertia += density / 12.0f * Math.Abs(Vertices[k].Cross(Vertices[i])) * (Vertices[i].LengthSquared() + Vertices[i].Dot(Vertices[k]) + Vertices[k].LengthSquared());
            }
            
            return inertia;
        }

        public float GetMass(float density = 1.0f)
        {
            float mass = 0;

            for (int i = 0; i < Vertices.Length; i++)
            {
                int k = i == Vertices.Length - 1 ? 0 : i + 1;

                mass += Vertices[i].X * Vertices[k].Y - Vertices[i].Y * Vertices[k].X;
            }

            return (float)Math.Abs(mass) / 2.0f * density;
        }

        public float GetRadius()
        {
            float radius = 0;
            foreach (Vector2 vertex in Vertices)
                if (vertex.LengthSquared() > radius)
                    radius = vertex.LengthSquared();

            return (float)Math.Sqrt(radius);
        }

        public GameShape(Vector2[] Vertices)
        {
            this.Vertices = Vertices;

            Vector2 Center = Vector2.Zero;

            foreach (Vector2 vertex in Vertices)
                Center += vertex;
            Center /= Vertices.Count();

            for (int i = 0; i < Vertices.Length; i++)
                Vertices[i] -= Center;

            Axes = new Vector2[Vertices.Length];

            for (int i = 0; i < Vertices.Length; i++)
            {
                int k = i == Vertices.Length - 1 ? 0 : i + 1;

                Axes[i] = (Vertices[k] - Vertices[i]).Perpendicular();
            }

            float width = 4.0f;

            data = new DataStream(Vertices.Length * 2 * 24 + 48, true, true);
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vector3 a = new Vector3(Vertices[i].X, Vertices[i].Y, 1);
                float len = a.Length();
                Vector3 b = new Vector3(Vertices[i].X * (1 - width / len), Vertices[i].Y * (1 - width / len), 1);

                data.Write(a);
                data.Write(b);
            }

            data.Write(new Vector3(Vertices[0].X, Vertices[0].Y, 1));
            float len_ = Vertices[0].Length();
            data.Write(new Vector3(Vertices[0].X * (1 - width / len_), Vertices[0].Y * (1 - width / len_), 1));

            data.Position = 0;
        }
    }
}
