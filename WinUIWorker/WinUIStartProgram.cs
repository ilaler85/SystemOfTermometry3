using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemOfThermometry3.DAO;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.WinUIWorker;
public partial class WinUIWorker
{
    public void loadProgram()
    {
        /*
        if (SecurityService.IsActivate())
            connectDB();

        presentation.openFormApplyKeyForm();
        */
        connectDB();
    }

    public void successfulActivation()
    {
        connectDB();
    }

    private void connectDB()
    {
        dao = new MySQLDAO();
        string connectionString = FileProcessingService.getConnectionString();


        if (connectionString == "") //Первое подключение
        {
            presentation.openFormConnectDBDialog2();
            /*var connestString = presentation.openFormConnectDBDialog().Result;
            asyncConnectDB(connestString);*/
        }
        else
        {
            if (dao.connectToDB(connectionString, SettingsService.IsOnlyReadModeS) && (
                    SettingsService.IsOnlyReadModeS || dao.DBisCorrect())) // попытка подключения
            {
                successStartWithDB(); //удачный старт
            }
            else // не удалось подключиться
                successStartOfflineMode();
        }
    }

    public void successStartWithDB()
    {
        presentation.showWindowDownload(true);
        settingsService = new SettingsService(dao);
        settingsService.OfflineMode = false;
        initCustomComponent();
        /*if (settingsService.OfflineMode)
            presentation.setOfflineMode();
        else
        {
            if (settingsService.IsAdminMode)
                presentation.setAdminMode();
            else
                presentation.setNormalMode();
        }*/
        presentation.closeWindowDownload();
    }

    /// <summary>
    /// Вызывается, если работа начата в автономном режиме, т.е. без базы данных.
    /// </summary>
    public void successStartOfflineMode()
    {
        presentation.showWindowDownload(true);
        settingsService = new SettingsService(dao);
        settingsService.OfflineMode = true;
        initCustomComponent();
        presentation.closeWindowDownload();
    }


    private void startMainForm()
    {
        presentation.startMainForm();
    }

    public void failedActivation()
    {
        CustomClose();
    }
}
