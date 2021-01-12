using Microsoft.VisualBasic.FileIO;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using T = Text;

namespace Insert
{
    class Program
    {
        static void Main(string[] args)
        {
            if (0 == args.Length)
            {
                Console.WriteLine("テーブル名が必要");
            }
            var tableName = args[0];
            var csvFilePath = args[1];

            DataColumn[] columns = null;
            Dictionary<string, int> colIndexes = new Dictionary<string, int>();
            using (var tfp = new TextFieldParser(csvFilePath) { Delimiters = new string[]{","} })
            {
                var fields = tfp.ReadFields();
                columns = new DataColumn[fields.Length];
                for (int i = 0; i < fields.Length; i++)
                {
                    Debug.WriteLine(fields[i]);
                    var columnName = "f" + i;
                    columns[i] = new DataColumn(columnName) { Caption = fields[i] };
                    colIndexes.Add(columnName, i);
                }
            }

            using (FileStream fs = File.OpenRead(csvFilePath))
            {
                var table = T.Csv.Convert(fs, "", columns, null, colIndexes);
                Debug.WriteLine(table.Rows.Count);
            }

            using (var conn = new MySqlConnection(ConnectionString))
            {
                try
                {
                    Console.WriteLine("Connecting to MySQL...");
                    conn.Open();
                    // Perform database operations
                    string sql = String.Format("SELECT * FROM `{0}`;", tableName);
                    var adapter = new MySqlDataAdapter(sql, conn);
                    MySqlCommandBuilder cb = new MySqlCommandBuilder(adapter);

                    var data = new DataSet();
                    adapter.Fill(data);
                    Console.WriteLine(data.Tables[0].Rows.Count);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        private static readonly string ConnectionString = new MySqlConnectionStringBuilder()
        {
            Server = ConfigurationManager.AppSettings["db:server"],
            Port = uint.Parse(ConfigurationManager.AppSettings["db:port"]),
            UserID = ConfigurationManager.AppSettings["db:user"],
            Password = ConfigurationManager.AppSettings["db:password"],
            Database = ConfigurationManager.AppSettings["db:database"],
            ConvertZeroDateTime = true,
        }.ToString();
    }
}
