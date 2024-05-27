using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SystemOfThermometry3.Model;
using SystemOfThermometry3.WinUIWorker;
using GrainItem = SystemOfThermometry3.CustomComponent.GrainComponent.GrainComponent;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SystemOfThermometry3.CustomComponent.Setting_components;


/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class GrainSetting : Page
{
    Dictionary<int, Grain> grains;
    List<GrainItem> grainComponents;
    IBisnesLogicLayer bll;

    public GrainSetting()
    {
        this.InitializeComponent();


    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e != null)
            bll = e.Parameter as IBisnesLogicLayer;

        grains = bll.getGrains();
        foreach (int item in grains.Keys)
        {
            GrainItem grainItem = new GrainItem(grains[item]);
            grainItem.save += updateGrain;
            grainItem.delete += deleteGrain;

            grainComponents.Add(grainItem);
            stackGrain.Children.Add(grainItem);
        }

        
    }

    private void ButtonNewGrain_Click(object sender, RoutedEventArgs e)
    {
        Grain newGrain = bll.addGrain();
        GrainItem item = new GrainItem(newGrain);
        item.delete += deleteGrain;
        item.save += updateGrain;
        grains.Add(newGrain.ID, newGrain);
        grainComponents.Add(item);
    }

    public void updateGrain(Grain grain)
    {
        bll.updateGrain(grain);
    }

    public void deleteGrain(Grain grain, object sender)
    {
        grainComponents.RemoveAt(grainComponents.IndexOf(sender as GrainItem));  
        bll.deleteGrain(grain);
        
    }
}
