using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocketSharp;

namespace ProtocolTranslator
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ProtocolTranslator());
        }

        public static void log(string level, string message)
        {
            Console.WriteLine(string.Format("[{0}]{1}", level, message));
            LogEventArgs temp = new LogEventArgs {
                level = level,
                message = message
            };
            if (eventhandler != null)
            {
                eventhandler.Invoke(null,temp);
            }
        }
        public delegate void LogEventHandler(object sender, LogEventArgs e);
        public static event LogEventHandler eventhandler;
    }
    public class LogEventArgs
    {
        public string level;
        public string message;
    }
}
