using ChocolArm64.Memory;
using MonoNX.Graphics.Gal;
using MonoNX.Graphics.Gpu;
using MonoNX.OsHle;
using System;
using System.Runtime.InteropServices;

namespace MonoNX
{
    public class Switch : IDisposable
    {
        public IntPtr Ram {get; private set; }

        internal NsGpu     Gpu { get; private set; }
        internal Horizon   Os  { get; private set; }
        internal VirtualFs VFs { get; private set; }
        internal Hid       Hid { get; private set; }

        public event EventHandler Finish;

        public Switch(IGalRenderer Renderer)
        {
            Ram = Marshal.AllocHGlobal((IntPtr)AMemoryMgr.RamSize);

            Gpu = new NsGpu(Renderer);
            Os  = new Horizon(this);
            VFs = new VirtualFs();
            Hid = new Hid(this);
        }

        internal virtual void OnFinish(EventArgs e)
        {
            Finish?.Invoke(this, e);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                VFs.Dispose();
            }

            Marshal.FreeHGlobal(Ram);
        }
    }
}