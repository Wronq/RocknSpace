using System;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D10;
using Buffer = SharpDX.Direct3D10.Buffer;
using RocknSpace.Utils;
using System.Xml.Serialization;

namespace RocknSpace
{
    public class GameShape : IDisposable, IOnDeserialized
    {
        public Vector2[] Vertices;
        public float Radius;
        
        [XmlIgnore]
        public VertexBufferBinding Buffer;
        [XmlIgnore]
        private Buffer buf;

        public GameShape()
        { }

        public GameShape(Vector2[] vertices)
        {
            this.Vertices = vertices;

            Vector2 Center = Vector2.Zero;

            foreach (Vector2 vertex in vertices)
                Center += vertex;
            Center /= vertices.Count();

            for (int i = 0; i < vertices.Length; i++)
                vertices[i] -= Center;

            CreateBuffers();
        }

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

        private float GetRadius()
        {
            float radius = 0;
            foreach (Vector2 vertex in Vertices)
                if (vertex.LengthSquared() > radius)
                    radius = vertex.LengthSquared();

            return (float)Math.Sqrt(radius);
        }
        
        private void CreateBuffers()
        {
            if (!GameRoot.Initialized) return;

            float width = 4.0f;

            DataStream data = new DataStream((Vertices.Length + 1) * 2 * 12, true, true);
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

            Radius = GetRadius();

            buf = new Buffer(GameRoot.Device, data, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = (Vertices.Length + 1) * 2 * 12,
                Usage = ResourceUsage.Default
            });

            Buffer = new VertexBufferBinding(buf, 12, 0);
        }

        public void Dispose()
        {
            Disposer.RemoveAndDispose(ref buf);
        }

        public void OnDeserialized()
        {
            CreateBuffers();
        }
    }
}
