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
using System.ComponentModel;
using SystemOfThermometry3.CustomComponent.SettingComponents;
using SystemOfThermometry3.CustomComponent.Setting_components;
using SystemOfThermometry3.Services;
using SystemOfThermometry3.WinUIWorker;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingComponent : Page
{
    private IBisnesLogicLayer bll;
    private Type pageType;
    public SettingComponent()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e != null)
        {
            bll = (IBisnesLogicLayer)e.Parameter;
            changeModeSetting();
        }
    }

    public void changeModeSetting()
    {
        if (!bll.isAdminMode())
            initNormalSettingMode();
        else
            initAdminSettingMode();
    }

    private void initNormalSettingMode()
    {
        DBConnect.IsEnabled = false;
        SettingProvider.IsEnabled = false;
        GrainSetting.IsEnabled = false;
        SilosSetting.IsEnabled = false;
        StructuralDivisions.IsEnabled = false;
        OfflineMode.IsEnabled = false;
        But.IsEnabled = true;
    }

    private void initAdminSettingMode()
    {
        DBConnect.IsEnabled = true;
        SettingProvider.IsEnabled = true;
        GrainSetting.IsEnabled = true;
        SilosSetting.IsEnabled = true;
        StructuralDivisions.IsEnabled = true;
        OfflineMode.IsEnabled = true;
        But.IsEnabled = false;
    }


    private void settingNavigat_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        FrameNavigationOptions navOptions = new FrameNavigationOptions();
        navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;

        if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Left)
        {
            navOptions.IsNavigationStackEnabled = false;
        }

         //init
        var selectedItem = (NavigationViewItem)args.SelectedItem;
        settingNavigat.Header = selectedItem.Content;
        
        switch (selectedItem.Name)
        {
            case "GeneralSetting":
                pageType = typeof(GeneralSetting);
                
                break;
            case "BoardParameters":
                pageType = typeof(BoardParameters);
                break;

            case "SilosSetting":
                pageType = typeof(SilosSetting);
                break;

            case "GrainSetting":
                pageType = typeof(GrainSetting);
                break;

            case "OfflineMode":
                pageType = typeof(OfflineMode);
                break;

            case "DBConnect":
                pageType = typeof(DBConnect);
                break;
            case "MailSetting":
                pageType = typeof(MailParameters);
                break;

            case "SettingProvider":
                pageType = typeof(SettingProvider);
                break;

            case "StructuralDivisions":
                pageType = typeof(StructuralDivisions);
                break;

            default: pageType = typeof(GeneralSetting); break;
        }
        _ = contentFrame.Navigate(pageType, bll);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if(bll.enterAdminMode())
            initAdminSettingMode();
    }
}
