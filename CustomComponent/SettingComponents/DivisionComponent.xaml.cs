using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfTermometry2.CustomComponent.SettingComponents;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class DivisionComponent : Page
{
    public delegate void DeleteDelegate(int id);
    public event DeleteDelegate deleteItem;
    public DivisionComponent()
    {
        this.InitializeComponent();
    }

    public void butDelete_Click(object sender, RoutedEventArgs e)
    {
        deleteItem.Invoke(1);
    }

    private void StackPanel_GettingFocus(UIElement sender, GettingFocusEventArgs args)
    {
        butDelete.Visibility = Visibility.Visible;
    }

    private void StackPanel_LosingFocus(UIElement sender, LosingFocusEventArgs args)
    {
        butDelete.Visibility = Visibility.Collapsed;
    }
}
