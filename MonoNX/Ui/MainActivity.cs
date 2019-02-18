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
using MonoNX.Graphics.Gal.OpenGL;

namespace MonoNX
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        GLScreen view;

        public static TextView nxLog;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Config.Read();

            IGalRenderer Renderer = new OpenGLRenderer();

            Switch Ns = new Switch(Renderer);

            view = new GLScreen(this, Ns, Renderer);

            SetContentView(Resource.Layout.activity_main);

            DisplayMetrics displayMetrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetRealMetrics(displayMetrics);

            nxLog = FindViewById<TextView>(Resource.Id.nxLog);

            nxLog.MovementMethod = new ScrollingMovementMethod();

            nxLog.LayoutParameters.Width  = displayMetrics.WidthPixels;
            nxLog.LayoutParameters.Height = displayMetrics.HeightPixels;

            Button loadRomBtn     = FindViewById<Button>(Resource.Id.loadRom);
            Button viewDisplayBtn = FindViewById<Button>(Resource.Id.viewDisplay);

            loadRomBtn.Click += async delegate {
                try
                {
                    FileData FileInput = await CrossFilePicker.Current.PickFile();
                    if (FileInput == null)
                        return;

                    Ns.Os.LoadProgram(FileInput.FileName, FileInput.GetStream());
                }
                catch (Exception e)
                {
                    Logging.Error($"An exception has occured: {e.ToString()}");
                }
            };

            viewDisplayBtn.Click += delegate {
                SetContentView(view);
            };
        }

        protected override void OnPause()
        {
            base.OnPause();
            view.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            view.Resume();
        }
    }
}