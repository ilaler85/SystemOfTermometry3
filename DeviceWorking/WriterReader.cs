using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SystemOfThermometry3.Model;
using SystemOfThermometry3.Services;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.DeviceWorking
{
    /// <summary>
    /// Класс, который в бесконечном цикле опрашивает платы и пишет данные в базу данных.
    /// В конце каждой итерации по всем платам шлет сообщение, что итерация завершена.
    /// Также посылает различного рода сообщения об ошибках.
    /// </summary>
    class WriterReader
    {
        public delegate void MessageDelegate(string message);
        public MessageDelegate asyncMessage; //Некоторое сообщение, которое не должно останавливать опрос
        public MessageDelegate errorMessage; //Ошибка, напримет, связанная с открытием порта
        public MessageDelegate criticalErrorMessage; //Сообщение об ошибке, после которой останавливается опрос

        public MessageDelegate beginIterationEvent; //Сообщение о начале итерации
        public MessageDelegate endIterationEvent; //Сообщение о конце итерации
        public delegate void ProgressDelegate(int progress);
        public event ProgressDelegate progressEvent;
        /// <summary>
        /// Делегат, который представляет собой сообщение об окончании итерации
        /// и посылает собранные данные.
        /// </summary>
        /// <param name="siloses">Словарь. Ключ - id силоса, значение - силос</param>
        /// <param name="tempertures">СловарьСловарь. Ключ - id подвески, значение - массив температур</param>
        public delegate void SendDataDelegate(Dictionary<int, Silos> siloses, Dictionary<int, Dictionary<int, float[]>> tempertures);
        public SendDataDelegate sendDataEvent; //Сообщение, передающие данные о температурах

        private DeviceObserver observer; //Опросчик плат
        private SilosService silosService; //Сервис для получения силосов
        Dictionary<int, Silos> siloses; //Силосы.
        private SettingsService settingsService; //Сервис для получения настроек

        private HashSet<int> brokenWires; //Подвески, на которыйх все сенсоры сломаны или которые принадлежат неработающим платам
        private Dictionary<int, int> wiresBrokeTimes; //Количество раз, сколько подвеска сломалась
        private Dictionary<int, int> wiresTimeToRepairLeft; //Время, которое осталось до того, как подвеска снова бедет опрашиваться
        //private int maximumBrokenWireTimes = 5; //Максимальное количество раз, сколько подвеска может сломаться до того, как будет игнорироваться

        private DateTime startIterationTime; // Время начала итерации

        private bool needToStop; //Флаг, говорящий о том, что требуется остановка опроса.
        private bool stoped; //Флаг, говорящий о том, что опрос остановлен или будет остановлен в ближайшее время. т.е. опросов уже не будет.
        private bool awaiting; //Флаг, говорящий о том, что процесс опроса ждет до следующего итерации.

        private Thread observationThread; //Поток, в котором выполняются итерации
        private int iterationPeriod; // Время между итерациями в милисекундах (может быть больше, если не успеет опросить) 
        private int dataBaseErrorCount; //Количество ошибок бд за итерацию. не используется
        //private bool was_data_base_error; // При первой ошибке выводим сообщение, при последующих - нет

        public bool IsRunning
        {
            get
            {
                return !stoped;
            }
        }


        public WriterReader(SilosService silosService, SettingsService settingsService)
        {
            observer = new ModbusRTUObserver();
            this.silosService = silosService ?? throw new ArgumentNullException("Silos service is null");
            this.settingsService = settingsService ?? throw new ArgumentNullException("Settings service is null");

            brokenWires = new HashSet<int>();
            wiresBrokeTimes = new Dictionary<int, int>();
            wiresTimeToRepairLeft = new Dictionary<int, int>();
            stoped = true;
            needToStop = true;
            awaiting = false;
        }

        /// <summary>
        /// Начинает опрос плат
        /// </summary>
        /// <returns>true - все хорошо. false - опрос не начался, возникла ошибка</returns>
        public bool startAsyncObservation()
        {
            iterationPeriod = 1000 * (int)settingsService.ObserveIterationPeriod;
            siloses = silosService.getAllSiloses();
            if (siloses == null || siloses.Count == 0)
            {
                errorMessage?.Invoke("Нет силосов для опроса!");
                MyLoger.LogError("WriterReader. Нет силосов для опроса!");
                return false;
            }

            var result = observer.ConnectComPort(
                settingsService.PortComPort, settingsService.PortBaudRate,
                settingsService.PortParity, settingsService.PortDataBits,
                settingsService.PortStopBits, settingsService.PortTimeOut);

            switch (result)
            {
                case PortExceptionEnum.PortNotExist:
                    observer.DisconnectFromPort();
                    errorMessage?.Invoke("Порт не существует");
                    return false;
                case PortExceptionEnum.PortIsUsing:
                    observer.DisconnectFromPort();
                    errorMessage?.Invoke("Порт уже используется");
                    return false;
                case PortExceptionEnum.PortWrongName:
                    observer.DisconnectFromPort();
                    errorMessage?.Invoke("Неверное имя порта");
                    return false;
                case PortExceptionEnum.PortUnknowingError:
                    observer.DisconnectFromPort();
                    errorMessage?.Invoke("Неизвестная ошибка при открытии порта");
                    return false;
                case PortExceptionEnum.PortOpenSuccessfully:
                    try
                    {
                        needToStop = false;
                        stoped = false;
                        awaiting = false;
                        observationThread = new Thread(runObservation);
                        observationThread.Start();
                        asyncMessage?.Invoke("Порт успешно открыт");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        criticalErrorMessage("Не удалось запустить поток опроса платы: " + ex.Message);
                        return false;
                    }

                default:
                    return false;
            }

        }

        public void stopAsyncObservation()
        {
            needToStop = true;
            progressEvent?.Invoke(-2);
            if (awaiting) // процесс ждет, можно сразу все закрыть, больше опроса не будет
            {

                stoped = true;
                observationThread.Abort();
            }
            else //Процесс опрашивает, нужно подождать, пока он завершит опрос.
            {
                //int maxCount = 0;
                while (!stoped)// && maxCount < 10)
                {
                    Thread.Sleep(200);
                    //maxCount++;
                }
            }

            closePort();
            MyLoger.LogError("WriterReader. Stop observ!");
        }

        private void closePort()
        {
            var result = observer.DisconnectFromPort();
            switch (result)
            {
                case PortExceptionEnum.PortCloseSuccessfully:
                    //sendMessage("Порт успешно закрыт");
                    break;
                case PortExceptionEnum.PortCloseError:
                    //sendMessage("Не удалось закрыть");
                    break;
            }
        }

        private void runObservation()
        {
            MyLoger.LogError("WriterReader. Strat observ");
            var str = "";
            foreach (var s in silosService.getAllSiloses().Values)
            {
                str += "\r\nSilos " + s.Name + " id " + s.Id;
                foreach (var w in s.Wires.Values)
                    str += "\r\n   " + w.ToString();
            }
            MyLoger.LogError(str);

            needToStop = false;
            stoped = false;

            try
            {
                brokenWires.Clear();
                wiresBrokeTimes.Clear();

                while (!needToStop)
                {
                    if (!observer.IsPortOpen)
                    {
                        MyLoger.LogError("Port is closed. Try to reopen");
                        var result = observer.ConnectComPort(
                            settingsService.PortComPort, settingsService.PortBaudRate,
                            settingsService.PortParity, settingsService.PortDataBits,
                            settingsService.PortStopBits, settingsService.PortTimeOut);
                        if (result != PortExceptionEnum.PortOpenSuccessfully)
                        {
                            MyLoger.LogError("Unable to reopen port!");
                        }
                    }

                    startIterationTime = DateTime.Now;
                    beginIterationEvent?.Invoke("Итерация началась.");
                    progressEvent?.Invoke(-1);
                    dataBaseErrorCount = 0;

                    awaiting = false;
                    iterate();

                    if (needToStop)//Прервалось, ждать не надо                    
                        break;

                    var endTime = DateTime.Now;
                    var observTime = (int)(endTime - startIterationTime).TotalMilliseconds;
                    var sleepTime = Math.Max(0, iterationPeriod - observTime);

                    endIterationEvent?.Invoke("Итерация закончилась. Занятое время: "
                        + (observTime / 1000.0f).ToString() + " секунд." //);
                        + "\r\nДо следующего опроса " + (sleepTime / 1000.0f).ToString() + " секунд.");

                    progressEvent?.Invoke(100);
                    awaiting = true;
                    Thread.Sleep(sleepTime);
                    repairWires(iterationPeriod);

                }
            }
            catch (ThreadAbortException ex) // исключение, вызываемое, когда поток прерывается. Вызывается для прерывания ждущего потока.
            {
                return;
            }
            catch (Exception ex)
            {
                MyLoger.LogError("WR: Unknown Error " + ex.Message);
                needToStop = true;
                stoped = true;
                stopAsyncObservation();
                criticalErrorMessage?.Invoke("Завершение опроса, вызванное исключением: " + ex.Message);
            }
            stoped = true;
            closePort();
        }



        /// <summary>
        /// Итерация по всем силосам
        /// </summary>
        private void iterate()
        {
            var iterateTime = DateTime.Now;
            var wireCount = 0; //Количество плат, которые удалось опросить
            var resultTemperatures = new Dictionary<int, Dictionary<int, float[]>>();
            float progress = 100 / silosService.getAllSiloses().Count;
            float sumprogress = 0;
            foreach (var silos in silosService.getAllSiloses().Values)
            {
                if (needToStop) return; //Изменение флага асинхронно, нужно постоянно проверять

                float max = -99, min = 150, temperatureSum = 0; // минимальная и максимальная температура
                var sensorCount = 0;// и переменные, необходимые для подсчета средней температуры
                var wiresProgress = progress / silos.Wires.Count;//считаем какую часть состовляет опрос 1 подвески от количества подвесок в силосе
                var k = 1;
                foreach (var wireItem in silos.Wires)
                {
                    if (needToStop) return; //Изменение флага асинхронно, нужно постоянно проверять

                    var wire = wireItem.Value;
                    // если плата отключена или оказалась сломанной или у нее не задано кол-во сенсоров
                    if (!wire.Enable || brokenWires.Contains(wire.Id) || wire.SensorCount == 0)
                        continue; //то ее не опрашиваем                    
                    else
                        wireCount++;

                    float[] data;
                    try //Попытка опросить. Мы нумеруем ножки с 1, а плата с 0.
                    {
                        var leg = (ushort)Math.Max(wire.Leg - 1, 0);
                        data = observer.GetData(wire.DeviceAddress, leg, wire.SensorCount);

                        //Форматируем в зависимости от типа подвески.
                        data = formatData(data, wire.Type);

                    }
                    catch (Exception e)
                    {
                        var msg = string.Format("Неудалось опросить подвеску." +
                            "\nCилос: {0}, номер подвески: {1}, адрес платы: {2}, номер ножки {3}." +
                            "\nОшибка: {4}",
                            silos.Name, wire.Number, wire.DeviceAddress, wire.Leg, e.Message);

                        if (needToStop) return;

                        asyncMessage?.Invoke(msg);
                        brokeWire(wire, e.Message);

                        //зануляем последнюю температуру
                        silosService.addTemperatures(wire, new float[0], DateTime.Now);
                        progressEvent?.Invoke(Convert.ToInt32(Math.Round(wiresProgress * k + sumprogress, 0)));
                        ++k;

                        continue;
                    }

                    var brokenSensors = 0;
                    for (var i = 0; i < data.Length; i++)
                    {
                        if (data[i] < -90) //сенсор не опрошен
                        {
                            brokenSensors++;
                            continue;
                        }

                        sensorCount++;
                        temperatureSum += data[i];
                        max = Math.Max(max, data[i]);
                        min = Math.Min(min, data[i]);
                    }

                    if (brokenSensors == wire.SensorCount)
                    {
                        wireCount--;
                        if (needToStop) return;
                        asyncMessage?.Invoke(string.Format("Неудалось опросить подвеску." +
                            "\nНи один сенсор не дал верных результатов." +
                            "\n Cилос: {0}, номер подвески:{3}, адрес платы: {1}, Номер ножки {2}",
                            silos.Name, wire.DeviceAddress, wire.Leg, wire.Number));

                        //зануляем последнюю температуру
                        silosService.addTemperatures(wire, new float[0], iterateTime);
                        brokeWire(wire, "All sensors is broken");
                        if (needToStop) return;
                    }
                    else //Хотябы один сенсор есть
                    {
                        if (!silosService.addTemperatures(wire, data, iterateTime))
                        {
                            if (needToStop) { progressEvent?.Invoke(-2); return; }
                            asyncMessage?.Invoke("Не удалось записать значения температур в базу данных.\n" +
                                "Остановите опрос и проверьте подключение к базе данных.\n" +
                                "Последующие значения будут записываться в файл " + settingsService.temperatureFilePath);
                            dataBaseErrorCount++;
                        }

                    }
                    progressEvent?.Invoke(Convert.ToInt32(Math.Round(wiresProgress * k + sumprogress, 0)));
                    ++k;
                    if (needToStop)
                    {
                        progressEvent?.Invoke(-2);
                        return;
                    } //Изменение флага асинхронно, нужно постоянно проверять
                }

                //Обновление силоса
                // Ниодного сенсора опрошено небыло - не обновляем
                if (sensorCount == 0 || max < -90 || min > 140)
                {
                    MyLoger.LogError("WR: SensorCount = 0. Silos: " + silos.ToString());
                    continue;
                }

                silos.Max = max;
                silos.Mid = temperatureSum / sensorCount;
                silos.Min = min;

                silosService.updateSilos(silos);
                silosService.addFilling(silos);
                if (needToStop)
                {
                    progressEvent?.Invoke(-2);
                    return;
                }//Изменение флага асинхронно, нужно постоянно проверять
                sumprogress += progress;
            }

            //Прекращеник опроса, так как небыло опрошено ниодной платы
            if (wireCount == 0 && !needToStop)
            {
                MyLoger.LogError("WR: No wires observed. Stop observ");
                criticalErrorMessage?.Invoke("Небыло опрошено ни одной платы!\n Прекращение опроса.");
                needToStop = true;
                return;
            }

            //sendDataEvent?.Invoke(siloses, resultTemperatures);
        }

        /// <summary>
        /// Вызывается, когда подвеска, предположительно, сломана и принмает решение -
        /// добавить ее в список исключаемых или нет
        /// </summary>
        /// <param name="id"></param>
        private void brokeWire(Wire wire, string e)
        {
            if (!wiresBrokeTimes.ContainsKey(wire.Id))
                wiresBrokeTimes.Add(wire.Id, 0);

            wiresBrokeTimes[wire.Id]++;
            //MyLoger.Log(e, "WR: wire is broken: " + wire.ToString() +". Count of brokes: " + wiresBrokeTimes[wire.Id]);
            MyLoger.LogError("[" + e + "] WR: wire is broken: " + wire.ToString() + ". Count of brokes: " + wiresBrokeTimes[wire.Id]);
            if (wiresBrokeTimes[wire.Id] > settingsService.ObserveTriesToObserveBrokenWire)
            {
                brokenWires.Add(wire.Id);
                if (needToStop) return;
                try
                {
                    if (needToStop) return;
                    asyncMessage?.Invoke(string.Format("Подвеска с силосе {0} с номером {1} больше не будет опрашиваться",
                        silosService.getSilos(wire.SilosId), wire.Number));
                }
                catch
                {
                    if (needToStop) return;
                    asyncMessage?.Invoke(string.Format("Подвеска с адресом платы {0} на ножке {1} больше не будет опрашиваться",
                        wire.DeviceAddress, wire.Leg));
                }
                if (settingsService.ObserveIsGiveBrokenWireSecondChanse)
                    if (wiresTimeToRepairLeft.ContainsKey(wire.Id))
                        wiresTimeToRepairLeft[wire.Id] = (int)settingsService.ObserveBrokenWireTimeOutMilisec;
                    else
                        wiresTimeToRepairLeft.Add(wire.Id, (int)settingsService.ObserveBrokenWireTimeOutMilisec);
            }

        }

        /// <summary>
        /// Попытка сделать подвеску снова опрашиваемой, если это необходимо
        /// </summary>
        /// <param name="milisec">Время с начала итерации в милисекундах</param>
        private void repairWires(int milisec)
        {
            if (!settingsService.ObserveIsGiveBrokenWireSecondChanse)
                return;
            try
            {
                foreach (var id in brokenWires.ToArray())
                {
                    if (wiresTimeToRepairLeft.ContainsKey(id))
                    {
                        wiresTimeToRepairLeft[id] -= milisec;
                        if (wiresTimeToRepairLeft[id] > 0)
                            continue;

                        if (wiresBrokeTimes.ContainsKey(id))
                            wiresBrokeTimes[id] = 0;

                        brokenWires.Remove(id);
                        foreach (var s in silosService.getAllSiloses().Values)
                        {

                            if (s.Wires.ContainsKey(id))
                            {
                                if (needToStop) return;

                                asyncMessage?.Invoke(string.Format("Подвеска в силосе {0}, c номером {1} будет снова опрашиваться!",
                                    s.Name, s.Wires[id].Number));
                                break;
                            }
                        }

                    }
                    else
                        wiresTimeToRepairLeft.Add(id, (int)settingsService.ObserveBrokenWireTimeOutMilisec);
                }
            }
            catch (Exception e)
            {
                MyLoger.LogError("WR: repair wire error: " + e.Message);
            }
        }


        private float[] formatData(float[] data, WireTypeEnum type)
        {
            switch (type)
            {
                case WireTypeEnum.TOP_TO_BOT_DS18b20:
                    break;
                case WireTypeEnum.BOT_TO_TOP_DS18b20:
                    for (var i = 0; i < data.Length / 2; i++)
                    {
                        var tmp = data[i];
                        data[i] = data[data.Length - i - 1];
                        data[data.Length - i - 1] = tmp;
                    }
                    break;
                case WireTypeEnum.TOP_TO_BOT_DS1820:
                    for (var i = 0; i < data.Length; i++)
                        data[i] *= 10;
                    break;
                case WireTypeEnum.BOT_TO_TOP_DS1820:
                    for (var i = 0; i < data.Length / 2; i++)
                    {
                        var tmp = data[i];
                        data[i] = data[data.Length - i - 1];
                        data[data.Length - i - 1] = tmp;
                    }

                    for (var i = 0; i < data.Length; i++)
                        data[i] *= 10;
                    break;
                default:
                    break;
            }
            return data;
        }
    }
}
