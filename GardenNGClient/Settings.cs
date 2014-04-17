using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace GardenNGClient
{
    public class Settings
    {

        public class JSONStore {

            public String ServerHost = "114.215.177.153";
            //public String ServerHost = "127.0.0.1";
            public int ServerPort = 8888;
            public String Nickname = "unknown";
            public String GamePath = "";

        }

        static Settings mInstance;

        public static Settings GetInstance()
        {

            if (mInstance == null)
            {
                mInstance = new Settings();
            }

            return mInstance;

        }

        public JSONStore Store
        {
            get;
            set;
        }

        String mPath;

        private Settings()
        {

            mPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)), "settings.json");

            Load();

        }

        public void Load()
        {

            Console.WriteLine("Loading settings: {0}.", mPath);

            if (File.Exists(mPath))
            {
                Store = SimpleJson.SimpleJson.DeserializeObject<JSONStore>(File.ReadAllText(mPath));
            }
            else {
                Store = new JSONStore();
            }

        }

        public void Save()
        {

            File.WriteAllText(mPath, SimpleJson.SimpleJson.SerializeObject(Store));

        }

    }
}
