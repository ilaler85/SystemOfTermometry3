using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using Microsoft.UI.Xaml;
using Mysqlx.Cursor;
using SystemOfThermometry3.Model;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.WinUIWorker;
public partial class WinUIWorker
{

    private void showStatusSetting()
    {
        presentation.setStatus("Настройки " + (settingsService.OfflineMode ? "- автономный режим." : "- подключено к базе данных. ")
                + (settingsService.IsOnlyReadMode ? " Обзор." : ""));
    }

   


    #region структура подразделений
    public void saveStructureSubvision(List<StructureSubdivision> subdivisions)
    {
        List<int> subDivToDelete = silosService.getSubdivisions().Keys.ToList();
        foreach (var subdiv in subdivisions)
        {
            if (!subDivToDelete.Contains(subdiv.Id)) //Новое добавляем
            {
                var tmp = subdiv;
                silosService.addSubdivision(ref tmp);
            }
            else
            {
                silosService.updateSubdivisions(subdiv);
                subDivToDelete.Remove(subdiv.Id);
            }
        }

        foreach (int id in subDivToDelete)
            silosService.deleteSubdivision(id);
    }
    #endregion

    #region вычисление заполненности 
    public void calculateFillingSilosesTable(DateTime time)
    {
        silosService.fillingInSilosTable(time);
    }
    #endregion

    #region настройки провайдера
    public bool attemptOpenSetttingProvider()
    {
        return presentation.openEnterForm(checkProviderPassword);
    }
    #endregion

    #region Настройка порта
    public void savePortSettings(string portName, int portBoundRate, Parity portParity, int portDataBits, StopBits portStopBits, int portTimeOut)
    {
        try
        {
            settingsService.PortComPort = portName;
            settingsService.PortBaudRate = portBoundRate;
            settingsService.PortParity = portParity;
            settingsService.PortDataBits = portDataBits;
            settingsService.PortStopBits = portStopBits;
            settingsService.PortTimeOut = portTimeOut;
        }
        catch
        {
            presentation.callMessageBox("Ошибка. Введите корректные данные");
        }
    }
    #endregion

    #region Настройка опроса

    public void saveObservSetting(uint newPeriod, uint newTriesCount, bool isGiveSecondChanse, uint newTimeout)
    {
        try
        {
            settingsService.ObserveIterationPeriod = newPeriod;
            settingsService.ObserveTriesToObserveBrokenWire = newTriesCount;
            settingsService.ObserveIsGiveBrokenWireSecondChanse = isGiveSecondChanse;
            settingsService.ObserveBrokenWireTimeOutMilisec = newTimeout;
        }
        catch
        {
            presentation.callMessageBox("Введите корректное значение!");
        }
    }

    #endregion

    #region База Данных
    public void connectDBAndGetData(string connectString, string user)
    {

        if (user.Trim() == "user")
        {
            settingsService.IsOnlyReadMode = true;
        }

        if (!dao.connectToDB(connectString, settingsService.IsOnlyReadMode))
        {
            presentation.callMessageBox("Не удалось подключиться!\n" +
                "проверьте имя пользователя и пароль");
        }
        else
        {
            presentation.callMessageBox("Подключение успешно!");
            FileProcessingService.setConnectionString(connectString);
            silosService.synchronizeWithGettingData();
            successStartWithDB();
            presentation.refreshSilosTabControl();//Обновляем
            presentation.refreshSetting();
        }
        showStatusSetting();
    }

    public void connectAndSetData(string connectString)
    {
        if (!dao.connectToDB(connectString))
        {
            presentation.callMessageBox("Не удалось подключиться!\n" +
                "проверьте имя пользователя и пароль");
        }
        else
        {
            presentation.callMessageBox("Подключение успешно!");
            FileProcessingService.setConnectionString(connectString);
            silosService.synchronizeWithSettingData();
            successStartWithDB();
            presentation.refreshSilosTabControl(); //Обновляем
            presentation.refreshSetting();
            Application.Current.Exit();
        }
        showStatusSetting();
    }

    public void dropDB(string connectString)
    {
        if (!dao.connectToDB(connectString) || !dao.dropAndCreateDB())
        {
            presentation.callMessageBox("Не удалось подключиться. Проверьте пароль");
        }
        else
        {
            presentation.callMessageBox("Подключение успешно!");
            FileProcessingService.setConnectionString(connectString);
            successStartWithDB();
            presentation.refreshSilosTabControl(); //Обновляем
            presentation.refreshSetting();
        }
        showStatusSetting();
    }

    public void dropTemperature(string connectString, DateTime time)
    {
        if (!dao.connectToDB(connectString) || !dao.deleteTemperaturesBeforeDate(time))
        {
            //!dao.dropTemperatureTable())            
            presentation.callMessageBox("Не удалось подключиться. Проверьте пароль");
        }
        else
        {
            presentation.callMessageBox("Подключение успешно!");
            FileProcessingService.setConnectionString(connectString);
        }
        showStatusSetting();
    }
    #endregion

    #region автономный режим
    public void offlineMode(string fileName)
    {
        if (fileName != "")
        {
            Dictionary<int, Silos> newSiloses;
            Dictionary<int, Dictionary<int, Wire>> newDevices;
            Dictionary<int, StructureSubdivision> subdivs;

            if (FileProcessingService.deserialize(fileName,
                out newSiloses, out newDevices, out subdivs)
                && (newSiloses != null && newDevices != null && subdivs != null))
            {
                silosService.reInitService(newSiloses, newDevices, subdivs);
                successStartWithDB();
                presentation.refreshSilosTabControl(); //Обновляем
                presentation.refreshSetting();

                settingsService.OfflineMode = true;

                presentation.callMessageBox("Теперь база данных не будет использоватья.\n" +
                    "Значения температур будут записываться в файл \"Temperature\"");
                showStatusSetting();

                return;
            }
        }
    }

    public void setOfflineMode()
    {
        settingsService.OfflineMode = true;
        successStartWithDB();
        presentation.callMessageBox("Теперь база данных не будет использоватья.\n" +
            "Значения температур будут записываться в файл \"Temperature\"");
        showStatusSetting();
    }

    public void saveToFile(string filepath)
    {
        if (filepath != "")
        {
            if (FileProcessingService.serialize(filepath,
                silosService.getAllSiloses(), silosService.getDevices(), silosService.getSubdivisions()))
                presentation.callMessageBox("Файл сохранен.");
            else
                presentation.callMessageBox("Не удалось сохранить в файл.");
        }
    }

    public void loadTemperature(string filename)
    {
        if (filename != "")
            if (silosService.loadTemperaturesFromFile(filename))
            {
                presentation.callMessageBox("Файл загружен!");
                return;
            }

        presentation.callMessageBox("Не удалось загрузить файл.");
    }

    #endregion

    #region почтовая рассылка

    public void saveMail(string mailSMTPtextBox, string mailSenderTextBox, string mailPasswordTextBox, 
        string mailCaptionTextBox, bool addPlotsCheckBox, int mailTimeNumericUpDown, bool mailSEndByTimeCheckBox, string[][] destinations)
    {
        settingsService.MailSmtpServer = mailSMTPtextBox;
        settingsService.MailSender = mailSenderTextBox;
        settingsService.MailSenderPassword = mailPasswordTextBox;
        settingsService.MailCaption = mailCaptionTextBox;
        settingsService.MailIsSendPlot = addPlotsCheckBox;
        settingsService.MailSendingHour = mailTimeNumericUpDown;
        settingsService.MailSendByTimer = mailSEndByTimeCheckBox;
        settingsService.setMailDestinations(destinations);
    }

    public void mailSend(string mailSMTPtextBox, string mailSenderTextBox, string mailPasswordTextBox,
        string mailCaptionTextBox, bool addPlotsCheckBox, int mailTimeNumericUpDown, bool mailSEndByTimeCheckBox, string[][] destinations)
    {
        saveMail(mailSMTPtextBox, mailSenderTextBox, mailPasswordTextBox, mailCaptionTextBox,
            addPlotsCheckBox, mailTimeNumericUpDown, mailSEndByTimeCheckBox, destinations);
        string errorMessage;
        MailSendingService.sendMail(settingsService, silosService, out errorMessage);
        presentation.callMessageBox(errorMessage);
    }


    public void saveErrorMail(string MailErrorSmtpServer, string MailErrorSender, string MailErrorSenderPassword, 
        string MailErrorCaption, bool MailErrorSendByTimer, int MailErrorSendingPeriod, string[][] destinations)
    {
        settingsService.MailErrorSmtpServer = MailErrorSmtpServer;
        settingsService.MailErrorSender = MailErrorSender;
        settingsService.MailErrorSenderPassword = MailErrorSenderPassword;
        settingsService.MailErrorCaption = MailErrorCaption;
        settingsService.MailErrorSendByTimer = MailErrorSendByTimer;
        settingsService.MailErrorSendingPeriod = MailErrorSendingPeriod;
        settingsService.MailErrorDestination = destinations;

    }


    public void mailErrorSend(string MailErrorSmtpServer, string MailErrorSender, string MailErrorSenderPassword,
        string MailErrorCaption, bool MailErrorSendByTimer, int MailErrorSendingPeriod, string[][] destinations)
    {
        saveErrorMail(MailErrorSmtpServer, MailErrorSender, MailErrorSenderPassword,
        MailErrorCaption, MailErrorSendByTimer, MailErrorSendingPeriod, destinations);

        string pathToErrorLog = Path.Combine(FileProcessingService.LogDir, "Errors.txt");
        string errorMessage;
        if (File.Exists(pathToErrorLog))
        {
            if (MailSendingService.sendErrors(
                settingsService.MailErrorSmtpServer,
                settingsService.MailErrorSender,
                settingsService.MailErrorSenderPassword,
                settingsService.MailErrorCaption,
                settingsService.MailErrorDestination[0],
                pathToErrorLog,
                out errorMessage))
            {
                File.Delete(pathToErrorLog);
                presentation.callMessageBox("Файл с ошибками отправлен.");
            }
            else
            {
                presentation.callMessageBox("Ошибка при отправке!\n" + errorMessage);
            }
        }
        else
        {
            presentation.callMessageBox("Файла с ошибками не существует!");
        }
    }
    #endregion

    #region настройка пароля

    public void saveChangeAdminPassword(string oldPswd, string newPswd, string newPswd2)
    {
        bool allIsOk = true;
        string wrongAnswer = "Не удалось сменить пароль!";

        if (!SecurityService.checkAdminPassword(oldPswd))
        {
            allIsOk = false;
            wrongAnswer += "\nСтарый пароль введен не верно!";
        }

        if (newPswd != newPswd2)
        {
            allIsOk = false;
            wrongAnswer += "\nНовые пароли должны совподать!";
        }

        if (allIsOk)
        {
            SecurityService.setAdminPassword(newPswd);
            presentation.callMessageBox("Пароль успешно сменен");
        }
        else
        {
            presentation.callMessageBox(wrongAnswer);
        }
    }

    #endregion


}
