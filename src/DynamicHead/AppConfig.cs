using System.Text.Json;
using System.Text.Json.Serialization;

namespace DynamicHead;

/// <summary>
/// Настройки приложения. Хранятся в config.json рядом с exe.
/// </summary>
public sealed class AppConfig
{
    /// <summary>Идентификатор устройства «Колонки» (пустой — не выбрано).</summary>
    public string DeviceAId { get; set; } = string.Empty;

    /// <summary>Имя устройства «Колонки» — резервный поиск, если id изменился.</summary>
    public string DeviceAName { get; set; } = string.Empty;

    /// <summary>Идентификатор устройства «Наушники» (пустой — не выбрано).</summary>
    public string DeviceBId { get; set; } = string.Empty;

    /// <summary>Имя устройства «Наушники» — резервный поиск, если id изменился.</summary>
    public string DeviceBName { get; set; } = string.Empty;

    /// <summary>Подпись первого устройства в меню трея.</summary>
    public string LabelA { get; set; } = "Колонки";

    /// <summary>Подпись второго устройства в меню трея.</summary>
    public string LabelB { get; set; } = "Наушники";

    /// <summary>Автозапуск при входе пользователя в систему.</summary>
    public bool RunAtStartup { get; set; }

    /// <summary>Последнее выбранное устройство: A или B.</summary>
    public string ActiveSlot { get; set; } = "A";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    /// <summary>Путь к файлу настроек (рядом с exe).</summary>
    public static string ConfigPath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    /// <summary>Загрузить настройки; при отсутствии/ошибке вернуть значения по умолчанию.</summary>
    public static AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                AppConfig? cfg = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
                if (cfg != null)
                {
                    return cfg;
                }
            }
        }
        catch
        {
            // Повреждённый файл не должен ронять приложение — используем значения по умолчанию
        }

        return new AppConfig();
    }

    /// <summary>Сохранить настройки на диск.</summary>
    public void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(this, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
        catch
        {
            // Ошибка записи настроек не критична для работы приложения
        }
    }

    /// <summary>Заполнены ли оба устройства.</summary>
    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(DeviceAId) && !string.IsNullOrWhiteSpace(DeviceBId);
}
