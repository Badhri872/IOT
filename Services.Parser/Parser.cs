using System;

namespace Services
{
    public static class Parser
    {
        public static float Float32(byte[] bufferReceiver)
        {
            var reg1 = (ushort)((bufferReceiver[3] << 8 | bufferReceiver[4]));
            var reg2 = (ushort)((bufferReceiver[5] << 8 | bufferReceiver[6]));
            int actualData = reg1 << 16 | reg2; 
            var floatData = BitConverter.ToSingle(BitConverter.GetBytes(actualData), 0);
            return float.IsNaN(floatData) ? 0.0f : floatData;
        }

        public static int Int16U(byte[] bufferReceiver)
        {
            var regData = (ushort)((bufferReceiver[3] << 8 | bufferReceiver[4]));
            return BitConverter.ToInt32(BitConverter.GetBytes(regData), 0);
        }
    }
}
