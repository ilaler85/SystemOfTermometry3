using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.WindowsForms;
using SystemOfThermometry3.CustomComponent.SilosComponent;
using SystemOfThermometry3.Model;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.WinUIWorker;
public partial class WinUIWorker : IBisnesLogicLayer
{


    public PlotModel getChart(int modeTime, IEnumerable<Wire> list)
    {
        PlotModel plotModel = null;
        switch (modeTime)
        {
            case 1:
                plotModel = PlotingService.buildPlotOneDayN(settingsService, silosService, list, -1, presentation.setProgressBar);
                break;
            case 2:
                plotModel = PlotingService.buildPlotWeekN(settingsService, silosService, list, presentation.setProgressBar);
                break;
            case 3:
                plotModel = PlotingService.buildPlotMonthN(settingsService, silosService, list, presentation.setProgressBar);
                break;
            case 4:
                plotModel = PlotingService.buildPlotYearN(settingsService, silosService, list, presentation.setProgressBar);
                break;
        }
        return plotModel;
    }

    public PlotModel getChart(IEnumerable<Wire> list, DateTime start, DateTime end)
    {
        PlotModel plotModel = null;
        plotModel = PlotingService.buildPlotCustomN(settingsService, silosService, list, start, end, presentation.setProgressBar);
        return plotModel;
    }

    public PlotModel getChart(IEnumerable<Silos> list, DateTime start, DateTime end)
    {
        PlotModel plotModel = null;

        plotModel = PlotingService.plottingFilling(settingsService, silosService, , start, end, presentation.setProgressBar);

        return plotModel;
    }
}
