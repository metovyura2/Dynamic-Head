using System.Drawing;
using Microsoft.Win32;

namespace DynamicHead;

/// <summary>
/// Логика приложения в системном трее: один тумблер между двумя устройствами вывода.
/// </summary>
public sealed class TrayApp : IDisposable
{
    private const string StartupRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string StartupValueName = "Dynamic-Head";

    private readonly AudioSwitcher _switcher = new();
    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _menu;
    private readonly ToolStripMenuItem _toggleItem;
    private readonly ToolStripMenuItem _settingsItem;
    private readonly ToolStripMenuItem _refreshItem;
    private readonly ToolStripMenuItem _startupItem;
    private readonly ToolStripMenuItem _exitItem;

    private AppConfig _config;
    private SettingsForm? _settingsForm;

    public TrayApp()
    {
        _config = AppConfig.Load();

        _toggleItem = new ToolStripMenuItem("Переключить");
        _toggleItem.Click += (_, _) => ToggleDevice();

        _settingsItem = new ToolStripMenuItem("Настройки…");
        _settingsItem.Click += (_, _) => ShowSettings();

        _refreshItem = new ToolStripMenuItem("Обновить список устройств");
        _refreshItem.Click += (_, _) => RefreshState();

        _startupItem = new ToolStripMenuItem("Автозапуск при входе")
        {
            CheckOnClick = true,
            Checked = IsStartupEnabled()
        };
        _startupItem.CheckedChanged += (_, _) => SetStartup(_startupItem.Checked);

        _exitItem = new ToolStripMenuItem("Выход");
        _exitItem.Click += (_, _) => ExitApp();

        _menu = new ContextMenuStrip();
        _menu.Items.Add(_toggleItem);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(_settingsItem);
        _menu.Items.Add(_refreshItem);
        _menu.Items.Add(_startupItem);
        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(_exitItem);

        _trayIcon = new NotifyIcon
        {
            Visible = true,
            ContextMenuStrip = _menu,
            Text = "Dynamic-Head"
        };
        _trayIcon.MouseClick += OnTrayMouseClick;
        _trayIcon.DoubleClick += (_, _) => ShowSettings();

        RefreshState();

        // Первый запуск без настроек — сразу предлагаем выбрать устройства
        if (!_config.IsComplete)
        {
            ShowSettings();
        }
    }

    /// <summary>Обработка кликов по иконке в трее: левый — тумблер, правый — меню.</summary>
    private void OnTrayMouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            ToggleDevice();
        }
    }

    /// <summary>Переключить активное устройство на противоположное.</summary>
    private void ToggleDevice()
    {
        if (!_config.IsComplete)
        {
            ShowSettings();
            return;
        }

        string targetSlot = _config.ActiveSlot == "A" ? "B" : "A";
        SwitchTo(targetSlot);
    }

    /// <summary>Переключение на указанное устройство (A или B).</summary>
    private void SwitchTo(string slot)
    {
        string id = slot == "A" ? _config.DeviceAId : _config.DeviceBId;
        string name = slot == "A" ? _config.DeviceAName : _config.DeviceBName;

        AudioDevice? device = _switcher.Resolve(id, name);
        if (device == null)
        {
            ShowBalloon($"Устройство «{(slot == "A" ? _config.LabelA : _config.LabelB)}» не найдено. Откройте настройки.", ToolTipIcon.Warning);
            return;
        }

        if (!_switcher.SetDefaultDevice(device.Id) || !_switcher.IsCurrentlyDefault(device.Id))
        {
            ShowBalloon("Не удалось переключить устройство вывода.", ToolTipIcon.Error);
            return;
        }

        // Запоминаем фактический идентификатор и активный слот
        if (slot == "A")
        {
            _config.DeviceAId = device.Id;
            _config.DeviceAName = device.Name;
        }
        else
        {
            _config.DeviceBId = device.Id;
            _config.DeviceBName = device.Name;
        }

        _config.ActiveSlot = slot;
        _config.Save();

        UpdateTrayPresentation();
    }

    /// <summary>Синхронизация состояния с системой (иконка, подписи).</summary>
    public void RefreshState()
    {
        string? currentId = _switcher.GetDefaultDeviceId();

        if (currentId != null)
        {
            if (!string.IsNullOrWhiteSpace(_config.DeviceAId) &&
                string.Equals(currentId, _config.DeviceAId, StringComparison.OrdinalIgnoreCase))
            {
                _config.ActiveSlot = "A";
            }
            else if (!string.IsNullOrWhiteSpace(_config.DeviceBId) &&
                     string.Equals(currentId, _config.DeviceBId, StringComparison.OrdinalIgnoreCase))
            {
                _config.ActiveSlot = "B";
            }
        }

        UpdateTrayPresentation();
    }

    /// <summary>Обновление иконки, подсказки и пункта-тумблера в трее.</summary>
    private void UpdateTrayPresentation()
    {
        bool slotA = _config.ActiveSlot == "A";
        string currentLabel = slotA ? _config.LabelA : _config.LabelB;
        string otherLabel = slotA ? _config.LabelB : _config.LabelA;

        _toggleItem.Text = $"Переключить на «{otherLabel}»";
        _trayIcon.Text = Truncate($"Dynamic-Head: {currentLabel}");
        _trayIcon.Icon = IconLoader.Load(slotA ? "app-speakers.ico" : "app-headphones.ico");
    }

    /// <summary>NotifyIcon.Text ограничен 63 символами.</summary>
    private static string Truncate(string value) =>
        value.Length > 63 ? value[..63] : value;

    /// <summary>Показать всплывающее уведомление из трея.</summary>
    private void ShowBalloon(string message, ToolTipIcon icon) =>
        _trayIcon.ShowBalloonTip(4000, "Dynamic-Head", Truncate(message), icon);

    /// <summary>Открыть окно настроек (единственный экземпляр).</summary>
    private void ShowSettings()
    {
        if (_settingsForm != null && !_settingsForm.IsDisposed)
        {
            _settingsForm.Activate();
            if (_settingsForm.WindowState == FormWindowState.Minimized)
            {
                _settingsForm.WindowState = FormWindowState.Normal;
            }

            return;
        }

        _settingsForm = new SettingsForm(_config, _switcher);
        _settingsForm.ConfigApplied += (_, _) =>
        {
            _config.Save();
            UpdateTrayPresentation();
        };
        _settingsForm.Show();
    }

    /// <summary>Проверить, включён ли автозапуск в реестре.</summary>
    private static bool IsStartupEnabled()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, false);
            return key?.GetValue(StartupValueName) != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Включить или выключить автозапуск при входе пользователя.</summary>
    private void SetStartup(bool enabled)
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true);
            if (key == null)
            {
                return;
            }

            if (enabled)
            {
                string exePath = Environment.ProcessPath ?? Application.ExecutablePath;
                key.SetValue(StartupValueName, $"\"{exePath}\" --tray");
            }
            else
            {
                key.DeleteValue(StartupValueName, false);
            }

            _config.RunAtStartup = enabled;
            _config.Save();
        }
        catch
        {
            // Отсутствие прав на запись в реестр не критично
        }
    }

    /// <summary>Корректное завершение приложения.</summary>
    private void ExitApp()
    {
        _trayIcon.Visible = false;
        Dispose();
        Application.Exit();
    }

    public void Dispose()
    {
        _trayIcon.Dispose();
        _menu.Dispose();
        _settingsForm?.Dispose();
    }
}

/// <summary>Загрузка иконок из папки assets рядом с exe.</summary>
internal static class IconLoader
{
    private static readonly Dictionary<string, Icon> Cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Вернуть иконку по имени файла; null — если файла нет.</summary>
    public static Icon? Load(string fileName)
    {
        if (Cache.TryGetValue(fileName, out Icon? cached))
        {
            return cached;
        }

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string[] candidates =
        {
            Path.Combine(baseDir, "assets", fileName),
            Path.Combine(baseDir, fileName)
        };

        foreach (string path in candidates)
        {
            if (!File.Exists(path))
            {
                continue;
            }

            try
            {
                Icon icon = new(path);
                Cache[fileName] = icon;
                return icon;
            }
            catch
            {
                // Повреждённая иконка — пробуем следующую
            }
        }

        return null;
    }
}
