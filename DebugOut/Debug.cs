using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace DebugOut
{
    public static class Debug
    {
        private static StreamWriter file;
        public static StreamWriter GetInstance()
        {
            if (file == null)
            {
                file = new StreamWriter("debugout.txt", true);
            }
            return file;
        }



        public static void WriteLine(object message)
        {
            GetInstance().WriteLine(message.ToString());
            GetInstance().Flush();

        }
        public static void Write(object message)
        {
            GetInstance().Write(message.ToString());
            GetInstance().Flush();
        }

        public static void WriteLine(string format, params object[] args)
        {
            WriteLine((object)string.Format(format, args));
        }
        public static void Write(string format, params object[] args)
        {
            WriteLine((object)string.Format(format, args));
        }

        public static void Exit(int code)
        {
            Environment.Exit(code);
        }
    }
}
