using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;//DllImport需要

namespace CMMInterpreter.Process
{
    class Run
    {
        public class MidConsole
        {
            [DllImport("kernel32.dll")]
            public static extern Boolean AllocConsole();
            [DllImport("kernel32.dll")]
            public static extern Boolean FreeConsole();
            [DllImport("VirtualMachine.dll")]
            public static extern int main();
        }

        public void StartRun()
        {
            MidConsole.AllocConsole();  //调出控制台
            MidConsole.main();         //运行虚拟机
            MidConsole.FreeConsole();   //关闭控制台
        }
    }
}
