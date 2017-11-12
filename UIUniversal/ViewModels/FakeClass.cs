using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UiPortable.ViewModels
{
    public class FakeClass
    {
        [DllImport("api-ms-win-core-processthreads-l1-1-2.dll", SetLastError = true, EntryPoint = "GetCurrentThreadId")]
        public static extern int GetCurrentThreadId();

        [DllImport("api-ms-win-core-processthreads-l1-1-2.dll", SetLastError = true, EntryPoint = "GetCurrentThread")]
        public static extern IntPtr GetCurrentThread();

        [DllImport("api-ms-win-core-processthreads-l1-1-2.dll", SetLastError = true, EntryPoint = "SetThreadPriority")]
        static extern bool SetThreadPriority(IntPtr hThread, ThreadPriority nPriority);

        public FakeClass(string trackstepview)
        {
            Debug.WriteLine("Current Thread ID: {0} {1}", GetCurrentThreadId(), trackstepview);

            if (SetThreadPriority(GetCurrentThread(), ThreadPriority.THREAD_PRIORITY_NORMAL))
            {
                Debug.WriteLine("Current: now below normal");
            }
        }


        ~FakeClass()
        {
            //Debug.WriteLine("Finalizer: Thread ID: {0}", GetCurrentThreadId());

            //if (SetThreadPriority(GetCurrentThread(), ThreadPriority.THREAD_PRIORITY_NORMAL))
            //{
            //    Debug.WriteLine("Finalizer:  now below normal");
            //}
        }

        public void DoSomeThing()
        {
            Task.Delay(10).Wait();
        }
    }
}
