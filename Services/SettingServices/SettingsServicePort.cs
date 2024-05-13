using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemOfThermometry3.Services
{
    public partial class SettingsService
    {
        /// <summary>
        /// Имя Com порта
        /// </summary>
        public string PortComPort
        {
            get => getProperty("port_com_name", "COM");
            set => setProperty("port_com_name", value.ToString());
        }

        /// <summary>
        /// Скорость чтения прота
        /// </summary>
        public int PortBaudRate
        {
            get => Convert.ToInt32(getProperty("port_baude_rate", "9600"));
            set => setProperty("port_baude_rate", value.ToString());
        }        

        /// <summary>
        /// Четность 
        /// </summary>
        public Parity PortParity
        {
            get => (Parity)Enum.Parse(typeof(Parity), (getProperty("prot_parity", Parity.None.ToString())));
            set => setProperty("prot_parity", value.ToString());
        }

        /// <summary>
        /// Число бит (7,8)
        /// </summary>
        public int PortDataBits
        {
            get => Convert.ToInt32(getProperty("port_data_bits", "8"));
            set => setProperty("port_data_bits", value.ToString());
        }

        /// <summary>
        ///  Число стоп битов
        /// </summary>
        public StopBits PortStopBits
        {
            get => (StopBits)Enum.Parse(typeof(StopBits), getProperty("prot_stop_bits", StopBits.One.ToString()));
            set => setProperty("prot_stop_bits", value.ToString());
        }

        /// <summary>
        /// Время до таймаута
        /// </summary>
        public int PortTimeOut
        {
            get => Convert.ToInt32(getProperty("port_time_out", "1000"));
            set => setProperty("port_time_out", value.ToString());
        }


    }
}
