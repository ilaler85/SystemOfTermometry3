using System;
using SystemOfTermometry2.Services;

namespace SystemOfThermometry2.Services
{
    public partial class SettingsService
    {

        public const int EXCEL_EXPORT_STANDARD_TEMPLATE = 1;
        public const int EXCEL_EXPORT_LEV_TEMPLATE = 2;

        public int ExcelExportType
        {
            get => Convert.ToInt32(getProperty("excel_export_position_type", "1"));
            set => setProperty("excel_export_position_type", value.ToString());
        }


        /// <summary>
        /// Час, когда будет отправляться почта.
        /// </summary>
        public int MailSendingHour
        {
            get => Convert.ToInt32(getProperty("mail_sending_hour", "8"));
            set => setProperty("mail_sending_hour", value.ToString());
        }

        /// <summary>
        /// SMTP Сервер
        /// </summary>
        public string MailSmtpServer
        {
            get => getProperty("mail_smtp_server", "smtp.mail.ru");
            set => setProperty("mail_smtp_server", value);
        }

        /// <summary>
        /// Почта получателя
        /// </summary>
        public string MailSender
        {
            get => getProperty("mail_sender", "");
            set => setProperty("mail_sender", value);
        }

        /// <summary>
        /// Пароль от почты получателя
        /// </summary>
        public string MailSenderPassword
        {
            get => getProperty("mail_sender_password", "");
            set => setProperty("mail_sender_password", value);
        }

        /// <summary>
        /// Тема письма
        /// </summary>
        private string caption = "Показания термометрии VIK";
        public string MailCaption
        {
            get
            {
                if (caption == null)
                    caption = FileProcessingService.getStringFromFile("caption");
                return caption;
            }
            set
            {
                caption = value;
                FileProcessingService.setStringToFile("caption", caption);
            }
        }

        /// <summary>
        /// Путь к отправляемым графикам
        /// </summary>
        public string MailPlotToSendBasisPath
        {
            get => FileProcessingService.ExportDir;
            //getProperty("mail_plot_to_send_basis_path", Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "Export"));
            //set => setProperty("mail_plot_to_send_basis_path", value);
        }

        /// <summary>
        /// Нужно ли отправлять графики
        /// </summary>
        public bool MailIsSendPlot
        {
            get => Convert.ToBoolean(getProperty("mail_is_send_plot", true.ToString()));
            set => setProperty("mail_is_send_plot", value.ToString());
        }

        /// <summary>
        /// Нужно ли показывать цвет на клетках с температурами в excel
        /// </summary>
        public bool ExportIsSetColorOnCellWithTemp
        {
            get => Convert.ToBoolean(getProperty("mail_is_set_color_on_cell_with_temp", true.ToString()));
            set => setProperty("mail_is_set_color_on_cell_with_temp", value.ToString());
        }

        /// <summary>
        /// Получает массив с получателями рассылки, храниться в файле
        /// </summary>
        /// <returns>Массив с почтами</returns>
        public string[] getMailDestinations()
        {
            return FileProcessingService.getDestinations("destinations")[0];
        }

        /// <summary>
        /// Получает массив с получателями рассылки и заметки о них, храниться в файле
        /// </summary>
        /// <returns>получатели рассылки, нулевой массив - получатели
        /// первый - заметки</returns>
        public string[][] getMailDestinationsWithNotes()
        {
            return FileProcessingService.getDestinations("destinations");
        }

        /// <summary>
        /// Задает массив с получателями почты
        /// </summary>
        /// <param name="destinations">получатели рассылки, нулевой массив - получатели
        /// первый - заметки</param>
        public void setMailDestinations(string[][] destinations)
        {
            FileProcessingService.setDestinations("destinations", destinations);
        }

        /// <summary>
        /// Название компании, которое будет отображаться в Эксельке, храниться в файле
        /// </summary>
        private string companyName = null;
        public string CompanyName
        {
            get => getProperty("company_name", "");
            set => setProperty("company_name", value);
            /*get
            {
                if (companyName == null)
                    companyName = FileProcessingService.getStringFromFile("companyName");
                return companyName;
            }
            set
            {

                companyName = value;
                FileProcessingService.setStringToFile("companyName", companyName);
            }*/
        }

        public string SubdivisionFileName
        {
            get => getProperty("subdivision_file_name", "subdivisions");
            //set => setProperty("subdivision_file_name", value);
        }

        /// <summary>
        /// True - рассылка писем в выбраный час
        /// </summary>
        public bool MailSendByTimer
        {
            get => Convert.ToBoolean(getProperty("mail_send_by_timer", false.ToString()));
            set => setProperty("mail_send_by_timer", value.ToString());
        }

        /// <summary>
        /// Рассылка писем при превышении температуры
        /// </summary>
        public bool MailErrorSendByTimer
        {
            get => Convert.ToBoolean(getProperty("mail_error_send", false.ToString()));
            set => setProperty("mail_error_send", value.ToString());
        }

        /// <summary>
        /// Час, когда будет отправляться ошыбки на почту.
        /// </summary>
        public int MailErrorSendingPeriod
        {
            get => Convert.ToInt32(getProperty("mail_error_sending_preiod", "3"));
            set => setProperty("mail_error_sending_preiod", value.ToString());
        }

        /// <summary>
        /// SMTP Сервер для почты ошибок
        /// </summary>
        public string MailErrorSmtpServer
        {
            get => getProperty("mail_error_smtp_server", "smtp.mail.ru");
            set => setProperty("mail_error_smtp_server", value);
        }

        /// <summary>
        /// Почта получателя и отрпавителя ошибок
        /// </summary>
        public string MailErrorSender
        {
            get => getProperty("mail_error_sender", "");
            set => setProperty("mail_error_sender", value);
        }

        /// <summary>
        /// Пароль от почты получателя и отрпавителя ошибок
        /// </summary>
        public string MailErrorSenderPassword
        {
            get => getProperty("mail_error_sender_password", "");
            set => setProperty("mail_error_sender_password", value);
        }

        /// <summary>
        /// Тема отчета об ошибках
        /// </summary>
        private string captionError = "Ошибки Термометрии";
        public string MailErrorCaption
        {
            get
            {
                if (captionError == null)
                    captionError = FileProcessingService.getStringFromFile("captionError");
                return captionError;
            }
            set
            {
                captionError = value;
                FileProcessingService.setStringToFile("captionError", captionError);
            }
        }

        /// <summary>
        /// Получатели отчета об ошибках
        /// </summary>
        public string[][] MailErrorDestination
        {
            get
            {
                return FileProcessingService.getDestinations("error_destinations");
            }
            set
            {
                FileProcessingService.setDestinations("error_destinations", value);
            }
        }       

        /*public string MailErrorDestination
        {
            get => getProperty("mail_error_destination", "");
            set => setProperty("mail_error_destination", value);
        }*/

        /// <summary>
        /// Путь к логам
        /// </summary>
        public string LogBasisPath
        {
            get => getProperty("log_basis_path", FileProcessingService.LogDir);
            set => setProperty("log_basis_path", value);
        }


    }
}
