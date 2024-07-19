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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EnterPassword : Page
    {
        public EnterPassword()
        {
            this.InitializeComponent();
        }

        private void revealModeCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void revealModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private void RevealModeCheckbox_Changed(object sender, RoutedEventArgs e)
        {
            if (revealModeCheckBox.IsChecked == true)
            {
                passworBoxWithRevealmode.PasswordRevealMode = PasswordRevealMode.Visible;
            }
            else
            {
                passworBoxWithRevealmode.PasswordRevealMode = PasswordRevealMode.Hidden;
            }
        }
    }
}
