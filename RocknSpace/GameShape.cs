using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device;


namespace RocknSpace
{
    class GameShape
    {
        public Vector[] Vertices;
        public Vector[] Axes;

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
            foreach (Vector vertex in Vertices)
                if (vertex.LengthSquared() > radius)
                    radius = vertex.LengthSquared();

            return (float)Math.Sqrt(radius);
        }

        public GameShape(Vector[] Vertices)
        {
            this.Vertices = Vertices;

            Vector Center = Vector.Zero;

            foreach (Vector vertex in Vertices)
                Center += vertex;
            Center /= Vertices.Count();

            for (int i = 0; i < Vertices.Length; i++)
                Vertices[i] -= Center;

            Axes = new Vector[Vertices.Length];

            for (int i = 0; i < Vertices.Length; i++)
            {
                int k = i == Vertices.Length - 1 ? 0 : i + 1;

                Axes[i] = (Vertices[k] - Vertices[i]).Perpendicular();
            }


            data = new DataStream(Vertices.Length * 2 * 24 + 48, true, true);
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vector3 a = new Vector3(Vertices[i].X, Vertices[i].Y, 1);
                Vector3 b = new Vector3(Vertices[i].X * 0.9f, Vertices[i].Y * 0.9f, 1);

                data.Write(a);
                data.Write(b);
            }

            data.Write(new Vector3(Vertices[0].X, Vertices[0].Y, 1));
            data.Write(new Vector3(Vertices[0].X * 0.9f, Vertices[0].Y * 0.9f, 1));

            data.Position = 0;
        }
    }
}
