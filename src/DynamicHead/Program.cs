namespace DynamicHead;

/// <summary>Точка входа приложения: скрытый контекст WinForms и иконка в трее.</summary>
internal static class Program
{
    /// <summary>Признак запуска автозагрузкой — окно настроек показывать не нужно.</summary>
    public static bool StartMinimized { get; set; }

    private static TrayApp? _trayApp;

    [STAThread]
    private static void Main(string[] args)
    {
        // Запуск из автозагрузки — сразу в трей, без окон
        StartMinimized = args.Any(a => string.Equals(a, "--tray", StringComparison.OrdinalIgnoreCase));

        // Один экземпляр приложения на пользователя
        using var singleInstance = new Mutex(true, @"Local\DynamicHead_SingleInstance", out bool isFirstInstance);
        if (!isFirstInstance)
        {
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.ThreadException += (_, e) => ShowFatalError(e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => ShowFatalError(e.ExceptionObject as Exception);

        _trayApp = new TrayApp();

        try
        {
            Application.Run();
        }
        finally
        {
            _trayApp?.Dispose();
        }
    }

    /// <summary>Сообщить о непредвиденной ошибке, не роняя приложение молча.</summary>
    private static void ShowFatalError(Exception? exception)
    {
        string message = exception?.Message ?? "Неизвестная ошибка";
        MessageBox.Show(
            $"Произошла ошибка:\n{message}",
            "Dynamic-Head",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
