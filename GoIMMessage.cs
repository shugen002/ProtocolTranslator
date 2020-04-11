namespace ProtocolTranslator
{

    public class GoIMMessage
    {
        public uint packageLength
        {
            get { return this.headerLength + (uint)this.Body.Length; }
        }
        public ushort headerLength;
        public ushort protocolVersion;
        public uint operation;
        public uint sequenceId;
        public byte[] Body;

        public GoIMMessage(ushort headerLength, ushort protocolVersion, uint operation, uint sequenceId, byte[] Body)
        {
            this.headerLength = headerLength;
            this.protocolVersion = protocolVersion;
            this.operation = operation;
            this.sequenceId = sequenceId;
            this.Body = Body;
        }
        public GoIMMessage(uint operation, uint sequenceId, byte[] Body)
        {
            this.headerLength = 16;
            this.protocolVersion = 1;
            this.operation = operation;
            this.sequenceId = sequenceId;
            this.Body = Body;
        }
        public GoIMMessage(uint operation, byte[] Body)
        {
            this.headerLength = 16;
            this.protocolVersion = 1;
            this.operation = operation;
            this.sequenceId = 1;
            this.Body = Body;
        }
    }
}


