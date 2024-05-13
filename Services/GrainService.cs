using System;
using System.Collections.Generic;
using System.Linq;
using SystemOfThermometry3.Model;
using SystemOfThermometry3.Services;

namespace SystemOfThermometry3.Services
{
    public class GrainService
    {
        private DAO.Dao dao;
        private SettingsService settingsService;
        private Dictionary<int, Grain> grains;
        public delegate void UpdateGrain();
        public event UpdateGrain updateGrain;


        public GrainService(DAO.Dao dao, SettingsService settingsService)
        {
            this.dao = dao ?? throw new ArgumentNullException("dao is null");
            this.settingsService = settingsService ?? throw new ArgumentNullException("setting service is null");
            grains = new Dictionary<int, Grain>();
            loadGrains();

        }



        public void loadGrains()
        {
            grains = dao.getAllGrains();
            if (grains == null)
                grains = new Dictionary<int, Grain>();
        }

        public void reInitService(Dictionary<int, Grain> graines)
        {
            grains = graines ?? throw new ArgumentNullException("Grain is null");

        }

        public void synchronizeWithGettingData()
        {
            settingsService.OfflineMode = false;
            settingsService.synchronizeWithGettingData();
            loadGrains();
        }

        public Grain addGrain()
        {
            var grain = new Grain();
            if (settingsService.OfflineMode)
            {
                // Ключ выбирается как максимальный из ключей + 1
                if (grains.Keys.Count == 0)
                    grain.ID = 1;
                else
                    grain.ID = grains.Keys.Max() + 1;
                grains.Add(grain.ID, grain);
                MyLoger.Log("Grain: add grain offline " + grain.Name);
                return grain;
            }

            var newId = dao.addGrain(grain);
            if (newId == -1)
            {
                MyLoger.Log("Grain: add grain error " + grain.Name);
                return null;
            }

            grain.ID = newId;
            grains.Add(newId, grain);
            MyLoger.Log("Grain: add grain " + grain.Name);
            return grain;
        }

        public bool updateGrains(Grain g)
        {
            var res = dao.updateGrain(g);
            grains[g.ID].Update(g);
            updateGrain?.Invoke();
            return res;

        }

        public bool deleteGrains(Grain g)
        {
            var res = dao.deleteGrain(g.ID);
            grains.Remove(g.ID);
            return res;
        }


        public Dictionary<int, Grain> getGrains()
        {
            return grains;
        }
    }
}
