using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Ctrls = Controls;

namespace DwnSalesJournal
{
    /**
     * usage : DwnSalesJournal -from -to -o filename
     * DwnSalesJournal -f 2015/1/1 -t 2015/1/10 -o out.xlsx
     * DwnSalesJournal -f 2015/1/1 -t 2015/1/10 -o out.xlsx --store-code 001
     * DwnSalesJournal -f 2015/1/1 -t 2015/1/10 -o out.csv --as-csv
     * 
     * [required]
     * -f : 売上期間　自
     * -t : 売上期間　至
     * -o : 保存先（絶対パス指定）
     * 
     * [Optional]
     * --store-code : 店舗コード
     * --as-csv : csvファイルとして出力する
     * -g : 処理中のダイアログを表示する
     */
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Console.CancelKeyPress += new ConsoleCancelEventHandler(Ctrl_C_Pressed);

            var between = new FMWW.Component.Between<Nullable<DateTime>>();
            var storeCode = "";
            var asCsv = false;
            var hasGui = false;
            string o = "";
            var args = Environment.GetCommandLineArgs();
            #region コマンドラインオプションを設定する
            var p = new Mono.Options.OptionSet() {
                //オプションとオプションの説明、そのオプションの引数に対するアクションを定義する
                {"c|store-code=", "店舗コード", v => storeCode = v},
                {"a|as-csv", "結果をCSV形式で受取る", v => asCsv = v != null},
                {"g|has-gui", "プログレスバーを表示する", v => hasGui = v != null},
                {"o|output-file=", "エクセルの保存ファイルパス（拡張子:xlsxを含める）", v => o = v},
                {"f|from=", "売上日", v => between.From = DateTime.Parse(v)},
                {"t|to=", "売上日", v => between.To = DateTime.Parse(v)},
            };
            #endregion
            #region parse command line.
            List<string> extra;
            //OptionSetクラスのParseメソッドでOptionSetの内容に合わせて引数をパースする。
            //Paseメソッドはパース仕切れなかったコマンドラインのオプションの内容をstring型のリストで返す。
            try
            {
                extra = p.Parse(args);
                extra.ForEach(t => Debug.WriteLine(t));
            }
            //パースに失敗した場合OptionExceptionを発生させる
            catch (Mono.Options.OptionException e)
            {
                Console.Error.WriteLine(e.Message);
                Console.WriteLine("Try `CommandLineOption --help' for more information.");
                return;
            }
            #endregion
            var context = new FMWW.ForShop.Work.Journals.Ref.Context()
            {
                PeriodOfSales = between,
                StoreCode = storeCode,
                AsCsv = asCsv
            };
            Task.Factory.StartNew(() =>
            {
                var task = FMWW.ForShop.Work.Journals.Ref.Downloader.DownloadTask(context);
                task.Wait();
                return task.Result;
            })
            .ContinueWith((task) =>
            {
                var filepath = task.Result;
                try
                {
                    Console.WriteLine(filepath);
                    if (asCsv)
                    {
                        Util.FileSystem.MoveFile(MsOffice.MsExcel.SaveAsCsv(filepath, false), o);
                    }
                    else
                    {
                        Util.FileSystem.MoveFile(MsOffice.MsExcel.SaveAs(filepath), o);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
//                    System.IO.File.Delete(filepath);
                    Application.Exit();
                }
            }/*, TaskScheduler.FromCurrentSynchronizationContext()*/);

            if (hasGui)
            {
                Application.Run(new Form1());
            }
            else
            {
                Application.Run();
            }
        }

        // ［Ctrl］＋［C］キーが押されたときに呼び出される
        private static void Ctrl_C_Pressed(object sender, ConsoleCancelEventArgs e)
        {
            //logger.Save(); // ログの保存
        }
    }
}
