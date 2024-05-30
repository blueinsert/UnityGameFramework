
using System;
using System.Text;
using FixMath;
using FixMath.NET;

namespace bluebean.UGFramework.GamePlay
{

    public class SnapShotReader
    {

        private byte[] m_buffer;
        private int m_offset;

        public SnapShotReader(byte[] buffer, int offset)
        {
            m_buffer = buffer;
            m_offset = offset;
        }

        public byte ReadUInt8()
        {
            var n = m_buffer[m_offset];
            m_offset += 1;
            return n;
        }

        public bool ReadBool()
        {
            var n = m_buffer[m_offset];
            m_offset += 1;
            if(n == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public UInt32 ReadUInt16()
        {
            var n = Bits.GetUInt16(m_buffer, m_offset);
            m_offset += 2;
            return n;
        }

        public UInt32 ReadUInt32()
        {
            var n = Bits.GetUInt32(m_buffer, m_offset);
            m_offset += 4;
            return n;
        }

        public Int32 ReadInt32()
        {
            var n = Bits.GetInt32(m_buffer, m_offset);
            m_offset += 4;
            return n;
        }

        public UInt64 ReadUInt64()
        {
            var n = Bits.GetUInt64(m_buffer, m_offset);
            m_offset += 8;
            return n;
        }

        public Fix64 ReadNumber()
        {
            var n = Bits.GetInt64(m_buffer, m_offset);
            m_offset += 8;
            Fix64 value = Fix64.FromRaw(n);
            return value;
        }

        public string ReadString()
        {
            var length = (int)ReadUInt32();
            var bytes = ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }

        public byte[] ReadBytes(int size)
        {
            var bytes = new byte[size];
            Buffer.BlockCopy(m_buffer, m_offset, bytes, 0, bytes.Length);
            m_offset += size;
            return bytes;
        }

    }

}