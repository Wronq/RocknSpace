// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace RocknSpace.DirectWpf
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using SharpDX;
    using SharpDX.Direct3D10;
    using SharpDX.DXGI;
    using SharpDX.D3DCompiler;
    using Device = SharpDX.Direct3D10.Device1;
    using Buffer = SharpDX.Direct3D10.Buffer;
    using Utils;

    public partial class DPFCanvas : Image, ISceneHost
    {
        private Device Device;
        private Texture2D RenderTarget;
        private Texture2D RenderTarget1;
        private Texture2D RenderTarget2;
        private Texture2D OutputRenderTarget;

        private Texture2D DepthStencil;
        private RenderTargetView RenderTargetView;
        private RenderTargetView RenderTarget1View;
        private RenderTargetView RenderTarget2View;
        private RenderTargetView OutputRenderTargetView;
        private ShaderResourceView ShaderTargetView;
        private ShaderResourceView ShaderTarget1View;
        private ShaderResourceView ShaderTarget2View;

        private DepthStencilView DepthStencilView;
        private BlendState BlendingState;
        private DX10ImageSource D3DSurface;
        private Stopwatch RenderTimer;
        private IScene RenderScene;
        private bool SceneAttached;

        private Effect BloomEffect;
        private InputLayout VertexLayout;
        private Buffer QuadBuffer;

        public Color4 ClearColor = SharpDX.Color.Black;

        BloomSettings Settings = new BloomSettings(0.15f, 8, 1.5f, 1, 1.5f, 1.5f);

        private float width, height;
        float ISceneHost.Width { get { return width; } }

        float ISceneHost.Height { get{ return height; } }

        public DPFCanvas()
        {
            this.RenderTimer = new Stopwatch();
            this.Loaded += this.Window_Loaded;
            this.Unloaded += this.Window_Closing;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DPFCanvas.IsInDesignMode)
                return;

            this.StartD3D();
            this.StartRendering();
        }

        private void Window_Closing(object sender, RoutedEventArgs e)
        {
            if (DPFCanvas.IsInDesignMode)
                return;

            this.StopRendering();
            this.EndD3D();
        }

        private void StartD3D()
        {
            this.Device = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport, FeatureLevel.Level_10_0);

            this.D3DSurface = new DX10ImageSource();
            this.D3DSurface.IsFrontBufferAvailableChanged += OnIsFrontBufferAvailableChanged;

            this.CreateAndBindTargets();

            this.Source = this.D3DSurface;
            try
            {
                ShaderBytecode shaderBytes = ShaderBytecode.CompileFromFile("Shaders\\Bloom.fx", "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);
                this.BloomEffect = new Effect(Device, shaderBytes);
            }
            catch
            { }

            EffectTechnique technique = this.BloomEffect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);

                this.VertexLayout = new InputLayout(Device, pass.Description.Signature, new[] {
                    new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                    new InputElement("TEXCOORD", 0, Format.R32G32_Float, 16, 0)
                });

            DataStream vertices = new DataStream(4 * 6 * 4, true, true);
            vertices.Write(new Vector4(-1.0f, 1.0f, 0.0f, 1.0f));
            vertices.Write(new Vector2(0.0f, 0.0f));
            vertices.Write(new Vector4(1.0f, 1.0f, 0.0f, 1.0f));
            vertices.Write(new Vector2(1.0f, 0.0f));
            vertices.Write(new Vector4(-1.0f, -1.0f, 0.0f, 1.0f));
            vertices.Write(new Vector2(0.0f, 1.0f));
            vertices.Write(new Vector4(1.0f, -1.0f, 0.0f, 1.0f));
            vertices.Write(new Vector2(1.0f, 1.0f));
            vertices.Position = 0;

            this.QuadBuffer = new Buffer(Device, vertices, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = 4 * 6 * 4,
                Usage = ResourceUsage.Default
            });
        }

        private void EndD3D()
        {
            if (this.RenderScene != null)
            {
                this.RenderScene.Detach();
                this.SceneAttached = false;
            }

            this.D3DSurface.IsFrontBufferAvailableChanged -= OnIsFrontBufferAvailableChanged;
            this.Source = null;

            Disposer.RemoveAndDispose(ref this.D3DSurface);
            Disposer.RemoveAndDispose(ref this.RenderTargetView);
            Disposer.RemoveAndDispose(ref this.RenderTarget1View);
            Disposer.RemoveAndDispose(ref this.RenderTarget2View);
            Disposer.RemoveAndDispose(ref this.OutputRenderTargetView);
            Disposer.RemoveAndDispose(ref this.DepthStencilView);
            Disposer.RemoveAndDispose(ref this.RenderTarget);
            Disposer.RemoveAndDispose(ref this.OutputRenderTarget);
            Disposer.RemoveAndDispose(ref this.DepthStencil);
            Disposer.RemoveAndDispose(ref this.BlendingState);
            Disposer.RemoveAndDispose(ref this.RenderTarget1);
            Disposer.RemoveAndDispose(ref this.RenderTarget2);
            Disposer.RemoveAndDispose(ref this.ShaderTargetView);
            Disposer.RemoveAndDispose(ref this.ShaderTarget1View);
            Disposer.RemoveAndDispose(ref this.ShaderTarget2View);
            Disposer.RemoveAndDispose(ref this.BloomEffect);
            Disposer.RemoveAndDispose(ref this.VertexLayout);
            Disposer.RemoveAndDispose(ref this.QuadBuffer);
            Disposer.RemoveAndDispose(ref this.Device);
        }

        private void CreateAndBindTargets()
        {
            this.D3DSurface.SetRenderTargetDX10(null);

            Disposer.RemoveAndDispose(ref this.RenderTargetView);
            Disposer.RemoveAndDispose(ref this.RenderTarget1View);
            Disposer.RemoveAndDispose(ref this.RenderTarget2View);
            Disposer.RemoveAndDispose(ref this.OutputRenderTargetView);
            Disposer.RemoveAndDispose(ref this.DepthStencilView);
            Disposer.RemoveAndDispose(ref this.BlendingState);
            Disposer.RemoveAndDispose(ref this.OutputRenderTarget);
            Disposer.RemoveAndDispose(ref this.RenderTarget);
            Disposer.RemoveAndDispose(ref this.RenderTarget1);
            Disposer.RemoveAndDispose(ref this.RenderTarget2);
            Disposer.RemoveAndDispose(ref this.ShaderTargetView);
            Disposer.RemoveAndDispose(ref this.ShaderTarget1View);
            Disposer.RemoveAndDispose(ref this.ShaderTarget2View);
            Disposer.RemoveAndDispose(ref this.DepthStencil);

            width = Math.Max((int)base.ActualWidth, 100);
            height = Math.Max((int)base.ActualHeight, 100);

            Texture2DDescription colordesc = new Texture2DDescription
            {
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = (int)width,
                Height = (int)height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.Shared,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };

            Texture2DDescription depthdesc = new Texture2DDescription
            {
                BindFlags = BindFlags.DepthStencil,
                Format = Format.D32_Float_S8X24_UInt,
                Width = (int)width,
                Height = (int)height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.None,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };

            this.RenderTarget = new Texture2D(this.Device, colordesc);
            this.OutputRenderTarget = new Texture2D(this.Device, colordesc);

            this.DepthStencil = new Texture2D(this.Device, depthdesc);

            RenderTarget1 = new Texture2D(this.Device, new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = (int)width,
                Height = (int)height,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.Shared,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default
            });

            RenderTarget2 = new Texture2D(this.Device, new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Width = (int)width,
                Height = (int)height,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.Shared,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default
            });

            BlendStateDescription stateDesc = new BlendStateDescription
            {
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add
            };

            stateDesc.IsBlendEnabled[0] = true;
            stateDesc.RenderTargetWriteMask[0] = ColorWriteMaskFlags.All;
            stateDesc.IsAlphaToCoverageEnabled = false;

            BlendingState = new BlendState(Device, ref stateDesc);

            this.DepthStencilView = new DepthStencilView(this.Device, this.DepthStencil);

            this.RenderTargetView = new RenderTargetView(this.Device, this.RenderTarget);
            this.RenderTarget1View = new RenderTargetView(this.Device, this.RenderTarget1);
            this.RenderTarget2View = new RenderTargetView(this.Device, this.RenderTarget2);
            this.OutputRenderTargetView = new RenderTargetView(this.Device, this.OutputRenderTarget);

            this.ShaderTargetView = new ShaderResourceView(this.Device, this.RenderTarget);
            this.ShaderTarget1View = new ShaderResourceView(this.Device, this.RenderTarget1);
            this.ShaderTarget2View = new ShaderResourceView(this.Device, this.RenderTarget2);

            try
            {
                this.D3DSurface.SetRenderTargetDX10(this.OutputRenderTarget);
            }
            catch
            { }
        }

        private void StartRendering()
        {
            if (this.RenderTimer.IsRunning)
                return;

            CompositionTarget.Rendering += OnRendering;
            this.RenderTimer.Start();
        }

        private void StopRendering()
        {
            if (!this.RenderTimer.IsRunning)
                return;

            CompositionTarget.Rendering -= OnRendering;
            this.RenderTimer.Stop();
        }

        private void OnRendering(object sender, EventArgs e)
        {
            if (!this.RenderTimer.IsRunning)
                return;

            this.Render(this.RenderTimer.Elapsed);
            this.D3DSurface.InvalidateD3DImage();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            this.CreateAndBindTargets();
            base.OnRenderSizeChanged(sizeInfo);
        }

        private float ComputeGaussian(float n)
        {
            float theta = Settings.BlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        private void SetBlurEffectParameters(float dx, float dy)
        {
            int sampleCount = 15;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector4[] sampleOffsets = new Vector4[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector4(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = new Vector4(delta, 0, 0);
                sampleOffsets[i * 2 + 2] = -new Vector4(delta, 0, 0);
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
                sampleWeights[i] /= totalWeights;

            BloomEffect.GetVariableByName("SampleOffsets").AsVector().Set(sampleOffsets);
            BloomEffect.GetVariableByName("SampleWeights").AsScalar().Set(sampleWeights);
        }
        void Render(TimeSpan sceneTime)
        {
            SharpDX.Direct3D10.Device device = this.Device;
            if (device == null)
                return;

            Texture2D renderTarget = this.RenderTarget;
            if (renderTarget == null)
                return;

            int targetWidth = renderTarget.Description.Width;
            int targetHeight = renderTarget.Description.Height;

            device.OutputMerger.SetTargets(this.DepthStencilView, this.RenderTargetView);
            device.OutputMerger.SetBlendState(this.BlendingState, new Color4(0, 0, 0, 0), 0xfffffff);

            device.Rasterizer.SetViewports(new Viewport(0, 0, targetWidth, targetHeight, 0.0f, 1.0f));

            device.ClearRenderTargetView(this.RenderTargetView, this.ClearColor);
            device.ClearDepthStencilView(this.DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);

            if (this.Scene != null)
            {
                if (!this.SceneAttached)
                {
                    this.SceneAttached = true;
                    try
                    {
                        this.RenderScene.Attach(this);
                    }
                    catch
                    { }
                }

                this.Scene.Update(this.RenderTimer.Elapsed);
                this.Scene.Render();
            }

            device.OutputMerger.SetTargets(this.DepthStencilView, this.RenderTarget1View);

            device.ClearDepthStencilView(this.DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            device.ClearRenderTargetView(this.RenderTarget1View, SharpDX.Color.Black);

            device.InputAssembler.InputLayout = this.VertexLayout;
            device.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;
            device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.QuadBuffer, 24, 0));

            BloomEffect.GetVariableByName("Texture1").AsShaderResource().SetResource(ShaderTargetView);
            BloomEffect.GetVariableByName("BloomThreshold").AsScalar().Set(Settings.BloomThreshold);

            BloomEffect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply();
            device.Draw(4, 0);



            device.OutputMerger.SetTargets(this.DepthStencilView, this.RenderTarget2View);

            device.ClearDepthStencilView(this.DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            device.ClearRenderTargetView(this.RenderTarget2View, SharpDX.Color.Black);

            BloomEffect.GetVariableByName("Texture1").AsShaderResource().SetResource(ShaderTarget1View);
            SetBlurEffectParameters(1.0f / RenderTarget1.Description.Width, 0);

            BloomEffect.GetTechniqueByIndex(0).GetPassByIndex(1).Apply();
            device.Draw(4, 0);



            device.OutputMerger.SetTargets(this.DepthStencilView, this.RenderTarget1View);

            device.ClearDepthStencilView(this.DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            device.ClearRenderTargetView(this.RenderTarget1View, SharpDX.Color.Black);

            BloomEffect.GetVariableByName("Texture1").AsShaderResource().SetResource(ShaderTarget2View);
            SetBlurEffectParameters(0, 1.0f / RenderTarget1.Description.Height);

            BloomEffect.GetTechniqueByIndex(0).GetPassByIndex(1).Apply();
            device.Draw(4, 0);




            device.OutputMerger.SetTargets(this.DepthStencilView, this.OutputRenderTargetView);

            device.ClearDepthStencilView(this.DepthStencilView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            device.ClearRenderTargetView(this.OutputRenderTargetView, SharpDX.Color.Black);

            BloomEffect.GetVariableByName("BaseTexture").AsShaderResource().SetResource(ShaderTargetView);
            BloomEffect.GetVariableByName("BloomTexture").AsShaderResource().SetResource(ShaderTarget1View);

            BloomEffect.GetVariableByName("BloomIntensity").AsScalar().Set(Settings.BloomIntensity);
            BloomEffect.GetVariableByName("BaseIntensity").AsScalar().Set(Settings.BaseIntensity);
            BloomEffect.GetVariableByName("BloomSaturation").AsScalar().Set(Settings.BloomSaturation);
            BloomEffect.GetVariableByName("BaseSaturation").AsScalar().Set(Settings.BaseSaturation);

            BloomEffect.GetTechniqueByIndex(0).GetPassByIndex(2).Apply();
            device.Draw(4, 0);

            device.Flush();
        }

        private void OnIsFrontBufferAvailableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // this fires when the screensaver kicks in, the machine goes into sleep or hibernate
            // and any other catastrophic losses of the d3d device from WPF's point of view
            if (this.D3DSurface.IsFrontBufferAvailable)
            {
                CreateAndBindTargets();
                this.StartRendering();
            }
            else
                this.StopRendering();
        }

        /// <summary>
        /// Gets a value indicating whether the control is in design mode
        /// (running in Blend or Visual Studio).
        /// </summary>
        public static bool IsInDesignMode
        {
            get
            {
                DependencyProperty prop = DesignerProperties.IsInDesignModeProperty;
                bool isDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;
                return isDesignMode;
            }
        }

        public IScene Scene
        {
            get { return this.RenderScene; }
            set
            {
                if (ReferenceEquals(this.RenderScene, value))
                    return;

                if (this.RenderScene != null)
                    this.RenderScene.Detach();

                this.SceneAttached = false;
                this.RenderScene = value;
            }
        }

        SharpDX.Direct3D10.Device ISceneHost.Device
        {
            get { return this.Device; }
        }
    }
}
