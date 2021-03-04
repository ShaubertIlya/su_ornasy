using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Devir.DMS.BL.SMTP
{
    public static class SMTPHelper
    {
        public static void sendMessage(Devir.DMS.DL.Models.Document.DocumentNotifications.Notifications notify)
        {
            Task tsk = new Task(() =>
            {
            try
            {
                
                SmtpClient s = new SmtpClient("192.168.1.202");
                s.SendCompleted += s_SendCompleted;                
                s.UseDefaultCredentials = false;
                s.Credentials = new NetworkCredential("noreply@astanasu.kz", "Alma123");
                s.DeliveryMethod = SmtpDeliveryMethod.Network;
                MailMessage m = new MailMessage("noreply@Astanasu.kz", notify.ForWho.Email, "Уведомление", notify.Text + "</br>" + notify.LinkText);
                m.IsBodyHtml = true;
                s.Send(m);
                
                
            
            }catch(Exception ex){
            }
        });
            tsk.Start();
        }

        static void s_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            var s = e;
            if (e.Error != null)
            {
                var s1 = e;
            }
        }
    }
}
