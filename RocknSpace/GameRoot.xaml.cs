using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using RocknSpace.DirectWpf;
using Buffer = SharpDX.Direct3D10.Buffer;
using Device = SharpDX.Direct3D10.Device;

namespace RocknSpace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IScene
    {
        Random rand = new Random();
        private ISceneHost Host;
        private InputLayout VertexLayout;
        private InputLayout ShapeLayout;
        private Buffer Particles;
        private Buffer constantBuffer;
        private Effect ParticleEffect;
        private Effect ShapeEffect;

        public MainWindow()
        {
            InitializeComponent();

            this.Canvas1.Scene = this;

            EntityManager.Add(PlayerShip.Instance);
        }

        DateTime Last = DateTime.Now;
        int counter = 0;
        float sumFps = 0;

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
            Disposer.RemoveAndDispose(ref this.VertexLayout);
            Disposer.RemoveAndDispose(ref this.ShapeLayout);
            Disposer.RemoveAndDispose(ref this.ParticleEffect);
            Disposer.RemoveAndDispose(ref this.constantBuffer);
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
                this.Title = (sumFps / 120).ToString();
                sumFps = 0;

                for (int i = 0; i < 1200; i++)
                {
                    float speed = 12.0f * (1.0f - 1 / rand.NextFloat(1, 10));

                    ParticleManager.CreateParticle(new Vector(350, 250), new Color4(0, 1, 0, 0.3f), 190, Vector.One, rand.NextVector(speed, speed), 0.3f);
                }
            }
 

            EntityManager.Update();
            ParticleManager.Update();

            Device device = Host.Device;

            DataStream particles = new DataStream(ParticleManager.particleList.Count * 32, true, true);
            for (int i = 0; i < ParticleManager.particleList.Count; i++)
            {
                var particle = ParticleManager.particleList[i];

                Vector2 a = new Vector2(particle.Position.X, particle.Position.Y);
                Vector2 b = new Vector2(particle.Scale.X / 5, -particle.Orientation);

                particles.WriteRange(new[] { a, b });
                particles.Write(particle.Color);
            }

            particles.Position = 0;
            constantBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None);

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

            /*var renderToTexture = new Texture2D(device, new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R8G8B8A8_UNorm,
                Width = 1024,
                Height = 1024,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default
            });

            var renderToTextureRTV = new RenderTargetView(device, renderToTexture);
            var renderToTextureSRV = new ShaderResourceView(device, renderToTexture);*/


            
            device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.Particles, 32, 0));
            device.Draw(ParticleManager.particleList.Count, 0);







            technique = this.ShapeEffect.GetTechniqueByIndex(0);
            pass = technique.GetPassByIndex(0);
            pass.Apply();

            device.InputAssembler.InputLayout = this.ShapeLayout;
            device.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;

            PlayerShip.Instance.Shape.data.Position = 0;
            this.Particles = new Buffer(device, PlayerShip.Instance.Shape.data, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = PlayerShip.Instance.Shape.Vertices.Length * 2 * 12 + 24,
                Usage = ResourceUsage.Default
            });
            device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.Particles, 12, 0));

            this.ShapeEffect.GetVariableByName("Color").AsVector().Set(new Color4(1, 0, 0, 0.1f));
            this.ShapeEffect.GetVariableByName("Position").AsVector().Set(new Vector3(PlayerShip.Instance.Position.X, PlayerShip.Instance.Position.Y, PlayerShip.Instance.Orientation));

            device.Draw(PlayerShip.Instance.Shape.Vertices.Length * 2 + 2, 0);

            /*for (int i = 0; i < PlayerShip.Instance.Shape.Vertices.Length * 2 + 2; ++i)
            {
                device.Draw(4, i);
            }*/
            //device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.Vertices, 32, 0));

            //pass.Apply();
            //device.Draw(3, 0);
        }
    }
}
