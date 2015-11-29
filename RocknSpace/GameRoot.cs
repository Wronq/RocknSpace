using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using RocknSpace.DirectWpf;
using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device;
using RocknSpace.Utils;
using RocknSpace.Entities;

namespace RocknSpace
{
    class GameRoot : IScene
    {
        private Random Rand;
        private ISceneHost Host;
        private InputLayout VertexLayout;
        private InputLayout ShapeLayout;
        private Buffer Particles;
        private Buffer Vertices;
        private Effect ParticleEffect;
        private Effect ShapeEffect;

        private DateTime Last = DateTime.Now;
        private int counter = 0;
        private float sumFps = 0;

        public GameRoot()
        {
            Rand = new Random();

            EntityManager.Add(PlayerShip.Instance);
            EntityManager.Add(Rock.Create(new Vector2(0, 0), 500000));
        }

        public void Attach(ISceneHost host)
        {
            this.Host = host;

            Device device = host.Device;
            if (device == null)
                throw new Exception("Scene host device is null");

            try
            {
                ShaderBytecode shaderBytes = ShaderBytecode.CompileFromFile("Shaders\\Particle.fx", "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);
                this.ParticleEffect = new Effect(device, shaderBytes);
            }
            catch
            { }

            try
            {
                ShaderBytecode shaderBytes = ShaderBytecode.CompileFromFile("Shaders\\Shape.fx", "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);
                this.ShapeEffect = new Effect(device, shaderBytes);
            }
            catch
            { }

            EffectTechnique technique = this.ParticleEffect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);

            this.VertexLayout = new InputLayout(device, pass.Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
                new InputElement("POSITION", 1, Format.R32G32_Float, 8, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
            });

            this.ShapeLayout = new InputLayout(device, this.ShapeEffect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0)
            });

            device.Flush();
        }

        public void Detach()
        {
            Disposer.RemoveAndDispose(ref this.Particles);
            Disposer.RemoveAndDispose(ref this.Vertices);
            Disposer.RemoveAndDispose(ref this.VertexLayout);
            Disposer.RemoveAndDispose(ref this.ShapeLayout);
            Disposer.RemoveAndDispose(ref this.ParticleEffect);
            Disposer.RemoveAndDispose(ref this.ShapeEffect);
        }

        public void Update(TimeSpan timeSpan)
        {
            TimeSpan ts = (DateTime.Now - Last);
            Time.Dt = (float)ts.TotalSeconds;

            Last = DateTime.Now;
            double fps = 1 / ts.TotalSeconds;
            sumFps += (float)fps;

            if (counter++ % 120 == 0)
            {
                //this.Title = (sumFps / 120).ToString();
                sumFps = 0;

                for (int i = 0; i < 1200; i++)
                {
                    float speed = 12.0f * (1.0f - 1 / Rand.NextFloat(1, 10));

                    ParticleManager.CreateParticle(new Vector2(350, 250), new Color4(0, 1, 0, 0.3f), 190, Vector2.One, Rand.NextVector2(speed, speed), 0.3f);
                }
                //EntityManager.Entities.Last().isExpired = true;
                EntityManager.Add(Rock.Create(new Vector2(0, 0), 500000));
            }

            EntityManager.Update();
            ParticleManager.Update();

            Device device = Host.Device;

            DataStream particles = new DataStream(ParticleManager.particleList.Count * 32, true, true);
            for (int i = 0; i < ParticleManager.particleList.Count; i++)
            {
                var particle = ParticleManager.particleList[i];

                Vector2 a = new Vector2(particle.Position.X, particle.Position.Y);
                Vector2 b = new Vector2(particle.Scale.X, -particle.Orientation);

                particles.WriteRange(new[] { a, b });
                particles.Write(particle.Color);
            }
            particles.Position = 0;

            Disposer.RemoveAndDispose(ref this.Particles);

            this.Particles = new Buffer(device, particles, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = ParticleManager.particleList.Count * 32,
                Usage = ResourceUsage.Default
            });
        }

        public void Render()
        {
            Device device = this.Host.Device;
            if (device == null)
                return;

            device.InputAssembler.InputLayout = this.VertexLayout;
            device.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;

            EffectTechnique technique = this.ParticleEffect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);
            pass.Apply();

            device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.Particles, 32, 0));
            this.ParticleEffect.GetVariableByName("Projection").AsVector().Set(new Vector2(Host.Width, Host.Height));
            device.Draw(ParticleManager.particleList.Count, 0);

            technique = this.ShapeEffect.GetTechniqueByIndex(0);
            pass = technique.GetPassByIndex(0);
            

            device.InputAssembler.InputLayout = this.ShapeLayout;
            device.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;
           

            foreach (Entity entity in EntityManager.Entities)
            {
                this.Vertices = new Buffer(device, entity.Shape.data, new BufferDescription()
                {
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                    SizeInBytes = entity.Shape.Vertices.Length * 2 * 12 + 24,
                    Usage = ResourceUsage.Default
                });

                device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.Vertices, 12, 0));

                this.ShapeEffect.GetVariableByName("Color").AsVector().Set(entity.Color);
                this.ShapeEffect.GetVariableByName("Position").AsVector().Set(new Vector3(entity.Position, entity.Orientation));
                this.ShapeEffect.GetVariableByName("Projection").AsVector().Set(new Vector2(Host.Width, Host.Height));

                pass.Apply();
                device.Draw(entity.Shape.Vertices.Length * 2 + 2, 0);

                Disposer.RemoveAndDispose(ref this.Vertices);
            }
        }    
    }
}
