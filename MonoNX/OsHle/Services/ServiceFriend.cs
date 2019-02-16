using MonoNX.OsHle.Objects.Friend;

using static MonoNX.OsHle.Objects.ObjHelper;

namespace MonoNX.OsHle.Services
{
    static partial class Service
    {
        public static long FriendCreateFriendService(ServiceCtx Context)
        {
            MakeObject(Context, new IFriendService());

            return 0;
        }
    }
}