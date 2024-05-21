using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SystemOfThermometry3.WinUIWorker;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent.Setting_components;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class OfflineMode : Page
{
    IBisnesLogicLayer bll;
    public OfflineMode()
    {
        this.InitializeComponent();
    }


    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e != null)
            bll = e.Parameter as IBisnesLogicLayer;
    }

    private void offlineModeButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFile = new OpenFileDialog();
        openFile.Filter = "Файлы с конфигурацией|*.cfg|Все файлы|*.*";
        openFile.FileName = "config.cfg";
        if (openFile.ShowDialog() == DialogResult.OK)
        {
            if (openFile.FileName != "")
                bll.offlineMode(openFile.FileName);
        }
    }

    private void setOfflineModeButton_Click(object sender, RoutedEventArgs e)
    {
        bll.setOfflineMode();
    }

    private void saveSettingButton_Click(object sender, RoutedEventArgs e)
    {
        SaveFileDialog saveFile = new SaveFileDialog();
        saveFile.Filter = "Файлы с конфигурацией|*.cfg|Все файлы|*.*";
        saveFile.FileName = "";
        if (saveFile.ShowDialog() == DialogResult.OK)
        {
            if (saveFile.FileName != "")
                bll.saveToFile(saveFile.FileName);
        }

    }

    private void downloadTemperatureFromFileButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog openFile = new OpenFileDialog();
        openFile.Filter = "Файлы с температурой|*.txt|Все файлы|*.*";
        openFile.FileName = "temperature.txt";
        if (openFile.ShowDialog() == DialogResult.OK)
        {
            if (openFile.FileName != "")
                bll.loadTemperature(openFile.FileName);
        }
    }
}
