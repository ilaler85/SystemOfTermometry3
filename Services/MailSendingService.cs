using OxyPlot;
using OxyPlot.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using SystemOfThermometry3.Model;
using SystemOfThermometry3.Services;


namespace SystemOfThermometry3.Services
{
    /// <summary>
    /// Сервис по отправки писем
    /// </summary>
    static class MailSendingService
    {
        public static bool canSendMail(SettingsService settingsService, SilosService silosService
            , out string errorMessage)
        {
            var smtpServer = settingsService.MailSmtpServer;
            var sender = settingsService.MailSender;
            var password = settingsService.MailSenderPassword;
            var caption = settingsService.MailCaption;
            var destinations = settingsService.getMailDestinations();
            if (smtpServer == "") { errorMessage = "Пустой SMTP сервер"; return false; }
            if (sender == "") { errorMessage = "Почта отправителя не указана."; return false; }
            if (password == "") { errorMessage = "Пароль для почты отправителя не указана."; return false; }
            if (destinations == null || destinations.Length == 0) { errorMessage = "Нет ни одного получателя."; return false; }

            errorMessage = "При отправке возникла ошибка: \n";
            var mail = new MailMessage();
            try
            {
                mail.From = new MailAddress(sender);
                for (var i = 0; i < destinations.Length; i++)
                {
                    mail.To.Add(new MailAddress(sender));
                }
                mail.Subject = caption;
                mail.Body = "Check Connection";

                var client = new SmtpClient();
                client.Host = smtpServer;
                client.Port = 587;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(sender.Split('@')[0], password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(mail);
                mail.Dispose();
                return true;

            }
            catch (Exception e)
            {
                mail.Dispose();
                MyLoger.Log("SendMail check connection error! " + e.Message);
                errorMessage += e.Message;
                return false;
            }
        }

        public static bool sendMail(SettingsService settingsService, SilosService silosService
            , out string errorMessage)
        {
            var smtpServer = settingsService.MailSmtpServer;
            var sender = settingsService.MailSender;
            var password = settingsService.MailSenderPassword;
            var caption = settingsService.MailCaption;
            var destinations = settingsService.getMailDestinations();

            if (smtpServer == "") { errorMessage = "Пустой SMTP сервер"; return false; }
            if (sender == "") { errorMessage = "Почта отправителя не указана."; return false; }
            if (password == "") { errorMessage = "Пароль для почты отправителя не указана."; return false; }
            if (destinations == null || destinations.Length == 0) { errorMessage = "Нет ни одного получателя."; return false; }

            errorMessage = "При отправке возникла ошибка: \n";
            var allSend = true;
            var mail = new MailMessage();
            try
            {
                mail.From = new MailAddress(sender);
                for (var i = 0; i < destinations.Length; i++)
                {
                    mail.To.Add(new MailAddress(destinations[i]));
                }
                mail.Subject = caption;
                mail.IsBodyHtml = true;

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settingsService.MailPlotToSendBasisPath);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                //Подгружаем графики
                if (settingsService.MailIsSendPlot)
                    buildPlotsAndAddToMessage(settingsService, silosService, mail);
                //Добавляем эксель документ
                var path_to_excel = Path.Combine(path, "export.xlsx");
                ExportService.exportToExcelLastTemperatures(path_to_excel, silosService, settingsService);
                mail.Attachments.Add(new Attachment(path_to_excel));

                var client = new SmtpClient();
                client.Host = smtpServer;
                client.Port = 587;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(sender.Split('@')[0], password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(mail);
                mail.Dispose();

            }
            catch (Exception e)
            {
                MyLoger.LogError("SendMail error! " + e.Message);
                allSend = false;
                errorMessage = e.Message;
            }
            finally
            {
                mail.Dispose();
                cleanDirectory(settingsService.MailPlotToSendBasisPath);
            }
            if (allSend)
            {
                errorMessage = "Результаты отправлены!";
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string getMessagePattern(string[] pictureCIDs = null)
        {
            var pictureHtmlAddition = "";
            if (pictureCIDs != null && pictureCIDs.Length > 0)
                for (var i = 0; i < pictureCIDs.Length; i++)
                {
                    pictureHtmlAddition += string.Format(@"
                    <tr>
                    <td style = ""width: 100%;"">
                    <img align = ""center"" src = ""cid:{0}"" style = ""display: block; margin-left: auto; margin-right: auto;""/>
                    </td>
                    </tr>", pictureCIDs[i]);
                }

            return string.Format(@"
            <table style=""border - collapse: collapse; width: 100 %; "">
            <tbody>
            <tr>
            <td style = ""width: 100%; text-align: center;"" >
            <h2> <p> <strong> Термометрия VIK </strong></p> </h2>
            <h4> <p> <strong> Показания температур за последние сутки.</strong></p> </h4>
            </td>
            </tr>
            {0}
            <tr>
            <td style = ""width: 100%;""></td>
            </tr>
            </tbody>
            </table>
            ",
            pictureHtmlAddition);
        }

        private static void buildPlotsAndAddToMessage(SettingsService settingsService, SilosService silosService, MailMessage message)
        {
            string[] pictureCIDs = null;
            var silosIdx = 0;
            pictureCIDs = new string[silosService.getAllSiloses().Count];
            foreach (var s in silosService.getAllSiloses().Values)
            {
                var plot = PlotingService.buildPlotOneDay(settingsService, silosService, s.Wires.Values.ToList(), s.Id);
                if (plot != null)
                {
                    var path = Path.Combine(settingsService.MailPlotToSendBasisPath, "plot_" + silosIdx + ".png");
                    PngExporter.Export(plot, path, settingsService.PlotWidth, settingsService.PlotHeight/*,
                        new SolidBrush(Color.White)*/);
                    message.Attachments.Add(new Attachment(path));
                    pictureCIDs[silosIdx] = message.Attachments[silosIdx].ContentId;
                    silosIdx++;
                }
            }
            message.Body = getMessagePattern(pictureCIDs);
        }

        public static bool sendErrors(string smtpServer, string sender, string password, string caption,
            string[] destinations, string errorFilePath, out string errorMessage)
        {
            if (smtpServer == "") { errorMessage = "Пустой SMTP сервер"; return false; }
            if (sender == "") { errorMessage = "Почта отправителя не указана."; return false; }
            if (password == "") { errorMessage = "Пароль для почты отправителя не указана."; return false; }
            if (destinations == null || destinations.Length == 0) { errorMessage = "Нет ни одного получателя."; return false; }

            errorMessage = "При отправке возникла ошибка: \n";

            var mail = new MailMessage();
            var result = true;
            try
            {
                mail.From = new MailAddress(sender);

                mail.To.Add(new MailAddress(sender));
                for (var i = 0; i < destinations.Length; i++)
                    mail.To.Add(new MailAddress(destinations[i]));

                mail.Subject = caption;
                mail.Body = "Отчет об ошибках Термометрии.";
                if (File.Exists(errorFilePath))
                    mail.Attachments.Add(new Attachment(errorFilePath));

                var client = new SmtpClient();
                client.Host = smtpServer;
                client.Port = 587;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(sender.Split('@')[0], password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(mail);
                result = true;

            }
            catch (Exception e)
            {
                MyLoger.LogError("SendMail errorSend error! " + e.Message);
                errorMessage = e.Message;
                result = false;
            }
            finally
            {
                mail.Dispose();
            }

            return result;
        }

        public static bool sendOverheatMail(SettingsService settingsService, SilosService silosService
            , out string errorMessage)
        {
            var smtpServer = settingsService.MailSmtpServer;
            var sender = settingsService.MailSender;
            var password = settingsService.MailSenderPassword;
            var caption = settingsService.MailCaption;
            var destinations = settingsService.getMailDestinations();

            if (smtpServer == "") { errorMessage = "Пустой SMTP сервер"; return false; }
            if (sender == "") { errorMessage = "Почта отправителя не указана."; return false; }
            if (password == "") { errorMessage = "Пароль для почты отправителя не указана."; return false; }
            if (destinations == null || destinations.Length == 0) { errorMessage = "Нет ни одного получателя."; return false; }

            errorMessage = "При отправку возникла ошибка: \n";
            var allSend = true;
            var mail = new MailMessage();
            try
            {

                mail.From = new MailAddress(sender);
                for (var i = 0; i < destinations.Length; i++)
                {
                    mail.To.Add(new MailAddress(destinations[i]));
                }
                mail.Subject = caption;
                mail.IsBodyHtml = true;

                //Подгружаем графики, если надо
                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settingsService.OverheatMailExportPath);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                var buildingPlots = "";
                if (settingsService.OverheatMailAddPlot)
                    buildingPlots = buildPlotsAndAddToOverheatMessage(settingsService, silosService, mail);

                mail.Body = getOverheatMessagePattern(OverheatTrigger.getAllOverheatSiloses(silosService, settingsService), buildingPlots);

                //Добавляем эксель документ
                if (settingsService.OverheatMailAddExcel)
                {
                    var path_excel = Path.Combine(path, "export.xlsx");
                    ExportService.exportToExcelLastTemperatures(path_excel, silosService, settingsService);
                    mail.Attachments.Add(new Attachment(path_excel));
                }



                var client = new SmtpClient();
                client.Host = smtpServer;
                client.Port = 587;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(sender.Split('@')[0], password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(mail);

            }
            catch (Exception e)
            {
                MyLoger.LogError("SendMail error! " + e.Message);
                allSend = false;
                errorMessage = e.Message;
            }
            finally
            {
                mail.Dispose();
                cleanDirectory(settingsService.OverheatMailExportPath);
            }

            //}
            if (allSend)
            {
                errorMessage = "Результаты отправлены!";
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string buildPlotsAndAddToOverheatMessage(SettingsService settingsService, SilosService silosService, MailMessage message)
        {
            string[] pictureCIDs = null;
            var silosIdx = 0;
            var siloses = OverheatTrigger.getAllOverheatSiloses(silosService, settingsService);
            pictureCIDs = new string[siloses.Count];

            foreach (var s in siloses.Keys)
            {
                var plot = PlotingService.buildPlotOneDay(settingsService, silosService, s.Wires.Values.ToList(), s.Id);
                if (plot != null)
                {
                    var path = Path.Combine(settingsService.OverheatMailExportPath, "plot_err_" + silosIdx + ".png");
                    PngExporter.Export(plot, path,
                        settingsService.PlotWidth, settingsService.PlotHeight/*,
                        new SolidBrush(Color.White)*/);
                    message.Attachments.Add(new Attachment(path));
                    pictureCIDs[silosIdx] = message.Attachments[silosIdx].ContentId;
                    silosIdx++;

                }
            }
            var pictureHtmlAddition = "";
            if (pictureCIDs != null && pictureCIDs.Length > 0)
                for (var i = 0; i < pictureCIDs.Length; i++)
                {
                    pictureHtmlAddition += string.Format(@"
                    <tr>
                    <td style = ""width: 100%;"">
                    <img align = ""center"" src = ""cid:{0}"" style = ""display: block; margin-left: auto; margin-right: auto;""/>
                    </td>
                    </tr>", pictureCIDs[i]);

                }

            return pictureHtmlAddition;
        }

        private static string getOverheatMessagePattern(Dictionary<Silos, Dictionary<Wire, int>> overheatedSiloses, string pictureHtmlAddition = "")
        {
            var overheated = "";
            foreach (var s in overheatedSiloses.Keys)
            {
                overheated += string.Format("<h4> <p> <strong>Силос № {0}.</strong></p></h4><p>Максимальный допустимы уровень температуры: {1}.</p><ul>", s.Name, s.Red);
                foreach (var item in overheatedSiloses[s])
                {
                    var w = item.Key;
                    overheated += string.Format("<li>Подвеска номер {0}. {1}/{2} сенсоров перегреты!</li>", w.Number, item.Value, w.SensorCount);
                }
                overheated += "</ul>";
            }

            return string.Format(@"
            <table style=""border - collapse: collapse; width: 100 %; "">
            <tbody>
            <tr>
            <td style = ""width: 100%; text-align: center;"" >
            <h2> <p> <strong> Термометрия VIK </strong></p> </h2>
            <h4> <p> <strong> В силосах обнаружен перегрев!</strong></p> </h4>
            </td>
            </tr>
            <tr>
            {0}
            </tr>
            {1}
            <tr>
            <td style = ""width: 100%;""></td>
            </tr>
            </tbody>
            </table>
            ", overheated, pictureHtmlAddition);
        }

        private static void cleanDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

            foreach (var path in Directory.GetFiles(directoryPath))
            {
                try
                {
                    File.Delete(path);
                }
                catch (Exception e)
                {
                    MyLoger.LogError("MailSend Clean dir error! " + e.Message);
                }
            }
        }
    }
}
