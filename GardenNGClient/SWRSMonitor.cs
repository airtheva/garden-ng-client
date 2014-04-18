using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GardenNGClient
{
    public class SWRSMonitor
    {

        public event EventHandler OnGameFound;
        public event EventHandler OnGameMiss;

        static SWRSMonitor mInstance;

        public static SWRSMonitor GetInstance() {

            if (mInstance == null)
            {
                mInstance = new SWRSMonitor();
            }

            return mInstance;

        }

        Thread mThread;
        IntPtr mHProcess;
        bool mIsWatching;

        private SWRSMonitor()
        {

            mThread = new Thread(new ThreadStart(watch));

            mHProcess = IntPtr.Zero;

            mIsWatching = false;

        }

        void watch()
        {

            while (true)
            {

                if (!mIsWatching)
                {

                    goto Break;

                }

                if (mHProcess == IntPtr.Zero)
                {

                    OnGameMiss(this, null);

                    IntPtr hWnd = IntPtr.Zero;
                    while (hWnd == IntPtr.Zero)
                    {

                        if (!mIsWatching)
                        {
                            goto Break;
                        }

                        hWnd = Win32API.FindWindow("th123_110", null);
                        Thread.Sleep(1000);
                    }
                    Console.WriteLine("hWnd: {0}.", hWnd);

                    uint dwProcessId = 0;
                    Win32API.GetWindowThreadProcessId(hWnd, out dwProcessId);
                    Console.WriteLine("dwProcessId: {0}.", dwProcessId);

                    mHProcess = Win32API.OpenProcess(Win32API.ProcessAccessFlags.VMRead, false, dwProcessId);
                    Console.WriteLine("hProcess: {0}.", mHProcess);

                    OnGameFound(this, null);

                }

                byte[] lpBuffer = new byte[256];
                uint lpNumberOfBytesRead = 0;

                if (!Win32API.ReadProcessMemory(mHProcess, 0x0088D024, lpBuffer, 4, out lpNumberOfBytesRead))
                {
                    Win32API.CloseHandle(mHProcess);
                    mHProcess = IntPtr.Zero;
                    continue;
                }

                
                //Console.WriteLine("sceneId: {0}.", BitConverter.ToInt32(lpBuffer, 0));

                Thread.Sleep(1000);

            }

            Break:

            Win32API.CloseHandle(mHProcess);
            mHProcess = IntPtr.Zero;

            Console.WriteLine("Stopped.");

        }

        public void Start()
        {

            mIsWatching = true;

            mThread.Start();

        }

        public void Stop()
        {

            mIsWatching = false;

        }

    }
}
