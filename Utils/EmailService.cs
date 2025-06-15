using System.Net;
using System.Net.Mail;

public static class EmailService
{
    public static void Send(string to, string subject, string body)
    {
        var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("youremail@gmail.com", "your_app_password"),
            EnableSsl = true
        };

        var message = new MailMessage("youremail@gmail.com", to, subject, body)
        {
            IsBodyHtml = true
        };

        smtp.Send(message);
    }
}
