using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AdvancedWarpplate
{
    public class PacketFactory
    {
        private MemoryStream memoryStream;
        private BinaryWriter writer;
        public PacketFactory()
        {
            memoryStream = new MemoryStream();
            writer = new BinaryWriter(memoryStream);
            writer.BaseStream.Position = 3L;
        }

        public PacketFactory SetType(short type)
        {
            long currentPosition = writer.BaseStream.Position;
            writer.BaseStream.Position = 2L;
            writer.Write(type);
            writer.BaseStream.Position = currentPosition;
            return this;
        }

        public PacketFactory PackByte(byte num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackInt16(short num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackInt32(int num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackUInt64(ulong num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackSingle(float num)
        {
            writer.Write(num);
            return this;
        }

        public PacketFactory PackString(string str)
        {
            writer.Write(str);
            return this;
        }

        private void UpdateLength()
        {
            long currentPosition = writer.BaseStream.Position;
            writer.BaseStream.Position = 0L;
            writer.Write((short)currentPosition);
            writer.BaseStream.Position = currentPosition;
        }
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public byte[] GetByteData()
        {
            UpdateLength();
            return memoryStream.ToArray();
        }
    }
}
