using OxyPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemOfTermometry2.Model;

public static class GrainColor
{
    public static Dictionary<int, OxyColor> colorGrain = new Dictionary<int, OxyColor>()
    {

        { 1, OxyColors.Red },
        { 2, OxyColors.Blue},
        { 3, OxyColors.Green},
        { 4, OxyColors.Yellow},
        { 5, OxyColors.Orange},
        { 6, OxyColors.Violet},
        { 7, OxyColors.Cyan},
        { 8, OxyColors.Magenta},
        { 9, OxyColors.Pink},
        { 10, OxyColors.Purple},
        { 11, OxyColors.SeaGreen},
        { 12, OxyColors.YellowGreen},
        { 13, OxyColors.DarkOrange},
        { 14, OxyColors.DarkSlateBlue},
        { 0, OxyColors.DarkCyan},
        { -1, OxyColors.Black}

    };
}
