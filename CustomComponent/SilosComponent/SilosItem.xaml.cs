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
using SystemOfThermometry3.Model;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CustomComponent.SilosComponent
{
    public sealed partial class SilosItem : UserControl
    {

        private IBisnesLogicLayer bll;
        private Silos silos;
        public SilosItem(IBisnesLogicLayer logic, Silos currentSilos)
        {
            this.InitializeComponent();
            bll = logic;
            silos = currentSilos;

        }

        public void setParametrsSilos()
        {
            if (silos.W == 1)
            {
                
            }
        }

        private void setMiniSilos()
        {
            MinTempLabel.Visibility = Visibility.Collapsed;
            MaxTempLabel.Visibility = Visibility.Collapsed;
            //silosImage.Source = ;
        }

        private string getColorSilos()
        {
            return "";
            //if(silos.Max =) 
        }
    }
}
