using NPOI.OpenXmlFormats.Dml;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SystemOfThermometry3.Model;
using SystemOfThermometry3.Services;
using BorderStyle = NPOI.SS.UserModel.BorderStyle;
using HorizontalAlignment = NPOI.SS.UserModel.HorizontalAlignment;

namespace SystemOfThermometry3.Services
{
    public static class ExportService
    {
        public delegate void ExportCallbackDelegate(string message, bool succesfull);
        public delegate void Progress(int value);
        public static Progress progress;
        private static object sync = new object();
        private static bool inExport = false;

        /// <summary>
        /// Создает строку и увеличивает индекс
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowIdx"></param>
        /// <param name="len"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        private static IRow createRow(ISheet sheet, int rowIdx, int len, ICellStyle style = null)
        {
            var row = sheet.CreateRow(rowIdx);
            for (var i = 0; i < len; i++)
            {
                row.CreateCell(i);
                if (style != null)
                    row.GetCell(i).CellStyle = style;
            }
            return row;
        }

        /// <summary>
        /// Создает строку и увеличивает индекс
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="rowIdx"></param>
        /// <param name="len"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        private static IRow createRow(ISheet sheet, int rowIdx, int len, int startStyleIdx, int endStyleIdx, ICellStyle style)
        {
            var row = sheet.CreateRow(rowIdx);
            for (var i = 0; i < len; i++)
            {
                row.CreateCell(i);
                if (i >= startStyleIdx && i <= endStyleIdx)
                    row.GetCell(i).CellStyle = style;
            }
            return row;
        }


        #region отключенные методы

        /// <summary>
        /// Создает строку и увеличивает индекс, для каждой клетки создается свой стиль
        /// </summary>
        private static IRow createRowWithOwnStyle(XSSFWorkbook workbook, ISheet sheet, int rowIdx, int len)
        {
            var row = sheet.CreateRow(rowIdx);
            for (var i = 0; i < len; i++)
            {
                row.CreateCell(i);
                row.GetCell(i).CellStyle = workbook.CreateCellStyle();
            }
            return row;
        }
        // Отключено так как по какой-то причине выдавало ошибку
        private static void setCellStyleThinBorder(IRow row, int start, int end)
        {
            for (var i = start; i < end; i++)
            {
                var cell = row.GetCell(i);
                cell.CellStyle.BorderBottom = BorderStyle.Thin;
                cell.CellStyle.BorderTop = BorderStyle.Thin;
                cell.CellStyle.BorderRight = BorderStyle.Thin;
                cell.CellStyle.BorderLeft = BorderStyle.Thin;
            }
        }

        private static void setCellStyleThinBorder(ICell cell)
        {
            cell.CellStyle.BorderBottom = BorderStyle.Thin;
            cell.CellStyle.BorderTop = BorderStyle.Thin;
            cell.CellStyle.BorderRight = BorderStyle.Thin;
            cell.CellStyle.BorderLeft = BorderStyle.Thin;
        }

        private static void setCellStyleMediumBorder(ICell cell, bool t, bool b, bool r, bool l)
        {
            if (t) cell.CellStyle.BorderTop = BorderStyle.Medium;
            if (b) cell.CellStyle.BorderBottom = BorderStyle.Medium;
            if (r) cell.CellStyle.BorderRight = BorderStyle.Medium;
            if (l) cell.CellStyle.BorderLeft = BorderStyle.Medium;
        }

        private static ICellStyle createStyle(IWorkbook workbook, bool t, bool b, bool r, bool l)
        {
            var s = workbook.CreateCellStyle();
            s.BorderTop = t ? BorderStyle.Medium : BorderStyle.Thin;
            s.BorderBottom = b ? BorderStyle.Medium : BorderStyle.Thin;
            s.BorderRight = r ? BorderStyle.Medium : BorderStyle.Thin;
            s.BorderLeft = l ? BorderStyle.Medium : BorderStyle.Thin;
            s.Alignment = HorizontalAlignment.Center;
            return s;
        }
        #endregion
        #region new version
        /// <summary>
        /// Метод выполняет экспорт в уже созданный ранее файл эксель. Если файла нет, то создает его
        /// </summary>
        /// <param name="filepath">Путь файла Эксель</param>
        /// <param name="silosService"></param>
        /// <param name="settingsService"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static bool exportExcelThemperature(string filepath, SilosService silosService, SettingsService settingsService, ExportCallbackDelegate callback, DateTime time)
        {
            try
            {
                var workbook = new XSSFWorkbook();
                var result = true;
                var path = @"d:\Термометрия\maket.xlsx";
                if (!File.Exists(path))
                {
                    initExcelFile(workbook, path, silosService, settingsService, callback, time);
                }
                replaceExcelFile(workbook, path, silosService, settingsService, callback, time);

                return true;


            }
            catch (Exception ex)
            {
                MyLoger.LogError("Export error " + ex.Message);
                return false;
            }
        }

        private static void initExcelFile(XSSFWorkbook workbook, string filepath, SilosService silosService, SettingsService settingsService, ExportCallbackDelegate callback, DateTime time)
        {
            var siloses = silosService.getAllSiloses();

            if (siloses == null)
            {
                MyLoger.Log("siloses or temperatures is null");
            }

            var sheet = workbook.CreateSheet("Силосы");

            #region settings and styles
            var cellStyleBorder = workbook.CreateCellStyle(); // стиль с обводкой и выравниванием по центру
            cellStyleBorder.BorderBottom = BorderStyle.Thin;
            cellStyleBorder.BorderLeft = BorderStyle.Thin;
            cellStyleBorder.BorderRight = BorderStyle.Thin;
            cellStyleBorder.BorderTop = BorderStyle.Thin;
            cellStyleBorder.Alignment = HorizontalAlignment.Center;
            cellStyleBorder.VerticalAlignment = VerticalAlignment.Center;

            var redCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            redCellStyle.SetFillForegroundColor(new XSSFColor(Color.OrangeRed));
            redCellStyle.FillPattern = FillPattern.SolidForeground;
            var yllowCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            yllowCellStyle.SetFillForegroundColor(new XSSFColor(Color.Yellow));
            yllowCellStyle.FillPattern = FillPattern.SolidForeground;
            var greenCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            greenCellStyle.SetFillForegroundColor(new XSSFColor(Color.LightGreen));
            greenCellStyle.FillPattern = FillPattern.SolidForeground;
            var blueCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            blueCellStyle.SetFillForegroundColor(new XSSFColor(Color.Blue));
            blueCellStyle.FillPattern = FillPattern.SolidForeground;
            var grayCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            grayCellStyle.SetFillForegroundColor(new XSSFColor(Color.Gray));
            grayCellStyle.FillPattern = FillPattern.SolidForeground;

            var colorToStyleMap = new Dictionary<Color, XSSFCellStyle>()
                {
                    {Color.Red, redCellStyle},
                    {Color.Yellow, yllowCellStyle},
                    {Color.LightGreen,greenCellStyle},
                    {Color.Blue, blueCellStyle},
                    {Color.Gray , grayCellStyle},
                };


            #endregion
            #region create header
            var lastRowIdx = -1; //Переменная, которая указывает на индекс последней созданной стоки

            //Делаем шапку
            var row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder);
            var text = new XSSFRichTextString(settingsService.CompanyName);
            var font = new XSSFFont();
            font.IsBold = true;
            font.FontHeightInPoints = 16;
            text.ApplyFont(font);
            row.GetCell(0).SetCellValue(text);

            row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder);
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, 10));


            sheet.CreateRow(++lastRowIdx); //Отступ
            row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder);

            sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 10));
            sheet.GetRow(lastRowIdx).GetCell(0).SetCellValue(
                "Показатели температуры продукта за " + time.ToString("dd.MM.yyyy - HH:mm"));

            sheet.CreateRow(++lastRowIdx); //Отступ
            MyLoger.Log("Create header success.");
            #endregion

            // создаем список силосов для каждого структурного подразделения.
            var subdivIdToSiloses = new SortedDictionary<int, List<int>>
                (Comparer<int>.Create((x, y) =>
                {
                    if (x == y)
                        return 0;
                    return x > y ? -1 : 1;
                }));

            subdivIdToSiloses.Add(-1, new List<int>()); //Силосы без подразделения                

            foreach (var silos in siloses.Values)
            {
                //Не имеет корректного подразделения
                if (!silosService.getSubdivisions().ContainsKey(silos.StructureId))
                {
                    subdivIdToSiloses[-1].Add(silos.Id);
                    continue;
                }

                if (!subdivIdToSiloses.ContainsKey(silos.StructureId))
                    subdivIdToSiloses.Add(silos.StructureId, new List<int>());

                subdivIdToSiloses[silos.StructureId].Add(silos.Id);
            }

            MyLoger.Log("Create subdivInSilos success.");

            //Идем по всем структурным подразделениям.
            foreach (var subdivEntry in subdivIdToSiloses)
            {
                MyLoger.Log("Subdiv " + subdivEntry.Key + " Count: " + subdivEntry.Value.Count);

                MyLoger.Log("Subdiv " + subdivEntry.Key + " Count: " + subdivEntry.Value.Count);

                row = sheet.CreateRow(++lastRowIdx); //Отступ
                row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder); //Строка с подразделением
                sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 10));
                if (subdivEntry.Key == -1)
                    row.GetCell(0).SetCellValue("Силосы без структурного подразделения");
                else
                    row.GetCell(0).SetCellValue("Структурное подразделение: " + silosService.getSubdivisions()[subdivEntry.Key]);

                //sheet.CreateRow(++lastRowIdx); //Отступ

                //Проходим по всем силосам в структурном подразделении
                foreach (var silosID in subdivEntry.Value)
                {
                    sheet.CreateRow(++lastRowIdx); //Отступ
                    MyLoger.Log("Silos id: " + silosID);
                    var s = silosService.getSilos(silosID);
                    MyLoger.Log("Success get from service, name: " + s.Name);

                    var columnCount = Math.Min(s.getEnabledWireCount(), 10);
                    row = createRow(sheet, ++lastRowIdx, columnCount + 1, cellStyleBorder);//Строка с силосом
                    sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, columnCount));
                    row.GetCell(0).SetCellValue("Силос № " + s.Name);

                    var wiresList = s.getEnabledWires();
                    wiresList.Sort((w1, w2) => w1.Number.CompareTo(w2.Number));

                    if (wiresList.Count == 0 || columnCount == 0) //Нет ни одной активной подвески.
                    {
                        sheet.CreateRow(++lastRowIdx); //Отступ
                        row = createRow(sheet, ++lastRowIdx, 5);
                        sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 4));
                        row.GetCell(0).SetCellValue("В Силосе нет ни одной активной подвески!");
                        continue;
                    }


                    //Бъем на несколько частей по 10 подвесок
                    for (var i = 0; i < wiresList.Count / 10 + 1; i++)
                    {
                        var currentColumnCount = Math.Min(s.getEnabledWireCount() - i * 10, 10); // количество столбцов в строке.

                        if (currentColumnCount == 0)
                            break;

                        sheet.CreateRow(++lastRowIdx); //Отступ
                        row = createRow(sheet, ++lastRowIdx, currentColumnCount + 1); //Строка "номер термоподвески"
                        sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 1, currentColumnCount));
                        row.GetCell(1).SetCellValue("Номер термоподвески");
                        row.GetCell(0).SetCellValue("Номер\r\nДатчика");

                        row = createRow(sheet, ++lastRowIdx, currentColumnCount + 1); //Строка "номер датчика"
                        sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx - 1, lastRowIdx, 0, 0));

                        //считаем максимальное кол-во датчиков в данном диапазоне.
                        var maxSensor = 1;

                        for (var j = 0; j < 10; j++)
                        {
                            if (i * 10 + j >= wiresList.Count)
                                break;
                            maxSensor = Math.Max(wiresList[i * 10 + j].SensorCount, maxSensor);
                        }

                        MyLoger.Log("Fill wires numbers");
                        //Заполняем номера подвесок.
                        for (var colIdx = 0; colIdx < currentColumnCount; colIdx++)
                        {
                            var w = wiresList[i * 10 + colIdx];
                            row.GetCell(1 + colIdx).SetCellValue(w.Number);
                        }

                        MyLoger.Log("Fill data");
                        //Создаем строки и сразу их заполняем
                        for (var rowIdx = 0; rowIdx < maxSensor; rowIdx++)
                        {
                            MyLoger.Log("Row idx: " + rowIdx);
                            row = createRow(sheet, ++lastRowIdx, currentColumnCount + 1);
                            row.GetCell(0).SetCellValue(rowIdx + 1); // Номер сенсора
                            /*for (int colIdx = 1; colIdx <= currentColumnCount; colIdx++)
                            {
                                Wire w = wiresList[i * 10 + colIdx - 1];
                                MyLoger.Log("Column idx: " + colIdx + " " + w.ToString());
                                if (!temperatures.ContainsKey(w.Id) || rowIdx >= temperatures[w.Id].Length) // В этой подвеске нет такого сенсора
                                {
                                    MyLoger.Log("Temperatures not contained");
                                    row.GetCell(colIdx).SetCellValue("-");
                                    continue;
                                }

                                if (temperatures[w.Id][rowIdx] != "-")
                                    row.GetCell(colIdx).SetCellValue(Math.Round(Convert.ToSingle(temperatures[w.Id][rowIdx]), 1));
                                else
                                    row.GetCell(colIdx).SetCellValue(temperatures[w.Id][rowIdx]);

                                if (settingsService.ExportIsSetColorOnCellWithTemp) //Делаем стиль, если надо                                    
                                    row.GetCell(colIdx).CellStyle = colorToStyleMap[SilosService.getColor(temperatures[w.Id][rowIdx], s.Yellow, s.Red)];

                            }*/
                            MyLoger.Log("sucsess row");
                        }
                    }

                } //Проход по силосам в структурном подразделении


                sheet.CreateRow(++lastRowIdx); //Отступ
                MyLoger.Log("Subdiv " + subdivEntry.Key + " success");
            }

        }

        private static bool replaceExcelFile(XSSFWorkbook workbook, string filepath, SilosService silosService, SettingsService settingsService, ExportCallbackDelegate callback, DateTime time)
        {
            var sheet = workbook.GetSheetAt(0);

            return true;
        }




        #endregion


        /// <summary>
        /// Делает запись в файл пследних значений температур в отдельном потоке
        /// </summary>
        /// <param name="filepath">файл</param>
        /// <param name="silosService">Сервис по работе с силосами</param>
        /// <param name="settingsService">Сервис по работе с настройками</param>
        /// <returns>false если уже в процессе экспорта</returns>
        public static bool exportToExcelLastTemperatureAsync(string filepath, SilosService silosService, SettingsService settingsService, ExportCallbackDelegate callback, Progress progress)
        {
            //lock (sync)
            //{
            if (inExport)
                return false;

            inExport = true;
            var exportThread = new Thread(exportInnerThreadFunctionForLastTemp);
            exportThread.Start(new Tuple<string, SilosService, SettingsService, ExportCallbackDelegate, Progress>(filepath, silosService, settingsService, callback, progress));
            return true;
        }



        private static void exportInnerThreadFunctionForLastTemp(object parameters)
        {
            var tuple = (Tuple<string, SilosService, SettingsService, ExportCallbackDelegate, Progress>)parameters;
            var filepath = tuple.Item1;
            var silosService = tuple.Item2;
            var settingsService = tuple.Item3;
            var callback = tuple.Item4;
            progress = tuple.Item5;
            progress?.Invoke(-1);
            if (exportToExcelLastTemperatures(filepath, silosService, settingsService, progress))
            {
                callback("Выгрузка сделана успешно!", true);

            }
            else
            {
                callback("Не удалось сделать выгрузку в Файл. \n" +
                    "Возможно он открыт в другой программе", false);
                //MyLoger.LogError("Export error " + ex.Message);
            }
            inExport = false;
        }

        /// <summary>
        /// Делает запись в файл задданных значений температурв отдельном потоке
        /// </summary>
        /// <param name="filepath">файл</param>
        /// <param name="silosService">Сервис по работе с силосами</param>
        /// <param name="settingsService">Сервис по работе с настройками</param>
        /// <param name="temperatures">Заданные температуры, ключ - ид подвески</param>
        /// <param name="time">Время выгрузки</param>
        /// <returns>false если уже в процессе экспорта</returns>
        public static bool exportToExcelAsync(string filepath, SilosService silosService, SettingsService settingsService, DateTime time, ExportCallbackDelegate callback, Progress progress)
        {
            if (inExport)
                return false;

            inExport = true;
            var exportThread = new Thread(exportInnerThreadFunctionForChosenTime);
            exportThread.Priority = ThreadPriority.Highest;
            exportThread.Start(new Tuple<string, SilosService, SettingsService, DateTime, ExportCallbackDelegate, Progress>(filepath, silosService, settingsService, time, callback, progress));
            return true;

        }


        private static void newVersionExportFunctionForChosenTime(string filepath, SilosService silosService,
            SettingsService settingsService, DateTime time, ExportCallbackDelegate callback)
        {
            var siloses = silosService.getAllSiloses();
            var temperature = new Dictionary<int, List<string>>();
            var nowTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);

            Parallel.ForEach(siloses.Values, s =>
            {
                var tmpTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
                //List<string> temp = 

            });
        }


        private static void exportInnerThreadFunctionForChosenTime(object parameters)
        {
            //DateTime timer = DateTime.Now;

            var tuple = (Tuple<string, SilosService, SettingsService, DateTime, ExportCallbackDelegate, Progress>)parameters;
            var filepath = tuple.Item1;
            var silosService = tuple.Item2;
            var settingsService = tuple.Item3;
            var time = tuple.Item4;
            var callback = tuple.Item5;
            progress = tuple.Item6;

            var siloses = silosService.getAllSiloses();
            var temperature = new Dictionary<int, float[]>();
            var realTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);

            //DateTime timer2 =  DateTime.Now;
            /*
            Parallel.ForEach(siloses.Values, s => 
            {
                DateTime tmpTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
                foreach (Wire w in s.getSortedByNumberWires().Values)
                {
                    string[] temp = silosService.getUpperBoundTempForWireInStringFormat(w, ref tmpTime);

                    //Считаем, что это время правильное!
                    if (temp.Length > 0)
                        realTime = tmpTime;

                    temperature.Add(w.Id, temp);
                }
            });*/
            var value = 0;
            var tmpTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
            var lastTime = silosService.getTime(ref tmpTime);

            if (lastTime == DateTime.MinValue)
            {
                callback?.Invoke("Нет записей в это время!", false);
                progress?.Invoke(-2);
                return;
            }

            foreach (var s in siloses.Values)
            {
                Parallel.ForEach(s.getSortedByNumberWires().Values, w =>

                //foreach (Wire w in s.getSortedByNumberWires().Values)
                {
                    var temp = silosService.getUpperBoundTempForWireInStringFormat(w, ref lastTime);

                    //Считаем, что это время правильное!
                    if (temp.Length > 0)
                        realTime = tmpTime;

                    temperature.Add(w.Id, temp);
                });
                value += 50 / siloses.Count;
                progress?.Invoke(value);
            }

            /*
            // maclay
            foreach (Silos s in siloses.Values)
            {

                
                DateTime tmpTime = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
                //string cte = silosService.getTime(s.Wires.First().Value, ref tmpTime);

                foreach (Wire w in s.getSortedByNumberWires().Values)
                {
                    string[] temp = silosService.getUpperBoundTempForWireInStringFormat(w, ref tmpTime);

                    //Считаем, что это время правильное!
                    if (temp.Length > 0)
                        realTime = tmpTime;

                    temperature.Add(w.Id, temp);
                }
            }*/

            //MessageBox.Show((DateTime.Now - timer2).TotalSeconds.ToString());

            if (exportToExcel(filepath, silosService, settingsService, temperature, time))
            {
                progress?.Invoke(100);
                callback("Выгрузка сделана успешно!", true);
                //MessageBox.Show((DateTime.Now - timer).TotalSeconds.ToString());
            }
            else
            {
                progress?.Invoke(-2);
                callback("Не удалось сделать выгрузку в Файл. \n" +
                    "Возможно он открыт в другой программе", false);
                //MyLoger.LogError("Export error " + ex.Message);
            }
            inExport = false;
        }

        /// <summary>
        /// Делает запись в файл пследних значений температур
        /// </summary>
        /// <param name="filepath">файл</param>
        /// <param name="silosService">Сервис по работе с силосами</param>
        /// <param name="settingsService">Сервис по работе с настройками</param>
        /// <returns>Удачность</returns>
        public static bool exportToExcelLastTemperatures(string filepath, SilosService silosService, SettingsService settingsService, Progress progress)
        {

            var temperatures = silosService.getLastTemperaturesInStringFormat();
            progress?.Invoke(50);
            var time = silosService.LastTempTime;
            if (exportToExcel(filepath, silosService, settingsService, temperatures, time))
            {
                progress?.Invoke(100);
                return true;
            }
            else
            {
                progress?.Invoke(-2);
                return false;
            }
        }
        public static bool exportToExcelLastTemperatures(string filepath, SilosService silosService, SettingsService settingsService)
        {

            var temperatures = silosService.getLastTemperaturesInStringFormat();
            var time = silosService.LastTempTime;
            return exportToExcel(filepath, silosService, settingsService, temperatures, time);
        }

        /// <summary>
        /// Делает запись в файл задданных значений температур
        /// </summary>
        /// <param name="filepath">файл</param>
        /// <param name="silosService">Сервис по работе с силосами</param>
        /// <param name="settingsService">Сервис по работе с настройками</param>
        /// <param name="temperatures">Заданные температуры, ключ - ид подвески</param>
        /// <param name="time">Время выгрузки</param>
        /// <returns>Удачность</returns>
        public static bool exportToExcel(string filepath, SilosService silosService, SettingsService settingsService, Dictionary<int, float[]> temperatures, DateTime time)
        {
            try
            {
                var workbook = new XSSFWorkbook();
                var result = true;

                if (settingsService.ExcelExportType == SettingsService.EXCEL_EXPORT_LEV_TEMPLATE)
                    result = fillLevTemplate(workbook, silosService, settingsService, temperatures, time);
                else
                    result = fillStandardTemplate(workbook, silosService, settingsService, temperatures, time);

                if (!result)
                    return result;

                #region old
                /*
                 * #region create header
            int lastRowIdx = -1; //Переменная, которая указывает на индекс последней созданной стоки

            //Делаем шапку
            var row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder);
            XSSFRichTextString text = new XSSFRichTextString(settingsService.CompanyName);
            XSSFFont font = new XSSFFont();
            font.IsBold = true;
            font.FontHeightInPoints = 16;
            text.ApplyFont(font);
            row.GetCell(0).SetCellValue(text);

            row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder);
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, 10));


            sheet.CreateRow(++lastRowIdx); //Отступ
            row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder);

            sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 10));
            sheet.GetRow(lastRowIdx).GetCell(0).SetCellValue(
                "Показатели температуры продукта за " + time.ToString("dd.MM.yyyy - HH.mm"));

            sheet.CreateRow(++lastRowIdx); //Отступ
            MyLoger.Log("Create header success.");
            #endregion

            // создаем список силосов для каждого структурного подразделения.
            SortedDictionary<int, List<int>> subdivIdToSiloses = new SortedDictionary<int, List<int>>
                (Comparer<int>.Create((x, y) =>
                {
                    if (x == y)
                        return 0;
                    return x > y ? -1 : 1;
                }));

            subdivIdToSiloses.Add(-1, new List<int>()); //Силосы без подразделения                

            foreach (Silos silos in siloses.Values)
            {
                //Не имеет корректного подразделения
                if (!silosService.getSubdivisions().ContainsKey(silos.StructureId))
                {
                    subdivIdToSiloses[-1].Add(silos.Id);
                    continue;
                }

                if (!subdivIdToSiloses.ContainsKey(silos.StructureId))
                    subdivIdToSiloses.Add(silos.StructureId, new List<int>());

                subdivIdToSiloses[silos.StructureId].Add(silos.Id);
            }

            MyLoger.Log("Create subdivInSilos success.");

            //Идем по всем структурным подразделениям.
            foreach (var subdivEntry in subdivIdToSiloses)
            {
                MyLoger.Log("Subdiv " + subdivEntry.Key + " Count: " + subdivEntry.Value.Count);


            MyLoger.Log("Subdiv " + subdivEntry.Key + " Count: " + subdivEntry.Value.Count);

            row = sheet.CreateRow(++lastRowIdx); //Отступ
            row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder); //Строка с подразделением
            sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 10));
            if (subdivEntry.Key == -1)
                row.GetCell(0).SetCellValue("Силосы без структурного подразделения");
            else
                row.GetCell(0).SetCellValue("Структурное подразделение: " + silosService.getSubdivisions()[subdivEntry.Key]);

            //sheet.CreateRow(++lastRowIdx); //Отступ

            //Проходим по всем силосам в структурном подразделении
            foreach (int silosID in subdivEntry.Value)
            {
                sheet.CreateRow(++lastRowIdx); //Отступ
                MyLoger.Log("Silos id: " + silosID);
                Silos s = silosService.getSilos(silosID);
                MyLoger.Log("Success get from service, name: " + s.Name);

                int columnCount = Math.Min(s.getEnabledWireCount(), 10);
                row = createRow(sheet, ++lastRowIdx, columnCount + 1, cellStyleBorder);//Строка с силосом
                sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, columnCount));
                row.GetCell(0).SetCellValue("Силос № " + s.Name);

                List<Wire> wiresList = s.getEnabledWires();
                wiresList.Sort((w1, w2) => w1.Number.CompareTo(w2.Number));

                if (wiresList.Count == 0 || columnCount == 0) //Нет ни одной активной подвески.
                {
                    sheet.CreateRow(++lastRowIdx); //Отступ
                    row = createRow(sheet, ++lastRowIdx, 5);
                    sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 4));
                    row.GetCell(0).SetCellValue("В Силосе нет ни одной активной подвески!");
                    continue;
                }


                //Бъем на несколько частей по 10 подвесок
                for (int i = 0; i < (wiresList.Count / 10) + 1; i++)
                {
                    sheet.CreateRow(++lastRowIdx); //Отступ
                    int currentColumnCount = Math.Min(s.getEnabledWireCount() - i * 10, 10); // количество столбцов в строке.
                    row = createRow(sheet, ++lastRowIdx, currentColumnCount + 1); //Строка "номер термоподвески"
                    sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 1, currentColumnCount));
                    row.GetCell(1).SetCellValue("Номер термоподвески");
                    row.GetCell(0).SetCellValue("Номер\r\nДатчика");

                    row = createRow(sheet, ++lastRowIdx, currentColumnCount + 1); //Строка "номер датчика"
                    sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx - 1, lastRowIdx, 0, 0));

                    //считаем максимальное кол-во датчиков в данном диапазоне.
                    int maxSensor = 1;

                    for (int j = 0; j < 10; j++)
                    {
                        if (i * 10 + j >= wiresList.Count)
                            break;
                        maxSensor = Math.Max(wiresList[i * 10 + j].SensorCount, maxSensor);
                    }

                    MyLoger.Log("Fill wires numbers");
                    //Заполняем номера подвесок.
                    for (int colIdx = 0; colIdx < currentColumnCount; colIdx++)
                    {
                        Wire w = wiresList[i * 10 + colIdx];
                        row.GetCell(1 + colIdx).SetCellValue(w.Number);
                    }

                    MyLoger.Log("Fill data");
                    //Создаем строки и сразу их заполняем
                    for (int rowIdx = 0; rowIdx < maxSensor; rowIdx++)
                    {
                        MyLoger.Log("Row idx: " + rowIdx);
                        row = createRow(sheet, ++lastRowIdx, currentColumnCount + 1);
                        row.GetCell(0).SetCellValue(rowIdx + 1); // Номер сенсора
                        for (int colIdx = 1; colIdx <= currentColumnCount; colIdx++)
                        {
                            Wire w = wiresList[i * 10 + colIdx - 1];
                            MyLoger.Log("Column idx: " + colIdx + " " + w.ToString());
                            if (!temperatures.ContainsKey(w.Id) || rowIdx >= temperatures[w.Id].Length) // В этой подвеске нет такого сенсора
                            {
                                MyLoger.Log("Temperatures not contained");
                                row.GetCell(colIdx).SetCellValue("-");
                                continue;
                            }

                            if (temperatures[w.Id][rowIdx] != "-")
                                row.GetCell(colIdx).SetCellValue(Math.Round(Convert.ToSingle(temperatures[w.Id][rowIdx]), 1));
                            else
                                row.GetCell(colIdx).SetCellValue(temperatures[w.Id][rowIdx]);

                            if (settingsService.ExportIsSetColorOnCellWithTemp) //Делаем стиль, если надо                                    
                                row.GetCell(colIdx).CellStyle = colorToStyleMap[SilosService.getColor(temperatures[w.Id][rowIdx], s.Yellow, s.Red)];

                        }
                        MyLoger.Log("sucsess row");
                    }
                                }

                } //Проход по силосам в структурном подразделении
            

                sheet.CreateRow(++lastRowIdx); //Отступ
                    MyLoger.Log("Subdiv " + subdivEntry.Key + " success");
                }*/
                #endregion

                using (var fileStream = new FileStream(filepath, FileMode.Create))
                {
                    workbook.Write(fileStream);
                }
                progress?.Invoke(100);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                progress?.Invoke(-2);
                MyLoger.LogError("Export error " + ex.Message);
                return false;
            }
        }


        private static bool fillLevTemplate(XSSFWorkbook workbook, SilosService silosService, SettingsService settingsService, Dictionary<int, float[]> temperatures, DateTime time)
        {
            var siloses = silosService.getAllSiloses();

            if (siloses == null || temperatures == null)
            {
                MyLoger.Log("siloses or temperatures is null");
                return false;
            }

            var sheet = workbook.CreateSheet("Силосы");
            //уменьшаем ширину колонок
            for (var i = 1; i < 40; i++)
                sheet.SetColumnWidth(i, 45 * 256 / 7); // нужно размер умножить на 256 и разделить на 7, т.е 45 * 256 = 11520
            sheet.SetColumnWidth(2, 34 * 256 / 7);


            #region settings and styles

            var cellStyleBorder = workbook.CreateCellStyle(); // стиль с обводкой и выравниванием по центру
            cellStyleBorder.BorderBottom = BorderStyle.Thin;
            cellStyleBorder.BorderLeft = BorderStyle.Thin;
            cellStyleBorder.BorderRight = BorderStyle.Thin;
            cellStyleBorder.BorderTop = BorderStyle.Thin;
            //cellStyleBorder.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;

            var cellStyleBorderOrange = (XSSFCellStyle)workbook.CreateCellStyle(); // стиль с обводкой и выравниванием по центру
            cellStyleBorderOrange.BorderBottom = BorderStyle.Thin;
            cellStyleBorderOrange.BorderLeft = BorderStyle.Thin;
            cellStyleBorderOrange.BorderRight = BorderStyle.Thin;
            cellStyleBorderOrange.BorderTop = BorderStyle.Thin;
            cellStyleBorderOrange.Alignment = HorizontalAlignment.Center;
            cellStyleBorderOrange.SetFillForegroundColor(new XSSFColor(Color.Gold));
            cellStyleBorderOrange.FillPattern = FillPattern.SolidForeground;


            var cellStyleCenter = workbook.CreateCellStyle(); // стиль с обводкой и выравниванием по центру
            cellStyleCenter.Alignment = HorizontalAlignment.Center;

            var redCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            redCellStyle.SetFillForegroundColor(new XSSFColor(Color.OrangeRed));
            redCellStyle.FillPattern = FillPattern.SolidForeground;
            var yllowCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            yllowCellStyle.SetFillForegroundColor(new XSSFColor(Color.Yellow));
            yllowCellStyle.FillPattern = FillPattern.SolidForeground;
            var greenCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            greenCellStyle.SetFillForegroundColor(new XSSFColor(Color.LightGreen));
            greenCellStyle.FillPattern = FillPattern.SolidForeground;
            var blueCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            blueCellStyle.SetFillForegroundColor(new XSSFColor(Color.Blue));
            blueCellStyle.FillPattern = FillPattern.SolidForeground;
            var grayCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            grayCellStyle.SetFillForegroundColor(new XSSFColor(Color.Gray));
            grayCellStyle.FillPattern = FillPattern.SolidForeground;

            var colorToStyleMap = new Dictionary<Color, XSSFCellStyle>()
                {
                    {Color.Red, redCellStyle},
                    {Color.Yellow, yllowCellStyle},
                    {Color.LightGreen,greenCellStyle},
                    {Color.Blue, blueCellStyle},
                    {Color.Gray , grayCellStyle},
                };

            var styleMediumBorderT = workbook.CreateCellStyle();
            styleMediumBorderT.BorderTop = BorderStyle.Medium;
            styleMediumBorderT.BorderBottom = BorderStyle.Thin;
            styleMediumBorderT.BorderLeft = BorderStyle.Thin;
            styleMediumBorderT.BorderRight = BorderStyle.Thin;

            var styleMediumBorderB = workbook.CreateCellStyle();
            styleMediumBorderB.BorderTop = BorderStyle.Thin;
            styleMediumBorderB.BorderBottom = BorderStyle.Medium;
            styleMediumBorderB.BorderLeft = BorderStyle.Thin;
            styleMediumBorderB.BorderRight = BorderStyle.Thin;

            var styleMediumBorderR = workbook.CreateCellStyle();
            styleMediumBorderR.BorderTop = BorderStyle.Thin;
            styleMediumBorderR.BorderBottom = BorderStyle.Thin;
            styleMediumBorderR.BorderLeft = BorderStyle.Thin;
            styleMediumBorderR.BorderRight = BorderStyle.Medium;

            var styleMediumBorderL = workbook.CreateCellStyle();
            styleMediumBorderL.BorderTop = BorderStyle.Thin;
            styleMediumBorderL.BorderBottom = BorderStyle.Thin;
            styleMediumBorderL.BorderLeft = BorderStyle.Medium;
            styleMediumBorderL.BorderRight = BorderStyle.Thin;

            var styleMediumBorderLR = workbook.CreateCellStyle();
            styleMediumBorderLR.BorderLeft = BorderStyle.Medium;
            styleMediumBorderLR.BorderRight = BorderStyle.Medium;
            styleMediumBorderLR.Alignment = HorizontalAlignment.Center;

            var styleMediumBorderWithThinLR = workbook.CreateCellStyle();
            styleMediumBorderWithThinLR.BorderTop = BorderStyle.Thin;
            styleMediumBorderWithThinLR.BorderBottom = BorderStyle.Thin;
            styleMediumBorderWithThinLR.BorderLeft = BorderStyle.Medium;
            styleMediumBorderWithThinLR.BorderRight = BorderStyle.Medium;
            styleMediumBorderWithThinLR.Alignment = HorizontalAlignment.Center;

            //Правая граница для силоса с одной или ни одной подвеской
            var styleMediumBorderTBR = workbook.CreateCellStyle();
            styleMediumBorderTBR.BorderTop = BorderStyle.Medium;
            styleMediumBorderTBR.BorderBottom = BorderStyle.Medium;
            styleMediumBorderTBR.BorderLeft = BorderStyle.Medium;
            styleMediumBorderTBR.BorderRight = BorderStyle.Thin;
            styleMediumBorderTBR.Alignment = HorizontalAlignment.Center;

            //Верхняя граница для столбца с номерами подвесок и макс температурой
            var styleMediumBorderTRL = workbook.CreateCellStyle();
            styleMediumBorderTRL.BorderTop = BorderStyle.Medium;
            styleMediumBorderTRL.BorderBottom = BorderStyle.Thin;
            styleMediumBorderTRL.BorderLeft = BorderStyle.Medium;
            styleMediumBorderTRL.BorderRight = BorderStyle.Medium;
            styleMediumBorderTRL.Alignment = HorizontalAlignment.Center;

            //Нижняя граница для столбца с номерами подвесок и макс температурой
            var styleMediumBorderBRL = workbook.CreateCellStyle();
            styleMediumBorderBRL.BorderTop = BorderStyle.Thin;
            styleMediumBorderBRL.BorderBottom = BorderStyle.Medium;
            styleMediumBorderBRL.BorderLeft = BorderStyle.Medium;
            styleMediumBorderBRL.BorderRight = BorderStyle.Medium;
            styleMediumBorderBRL.Alignment = HorizontalAlignment.Center;

            // левый верхний угол
            var styleMediumBorderTL = workbook.CreateCellStyle();
            styleMediumBorderTL.BorderTop = BorderStyle.Medium;
            styleMediumBorderTL.BorderLeft = BorderStyle.Medium;
            styleMediumBorderTL.Alignment = HorizontalAlignment.Center;

            //левый нижний угол
            var styleMediumBorderBL = workbook.CreateCellStyle();
            styleMediumBorderBL.BorderBottom = BorderStyle.Medium;
            styleMediumBorderBL.BorderLeft = BorderStyle.Medium;
            styleMediumBorderBL.Alignment = HorizontalAlignment.Center;

            //правый верхний угол
            var styleMediumBorderTR = workbook.CreateCellStyle();
            styleMediumBorderTR.BorderTop = BorderStyle.Medium;
            styleMediumBorderTR.BorderRight = BorderStyle.Medium;

            //правый нижний угол
            var styleMediumBorderBR = workbook.CreateCellStyle();
            styleMediumBorderBR.BorderBottom = BorderStyle.Medium;
            styleMediumBorderBR.BorderRight = BorderStyle.Medium;

            // Для номера подвесок в случае отсутствия подвесок
            var cellStyleBorderMediumOrangeTB = (XSSFCellStyle)workbook.CreateCellStyle(); // стиль с обводкой и выравниванием по центру
            cellStyleBorderMediumOrangeTB.BorderTop = BorderStyle.Medium;
            cellStyleBorderMediumOrangeTB.BorderBottom = BorderStyle.Medium;
            cellStyleBorderMediumOrangeTB.BorderLeft = BorderStyle.Thin;
            cellStyleBorderMediumOrangeTB.BorderRight = BorderStyle.Thin;
            cellStyleBorderMediumOrangeTB.Alignment = HorizontalAlignment.Center;
            cellStyleBorderMediumOrangeTB.SetFillForegroundColor(new XSSFColor(Color.Gold));
            cellStyleBorderMediumOrangeTB.FillPattern = FillPattern.SolidForeground;

            // Для номера подвесок в случае одной подвески
            var styleMediumBorderTB = workbook.CreateCellStyle();
            styleMediumBorderTB.BorderTop = BorderStyle.Medium;
            styleMediumBorderTB.BorderBottom = BorderStyle.Medium;
            styleMediumBorderTB.BorderLeft = BorderStyle.Thin;
            styleMediumBorderTB.BorderRight = BorderStyle.Thin;
            styleMediumBorderTB.Alignment = HorizontalAlignment.Center;


            #endregion

            var lastRowIdx = -1; //Переменная, которая указывает на индекс последней созданной стоки

            //Делаем шапку
            var row = createRow(sheet, ++lastRowIdx, 2);
            row.GetCell(1).SetCellValue(settingsService.CompanyName);

            #region create list of silos for subdiv
            // создаем список силосов для каждого структурного подразделения.
            var subdivIdToSiloses = new SortedDictionary<int, List<int>>
                (Comparer<int>.Create((x, y) =>
                {
                    if (x == y)
                        return 0;
                    return x > y ? -1 : 1;
                }));

            subdivIdToSiloses.Add(-1, new List<int>()); //Силосы без подразделения                

            foreach (var silos in siloses.Values)
            {
                //Не имеет корректного подразделения
                if (!silosService.getSubdivisions().ContainsKey(silos.StructureId))
                {
                    subdivIdToSiloses[-1].Add(silos.Id);
                    continue;
                }

                if (!subdivIdToSiloses.ContainsKey(silos.StructureId))
                    subdivIdToSiloses.Add(silos.StructureId, new List<int>());

                subdivIdToSiloses[silos.StructureId].Add(silos.Id);
            }

            //MyLoger.Log("Create subdivInSilos success.");
            #endregion

            var sensorsShift = 4; // насколько смещены сенсоры, т.е. пустой столбец, силос и максимальная температура

            //Идем по всем структурным подразделениям.
            foreach (var subdivEntry in subdivIdToSiloses)
            {
                if (subdivEntry.Value.Count == 0)
                    continue;

                //MyLoger.Log("Subdiv " + subdivEntry.Key + " Count: " + subdivEntry.Value.Count);

                //row = sheet.CreateRow(++lastRowIdx); //Отступ
                row = createRow(sheet, ++lastRowIdx, 7); //Строка с подразделением и датой
                row.GetCell(1).SetCellValue(time.ToString("Дата: dd.MM.yyyy"));

                //sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 10));
                if (subdivEntry.Key == -1)
                    row.GetCell(4).SetCellValue("Силосы без структурного подразделения");
                else
                    row.GetCell(4).SetCellValue(silosService.getSubdivisions()[subdivEntry.Key].ToString());

                row = createRow(sheet, ++lastRowIdx, 4); //Строка с временем
                row.GetCell(1).SetCellValue(time.ToString("Время: hh:mm"));

                //Ищем макссимальное число датчиков
                var maxSensorCountInSubdiv = 1;
                foreach (var silosID in subdivEntry.Value)
                {
                    var s = silosService.getSilos(silosID);
                    foreach (var w in s.getEnabledWires())
                    {
                        maxSensorCountInSubdiv = Math.Max(w.SensorCount, maxSensorCountInSubdiv);
                    }
                }

                var columnCount = maxSensorCountInSubdiv + sensorsShift; // + пустой столбец, силос и максимальная температура.

                row = createRow(sheet, ++lastRowIdx, columnCount / 2 + 1); //Строка "датчик"
                row.GetCell(columnCount / 2).SetCellValue("Датчик");
                row = createRow(sheet, ++lastRowIdx, columnCount); //Строка c заголовками
                row.GetCell(3).SetCellValue("max t °С");


                for (var i = 0; i < maxSensorCountInSubdiv; i++) // строка с номерами сенсоров
                {
                    row.GetCell(sensorsShift + i).SetCellValue(i + 1);
                    row.GetCell(sensorsShift + i).CellStyle = cellStyleCenter;
                }

                //Проходим по всем силосам в структурном подразделении
                foreach (var silosID in subdivEntry.Value)
                {
                    var firstRowIdx = lastRowIdx + 1;
                    var s = silosService.getSilos(silosID);

                    var wiresList = s.getEnabledWires();
                    wiresList.Sort((w1, w2) => w1.Number.CompareTo(w2.Number));

                    if (wiresList.Count == 0) //Нет ни одной активной подвески.
                    {
                        //row = createRow(sheet, ++lastRowIdx, columnCount);
                        row = createRow(sheet, ++lastRowIdx, columnCount, 4, columnCount, cellStyleBorderOrange);
                        //row = createRowWithOwnStyle(workbook, sheet, ++lastRowIdx, columnCount);
                        //setCellStyleThinBorder(row, 1, columnCount);
                        row.GetCell(1).SetCellValue(s.Name);
                        row.GetCell(2).SetCellValue(1);
                        row.GetCell(4).SetCellValue("Нет");
                        row.GetCell(3).CellStyle = cellStyleBorder;
                    }
                    else
                    {
                        for (var wireIdx = 0; wireIdx < wiresList.Count; wireIdx++)
                        {
                            //row = createRow(sheet, ++lastRowIdx, columnCount);
                            row = createRow(sheet, ++lastRowIdx, columnCount, 2, columnCount, cellStyleBorder);

                            //row = createRowWithOwnStyle(workbook, sheet, ++lastRowIdx, columnCount);
                            //setCellStyleThinBorder(row, 2, columnCount);

                            if (wireIdx == wiresList.Count / 2)// середина.
                                row.GetCell(1).SetCellValue(s.Name);

                            var w = wiresList[wireIdx];
                            row.GetCell(2).SetCellValue(w.Number);
                            //Заполняем температуры
                            for (var sensorIdx = 0; sensorIdx < maxSensorCountInSubdiv; sensorIdx++)
                            {
                                var colIdx = sensorIdx + 4;
                                //MyLoger.Log("Column idx: " + colIdx + " " + w.ToString());
                                if (!temperatures.ContainsKey(w.Id) || sensorIdx >= temperatures[w.Id].Length) // В этой подвеске нет такого сенсора
                                {
                                    //MyLoger.Log("Temperatures not contained");
                                    row.GetCell(colIdx).SetCellValue("-");
                                    continue;
                                }

                                if (temperatures[w.Id][sensorIdx] != -100)
                                    row.GetCell(colIdx).SetCellValue(Math.Round(temperatures[w.Id][sensorIdx], 1));
                                else
                                    row.GetCell(colIdx).SetCellValue(temperatures[w.Id][sensorIdx]);

                                //if (settingsService.ExportIsSetColorOnCellWithTemp)  // НЕ Делаем стиль даже если надо                                    
                                //row.GetCell(colIdx).CellStyle = colorToStyleMap[SilosService.getColor(temperatures[w.Id][sensorIdx], s.Yellow, s.Red)];

                            }
                        } // for wire
                    } //else


                    //Применяем стили, непонятно как
                    #region apply style again
                    if (wiresList.Count <= 1)
                    {
                        row = sheet.GetRow(firstRowIdx);
                        row.GetCell(1).CellStyle = styleMediumBorderTBR;
                        if (wiresList.Count == 1) row.GetCell(2).CellStyle = styleMediumBorderTB;
                        else row.GetCell(2).CellStyle = cellStyleBorderMediumOrangeTB;
                        row.GetCell(3).CellStyle = styleMediumBorderTB;
                    }
                    else
                    {
                        //Врерхняя толстая полоска
                        row = sheet.GetRow(firstRowIdx);
                        for (var i = 1; i < columnCount; i++)
                        {
                            row.GetCell(i).CellStyle = styleMediumBorderT;
                        }

                        // Нижняя толстая полоска
                        row = sheet.GetRow(lastRowIdx);
                        for (var i = 1; i < columnCount; i++)
                        {
                            row.GetCell(i).CellStyle = styleMediumBorderB;
                        }


                        // Вертикальные толстые полоски                    
                        for (var i = firstRowIdx; i <= lastRowIdx; i++)
                        {
                            row = sheet.GetRow(i);
                            row.GetCell(1).CellStyle = styleMediumBorderLR;
                            row.GetCell(2).CellStyle = styleMediumBorderWithThinLR;
                            row.GetCell(3).CellStyle = styleMediumBorderWithThinLR;
                            row.GetCell(columnCount - 1).CellStyle = styleMediumBorderR;
                        }
                        /**/

                        //углы и крышки
                        row = sheet.GetRow(firstRowIdx);
                        row.GetCell(1).CellStyle = styleMediumBorderTL;
                        row.GetCell(2).CellStyle = styleMediumBorderTRL;
                        row.GetCell(3).CellStyle = styleMediumBorderTRL;
                        row.GetCell(columnCount - 1).CellStyle = styleMediumBorderTR;


                        row = sheet.GetRow(lastRowIdx);
                        row.GetCell(1).CellStyle = styleMediumBorderBL;
                        row.GetCell(2).CellStyle = styleMediumBorderBRL;
                        row.GetCell(3).CellStyle = styleMediumBorderBRL;
                        row.GetCell(columnCount - 1).CellStyle = styleMediumBorderBR;

                    }

                    /*
                    if (wiresList.Count == 1)
                    {
                        var style = workbook.CreateCellStyle();
                        style.BorderLeft = BorderStyle.Thin;
                        style.BorderTop = BorderStyle.Thin;
                        style.BorderBottom = BorderStyle.Thin;
                        row = sheet.GetRow(firstRowIdx);
                        row.GetCell(1).CellStyle = style;
                        continue;
                    }
                    else
                    {
                        var style = workbook.CreateCellStyle();
                        style.BorderTop = BorderStyle.Thin;
                        style.BorderLeft = BorderStyle.Thin;
                        sheet.GetRow(firstRowIdx).GetCell(1).CellStyle = style;


                        style = workbook.CreateCellStyle();
                        style.BorderLeft = BorderStyle.Thin;
                        for (int i = firstRowIdx + 1; i < lastRowIdx; i++)
                        {
                            sheet.GetRow(i).GetCell(1).CellStyle = style;
                        }

                        style = workbook.CreateCellStyle();
                        style.BorderBottom = BorderStyle.Thin;
                        style.BorderLeft = BorderStyle.Thin;
                        sheet.GetRow(lastRowIdx).GetCell(1).CellStyle = style;
                    }
                    */
                    //var t = workbook.CreateCellStyle();
                    /*
                    //


                    var cworkbook.CreateCellStyle();
                    //Врерхняя толстая полоска
                    row = sheet.GetRow(firstRowIdx);
                    for (int i = 1; i < columnCount; i++)
                    {
                        var old = row.GetCell(i).CellStyle;
                        //var style = copyStyle(workbook, row.GetCell(i).CellStyle); //workbook.CreateCellStyle();

                        //style.BorderTop = BorderStyle.Medium;
                        row.GetCell(i).CellStyle = style;
                        row.GetCell(i).CellStyle.BorderTop = BorderStyle.Medium;
                    }
                    
                    // Нижняя толстая полоска
                    row = sheet.GetRow(lastRowIdx);
                    for (int i = 1; i < columnCount; i++)
                    {
                        var style = copyStyle(workbook, row.GetCell(i).CellStyle);
                        style.BorderBottom = BorderStyle.Medium;
                        row.GetCell(i).CellStyle = style;
                    }

                    int[] idxs = { 0, 1, 3, columnCount - 1 };
                    
                    // Вертикальные толстые полоски                    
                    for (int i = firstRowIdx; i <= lastRowIdx; i++)
                    {
                        row = sheet.GetRow(i);
                        for (int j = 0; j < idxs.Length; j++)
                        {
                            var style = copyStyle(workbook, row.GetCell(j).CellStyle);
                            style.BorderRight = BorderStyle.Medium;
                            row.GetCell(j).CellStyle = style;
                        }
                    }
                    */
                    #endregion


                } //Проход по силосам в структурном подразделении


                sheet.CreateRow(++lastRowIdx); //Отступы
                sheet.CreateRow(++lastRowIdx); //Отступы
                sheet.CreateRow(++lastRowIdx); //Отступы
                sheet.CreateRow(++lastRowIdx); //Отступы

                MyLoger.Log("Subdiv " + subdivEntry.Key + " success");
            }

            return true;
        }

        private static bool fillStandardTemplate(XSSFWorkbook workbook, SilosService silosService, SettingsService settingsService, Dictionary<int, float[]> temperatures, DateTime time)
        {
            var siloses = silosService.getAllSiloses();
            if (siloses == null || temperatures == null)
            {
                MyLoger.Log("siloses or temperatures is null");
                MessageBox.Show("error");
                return false;
            }

            var sheet = workbook.CreateSheet("Силосы");

            #region settings and styles
            var cellStyleBorder = workbook.CreateCellStyle(); // стиль с обводкой и выравниванием по центру
            cellStyleBorder.BorderBottom = BorderStyle.Thin;
            cellStyleBorder.BorderLeft = BorderStyle.Thin;
            cellStyleBorder.BorderRight = BorderStyle.Thin;
            cellStyleBorder.BorderTop = BorderStyle.Thin;
            cellStyleBorder.Alignment = HorizontalAlignment.Center;
            cellStyleBorder.VerticalAlignment = VerticalAlignment.Center;

            var redCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            redCellStyle.SetFillForegroundColor(new XSSFColor(Color.OrangeRed));
            redCellStyle.FillPattern = FillPattern.SolidForeground;
            var yllowCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            yllowCellStyle.SetFillForegroundColor(new XSSFColor(Color.Yellow));
            yllowCellStyle.FillPattern = FillPattern.SolidForeground;
            var greenCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            greenCellStyle.SetFillForegroundColor(new XSSFColor(Color.LightGreen));
            greenCellStyle.FillPattern = FillPattern.SolidForeground;
            var blueCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            blueCellStyle.SetFillForegroundColor(new XSSFColor(Color.Blue));
            blueCellStyle.FillPattern = FillPattern.SolidForeground;
            var grayCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            grayCellStyle.SetFillForegroundColor(new XSSFColor(Color.Gray));
            grayCellStyle.FillPattern = FillPattern.SolidForeground;

            var colorToStyleMap = new Dictionary<Color, XSSFCellStyle>()
                {
                    {Color.Red, redCellStyle},
                    {Color.Yellow, yllowCellStyle},
                    {Color.LightGreen,greenCellStyle},
                    {Color.Blue, blueCellStyle},
                    {Color.Gray , grayCellStyle},
                };


            #endregion
            #region create header
            var lastRowIdx = -1; //Переменная, которая указывает на индекс последней созданной стоки

            //Делаем шапку
            var row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder);
            var text = new XSSFRichTextString(settingsService.CompanyName);
            var font = new XSSFFont();
            font.IsBold = true;
            font.FontHeightInPoints = 16;
            text.ApplyFont(font);
            row.GetCell(0).SetCellValue(text);

            row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder);
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, 10));


            sheet.CreateRow(++lastRowIdx); //Отступ
            row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder);

            sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 10));
            sheet.GetRow(lastRowIdx).GetCell(0).SetCellValue(
                "Показатели температуры продукта за " + time.ToString("dd.MM.yyyy - HH:mm"));

            sheet.CreateRow(++lastRowIdx); //Отступ
            MyLoger.Log("Create header success.");
            #endregion

            // создаем список силосов для каждого структурного подразделения.
            var subdivIdToSiloses = new SortedDictionary<int, List<int>>
                (Comparer<int>.Create((x, y) =>
                {
                    if (x == y)
                        return 0;
                    return x > y ? -1 : 1;
                }));

            subdivIdToSiloses.Add(-1, new List<int>()); //Силосы без подразделения                
            float value = 50 / siloses.Count;
            float sumProgress = 0;
            foreach (var silos in siloses.Values)
            {
                sumProgress = sumProgress + value;
                progress?.Invoke(50 + Convert.ToInt32(Math.Round(sumProgress, 0)));
                //Не имеет корректного подразделения
                if (!silosService.getSubdivisions().ContainsKey(silos.StructureId))
                {
                    subdivIdToSiloses[-1].Add(silos.Id);
                    continue;
                }

                if (!subdivIdToSiloses.ContainsKey(silos.StructureId))
                    subdivIdToSiloses.Add(silos.StructureId, new List<int>());

                subdivIdToSiloses[silos.StructureId].Add(silos.Id);
            }

            MyLoger.Log("Create subdivInSilos success.");

            //Идем по всем структурным подразделениям.
            foreach (var subdivEntry in subdivIdToSiloses)
            {
                MyLoger.Log("Subdiv " + subdivEntry.Key + " Count: " + subdivEntry.Value.Count);

                MyLoger.Log("Subdiv " + subdivEntry.Key + " Count: " + subdivEntry.Value.Count);

                row = sheet.CreateRow(++lastRowIdx); //Отступ
                row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder); //Строка с подразделением
                sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 10));
                if (subdivEntry.Key == -1)
                    row.GetCell(0).SetCellValue("Силосы без структурного подразделения");
                else
                    row.GetCell(0).SetCellValue("Структурное подразделение: " + silosService.getSubdivisions()[subdivEntry.Key]);

                //sheet.CreateRow(++lastRowIdx); //Отступ

                //Проходим по всем силосам в структурном подразделении
                foreach (var silosID in subdivEntry.Value)
                {
                    sheet.CreateRow(++lastRowIdx); //Отступ
                    MyLoger.Log("Silos id: " + silosID);
                    var s = silosService.getSilos(silosID);
                    MyLoger.Log("Success get from service, name: " + s.Name);

                    var columnCount = s.getEnabledWireCount();
                    row = createRow(sheet, ++lastRowIdx, columnCount + 1, cellStyleBorder);//Строка с силосом
                    sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, columnCount));
                    row.GetCell(0).SetCellValue("Силос № " + s.Name);

                    var wiresList = s.getEnabledWires();
                    wiresList.Sort((w1, w2) => w1.Number.CompareTo(w2.Number));

                    if (wiresList.Count == 0 || columnCount == 0) //Нет ни одной активной подвески.
                    {
                        sheet.CreateRow(++lastRowIdx); //Отступ
                        row = createRow(sheet, ++lastRowIdx, 5);
                        sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 4));
                        row.GetCell(0).SetCellValue("В Силосе нет ни одной активной подвески!");
                        continue;
                    }


                    //Бъем на несколько частей по 10 подвесок
                    /* for (int i = 0; i < (wiresList.Count / 10) + 1; i++)
                     {*/
                    //int currentColumnCount = Math.Min(s.getEnabledWireCount() - i * 10, 10); // количество столбцов в строке.
                    var currentColumnCount = s.getEnabledWireCount();
                    if (currentColumnCount == 0)
                        break;

                    sheet.CreateRow(++lastRowIdx); //Отступ
                    row = createRow(sheet, ++lastRowIdx, currentColumnCount + 1); //Строка "номер термоподвески"
                    sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 1, currentColumnCount));
                    row.GetCell(1).SetCellValue("Номер термоподвески");
                    row.GetCell(0).SetCellValue("Номер\r\nДатчика");

                    row = createRow(sheet, ++lastRowIdx, currentColumnCount + 1); //Строка "номер датчика"
                    sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx - 1, lastRowIdx, 0, 0));

                    //считаем максимальное кол-во датчиков в данном диапазоне.
                    var maxSensor = 1;

                    for (var j = 0; j < currentColumnCount; j++)
                    {
                        if (j >= wiresList.Count)
                            break;
                        maxSensor = Math.Max(wiresList[j].SensorCount, maxSensor);
                    }

                    MyLoger.Log("Fill wires numbers");
                    //Заполняем номера подвесок.
                    for (var colIdx = 0; colIdx < currentColumnCount; colIdx++)
                    {
                        var w = wiresList[colIdx];
                        row.GetCell(1 + colIdx).SetCellValue(w.Number);
                    }

                    MyLoger.Log("Fill data");

                    //Создаем строки и сразу их заполняем
                    //for (int rowIdx = 0; rowIdx < maxSensor; rowIdx++)
                    for (var rowIdx = maxSensor - 1; rowIdx >= 0; --rowIdx)
                    {
                        MyLoger.Log("Row idx: " + rowIdx);
                        row = createRow(sheet, ++lastRowIdx, currentColumnCount + 1);
                        row.GetCell(0).SetCellValue(rowIdx + 1); // Номер сенсора
                        for (var colIdx = 1; colIdx <= currentColumnCount; colIdx++)
                        {
                            var w = wiresList[colIdx - 1];
                            //MessageBox.Show(w.Id.ToString());
                            MyLoger.Log("Column idx: " + colIdx + " " + w.ToString());

                            if (!temperatures.ContainsKey(w.Id) || rowIdx >= temperatures[w.Id].Length) // В этой подвеске нет такого сенсора
                            {
                                MyLoger.Log("Temperatures not contained");
                                row.GetCell(colIdx).SetCellValue("-");
                                continue;
                            }
                            //MessageBox.Show(temperatures[w.Id][rowIdx].ToString());
                            if (!temperatures[w.Id][rowIdx].Equals(-100))
                            {
                                row.GetCell(colIdx).SetCellValue(Math.Round(temperatures[w.Id][rowIdx], 1));
                            }
                            else
                            {
                                // MessageBox.Show("tut");
                                row.GetCell(colIdx).SetCellValue("-");
                            }
                            if (settingsService.ExportIsSetColorOnCellWithTemp) //Делаем стиль, если надо                                    
                                row.GetCell(colIdx).CellStyle = colorToStyleMap[SilosService.getColor(row.GetCell(colIdx).ToString(), s.Yellow, s.Red)];

                        }
                        MyLoger.Log("sucsess row");
                    }
                    //}

                } //Проход по силосам в структурном подразделении


                sheet.CreateRow(++lastRowIdx); //Отступ
                MyLoger.Log("Subdiv " + subdivEntry.Key + " success");
            }
            return true;
        }

        //перегруженный метод без вывода логов
        //проверка времени работы 
        private static bool fillStandardTemplate(XSSFWorkbook workbook, SilosService silosService, SettingsService settingsService, Dictionary<int, float[]> temperatures, DateTime time, bool flag)
        {
            var siloses = silosService.getAllSiloses();
            if (siloses == null || temperatures == null)
            {
                return false;
            }

            var sheet = workbook.CreateSheet("Силосы");

            #region settings and styles
            var cellStyleBorder = workbook.CreateCellStyle(); // стиль с обводкой и выравниванием по центру
            cellStyleBorder.BorderBottom = BorderStyle.Thin;
            cellStyleBorder.BorderLeft = BorderStyle.Thin;
            cellStyleBorder.BorderRight = BorderStyle.Thin;
            cellStyleBorder.BorderTop = BorderStyle.Thin;
            cellStyleBorder.Alignment = HorizontalAlignment.Center;
            cellStyleBorder.VerticalAlignment = VerticalAlignment.Center;

            var redCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            redCellStyle.SetFillForegroundColor(new XSSFColor( Color.OrangeRed) );
            redCellStyle.FillPattern = FillPattern.SolidForeground;
            var yllowCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            yllowCellStyle.SetFillForegroundColor(new XSSFColor(Color.Yellow));
            yllowCellStyle.FillPattern = FillPattern.SolidForeground;
            var greenCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            greenCellStyle.SetFillForegroundColor(new XSSFColor(Color.LightGreen));
            greenCellStyle.FillPattern = FillPattern.SolidForeground;
            var blueCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            blueCellStyle.SetFillForegroundColor(new XSSFColor(Color.Blue));
            blueCellStyle.FillPattern = FillPattern.SolidForeground;
            var grayCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
            grayCellStyle.SetFillForegroundColor(new XSSFColor(Color.Gray));
            grayCellStyle.FillPattern = FillPattern.SolidForeground;

            var colorToStyleMap = new Dictionary<Color, XSSFCellStyle>()
                {
                    {Color.Red, redCellStyle},
                    {Color.Yellow, yllowCellStyle},
                    {Color.LightGreen,greenCellStyle},
                    {Color.Blue, blueCellStyle},
                    {Color.Gray , grayCellStyle},
                };


            #endregion
            #region create header
            var lastRowIdx = -1; //Переменная, которая указывает на индекс последней созданной стоки

            //Делаем шапку
            var row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder);
            var text = new XSSFRichTextString(settingsService.CompanyName);
            var font = new XSSFFont();
            font.IsBold = true;
            font.FontHeightInPoints = 16;
            text.ApplyFont(font);
            row.GetCell(0).SetCellValue(text);

            row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder);
            sheet.AddMergedRegion(new CellRangeAddress(0, 1, 0, 10));


            sheet.CreateRow(++lastRowIdx); //Отступ
            row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder);

            sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 10));
            sheet.GetRow(lastRowIdx).GetCell(0).SetCellValue(
                "Показатели температуры продукта за " + time.ToString("dd.MM.yyyy - HH:mm"));

            sheet.CreateRow(++lastRowIdx); //Отступ
            MyLoger.Log("Create header success.");
            #endregion

            // создаем список силосов для каждого структурного подразделения.
            var subdivIdToSiloses = new SortedDictionary<int, List<int>>
                (Comparer<int>.Create((x, y) =>
                {
                    if (x == y)
                        return 0;
                    return x > y ? -1 : 1;
                }));

            subdivIdToSiloses.Add(-1, new List<int>()); //Силосы без подразделения                

            foreach (var silos in siloses.Values)
            {
                //Не имеет корректного подразделения
                if (!silosService.getSubdivisions().ContainsKey(silos.StructureId))
                {
                    subdivIdToSiloses[-1].Add(silos.Id);
                    continue;
                }

                if (!subdivIdToSiloses.ContainsKey(silos.StructureId))
                    subdivIdToSiloses.Add(silos.StructureId, new List<int>());

                subdivIdToSiloses[silos.StructureId].Add(silos.Id);
            }



            //Идем по всем структурным подразделениям.
            foreach (var subdivEntry in subdivIdToSiloses)
            {
                row = sheet.CreateRow(++lastRowIdx); //Отступ
                row = createRow(sheet, ++lastRowIdx, 11, cellStyleBorder); //Строка с подразделением
                sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 10));
                if (subdivEntry.Key == -1)
                    row.GetCell(0).SetCellValue("Силосы без структурного подразделения");
                else
                    row.GetCell(0).SetCellValue("Структурное подразделение: " + silosService.getSubdivisions()[subdivEntry.Key]);

                //sheet.CreateRow(++lastRowIdx); //Отступ

                //Проходим по всем силосам в структурном подразделении
                foreach (var silosID in subdivEntry.Value)
                {
                    sheet.CreateRow(++lastRowIdx); //Отступ
                    var s = silosService.getSilos(silosID);

                    var columnCount = s.getEnabledWireCount();
                    row = createRow(sheet, ++lastRowIdx, columnCount + 1, cellStyleBorder);//Строка с силосом
                    sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, columnCount));
                    row.GetCell(0).SetCellValue("Силос № " + s.Name);

                    var wiresList = s.getEnabledWires();
                    wiresList.Sort((w1, w2) => w1.Number.CompareTo(w2.Number));

                    if (wiresList.Count == 0 || columnCount == 0) //Нет ни одной активной подвески.
                    {
                        sheet.CreateRow(++lastRowIdx); //Отступ
                        row = createRow(sheet, ++lastRowIdx, 5);
                        sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 0, 4));
                        row.GetCell(0).SetCellValue("В Силосе нет ни одной активной подвески!");
                        continue;
                    }


                    //Бъем на несколько частей по 10 подвесок
                    /* for (int i = 0; i < (wiresList.Count / 10) + 1; i++)
                     {*/
                    //int currentColumnCount = Math.Min(s.getEnabledWireCount() - i * 10, 10); // количество столбцов в строке.
                    var currentColumnCount = s.getEnabledWireCount();
                    if (currentColumnCount == 0)
                        break;

                    sheet.CreateRow(++lastRowIdx); //Отступ
                    row = createRow(sheet, ++lastRowIdx, currentColumnCount + 1); //Строка "номер термоподвески"
                    sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx, lastRowIdx, 1, currentColumnCount));
                    row.GetCell(1).SetCellValue("Номер термоподвески");
                    row.GetCell(0).SetCellValue("Номер\r\nДатчика");

                    row = createRow(sheet, ++lastRowIdx, currentColumnCount + 1); //Строка "номер датчика"
                    sheet.AddMergedRegion(new CellRangeAddress(lastRowIdx - 1, lastRowIdx, 0, 0));

                    //считаем максимальное кол-во датчиков в данном диапазоне.
                    var maxSensor = 1;

                    for (var j = 0; j < currentColumnCount; j++)
                    {
                        if (j >= wiresList.Count)
                            break;
                        maxSensor = Math.Max(wiresList[j].SensorCount, maxSensor);
                    }


                    //Заполняем номера подвесок.
                    for (var colIdx = 0; colIdx < currentColumnCount; colIdx++)
                    {
                        var w = wiresList[colIdx];
                        row.GetCell(1 + colIdx).SetCellValue(w.Number);
                    }



                    //Создаем строки и сразу их заполняем
                    for (var rowIdx = 0; rowIdx < maxSensor; rowIdx++)
                    {

                        row = createRow(sheet, ++lastRowIdx, currentColumnCount + 1);
                        row.GetCell(0).SetCellValue(rowIdx + 1); // Номер сенсора
                        for (var colIdx = 1; colIdx <= currentColumnCount; colIdx++)
                        {
                            var w = wiresList[colIdx - 1];
                            //MessageBox.Show(w.Id.ToString());


                            if (!temperatures.ContainsKey(w.Id) || rowIdx >= temperatures[w.Id].Length) // В этой подвеске нет такого сенсора
                            {

                                row.GetCell(colIdx).SetCellValue("-");
                                continue;
                            }
                            //MessageBox.Show(temperatures[w.Id][rowIdx].ToString());
                            if (!temperatures[w.Id][rowIdx].Equals(-100))
                            {
                                row.GetCell(colIdx).SetCellValue(Math.Round(temperatures[w.Id][rowIdx], 1));
                            }
                            else
                            {
                                // MessageBox.Show("tut");
                                row.GetCell(colIdx).SetCellValue("-");
                            }
                            if (settingsService.ExportIsSetColorOnCellWithTemp) //Делаем стиль, если надо                                    
                                row.GetCell(colIdx).CellStyle = colorToStyleMap[SilosService.getColor(row.GetCell(colIdx).ToString(), s.Yellow, s.Red)];

                        }

                    }
                    //}

                } //Проход по силосам в структурном подразделении


                sheet.CreateRow(++lastRowIdx); //Отступ
            }
            return true;
        }

    }

}
