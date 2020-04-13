using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;



namespace ProtocolTranslator
{
    class GoIMProxy : WebSocketBehavior
    {
        private WebSocket ws;
        private bool connected = false;
        private ArrayList cache = new ArrayList();
        public string userpoint;
        public GoIMProxy()
        {
            this.IgnoreExtensions = true;
        }
        protected override void OnOpen()
        {
            userpoint = Context.UserEndPoint.ToString();
            Program.log("INFO", userpoint + " Connected");
            ws = new WebSocket(ServerController.url);
            ws.OnClose += OnWSClose;
            ws.OnOpen += OnWSOpen;
            ws.OnError += OnWSError;
            ws.OnMessage += OnWSMessage;
            ws.Connect();
        }

        private void OnWSMessage(object sender, MessageEventArgs e)
        {
            Program.log("INFO", userpoint + " Received " + e.RawData.Length + " Bytes from Remote");
            packetHandler(e.RawData);
        }

        private void packetHandler(Byte[] bytes)
        {
            Byte[][] packets = GoIMProtocol.Cut(bytes);
            Program.log("INFO", userpoint + " Received " + packets.Length + " Packets from Remote");
            foreach (Byte[] packet in packets)
            {
                GoIMMessage data = GoIMProtocol.Decode(packet);
                if (data.operation == GoIMProtocol.WS_OP_MESSAGE && data.protocolVersion == 2)
                {
                    Program.log("INFO", userpoint + " Received Compressed Packets from Remote");
                    var rawStream = new MemoryStream(data.Body, 2, data.Body.Length - 2);
                    DeflateStream deflateStream = new DeflateStream(rawStream, CompressionMode.Decompress,false);
                    MemoryStream memoryStream = new MemoryStream();
                    deflateStream.CopyTo(memoryStream);
                    packetHandler(memoryStream.ToArray());
                }
                else
                {
                    Program.log("INFO", userpoint + " Sending " + packet.Length + " Bytes to Local.");
                    Send(packet);
                }
            }
        }

        private void OnWSError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Program.log("ERROR", userpoint + " Remote Side Error :" + e.ToString());
            SafeCloseLocal();
            SafeCloseRemote();
        }

        private void OnWSOpen(object sender, EventArgs e)
        {
            Program.log("INFO", userpoint + " Remote Side Open");
            connected = true;
            if (cache.Count > 0)
            {
                foreach (byte[] item in cache)
                {
                    Program.log("INFO", userpoint + " Sending " + item.Length + " to Remote.");
                    ws.Send(item);
                }
                cache.Clear();
            }
        }

        private void OnWSClose(object sender, CloseEventArgs e)
        {
            Program.log("INFO", userpoint + " Remote Side Closed. Code: " + e.Code + " Reason:" + e.Reason);
            SafeCloseLocal(e.Code, e.Reason);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            Program.log("INFO", userpoint + " Received " + e.RawData.Length + " Bytes from Local");
            byte[][] packets = GoIMProtocol.Cut(e.RawData);
            Program.log("INFO", userpoint + " Received " + packets.Length + " Packets from Local");
            foreach (byte[] packet in packets)
            {
                GoIMMessage data = GoIMProtocol.Decode(e.RawData);
                if (data.operation == GoIMProtocol.WS_OP_USER_AUTHENTICATION)
                {

                    var authInfo = JObject.Parse(Encoding.UTF8.GetString(data.Body));
                    if (!authInfo.ContainsKey("protover"))
                    {
                        authInfo.Add("protover", 2);
                        data.Body = Encoding.UTF8.GetBytes(authInfo.ToString());
                    }
                    else if ((int)authInfo["protover"] < 2)
                    {
                        authInfo["protover"] = 2;
                        data.Body = Encoding.UTF8.GetBytes(authInfo.ToString());
                    }
                    if (connected)
                    {
                        var newpacket = GoIMProtocol.Encode(data);
                        Program.log("INFO", userpoint + " Sending " + newpacket.Length + " Bytes to Remote.");
                        ws.Send(newpacket);

                    }
                    else
                    {
                        cache.Add(GoIMProtocol.Encode(data));
                    }
                }
                else
                {
                    if (connected)
                    {
                        Program.log("INFO", userpoint + " Sending " + packet.Length + " Bytes to Remote.");
                        ws.Send(packet);
                    }
                    else
                    {
                        cache.Add(packet);
                    }
                }
            }
        }
        protected override void OnClose(CloseEventArgs e)
        {
            Program.log("INFO", userpoint + " Local Side Closed. Code: " + e.Code + " Reason:" + e.Reason);
            SafeCloseRemote();
        }
        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            Program.log("ERROR", userpoint + " Local Side Error :" + e.ToString());
            SafeCloseLocal();
            SafeCloseRemote();
        }
        protected void SafeCloseRemote()
        {
            if (ws != null && ws.IsAlive)
            {
                ws.Close();
            }
        }
        protected void SafeCloseLocal(ushort code = 1011, string reason = "Unknown Error")
        {
            if (Context.WebSocket.IsAlive)
            {
                Context.WebSocket.Close();
            }
        }
    }

}
