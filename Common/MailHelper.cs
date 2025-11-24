using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace old_phone.Common
{
    public class MailHelper
    {
        public static void SendEmail(string toEmail, string subject, string body)
        {
            var fromEmail = "23050118@student.bdu.edu.vn";
            var fromPassword = "zolo xihn puej ikam"; 

            var message = new MailMessage();
            message.From = new MailAddress(fromEmail,"Old Phone Support");
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, fromPassword),
                EnableSsl = true, // Bắt buộc bật SSL với Gmail
            };
            try
            {
                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                // Xử lý lỗi gửi email (nếu cần)
                throw new Exception("Lỗi khi gửi email: " + ex.Message);
            }
        }
    }
}