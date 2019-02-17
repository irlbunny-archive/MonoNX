using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using MonoNX.Graphics.Gal;
using MonoNX.Graphics.Gal.OpenGL;
using System.IO;
using Plugin.FilePicker.Abstractions;
using System;
using Plugin.FilePicker;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Android.Util;

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

            //TODO: Permission check for storage here?

            Config.Read();

            IGalRenderer Renderer = new OpenGLRenderer();

            Switch Ns = new Switch(Renderer);

            view = new GLScreen(this, Ns, Renderer);

            SetContentView(Resource.Layout.activity_main);

            DisplayMetrics displayMetrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetRealMetrics(displayMetrics);

            nxLog = FindViewById<TextView>(Resource.Id.nxLog);

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

                    Ns.Os.LoadProgram(FileInput.FilePath); //TODO: Add resolver for content:/?
                }
                catch (Exception ex)
                {
                    Logging.Error($"An exception has occured while loading a ROM: {ex.ToString()}");
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