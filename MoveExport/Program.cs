using System;
using System.Linq;
using System.Collections.Generic;

namespace MoveExport
{
    class Program
    {
        /**
         * usage : MoveExport [from] [to] [load] [unload] > out.txt
         * MoveExport 2015/1/1 2015/1/10 9998 1101
         * オプション：-cmd：
         */
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 3)
                {
                    throw new Exception("引数不足");
                }

                var dt = new List<DateTime>();
                new List<string>() { args[0], args[1] }
                    .ForEach(item =>
                    {
                        if (3 != item.Split('/').Count())
                        {
                            throw new Exception("日付の形式が正しくない");
                        }
                        var t = new List<string>(item.Split('/')).Select(x => int.Parse(x)).ToList();
                        dt.Add(new DateTime(t[0], t[1], t[2]));
                    });
                if (dt.First() > dt.Last())
                {
                    throw new Exception("入力された期間は存在しない");
                }
                var separator = new char[] { ' ' };
                var f = "yyyy年M月d日";
                string[] load = null;
                string[] unload = null;
                if (args.Length > 2)
                {
                    load = args[2].Split(separator);
                    if (args.Length > 3)
                    {
                        unload = args[3].Split(separator);
                    }
                }
                var csv = MoveExport(dt.First().ToString(f), dt.Last().ToString(f), load, unload);
                Console.WriteLine(csv);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static readonly FMWW.Entity.UserAccount UserAccount = FMWW.Entity.Factory.UserAccount.Load(".user.json");

        static string MoveExport(string from, string to, string[] load, string[] unload)
        {
            var context = new FMWW.Movement.Movement.MovementExport.Ref.Context()
            {
                MovementDate = new FMWW.Component.Between<System.Nullable<DateTime>>()
                {
                    From = new System.Nullable<DateTime>(DateTime.Parse(from)),
                    To = new System.Nullable<DateTime>(DateTime.Parse(to))
                }
            };
            if (load != null)
            {
                context.Load.AddRange(load);
            }
            if (unload != null)
            {
                context.Unload.AddRange(unload);
            }

            var page = new FMWW.Movement.Movement.MovementExport.Ref.Page()
            {
                UserAccount = UserAccount,
                PageContext = context
            };
            return page.Csv();
        }
    }
}
