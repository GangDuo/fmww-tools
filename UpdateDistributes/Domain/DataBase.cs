using System;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.Configuration;

namespace UpdateDistributes.Domain
{
    class DataBase
    {
        private static readonly string ServerAddress = ConfigurationManager.AppSettings["db:server"];
        private static readonly uint Port = uint.Parse(ConfigurationManager.AppSettings["db:port"]);
        private static readonly string UserID = ConfigurationManager.AppSettings["db:user"];
        private static readonly string Password = ConfigurationManager.AppSettings["db:password"];

        public static void DeleteAndInsert(DataTable table)
        {
            using (var con = new MySQL.Transcation(ServerAddress, UserID, Password, Port))
            {
                try
                {
                    // 登録済みすべての投入表番号を取得
                    var sql = @"SELECT DISTINCT `TDistributes`.`distributeNo`
                                          FROM `humpty_dumpty`.`TDistributes`;";
                    var distributes = con.GetTable(sql);
                    // 重複する投入表番号を取得
                    var duplicates = distributes.AsEnumerable().Select(r => r["distributeNo"]).Intersect(
                        table.AsEnumerable().Select(r => r["distributeNo"]).Distinct());
                    // 重複する投入表を削除
                    sql = String.Format(
                          @"DELETE FROM `humpty_dumpty`.`TDistributes`
                                          WHERE `TDistributes`.`distributeNo` IN ({0});", String.Join(",", duplicates));
                    con.SendQuery(sql);
                    // 投入表を登録
                    con.BulkSend("humpty_dumpty", "TDistributes", table);
                    con.Commit();
                }
                catch (Exception ex)
                {
                    con.Rollback();
                    Debug.WriteLine(ex.Message);
                    throw new Exception(ex.Message);
                }
            }
        }
    }
}
