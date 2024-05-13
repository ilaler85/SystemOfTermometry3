using System.Runtime.Serialization;

namespace SystemOfThermometry3.Model;

[DataContract]
public class Grain
{
    private int id; // уникальный идентификатор зерна
    private string name; // имя зерна
    private float redTemp;
    private float yellowTemp;



    [DataMember]
    public int ID
    {
        get => id; set => id = value;
    }

    [DataMember]
    public string Name
    {
        get => name; set => name = value;
    }

    [DataMember]
    public float RedTemp
    {
        get => redTemp; set => redTemp = value;
    }

    [DataMember]
    public float YellowTemp
    {
        get => yellowTemp; set => yellowTemp = value;
    }

    public override string ToString()
    {
        return name;
    }

    public Grain()
    {
        Name = "Зерновая культура";
        RedTemp = 25;
        YellowTemp = 20;
    }

    public void Update(string name, float redTemp, float yellowTemp)
    {
        this.name = name;
        this.redTemp = redTemp;
        this.yellowTemp = yellowTemp;
    }

    public void Update(Grain g)
    {
        name = g.name;
        redTemp = g.redTemp;
        yellowTemp = g.yellowTemp;
    }

}
