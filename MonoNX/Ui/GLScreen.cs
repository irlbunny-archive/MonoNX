using Android.Content;
using Android.Util;

using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES30;
using OpenTK.Platform.Android;

using MonoNX.Graphics.Gal;

namespace MonoNX
{
    public class GLScreen : AndroidGameView
    {
        class ScreenTexture : IDisposable
        {
            private Switch       Ns;
            private IGalRenderer Renderer;
            
            private int Width;
            private int Height;
            private int TexHandle;            

            private int[] Pixels;

            public ScreenTexture(Switch Ns, IGalRenderer Renderer, int Width, int Height)
            {
                this.Ns       = Ns;
                this.Renderer = Renderer;
                this.Width    = Width;
                this.Height   = Height;

                Pixels = new int[Width * Height];

                TexHandle = GL.GenTexture();

                GL.BindTexture(TextureTarget.Texture2D, TexHandle);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    Width,
                    Height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    IntPtr.Zero);
            }

            public int Texture
            {
                get
                {
                    UploadBitmap();

                    return TexHandle;
                }
            }

            unsafe void UploadBitmap()
            {
                int FbSize = Width * Height * 4;

                if (Renderer.FrameBufferPtr == 0 || Renderer.FrameBufferPtr + FbSize > uint.MaxValue)
                {
                    return;
                }

                byte* SrcPtr = (byte*)Ns.Ram + (uint)Renderer.FrameBufferPtr;

                for (int Y = 0; Y < Height; Y++)
                {
                    for (int X = 0; X < Width; X++)
                    {
                        int SrcOffs = GetSwizzleOffset(X, Y, 4);

                        Pixels[X + Y * Width] = *((int*)(SrcPtr + SrcOffs));
                    }
                }

                GL.BindTexture(TextureTarget.Texture2D, TexHandle);
                GL.TexSubImage2D(TextureTarget.Texture2D,
                    0,
                    0,
                    0,
                    Width,
                    Height,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    Pixels);
            }

            private int GetSwizzleOffset(int X, int Y, int Bpp)
            {
                int Pos;

                Pos  = (Y & 0x7f) >> 4;
                Pos += (X >> 4) << 3;
                Pos += (Y >> 7) * ((Width >> 4) << 3);
                Pos *= 1024;
                Pos += ((Y & 0xf) >> 3) << 9;
                Pos += ((X & 0xf) >> 3) << 8;
                Pos += ((Y & 0x7) >> 1) << 6;
                Pos += ((X & 0x7) >> 2) << 5;
                Pos += ((Y & 0x1) >> 0) << 4;
                Pos += ((X & 0x3) >> 0) << 2;

                return Pos;
            }

            private bool disposed;

            public void Dispose()
            {
                Dispose(true);
                
                GC.SuppressFinalize(this);
            }

            void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        GL.DeleteTexture(TexHandle);
                    }

                    disposed = true;
                }
            }
        }

        private string VtxShaderSource = @"
#version 330 core

precision highp float;

uniform vec2 window_size;

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec4 in_color;
layout(location = 2) in vec2 in_tex_coord;

out vec4 color;
out vec2 tex_coord;

// Have a fixed aspect ratio, fit the image within the available space.
vec3 get_scale_ratio() {
    vec2 native_size = vec2(1280, 720);
    vec2 ratio = vec2(
        (window_size.y * native_size.x) / (native_size.y * window_size.x),
        (window_size.x * native_size.y) / (native_size.x * window_size.y)
    );
    return vec3(min(ratio, vec2(1, 1)), 1);
}

void main(void) { 
    color = in_color;
    tex_coord = in_tex_coord;
    gl_Position = vec4(in_position * get_scale_ratio(), 1);
}";

        private string FragShaderSource = @"
#version 330 core

precision highp float;

uniform sampler2D tex;

in vec4 color;
in vec2 tex_coord;
out vec4 out_frag_color;

void main(void) {
    out_frag_color = vec4(texture(tex, tex_coord).rgb, color.a);
}";

        private int VtxShaderHandle,
                    FragShaderHandle,
                    PrgShaderHandle;

        private int WindowSizeUniformLocation;
        
        private int VaoHandle;
        private int VboHandle;

        private Switch Ns;

        private IGalRenderer Renderer;

        private ScreenTexture ScreenTex;

        public GLScreen(Context Context, Switch Ns, IGalRenderer Renderer)
            : base(Context)
        {
            this.Ns       = Ns;
            this.Renderer = Renderer;

            ScreenTex = new ScreenTexture(Ns, Renderer, 1280, 720);

            AutoSetContextOnRenderFrame = false;
            RenderOnUIThread            = false;
            ContextRenderingApi         = GLVersion.ES3;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CreateShaders();
            CreateVbo();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            Run();
        }

        protected override void OnUnload(EventArgs e)
        {
            ScreenTex.Dispose();
        }

        protected override void CreateFrameBuffer()
        {
            try
            {
                Log.Verbose("MonoNX", "Loading with default settings");

                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("MonoNX", ex.ToString());
            }

            try
            {
                Log.Verbose("MonoNX", "Loading with custom Android settings (low mode)");
                GraphicsMode = new AndroidGraphicsMode();

                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("MonoNX", ex.ToString());
            }

            throw new Exception("Can't load EGL, aborting");
        }

        private void CreateVbo()
        {
            GL.GenVertexArrays(1, out VaoHandle);
            GL.GenBuffers(1, out VboHandle);

            uint[] Buffer = new uint[]
            {
                0xbf800000, 0x3f800000, 0x00000000, 0xffffffff, 0x00000000, 0x00000000, 0x00000000,
                0x3f800000, 0x3f800000, 0x00000000, 0xffffffff, 0x00000000, 0x3f800000, 0x00000000,
                0xbf800000, 0xbf800000, 0x00000000, 0xffffffff, 0x00000000, 0x00000000, 0x3f800000,
                0x3f800000, 0xbf800000, 0x00000000, 0xffffffff, 0x00000000, 0x3f800000, 0x3f800000
            };

            IntPtr Length = new IntPtr(Buffer.Length * 4);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, Length, Buffer, BufferUsage.StreamDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(VaoHandle);

            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 28, 0);

            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);

            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, false, 28, 12);

            GL.EnableVertexAttribArray(2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 28, 20);

            GL.BindVertexArray(0);
        }

        private void CreateShaders()
        {
            VtxShaderHandle  = GL.CreateShader(ShaderType.VertexShader);
            FragShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(VtxShaderHandle, VtxShaderSource);
            GL.ShaderSource(FragShaderHandle, FragShaderSource);
            GL.CompileShader(VtxShaderHandle);
            GL.CompileShader(FragShaderHandle);

            PrgShaderHandle = GL.CreateProgram();

            GL.AttachShader(PrgShaderHandle, VtxShaderHandle);
            GL.AttachShader(PrgShaderHandle, FragShaderHandle);
            GL.LinkProgram(PrgShaderHandle);
            GL.UseProgram(PrgShaderHandle);

            int TexLocation = GL.GetUniformLocation(PrgShaderHandle, "tex");
            GL.Uniform1(TexLocation, 0);

            WindowSizeUniformLocation = GL.GetUniformLocation(PrgShaderHandle, "window_size");
            GL.Uniform2(WindowSizeUniformLocation, new Vector2(1280.0f, 720.0f));
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            HidControllerKeys CurrentButton = 0;
            JoystickPosition LeftJoystick;
            JoystickPosition RightJoystick;

            //RightJoystick
            int LeftJoystickDX = 0;
            int LeftJoystickDY = 0;

            //RightJoystick
            int RightJoystickDX = 0;
            int RightJoystickDY = 0;

            LeftJoystick = new JoystickPosition
            {
                DX = LeftJoystickDX,
                DY = LeftJoystickDY
            };

            RightJoystick = new JoystickPosition
            {
                DX = RightJoystickDX,
                DY = RightJoystickDY
            };

            //We just need one pair of JoyCon because it's emulate by the keyboard.
            Ns.Hid.SendControllerButtons(HidControllerID.CONTROLLER_HANDHELD, HidControllerLayouts.Main, CurrentButton, LeftJoystick, RightJoystick);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //TODO: Figure out why we can't render a frame!
            base.OnRenderFrame(e);

            GL.Viewport(0, 0, Width, Height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            RenderFb();

            GL.UseProgram(PrgShaderHandle);
            
            Renderer.RunActions();
            Renderer.BindTexture(0);
            Renderer.Render();

            SwapBuffers();
        }

        void RenderFb()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ScreenTex.Texture);
            GL.BindVertexArray(VaoHandle);
            GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
        }
    }
}