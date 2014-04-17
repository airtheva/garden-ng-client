using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Timers;

namespace GardenNGClient
{
    class UDPSocketProxy
    {

        static UDPSocketProxy mInstance;

        public static UDPSocketProxy GetInstance()
        {

            if (mInstance == null)
            {
                mInstance = new UDPSocketProxy();
            }

            return mInstance;

        }

        public static uint IOC_IN = 0x80000000;
        public static uint IOC_VENDOR = 0x18000000;
        public static uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

        public IPEndPoint HeartbeatEP;

        Timer mHeartbeatTimer;

        byte[] mLoopbackBuffer;
        byte[] mListenBuffer;

        EndPoint mLoopbackEP;
        EndPoint mListenEP;

        Socket mLoopback;
        Socket mListen;

        private UDPSocketProxy()
        {

            mHeartbeatTimer = new Timer();
            mHeartbeatTimer.Interval = 1000;
            mHeartbeatTimer.Elapsed += onHeartbeatElapsed;

            mLoopbackBuffer = new byte[2048];
            mListenBuffer = new byte[2048];

            mLoopbackEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 10800);
            mListenEP = new IPEndPoint(IPAddress.Any, 0);

            mLoopback = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mLoopback.IOControl((int) SIO_UDP_CONNRESET, new byte[] { 0x00 }, null);
            mListen = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            mListen.IOControl((int) SIO_UDP_CONNRESET, new byte[] { 0x00 }, null);

            mLoopback.Bind(new IPEndPoint(IPAddress.Any, 0));
            mListen.Bind(new IPEndPoint(IPAddress.Any, 0));

            Start();

        }

        void onHeartbeatElapsed(object sender, ElapsedEventArgs e)
        {

            heartbeat();

        }

        void loopbackReceiveCallback(IAsyncResult ar)
        {

            int length = mLoopback.EndReceiveFrom(ar, ref mLoopbackEP);

            mListen.SendTo(mLoopbackBuffer, 0, length, SocketFlags.None, mListenEP);

            mLoopback.BeginReceiveFrom(mLoopbackBuffer, 0, 2048, SocketFlags.None, ref mLoopbackEP, new AsyncCallback(loopbackReceiveCallback), mLoopback);

        }

        void listenReceiveCallback(IAsyncResult ar)
        {

            int length = mListen.EndReceiveFrom(ar, ref mListenEP);

            if (length == 1)
            {

                Console.WriteLine("heartbeat, receive.");

            }
            else
            {

                mLoopback.SendTo(mListenBuffer, 0, length, SocketFlags.None, mLoopbackEP);

            }

            mListen.BeginReceiveFrom(mListenBuffer, 0, 2048, SocketFlags.None, ref mListenEP, new AsyncCallback(listenReceiveCallback), mListen);

        }

        void heartbeat()
        {

            if (HeartbeatEP != null)
            {
                Console.WriteLine("heartbeat, send.");
                mListen.SendTo(new byte[] { 0x00 }, 0, 1, SocketFlags.None, HeartbeatEP);
            }
            
        }

        public void Start()
        {

            mLoopback.BeginReceiveFrom(mLoopbackBuffer, 0, 2048, SocketFlags.None, ref mLoopbackEP, new AsyncCallback(loopbackReceiveCallback), mLoopback);
            mListen.BeginReceiveFrom(mListenBuffer, 0, 2048, SocketFlags.None, ref mListenEP, new AsyncCallback(listenReceiveCallback), mListen);

            mHeartbeatTimer.Start();

        }

        public void Stop()
        {

            mHeartbeatTimer.Stop();

            mLoopback.Close();
            mListen.Close();

        }

    }
}
