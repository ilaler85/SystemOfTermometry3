﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemOfThermometry3.CustomComponent;

namespace SystemOfThermometry3.Services;
public static class MessageBox
{



    public static void Show(string message) 
    {
        var box = new CustomComponent.MessageBoxWindow(message);
        box.Activate();
    }
}
