<#
.SYNOPSIS
    Генерация иконок приложения для Dynamic-Head.

.DESCRIPTION
    Создаёт два значка: колонки (синий) и наушники (зелёный).
    Пишет PNG внутри ICO-контейнера (формат поддерживается Windows Vista+).
#>

param(
    [string]$OutDir = (Join-Path $PSScriptRoot '..\src\DynamicHead\assets')
)

Add-Type -AssemblyName System.Drawing

$ErrorActionPreference = 'Stop'

# Размер значка
$size = 32

function New-IconBitmap {
    param([string]$Kind)

    $bmp = New-Object System.Drawing.Bitmap($size, $size)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)

    if ($Kind -eq 'Speakers') {
        # Синий цвет для колонок
        $brush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 40, 110, 200))
        $pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 20, 70, 150), 2)

        # Корпус колонки
        $g.FillRectangle($brush, 5, 7, 9, 18)
        # Динамик
        $g.FillEllipse((New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)), 8, 10, 3, 3)
        # Звуковые волны
        $g.DrawArc($pen, 15, 9, 8, 14, -60, 120)
        $g.DrawArc($pen, 17, 6, 12, 20, -60, 120)

        $brush.Dispose(); $pen.Dispose()
    }
    else {
        # Зелёный цвет для наушников
        $pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(255, 30, 150, 80), 3)
        $fill = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(255, 30, 150, 80))

        # Дужка
        $g.DrawArc($pen, 6, 4, 20, 20, 180, 180)
        # Амбушюры
        $g.FillRectangle($fill, 4, 15, 6, 11)
        $g.FillRectangle($fill, 22, 15, 6, 11)

        $pen.Dispose(); $fill.Dispose()
    }

    $g.Dispose()
    return $bmp
}

function Save-IcoFile {
    param(
        [System.Drawing.Bitmap]$Bitmap,
        [string]$Path
    )

    # Сохраняем картинку в PNG-поток
    $pngStream = New-Object System.IO.MemoryStream
    $Bitmap.Save($pngStream, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngBytes = $pngStream.ToArray()

    # Собираем ICO-контейнер с одним изображением в формате PNG
    $fs = [System.IO.File]::Create($Path)
    $bw = New-Object System.IO.BinaryWriter($fs)

    # ICONDIR
    $bw.Write([UInt16]0)                # reserved
    $bw.Write([UInt16]1)                # type: icon
    $bw.Write([UInt16]1)                # количество изображений

    # ICONDIRENTRY
    $bw.Write([Byte]$Bitmap.Width)      # ширина
    $bw.Write([Byte]$Bitmap.Height)     # высота
    $bw.Write([Byte]0)                  # цветов
    $bw.Write([Byte]0)                  # reserved
    $bw.Write([UInt16]1)                # цветовые плоскости
    $bw.Write([UInt16]32)               # бит на пиксель
    $bw.Write([UInt32]$pngBytes.Length) # размер данных
    $bw.Write([UInt32]22)               # смещение данных (6 + 16)

    $bw.Write($pngBytes)

    $bw.Flush()
    $bw.Close()
    $pngStream.Dispose()
}

if (-not (Test-Path $OutDir)) {
    New-Item -ItemType Directory -Path $OutDir -Force | Out-Null
}

$speakers = New-IconBitmap -Kind 'Speakers'
Save-IcoFile -Bitmap $speakers -Path (Join-Path $OutDir 'app-speakers.ico')
$speakers.Dispose()

$headphones = New-IconBitmap -Kind 'Headphones'
Save-IcoFile -Bitmap $headphones -Path (Join-Path $OutDir 'app-headphones.ico')
$headphones.Dispose()

Write-Host "Иконки созданы в $OutDir"
