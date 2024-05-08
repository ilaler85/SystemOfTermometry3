using System.Runtime.Serialization;

namespace SystemOfTermometry2.Model;

/// <summary>
/// Представляет собой струкурное объединение - одно из свойств силоса
/// </summary>
public class StructureSubdivision
{

    private int id;
    private string name;

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


    public StructureSubdivision()
    {
        Id = -1;
        Name = "";
    }

    public StructureSubdivision(int id, string name)
    {
        Id = id;
        Name = name;
    }



    public override bool Equals(object obj)
    {
        return obj is StructureSubdivision subdivision &&
               Id == subdivision.Id;
    }

    public override int GetHashCode()
    {
        return 1877310944 + Id.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }


}
