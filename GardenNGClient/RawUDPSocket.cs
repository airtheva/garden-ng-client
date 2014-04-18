using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace GardenNGClient
{
    public static class RawUDPSocket
    {

        public class UDPPacket {

            byte[] mSourcePort = null;
            byte[] mTargetAddress = null;
            byte[] mTargetPort = null;
            byte[] mLength = null;
            byte[] mChecksum = null;
            byte[] mData = null;

            public UDPPacket(ushort sourcePort, IPAddress targetAddress, ushort targetPort, byte[] data)
            {

                mSourcePort = BitConverter.GetBytes(sourcePort);
                mTargetAddress = targetAddress.GetAddressBytes();
                mTargetPort = BitConverter.GetBytes(targetPort);
                mLength = BitConverter.GetBytes((ushort)(8 + data.Length));

                Array.Reverse(mSourcePort);
                Array.Reverse(mTargetAddress);
                Array.Reverse(mTargetPort);
                Array.Reverse(mLength);

                mData = data;

            }

            ushort checksum()
            {

                // TODO:
                byte[] buffer = new byte[1];

                int length = buffer.Length / 2;

                uint checksum = 0;

                for (int i = 0; i < length; i++)
                {
                    System.Console.WriteLine(i);
                    checksum += BitConverter.ToUInt16(buffer, i * 2);
                }

                checksum = (checksum >> 16) + (checksum & 0xffff);
                checksum += checksum >> 16;

                return (ushort)~checksum;

            }

            public byte[] GetBytes()
            {

                byte[] bytes = new byte[8 + mData.Length];
                mSourcePort.CopyTo(bytes, 0);
                mTargetPort.CopyTo(bytes, 2);
                mLength.CopyTo(bytes, 4);
                (new byte[] { 0x00, 0x00 }).CopyTo(bytes, 6);
                mData.CopyTo(bytes, 8);

                mChecksum = BitConverter.GetBytes(checksum());

                Array.Reverse(mChecksum);

                mChecksum.CopyTo(bytes, 6);

                return bytes;

            }

        }

    }
}
