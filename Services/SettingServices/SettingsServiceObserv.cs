using System;

namespace SystemOfThermometry2.Services
{
    partial class SettingsService
    {
        /// <summary>
        /// Минимальное время между итерациями опроса плат в секундах (может быть больше, если не успеет опросить)
        /// </summary>
        public uint ObserveIterationPeriod
        {
            get => Convert.ToUInt32(getProperty("observ_iteration_preiod", "300"));
            set => setProperty("observ_iteration_preiod", value.ToString());
        }

        /// <summary>
        /// Количество попыток опросить сломанную подвеску 
        /// </summary>
        public uint ObserveTriesToObserveBrokenWire
        {
            get => Convert.ToUInt32(getProperty("observ_tries_to_observe_broken_wire", "3"));
            set => setProperty("observ_tries_to_observe_broken_wire", value.ToString());
        }

        /// <summary>
        /// Если true, то сломанная подвеска будет через некоторое время снова опрашиваться
        /// </summary>
        public bool ObserveIsGiveBrokenWireSecondChanse
        {
            get => Convert.ToBoolean(getProperty("observ_is_give_second_chanse", false.ToString()));
            set => setProperty("observ_is_give_second_chanse", value.ToString());
        }

        /// <summary>
        /// Время, через которое сломанная подвеска будет снова опроашиваться 
        /// </summary>
        public uint ObserveBrokenWireTimeOutMilisec
        {
            get => Convert.ToUInt32(getProperty("observ_broken_wire_timeout", "36000000"));
            set => setProperty("observ_broken_wire_timeout", value.ToString());
        }

        public bool IsStartObservAutomatically
        {
            get => Convert.ToBoolean(getProperty("observ_is_automatically", false.ToString()));
            set => setProperty("observ_is_automatically", value.ToString());
        }

    }
}
