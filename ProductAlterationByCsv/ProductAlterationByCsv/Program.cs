using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProductAlterationByCsv
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("csvファイルを指定してください");
                return;
            }
            var csvFileShiftJIS = args[0];
            if (!System.IO.File.Exists(csvFileShiftJIS))
            {
                Console.Error.WriteLine("csvファイルが見つからない");
                return;
            }
            var task = FMWW.ExternalInterface.Products.Alteration.Uploader.UploadTask(csvFileShiftJIS);
            task.Wait();
            Console.Out.WriteLine(task.Result);
        }
    }
}
