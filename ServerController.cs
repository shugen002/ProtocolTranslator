using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ProtocolTranslator
{
    class ServerController
    {
        public static string targetHosts = "127.0.0.1";
        public static int port = 2244;
        public static int type = 1;
        public static string url= "ws://120.92.112.150:2244/sub";
        private HttpServer httpsv;
        public bool StartWebsocketServer()
        {
            httpsv = new HttpServer(2244);
            httpsv.AddWebSocketService<GoIMProxy>("/sub");
            try
            {
                httpsv.Start();
            }
            catch (Exception e)
            {
                Program.log("Error", e.ToString());
                return false;
            }
            return true;
        }
    }
}
