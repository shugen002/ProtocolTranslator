using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ProtocolTranslator
{
    class ServerController
    {
        public static IPAddress localhost = IPAddress.Parse("127.0.0.1");
        public static int localport = 2244;
        public static int type = 1;
        public static string url= "ws://120.92.112.150:2244/sub";
        private HttpServer httpsv;
        public bool StartWebsocketServer()
        {
            httpsv = new HttpServer(localhost, localport);
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
            Program.log("INFO", localhost.ToString() + ":" + localport + " 服务器启动");
            return true;
        }
    }
}
