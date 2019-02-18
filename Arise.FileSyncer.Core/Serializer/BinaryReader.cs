using System;
using System.IO;
using System.Text;

namespace Arise.FileSyncer.Serializer
{
    public static class BinaryReader
    {
        #region Strings
        /// <summary>
        /// Reads a string from the stream.
        /// </summary>
        public static string ReadString(this Stream stream)
        {
            return Encoding.UTF8.GetString(stream.Read(stream.ReadUInt16()));
        }

        /// <summary>
        /// Reads a string array from the stream.
        /// </summary>
        /// <param name="amount">The number of elements to read. Less then 0 means read amount from the stream.</param>
        public static string[] ReadStringArray(this Stream stream, int amount = -1)
        {
            if (amount < 0) amount = stream.ReadInt32();
            string[] data = new string[amount];
            for (int i = 0; i < amount; i++)
            {
                data[i] = stream.ReadString();
            }
            return data;
        }
        #endregion

        #region Integers
        /// <summary>
        /// Reads a short from the stream.
        /// </summary>
        public static short ReadInt16(this Stream stream)
        {
            return BitConverter.ToInt16(stream.Read(sizeof(short)), 0);
        }

        /// <summary>
        /// Reads a short array from the stream.
        /// </summary>
        /// <param name="amount">The number of elements to read. Less then 0 means read amount from the stream.</param>
        public static short[] ReadInt16Array(this Stream stream, int amount = -1)
        {
            if (amount < 0) amount = stream.ReadInt32();
            short[] data = new short[amount];
            for (int i = 0; i < amount; i++)
            {
                data[i] = stream.ReadInt16();
            }
            return data;
        }

        /// <summary>
        /// Reads an int from the stream.
        /// </summary>
        public static int ReadInt32(this Stream stream)
        {
            return BitConverter.ToInt32(stream.Read(sizeof(int)), 0);
        }

        /// <summary>
        /// Reads an int array from the stream.
        /// </summary>
        /// <param name="amount">The number of elements to read. Less then 0 means read amount from the stream.</param>
        public static int[] ReadInt32Array(this Stream stream, int amount = -1)
        {
            if (amount < 0) amount = stream.ReadInt32();
            int[] data = new int[amount];
            for (int i = 0; i < amount; i++)
            {
                data[i] = stream.ReadInt32();
            }
            return data;
        }

        /// <summary>
        /// Reads a long from the stream.
        /// </summary>
        public static long ReadInt64(this Stream stream)
        {
            return BitConverter.ToInt64(stream.Read(sizeof(long)), 0);
        }

        /// <summary>
        /// Reads a long array from the stream.
        /// </summary>
        /// <param name="amount">The number of elements to read. Less then 0 means read amount from the stream.</param>
        public static long[] ReadInt64Array(this Stream stream, int amount = -1)
        {
            if (amount < 0) amount = stream.ReadInt32();
            long[] data = new long[amount];
            for (int i = 0; i < amount; i++)
            {
                data[i] = stream.ReadInt64();
            }
            return data;
        }

        /// <summary>
        /// Reads an unsigned short from the stream.
        /// </summary>
        public static ushort ReadUInt16(this Stream stream)
        {
            return BitConverter.ToUInt16(stream.Read(sizeof(ushort)), 0);
        }

        /// <summary>
        /// Reads an unsigned short array from the stream.
        /// </summary>
        /// <param name="amount">The number of elements to read. Less then 0 means read amount from the stream.</param>
        public static ushort[] ReadUInt16Array(this Stream stream, int amount = -1)
        {
            if (amount < 0) amount = stream.ReadInt32();
            ushort[] data = new ushort[amount];
            for (int i = 0; i < amount; i++)
            {
                data[i] = stream.ReadUInt16();
            }
            return data;
        }

        /// <summary>
        /// Reads an unsigned int from the stream.
        /// </summary>
        public static uint ReadUInt32(this Stream stream)
        {
            return BitConverter.ToUInt32(stream.Read(sizeof(uint)), 0);
        }

        /// <summary>
        /// Reads an unsigned int array from the stream.
        /// </summary>
        /// <param name="amount">The number of elements to read. Less then 0 means read amount from the stream.</param>
        public static uint[] ReadUInt32Array(this Stream stream, int amount = -1)
        {
            if (amount < 0) amount = stream.ReadInt32();
            uint[] data = new uint[amount];
            for (int i = 0; i < amount; i++)
            {
                data[i] = stream.ReadUInt32();
            }
            return data;
        }

        /// <summary>
        /// Reads an unsigned long from the stream.
        /// </summary>
        public static ulong ReadUInt64(this Stream stream)
        {
            return BitConverter.ToUInt64(stream.Read(sizeof(ulong)), 0);
        }

        /// <summary>
        /// Reads an unsigned long array from the stream.
        /// </summary>
        /// <param name="amount">The number of elements to read. Less then 0 means read amount from the stream.</param>
        public static ulong[] ReadUInt64Array(this Stream stream, int amount = -1)
        {
            if (amount < 0) amount = stream.ReadInt32();
            ulong[] data = new ulong[amount];
            for (int i = 0; i < amount; i++)
            {
                data[i] = stream.ReadUInt64();
            }
            return data;
        }
        #endregion

        #region Bytes
        /// <summary>
        /// Reads a bool from the stream.
        /// </summary>
        public static bool ReadBoolean(this Stream stream)
        {
            return (stream.ReadOneByte() > 0) ? true : false;
        }

        /// <summary>
        /// Reads a bool array from the stream.
        /// </summary>
        /// <param name="amount">The number of elements to read. Less then 0 means read amount from the stream.</param>
        public static bool[] ReadBooleanArray(this Stream stream, int amount = -1)
        {
            if (amount < 0) amount = stream.ReadInt32();
            bool[] data = new bool[amount];
            for (int i = 0; i < amount; i++)
            {
                data[i] = stream.ReadBoolean();
            }
            return data;
        }

        /// <summary>
        /// Reads a byte from the stream.
        /// </summary>
        public static byte ReadOneByte(this Stream stream)
        {
            int read = stream.ReadByte();
            if (read == -1) throw new EndOfStreamException();
            return (byte)read;
        }

        /// <summary>
        /// Reads a byte array from the stream.
        /// </summary>
        /// <param name="amount">The number of elements to read. Less then 0 means read amount from the stream.</param>
        public static byte[] ReadByteArray(this Stream stream, int amount = -1)
        {
            if (amount < 0) amount = stream.ReadInt32();
            return stream.Read(amount);
        }
        #endregion

        #region IBinarySerializable
        /// <summary>
        /// Reads an IBinarySerializable from the stream.
        /// </summary>
        public static T Read<T>(this Stream stream) where T : IBinarySerializable, new()
        {
            T data = new T();
            data.Deserialize(stream);
            return data;
        }

        /// <summary>
        /// Reads an IBinarySerializable array from the stream.
        /// </summary>
        /// <param name="amount">The number of elements to read. Less then 0 means read amount from the stream.</param>
        public static T[] ReadArray<T>(this Stream stream, int amount = -1) where T : IBinarySerializable, new()
        {
            if (amount < 0) amount = stream.ReadInt32();
            T[] data = new T[amount];
            for (int i = 0; i < amount; i++)
            {
                data[i] = stream.Read<T>();
            }
            return data;
        }
        #endregion

        #region Miscellaneous
        /// <summary>
        /// Reads a System.DateTime (as Ticks.ToLocalTime()) from the stream.
        /// </summary>
        public static DateTime ReadDateTime(this Stream stream)
        {
            return new DateTime(stream.ReadInt64()).ToLocalTime();
        }

        /// <summary>
        /// Reads a System.DateTime array from the stream.
        /// </summary>
        /// <param name="amount">The number of elements to read. Less then 0 means read amount from the stream.</param>
        public static DateTime[] ReadDateTimeArray(this Stream stream, int amount = -1)
        {
            if (amount < 0) amount = stream.ReadInt32();
            DateTime[] data = new DateTime[amount];
            for (int i = 0; i < amount; i++)
            {
                data[i] = stream.ReadDateTime();
            }
            return data;
        }

        /// <summary>
        /// Reads a GUID from the stream.
        /// </summary>
        public static Guid ReadGuid(this Stream stream)
        {
            return new Guid(stream.Read(16));
        }

        /// <summary>
        /// Reads a Guid array from the stream.
        /// </summary>
        /// <param name="amount">The number of elements to read. Less then 0 means read amount from the stream.</param>
        public static Guid[] ReadGuidArray(this Stream stream, int amount = -1)
        {
            if (amount < 0) amount = stream.ReadInt32();
            Guid[] data = new Guid[amount];
            for (int i = 0; i < amount; i++)
            {
                data[i] = stream.ReadGuid();
            }
            return data;
        }
        #endregion

        #region Helper
        private static byte[] Read(this Stream stream, int amount)
        {
            byte[] buffer = new byte[amount];
            int read = 0;

            while (read < amount)
            {
                int localRead = stream.Read(buffer, read, amount - read);
                if (localRead == 0) throw new EndOfStreamException();
                read += localRead;
            }

            return buffer;
        }
        #endregion
    }
}
