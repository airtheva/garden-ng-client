using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GardenNGClient
{
    public class SWRSMonitor
    {

        public static class SWRS_ADDR
        {

            public static int SCENEID;
            public static int COMMMODE;
            public static int PNETOBJECT;
            public static int PBATTLEMGR;
            public static int BTLMODEOFS;
            public static int LPROFOFS;
            public static int RPROFOFS;
            public static int LCHARID;
            public static int RCHARID;
            public static int LCHAROFS;
            public static int RCHAROFS;
            public static int WINCNTOFS;

            public static void Load(String version)
            {
                switch (version)
                {
                    case "1.10":
                        SCENEID = 0x0088D024;
                        COMMMODE = 0x00885670;
                        PNETOBJECT = 0x00885680;
                        PBATTLEMGR = 0x008855C4;
                        LCHARID = 0x00886CF0;
                        RCHARID = 0x00886D10;
                        break;
                    case "1.10a":
                        SCENEID = 0x008A0044;
                        COMMMODE = 0x00898690;
                        PNETOBJECT = 0x008986A0;
                        PBATTLEMGR = 0x008985E4;
                        LCHARID = 0x00899D10;
                        RCHARID = 0x00899D30;
                        break;
                }

                BTLMODEOFS = 0x88;
                LPROFOFS = 0x04;
                RPROFOFS = 0x24;
                LCHAROFS = 0x0c;
                RCHAROFS = 0x10;
                WINCNTOFS = 0x573;

            }

        }

        public static class SWRSSCENE
        {
            public const int LOGO = 0;
            public const int OPENING = 1;
            public const int TITLE = 2;
            public const int SELECT = 3;
            public const int BATTLE = 5;
            public const int LOADING = 6;
            public const int SELECTSV = 8;
            public const int SELECTCL = 9;
            public const int LOADINGSV = 10;
            public const int LOADINGCL = 11;
            public const int LOADINGWATCH = 12;
            public const int BATTLESV = 13;
            public const int BATTLECL = 14;
            public const int BATTLEWATCH = 15;
            public const int SELECTSENARIO = 16;
            public const int ENDING = 20;

        }

        public static class SWRSCHAR
        {

            public const int REIMU = 0;
            public const int MARISA = 1;
            public const int SAKUYA = 2;
            public const int ALICE = 3;
            public const int PATCHOULI = 4;
            public const int YOUMU = 5;
            public const int REMILIA = 6;
            public const int YUYUKO = 7;
            public const int YUKARI = 8;
            public const int SUICA = 9;
            public const int REISEN = 10;
            public const int AYA = 11;
            public const int KOMACHI = 12;
            public const int IKU = 13;
            public const int TENSHI = 14;
            public const int SANAE = 15;
            public const int CIRNO = 16;
            public const int MEILING = 17;
            public const int UTSUHO = 18;
            public const int SUWAKO = 19;
            public const int ROMAN = 20;

            public static String GetCharacterName(int character)
            {

                String characterName = "";

                switch (character)
                {
                    case 0:
                        characterName = "红白";
                        break;
                    case 1:
                        characterName = "黑白";
                        break;
                    case 2:
                        characterName = "16";
                        break;
                    case 3:
                        characterName = "小爱";
                        break;
                    case 4:
                        characterName = "帕秋莉♂GO";
                        break;
                    case 5:
                        characterName = "妖梦";
                        break;
                    case 6:
                        characterName = "红魔";
                        break;
                    case 7:
                        characterName = "UU";
                        break;
                    case 8:
                        characterName = "紫妈";
                        break;
                    case 9:
                        characterName = "西瓜";
                        break;
                    case 10:
                        characterName = "兔子";
                        break;
                    case 11:
                        characterName = "文文";
                        break;
                    case 12:
                        characterName = "小町";
                        break;
                    case 13:
                        characterName = "19";
                        break;
                    case 14:
                        characterName = "M子";
                        break;
                    case 15:
                        characterName = "早苗";
                        break;
                    case 16:
                        characterName = "⑨";
                        break;
                    case 17:
                        characterName = "中国";
                        break;
                    case 18:
                        characterName = "高达";
                        break;
                    case 19:
                        characterName = "呱呱";
                        break;
                    default:
                        characterName = "纳尼？";
                        break;
                }

                return characterName;

            }

        }

        public static class SWRSCOMMMODE
        {

            public const int SERVER = 4;
            public const int CLIENT = 5;
            public const int WATCH = 6;

        }

        public static class BattleMode
        {
            public const int UNKNOWN = 0;
            public const int PREPARE = 1;
            public const int FIGHT = 2;
            public const int REST = 3;
            public const int END = 5;
            public const int CHAT = 6;
            public const int WAIT = 7;

        }

        public class GameFoundEventArgs : EventArgs
        {

            public String Version;

        }

        public class BattleEndedEventArgs : EventArgs
        {

            public DateTime Time;
            public String LeftPlayerProfile;
            public int LeftPlayerCharacter;
            public String LeftPlayerCharacterName;
            public int LeftPlayerScore;
            public String RightPlayerProfile;
            public int RightPlayerCharacter;
            public String RightPlayerCharacterName;
            public int RightPlayerScore;
            public bool IsHost;
            public int Skip;
            public String Version;

        }

        public delegate void GameFoundEventHandler(Object sender, GameFoundEventArgs e);
        public delegate void BattleEndedEventHandler(Object sender, BattleEndedEventArgs e);
        public delegate void GameLostEventHandler(Object sender, EventArgs e);

        public event GameFoundEventHandler OnGameFound;
        public event BattleEndedEventHandler OnBattleEnded;
        public event GameLostEventHandler OnGameLost;

        static SWRSMonitor mInstance;

        public static SWRSMonitor GetInstance() {

            if (mInstance == null)
            {
                mInstance = new SWRSMonitor();
            }

            return mInstance;

        }

        bool mIsWatching;

        Thread mThread;
        IntPtr mHProcess;

        String mVersion;
        int mLastBattleMode;

        private SWRSMonitor()
        {

            mIsWatching = false;

            mHProcess = IntPtr.Zero;

        }

        void watch()
        {

            while (true)
            {

                if (!mIsWatching)
                {

                    // 完全潇洒の退出。
                    goto Break;

                }

                if (mHProcess == IntPtr.Zero)
                {

                    // 也是要强行寻找游戏。
                    IntPtr hWnd = IntPtr.Zero;
                    while (true)
                    {

                        if (!mIsWatching)
                        {
                            // Me too.
                            goto Break;
                        }

                        if ((hWnd = Win32API.FindWindow("th123_110", null)) != IntPtr.Zero)
                        {

                            mVersion = "1.10";
                            break;

                        }
                        else if ((hWnd = Win32API.FindWindow("th123_110a", null)) != IntPtr.Zero)
                        {

                            mVersion = "1.10a";
                            break;

                        }
                        else
                        {
                            Thread.Sleep(1000);
                            continue;
                        }

                    }

                    // 如果能够执行到这里，那就是找到游戏了。下面输出一些信息。

                    Console.WriteLine("hWnd: {0}.", hWnd);

                    uint dwProcessId = 0;
                    Win32API.GetWindowThreadProcessId(hWnd, out dwProcessId);
                    Console.WriteLine("dwProcessId: {0}.", dwProcessId);

                    mHProcess = Win32API.OpenProcess(Win32API.ProcessAccessFlags.VMRead, false, dwProcessId);
                    Console.WriteLine("hProcess: {0}.", mHProcess);

                    SWRS_ADDR.Load(mVersion);
                    GameFoundEventArgs e = new GameFoundEventArgs();
                    e.Version = mVersion;
                    OnGameFound(this, e);

                }

                byte[] lpBuffer = new byte[256];
                uint lpNumberOfBytesRead = 0;

                // 选择先读取sceneID是因为sceneID从一开始就存在。
                // 如果这里出错，那就下回再见吧。
                if (!Win32API.ReadProcessMemory(mHProcess, SWRS_ADDR.SCENEID, lpBuffer, 4, out lpNumberOfBytesRead))
                {

                    // 触发丢失游戏事件。
                    OnGameLost(this, null);

                    // 释放与重置。
                    Win32API.CloseHandle(mHProcess);
                    mHProcess = IntPtr.Zero;

                    Thread.Sleep(1000);
                    continue;
                }

                int sceneID = BitConverter.ToInt32(lpBuffer, 0);

                switch (sceneID)
                {
                    // 作为服务端或者客户端战斗中，也可能是观战，这时候就要判断CommunicationMode。
                    case SWRSSCENE.BATTLESV:
                    case SWRSSCENE.BATTLECL:

                        Win32API.ReadProcessMemory(mHProcess, SWRS_ADDR.COMMMODE, lpBuffer, 4, out lpNumberOfBytesRead);
                        int communicationMode = BitConverter.ToInt32(lpBuffer, 0);

                        switch (communicationMode)
                        {
                            case SWRSCOMMMODE.SERVER:
                            case SWRSCOMMMODE.CLIENT:

                                Win32API.ReadProcessMemory(mHProcess, SWRS_ADDR.PBATTLEMGR, lpBuffer, 4, out lpNumberOfBytesRead);
                                int pBattleMgr = BitConverter.ToInt32(lpBuffer, 0);
                                Win32API.ReadProcessMemory(mHProcess, pBattleMgr + SWRS_ADDR.BTLMODEOFS, lpBuffer, 4, out lpNumberOfBytesRead);
                                int battleMode = BitConverter.ToInt32(lpBuffer, 0);

                                switch (battleMode)
                                {
                                    case BattleMode.END:

                                        // 这个在天则观里是OnSWRSParamChange。
                                        if (battleMode != mLastBattleMode)
                                        {

                                            BattleEndedEventArgs e = new BattleEndedEventArgs();

                                            e.Time = DateTime.UtcNow;

                                            Win32API.ReadProcessMemory(mHProcess, SWRS_ADDR.PNETOBJECT, lpBuffer, 4, out lpNumberOfBytesRead);
                                            int pNetObject = BitConverter.ToInt32(lpBuffer, 0);

                                            Win32API.ReadProcessMemory(mHProcess, pNetObject + SWRS_ADDR.LPROFOFS, lpBuffer, 0x20, out lpNumberOfBytesRead);
                                            e.LeftPlayerProfile = Encoding.ASCII.GetString(lpBuffer, 0, (int)lpNumberOfBytesRead).Split('\0')[0];

                                            Win32API.ReadProcessMemory(mHProcess, SWRS_ADDR.LCHARID, lpBuffer, 4, out lpNumberOfBytesRead);
                                            e.LeftPlayerCharacter = BitConverter.ToInt32(lpBuffer, 0);
                                            e.LeftPlayerCharacterName = SWRSCHAR.GetCharacterName(e.LeftPlayerCharacter);

                                            Win32API.ReadProcessMemory(mHProcess, pBattleMgr + SWRS_ADDR.LCHAROFS, lpBuffer, 4, out lpNumberOfBytesRead);
                                            int pLChar = BitConverter.ToInt32(lpBuffer, 0);
                                            Win32API.ReadProcessMemory(mHProcess, pLChar + SWRS_ADDR.WINCNTOFS, lpBuffer, 4, out lpNumberOfBytesRead);
                                            e.LeftPlayerScore = lpBuffer[0];

                                            Win32API.ReadProcessMemory(mHProcess, pNetObject + SWRS_ADDR.RPROFOFS, lpBuffer, 0x20, out lpNumberOfBytesRead);
                                            e.RightPlayerProfile = Encoding.ASCII.GetString(lpBuffer, 0, (int)lpNumberOfBytesRead).Split('\0')[0];

                                            Win32API.ReadProcessMemory(mHProcess, SWRS_ADDR.RCHARID, lpBuffer, 4, out lpNumberOfBytesRead);
                                            e.RightPlayerCharacter = BitConverter.ToInt32(lpBuffer, 0);
                                            e.RightPlayerCharacterName = SWRSCHAR.GetCharacterName(e.RightPlayerCharacter);

                                            Win32API.ReadProcessMemory(mHProcess, pBattleMgr + SWRS_ADDR.RCHAROFS, lpBuffer, 4, out lpNumberOfBytesRead);
                                            int pRChar = BitConverter.ToInt32(lpBuffer, 0);
                                            Win32API.ReadProcessMemory(mHProcess, pRChar + SWRS_ADDR.WINCNTOFS, lpBuffer, 4, out lpNumberOfBytesRead);
                                            e.RightPlayerScore = lpBuffer[0];

                                            e.IsHost = communicationMode == SWRSCOMMMODE.SERVER ? true : false;
                                            e.Skip = 0;
                                            e.Version = mVersion;

                                            OnBattleEnded(this, e);

                                        }
                                        break;
                                }

                                mLastBattleMode = battleMode;

                                break;
                            case SWRSCOMMMODE.WATCH:
                                break;
                            default:
                                break;
                        }

                        break;

                }

                Thread.Sleep(1000);
                continue;

            }

            Break:

            Win32API.CloseHandle(mHProcess);
            mHProcess = IntPtr.Zero;
            return;

        }

        public void Start()
        {

            if (!mIsWatching && mThread == null)
            {

                mIsWatching = true;

                mThread = new Thread(new ThreadStart(watch));

                mThread.Start();

            }

        }

        public void Stop()
        {

            mIsWatching = false;

            mThread = null;

        }

    }
}
