using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.DeviceWorking
{
    /// <summary>
    /// Абстрактный класс, который является базовым для всех опросчиков устройств
    /// Сделан на будующие
    /// </summary>
    abstract class DeviceObserver
    {
        //Порт, через котое осуществляется общение
        protected SerialPort port = new SerialPort();

        public virtual bool IsPortOpen
        {
            get
            {
                if (port.IsOpen)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Соединяет с заданным Com портом. 
        /// Возвращает елемент перечисления
        /// </summary>
        /// <param name="r_ComPort">Порт</param>
        /// <param name="r_BaudRate">Скорость</param>
        /// <param name="r_parity">Четность</param>
        /// <param name="r_DataBits">Число бит (7,8)</param>
        /// <param name="r_StopBits">Число стоп битов</param>
        /// <param name="r_Timeout">Время до таймаута</param>
        /// <returns>Элемент перечисления ошибок порта. PortOpenSuccessfully - если удачно</returns>
        public virtual PortExceptionEnum ConnectComPort(string r_ComPort, int r_BaudRate, Parity r_parity, int r_DataBits,
                    StopBits r_StopBits, int r_Timeout)
        {
            try
            {
                if (port == null)
                    port = new SerialPort();

                port.BaudRate = r_BaudRate;
                port.PortName = r_ComPort;
                port.Parity = r_parity;
                port.DataBits = r_DataBits;
                port.StopBits = r_StopBits;
                port.ReadTimeout = r_Timeout;
                port.WriteTimeout = r_Timeout;

                port.Open();
            }
            catch (System.IO.IOException) { return PortExceptionEnum.PortNotExist; }
            catch (InvalidOperationException) { return PortExceptionEnum.PortIsUsing; }
            catch (ArgumentException) { return PortExceptionEnum.PortWrongName; }
            catch (Exception ex)
            {
                MyLoger.Log("Port exception: " + ex.Message);
                return PortExceptionEnum.PortUnknowingError;
            }

            if (port.IsOpen)
            {
                return PortExceptionEnum.PortOpenSuccessfully;
            }

            return PortExceptionEnum.PortUnknowingError;
        }

        /// <summary>
        /// Закрытие порта
        /// </summary>
        /// <returns>Экземпляр перечисления ошибок порта. PortCloseSuccessfully - если удачно</returns>
        public PortExceptionEnum DisconnectFromPort()
        {
            if (port == null)
                return PortExceptionEnum.PortCloseError;
            try
            {
                port.Close();
                if (!port.IsOpen)
                    return PortExceptionEnum.PortCloseSuccessfully;
                else
                    return PortExceptionEnum.PortCloseError;
            }
            catch
            {
                return PortExceptionEnum.PortCloseError;
            }
        }

        /// <summary>
        /// Получение данных с платы
        /// </summary>
        /// <param name="devieceAddres">Адрес устройства</param>
        /// <param name="leg">Ножка</param>
        /// <param name="sensorCount">Количество сенсоров</param>
        /// <returns>Полученные данные</returns>
        public abstract float[] GetData(byte devieceAddres, ushort leg, ushort sensorCount);

    }
}
