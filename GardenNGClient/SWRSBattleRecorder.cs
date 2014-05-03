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

            mDatabase = new SQLiteConnection(String.Format("Data Source={0};", mPath));

            mDatabase.Open();

            mOnBattleEndedEventHandler = delegate(Object sender, SWRSMonitor.BattleEndedEventArgs e)
            {

                Console.WriteLine("Battle recording.");
                Add(e.Time, e.LeftPlayerProfile, e.LeftPlayerCharacter, e.LeftPlayerScore, e.RightPlayerProfile, e.RightPlayerCharacter, e.RightPlayerScore, e.IsHost, e.Skip, e.Version);

            };

            using (SQLiteTransaction transaction = mDatabase.BeginTransaction())
            {

                using (SQLiteCommand sql = new SQLiteCommand(mDatabase))
                {

                    sql.CommandText = @"CREATE TABLE IF NOT EXISTS `battles` (
                      `id` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
                      `t` TEXT NOT NULL,
                      `lpp` TEXT NOT NULL,
                      `lpc` TEXT NOT NULL,
                      `lps` INTEGER NOT NULL,
                      `rpp` TEXT NOT NULL,
                      `rpc` TEXT NOT NULL,
                      `rps` INTEGER NOT NULL,
                      `ih` BOOLEAN NOT NULL,
                      `s` INTEGER NOT NULL,
                      `v` TEXT NOT NULL
                    );";

                    sql.ExecuteNonQuery();

                }

                transaction.Commit();

            }

        }

        public void Start()
        {

            SWRSMonitor.GetInstance().OnBattleEnded += mOnBattleEndedEventHandler;

        }

        public void Add(DateTime t, String lpp, int lpc, int lps, String rpp, int rpc, int rps, bool ih, int s, String v)
        {

            using (SQLiteTransaction transaction = mDatabase.BeginTransaction())
            {

                using(SQLiteCommand sql = new SQLiteCommand(@"INSERT INTO `battles` (`t`, `lpp`, `lpc`, `lps`, `rpp`, `rpc`, `rps`, `ih`, `s`, `v`) VALUES (@t, @lpp, @lpc, @lps, @rpp, @rpc, @rps, @ih, @s, @v);", mDatabase)) {
                    sql.Parameters.Add(new SQLiteParameter("t", t.ToFileTimeUtc()));
                    sql.Parameters.Add(new SQLiteParameter("lpp", lpp));
                    sql.Parameters.Add(new SQLiteParameter("lpc", lpc));
                    sql.Parameters.Add(new SQLiteParameter("lps", lps));
                    sql.Parameters.Add(new SQLiteParameter("rpp", rpp));
                    sql.Parameters.Add(new SQLiteParameter("rpc", rpc));
                    sql.Parameters.Add(new SQLiteParameter("rps", rps));
                    sql.Parameters.Add(new SQLiteParameter("ih", ih));
                    sql.Parameters.Add(new SQLiteParameter("s", s));
                    sql.Parameters.Add(new SQLiteParameter("v", v));
                    sql.ExecuteNonQuery();
                }

                transaction.Commit();

            }

        }

        public void MergeToTSK(String tskDBPath)
        {

            SQLiteConnection tskDatabase = new SQLiteConnection(String.Format("Data Source={0};", tskDBPath));

            tskDatabase.Open();

            using (SQLiteCommand sql = new SQLiteCommand(@"SELECT * FROM `battles`", mDatabase))
            {

                SQLiteDataReader reader = sql.ExecuteReader();

                using (SQLiteTransaction transaction = tskDatabase.BeginTransaction())
                {

                    using (SQLiteCommand _sql = new SQLiteCommand(@"INSERT INTO `trackrecord123` (`timestamp`, `p1name`, `p1id`, `p1win`, `p2name`, `p2id`, `p2win`) VALUES (@timestamp, @p1name, @p1id, @p1win, @p2name, @p2id, @p2win);", tskDatabase))
                    {

                        while (reader.Read())
                        {

                            _sql.Parameters.Add(new SQLiteParameter("timestamp", Int64.Parse((String)reader["t"]) / 10000 * 10000));

                            if ((bool)reader["ih"])
                            {

                                _sql.Parameters.Add(new SQLiteParameter("p1name", (String)reader["lpp"]));
                                _sql.Parameters.Add(new SQLiteParameter("p1id", Int32.Parse((String)reader["lpc"])));
                                _sql.Parameters.Add(new SQLiteParameter("p1win", reader["lps"]));
                                _sql.Parameters.Add(new SQLiteParameter("p2name", (String)reader["rpp"]));
                                _sql.Parameters.Add(new SQLiteParameter("p2id", Int32.Parse((String)reader["rpc"])));
                                _sql.Parameters.Add(new SQLiteParameter("p2win", reader["rps"]));

                            }
                            else
                            {

                                _sql.Parameters.Add(new SQLiteParameter("p1name", (String)reader["rpp"]));
                                _sql.Parameters.Add(new SQLiteParameter("p1id", Int32.Parse((String)reader["rpc"])));
                                _sql.Parameters.Add(new SQLiteParameter("p1win", reader["rps"]));
                                _sql.Parameters.Add(new SQLiteParameter("p2name", (String)reader["lpp"]));
                                _sql.Parameters.Add(new SQLiteParameter("p2id", Int32.Parse((String)reader["lpc"])));
                                _sql.Parameters.Add(new SQLiteParameter("p2win", reader["lps"]));

                            }

                            try
                            {
                                _sql.ExecuteNonQuery();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }

                        }

                    }

                    transaction.Commit();

                }

            }

            tskDatabase.Close();

        }

        public DataTable GetDataTable()
        {

            DataTable dataTable = new DataTable();

            

            SQLiteDataAdapter adapter = new SQLiteDataAdapter(@"SELECT * FROM `battles`;", mDatabase);

            adapter.Fill(dataTable);

            dataTable.Columns["t"].ColumnName = "时间";
            dataTable.Columns["lpp"].ColumnName = "左边玩家昵称";
            dataTable.Columns["lpc"].ColumnName = "左边玩家角色";
            dataTable.Columns["lps"].ColumnName = "左边玩家分数";
            dataTable.Columns["rpp"].ColumnName = "右边玩家昵称";
            dataTable.Columns["rpc"].ColumnName = "右边玩家角色";
            dataTable.Columns["rps"].ColumnName = "右边玩家分数";
            dataTable.Columns["ih"].ColumnName = "主机";
            dataTable.Columns["s"].ColumnName = "跳帧";
            dataTable.Columns["v"].ColumnName = "版本";

            foreach (DataRow row in dataTable.Rows)
            {
                row[1] = DateTime.FromFileTimeUtc(Int64.Parse((String) row[1])).ToLocalTime().ToString("yyyyMMdd HHmmss");
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
