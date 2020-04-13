using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ProtocolTranslator
{
    static class GoIMProtocol
    {
        public static int WS_PACKAGE_OFFSET = 0;
        public static int WS_HEADER_OFFSET = 4;
        public static int WS_VERSION_OFFSET = 6;
        public static int WS_OPERATION_OFFSET = 8;
        public static int WS_SEQUENCE_OFFSET = 12;
        public static uint WS_OP_HEARTBEAT = 2;
        public static uint WS_OP_HEARTBEAT_REPLY = 3;
        public static uint WS_OP_MESSAGE = 5;
        public static uint WS_OP_USER_AUTHENTICATION = 7;
        public static uint WS_OP_CONNECT_SUCCESS = 8;
        public static byte[] Encode(GoIMMessage message)
        {
            byte[] result = new Byte[message.packageLength];
            MemoryStream stream = new MemoryStream(result);
            stream.Write(ToBE(BitConverter.GetBytes(message.packageLength)), WS_PACKAGE_OFFSET - (int)stream.Position, 4);
            stream.Write(ToBE(BitConverter.GetBytes(message.headerLength)), WS_HEADER_OFFSET - (int)stream.Position, 2);
            stream.Write(ToBE(BitConverter.GetBytes(message.protocolVersion)), WS_VERSION_OFFSET - (int)stream.Position, 2);
            stream.Write(ToBE(BitConverter.GetBytes(message.operation)), WS_OPERATION_OFFSET - (int)stream.Position, 4);
            stream.Write(ToBE(BitConverter.GetBytes(message.sequenceId)), WS_SEQUENCE_OFFSET - (int)stream.Position, 4);
            stream.Write(message.Body, message.headerLength - (int)stream.Position, message.Body.Length);
            return result;
        }
        public static byte[][] Cut(byte[] data)
        {
            var result = new List<byte[]>();
            int pointer = 0;
            while (pointer < data.Length)
            {
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(data, WS_PACKAGE_OFFSET + pointer, 4);
                }
                int packageLength = BitConverter.ToInt32(data, WS_PACKAGE_OFFSET + pointer);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(data, WS_PACKAGE_OFFSET + pointer, 4);
                }
                if (packageLength == 0) break;
                result.Add(data.Skip(pointer).Take(packageLength).ToArray());
                pointer += packageLength;
            };
            return result.ToArray();
        }
        public static GoIMMessage Decode(byte[] data)
        {
            var bigData = (byte[])data.Clone();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bigData, WS_HEADER_OFFSET, 2);
                Array.Reverse(bigData, WS_VERSION_OFFSET, 2);
                Array.Reverse(bigData, WS_OPERATION_OFFSET, 4);
                Array.Reverse(bigData, WS_SEQUENCE_OFFSET, 4);
            }

            ushort headerLength = BitConverter.ToUInt16(bigData, WS_HEADER_OFFSET);
            return new GoIMMessage((ushort)headerLength,
             BitConverter.ToUInt16(bigData, WS_VERSION_OFFSET),
             BitConverter.ToUInt32(bigData, WS_OPERATION_OFFSET),
             BitConverter.ToUInt32(bigData, WS_SEQUENCE_OFFSET),
             bigData.Skip(headerLength).Take(bigData.Length - headerLength).ToArray());
        }
        public static byte[] ToBE(byte[] b)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(b);
            }
            return b;
        }
    }
}
