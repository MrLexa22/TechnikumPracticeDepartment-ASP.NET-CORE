using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace TechnikumPracticeDepartment
{
    public class EmailService
    {
        private IConfiguration configuration;
        public EmailService(IConfiguration conf) 
        { 
            configuration = conf;
        }
        public async Task SendEmailRegistrationAsync(string email, string password, int? title)
        {
            try
            {
                string FilePath = BaseAddresse.Address + "TemplatesMessage/TemplateRegistration.html";
                WebClient clientss = new WebClient();
                string htmlCode = clientss.DownloadString(FilePath);

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("АИС Электронный студент - ПП", "no-reply@mrlexao.ru"));
                emailMessage.To.Add(new MailboxAddress("", email));
                if (title == 1)
                {
                    emailMessage.Subject = "Успешная регистрация на сайте";
                    htmlCode = htmlCode.Replace("{0}", "Ваш аккаунт успешно создан в АИС \"Электронный студент\" - Производственная практика");

                }
                if (title == 2)
                {
                    emailMessage.Subject = "Ваш пароль был сброшен";
                    htmlCode = htmlCode.Replace("{0}", "Ваш пароль был сброшен в АИС \"Электронный студент\" - Производственная практика");
                }


                htmlCode = htmlCode.Replace("{1}", email);
                htmlCode = htmlCode.Replace("{2}", password);
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = htmlCode
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.mail.ru", 465, true);
                    await client.AuthenticateAsync(configuration["TechnikumPracticeDepartment:UserName_SMPT"], configuration["TechnikumPracticeDepartment:Password_SMPT"]);
                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                }
            }
            catch { }
        }
        public async Task SendEmailRecovertPassword(int Code, string email)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("АИС Электронный студент - ПП", "no-reply@mrlexao.ru"));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = "Код для восстановления пароля";

                string FilePath = BaseAddresse.Address + "TemplatesMessage/recovetPassword.html";
                WebClient clientss = new WebClient();
                string htmlCode = clientss.DownloadString(FilePath);
                htmlCode = htmlCode.Replace("{0}", "Код для восстановления пароля");
                htmlCode = htmlCode.Replace("{1}", Code.ToString());
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = htmlCode
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.mail.ru", 465, true);
                    await client.AuthenticateAsync(configuration["TechnikumPracticeDepartment:UserName_SMPT"], configuration["TechnikumPracticeDepartment:Password_SMPT"]);
                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                }
            }
            catch { }
        }
        public async Task SendEmailDistribution(
            string email_student,
            string NameModule,
            string NamePractice,
            string datesAndDaysPractice,
            string Organization,
            string contactsOrganization,
            string addressOrganization)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("АИС Электронный студент - ПП", "no-reply@mrlexao.ru"));
                emailMessage.To.Add(new MailboxAddress("", email_student));

                emailMessage.Subject = "Обновление базы практики: "+NamePractice;

                string FilePath = BaseAddresse.Address + "TemplatesMessage/TemplateDistribution.html";
                WebClient clientss = new WebClient();
                string htmlCode = clientss.DownloadString(FilePath);
                htmlCode = htmlCode.Replace("{0}", "Обновление базы практики: "+NamePractice);
                htmlCode = htmlCode.Replace("{1}", "["+NameModule+"] "+NamePractice);
                htmlCode = htmlCode.Replace("{2}", datesAndDaysPractice);
                htmlCode = htmlCode.Replace("{3}", Organization);
                htmlCode = htmlCode.Replace("{4}", contactsOrganization);
                htmlCode = htmlCode.Replace("{5}", addressOrganization);
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = htmlCode
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.mail.ru", 465, true);
                    await client.AuthenticateAsync(configuration["TechnikumPracticeDepartment:UserName_SMPT"], configuration["TechnikumPracticeDepartment:Password_SMPT"]);
                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                }
            }
            catch { }
        }
        public async Task SendEmailWithStatusResponseFromStudent(
            string email_student,
            string NameVacancy,
            string NameOrganization,
            string CommentFromOrganization,
            string CommentFromStudent,
            int Status,
            DateTime ResponseCreate,
            int updateOrAdd)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("АИС Электронный студент - ПП", "no-reply@mrlexao.ru"));
                emailMessage.To.Add(new MailboxAddress("", email_student));
                if(updateOrAdd == 1)
                    emailMessage.Subject = "Обновление статуса отклика: " + NameVacancy;
                if(updateOrAdd == 2)
                    emailMessage.Subject = "Организация откликнулась на ваше резюме: " + NameVacancy;

                string FilePath = BaseAddresse.Address + "TemplatesMessage/TemplateStatusResponseFromStudent.html";
                WebClient clientss = new WebClient();
                string htmlCode = clientss.DownloadString(FilePath);
                if (updateOrAdd == 1)
                    htmlCode = htmlCode.Replace("{0}", "Обновление статуса отклика: " + NameVacancy);
                if (updateOrAdd == 2)
                    htmlCode = htmlCode.Replace("{0}", "Новый откликн: " + NameVacancy);
                htmlCode = htmlCode.Replace("{1}", NameVacancy);
                htmlCode = htmlCode.Replace("{2}", Status == 0 ? "создан" : (Status == 1 ? "на рассмотрении (ожидание ответа студента)" : (Status == 2 ? "на рассмотрении (получен ответ студента)" : (Status == 3 ? "принят (на рассмотрении образовательным учреждением)" : (Status == 4 ? "принято. Вы распределены в организацию" : (Status == 5 ? "отказ организации" : "отказ образовательным учреждением в распределении"))))));
                htmlCode = htmlCode.Replace("{3}", NameOrganization);
                htmlCode = htmlCode.Replace("{4}", CommentFromOrganization);
                htmlCode = htmlCode.Replace("{6}", CommentFromStudent?.ToString());
                htmlCode = htmlCode.Replace("{5}", ResponseCreate.ToShortDateString() + " " + ResponseCreate.ToShortTimeString());
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = htmlCode
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.mail.ru", 465, true);
                    await client.AuthenticateAsync(configuration["TechnikumPracticeDepartment:UserName_SMPT"], configuration["TechnikumPracticeDepartment:Password_SMPT"]);
                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                }
            }
            catch { }
        }
        public async Task SendEmailWithStatusRequest(
            string email_student,
            string status)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("АИС Электронный студент - ПП", "no-reply@mrlexao.ru"));
                emailMessage.To.Add(new MailboxAddress("", email_student));
                emailMessage.Subject = "Обновлён статус вашего запроса";

                string FilePath = BaseAddresse.Address + "TemplatesMessage/TemplateStatusRequestUpdate.html";
                WebClient clientss = new WebClient();
                string htmlCode = clientss.DownloadString(FilePath);
                htmlCode = htmlCode.Replace("{0}", "Обновёл статус вашего запроса на распределение в организацию по договору");
                htmlCode = htmlCode.Replace("{1}", status);
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = htmlCode
                };

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.mail.ru", 465, true);
                    await client.AuthenticateAsync(configuration["TechnikumPracticeDepartment:UserName_SMPT"], configuration["TechnikumPracticeDepartment:Password_SMPT"]);
                    await client.SendAsync(emailMessage);

                    await client.DisconnectAsync(true);
                }
            }
            catch { }
        }
    }
}
