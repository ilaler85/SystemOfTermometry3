using System;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace SystemOfTermometry2.Services;

public class SecurityService
{
    /// <summary>
    /// Проверяет на подлинность пароль администратора
    /// </summary>
    /// <param name="password">пароль</param>
    /// <returns>подлинность</returns>
    public static bool checkAdminPassword(string password)
    {
        var pswdSalt = FileProcessingService.getHashPassword("admin");
        if (pswdSalt == null) //Пароль не был установлен
        {
            return true;
        }
        return checkPassword(password, pswdSalt[0], pswdSalt[1]);
    }

    /// <summary>
    /// Устанавливает пароль администратора
    /// </summary>
    /// <param name="password">пароль</param>
    public static void setAdminPassword(string password)
    {
        using (var deriveBytes = new Rfc2898DeriveBytes(password, 20))
        {
            var salt = deriveBytes.Salt;
            var key = deriveBytes.GetBytes(20);  // derive a 20-byte key

            FileProcessingService.setHashPassword("admin", key, salt);
        }
    }

    /// <summary>
    /// Проверяет на подлинность пароль начальника смены
    /// </summary>
    /// <param name="password">пароль</param>
    /// <returns>подлинность</returns>
    public static bool checkMainOperPassword(string password)
    {
        var pswdSalt = FileProcessingService.getHashPassword("operator");
        if (pswdSalt == null) //Пароль не был установлен
        {
            return true;
        }

        return checkPassword(password, pswdSalt[0], pswdSalt[1]);
    }

    /// <summary>
    /// Устанавливает пароль начальника смены
    /// </summary>
    /// <param name="password">пароль</param>
    public static void setOperatorPassword(string password)
    {
        using (var deriveBytes = new Rfc2898DeriveBytes(password, 20))
        {
            var salt = deriveBytes.Salt;
            var key = deriveBytes.GetBytes(20);  // derive a 20-byte key

            FileProcessingService.setHashPassword("operator", key, salt);
        }
    }


    /// <summary>
    /// Проверяет на подлинность пароль поставщика
    /// </summary>
    /// <param name="password">пароль</param>
    /// <returns>подлинность</returns>
    public static bool checkProviderPassword(string password)
    {
        var pswdSalt = FileProcessingService.getHashPassword("provider");
        if (pswdSalt == null) //Пароль не был установлен
        {
            return password.Equals("VRN VIK");
            //return true;
        }

        return checkPassword(password, pswdSalt[0], pswdSalt[1]);
    }

    /// <summary>
    /// Устанавливает пароль поставщика
    /// </summary>
    /// <param name="password">пароль</param>
    public static void setProviderPassword(string password)
    {
        using (var deriveBytes = new Rfc2898DeriveBytes(password, 20))
        {
            var salt = deriveBytes.Salt;
            var key = deriveBytes.GetBytes(20);  // derive a 20-byte key

            FileProcessingService.setHashPassword("provider", key, salt);
        }
    }

    /// <summary>
    /// Сравнивает пароль с хеш функцией имеющегося пароля
    /// </summary>
    /// <param name="password">пароль</param>
    /// <param name="oldPswdHash">хэш функция имеющегося пароля</param>
    /// <param name="salt">соль</param>
    /// <returns>результат сравнения</returns>
    private static bool checkPassword(string password, byte[] oldPswdHash, byte[] salt)
    {
        using (var deriveBytes = new Rfc2898DeriveBytes(password, salt))
        {
            var newKey = deriveBytes.GetBytes(20);  // derive a 20-byte key
            if (!newKey.SequenceEqual(oldPswdHash))
                return false;
        }
        return true;
    }


    /// <summary>
    /// Проверяет, активирована ли программа или нет
    /// </summary>
    /// <returns></returns>
    public static bool IsActivate()
    {
        var key = FileProcessingService.getStringFromFile("ActivateKey");
        return key.CompareTo(getActivateKeyFromHardware()) == 0;
    }

    /// <summary>
    /// Проверяет правильность ключа
    /// </summary>
    public static bool CheckActivateKey(string key)
    {
        if (key.CompareTo(getActivateKeyFromHardware()) == 0)
        {
            FileProcessingService.setStringToFile("ActivateKey", key);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static string GetActivateKeyRequestFromHardware()
    {
        var request = "69EF0CEC0ACC501F21718BE625C29BF6";
        var mbInfo = "69EF0CEC0ACC501F21718BE625C29BF6";
        try
        {
            var scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
            scope.Connect();
            var wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions());

            foreach (var propData in wmiClass.Properties)
            {
                if (propData.Name == "SerialNumber" || propData.Name == "Manufacturer")
                {
                    mbInfo += propData.Value.ToString();
                }
            }

            request = ComputeHash(mbInfo);

            //throw new Exception("qwerty");
        }
        catch (Exception e)
        {
            //return e.Message;
            //throw new Exception("Не удалось получить доступ к \nинформации о комьютере!");
        }

        return request;
    }

    private static string getActivateKeyFromHardware()
    {
        var mbInfo = "69EF0CEC0ACC501F21718BE625C29BF6";
        try
        {

            var scope = new ManagementScope("\\\\" + Environment.MachineName + "\\root\\cimv2");
            scope.Connect();
            var wmiClass = new ManagementObject(scope, new ManagementPath("Win32_BaseBoard.Tag=\"Base Board\""), new ObjectGetOptions());

            foreach (var propData in wmiClass.Properties)
            {
                if (propData.Name == "SerialNumber" || propData.Name == "Manufacturer")
                {
                    mbInfo += propData.Value.ToString();
                }
            }
        }
        catch (Exception e)
        {
            //throw new Exception("Не удалось получить доступ к \nинформации о комьютере!");
        }

        return ComputeHash(ComputeHash(mbInfo));

    }

    private static string ComputeHash(string str)
    {
        byte[] tmpSource;
        byte[] tmpHash;
        tmpSource = Encoding.ASCII.GetBytes(str);
        tmpHash = new MD5CryptoServiceProvider().ComputeHash(tmpSource);
        int i;
        var sOutput = new StringBuilder(tmpHash.Length);
        for (i = 0; i < tmpHash.Length; i++)
        {
            sOutput.Append(tmpHash[i].ToString("X2"));
        }

        var hash = sOutput.ToString();
        return hash;
    }
}
