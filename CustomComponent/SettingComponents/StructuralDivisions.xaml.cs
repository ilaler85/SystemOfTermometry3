using System.Collections.Generic;
using System.Windows;
using Microsoft.UI.Xaml.Controls;
using SystemOfTermometry2.CustomComponent.SettingComponents;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfTermometry2.CustomComponent.Setting_components;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class StructuralDivisions : Microsoft.UI.Xaml.Controls.Page
{
    public StructuralDivisions()
    {
        this.InitializeComponent();
    }

    private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        DivisionComponent item = new DivisionComponent();
        item.deleteItem += deleteDivision;
        ListDivision.Children.Add(item);
    }
    public void deleteDivision(int id)
    {
      
        ListDivision.Children.RemoveAt(ListDivision.Children.Count-1);
            
        
    }
}
