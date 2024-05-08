namespace SystemOfTermometry2.Model;

/// <summary>
/// Перечислений возможных типов термоподвесок.
/// </summary>
public enum WireTypeEnum
{
    TOP_TO_BOT_DS18b20, //Сенсоры считаются сверху вниз. Сенсоры типа DS18b20. По умолчанию
    BOT_TO_TOP_DS18b20, //Сенсоры считаются снизу вверх. Сенсоры типа DS18b20
    TOP_TO_BOT_DS1820, //Сенсоры считаются сверху вниз. Сенсоры типа DS1820
    BOT_TO_TOP_DS1820 //Сенсоры считаются снизу вверх. Сенсоры типа DS1820
}
