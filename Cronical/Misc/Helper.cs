﻿using System.Collections;
using System.Net;
using System.Net.Mail;
using Cronical.Integrations;
using Serilog;

namespace Cronical.Misc;

public static class Helper
{
    /// <summary>
    /// Return the value of a BitArray as a long value. The BitArray cannot be longer
    /// than 64 bits.
    /// </summary>
    /// <param name="bitArray">The BitArray to extract</param>
    /// <returns>The ulong value contained in the BitArray.</returns>
    public static ulong Val(this BitArray bitArray)
    {
        if (bitArray.Length > 64)
            throw new ArgumentException("BitArray is longer than 64 bits.");

        var array = new byte[8];
        bitArray.CopyTo(array, 0);

        return BitConverter.ToUInt64(array, 0);
    }

    /// <summary>
    /// Send an email with a job result according to the given environment parameters. If enough
    /// parameters are missing (i.e. recipient, SMTP host, or text), no email is sent.
    /// </summary>
    /// <param name="title">Email title</param>
    /// <param name="text">Body of the email - usually a job result</param>
    /// <param name="env">The environment for this email</param>
    public static void SendMail(string title, string text, JobSettings env)
    {
        if (string.IsNullOrWhiteSpace(env.MailTo) || string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(env.SmtpHost))
            return;

        // Whatever exception happens in here, just catch it and log it
        try
        {
            Log.Debug("Sending results to " + env.MailTo);

            var msg = new MailMessage
            {
                Subject = title,
                Body = text,
                From = new MailAddress(string.IsNullOrWhiteSpace(env.MailFrom) ? env.MailTo : env.MailFrom)
            };

            foreach (var email in (env.MailTo ?? "").Split(',').Where(x => !string.IsNullOrWhiteSpace(x)))
                msg.To.Add(email.Trim());
            foreach (var email in (env.MailCc ?? "").Split(',').Where(x => !string.IsNullOrWhiteSpace(x)))
                msg.CC.Add(email);
            foreach (var email in (env.MailBcc ?? "").Split(',').Where(x => !string.IsNullOrWhiteSpace(x)))
                msg.Bcc.Add(email);

            if (env.SmtpSSL)
                Log.Debug("Using SSL connection");

            var credentials = string.IsNullOrEmpty(env.SmtpUser) && string.IsNullOrEmpty(env.SmtpPass)
                ? null
                : new NetworkCredential(env.SmtpUser, env.SmtpPass);


            Program.MailSender.Send(msg, env.SmtpHost, env.SmtpSSL, credentials);

            var emails = msg.To.Select(x => x.Address)
                .Concat(msg.CC.Select(x => x.Address))
                .Concat(msg.Bcc.Select(x => x.Address))
                .ToList();

            Log.Information("Mail sent to " + string.Join(", ", emails));
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }
    }
}