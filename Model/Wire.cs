using System.Runtime.Serialization;

namespace SystemOfTermometry2.Model;


/// <summary>
/// Подвеска с сенсорами. Сенсоры имеют индексы от 0 до их кол-ва не включительно
/// </summary>
[DataContract]
public class Wire
{
    private int id; // уникальный идентификатор подвески
    private int number; //Номер подвески в силосе.
    private int silosId; // id силоса, в котором весит подвеска
    private byte deviceAddress; // адресс платы, с которой соединена подвеска
    private ushort leg; // номер ножки от 1 до 10, на которой висит подвеска
    private ushort sensorCount; // количество сенсоров на плате
    private bool enable; // Опрашивается плата или нет
    private WireTypeEnum type; // тип платы

    //координаты подвески в силосе
    private float x;  //Значения от 0 до 1, где 0 -самый левый край, а 1 - самый правый
    private float y; //Значения от 0 до 1, где 0 -самый верхний край, а 1 - самый нижний

    [DataMember]
    public int Id
    {
        get => id; set => id = value;
    }
    [DataMember]
    public int Number
    {
        get => number; set => number = value;
    }
    [DataMember]
    public int SilosId
    {
        get => silosId; set => silosId = value;
    }
    [DataMember]
    public byte DeviceAddress
    {
        get => deviceAddress; set => deviceAddress = value;
    }
    [DataMember]
    public ushort Leg
    {
        get => leg; set => leg = value;
    }
    [DataMember]
    public ushort SensorCount
    {
        get => sensorCount; set => sensorCount = value;
    }
    [DataMember]
    public bool Enable
    {
        get => enable; set => enable = value;
    }
    [DataMember]
    public WireTypeEnum Type
    {
        get => type; set => type = value;
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


    public Wire()
    {

    }

    public Wire(int id, int silosId, byte deviceAddress, ushort leg, ushort sensorCount, bool enable, WireTypeEnum type, float x, float y)
    {
        this.id = id;
        this.silosId = silosId;
        this.deviceAddress = deviceAddress;
        this.leg = leg;
        this.sensorCount = sensorCount;
        this.enable = enable;
        this.type = type;
        this.x = x;
        this.y = y;
    }

    public override bool Equals(object obj)
    {
        return obj is Wire wire &&
               id == wire.id &&
               deviceAddress == wire.deviceAddress &&
               leg == wire.leg;
    }

    public override string ToString()
    {
        return "Wire. adr: " + deviceAddress + " leg: " + leg + " sens: " + sensorCount.ToString() + " silos: " + SilosId
            + (Enable ? " Enabled" : " Disabled");
    }

    public override int GetHashCode()
    {
        var hashCode = -1783375424;
        hashCode = hashCode * -1521134295 + id.GetHashCode();
        hashCode = hashCode * -1521134295 + deviceAddress.GetHashCode();
        hashCode = hashCode * -1521134295 + leg.GetHashCode();
        return hashCode;
    }
}
