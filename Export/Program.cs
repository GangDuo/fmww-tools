using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Export
{
    class Program
    {
        private static readonly FMWW.Entity.UserAccount UserAccount = FMWW.Entity.Factory.UserAccount.Load(".user.json");

        // cmd Distributes
        static void Main(string[] args)
        {
            var page = new FMWW.ScheduledArrival.DistributeExport.Ref.Page()
            {
                UserAccount = UserAccount,
                PageContext = new FMWW.ScheduledArrival.DistributeExport.Ref.Context()
            {
                //Code = "9374",
                ShopScheDate = new FMWW.Component.Between<DateTime?>() { From = DateTime.Today.AddDays(-7), To = DateTime.Today },
                //CreationDate = new Core.Between<DateTime>() { From = DateTime.Today }
            }
            };
            var csv = page.Csv();
            Debug.WriteLine(csv);

            return;
        }
    }
}
