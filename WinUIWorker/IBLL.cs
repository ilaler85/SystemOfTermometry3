using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemOfTermometry2.Services;

namespace SystemOfTermometry2.WinUIWorker;
public interface IBisnesLogicLayer
{

    public SilosService getAllSilos();

    public GrainService getGrainService();

    public void runObserv();
    public void stopObserv();
    public void exportExcel();

    public void painthart();

    public void getFilling();
    public void calculateFilling();
    public void enterAdminMode();

    public void changeTemperature();
}
