using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace bookShop.Web.Controllers
{
    public class MailController : Controller
    {
        [HttpPost]
        public IActionResult SendMail(string email)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("mclskn.bookshop@gmail.com", "mclskn123456");
            client.DeliveryMethod = SmtpDeliveryMethod.Network;

            MailMessage msj = new MailMessage();
            msj.From = new MailAddress("mclskn.bookshop@gmail.com", "mclskn123456");
            msj.To.Add(email); 
            msj.Subject = "BookShop Bülten";
            msj.Body = "Kayıt olduğunuz için teşekkür ederiz...";
            client.Send(msj);
            return View();
        }
    }
}
