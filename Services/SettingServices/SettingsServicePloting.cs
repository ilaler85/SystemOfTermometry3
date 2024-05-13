using System;

namespace SystemOfThermometry3.Services
{
    public partial class SettingsService
    {
        public const int IF_ONE_SILOS_SHOW_ALL_WIRES = 1;
        public const int IF_ONE_SILOS_SHOW_MIN_MID_MAX = 2;
        public const int IF_ONE_WIRE_SHOW_ALL_SENSORS = 1;
        public const int IF_ONE_WIRE_SHOW_MIN_MID_MAX = 2;

        private int showOneSilosMode = IF_ONE_SILOS_SHOW_ALL_WIRES;
        /// <summary>
        /// Показывать все подвески, если один силос.
        /// Реализация отличается в силу доступности настройки в режиме "Обзор"
        /// </summary>
        public int ShowOneSilosMode
        {
            get
            {
                if (IsOnlyReadMode)
                    return showOneSilosMode;

                return Convert.ToInt32(getProperty("show_all_wires_if_one_silos", "1"));
            }
            set {
                if (IsOnlyReadMode)
                {
                    showOneSilosMode = value;
                    return;
                }

                setProperty("show_all_wires_if_one_silos", value.ToString());
            }
        }

        private int showOneWireMode = IF_ONE_WIRE_SHOW_ALL_SENSORS;
        /// <summary>
        /// Показывать все сенсоры, если одина подвеска
        /// Реализация отличается в силу доступности настройки в режиме "Обзор"
        /// </summary>
        public int ShowOneWireMode
        {
            get
            {
                if (IsOnlyReadMode)
                    return showOneWireMode;

                return   Convert.ToInt32(getProperty("show_all_sensors_if_one_wire", "1"));

            }
            set
            {
                if (IsOnlyReadMode)
                {
                    showOneWireMode = value;
                    return;
                }

                setProperty("show_all_sensors_if_one_wire", value.ToString());
            }
        }

        /// <summary>
        /// Разрешение экспорта картинки - ширина
        /// </summary>
        public int PlotWidth
        {
            get => Convert.ToInt32(getProperty("plot_width", "800"));
            set => setProperty("plot_width", value.ToString());
        }

        /// <summary>
        /// Разрешение экспорта картинки - высота
        /// </summary>
        public int PlotHeight
        {
            get => Convert.ToInt32(getProperty("plot_height", "600"));
            set => setProperty("plot_height", value.ToString());
        }
    }
}
