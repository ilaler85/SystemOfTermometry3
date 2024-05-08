using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using SystemOfTermometry2.WinUIWorker;

namespace SystemOfTermometry2.CustomComponent;
public class PresentationLayerClass: IPresentationLayer
{
    private Window window;

    public PresentationLayerClass(Window window)
    {
        this.window = window;

    }

}
