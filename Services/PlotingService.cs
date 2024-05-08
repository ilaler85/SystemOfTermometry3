using MySqlX.XDevAPI.Common;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SystemOfTermometry2.Model;
using SystemOfThermometry2.Services;

namespace SystemOfTermometry2.Services;

class PlotingService
{

    public delegate void reportProgressCall(int percent);

    #region new

    private static FunctionSeries[] newBuildPlotForEachSensor(SilosService silosService, Wire wireToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {
        var allTemp = silosService.getTemperatureForWireBetweenTime(wireToView, start, end);

        if (allTemp == null || allTemp.Count == 0) return null;
        var times = silosService.getSetTimeBetweenTime(start, end);
        var result = new FunctionSeries[wireToView.SensorCount];


        Parallel.For
        (0, wireToView.SensorCount, index =>
        {
            var fs = new FunctionSeries();

            foreach (var point in times)
            {
                if (!allTemp.ContainsKey(point))
                {
                    fs.Points.Add(DataPoint.Undefined);
                    continue;
                }

                fs.Points.Add(new DataPoint(DateTimeAxis.ToDouble(point), allTemp[point][index]));
            }

            result[index] = new FunctionSeries();
            result[index] = fs;
            result[index].Title = "Сеносор " + (index + 1).ToString();
            result[index].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";


        });

        return result;
    }


    private static FunctionSeries[] buildPlotForEachSensor(SilosService silosService, Wire wireToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {

        var allTemp = silosService.getTemperatureForWireBetweenTime(wireToView, start, end);
        if (allTemp == null || allTemp.Count == 0) return null;
        var arrayTime = silosService.getSetTimeBetweenTime(start, end);
        var result = new FunctionSeries[wireToView.SensorCount];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = new FunctionSeries();
            result[i].Title = "Сеносор " + (i + 1).ToString();
            result[i].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
        }

        long count = 0;
        var lastPercent = 0;
        foreach (var item in arrayTime)
        {
            if (!allTemp.ContainsKey(item))
            {
                for (var i = 0; i < result.Length; i++)
                {
                    result[i].Points.Add(DataPoint.Undefined);
                }
                continue;
            }
            var temp = allTemp[item];
            for (var i = 0; i < temp.Length; i++)
            {
                if (i >= temp.Length || temp[i] < -80)
                    continue;

                result[i].Points.Add(new DataPoint(Axis.ToDouble(temp), temp[i]));
            }
            count++;
            var percent = (int)(count * 100 / allTemp.Count);
            if (percent != lastPercent)
            {
                progressFunc?.Invoke(percent);
                lastPercent = percent;
            }
        }

        return result;
    }


    private static FunctionSeries[] nevBuildPlotForEachWire(SilosService silosService, IEnumerable<Wire> wireToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {
        var built = false;
        var wireCount = wireToView.Count();
        var result = new FunctionSeries[wireCount];
        var arrayTime = silosService.getSetTimeBetweenTime(start, end);
        long count = 0;
        var lastPercent = 0;
        var wireIdx_ = 0;
        Parallel.ForEach(wireToView, w =>
        {
            var wireIdx = wireIdx_++;
            if (w.SilosId != -1)
            {
                result[wireIdx] = new FunctionSeries();
                result[wireIdx].Title = string.Format("Подвеска {0}. Плата: {1}. Ножка: {2}",
                    w.Number, w.DeviceAddress, w.Leg);
                result[wireIdx].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
                var allTemp = silosService.getAVGTemperatureSilosBetweenTime(w, start, end);
                if (allTemp != null && allTemp.Count != 0) built = true;
                count = 0;
                foreach (var time in arrayTime)
                {
                    if (!allTemp.ContainsKey(time))
                    {
                        result[wireIdx].Points.Add(DataPoint.Undefined);
                        continue;
                    }

                    result[wireIdx].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), allTemp[time]));
                    count++;
                    var percent = (int)(count * 100 / (wireCount * allTemp.Count));
                    if (percent != lastPercent)
                    {
                        progressFunc?.Invoke(percent);
                        lastPercent = percent;
                    }
                }
                // закончил вчера
            }


        });

        if (built) return result;
        else return null;
    }


    private static FunctionSeries[] buildPlotForEachWire(SilosService silosService, IEnumerable<Wire> wireToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {
        var built = false;
        var wireCount = wireToView.Count();
        var result = new FunctionSeries[wireCount];

        long count = 0;
        var lastPercent = 0;
        var wireIdx_ = 0;
        Parallel.ForEach(wireToView, w =>
        {
            var wireIdx = wireIdx_++;
            if (w.SilosId != -1)
            {
                result[wireIdx] = new FunctionSeries();
                result[wireIdx].Title = string.Format("Подвеска {0}. Плата: {1}. Ножка: {2}",
                    w.Number, w.DeviceAddress, w.Leg);
                result[wireIdx].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";

                var allTemp = silosService.getTemperatureForWireBetweenTime(w, start, end);
                if (allTemp != null && allTemp.Count != 0) built = true;

                foreach (var timeTempsEntry in allTemp)
                {
                    var midleValue = SilosService.getMiddleTemperature(timeTempsEntry.Value);

                    if (midleValue < -80) //некорректное значение
                        continue;

                    result[wireIdx].Points.Add(new DataPoint(DateTimeAxis.ToDouble(timeTempsEntry.Key), midleValue));

                    count++;
                    var percent = (int)(count * 100 / (wireCount * allTemp.Count));
                    if (percent != lastPercent)
                    {
                        progressFunc?.Invoke(percent);
                        lastPercent = percent;
                    }
                }
            }
        });

        if (built) return result;
        else return null;
    }



    private static FunctionSeries[] buildAVGPlotForEachSilos(SilosService silosService,
        IEnumerable<Wire> wireToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {
        var built = false;
        var siloses = new Dictionary<Silos, List<Wire>>();
        var arrayTime = silosService.getSetTimeBetweenTime(start, end);
        var silosIdx_ = 0;
        var lastPercent = 0;
        var wireCount = 0;
        long count = 0;

        foreach (var w in wireToView)
        {
            if (w.SilosId == -1)
                continue;
            var s = silosService.getSilos(w.SilosId);
            if (!siloses.ContainsKey(s))
                siloses.Add(s, new List<Wire>());
            siloses[s].Add(w);
            wireCount++;
        }

        var result = new FunctionSeries[siloses.Count];

        foreach (var item in siloses)
        {

            var s = item.Key;
            var silosIdx = silosIdx_++;
            var tempInSilos = new SortedDictionary<DateTime, List<float>>();

            result[silosIdx] = new FunctionSeries();
            result[silosIdx].Title = "Силос \"" + s.Name + "\"";
            result[silosIdx].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";

            Parallel.ForEach(item.Value, w =>
            {
                var allTemp = silosService.getAVGTemperatureSilosBetweenTime(w, start, end);
                if (allTemp != null && allTemp.Count != 0) built = true;

                foreach (var timeTempsEntry in allTemp)
                {
                    var time = timeTempsEntry.Key;
                    count++;
                    if (!tempInSilos.ContainsKey(time))
                        tempInSilos.Add(time, new List<float>(2) { 0, 0 });
                    tempInSilos[time][0] += timeTempsEntry.Value;
                    tempInSilos[time][1] += 1;
                    var percent = (int)(count * 100 / (wireCount * allTemp.Count));
                    if (percent != lastPercent)
                    {
                        progressFunc?.Invoke(percent);
                        lastPercent = percent;
                    }
                }

            });

            foreach (var point in arrayTime)
            {
                if (!tempInSilos.ContainsKey(point))
                {
                    result[silosIdx].Points.Add(DataPoint.Undefined);
                }
                else
                {
                    var temperatrue = tempInSilos[point][0] / tempInSilos[point][1];
                    result[silosIdx].Points.Add(new DataPoint(DateTimeAxis.ToDouble(point), temperatrue));
                }
            }
            /*foreach (var timeTempsEntry in tempInSilos)
            {
                DateTime time = timeTempsEntry.Key;
                float temperatrue = tempInSilos.Value[0] / tempInSilos.Value[0];

                result[silosIdx].Points.Add(new DataPoint(DateTimeAxis.ToDouble(point), temperatrue));
            }*/


            silosIdx++;
        }

        if (built) return result;
        else
        {
            return null;
        }

    }

    private static FunctionSeries[] buildPlotForEachSilos(SilosService silosService,
        IEnumerable<Wire> wireToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {
        var built = false;
        var wireCount = 0;
        var siloses = new Dictionary<Silos, List<Wire>>();

        foreach (var w in wireToView)
        {
            if (w.SilosId == -1)
                continue;
            var s = silosService.getSilos(w.SilosId);
            if (!siloses.ContainsKey(s))
                siloses.Add(s, new List<Wire>());
            siloses[s].Add(w);
            wireCount++;
        }

        var result = new FunctionSeries[siloses.Count];

        var silosIdx_ = 0;
        var lastPercent = 0;
        long count = 0;


        foreach (var item in siloses)
        {
            var s = item.Key;
            var silosIdx = silosIdx_++;
            var tempInSilos = new SortedDictionary<DateTime, List<float>>();

            Parallel.ForEach(item.Value, w =>
            {
                var allTemp = silosService.getTemperatureForWireBetweenTime(w, start, end);
                if (allTemp != null && allTemp.Count != 0) built = true;

                foreach (var timeTempsEntry in allTemp)
                {
                    var time = timeTempsEntry.Key;
                    var temp = timeTempsEntry.Value;
                    if (!tempInSilos.ContainsKey(time))
                        tempInSilos.Add(time, new List<float>(2) { 0, 0 });

                    foreach (var t in temp)
                        if (t > -80)
                        { //корректное значение
                            tempInSilos[time][0] += t;
                            tempInSilos[time][1] += 1;
                        }

                    count++;
                    var percent = (int)(count * 100 / (wireCount * allTemp.Count));
                    if (percent != lastPercent)
                    {
                        progressFunc?.Invoke(percent);
                        lastPercent = percent;
                    }
                }

            });

            result[silosIdx] = new FunctionSeries();
            result[silosIdx].Title = "Силос \"" + s.Name + "\"";
            result[silosIdx].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";

            foreach (var timeTempsEntry in tempInSilos)
            {
                var time = timeTempsEntry.Key;
                var temperatrue = timeTempsEntry.Value[0] / timeTempsEntry.Value[1];

                result[silosIdx].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), temperatrue));
            }

            silosIdx++;
        }
        //);

        if (built) return result;
        else return null;
    }

    private static FunctionSeries[] buildPlotMidMinMaxForWire(SilosService silosService, Wire wireToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {
        var built = false;
        var arrayTime = silosService.getSetTimeBetweenTime(start, end);
        var allTemp = silosService.getTemperatureForWireBetweenTime(wireToView, start, end);
        if (allTemp == null || allTemp.Count == 0) return null;

        var result = new FunctionSeries[3];
        result[0] = new FunctionSeries();
        result[1] = new FunctionSeries();
        result[2] = new FunctionSeries();
        result[0].Color = OxyColors.Red;
        result[0].Title = "Максимальная температура";
        result[1].Color = OxyColors.Yellow;
        result[1].Title = "Средняя температура";
        result[2].Color = OxyColors.Green;
        result[2].Title = "Минимальная температура";
        result[0].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
        result[1].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
        result[2].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";

        long count = 0;
        var lastPercent = 0;

        foreach (var item in arrayTime)
        {
            if (!allTemp.ContainsKey(item))
            {
                result[0].Points.Add(DataPoint.Undefined);
                result[1].Points.Add(DataPoint.Undefined);
                result[2].Points.Add(DataPoint.Undefined);
                continue;
            }
            var temp = allTemp[item];
            for (var i = 0; i < temp.Length; i++)
            {
                var tmpValue = SilosService.getMiddleTemperature(temp);
                if (allTemp != null && allTemp.Count != 0) built = true;

                if (tmpValue > -80 && tmpValue < 140)//корректное значение
                {
                    result[1].Points.Add(new DataPoint(DateTimeAxis.ToDouble(item), tmpValue));
                }
                tmpValue = temp.Max();
                if (tmpValue > -80 && tmpValue < 140)
                {
                    result[0].Points.Add(new DataPoint(DateTimeAxis.ToDouble(item), tmpValue));
                }

                tmpValue = temp.Min();
                if (tmpValue > -80 && tmpValue < 140)
                {
                    result[2].Points.Add(new DataPoint(DateTimeAxis.ToDouble(item), tmpValue));
                }

                count++;
                var percent = (int)(count * 100 / allTemp.Count);
                if (percent != lastPercent)
                {
                    progressFunc?.Invoke(percent);
                    lastPercent = percent;
                }
            }
        }
        if (built) return result;
        else return null;
    }


    private static FunctionSeries[] buildPlotMaxAVGMinForWires(SilosService silosService, IEnumerable<Wire> wireToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {
        var built = false;
        var wireCount = wireToView.Count();
        var result = new FunctionSeries[3];
        result[0] = new FunctionSeries();
        result[1] = new FunctionSeries();
        result[2] = new FunctionSeries();
        result[0].Color = OxyColors.Red;
        result[0].Title = "Максимальная температура";
        result[1].Color = OxyColors.Yellow;
        result[1].Title = "Средняя температура";
        result[2].Color = OxyColors.Green;
        result[2].Title = "Минимальная температура";

        result[0].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
        result[1].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
        result[2].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
        var wires = new int[wireToView.Count()];
        var i = 0;
        foreach (var wire in wireToView)
        {
            wires[i] = wire.Id;
            ++i;
        }

        var mamTemp = silosService.getMaxAvgMinBetweenTime(wires, start, end);

        foreach (var time in mamTemp.Keys)
        {
            if (mamTemp.ContainsKey(time))
            {
                result[0].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), mamTemp[time].ElementAt(0)));
                result[1].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), mamTemp[time].ElementAt(1)));
                result[2].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), mamTemp[time].ElementAt(2)));
            }
            built = true;
        }

        if (built) return result;
        else return null;
    }

    private static FunctionSeries[] buildPlotMidMinMaxForSilos(SilosService silosService, IEnumerable<Wire> wireToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {
        var built = false;
        var wireCount = wireToView.Count();
        var result = new FunctionSeries[3];
        result[0] = new FunctionSeries();
        result[1] = new FunctionSeries();
        result[2] = new FunctionSeries();
        result[0].Color = OxyColors.Red;
        result[0].Title = "Максимальная температура";
        result[1].Color = OxyColors.Yellow;
        result[1].Title = "Средняя температура";
        result[2].Color = OxyColors.Green;
        result[2].Title = "Минимальная температура";

        result[0].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
        result[1].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
        result[2].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";

        var mid = new SortedDictionary<DateTime, float>();
        var max = new SortedDictionary<DateTime, float>();
        var min = new SortedDictionary<DateTime, float>();
        var wireCounter = new SortedDictionary<DateTime, int>();
        long count = 0;
        var lastPercent = 0;

        var arrayTime = silosService.getSetTimeBetweenTime(start, end);

        Parallel.ForEach(wireToView, w =>
        {
            if (w.SilosId != -1)
            {
                var allTemp = silosService.getTemperatureForWireBetweenTime(w, start, end);

                var tmp = allTemp.ToList();
                foreach (var timeTempsEntry in allTemp)
                //Parallel.ForEach(tmp, timeTempsEntry =>
                {
                    var time = timeTempsEntry.Key;
                    if (!wireCounter.ContainsKey(time)) wireCounter.Add(time, 0);

                    var tmpValue = SilosService.getMiddleTemperature(timeTempsEntry.Value);
                    //убрать
                    if (!mid.ContainsKey(time))
                        mid.Add(time, 0);
                    mid[time] += tmpValue;
                    wireCounter[time]++;

                    tmpValue = timeTempsEntry.Value.Max();
                    if (!max.ContainsKey(time)) max.Add(time, -90);
                    max[time] = Math.Max(max[time], tmpValue);

                    tmpValue = SilosService.getMinTemperature(timeTempsEntry.Value); // из-за несуществующих значений

                    if (!min.ContainsKey(time)) min.Add(time, 150);
                    min[time] = Math.Min(min[time], tmpValue);

                    count++;
                    var percent = (int)(count * 100 / (wireCount * allTemp.Count));
                    if (percent != lastPercent)
                    {
                        progressFunc?.Invoke(percent);
                        lastPercent = percent;
                    }
                }
            }
        });

        foreach (var time in arrayTime)
        {
            if (max.ContainsKey(time))
                result[0].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), max[time]));
            else
                result[0].Points.Add(DataPoint.Undefined);
            if (mid.ContainsKey(time))
                result[1].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), mid[time] / wireCounter[time]));
            else
                result[1].Points.Add(DataPoint.Undefined);
            if (min.ContainsKey(time))
                result[2].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), min[time]));
            else
                result[2].Points.Add(DataPoint.Undefined);
            built = true;
        }

        if (built) return result;
        else return null;
    }

    public static PlotModel plottingFilling(SettingsService settingsService, SilosService silosService, IEnumerable<Silos> silosToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {
        try
        {
            var pm = initFillingPlotModel(settingsService, silosService);
            pm.Title = "График заполненности силосов за период от "
                + start.ToString("dd.MM.yyyy") + " до "
                + end.ToString("dd.MM.yyyy") + ".";

            pm.Axes[0].StringFormat = "dd.MM.yyyy";

            var series = getFillingSeries(silosService, silosToView, start, end, progressFunc);

            if (series == null)
                return null;

            for (var i = 0; i < series.Length; i++)
                pm.Series.Add(series[i]);
            return pm;

        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Plot Building Error");
            return null;
        }

    }

    private static StemSeries[] getFillingStemSeries(SilosService silosService, IEnumerable<Silos> silosToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {
        if (silosToView.Count() == 0)
            return null;
        var result = new StemSeries[silosToView.Count()];
        var built = false;
        Parallel.For(0, result.Length, i =>
        {
            var line = new StemSeries();
            var item = silosToView.ElementAt(i);
            line.Title = "Силос \"" + item.Name + "\"";
            line.TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
            var function = silosService.getFillingBetween(start, end, item.Id);
            var timesList = function.Keys.ToList();
            if (function != null && function.Count != 0) built = true;
            foreach (var time in timesList)
            {
                var prev = -1;
                var next = -1;
                if (timesList.IndexOf(time) != 0)
                    if (time.AddHours(-24) < timesList[timesList.IndexOf(time) - 1])
                        prev = function[timesList[timesList.IndexOf(time) - 1]];
                if (timesList.IndexOf(time) != timesList.Count - 1)
                    if (time.AddHours(24) > timesList[timesList.IndexOf(time) + 1])
                        next = function[timesList[timesList.IndexOf(time) + 1]];


                if (function[time] >= 95 && (next <= 5 || prev <= 5))
                {
                    function[time] = 0;
                    line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), 0));
                    continue;
                }
                if ((function[time] == 0 || function[time] == 100) && (prev != 0 && prev != 100 && prev != -1 || next != 100 && next != 0 && next != -1))
                {
                    function[time] = prev;
                    line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), function[time]));
                    continue;
                }
                line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), function[time]));
            }
            result[i] = line;
        });

        if (built)
            return result;
        else
            return null;
    }

    private static FunctionSeries[] getFillingSeries(SilosService silosService, IEnumerable<Silos> silosToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {
        if (silosToView.Count() == 0)
            return null;
        var result = new FunctionSeries[silosToView.Count()];
        var built = false;
        Parallel.For(0, result.Length, i =>
        {
            var line = new FunctionSeries
            {
                MarkerType = MarkerType.Circle
            };
            var item = silosToView.ElementAt(i);
            line.Title = "Силос \"" + item.Name + "\"";
            line.TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
            var function = silosService.getFillingBetween(start, end, item.Id);
            var timesList = function.Keys.ToList();
            if (function != null && function.Count != 0) built = true;
            foreach (var time in timesList)
            {
                var prev = -1;
                var next = -1;
                if (timesList.IndexOf(time) != 0)
                    if (time.AddHours(-24) < timesList[timesList.IndexOf(time) - 1])
                        prev = function[timesList[timesList.IndexOf(time) - 1]];
                if (timesList.IndexOf(time) != timesList.Count - 1)
                    if (time.AddHours(24) > timesList[timesList.IndexOf(time) + 1])
                        next = function[timesList[timesList.IndexOf(time) + 1]];


                if (function[time] >= 95 && (next <= 5 || prev <= 5))
                {
                    function[time] = 0;
                    line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), 0));
                    continue;
                }
                if ((function[time] == 0 || function[time] == 100) && (prev != 0 && prev != 100 && prev != -1 || next != 100 && next != 0 && next != -1))
                {
                    function[time] = prev;
                    line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), function[time]));
                    continue;
                }
                line.Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), function[time]));
            }
            result[i] = line;
        });

        if (built)
            return result;
        else
            return null;
    }

    private static FunctionSeries[] chooseShapeAndGetFuncSeries(SettingsService settingsService, SilosService silosService, IEnumerable<Wire> wireToView, DateTime start, DateTime end, reportProgressCall progressFunc = null)
    {
        if (wireToView.Count() == 0)
            return null;

        if (wireToView.Count() == 1) // Всего одна подвеска
        {
            if (settingsService.ShowOneWireMode == SettingsService.IF_ONE_WIRE_SHOW_ALL_SENSORS)
                return newBuildPlotForEachSensor(silosService, wireToView.First(), start, end, progressFunc);
            //return buildPlotForEachSensor(silosService, wireToView.First(), start, end, progressFunc);
            else if (settingsService.ShowOneWireMode == SettingsService.IF_ONE_WIRE_SHOW_MIN_MID_MAX)
                return buildPlotMidMinMaxForWire(silosService, wireToView.First(), start, end, progressFunc);

        }

        // изменение Maclay
        var oneSilosId = wireToView.First().SilosId;
        var isOnlyOneSilos = true;

        foreach (var w in wireToView) // Проверка на единственность силоса.
        {
            if (w.SilosId == -1)
                continue;
            if (w.SilosId != oneSilosId) // Силос не один  
                isOnlyOneSilos = false;
        }

        if (isOnlyOneSilos) // Всего один силос
        {
            if (settingsService.ShowOneSilosMode == SettingsService.IF_ONE_SILOS_SHOW_ALL_WIRES)
                return nevBuildPlotForEachWire(silosService, wireToView, start, end, progressFunc);
            //return buildPlotForEachWire(silosService, wireToView, start, end, progressFunc);
            else if (settingsService.ShowOneSilosMode == SettingsService.IF_ONE_SILOS_SHOW_MIN_MID_MAX)
                //return buildPlotMaxAVGMinForWires(silosService, wireToView, start, end, progressFunc);
                return buildPlotMidMinMaxForSilos(silosService, wireToView, start, end, progressFunc);

        }

        //return buildPlotForEachSilos(silosService, wireToView, start, end, progressFunc);
        return buildAVGPlotForEachSilos(silosService, wireToView, start, end, progressFunc);

    }

    public static PlotModel buildPlotOneDayN(SettingsService settingsService, SilosService silosService, IEnumerable<Wire> wireToView, int oneSilosID = -1, reportProgressCall progressFunc = null)
    {
        try
        {
            var pm = initPlotModel(settingsService, silosService);

            if (oneSilosID != -1)
                pm.Title = "Показания температур в силосе \"" + silosService.getSilos(oneSilosID).Name + "\" за последние сутки";
            else
                pm.Title = "Показания температур за последние сутки";

            pm.Axes[0].StringFormat = "dd. H:mm";

            var series = chooseShapeAndGetFuncSeries(settingsService, silosService, wireToView, DateTime.Now.AddDays(-1), DateTime.Now);
            if (series == null) return null;

            for (var i = 0; i < series.Length; i++)
                pm.Series.Add(series[i]);

            return pm;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Plot Building Error");
            return null;
        }
    }

    public static PlotModel buildPlotWeekN(SettingsService settingsService, SilosService silosService, IEnumerable<Wire> wireToView, reportProgressCall progressFunc = null)
    {
        try
        {
            var pm = initPlotModel(settingsService, silosService);
            pm.Title = "Показания температур за последнюю неделю";
            pm.Axes[0].StringFormat = "dd.MM : HH";

            var series = chooseShapeAndGetFuncSeries(settingsService, silosService, wireToView, DateTime.Now.AddDays(-7), DateTime.Now, progressFunc);
            if (series == null) return null;

            for (var i = 0; i < series.Length; i++)
                pm.Series.Add(series[i]);

            return pm;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Plot Building Error");
            return null;
        }
    }

    public static PlotModel buildPlotMonthN(SettingsService settingsService, SilosService silosService, IEnumerable<Wire> wireToView, reportProgressCall progressFunc = null)
    {
        try
        {
            var pm = initPlotModel(settingsService, silosService);
            pm.Title = "Показания температур за последний месяц";
            pm.Axes[0].StringFormat = "dd.MM.yyyy";

            var series = chooseShapeAndGetFuncSeries(settingsService, silosService, wireToView, DateTime.Now.AddMonths(-1), DateTime.Now, progressFunc);
            if (series == null) return null;

            for (var i = 0; i < series.Length; i++)
                pm.Series.Add(series[i]);

            return pm;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Plot Building Error");
            return null;
        }
    }

    public static PlotModel buildPlotYearN(SettingsService settingsService, SilosService silosService, IEnumerable<Wire> wireToView, reportProgressCall progressFunc = null)
    {
        try
        {
            var pm = initPlotModel(settingsService, silosService);
            pm.Title = "Показания температур за последний год";
            pm.Axes[0].StringFormat = "dd.MM.yyyy";

            var series = chooseShapeAndGetFuncSeries(settingsService, silosService, wireToView, DateTime.Now.AddYears(-1), DateTime.Now, progressFunc);
            if (series == null) return null;

            for (var i = 0; i < series.Length; i++)
                pm.Series.Add(series[i]);

            return pm;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Plot Building Error");
            return null;
        }
    }

    public static PlotModel buildPlotCustomN(SettingsService settingsService, SilosService silosService, IEnumerable<Wire> wireToView,
        DateTime startTime, DateTime endTime, reportProgressCall progressFunc = null)
    {
        try
        {
            var pm = initPlotModel(settingsService, silosService);
            pm.Title = "Показания температур за период от "
                + startTime.ToString("dd.MM.yyyy") + " до "
                + endTime.ToString("dd.MM.yyyy") + ".";

            pm.Axes[0].StringFormat = "dd.MM.yyyy";

            var series = chooseShapeAndGetFuncSeries(settingsService, silosService, wireToView, startTime, endTime, progressFunc);
            if (series == null)
            {
                return null;
            }

            for (var i = 0; i < series.Length; i++)
                pm.Series.Add(series[i]);
            return pm;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Plot Building Error");
            return null;
        }
    }

    #endregion

    private static FunctionSeries[] getFunctionSeries(SettingsService settingsService, SilosService silosService, Dictionary<Wire, SortedDictionary<DateTime, float[]>> temperatures)
    {
        if (temperatures == null || temperatures.Count == 0)
            return null;

        if (temperatures.Count == 1) // Всего одна подвеска            
            return getFunctionSeriesOneWire(settingsService, silosService, temperatures);

        var oneSilosId = -1;
        var isOnlyOneSilos = true;
        foreach (var w in temperatures.Keys) // Проверка на единственность силоса.
        {
            if (w.SilosId == -1)
                continue;
            if (oneSilosId == -1) // первая итерация
                oneSilosId = w.SilosId;
            else if (w.SilosId != oneSilosId) // Силос не один  
                isOnlyOneSilos = false;
        }

        if (isOnlyOneSilos) // Всего один силос
            return getFunctionSeriesOneSilos(settingsService, silosService, temperatures);

        //Силосов несколько, берем среднее значение для каждого.
        var middleTempForSilos = new Dictionary<int, SortedDictionary<DateTime, float>>();
        //Количество подвесок в силосе на данный момент silos, time, count
        var countOfWires = new Dictionary<KeyValuePair<int, DateTime>, int>();

        //Считаем средние температуры для силосов.
        foreach (var wireTempEntry in temperatures)
        {
            var w = wireTempEntry.Key;
            var temp = wireTempEntry.Value;

            if (!middleTempForSilos.ContainsKey(w.SilosId))
            {
                middleTempForSilos.Add(w.SilosId, new SortedDictionary<DateTime, float>());
            }
            //countOfWires[w.SilosId]++;

            //Суммируем температуры для данного времени для всей подвески.
            foreach (var timeTempEntry in temp)
            {
                var time = timeTempEntry.Key;

                var midleValue = SilosService.getMiddleTemperature(timeTempEntry.Value);
                if (midleValue < -80)//корректное значение
                    continue;

                if (!middleTempForSilos[w.SilosId].ContainsKey(time))
                    middleTempForSilos[w.SilosId].Add(time, 0);
                if (!countOfWires.ContainsKey(new KeyValuePair<int, DateTime>(w.SilosId, time)))
                    countOfWires.Add(new KeyValuePair<int, DateTime>(w.SilosId, time), 0);

                countOfWires[new KeyValuePair<int, DateTime>(w.SilosId, time)]++;

                middleTempForSilos[w.SilosId][timeTempEntry.Key] += midleValue;
            }
        }
        var result = new FunctionSeries[middleTempForSilos.Count];
        var idx = 0;
        // усредняем и заполняем график
        foreach (var silosTempEntry in middleTempForSilos)
        {
            var s = silosService.getSilos(silosTempEntry.Key);
            result[idx] = new FunctionSeries();
            result[idx].Title = "Силос \"" + s.Name + "\"";
            foreach (var timeTempsEntry in middleTempForSilos[s.Id])
            {
                var time = timeTempsEntry.Key;
                var temperatrue = timeTempsEntry.Value /
                    countOfWires[new KeyValuePair<int, DateTime>(s.Id, time)];
                result[idx].Points.Add(new DataPoint(
                    DateTimeAxis.ToDouble(time), temperatrue));
            }

            idx++;
        }

        return result;
    }

    private static FunctionSeries[] getFunctionSeriesOneSilos(SettingsService settingsService, SilosService silosService, Dictionary<Wire, SortedDictionary<DateTime, float[]>> temperatures)
    {
        //считаем кол-во подвесок.
        var wireCount = 0;
        foreach (var w in temperatures.Keys)
            if (w.SilosId != -1)
                wireCount++;

        var result = new FunctionSeries[0];

        // рисуем все подвески со среднем значением
        if (settingsService.ShowOneSilosMode == SettingsService.IF_ONE_SILOS_SHOW_ALL_WIRES)
        {
            result = new FunctionSeries[wireCount];
            var wireIdx = 0;
            foreach (var wireTempEntry in temperatures)
            {
                var w = wireTempEntry.Key;
                result[wireIdx] = new FunctionSeries();
                result[wireIdx].Title = string.Format("Подвеска {0}. Плата: {1}. Ножка: {2}",
                    w.Number, w.DeviceAddress, w.Leg);
                result[wireIdx].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
                //"Подвеска. Плата: " + w.DeviceAddress
                //+ "Ножка: " + w.Leg;

                foreach (var timeTempsEntry in wireTempEntry.Value)
                {
                    var midleValue = SilosService.getMiddleTemperature(timeTempsEntry.Value);

                    if (midleValue > -80)//корректное значение
                        result[wireIdx].Points.Add(new DataPoint(
                            DateTimeAxis.ToDouble(timeTempsEntry.Key),
                            midleValue
                            ));
                }
                wireIdx++;
            }
        }
        // рисуем максимальную, среднюю, и минимальную температуры. 
        else if (settingsService.ShowOneSilosMode == SettingsService.IF_ONE_SILOS_SHOW_MIN_MID_MAX)
        {
            result = new FunctionSeries[3];
            result[0] = new FunctionSeries();
            result[1] = new FunctionSeries();
            result[2] = new FunctionSeries();
            result[0].Color = OxyColors.Red;
            result[0].Title = "Максимальная температура";
            result[1].Color = OxyColors.Yellow;
            result[1].Title = "Средняя температура";
            result[2].Color = OxyColors.Green;
            result[2].Title = "Минимальная температура";

            result[0].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
            result[1].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
            result[2].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";

            var mid = new SortedDictionary<DateTime, float>();
            var max = new SortedDictionary<DateTime, float>();
            var min = new SortedDictionary<DateTime, float>();
            var wireCounter = new SortedDictionary<DateTime, int>();

            foreach (var wireTempEntry in temperatures)
            {
                var w = wireTempEntry.Key;
                foreach (var timeTempsEntry in wireTempEntry.Value)
                {
                    var time = timeTempsEntry.Key;
                    if (!wireCounter.ContainsKey(time)) wireCounter.Add(time, 0);

                    var tmpValue = SilosService.getMiddleTemperature(timeTempsEntry.Value);
                    if (tmpValue > -80 && tmpValue < 140)//корректное значение
                    {
                        if (!mid.ContainsKey(time)) mid.Add(time, 0);
                        mid[time] += tmpValue;
                        wireCounter[time]++;
                    }
                    tmpValue = timeTempsEntry.Value.Max();
                    if (tmpValue > -80 && tmpValue < 140)
                    {
                        if (!max.ContainsKey(time)) max.Add(time, -90);
                        max[time] = Math.Max(max[time], tmpValue);
                    }

                    tmpValue = SilosService.getMinTemperature(timeTempsEntry.Value);
                    if (tmpValue > -80 && tmpValue < 140)
                    {
                        if (!min.ContainsKey(time)) min.Add(time, 150);
                        min[time] = Math.Min(min[time], tmpValue);
                    }
                }
            }

            foreach (var time in wireCounter.Keys)
            {
                if (max.ContainsKey(time))
                    result[0].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), max[time]));
                if (mid.ContainsKey(time))
                    result[1].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), mid[time] / wireCounter[time]));
                if (min.ContainsKey(time))
                    result[2].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), min[time]));
            }
        }
        return result;
    }

    private static FunctionSeries[] getFunctionSeriesOneWire(SettingsService settingsService, SilosService silosService, Dictionary<Wire, SortedDictionary<DateTime, float[]>> temperatures)
    {

        var w = temperatures.First().Key;
        var tempsAllTime = temperatures.First().Value;
        var result = new FunctionSeries[0];

        //Рисуем все сенсоры
        if (settingsService.ShowOneWireMode == SettingsService.IF_ONE_WIRE_SHOW_ALL_SENSORS)
        {
            result = new FunctionSeries[w.SensorCount];
            for (var i = 0; i < w.SensorCount; i++)
            {
                result[i] = new FunctionSeries();
                result[i].Title = "Сеносор " + (i + 1).ToString();
                result[i].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
            }

            foreach (var timeTempsEntry in tempsAllTime)
            {
                var temps = timeTempsEntry.Value;
                for (var i = 0; i < w.SensorCount; i++)
                {
                    if (i >= temps.Length || temps[i] < -80)
                        continue;

                    result[i].Points.Add(new DataPoint(
                        DateTimeAxis.ToDouble(timeTempsEntry.Key),
                        temps[i]
                        ));
                }
            }
        }
        else if (settingsService.ShowOneWireMode == SettingsService.IF_ONE_WIRE_SHOW_MIN_MID_MAX)
        {
            result = new FunctionSeries[3];
            result[0] = new FunctionSeries();
            result[1] = new FunctionSeries();
            result[2] = new FunctionSeries();
            result[0].Color = OxyColors.Red;
            result[0].Title = "Максимальная температура";
            result[1].Color = OxyColors.Yellow;
            result[1].Title = "Средняя температура";
            result[2].Color = OxyColors.Green;
            result[2].Title = "Минимальная температура";
            result[0].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
            result[1].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";
            result[2].TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:0.0}";

            foreach (var timeTempsEntry in tempsAllTime)
            {
                var time = timeTempsEntry.Key;
                var temps = timeTempsEntry.Value;
                for (var i = 0; i < w.SensorCount; i++)
                {
                    var tmpValue = SilosService.getMiddleTemperature(temps);
                    if (tmpValue > -80 && tmpValue < 140)//корректное значение
                    {
                        result[1].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), tmpValue));
                    }
                    tmpValue = temps.Max();
                    if (tmpValue > -80 && tmpValue < 140)
                    {
                        result[0].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), tmpValue));
                    }

                    tmpValue = SilosService.getMinTemperature(temps);
                    if (tmpValue > -80 && tmpValue < 140)
                    {
                        result[2].Points.Add(new DataPoint(DateTimeAxis.ToDouble(time), tmpValue));
                    }
                }
            }
        }
        return result;
    }

    private static PlotModel initPlotModel(SettingsService settingsService, SilosService silosService)
    {
        var pm = new PlotModel
        {
            Subtitle = settingsService.CompanyName,
            PlotType = PlotType.XY,
            Background = OxyColors.White,
            Padding = new OxyThickness(10),
        };

        var xAxis = new DateTimeAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Время",
            IntervalType = DateTimeIntervalType.Seconds,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.None,
            IsPanEnabled = true,
            IsZoomEnabled = true
        };

        pm.Axes.Add(xAxis);
        pm.Axes.Add(new LinearAxis()
        {
            Title = "Температура",
            IsPanEnabled = false,
            IsZoomEnabled = false,
            MinimumPadding = 0.1,
            MaximumPadding = 0.1
        });

        return pm;
    }

    private static PlotModel initFillingPlotModel(SettingsService settingsService, SilosService silosService)
    {
        var pm = new PlotModel
        {
            Subtitle = settingsService.CompanyName,
            PlotType = PlotType.XY,
            Background = OxyColors.White,
            Padding = new OxyThickness(10),
        };

        var xAxis = new DateTimeAxis
        {
            Position = AxisPosition.Bottom,
            Title = "Время",
            IntervalType = DateTimeIntervalType.Seconds,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.None,
            IsPanEnabled = true,
            IsZoomEnabled = true
        };

        pm.Axes.Add(xAxis);
        pm.Axes.Add(new LinearAxis()
        {
            Title = "% заполненности",
            IsPanEnabled = false,
            IsZoomEnabled = false,
            MinimumPadding = 0.1,
            MaximumPadding = 0.1
        });

        return pm;
    }

    public static PlotModel buildPlotOneDay(SettingsService settingsService, SilosService silosService, IEnumerable<Wire> wireToView, int oneSilosID = -1)
    {
        try
        {
            var pm = initPlotModel(settingsService, silosService);

            if (oneSilosID != -1)
                pm.Title = "Показания температур в силосе \"" + silosService.getSilos(oneSilosID).Name + "\" за последние сутки";
            else
                pm.Title = "Показания температур за последние сутки";

            pm.Axes[0].StringFormat = "dd. H:mm";

            var temps = silosService.getTemperatureBetweenTime(wireToView,
                DateTime.Now.AddDays(-1), DateTime.Now);
            if (temps == null || temps.Count == 0)
                return null;

            var series = getFunctionSeries(settingsService, silosService, temps);
            for (var i = 0; i < series.Length; i++)
                pm.Series.Add(series[i]);

            return pm;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Plot Building Error");
            return null;
        }
    }

    public static PlotModel buildPlotWeek(SettingsService settingsService, SilosService silosService, IEnumerable<Wire> wireToView)
    {
        try
        {
            var pm = initPlotModel(settingsService, silosService);
            pm.Title = "Показания температур за последнюю неделю";
            pm.Axes[0].StringFormat = "dd.MM : HH";

            var temps = silosService.getTemperatureBetweenTime(wireToView,
                DateTime.Now.AddDays(-7), DateTime.Now);
            if (temps == null || temps.Count == 0)
                return null;

            var series = getFunctionSeries(settingsService, silosService, temps); ;
            for (var i = 0; i < series.Length; i++)
                pm.Series.Add(series[i]);

            return pm;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Plot Building Error");
            return null;
        }
    }

    public static PlotModel buildPlotMonth(SettingsService settingsService, SilosService silosService, IEnumerable<Wire> wireToView)
    {
        try
        {
            var pm = initPlotModel(settingsService, silosService);
            pm.Title = "Показания температур за последний месяц";
            pm.Axes[0].StringFormat = "dd.MM.yyyy";

            var temps = silosService.getTemperatureBetweenTime(wireToView,
                DateTime.Now.AddMonths(-1), DateTime.Now);
            if (temps == null || temps.Count == 0)
                return null;

            var series = getFunctionSeries(settingsService, silosService, temps); ;
            for (var i = 0; i < series.Length; i++)
                pm.Series.Add(series[i]);

            return pm;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Plot Building Error");
            return null;
        }
    }

    public static PlotModel buildPlotYear(SettingsService settingsService, SilosService silosService, IEnumerable<Wire> wireToView)
    {
        try
        {
            var pm = initPlotModel(settingsService, silosService);
            pm.Title = "Показания температур за последний год";
            pm.Axes[0].StringFormat = "dd.MM.yyyy";

            var temps = silosService.getTemperatureBetweenTime(wireToView,
                DateTime.Now.AddYears(-1), DateTime.Now);
            if (temps == null || temps.Count == 0)
                return null;

            var series = getFunctionSeries(settingsService, silosService, temps); ;
            for (var i = 0; i < series.Length; i++)
                pm.Series.Add(series[i]);

            return pm;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Plot Building Error");
            return null;
        }
    }

    public static PlotModel buildPlotCustom(SettingsService settingsService, SilosService silosService, IEnumerable<Wire> wireToView,
        DateTime startTime, DateTime endTime)
    {
        try
        {
            var pm = initPlotModel(settingsService, silosService);
            pm.Title = "Показания температур за период от "
                + startTime.ToString("dd.MM.yyyy") + " до "
                + endTime.ToString("dd.MM.yyyy") + ".";

            pm.Axes[0].StringFormat = "dd.MM.yyyy";

            var temps = silosService.getTemperatureBetweenTime(wireToView,
                startTime, endTime);
            if (temps == null || temps.Count == 0)
                return null;

            var series = getFunctionSeries(settingsService, silosService, temps); ;
            for (var i = 0; i < series.Length; i++)
                pm.Series.Add(series[i]);

            return pm;
        }
        catch (Exception ex)
        {
            MyLoger.Log(ex, "Plot Building Error");
            return null;
        }
    }

}
