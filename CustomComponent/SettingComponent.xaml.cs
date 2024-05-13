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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingComponent : Page
{
    public SettingComponent()
    {
        this.InitializeComponent();
    }

    private void settingNavigat_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        FrameNavigationOptions navOptions = new FrameNavigationOptions();
        navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;

        if (sender.PaneDisplayMode == NavigationViewPaneDisplayMode.Left)
        {
            navOptions.IsNavigationStackEnabled = false;
        }

        Type pageType; //init
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

            default: pageType = typeof(BoardParameters); break;
        }
        _ = contentFrame.Navigate(pageType);
    }


}
