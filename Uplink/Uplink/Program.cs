using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Uplink
{
    class Program
    {
        static void Main(string[] args)
        {
            FMWW.Http.AbstractUploader uploader = new FMWW.Http.NopUploader();
            switch (Models.UserData.Instance.SourceType)
            {
                case Models.SourceType.Inventory:
                    uploader = new FMWW.ExternalInterface.Inventory.New.Page();
                    break;
                case Models.SourceType.Pop:
                    uploader = new FMWW.Master.PriceTag.New.Page();
                    break;
                default:
                    break;
            }
            uploader.UserAccount = FMWW.Entity.Factory.UserAccount.Load(".user.json");
            uploader.PathShiftJis = Models.UserData.Instance.Source;
            if (uploader.CanExecute(Models.UserData.Instance.Source))
            {
                uploader.Execute(Models.UserData.Instance.Source);
                Console.WriteLine(String.Format("source = {0}\n{1}\n------------------------------------",
                    Models.UserData.Instance.Source, uploader.ResultMessage));
            }
        }
    }
}
