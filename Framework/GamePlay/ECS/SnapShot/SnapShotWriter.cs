
using System;
using System.Text;
using System.IO;
using FixMath;
using FixMath.NET;

namespace bluebean.UGFramework.GamePlay
{

    public class SnapShotWriter
    {

        private MemoryStream m_stream;

        public SnapShotWriter()
        {
            m_stream = new MemoryStream();
        }

        public byte[] GetBuffer()
        {
            return m_stream.ToArray();
        }

        public void WriteBool(bool value)
        {
            m_stream.WriteByte(value ? (byte)1 : (byte)0);
        }

        public void WriteUInt8(byte value)
        {
            m_stream.WriteByte(value);
        }

        public void WriteUInt16(UInt16 value)
        {
            var bytes = new byte[2];
            Bits.PutUInt16(value, bytes, 0);
            m_stream.Write(bytes, 0, bytes.Length);
        }

        public void WriteUInt32(UInt32 value)
        {
            var bytes = new byte[4];
            Bits.PutUInt32(value, bytes, 0);
            m_stream.Write(bytes, 0, bytes.Length);
        }

        public void WriteInt32(Int32 value)
        {
            var bytes = new byte[4];
            Bits.PutInt32(value, bytes, 0);
            m_stream.Write(bytes, 0, bytes.Length);
        }

        public void WriteUInt64(UInt64 value)
        {
            var bytes = new byte[8];
            Bits.PutUInt64(value, bytes, 0);
            m_stream.Write(bytes, 0, bytes.Length);
        }

        public void WriteNumber(Fix64 number)
        {
            WriteInt64(number.RawValue);
        }

        public void WriteInt64(Int64 value)
        {
            var bytes = new byte[8];
            Bits.PutInt64(value, bytes, 0);
            m_stream.Write(bytes, 0, bytes.Length);
        }

        public void WriteString(string value)
        {
            if (value == null) value = "";
            var bytes = Encoding.UTF8.GetBytes(value);
            WriteUInt32((UInt32)bytes.Length);
            WriteBytes(bytes, 0, bytes.Length);
        }

        public void WriteBytes(byte[] bytes, int offset, int size)
        {
            m_stream.Write(bytes, offset, size);
        }
    }
    /*
    public class BytesWriter
    {
        private byte[] m_data;
        private int m_size;

        public BytesWriter(byte[] buf)
        {
            m_data = buf;
            m_size = 0;
        }

        public void Reset()
        {
            m_size = 0;
        }

        public void Write(byte v)
        {
            Bits.PutUInt8(v, m_data, m_size);
            m_size += 1;
        }

        public void Write(UInt16 v)
        {
            Bits.PutUInt16(v, m_data, m_size);
            m_size += 2;
        }

        public void Write(UInt32 v)
        {
            Bits.PutUInt32(v, m_data, m_size);
            m_size += 4;
        }

        public void Write(UInt64 v)
        {
            Bits.PutUInt64(v, m_data, m_size);
            m_size += 8;
        }

        public void Write(byte[] buf)
        {
            Buffer.BlockCopy(buf, 0, m_data, m_size, buf.Length);
            m_size += buf.Length;
        }

        public int GetCapacity()
        {
            return m_data.Length;
        }

        public int GetSize()
        {
            return m_size;
        }

        public byte[] GetData()
        {
            return m_data;
        }
    }
    */
}