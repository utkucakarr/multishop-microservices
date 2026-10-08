using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using MultiShop.BuildingBlocks.Authentication;
using MultiShop.WebUI.Models;
using MultiShop.WebUI.Settings;

namespace MultiShop.WebUI.Controllers
{
    [Authorize(Roles = MultiShopRoles.Admin)]
    public class MailController : Controller
    {
        private readonly SmtpSettings _smtpSettings;

        public MailController(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        [HttpGet]
        public IActionResult SendMail()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendMail(MailRequest mailRequest)
        {
            MimeMessage mimeMessage = new MimeMessage();

            //burada mesajın kimden gönderildiği
            MailboxAddress mailboxAddressFrom = new MailboxAddress(_smtpSettings.SenderName, _smtpSettings.SenderEmail);
            mimeMessage.From.Add(mailboxAddressFrom);

            //Burada mesajın kime gönderildiği
            MailboxAddress mailboxAdressTo = new MailboxAddress("User", mailRequest.RecieverMail);
            mimeMessage.To.Add(mailboxAdressTo);

            //Burada mesajın içeriği
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = mailRequest.MessageContent;
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            //burada mesajın konusu alındı
            mimeMessage.Subject = mailRequest.Subject;

            //SMTP protokolü kullanarak mail atmaya izin verme
            SmtpClient client = new SmtpClient();
            client.Connect(_smtpSettings.Host, _smtpSettings.Port, false);
            client.Authenticate(_smtpSettings.UserName, _smtpSettings.Password);
            client.Send(mimeMessage);
            client.Disconnect(true);
            return View();
        }
    }
}
