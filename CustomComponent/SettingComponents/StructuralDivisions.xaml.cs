using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SystemOfThermometry3.CustomComponent.SettingComponents;
using SystemOfThermometry3.WinUIWorker;



namespace SystemOfThermometry3.CustomComponent.Setting_components;
public sealed partial class StructuralDivisions : Microsoft.UI.Xaml.Controls.Page
{
    IBisnesLogicLayer bll;
    Dictionary<int, string> divisions;
    int selectItem=-1;
    
    public StructuralDivisions()
    {
        this.InitializeComponent();
    }
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e != null)
            bll = e.Parameter as IBisnesLogicLayer;

        divisions = bll.getListDivision();
        listDivisions.ItemsSource = divisions.Values;
    }
    

    private void ListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        nameDivisions.Text = listDivisions.SelectedItem.ToString();
        ButtonDelete.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        selectItem = divisions.FirstOrDefault(x => x.Value == listDivisions.SelectedItem.ToString()).Key;
    }

    private void ButtonDelete_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        bll.deleteDivision(selectItem);
        divisions.Remove(selectItem);
        listDivisions.ItemsSource = divisions;
    }

    private void ButtonSave_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (selectItem == -1)
        {
            selectItem = bll.addDivision(nameDivisions.Text);
            divisions.Add(selectItem, nameDivisions.Text);
            listDivisions.ItemsSource = divisions;
            listDivisions.SelectedIndex = 0; 
            selectItem = -1;
        }
        else
        {
            bll.updateDivision(selectItem, nameDivisions.Text);
            divisions[selectItem] = nameDivisions.Text;
            listDivisions.ItemsSource = divisions; 
            listDivisions.SelectedIndex = 0; 
            selectItem = -1;
        }
    }
}
