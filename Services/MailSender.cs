using System;
using System.IO;
using System.Threading;

namespace SystemOfThermometry3.Services;

/// <summary>
/// Класс для выполнения некоторых действий каждый промежуток времени.
/// В частности отправка на почту.
/// Отправляет почту в отдельном потоке, что удобно.
/// </summary>
class MailSender
{
    public delegate void MessageDelegate(string message);
    public MessageDelegate successSendEvent; // письмо отправилось
    public MessageDelegate errorSendEvent; // произошла ошибка

    private SilosService silosService;
    private SettingsService settingsService;

    private bool todaySended;
    private bool isSendErrorThisHour;
    private int lastHourSendError;
    private DateTime lastOverhitSend;

    public MailSender(SilosService silosService, SettingsService settingsService)
    {
        this.silosService = silosService ?? throw new ArgumentNullException("Silos service is null!");
        this.settingsService = settingsService ?? throw new ArgumentNullException("Settings service is null!");

        todaySended = false;
        isSendErrorThisHour = false;
        lastHourSendError = -1;
        lastOverhitSend = DateTime.MinValue;
    }

    /// <summary>
    /// Отправляет сообщение о перегреве на почту в новом потоке.
    /// </summary>
    public void sendOverheatMailAsync()
    {
        if ((DateTime.Now - lastOverhitSend).TotalMinutes < settingsService.OverheatTriggerMinimumPeriod)
            return;

        Thread sendThread = null;
        try
        {
            successSendEvent?.Invoke("Отправка на почту началась!");
            sendThread = new Thread(sendOverheatMail);
            sendThread.Start();
        }
        catch (Exception ex)
        {
            if (sendThread != null)
                sendThread.Abort();

            errorSendEvent?.Invoke("Ошибка при отправке на почту " + ex.Message);
            MyLoger.LogError("MailSendError overheat " + ex.Message);
        }
        finally
        {
            lastOverhitSend = DateTime.Now;
        }
    }


    /// <summary>
    /// Вызывается по таймеру
    /// </summary>
    public void tick()
    {
        if (DateTime.Now.Hour == 1) //Час ночи, сбрасываем время
        {
            todaySended = false;
        }
        if (settingsService.MailSendByTimer)
            tryToSendRegularMailAsync();

        if (settingsService.MailErrorSendByTimer)
            tryToSendErrorMailAsync();

    }

    private void tryToSendRegularMailAsync()
    {
        if (todaySended || DateTime.Now.Hour != settingsService.MailSendingHour || !settingsService.MailSendByTimer)
            return;

        Thread sendThread = null;
        try
        {
            successSendEvent?.Invoke("Отправка на почту началась!");
            sendThread = new Thread(sendRegularMail);
            sendThread.Start();

        }
        catch (Exception ex)
        {
            if (sendThread != null)
                sendThread.Abort();

            errorSendEvent?.Invoke("Ошибка при отправке на почту " + ex.Message);
            MyLoger.LogError("Mail send by time error " + ex.Message);

        }
        finally
        {
            todaySended = true;
        }
    }

    private void sendRegularMail()
    {
        string message;
        if (MailSendingService.sendMail(settingsService, silosService, out message))
            successSendEvent?.Invoke("Отправка успешна!");
        else
            errorSendEvent?.Invoke("При отправке возникли ошибки!\r\n" + message);
    }

    private void tryToSendErrorMailAsync()
    {
        var nowHour = DateTime.Now.Hour;
        if (lastHourSendError == nowHour // в этот час уже отправляли
            || nowHour % settingsService.MailErrorSendingPeriod != 0 // в этот час не надо отправлять
            || !settingsService.MailErrorSendByTimer) // По таймеру отправлять не нужно
            return;

        Thread sendThread = null;
        try
        {
            sendThread = new Thread(sendErrorMail);
            sendThread.Start();
        }
        catch (Exception ex)
        {
            if (sendThread != null)
                sendThread.Abort();

            MyLoger.LogError("MailError send by time error " + ex.Message);
        }
        finally
        {
            lastHourSendError = nowHour;
        }
    }

    private void sendErrorMail()
    {
        string message;
        var dir = FileProcessingService.LogDir;
        var file = Path.Combine(dir, "Errors.txt");
        if (!Directory.Exists(dir) || !File.Exists(file))
            return;

        if (!MailSendingService.sendErrors(
            settingsService.MailErrorSmtpServer,
            settingsService.MailErrorSender,
            settingsService.MailErrorSenderPassword,
            settingsService.MailErrorCaption,
            settingsService.MailErrorDestination[0],
            file,
            out message))
        {
            MyLoger.LogError("MailSenderError " + message);
        }
        else
        {
            File.Delete(file);
        }

    }

    private void sendOverheatMail()
    {
        string message;
        if (MailSendingService.sendOverheatMail(settingsService, silosService, out message))
            successSendEvent?.Invoke("Отправка успешна!");
        else
            errorSendEvent?.Invoke("При отправке возникли ошибки!\r\n" + message);
    }
}
