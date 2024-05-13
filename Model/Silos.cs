using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System;

namespace SystemOfThermometry3.Model;

/// <summary>
/// Силос.
/// </summary>
[DataContract]
public class Silos
{
    private static string pattern = "^[a-zA-Z0-9\\\\_]{1,15}$"; //регулярное выражение для имени

    /// <summary>
    /// Говорит о том, будет ли данное имя корректно
    /// </summary>
    /// <param name="name">новое имя</param>
    /// <returns>true - подходит</returns>
    public static bool validateName(string name)
    {
        if (name.Length == 0)
        {
            return false;
        }
        try
        {
            return Regex.IsMatch(name, pattern);
        }
        catch
        {
            return false;
        }
    }

    public override bool Equals(object obj)
    {
        var silos = obj as Silos;
        return silos != null &&
               id == silos.id;
    }

    public override int GetHashCode()
    {
        return 1877310944 + id.GetHashCode();
    }

    public bool IsMini
    {
        get => w == 1;
        set
        {
            if (value)
                w = 1;
            else
                w = 0;
        }
    }


    private int id; // уникальный идентификатор силоса
    private string name; // имя силоса

    private float max; // максимальная температура в силосе на последний момет
    private float mid; // средняя температура в силосе
    private float min; // минимальная темперптура в силосе
    private float red; // граница температуры, после которой силос становится красным
    private float yellow; //граница температуры, после которой силос становится желтым

    private int structureId; //ИД структурного подразделения.
    private int grainId; //ид типа зерна
    private string grainName; //название типа зерна
    private float x; // координаты на экране
    private float y;
    private int w; // закодим сюда размер, говно // 0 - большой, 1 - маленький
    private int h;
    private SilosShapeEnum shape; // тип силоса

    private Dictionary<int, Wire> wires; // все подвески в силосе. Ключ - id подвески

    [DataMember]
    public int Id
    {
        get => id; set => id = value;
    }
    [DataMember]
    public string Name
    {
        get => name; set => name = value;
    }
    [DataMember]
    public float Max
    {
        get => max; set => max = value;
    }
    [DataMember]
    public float Mid
    {
        get => mid; set => mid = value;
    }
    [DataMember]
    public float Min
    {
        get => min; set => min = value;
    }
    [DataMember]
    public float Red
    {
        get => red; set => red = value;
    }
    [DataMember]
    public float Yellow
    {
        get => yellow; set => yellow = value;
    }
    [DataMember]
    public int StructureId
    {
        get => structureId; set => structureId = value;
    }
    [DataMember]
    public float X
    {
        get => x; set => x = value;
    }
    [DataMember]
    public float Y
    {
        get => y; set => y = value;
    }
    [DataMember]
    public int W
    {
        get => w; set => w = value;
    }
    [DataMember]
    public int H
    {
        get => h; set => h = value;
    }
    [DataMember]
    public SilosShapeEnum Shape
    {
        get => shape; set => shape = value;
    }
    [DataMember]
    public Dictionary<int, Wire> Wires
    {
        get => wires; set => wires = value;
    }
    [DataMember]
    public int GrainId
    {
        get => grainId; set => grainId = value;
    }
    [DataMember]
    public string GrainName
    {
        get => grainName; set => grainName = value;
    }

    public Silos()
    {
        id = -1;
        name = "tmpName";
        max = -99;
        mid = -99;
        min = 150;
        Shape = SilosShapeEnum.BIG;
        Wires = new Dictionary<int, Wire>();
        structureId = -1;
        grainId = -1;
        grainName = "-";
    }

    public Silos(int id, string name, float max, float mid, float min, float red, float yellow, int grainId, string grainName)
    {
        Id = id;
        Name = name;
        Max = max;
        Mid = mid;
        Min = min;
        Red = red;
        Yellow = yellow;
        Shape = SilosShapeEnum.BIG;
        Wires = new Dictionary<int, Wire>();
        structureId = -1;
        this.grainId = grainId;
        this.grainName = grainName;
    }

    public Silos(int id, string name, float max, float mid, float min, float red, float yellow, int x, int y, int w, int h, SilosShapeEnum shape, int grainId, string grainName) : this(id, name, max, mid, min, red, yellow, grainId, grainName)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
        this.shape = shape;
        structureId = -1;
    }

    public void Update(Silos silos)
    {
        Id = silos.id;
        Name = silos.name;
        Max = silos.max;
        Mid = silos.mid;
        Min = silos.min;
        Red = silos.red;
        Yellow = silos.yellow;
        Shape = silos.shape;
        Wires = silos.wires;
        x = silos.x;
        y = silos.y;
        w = silos.w;
        h = silos.h;
        shape = silos.shape;
        structureId = silos.structureId;
        grainId = silos.grainId;
        grainName = silos.grainName;

    }

    public override string ToString()
    {
        return name;
    }

    public int getEnabledWireCount()
    {
        return Wires.Count((wirePair) =>
        {
            return wirePair.Value.Enable;
        });
    }

    public List<Wire> getEnabledWires()
    {
        var result = new List<Wire>();
        foreach (var w in wires.Values)
        {
            if (w.Enable)
                result.Add(w);
        }

        return result;
    }

    public Dictionary<int, Wire> getSortedByNumberWires()
    {
        var myList = wires.ToList();
        myList.Sort((pair1, pair2) => pair1.Value.Number.CompareTo(pair2.Value.Number));
        return myList.ToDictionary(key => key.Key, key => key.Value);
    }
    public Dictionary<int, Wire> getSortedByXWires()
    {

        var leftWire = new List<Wire>();
        var rightWire = new List<Wire>();

        foreach (var w in wires.Values)
        {
            if (w.X < 0.5)
                leftWire.Add(w);
            else
                rightWire.Add(w);
        }

        leftWire.Sort((pair1, pair2) => Math.Sqrt(Math.Pow(pair2.X - 0.5, 2.0) + Math.Pow(pair2.Y - 0.5, 2.0)).
        CompareTo(Math.Sqrt(Math.Pow(pair1.X - 0.5, 2.0) + Math.Pow(pair1.Y - 0.5, 2.0))));


        rightWire.Sort((pair1, pair2) => Math.Sqrt(Math.Pow(pair1.X - 0.5, 2.0) + Math.Pow(pair1.Y - 0.5, 2.0)).
        CompareTo(Math.Sqrt(Math.Pow(pair2.X - 0.5, 2.0) + Math.Pow(pair2.Y - 0.5, 2.0))));
        leftWire.AddRange(rightWire);


        return leftWire.ToDictionary(key => key.Number, key => key);
    }


    public void refreshTemperature(Dictionary<int, float[]> wiresTemp)
    {
        min = 150;
        max = -99;
        mid = -99;
        float sum = 0;
        var count = 0;
        foreach (var w in wiresTemp)
        {
            if (wires.ContainsKey(w.Key))
            {
                foreach (var temp in w.Value)
                {
                    if (temp < -90 || temp > 140)
                        continue;

                    min = min > temp ? temp : min;
                    max = max < temp ? temp : max;
                    sum += temp;
                    count++;
                }
            }
        }

        if (count != 0)
            mid = sum / count;

    }
}
