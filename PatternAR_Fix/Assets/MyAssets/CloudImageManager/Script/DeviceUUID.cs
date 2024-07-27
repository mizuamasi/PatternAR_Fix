using UnityEngine;

public class DeviceUUID : MonoBehaviour
{
    public static string GetUUID()
    {
        string uuid = SystemInfo.deviceUniqueIdentifier;
        return uuid;
    }
}
