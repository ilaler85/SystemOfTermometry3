using System;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.Services;

partial class SettingsService
{

    public bool IsShowTitleAgain
    {
        get
        {
            if (IsOnlyReadMode)
            {
                bool result = true;
                string str = FileProcessingService.getStringFromFile("is_show_title_again");
                if (str != "")
                    try { result = Convert.ToBoolean(str); }
                    catch { }

                return result;
            }
            else
            {
                return Convert.ToBoolean(getProperty("is_show_title_again", true.ToString()));
            }
        }
        set
        {
            if (IsOnlyReadMode)
                FileProcessingService.setStringToFile("is_show_title_again", value.ToString());
            else
                setProperty("is_show_title_again", value.ToString());
        }
    }

    /// <summary>
    /// true нужно округлять значения не считанных сенсоров по остальным, false -прочерки
    /// </summary>
    public bool IsGetMidValueForBrokenSensor
    {
        get => Convert.ToBoolean(getProperty("mid_value_for_broken_sensor", false.ToString()));
        set => setProperty("mid_value_for_broken_sensor", value.ToString());
    }

    /// <summary>
    /// Минимальное значение ширины или высоты (в зависимости от положения) журнала
    /// </summary>
    public int LogTextboxWidthHeight
    {
        get => Convert.ToInt32(getProperty("log_textbox_wh", "150"));
        set => setProperty("log_textbox_wh", value.ToString());
    }

    /// <summary>
    /// Путь к фону для силосов.
    /// </summary>
    public string SilosBackgroundFilePath
    {
        get
        {
            string path = getProperty("silos_background_file_path", "");
            if (path == "")
            {
                return path;
            }
            else
            {
                return path.Replace("$", "\\");
            }
        }
        set
        {
            setProperty("silos_background_file_path", value.Replace("\\", "$"));
        }
    }

    /// <summary>
    /// Подсвечивать, если опрос остановлен
    /// </summary>
    public bool IsHighlightWhenObservStop
    {
        get => Convert.ToBoolean(getProperty("is_highlight_when_observ_stop", false.ToString()));
        set => setProperty("is_highlight_when_observ_stop", value.ToString());
    }


    /// <summary>
    /// Прижимание значений температур при отображении одного силоса
    /// </summary>
    public bool IsPressDownTemperatures
    {
        get
        {
            if (IsOnlyReadMode)
            {
                bool result = false;
                string str = FileProcessingService.getStringFromFile("is_press_down_temp");
                if (str != "")
                    try { result = Convert.ToBoolean(str); }
                    catch { }

                return result;
            }
            else
            {
                return Convert.ToBoolean(getProperty("is_press_down_temp", false.ToString()));
            }
        }
        set
        {
            if (IsOnlyReadMode)
                FileProcessingService.setStringToFile("is_press_down_temp", value.ToString());
            else
                setProperty("is_press_down_temp", value.ToString());
        }
    }

    /// <summary>
    /// сортировка подвесок по X при отображении одного силоса
    /// </summary>
    public bool IsSortWiresX
    {
        get
        {
            if (IsOnlyReadMode)
            {
                bool result = false;
                string str = FileProcessingService.getStringFromFile("is_sort_wires_x");
                if (str != "")
                    try { result = Convert.ToBoolean(str); }
                    catch { }

                return result;
            }
            else
            {
                return Convert.ToBoolean(getProperty("is_sort_wires_x", false.ToString()));
            }
        }
        set
        {
            if (IsOnlyReadMode)
                FileProcessingService.setStringToFile("is_sort_wires_x", value.ToString());
            else
                setProperty("is_sort_wires_x", value.ToString());
        }
    }

}
