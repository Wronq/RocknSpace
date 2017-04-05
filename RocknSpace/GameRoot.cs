using System;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using RocknSpace.DirectWpf;
using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device;
using RocknSpace.Utils;
using RocknSpace.Entities;
using RocknSpace.Menu;

namespace RocknSpace
{
    class GameRoot : IScene
    {
        public static GameRoot Instance;
        public static bool Initialized { get; private set; }
        public static float Width { get { return Instance.Host.Width; } }
        public static float Height { get { return Instance.Host.Height; } }
        public static Device Device { get { return Instance.Host.Device; } }

        private Random Rand;
        private ISceneHost Host;
        private InputLayout ParticleLayout, ShapeLayout, LineLayout;
        private Buffer Particles, LineBuffer;
        private Effect ParticleEffect, ShapeEffect, LineEffect;

        private DateTime Last = DateTime.Now;
        private float sumFps = 0;
        private int counter, maxEntities = 8;

        public bool isRunning = false;

        private EffectVectorVariable ShapeProjection, ShapeCamera, ShapeColor, ShapePosition;
        private EffectVectorVariable ParticleProjection, ParticleCamera;
        private EffectPass ParticlePass, ShapePass;

        private const int linesCount = 100;
        private float linesSpeed = 200.0f;

        static GameRoot()
        {
            Instance = new GameRoot();
        }

        private GameRoot()
        {
            Rand = new Random();
        }
        
        public void Attach(ISceneHost host)
        {
            Host = host;

            Device device = host.Device;
            if (device == null)
                throw new Exception("Scene host device is null");

            try
            {
                ShaderBytecode shaderBytes = ShaderBytecode.CompileFromFile("Shaders\\Particle.fx", "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);
                ParticleEffect = new Effect(device, shaderBytes);
            }
            catch
            { }

            try
            {
                ShaderBytecode shaderBytes = ShaderBytecode.CompileFromFile("Shaders\\Shape.fx", "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);
                ShapeEffect = new Effect(device, shaderBytes);
            }
            catch
            { }

            try
            {
                ShaderBytecode shaderBytes = ShaderBytecode.CompileFromFile("Shaders\\Line.fx", "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);
                LineEffect = new Effect(device, shaderBytes);
            }
            catch
            { }
            

            EffectTechnique technique = this.ParticleEffect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);

            this.ParticleLayout = new InputLayout(device, pass.Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
                new InputElement("POSITION", 1, Format.R32G32_Float, 8, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
            });

            this.ShapeLayout = new InputLayout(device, this.ShapeEffect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0)
            });

            this.LineLayout = new InputLayout(device, this.LineEffect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0)
            });

            ParticleProjection = ParticleEffect.GetVariableByName("Projection").AsVector();
            ParticleCamera = ParticleEffect.GetVariableByName("Camera").AsVector();
            
            ShapeCamera = ShapeEffect.GetVariableByName("Camera").AsVector();
            ShapeProjection = ShapeEffect.GetVariableByName("Projection").AsVector();
            ShapeColor = ShapeEffect.GetVariableByName("Color").AsVector();
            ShapePosition = ShapeEffect.GetVariableByName("Position").AsVector();

            ParticlePass = ParticleEffect.GetTechniqueByIndex(0).GetPassByIndex(0);
            ShapePass = ShapeEffect.GetTechniqueByIndex(0).GetPassByIndex(0);

            DataStream lines = new DataStream(linesCount * 8, true, true);
            for (int i = 0; i < linesCount; i++)
                lines.Write(new Vector2(-1.0f + (float)i / (float)linesCount * 2.0f, 0));

            lines.Position = 0;

            Disposer.RemoveAndDispose(ref this.LineBuffer);

            this.LineBuffer = new Buffer(device, lines, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = linesCount * 8,
                Usage = ResourceUsage.Default
            });

            Disposer.RemoveAndDispose(ref lines);

            device.Flush();

            Initialized = true;

        }

        public void Detach()
        {
            Disposer.RemoveAndDispose(ref this.Particles);
            Disposer.RemoveAndDispose(ref this.ParticleLayout);
            Disposer.RemoveAndDispose(ref this.ShapeLayout);
            Disposer.RemoveAndDispose(ref this.ParticleEffect);
            Disposer.RemoveAndDispose(ref this.ShapeEffect);
        }

        public void Start()
        {
            Last = DateTime.Now;
            isRunning = true;
            Profiles.Current.State = new GameState();

            MenuManager.Clear();
            MenuManager.Add(MenuType.HUD);
        }

        public void Resume()
        {
            isRunning = true;
            Last = DateTime.Now;

            MenuManager.Clear();
            MenuManager.Add(MenuType.HUD);
        }

        public void Stop()
        {
            isRunning = false;

            Sounds.Jet.Stop();
            MenuManager.Clear();
            MenuManager.Add(MenuType.Pause);
        }

        public void GameOver()
        {
            isRunning = false;

            Sounds.Jet.Stop();
            Highscores.Add();
            MenuManager.Clear();
            MenuManager.Add(MenuType.GameOver);

            Profiles.Current.State = null;
        }

        public void Update(TimeSpan timeSpan)
        {
            if (!isRunning) return;

            TimeSpan ts = (DateTime.Now - Last);
            Time.Dt = (float)ts.TotalSeconds;
            //Time.Dt = 1.0f / 60.0f;

            Last = DateTime.Now;
            double fps = 1 / ts.TotalSeconds;
            sumFps += (float)fps;

            Profiles.Current.State.Score = Math.Max((int)(PlayerShip.Instance.Position.Y / 10), Profiles.Current.State.Score);

            if (counter++ % 60 == 0)
            {
                {
                    float fa = 0.18618986725025f;
                    float fb = 210125.0f / 156.0f;
                    float fc = 31.0f / 6.0f;

                    maxEntities = (int)(fa * Math.Sqrt(fb + Profiles.Current.State.Score) + fc);
                }
                {
                    float fa = 38.0f / 9.0f;
                    float fb = 70.0f / 9.0f;
                    float fc = 200f;

                    float x = (float)Profiles.Current.State.Score / 1000.0f;
                    linesSpeed = (int)(fa * x * x + fb * x + fc);
                }
            }

            for (int i = EntityManager.Count; i < maxEntities; i++)
                EntityManager.Add(Rock.Create());

            EntityManager.Update();
            ParticleManager.Update();
            Profiles.Current.State.LinesPosition += linesSpeed * Time.Dt;

            if (Profiles.Current.State.LinesPosition > PlayerShip.Instance.Position.Y)
                GameOver();

            Device device = Host.Device;

            if (ParticleManager.particleList.Count > 0)
            {
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

                Disposer.RemoveAndDispose(ref particles);
            }
        }

        public void Render()
        {
            if (!isRunning) return;

            Device device = this.Host.Device;
            if (device == null)
                return;

            device.InputAssembler.InputLayout = this.ParticleLayout;
            device.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;

            ParticlePass.Apply();

            device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.Particles, 32, 0));
            ParticleProjection.Set(new Vector2(Host.Width, Host.Height));
            ParticleCamera.Set(new Vector2(0, -PlayerShip.Instance.Position.Y));
            device.Draw(ParticleManager.particleList.Count, 0);

            device.InputAssembler.InputLayout = this.ShapeLayout;
            device.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;

            ShapeCamera.Set(new Vector2(0, -PlayerShip.Instance.Position.Y));
            ShapeProjection.Set(new Vector2(Host.Width, Host.Height));

            foreach (Entity entity in EntityManager.Entities)
            {
                device.InputAssembler.SetVertexBuffers(0, entity.Shape.Buffer);

                ShapeColor.Set(entity.Color);
                ShapePosition.Set(new Vector3(entity.Position, entity.Orientation));
                
                ShapePass.Apply();
                device.Draw(entity.Shape.Vertices.Length * 2 + 2, 0);
            }

            if (Math.Abs(Profiles.Current.State.LinesPosition - PlayerShip.Instance.Position.Y) < Height)
            {
                device.InputAssembler.InputLayout = this.LineLayout;
                device.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.PointList;

                device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(LineBuffer, 8, 0));

                LineEffect.GetVariableByName("Step").AsScalar().Set(0.1f);
                LineEffect.GetVariableByName("Width").AsScalar().Set(2.0f / Width);
                LineEffect.GetVariableByName("Height").AsScalar().Set(1.0f - Math.Abs(Profiles.Current.State.LinesPosition - PlayerShip.Instance.Position.Y) / Height);
                LineEffect.GetVariableByName("Color").AsVector().Set(Color.Yellow);

                Vector4[] disturb = new Vector4[16];
                float maxX = 0.05f, maxY = 0.02f;

                for (int i = 0; i < linesCount; i++)
                {
                    for (int k = 0; k < 16; k++)
                        disturb[k] = new Vector4(Rand.NextFloat(-maxX, maxX), Rand.NextFloat(-maxY, maxY), Rand.NextFloat(-maxX, maxX), Rand.NextFloat(-maxY, maxY));
                    LineEffect.GetVariableByName("Disturb").AsVector().Set(disturb);
                    LineEffect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply();

                    device.Draw(1, i);
                }
            }           
        }    
    }
}
