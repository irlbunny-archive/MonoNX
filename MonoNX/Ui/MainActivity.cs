using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Widget;
using Android.Util;
using Android.Text.Method;

using System;

using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;

using MonoNX.Graphics.Gal;
using MonoNX.Graphics.Gal.Null;
using Android.Graphics;
using Android.Graphics.Drawables;
using Java.Nio;
using System.Threading;

namespace MonoNX
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        class ScreenTexture : IDisposable
        {
            private Switch       Ns;
            private IGalRenderer Renderer;

            private int Width;
            private int Height;

            public int[] Pixels;

            public ScreenTexture(Switch Ns, IGalRenderer Renderer, int Width, int Height)
            {
                this.Ns       = Ns;
                this.Renderer = Renderer;
                this.Width    = Width;
                this.Height   = Height;

                Pixels = new int[Width * Height];
            }

            public unsafe void UploadBitmap()
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
                        int SrcOffs = GetSwizzleOffset(X, Y);

                        Pixels[X + Y * Width] = *((int*)(SrcPtr + SrcOffs));
                    }
                }
            }

            private int GetSwizzleOffset(int X, int Y)
            {
                int Pos;

                Pos = (Y & 0x7f) >> 4;
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
                    disposed = true;
                }
            }
        }

        public static TextView NxLog;

        private IGalRenderer Renderer;

        private Switch Ns;

        private ScreenTexture ScreenTex;

        private ImageView ImageView;
        private Bitmap    ImageBitmap;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Config.Read();

            Renderer = new NullRenderer();

            Ns = new Switch(Renderer);

            ScreenTex = new ScreenTexture(Ns, Renderer, 1280, 720);

            SetContentView(Resource.Layout.activity_main);

            Thread RenderThread = new Thread(Render);

            ImageView   = FindViewById<ImageView>(Resource.Id.imageView);
            ImageBitmap = Bitmap.CreateBitmap(1280, 720, Bitmap.Config.Argb8888);

            DisplayMetrics DisplayMetrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetRealMetrics(DisplayMetrics);

            NxLog = FindViewById<TextView>(Resource.Id.nxLog);

            NxLog.MovementMethod = new ScrollingMovementMethod();

            NxLog.LayoutParameters.Width  = DisplayMetrics.WidthPixels;
            NxLog.LayoutParameters.Height = DisplayMetrics.HeightPixels - (int)(720 * 1.5);

            Button LoadRomButton = FindViewById<Button>(Resource.Id.loadRom);

            LoadRomButton.Click += async delegate {
                try
                {
                    FileData FileInput = await CrossFilePicker.Current.PickFile();
                    if (FileInput == null)
                    {
                        return;
                    }

                    Ns.Os.LoadProgram(FileInput.FileName, FileInput.GetStream());

                    RenderThread.Start();
                }
                catch (Exception e)
                {
                    Logging.Error($"An exception has occured: {e.ToString()}");
                }
            };
        }

        private void Render()
        {
            while (true)
            {
                // Update

                HidControllerKeys CurrentButton = 0;
                JoystickPosition LeftJoystick;
                JoystickPosition RightJoystick;

                int LeftJoystickDX = 0;
                int LeftJoystickDY = 0;

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

                Ns.Hid.SendControllerButtons(HidControllerID.CONTROLLER_HANDHELD, HidControllerLayouts.Main, CurrentButton, LeftJoystick, RightJoystick);

                // Draw

                ScreenTex.UploadBitmap();

                Renderer.RunActions();
                Renderer.BindTexture(0);
                Renderer.Render();

                if (ScreenTex.Pixels != null)
                {
                    ImageBitmap.CopyPixelsFromBuffer(IntBuffer.Wrap(ScreenTex.Pixels));
                }

                RunOnUiThread(() => ImageView.SetImageBitmap(ImageBitmap));
            }
        }
    }
}