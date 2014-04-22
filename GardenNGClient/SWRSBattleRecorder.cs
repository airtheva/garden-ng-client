using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Data;

namespace GardenNGClient
{
    public class SWRSBattleRecorder
    {

        static SWRSBattleRecorder mInstance;

        public static SWRSBattleRecorder GetInstance()
        {

            if (mInstance == null)
            {
                mInstance = new SWRSBattleRecorder();
            }

            return mInstance;

        }

        String mPath;
        SQLiteConnection mDatabase;
        SWRSMonitor.BattleEndedEventHandler mOnBattleEndedEventHandler;

        private SWRSBattleRecorder()
        {

            mPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)), "database.db");

            mDatabase = new SQLiteConnection(String.Format("Data Source={0}", mPath));

            mDatabase.Open();

            mOnBattleEndedEventHandler = delegate(Object sender, SWRSMonitor.BattleEndedEventArgs e)
            {

                Console.WriteLine("Battle recording.");
                Add(e.Time, e.LeftPlayerProfile, e.LeftPlayerCharacter, e.LeftPlayerScore, e.RightPlayerProfile, e.RightPlayerCharacter, e.RightPlayerScore);

            };

            SQLiteCommand sql = new SQLiteCommand(mDatabase);

            sql.CommandText = @"CREATE TABLE IF NOT EXISTS `battles` (
              `id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
              `t` TEXT NOT NULL,
              `lpp` TEXT NOT NULL,
              `lpc` TEXT NOT NULL,
              `lps` INTEGER NOT NULL,
              `rpp` TEXT NOT NULL,
              `rpc` TEXT NOT NULL,
              `rps` INTEGER NOT NULL
            );";

            sql.ExecuteNonQuery();

        }

        public void Start()
        {

            SWRSMonitor.GetInstance().OnBattleEnded += mOnBattleEndedEventHandler;

        }

        public void Add(DateTime t, String lpp, int lpc, int lps, String rpp, int rpc, int rps)
        {

            SQLiteCommand sql = new SQLiteCommand(mDatabase);
            sql.CommandText = @"INSERT INTO `battles` (`t`, `lpp`, `lpc`, `lps`, `rpp`, `rpc`, `rps`) VALUES (@t, @lpp, @lpc, @lps, @rpp, @rpc, @rps);";
            sql.Parameters.Add(new SQLiteParameter("t", t.ToBinary()));
            sql.Parameters.Add(new SQLiteParameter("lpp", lpp));
            sql.Parameters.Add(new SQLiteParameter("lpc", lpc));
            sql.Parameters.Add(new SQLiteParameter("lps", lps));
            sql.Parameters.Add(new SQLiteParameter("rpp", rpp));
            sql.Parameters.Add(new SQLiteParameter("rpc", rpc));
            sql.Parameters.Add(new SQLiteParameter("rps", rps));
            sql.ExecuteNonQuery();

            Console.WriteLine(sql.ToString());

        }

        public DataTable GetDataTable()
        {

            DataTable dataTable = new DataTable();

            

            SQLiteDataAdapter adapter = new SQLiteDataAdapter(@"SELECT * FROM `battles`;", mDatabase);

            adapter.Fill(dataTable);

            dataTable.Columns[0].ColumnName = "序号";
            dataTable.Columns[1].ColumnName = "时间";
            dataTable.Columns[2].ColumnName = "左边玩家昵称";
            dataTable.Columns[3].ColumnName = "左边玩家角色";
            dataTable.Columns[4].ColumnName = "左边玩家分数";
            dataTable.Columns[5].ColumnName = "右边玩家昵称";
            dataTable.Columns[6].ColumnName = "右边玩家角色";
            dataTable.Columns[7].ColumnName = "右边玩家分数";

            foreach (DataRow row in dataTable.Rows)
            {
                row[1] = DateTime.FromBinary(Int64.Parse((String) row[1])).ToLocalTime().ToString("yyyyMMdd HHmmss");
                row[3] = SWRSMonitor.SWRSCHAR.GetCharacterName(Int32.Parse((String)row[3]));
                row[6] = SWRSMonitor.SWRSCHAR.GetCharacterName(Int32.Parse((String)row[6]));
            }

            return dataTable;

        }

        public void Stop()
        {

            SWRSMonitor.GetInstance().OnBattleEnded -= mOnBattleEndedEventHandler;

        }

        ~SWRSBattleRecorder()
        {
            //mDatabase.Close();
        }

    }
}
