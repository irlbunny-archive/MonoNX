using MonoNX.OsHle.Objects.Vi;

using static MonoNX.OsHle.Objects.ObjHelper;

namespace MonoNX.OsHle.Services
{
    static partial class Service
    {
        public static long ViGetDisplayService(ServiceCtx Context)
        {
            int Unknown = Context.RequestData.ReadInt32();

            MakeObject(Context, new IApplicationDisplayService());

            return 0;
        }
    }
}