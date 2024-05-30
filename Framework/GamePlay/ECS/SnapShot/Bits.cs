// Copyright (C) 2017 Joywinds Inc.

using System;

namespace bluebean.UGFramework.GamePlay
{

    public class Bits
    {
        public static UInt16 GetUInt16(byte[] buf, int offset)
        {
            return (UInt16)((UInt16)buf[offset + 0] |
                            (UInt16)buf[offset + 1] << 8);
        }

        public static UInt32 GetUInt32(byte[] buf, int offset)
        {
            return (UInt32)buf[offset + 0] |
                   (UInt32)buf[offset + 1] << 8 |
                   (UInt32)buf[offset + 2] << 16 |
                   (UInt32)buf[offset + 3] << 24;
        }

        public static Int32 GetInt32(byte[] buf, int offset)
        {
            return (Int32)buf[offset + 0] |
                   (Int32)buf[offset + 1] << 8 |
                   (Int32)buf[offset + 2] << 16 |
                   (Int32)buf[offset + 3] << 24;
        }

        public static UInt64 GetUInt64(byte[] buf, int offset)
        {
            return (UInt64)buf[offset + 0] |
                   (UInt64)buf[offset + 1] << 8 |
                   (UInt64)buf[offset + 2] << 16 |
                   (UInt64)buf[offset + 3] << 24 |
                   (UInt64)buf[offset + 4] << 32 |
                   (UInt64)buf[offset + 5] << 40 |
                   (UInt64)buf[offset + 6] << 48 |
                   (UInt64)buf[offset + 7] << 56;
        }

        public static Int64 GetInt64(byte[] buf, int offset)
        {
            return (Int64)buf[offset + 0] |
                   (Int64)buf[offset + 1] << 8 |
                   (Int64)buf[offset + 2] << 16 |
                   (Int64)buf[offset + 3] << 24 |
                   (Int64)buf[offset + 4] << 32 |
                   (Int64)buf[offset + 5] << 40 |
                   (Int64)buf[offset + 6] << 48 |
                   (Int64)buf[offset + 7] << 56;
        }

        public static void PutUInt8(byte n, byte[] buf, int offset)
        {
            buf[offset] = n;
        }

        public static void PutUInt16(UInt16 n, byte[] buf, int offset)
        {
            buf[offset + 0] = (byte)(n);
            buf[offset + 1] = (byte)(n >> 8);
        }

        public static void PutUInt32(UInt32 n, byte[] buf, int offset)
        {
            buf[offset + 0] = (byte)(n);
            buf[offset + 1] = (byte)(n >> 8);
            buf[offset + 2] = (byte)(n >> 16);
            buf[offset + 3] = (byte)(n >> 24);
        }

        public static void PutInt32(Int32 n, byte[] buf, int offset)
        {
            buf[offset + 0] = (byte)(n);
            buf[offset + 1] = (byte)(n >> 8);
            buf[offset + 2] = (byte)(n >> 16);
            buf[offset + 3] = (byte)(n >> 24);
        }

        public static void PutUInt64(UInt64 n, byte[] buf, int offset)
        {
            buf[offset + 0] = (byte)(n);
            buf[offset + 1] = (byte)(n >> 8);
            buf[offset + 2] = (byte)(n >> 16);
            buf[offset + 3] = (byte)(n >> 24);
            buf[offset + 4] = (byte)(n >> 32);
            buf[offset + 5] = (byte)(n >> 40);
            buf[offset + 6] = (byte)(n >> 48);
            buf[offset + 7] = (byte)(n >> 56);
        }

        public static void PutInt64(Int64 n, byte[] buf, int offset)
        {
            buf[offset + 0] = (byte)(n);
            buf[offset + 1] = (byte)(n >> 8);
            buf[offset + 2] = (byte)(n >> 16);
            buf[offset + 3] = (byte)(n >> 24);
            buf[offset + 4] = (byte)(n >> 32);
            buf[offset + 5] = (byte)(n >> 40);
            buf[offset + 6] = (byte)(n >> 48);
            buf[offset + 7] = (byte)(n >> 56);
        }

        public static void PutBytes(byte[] bytes, byte[] buf, int offset)
        {
            Buffer.BlockCopy(bytes, 0, buf, offset, bytes.Length);
        }
    }

}