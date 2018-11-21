using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Cronical.Misc;

namespace Cronical.Test
{
    public class MockMailSender : IMailSender
    {
        public List<SentEmail> SentEmails = new List<SentEmail>();

        public class SentEmail
        {
            public string Host { get; set; }
            public bool Ssl { get; set; }
            public NetworkCredential Credentials { get; set; }
            public MailMessage Message { get; set; }
        }

        public void Send(MailMessage message, string host, bool ssl = false, NetworkCredential credentials = null)
        {
            SentEmails.Add(new SentEmail { Message = message, Host = host, Ssl = ssl, Credentials = credentials });
        }
    }
}
