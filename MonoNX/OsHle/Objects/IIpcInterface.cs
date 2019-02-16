using MonoNX.OsHle.Ipc;
using System.Collections.Generic;

namespace MonoNX.OsHle.Objects
{
    interface IIpcInterface
    {
        IReadOnlyDictionary<int, ServiceProcessRequest> Commands { get; } 
    }
}