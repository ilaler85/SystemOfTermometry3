using System;
using System.IO;
using System.Text;

namespace SystemOfTermometry2.Services
{
    class MyLoger
    {
        private static object sync = new object();
        private static object syncErr = new object();
        public static void Log(string message)
        {
            try
            {
                // Путь .\\Log
                var pathToLog = Path.Combine(FileProcessingService.LogDir, "Log");
                if (!Directory.Exists(pathToLog))
                    Directory.CreateDirectory(pathToLog); // Создаем директорию, если нужно
                var filename = Path.Combine(pathToLog, string.Format("SystemOfTermometryLog_{0}_.txt",
                DateTime.Now.ToString("dd.MM.yyyy")));
                var fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] {1}\r\n",
                DateTime.Now, message);
                lock (sync)
                {
                    File.AppendAllText(filename, fullText, Encoding.GetEncoding("Windows-1251"));
                }
            }
            catch
            {
                // Перехватываем все и ничего не делаем
            }
        }

        public static void Log(Exception ex, string message = "")
        {
            try
            {
                // Путь .\\Log
                var pathToLog = Path.Combine(FileProcessingService.LogDir, "Log");
                if (!Directory.Exists(pathToLog))
                    Directory.CreateDirectory(pathToLog); // Создаем директорию, если нужно
                var filename = Path.Combine(pathToLog, string.Format("{0}_.log",
                AppDomain.CurrentDomain.FriendlyName));
                var fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] [{1}] {2}\r\n",
                DateTime.Now, ex.Message, message);
                lock (sync)
                {
                    File.AppendAllText(filename, fullText, Encoding.GetEncoding("Windows-1251"));
                }
            }
            catch (Exception inner_ex)
            {
                var str = inner_ex.Message;
                // Перехватываем все и ничего не делаем
            }
        }

        public static void LogError(string message)
        {
            try
            {
                // Путь .\\Log
                var pathToLog = Path.Combine(FileProcessingService.LogDir, "Log");
                if (!Directory.Exists(pathToLog))
                    Directory.CreateDirectory(pathToLog); // Создаем директорию, если нужно
                var filename = Path.Combine(pathToLog, "Errors.txt");
                var fullText = string.Format("[{0:dd.MM.yyy HH:mm:ss.fff}] {1}\r\n",
                DateTime.Now, message);

                Log(message); //Добавляем в обычный лог тоже

                lock (syncErr)
                {
                    File.AppendAllText(filename, fullText, Encoding.GetEncoding("Windows-1251"));
                }
            }
            catch (Exception ex)
            {
                var str = ex.Message;
                // Перехватываем все и ничего не делаем
            }
        }

    }
}
