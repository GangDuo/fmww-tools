using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Uplink.Models
{
    internal sealed class UserData
    {
        private static volatile UserData instance;
        private static object syncRoot = new Object();

        private UserData() { }

        public SourceType SourceType { get; set; }
        public string Source { get; private set; }

        public static UserData Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new UserData();
                            instance.Init();
                        }
                    }
                }
                return instance;
            }
        }

        private void Init()
        {
            var args = new Dictionary<string, bool>();
            Source = String.Empty;

            #region コマンドラインオプションを設定する
            var options = new Mono.Options.OptionSet() {
                    {"i|inventory", "棚卸インポート", v => args["inventory"] = v != null},
                    {"c|create-catalog", "商品マスタ新規作成", v => args["inventory"] = v != null},
                    {"e|edit-catalog", "商品マスタ修正", v => args["inventory"] = v != null},
                    {"p|pop", "Cポップインポート", v => args["pop"] = v != null},
                    //{"|", "出荷確定", v => args["inventory"] = v != null},
                    //{"|", "仕入検品", v => args["inventory"] = v != null},
                    //{"|", "棚割マスタ", v => args["inventory"] = v != null},
                    {"s|source=", "データソースファイルパス", v => Source = v},
                };
            #endregion

            #region parse command line.
            List<string> extra;
            try
            {
                extra = options.Parse(Environment.GetCommandLineArgs());
                extra.ForEach(t => Debug.WriteLine(t));
                Validate(args);
            }
            catch (Mono.Options.OptionException e)
            {
                Console.Error.WriteLine(e.Message);
                Console.WriteLine("Try `CommandLineOption --help' for more information.");
                return;
            }
            #endregion

            SourceType = ToEnum<SourceType>(args.Keys.First());
        }

        private void Validate(Dictionary<string, bool> args)
        {
            if (1 != args.Values.Where(x => x).Count())
            {
                throw new ArgumentException("コマンドライン引数[i, p]は必ず1個指定しなければならない（0、2個以上もダメ）");
            }
            if (String.IsNullOrEmpty(Source))
            {
                throw new ArgumentException("コマンドライン引数 sourceは必ず指定しなければならない");
            }
        }

        private static T ToEnum<T>(string ss)
        {
            return (T)Enum.Parse(typeof(T),
                // 文字列の先頭の文字を大文字
                CultureInfo.CurrentCulture.TextInfo.ToTitleCase(ss));
        }
    }

    internal enum SourceType
    {
        Inventory,
        Pop,
    }
}
