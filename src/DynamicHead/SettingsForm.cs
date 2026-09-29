namespace DynamicHead;

/// <summary>
/// Окно настроек: выбор устройств «Колонки» и «Наушники» для переключения.
/// </summary>
public sealed class SettingsForm : Form
{
    private readonly AppConfig _config;
    private readonly AudioSwitcher _switcher;

    private ComboBox _comboA = null!;
    private ComboBox _comboB = null!;
    private TextBox _labelA = null!;
    private TextBox _labelB = null!;
    private Button _btnSave = null!;
    private Button _btnCancel = null!;
    private Label _statusLabel = null!;

    /// <summary>Событие: настройки сохранены и применены.</summary>
    public event EventHandler? ConfigApplied;

    public SettingsForm(AppConfig config, AudioSwitcher switcher)
    {
        _config = config;
        _switcher = switcher;

        BuildUi();
        LoadDevices();
    }

    /// <summary>Создание интерфейса окна.</summary>
    private void BuildUi()
    {
        Text = "Dynamic-Head — настройки";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(460, 250);
        Font = new Font("Segoe UI", 9F);

        var label1 = new Label
        {
            Text = "Устройство 1 (первое состояние):",
            Location = new Point(16, 18),
            AutoSize = true
        };

        _comboA = new ComboBox
        {
            Location = new Point(16, 40),
            Width = 300,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _labelA = new TextBox
        {
            Location = new Point(324, 40),
            Width = 120,
            PlaceholderText = "Колонки"
        };

        var label2 = new Label
        {
            Text = "Устройство 2 (второе состояние):",
            Location = new Point(16, 78),
            AutoSize = true
        };

        _comboB = new ComboBox
        {
            Location = new Point(16, 100),
            Width = 300,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        _labelB = new TextBox
        {
            Location = new Point(324, 100),
            Width = 120,
            PlaceholderText = "Наушники"
        };

        var hint = new Label
        {
            Text = "Слева — устройство вывода, справа — подпись в меню трея.",
            Location = new Point(16, 132),
            AutoSize = true,
            ForeColor = SystemColors.GrayText
        };

        _statusLabel = new Label
        {
            Location = new Point(16, 156),
            Size = new Size(428, 34),
            ForeColor = Color.DarkRed
        };

        _btnSave = new Button
        {
            Text = "Сохранить",
            Location = new Point(256, 200),
            Size = new Size(90, 30)
        };
        _btnSave.Click += (_, _) => SaveConfig();

        _btnCancel = new Button
        {
            Text = "Закрыть",
            Location = new Point(354, 200),
            Size = new Size(90, 30)
        };
        _btnCancel.Click += (_, _) => Hide();

        Controls.AddRange(new Control[]
        {
            label1, _comboA, _labelA,
            label2, _comboB, _labelB,
            hint, _statusLabel,
            _btnSave, _btnCancel
        });

        AcceptButton = _btnSave;
        CancelButton = _btnCancel;
    }

    /// <summary>Загрузить список устройств и выделить сохранённые.</summary>
    private void LoadDevices()
    {
        List<AudioDevice> devices = _switcher.GetRenderDevices();

        _comboA.Items.Clear();
        _comboB.Items.Clear();

        foreach (AudioDevice device in devices)
        {
            _comboA.Items.Add(device);
            _comboB.Items.Add(device);
        }

        SelectDevice(_comboA, _config.DeviceAId, _config.DeviceAName);
        SelectDevice(_comboB, _config.DeviceBId, _config.DeviceBName);

        _labelA.Text = _config.LabelA;
        _labelB.Text = _config.LabelB;

        if (devices.Count < 2)
        {
            _statusLabel.Text = "Нужно минимум два устройства вывода. Подключите колонки или наушники и перезапустите настройки.";
        }
        else
        {
            _statusLabel.Text = string.Empty;
        }
    }

    /// <summary>Выбрать в списке устройство по id, при неудаче — по имени.</summary>
    private static void SelectDevice(ComboBox combo, string id, string name)
    {
        for (int i = 0; i < combo.Items.Count; i++)
        {
            if (combo.Items[i] is AudioDevice device &&
                !string.IsNullOrWhiteSpace(id) &&
                string.Equals(device.Id, id, StringComparison.OrdinalIgnoreCase))
            {
                combo.SelectedIndex = i;
                return;
            }
        }

        for (int i = 0; i < combo.Items.Count; i++)
        {
            if (combo.Items[i] is AudioDevice device &&
                !string.IsNullOrWhiteSpace(name) &&
                string.Equals(device.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                combo.SelectedIndex = i;
                return;
            }
        }

        if (combo.Items.Count > 0 && combo.SelectedIndex < 0)
        {
            combo.SelectedIndex = 0;
        }
    }

    /// <summary>Проверить и применить выбранные устройства к настройкам.</summary>
    private void SaveConfig()
    {
        if (_comboA.SelectedItem is not AudioDevice deviceA ||
            _comboB.SelectedItem is not AudioDevice deviceB)
        {
            _statusLabel.ForeColor = Color.DarkRed;
            _statusLabel.Text = "Выберите оба устройства вывода.";
            return;
        }

        if (string.Equals(deviceA.Id, deviceB.Id, StringComparison.OrdinalIgnoreCase))
        {
            _statusLabel.ForeColor = Color.DarkRed;
            _statusLabel.Text = "Устройства не должны совпадать.";
            return;
        }

        _config.DeviceAId = deviceA.Id;
        _config.DeviceAName = deviceA.Name;
        _config.DeviceBId = deviceB.Id;
        _config.DeviceBName = deviceB.Name;
        _config.LabelA = string.IsNullOrWhiteSpace(_labelA.Text) ? "Колонки" : _labelA.Text.Trim();
        _config.LabelB = string.IsNullOrWhiteSpace(_labelB.Text) ? "Наушники" : _labelB.Text.Trim();

        _statusLabel.ForeColor = Color.DarkGreen;
        _statusLabel.Text = "Сохранено.";

        ConfigApplied?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Закрытие не завершает приложение — окно просто прячется.</summary>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }

        base.OnFormClosing(e);
    }
}
