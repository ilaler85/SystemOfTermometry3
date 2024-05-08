using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemOfTermometry2.CustomComponent;

namespace SystemOfTermometry2.Services;
public static class MessageBox
{



    public static void Show(string message) 
    {
        var box = new CustomComponent.MessageBox(message);
        box.Activate();
    }
}
