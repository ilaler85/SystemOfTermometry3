using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SystemOfThermometry3.WinUIWorker;
using System.Text;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Provider;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExportExcelComponent : Page
    {
        private IBisnesLogicLayer bll;
        private int idSilos = 0;
        private string pathFile = "";


        public ExportExcelComponent()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e != null)
                bll = (IBisnesLogicLayer)e.Parameter;
        }

        private void rbOneSilos_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void rbAllSilos_Checked(object sender, RoutedEventArgs e)
        {
        }
        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            bool flag = true;
            TimeSpan time;
            if (TimePicker.SelectedTime == null)
            {
                TipCalendar.IsOpen = true;
                flag = false;
            }
            else
                time = TimePicker.SelectedTime.Value;


            DateTime date;
            if (CalendarPicker.Date == null)
            {
                TipCalendar.IsOpen = true;
                flag = false;
            }
            else
                date = CalendarPicker.Date.Value.UtcDateTime;


            if(pathFile=="")
                flag = false;

            if (rbAllSilos.IsChecked == true && flag)
            {

                //bll.exportExcel(date);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private async void OpenFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file != null)
                filePath.Text = file.Path;
            
        }
    }
}
