using System;
using System.Net.Mail;
using System.Linq;
using System.Collections.Generic;

namespace UpdateDistributes
{
    class Program
    {
        public static void SendEmail(IEnumerable<string> to, string subject, string body, IEnumerable<string> attachments = null)
        {
            using (var message = new System.Net.Mail.MailMessage())
            using (var client = new System.Net.Mail.SmtpClient())
            {
                foreach (string v in to)
                {
                    message.To.Add(v);
                }
                message.Subject = subject;
                message.Body = body;
                if (null != attachments)
                {
                    foreach (var fileName in attachments.Distinct())
                    {
                        message.Attachments.Add(new Attachment(fileName));
                    }
                }

                try
                {
                    client.Send(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception caught in sendJobFinishedReport(): {0}",
                          ex.ToString());
                }
            }
        }

        static void Main(string[] args)
        {
            bool canTerminate = false;
            Object lockThis = new Object();
            var fromDate = DateTime.Today.AddDays(-14);

            var page = new FMWW.ScheduledArrival.DistributeExport.Ref.Page()
            {
                UserAccount = UserAccount,
                PageContext = new FMWW.ScheduledArrival.DistributeExport.Ref.Context()
                {
                    ShopScheDate = new FMWW.Component.Between<Nullable<DateTime>>() { From = fromDate },
                }
            };
            Action<string> onCsvDownloadCompleted = null;
            onCsvDownloadCompleted = (csv) =>
            {
                page.CsvDownloadCompleted -= onCsvDownloadCompleted;
                // csv -> table
                var table = FMWW.Entity.DistributeForExport.Convert(csv);
                Domain.DataBase.DeleteAndInsert(table);
                SendEmail(
                    Xml.Settings.Destination.Load("Settings.xml"),
                    "投入表マスタ更新完了通知",
                    fromDate.ToString("yyyy/MM/dd") + " ～");
                
                lock (lockThis)
                {
                    canTerminate = true;
                }
            };
            page.CsvDownloadCompleted += onCsvDownloadCompleted;
            page.CsvAsync();

            var animation = new List<char> { '／', '―', '＼', '｜' };
            var counter = 0;
            // ワーカースレッドが終わるまで待つ
            while (!canTerminate)
            {
                Console.Write("\r");
                Console.Write(animation[counter++] + "\b");
                if (counter == animation.Count)
                    counter = 0;
                System.Threading.Thread.Sleep(100);
            }
        }
        private static readonly FMWW.Entity.UserAccount UserAccount = FMWW.Entity.Factory.UserAccount.Load(".user.json");
    }
}
