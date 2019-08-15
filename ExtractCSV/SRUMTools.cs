using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractCSV
{
    class SRUMTools
    {
        public object[] Unpack(string fmt, byte[] bytes)
        {
            byte[] revBytes = bytes;
            // First we parse the format string to make sure it's proper.
            if (fmt.Length < 1) throw new ArgumentException("Format string cannot be empty.");

            bool endianFlip = false;
            if (fmt.Substring(0, 1) == "<")
            {
                // Little endian.
                // Do we need to flip endianness?
                if (BitConverter.IsLittleEndian == false)
                {
                    endianFlip = true;
                    Array.Reverse(revBytes);
                }
                fmt = fmt.Substring(1);
            }
            else if (fmt.Substring(0, 1) == ">")
            {
                // Big endian.
                // Do we need to flip endianness?
                if (BitConverter.IsLittleEndian == true)
                {
                    endianFlip = true;
                    Array.Reverse(revBytes);
                }
                fmt = fmt.Substring(1);
            }

            // Now, we find out how long the byte array needs to be
            int totalByteLength = 0;
            foreach (char c in fmt.ToCharArray())
            {
                switch (c)
                {
                    case 'b':
                    case 'B':
                        totalByteLength += 1;
                        break;
                    case 's':
                    case 'S':
                        totalByteLength += 2;
                        break;
                    case 'i':
                    case 'I':
                    case 'l':
                    case 'L':
                        totalByteLength += 4;
                        break;
                    case 'h':
                    case 'H':
                        totalByteLength += 2;
                        break;
                    case 'd':
                    case 'q':
                    case 'Q':
                        totalByteLength += 8;
                        break;
                    default:
                        throw new ArgumentException("Invalid character found in format string.");
                }
            }

            // Test the byte array length to see if it contains as many bytes as is needed for the string.
            if (revBytes.Length != totalByteLength) throw new ArgumentException("The number of bytes provided does not match the total length of the format string.");

            // Ok, we can go ahead and start parsing bytes!
            int byteArrayPosition = 0;
            List<object> outputList = new List<object>();

            foreach (char c in fmt.ToCharArray())
            {
                switch (c)
                {
                    case 'b':
                        outputList.Add((object)(sbyte)BitConverter.ToChar(revBytes, byteArrayPosition));
                        break;
                    case 'B':
                        outputList.Add((object)(byte)BitConverter.ToChar(revBytes, byteArrayPosition));
                        break;
                    case 's':
                        outputList.Add((object)(short)BitConverter.ToInt16(revBytes, byteArrayPosition));
                        break;
                    case 'S':
                        outputList.Add((object)(ushort)BitConverter.ToUInt16(revBytes, byteArrayPosition));
                        break;
                    case 'i':
                        outputList.Add((object)(int)BitConverter.ToInt32(revBytes, byteArrayPosition));
                        byteArrayPosition += 4;
                        break;
                    case 'I':
                        outputList.Add((object)(uint)BitConverter.ToUInt32(revBytes, byteArrayPosition));
                        byteArrayPosition += 4;
                        break;
                    case 'l':
                        outputList.Add((object)(long)BitConverter.ToInt64(revBytes, byteArrayPosition));
                        byteArrayPosition += 4;
                        break;
                    case 'L':
                        outputList.Add((object)(ulong)BitConverter.ToUInt64(revBytes, byteArrayPosition));
                        byteArrayPosition += 4;
                        break;
                    case 'h':
                        outputList.Add((object)(short)BitConverter.ToInt16(revBytes, byteArrayPosition));
                        byteArrayPosition += 2;
                        break;
                    case 'H':
                        outputList.Add((object)(ushort)BitConverter.ToUInt16(revBytes, byteArrayPosition));
                        byteArrayPosition += 2;
                        break;
                    case 'd':
                        outputList.Add((object)(double)BitConverter.ToDouble(revBytes, byteArrayPosition));
                        byteArrayPosition += 8;
                        break;
                    case 'q':
                        outputList.Add((object)(long)BitConverter.ToInt64(revBytes, byteArrayPosition));
                        byteArrayPosition += 8;
                        break;
                    case 'Q':
                        outputList.Add((object)(ulong)BitConverter.ToUInt64(revBytes, byteArrayPosition));
                        byteArrayPosition += 8;
                        break;
                    default:
                        throw new ArgumentException("You should not be here.");
                }
            }
            return outputList.ToArray();
        }

        uint swapEndianness(uint x)
        {
            // swap adjacent 16-bit blocks
            x = (x >> 16) | (x << 16);
            // swap adjacent 8-bit blocks
            return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
        }
    }
}
