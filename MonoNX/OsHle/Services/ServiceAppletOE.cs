using MonoNX.OsHle.Objects.Am;

using static MonoNX.OsHle.Objects.ObjHelper;

namespace MonoNX.OsHle.Services
{
    static partial class Service
    {
        public static long AppletOpenApplicationProxy(ServiceCtx Context)
        {
            MakeObject(Context, new IApplicationProxy());

            return 0;
        }
    }
}