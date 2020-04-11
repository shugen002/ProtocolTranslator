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
        private HttpServer httpsv;
        public bool StartWebsocketServer()
        {
            httpsv = new HttpServer(2244);
            httpsv.AddWebSocketService<GoIMServer>("/sub", (GoIMServer goIMServer) => {
                goIMServer.setUp();
            });
            try
            {
                httpsv.Start();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

    }
}
