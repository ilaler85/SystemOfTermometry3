using System;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.Services
{
    partial class SettingsService
    {
        /// <summary>
        /// Рассылка писем при превышении температуры
        /// </summary>
        public bool OverheatPlaySound
        {
            get => Convert.ToBoolean(getProperty("overheat_sound", false.ToString()));
            set => setProperty("overheat_sound", value.ToString());
        }

        /// <summary>
        /// Рассылка писем при превышении температуры
        /// </summary>
        public bool OverheatMailSend
        {
            get => Convert.ToBoolean(getProperty("overheat_send_mail", false.ToString()));
            set => setProperty("overheat_send_mail", value.ToString());
        }

        /// <summary>
        /// Добавлять excel файл при отправке на почту
        /// </summary>
        public bool OverheatMailAddExcel
        {
            get => Convert.ToBoolean(getProperty("overheat_mail_add_excel", false.ToString()));
            set => setProperty("overheat_mail_add_excel", value.ToString());
        }

        /// <summary>
        /// Добавлять графики при отправке на почту
        /// </summary>
        public bool OverheatMailAddPlot
        {
            get => Convert.ToBoolean(getProperty("overheat_mail_add_plot", false.ToString()));
            set => setProperty("overheat_mail_add_plot", value.ToString());
        }

        /// <summary>
        /// Рассылка писем при превышении температуры
        /// </summary>
        public string OverheatMailExportPath
        {
            get => FileProcessingService.ExportOverheatDir;
            //getProperty("overheat_send_path", Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "ExportOverheat"));
            //set => setProperty("overheat_send_path", value);
        }

        /// <summary>
        /// Минимальное время в минутах между срабатывании сигналов о перегреве в каждом силосе отдельно
        /// </summary>
        public int OverheatTriggerMinimumPeriod
        {
            get => Convert.ToInt32(getProperty("overheat_trigger_period", "30"));
            set => setProperty("overheat_trigger_period", value.ToString());
        }

        /// <summary>
        /// Минимальное количество сенсоров для срабатывания сигнала о перегреве
        /// </summary>
        public int OverheatMinimumSensorToTrigger
        {
            get => Convert.ToInt32(getProperty("overheat_min_sensor", "1"));
            set => setProperty("overheat_min_sensor", value.ToString());
        }
    }
}
