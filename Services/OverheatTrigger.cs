using System;
using System.Collections.Generic;
using SystemOfThermometry3.Model;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.Services
{
    /// <summary>
    /// Класс, который проверяет силоса на перегрев.
    /// Проверяет с учетом последних проверок.
    /// Если за установленное время изменений небыло, то отправляет пустой словарь.
    /// Иначе отправляет полный список перегретых силосов.
    /// </summary>
    class OverheatTrigger
    {
        private SilosService silosService;
        private SettingsService settingsService;

        // Силосы, которые будут игнорироваться. Ключ - id силоса. Значение - время добавленмя в игнор.
        private Dictionary<int, DateTime> silosesToIgnore;

        public OverheatTrigger(SilosService silosService, SettingsService settingsService)
        {
            this.silosService = silosService ?? throw new ArgumentNullException("Silos service is null!");
            this.settingsService = settingsService ?? throw new ArgumentNullException("Settings service is null!");
            silosesToIgnore = new Dictionary<int, DateTime>();
        }

        /// <summary>
        /// Возвращает силосы с перегревом, если появились новые или старые все еще перегреты.
        /// Обнавляет временные метки
        /// </summary>
        /// <returns>Ключ - перегретый силоса, Значение - словарь, где ключ - подвеска, значение - количество сенсеров с привышением.</returns>
        public Dictionary<Silos, Dictionary<Wire, int>> getAndCheckOverheat()
        {
            var overheatSiloses = getAllOverheatSiloses();

            var isNewOverHeat = false;
            foreach (var s in overheatSiloses.Keys)
            {
                // Этот силос уже был обнаружен как перегретый в данный период
                if (silosesToIgnore.ContainsKey(s.Id))
                    if ((silosesToIgnore[s.Id] - DateTime.Now).TotalMinutes > settingsService.OverheatTriggerMinimumPeriod)
                        continue;

                isNewOverHeat = true;
                break;
            }

            if (isNewOverHeat)
            {
                foreach (var s in overheatSiloses.Keys) //Обновляем временные метки
                {
                    if (!silosesToIgnore.ContainsKey(s.Id))
                        silosesToIgnore.Add(s.Id, DateTime.Now);
                    else
                        silosesToIgnore[s.Id] = DateTime.Now;
                }

                return overheatSiloses;
            }
            else
            {
                return new Dictionary<Silos, Dictionary<Wire, int>>();
            }
        }

        /// <summary>
        /// Возвращает все силосы с перегревом. Не обновляет временные метки.
        /// </summary>
        /// <returns>Ключ - id силоса, Значение - словарь, где ключ - id подвески, значение - количество сенсеров с привышением.</returns>
        public Dictionary<Silos, Dictionary<Wire, int>> getAllOverheatSiloses()
        {
            var result = new Dictionary<Silos, Dictionary<Wire, int>>();

            foreach (var s in silosService.getAllSiloses().Values)
            {
                foreach (var w in s.getSortedByNumberWires().Values)
                {
                    if (!w.Enable)
                        continue;

                    var temp = silosService.getLastTempForWire(w);
                    var overheatCount = 0;
                    for (var i = 0; i < temp.Length; i++)
                    {
                        if (temp[i] > s.Red)
                            overheatCount++;
                    }

                    if (overheatCount >= settingsService.OverheatMinimumSensorToTrigger)
                    {
                        if (!result.ContainsKey(s))
                            result.Add(s, new Dictionary<Wire, int>());

                        result[s].Add(w, overheatCount);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Возвращает все силосы с перегревом. Не обновляет временные метки.
        /// </summary>
        /// <returns>Ключ - id силоса, Значение - словарь, где ключ - id подвески, значение - количество сенсеров с привышением.</returns>
        static public Dictionary<Silos, Dictionary<Wire, int>> getAllOverheatSiloses(SilosService silosService, SettingsService settingsService)
        {
            var result = new Dictionary<Silos, Dictionary<Wire, int>>();

            if (silosService == null || settingsService == null)
                return result;

            foreach (var s in silosService.getAllSiloses().Values)
            {
                foreach (var w in s.getSortedByNumberWires().Values)
                {
                    if (!w.Enable)
                        continue;

                    var temp = silosService.getLastTempForWire(w);
                    var overheatCount = 0;
                    for (var i = 0; i < temp.Length; i++)
                    {
                        if (temp[i] > s.Red)
                            overheatCount++;
                    }

                    if (overheatCount >= settingsService.OverheatMinimumSensorToTrigger)
                    {
                        if (!result.ContainsKey(s))
                            result.Add(s, new Dictionary<Wire, int>());

                        result[s].Add(w, overheatCount);
                    }
                }
            }

            return result;
        }



    }
}
