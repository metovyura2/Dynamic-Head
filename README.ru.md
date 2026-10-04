# Dynamic-Head

> Переключение между двумя устройствами вывода звука одним кликом в системном трее Windows.

<!-- Бейджи -->
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-0078D6.svg)](#требования)
[![Build](https://github.com/metovyura2/Dynamic-Head/actions/workflows/build.yml/badge.svg)](https://github.com/metovyura2/Dynamic-Head/actions/workflows/build.yml)
[![Release](https://img.shields.io/github/v/release/metovyura2/Dynamic-Head)](https://github.com/metovyura2/Dynamic-Head/releases)
[![GitHub stars](https://img.shields.io/github/stars/metovyura2/Dynamic-Head?style=social)](https://github.com/metovyura2/Dynamic-Head/stargazers)

**Dynamic-Head** — крошечная утилита для трея Windows, которая переключает
устройство вывода звука по умолчанию между двумя заданными — например
**«Колонки» ⇄ «Наушники»** — одним левым кликом. Больше не нужно каждый раз
лезть в `Параметры → Система → Звук`, когда подключаешь гарнитуру.

- 🇬🇧 [English version](README.md)
- 🇷🇺 **Русский** (этот файл)

<!--
  Добавь скриншоты в docs/screenshots/ и сошлись на них здесь, например:
  ![Меню трея](docs/screenshots/tray-menu.png)
-->

## Зачем это нужно

В Windows нет встроенного переключения устройства вывода звука одним кликом.
Dynamic-Head живёт в системном трее и делает ровно это: один клик — одно
переключение. Программа лёгкая (один небольшой exe), не требует установки и
хранит настройки в обычном JSON-файле рядом с собой.

## Возможности

- 🔘 **Один тумблер в трее** — левый клик по иконке мгновенно меняет устройство
  вывода по умолчанию.
- 🖱️ **Пункт меню** «Переключить на «…»» — то же самое из контекстного меню.
- 🎨 **Две разные иконки** — синяя (колонки) и зелёная (наушники), текущее
  состояние видно сразу.
- ⚙️ **Окно настроек** — выбрать любые два активных устройства вывода и задать
  им свои подписи.
- 🚀 **Автозапуск при входе** — необязательная запись в
  `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`.
- 💾 **Портативные настройки** — `config.json` рядом с exe.
- 🔊 **Все роли звука** — переключение применяется сразу к ролям Console,
  Multimedia и Communications.
- 🛟 **Устойчивый поиск** — если id устройства изменился (сменился драйвер/порт),
  программа ищет устройство по имени.

## Требования

- **Windows 10 / 11**, x64.
- [**.NET 8 Desktop Runtime**](https://dotnet.microsoft.com/download/dotnet/8.0)
  — нужен для запуска готовой сборки.
- Для сборки из исходников — **.NET SDK 8**.

## Скачивание и установка

1. Открой страницу [**Releases**](https://github.com/metovyura2/Dynamic-Head/releases)
   и скачай последний архив `DynamicHead-*-win-x64.zip`.
2. Распакуй архив в любую папку (например, `C:\Tools\Dynamic-Head`).
3. Убедись, что установлен **.NET 8 Desktop Runtime**.
4. Запусти `DynamicHead.exe`. При первом запуске сразу откроется окно настроек.

Установщика нет — программа портативная. Чтобы удалить, выйди из меню трея и
удали папку.

## Использование

1. Запусти `DynamicHead.exe`. При первом запуске откроется **окно настроек**.
2. Выбери **Устройство 1** (например, колонки) и **Устройство 2** (например,
   наушники). Справа можно поменять подписи, которые видны в меню трея.
3. Нажми **Сохранить**. Окно можно закрыть — программа продолжает работать в трее.
4. **Левый клик** по иконке — переключение. **Правый клик** — меню:
   - `Переключить на «…»` — сменить устройство;
   - `Настройки…` — открыть окно настроек (также по двойному клику);
   - `Обновить список устройств` — синхронизировать состояние с системой;
   - `Автозапуск при входе` — включить/выключить;
   - `Выход` — завершить программу.

## Настройки (`config.json`)

Файл создаётся рядом с exe при первом сохранении:

```json
{
  "DeviceAId": "{0.0.0.00000000}.{...}",
  "DeviceAName": "Колонки (Realtek High Definition Audio)",
  "DeviceBId": "{0.0.0.00000000}.{...}",
  "DeviceBName": "Наушники (USB Audio)",
  "LabelA": "Колонки",
  "LabelB": "Наушники",
  "RunAtStartup": false,
  "ActiveSlot": "A"
}
```

## Сборка из исходников

```powershell
# Скриптом (рекомендуется)
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\build.ps1

# Или вручную
dotnet restore .\src\DynamicHead\DynamicHead.csproj
dotnet build   .\src\DynamicHead\DynamicHead.csproj -c Release
dotnet publish .\src\DynamicHead\DynamicHead.csproj -c Release -r win-x64 --self-contained false -o .\dist
```

Результат — `dist\DynamicHead.exe` (framework-dependent). Нужен **.NET 8 Desktop
Runtime**.

> Скрипт ищет `dotnet` сначала в `D:\dotnet`, затем в `PATH`. В CI (GitHub
> Actions) SDK предоставляется шагом `actions/setup-dotnet`.

## Структура проекта

```
src/DynamicHead/Program.cs        точка входа, один экземпляр приложения
src/DynamicHead/AppConfig.cs      настройки и config.json
src/DynamicHead/AudioSwitcher.cs  список устройств и смена устройства по умолчанию
src/DynamicHead/TrayApp.cs        иконка в трее, меню, автозапуск
src/DynamicHead/SettingsForm.cs   окно настроек
src/DynamicHead/assets/           иконки состояний (app-speakers.ico, app-headphones.ico)
tools/build.ps1                   сборка и публикация в dist
tools/gen-icons.ps1               генератор иконок (для сборки не нужен)
```

## Как это работает

- Список устройств и текущее устройство по умолчанию читаются через
  **Core Audio API** Windows с помощью [NAudio](https://github.com/naudio/NAudio).
- Смена устройства вывода выполняется через недокументированный COM-интерфейс
  `IPolicyConfig` (`PolicyConfigClient`) для всех трёх ролей звука.
- Настройки сериализуются через `System.Text.Json` в `config.json` рядом с exe.

## FAQ

**Работает ли с Bluetooth-наушниками?**
Да — подойдёт любое активное устройство вывода, которое видно в настройках звука
Windows.

**Нужны права администратора?**
Нет. Программа пишет только в `HKCU` и в свою папку.

**Есть ли глобальные горячие клавиши?**
Пока нет. Управление — кликами по иконке в трее. Можно оформить запрос функции.

**Где хранятся настройки?**
В `config.json` рядом с `DynamicHead.exe`. Удали файл, чтобы сбросить.

## Участие в разработке

Мы рады вкладу! Сначала прочитай [CONTRIBUTING.md](CONTRIBUTING.md) и
[Кодекс поведения](CODE_OF_CONDUCT.md). По вопросам безопасности — см.
[SECURITY.md](SECURITY.md).

## Лицензия

Проект распространяется под [лицензией MIT](LICENSE).

---

### Ключевые слова

`аудио переключатель` · `смена устройства вывода звука` · `переключение звука` ·
`переключатель звука` · `устройство вывода по умолчанию` · `системный трей` ·
`трей-приложение` · `windows` · `windows 11` · `dotnet` · `c#` · `csharp` ·
`winforms` · `naudio` · `core audio` · `audio switcher` · `sound switcher` ·
`default audio device` · `switch audio output`

