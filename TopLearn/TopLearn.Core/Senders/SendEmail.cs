using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace TopLearn.Core.Senders
{
    public class SendEmail
    {
        //public static void Send(string To, string subject, string body)
        //{
        //    MailMessage email = new MailMessage();
        //    email.From = new MailAddress("yoziraniv@gmail.com", "تاپ لرن");
        //    email.To.Add(To);
        //    email.Subject = subject;
        //    email.Body = body;
        //    email.IsBodyHtml = true;
        //    email.Priority = MailPriority.High;
        //    SmtpClient sc = new SmtpClient("smtp.gmail.com");
        //    sc.UseDefaultCredentials = false;
        //    sc.EnableSsl = true;
        //    sc.Port = 25;
        //    sc.Credentials = new NetworkCredential("yoziraniv@gmail.com", "342717905Wu&");
        //    sc.Send(email);
        //}
        //public static void Send(string to, string subject, string body)
        //{
        //    MailMessage mail = new MailMessage();
        //    SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
        //    mail.From = new MailAddress("yoziraniv@gmail.com", "تاپ لرن");
        //    mail.To.Add(to);
        //    mail.Subject = subject;
        //    mail.Body = body;
        //    mail.IsBodyHtml = true;
        //    SmtpServer.UseDefaultCredentials = false;
        //    SmtpServer.Port = 25;
        //    SmtpServer.Credentials = new System.Net.NetworkCredential("yoziraniv@gmail.com", "342717905Wu&");
        //    SmtpServer.EnableSsl = true;

        //    SmtpServer.Send(mail);

        //}
        public static void Send(string to, string subject, string body)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("freeeducation.ir");
            mail.From = new MailAddress("testTopLearn84@freeeducation.ir", "تاپ لرن");
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential("testTopLearn84@freeeducation.ir", "342717905Wu&");
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);

        }
    }
}