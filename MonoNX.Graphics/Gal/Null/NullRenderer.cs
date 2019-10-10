using System;
using System.Collections.Generic;

namespace MonoNX.Graphics.Gal.Null
{
    public class NullRenderer : IGalRenderer
    {
        private Queue<Action> ActionsQueue;

        public long FrameBufferPtr { get; set; }

        public NullRenderer()
        {
            ActionsQueue = new Queue<Action>();
        }

        public void QueueAction(Action ActionMthd)
        {
            ActionsQueue.Enqueue(ActionMthd);
        }

        public void RunActions()
        {
            while (ActionsQueue.Count > 0)
            {
                ActionsQueue.Dequeue()();
            }
        }

        public void Render()
        {
        }

        public void SendVertexBuffer(int Index, byte[] Buffer, int Stride, GalVertexAttrib[] Attribs)
        {
        }

        public void SendR8G8B8A8Texture(int Index, byte[] Buffer, int Width, int Height)
        {
        }

        public void BindTexture(int Index)
        {         
        }
    }
}