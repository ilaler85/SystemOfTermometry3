using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemOfTermometry2.Model;

namespace SystemOfTermometry2.Services
{
    /// <summary>
    /// Static class for calculating filling in silos
    /// </summary>
    public static class HistoryFillingService
    {

        private static int lookingForFillingBoundary(float[] derivative)
        {
            float globalIncrementDerivative = 0;
            var border = 0;

            //float[] 

            for (var i = 0; i < derivative.Length; ++i)
            {
                if (derivative[i] > globalIncrementDerivative)
                {
                    globalIncrementDerivative = derivative[i];
                    border = i + 1;
                }
            }
            if (globalIncrementDerivative < 2)
                return 0;

            return border;
        }

        /// <summary>
        /// вычисление производной графика
        /// </summary>
        /// <param name="midlArray"></param>
        /// <returns></returns>

        private static float[] calculatingDerivative(float[] midlArray, out float min, out float max)
        {

            min = midlArray[0];
            max = midlArray[0];

            //float[] derivative = new float[midlArray.Length - 2];
            //изменения чтобы попробовать вычислять не производную, а искать максимальную разницу между точками
            var derivative = new float[midlArray.Length - 1];

            for (var i = 0; i < derivative.Length; ++i)
            {
                min = Math.Min(min, midlArray[i]);
                max = Math.Max(max, midlArray[i]);
                derivative[i] = Math.Abs(midlArray[i] - midlArray[i + 1]);
                //derivative[i] = (4 * midlArray[i + 1] - 3 * midlArray[i] - midlArray[i + 2]) / 2;
            }

            return derivative;
        }


        /// <summary>
        /// вычисление процента заполненности
        /// </summary>
        /// <param name="border"></param>
        /// <param name="midSensor"></param>
        /// <returns></returns>

        private static int calculateFilling(int border, float midSensor)
        {
            if (border + 1 > midSensor)
                return 100;
            var result = (int)Math.Round(Convert.ToSingle(border) / midSensor * 100, 0);
            return result;
        }


        private static float[] getMidlTempLine(Dictionary<int, float[]> temperatures, Dictionary<int, Wire> wires, out float midSensor)
        {
            var maxCount = 0;
            var midlHeigh = 0;
            var factCountWire = 0;
            foreach (var item in temperatures.Values)
            {
                if (item.Length != 0)
                {
                    midlHeigh += item.Length;
                    factCountWire++;
                }
                maxCount = Math.Max(maxCount, item.Length);
            }
            if (factCountWire == 0)
            {
                midSensor = 1;
                return new float[1];
            }
            midSensor = midlHeigh / factCountWire;
            var midlTemp = new float[maxCount];

            for (var i = 0; i < maxCount; ++i)
            {
                float sum = 0;
                var factCount = 0;
                foreach (var item in wires)
                {
                    if (item.Value == null)
                        continue;
                    var wire = item.Value;

                    if (wire.Enable && temperatures.ContainsKey(wire.Id))
                        if (i < temperatures[wire.Id].Length)
                            if (temperatures[wire.Id][i] > -70 && temperatures[wire.Id][i] < 100)
                            {
                                sum = sum + temperatures[wire.Id][i];
                                factCount++;
                            }

                }
                midlTemp[i] = sum / factCount;
            }

            return midlTemp;

        }

        private static bool chekNullFil(float[] midlTemp)
        {
            float min = 160;
            float sum = 0;
            var count = 0;

            for (var i = 0; i < midlTemp.Length; ++i)
            {
                min = Math.Min(min, midlTemp[i]);
                sum += midlTemp[i];
                ++count;
            }
            if (sum / count > 24 && min > 25)
                return true;
            else return false;
        }


        public static int getFilling(List<float> temperatures)
        {
            float min = 0, max = 0;
            var border = 0;
            if (!temperatures.Any())
                return -10;
            var temp = temperatures.ToArray();
            MyLoger.Log(DateTime.Now.ToString() + " Количество элементов в массиве " + temp.Count().ToString());

            if (chekNullFil(temp))
                return -10;
            else
            {

                border = lookingForFillingBoundary(calculatingDerivative(temp, out min, out max));
                if (max - min < 5)
                {
                    return 0;
                }
                var filling = calculateFilling(border, temperatures.Count);
                return filling;
            }
        }

        public static int getFilling(Dictionary<int, float[]> temperatures, Dictionary<int, Wire> wires)
        {
            float midSensor = 0;
            var border = 0;

            float min = 0, max = 0;

            var midlTemp = getMidlTempLine(temperatures, wires, out midSensor);
            if (midSensor < 2)
            {
                return 0;
            }
            if (chekNullFil(midlTemp))
                return 0;
            else
            {
                border = lookingForFillingBoundary(calculatingDerivative(midlTemp, out min, out max));
                if (max - min < 5)
                {
                    return 0;
                }
                var filling = calculateFilling(border, midSensor);
                return filling;
            }
        }

    }
}
