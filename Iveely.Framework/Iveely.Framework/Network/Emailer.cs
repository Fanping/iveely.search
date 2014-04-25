using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Framework.Network
{
    public class Emailer
    {
        /// <summary>
        /// 邮件发送者
        /// </summary>
        private readonly string _emailAcount;

        /// <summary>
        /// 邮件显示的发送者
        /// </summary>
        private readonly string _senderName;

        /// <summary>
        /// 邮箱密码
        /// </summary>
        private readonly string _emailPassword;

        /// <summary>
        /// 邮件主机
        /// </summary>
        private readonly string _emailHost;

        /// <summary>
        /// 邮箱主机端口
        /// </summary>
        private readonly int _emailPort;

        /// <summary>
        /// 是否使用安全套接
        /// </summary>
        private readonly bool _userSsl;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailAcount">邮件的账户</param>
        /// <param name="senderName">邮件中显示的名字</param>
        /// <param name="password">密码</param>
        /// <param name="emailHost">邮件服务器</param>
        /// <param name="emailPort">邮件端口</param>
        public Emailer(string emailAcount, string senderName, string password, string emailHost, int emailPort, bool userSsl = true)
        {
            _emailAcount = emailAcount;
            _senderName = senderName;
            _emailPassword = password;
            _emailHost = emailHost;
            _emailPort = emailPort;
            _userSsl = userSsl;
        }

        /// <summary>
        /// 发送EMAIL
        /// </summary>
        /// <param name="recivers"></param>
        /// <param name="subject">主题</param>
        /// <param name="message">内容</param>
        /// <returns>发送是否成功</returns>
        public bool SendMail(string[] recivers, string subject, string message)
        {

            // 初始化邮件对象
            MailMessage emailMessage = new MailMessage();
            MailAddress from = new MailAddress(_emailAcount, _senderName);
            emailMessage.From = from;
            foreach (var reciver in recivers)
            {
                MailAddress to = new MailAddress(reciver);
                emailMessage.To.Add(to);
            }

            emailMessage.Subject = subject;
            emailMessage.Body = message;
            emailMessage.IsBodyHtml = true;
            emailMessage.SubjectEncoding = System.Text.Encoding.Default;
            emailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            emailMessage.Headers.Add("X-Priority", "3");
            emailMessage.Headers.Add("X-MSMail-Priority", "Normal");
            emailMessage.Headers.Add("X-Mailer", "Microsoft Outlook Express 6.00.2900.2869");
            emailMessage.Headers.Add("X-MimeOLE", "Produced By Microsoft MimeOLE V6.00.2900.2869");

            //邮件发送客户端
            SmtpClient client = new SmtpClient { Host = _emailHost };
            client.Port = _emailPort;
            client.EnableSsl = _userSsl;

            System.Net.NetworkCredential credential = new System.Net.NetworkCredential
            {
                UserName = _emailAcount,
                Password = _emailPassword
            };

            client.Credentials = credential;

            try
            {
                client.Send(emailMessage);
            }
            catch (Exception e)
            {
                throw e;
            }
            return true;

        }
    }
}
