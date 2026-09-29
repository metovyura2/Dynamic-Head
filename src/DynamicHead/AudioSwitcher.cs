using System.Runtime.InteropServices;
using NAudio.CoreAudioApi;

namespace DynamicHead;

/// <summary>
/// Управление устройствами воспроизведения: получение списка и переключение
/// устройства вывода по умолчанию через Core Audio API (IPolicyConfig).
/// </summary>
public sealed class AudioSwitcher
{
    private readonly MMDeviceEnumerator _enumerator = new();

    /// <summary>Список активных устройств воспроизведения (за исключением мнимых).</summary>
    public List<AudioDevice> GetRenderDevices()
    {
        var result = new List<AudioDevice>();

        try
        {
            MMDeviceCollection devices = _enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (MMDevice device in devices)
            {
                try
                {
                    result.Add(new AudioDevice(device.ID, device.FriendlyName));
                }
                catch
                {
                    // Пропускаем устройство, которое не удалось прочитать
                }
            }
        }
        catch
        {
            // Если перечисление не удалось — вернём пустой список
        }

        return result;
    }

    /// <summary>Идентификатор текущего устройства вывода по умолчанию (мультимедиа-роль).</summary>
    public string? GetDefaultDeviceId()
    {
        try
        {
            MMDevice device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return device?.ID;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Имя текущего устройства вывода по умолчанию.</summary>
    public string? GetDefaultDeviceName()
    {
        try
        {
            MMDevice device = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            return device?.FriendlyName;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Найти устройство по идентификатору; при неудаче — по имени.</summary>
    public AudioDevice? Resolve(string id, string name)
    {
        List<AudioDevice> devices = GetRenderDevices();

        if (!string.IsNullOrWhiteSpace(id))
        {
            AudioDevice? byId = devices.FirstOrDefault(d =>
                string.Equals(d.Id, id, StringComparison.OrdinalIgnoreCase));
            if (byId != null)
            {
                return byId;
            }
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            AudioDevice? byName = devices.FirstOrDefault(d =>
                string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
            if (byName != null)
            {
                return byName;
            }
        }

        return null;
    }

    /// <summary>
    /// Сделать устройство устройством вывода по умолчанию для всех ролей
    /// (обычные приложения, мультимедиа, связь). Возвращает true при успехе.
    /// </summary>
    public bool SetDefaultDevice(string deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            return false;
        }

        try
        {
            var policyConfig = (IPolicyConfig)new PolicyConfigClient();

            // Устанавливаем устройство для всех трёх ролей, чтобы переключение
            // срабатывало и для мультимедиа, и для программ связи
            foreach (Role role in new[] { Role.Console, Role.Multimedia, Role.Communications })
            {
                int hr = policyConfig.SetDefaultEndpoint(deviceId, role);
                if (hr != 0)
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Успешное переключение = выбранный идентификатор совпадает с текущим
    /// устройством по умолчанию (проверяем после применения настроек).
    /// </summary>
    public bool IsCurrentlyDefault(string deviceId)
    {
        string? current = GetDefaultDeviceId();
        return !string.IsNullOrWhiteSpace(current) &&
               string.Equals(current, deviceId, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>Описание устройства воспроизведения.</summary>
/// <param name="Id">Системный идентификатор устройства (как в Core Audio).</param>
/// <param name="Name">Понятное имя устройства.</param>
public sealed record AudioDevice(string Id, string Name)
{
    public override string ToString() => Name;
}

/// <summary>
/// Внутренний COM-интерфейс Windows для смены устройства по умолчанию.
/// </summary>
[ComImport]
[Guid("F8679F50-850A-41CF-9C72-430F290290C8")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPolicyConfig
{
    [PreserveSig] int GetMixFormat(string pszDeviceName, IntPtr ppFormat);
    [PreserveSig] int GetDeviceFormat(string pszDeviceName, bool bDefault, IntPtr ppFormat);
    [PreserveSig] int ResetDeviceFormat(string pszDeviceName);
    [PreserveSig] int SetDeviceFormat(string pszDeviceName, IntPtr pEndpointFormat, IntPtr mixFormat);
    [PreserveSig] int GetProcessingPeriod(string pszDeviceName, bool bDefault, IntPtr pmftDefaultPeriod, IntPtr pmftMinimumPeriod);
    [PreserveSig] int SetProcessingPeriod(string pszDeviceName, IntPtr pmftPeriod);
    [PreserveSig] int GetShareMode(string pszDeviceName, IntPtr pMode);
    [PreserveSig] int SetShareMode(string pszDeviceName, IntPtr mode);
    [PreserveSig] int GetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);
    [PreserveSig] int SetPropertyValue(string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);
    [PreserveSig] int SetDefaultEndpoint(string pszDeviceName, Role role);
    [PreserveSig] int SetEndpointVisibility(string pszDeviceName, bool bVisible);
}

/// <summary>Реализация COM-класса PolicyConfig (Windows Vista и новее).</summary>
[ComImport]
[Guid("870AF99C-171D-4F9E-AF0D-E63DF40C2BC9")]
internal class PolicyConfigClient
{
}
