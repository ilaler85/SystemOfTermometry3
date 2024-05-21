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
using SystemOfThermometry3.Services;
using Windows.Foundation;
using Windows.Foundation.Collections;
using CommunityToolkit.WinUI.UI.Controls;
using SystemOfThermometry3.WinUIWorker;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent.Setting_components;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MailParameters : Page
{

    private IBisnesLogicLayer bll;

    public delegate void SaveMail(string SMTPServer, string mailAdddress, string password, string themeMail,
        Dictionary<string, string> destinations, bool paintChart, bool sendMailTime, int? hour);
    public event SaveMail save;
    public event SaveMail saveAndSendTestEmail;
    public delegate void SaveErrorMail(string SMTPServer, string mailAdddress, string password, string themeMail,
        Dictionary<string, string> destinations, int periodTemp);
    public event SaveErrorMail saveError;
    public event SaveErrorMail saveErrorAndSendTestEmail;


    public MailParameters()
    {
        this.InitializeComponent();
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e == null)
            return;




        bll = e.Parameter as IBisnesLogicLayer;
        string smtp, mailAdress, password, themeMail;
        bool pinChart, SendMail;
        int timeNumericUpDown;
        bll.getSettingMail(out smtp, out mailAdress, out password, out themeMail, out pinChart, out timeNumericUpDown, out SendMail);
        BoxSMTPServer.Text = smtp;
        BoxMailAddress.Text = mailAdress;
        BoxPassword.Text = password;
        BoxThemeMail.Text = themeMail;
        CheckBoxPinCharts.IsChecked = pinChart;
        CheckBoxSendMail.IsChecked = SendMail;
        BoxTimeSend.Text = timeNumericUpDown.ToString();
        if (SendMail)
            BoxTimeSend.IsEnabled = false;

        gridMailRecipients.ItemsSource = bll.getMailAdress();



        bll.getSettingErrorMail(out smtp, out mailAdress, out password, out themeMail, out SendMail, out timeNumericUpDown);



        SwitchSendErrorMail.IsOn = SendMail;

        BoxErrorSMTPServer.Text = smtp;
        BoxErrorMailAddress.Text = mailAdress;
        BoxErrorPassword.Text = password;
        BoxThemeErrorMail.Text = themeMail;
        BoxTimeSend.Text = timeNumericUpDown.ToString();

        gridErrorMailRecipients.ItemsSource = bll.getErrorMailAdress();

        if (bll.isAdminMode())
        {
            changeEnableTrueMailSetting();

            if (SendMail)
                changeEnableTrueErrorMailSetting();
            else
                changeEnableFalseErrorMailSetting();
        }
        else
        {
            changeEnableFalseErrorMailSetting();
            changeEnableFalseMailSetting();
        }
    }


    private void changeEnableFalseMailSetting()
    {
        BoxSMTPServer.IsEnabled = false;
        BoxMailAddress.IsEnabled = false;
        BoxPassword.IsEnabled = false;
        BoxThemeMail.IsEnabled = false;
        CheckBoxPinCharts.IsEnabled = false;
        CheckBoxSendMail.IsEnabled = false;
        BoxTimeSend.IsEnabled = false;
        gridMailRecipients.IsEnabled = false;
        ButtonSave.IsEnabled = false;
        ButtonTestSendMail.IsEnabled = false;
    }

    private void changeEnableTrueMailSetting()
    {
        BoxSMTPServer.IsEnabled = true;
        BoxMailAddress.IsEnabled = true;
        BoxPassword.IsEnabled = true;
        BoxThemeMail.IsEnabled = true;
        CheckBoxPinCharts.IsEnabled = true;
        CheckBoxSendMail.IsEnabled = true;
        BoxTimeSend.IsEnabled = true;
        gridMailRecipients.IsEnabled = true;
        ButtonSave.IsEnabled = true;
        ButtonTestSendMail.IsEnabled = true;
    }

    private void changeEnableFalseErrorMailSetting()
    {
        BoxErrorSMTPServer.IsEnabled = false;
        BoxErrorMailAddress.IsEnabled = false;
        BoxErrorPassword.IsEnabled = false;
        BoxThemeErrorMail.IsEnabled = false;
        BoxTimeSend.IsEnabled = false;
        gridErrorMailRecipients.IsEnabled = false;
        ButtonSaveError.IsEnabled = false;
        ButtonTestSendErrorMail.IsEnabled = false;
    }
    private void changeEnableTrueErrorMailSetting()
    {
        BoxErrorSMTPServer.IsEnabled = true;
        BoxErrorMailAddress.IsEnabled = true;
        BoxErrorPassword.IsEnabled = true;
        BoxThemeErrorMail.IsEnabled = true;
        BoxTimeSend.IsEnabled = true;
        gridErrorMailRecipients.IsEnabled = true;
        ButtonSaveError.IsEnabled = true;
        ButtonTestSendErrorMail.IsEnabled = true;
    }

    private void SwitchSendErrorMail_Toggled(object sender, RoutedEventArgs e)
    {
        if (SwitchSendErrorMail.IsOn)
        {
            changeEnableTrueErrorMailSetting();
        }
        else
        {
            changeEnableFalseErrorMailSetting();
        }
    }

    private Dictionary<string, string> getArrayDestinations(DataGrid dataGrid)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        int count = 0;
        Dictionary<string, string> table = dataGrid.ItemsSource as Dictionary<string, string>;

        foreach (string key in table.Keys)
        {
            if (string.IsNullOrEmpty(key))
                table.Remove(key);
        }
        return table;
    }


    private void ButtonSaveError_Click(object sender, RoutedEventArgs e)
    {
        saveError.Invoke(BoxErrorSMTPServer.Text, BoxErrorMailAddress.Text, BoxErrorPassword.Text,
            BoxThemeErrorMail.Text, getArrayDestinations(gridErrorMailRecipients), Convert.ToInt32(BoxPeriodSend.Text));
    }

    private void ButtonTestSendErrorMail_Click(object sender, RoutedEventArgs e)
    {
        saveErrorAndSendTestEmail.Invoke(BoxErrorSMTPServer.Text, BoxErrorMailAddress.Text, BoxErrorPassword.Text,
            BoxThemeErrorMail.Text, getArrayDestinations(gridErrorMailRecipients), Convert.ToInt32(BoxPeriodSend.Text));
    }

    private void ButtonSave_Click(object sender, RoutedEventArgs e)
    {
        save.Invoke(BoxSMTPServer.Text, BoxMailAddress.Text, BoxPassword.Text, BoxThemeMail.Text, getArrayDestinations(gridMailRecipients),
            (bool)CheckBoxPinCharts.IsChecked, (bool)CheckBoxSendMail.IsChecked, (CheckBoxSendMail.IsChecked == true) ? Convert.ToInt32(BoxTimeSend.Text) : -1);
    }


    private void ButtonTestSendMail_Click(object sender, RoutedEventArgs e)
    {
        saveAndSendTestEmail.Invoke(BoxSMTPServer.Text, BoxMailAddress.Text, BoxPassword.Text, BoxThemeMail.Text, getArrayDestinations(gridMailRecipients),
            (bool)CheckBoxPinCharts.IsChecked, (bool)CheckBoxSendMail.IsChecked, (CheckBoxSendMail.IsChecked == true) ? Convert.ToInt32(BoxTimeSend.Text) : -1);
    }
}
