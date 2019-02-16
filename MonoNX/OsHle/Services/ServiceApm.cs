using MonoNX.OsHle.Objects.Apm;

using static MonoNX.OsHle.Objects.ObjHelper;

namespace MonoNX.OsHle.Services
{
    static partial class Service
    {
        public static long ApmOpenSession(ServiceCtx Context)
        {
            MakeObject(Context, new ISession());

            return 0;
        }
    }
}