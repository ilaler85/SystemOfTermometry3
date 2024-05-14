using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.WinUIWorker;
public partial class WinUIWorker
{

    public void exportExcel(DateTime dateTime, string fileName)
    {
        if (fileName != "")
        {
            MyLoger.Log(DateTime.Now.ToString("dd.MM-HH.mm")+" Start export excel");
            presentation.setProgressBar(-1);
            if (!ExportService.exportToExcelAsync(fileName, silosService, settingsService, dateTime, exportEndEvent))
            {
                presentation.setProgressBar(-2);
                presentation.callMessageBox("Выгрузка не произошла!");
            }
            else
            {
                presentation.setProgressBar(100);
            }

        }
    }

    /// <summary>
    /// Событие окончания выгрузки
    /// </summary>
    /// <param name="message">сообщение</param>
    /// <param name="success">удачность</param>
    private void exportEndEvent(string message, bool success)
    {
        if (success)
        {
            presentation.sendLogMessage("Выгрузка сделана успешно!", Color.Green);
            presentation.callMessageBox("Выгрузка сделана успешно!");
        }
        else
        {
            presentation.callMessageBox("Не удалось сделать выгрузку в Файл. \n" +
                "Возможно он открыт в другой программе");
            presentation.sendLogMessage("Неудалось сделать выгрузку!", Color.Red);
        }
    }


}
