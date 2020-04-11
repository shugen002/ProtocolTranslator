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
        public GoIMProxy()
        {
            this.IgnoreExtensions = true;
        }
        protected override void OnOpen()
        {
            Program.log("info", Context.UserEndPoint.ToString() + " Connected");
            ws = new WebSocket(ServerController.url);
            ws.OnClose += OnWSClose;
            ws.OnOpen += OnWSOpen;
            ws.OnError += OnWSError;
            ws.OnMessage += OnWSMessage;
            ws.Connect();
        }

        private void OnWSMessage(object sender, MessageEventArgs e)
        {
            Program.log("info", "WS Received " + e.RawData.Length);
            packetHandler(e.RawData);
        }

        private void packetHandler(Byte[] bytes)
        {
            Byte[][] packets = GoIMProtocol.Cut(bytes);
            foreach (Byte[] packet in packets)
            {
                GoIMMessage data = GoIMProtocol.Decode(packet);
                if (data.operation == GoIMProtocol.WS_OP_MESSAGE && data.protocolVersion == 2)
                {
                    var rawStream = new MemoryStream(data.Body, 2, data.Body.Length - 2);
                    DeflateStream deflateStream = new DeflateStream(rawStream, CompressionMode.Decompress);
                    MemoryStream memoryStream = new MemoryStream();
                    deflateStream.CopyTo(memoryStream);

                    Byte[][] decompressedPackets = GoIMProtocol.Cut(memoryStream.GetBuffer());
                    foreach (byte[] decompressedPacket in decompressedPackets)
                    {
                        packetHandler(decompressedPacket);
                    }
                }
                else
                {
                    Send(packet);
                }
            }
        }

        private void OnWSError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Program.log("Error", e.ToString());
            SafeCloseLocal();
            SafeCloseRemote();
        }

        private void OnWSOpen(object sender, EventArgs e)
        {
            connected = true;
            if (cache.Count > 0)
            {
                foreach (byte[] item in cache)
                {
                    ws.Send(item);
                }
            }
        }

        private void OnWSClose(object sender, CloseEventArgs e)
        {
            SafeCloseLocal();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            byte[][] packets = GoIMProtocol.Cut(e.RawData);
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
                        ws.Send(GoIMProtocol.Encode(data));

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
            SafeCloseRemote();
        }
        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            Program.log("Error", e.ToString());
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
        protected void SafeCloseLocal()
        {
            if (Context.WebSocket.IsAlive)
            {
                Context.WebSocket.Close();
            }
        }
    }

}
