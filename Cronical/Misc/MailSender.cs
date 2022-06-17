using System.Net;
using System.Net.Mail;

namespace Cronical.Misc;

/// <summary>
/// Interface that allows us to use different MailSenders. Good for testing purposes.
/// </summary>
public interface IMailSender
{
    void Send(MailMessage message, string host, bool ssl = false, NetworkCredential credentials = null);
}

/// <summary>
/// MailSender very thinly wraps the SmtpClient and allows us to easily send an email.
/// </summary>
public class MailSender : IMailSender
{
    public void Send(MailMessage message, string host, bool ssl = false, NetworkCredential credentials = null)
    {
        var smtp = new SmtpClient
        {
            EnableSsl = ssl,
            Host = host,
            Credentials = credentials
        };

        smtp.Send(message);
    }
}