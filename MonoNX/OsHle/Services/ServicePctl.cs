using MonoNX.OsHle.Objects.Am;

using static MonoNX.OsHle.Objects.ObjHelper;

namespace MonoNX.OsHle.Services
{
    static partial class Service
    {
        public static long PctlCreateService(ServiceCtx Context)
        {
            MakeObject(Context, new IParentalControlService());

            return 0;
        }
    }
}