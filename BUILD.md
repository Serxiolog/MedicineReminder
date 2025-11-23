# Инструкция по сборке и запуску приложения

## Требования к системе

### Операционная система:
- Windows 10/11 (версия 1809 или выше)
- macOS 12 Monterey или выше
- Linux (для разработки Blazor, но не для сборки Android)

### Программное обеспечение:
1. **Visual Studio 2022** (версия 17.8 или выше)
   - Workload: ".NET Multi-platform App UI development"
   - Компоненты: Android SDK, Android Emulator

2. **Или Visual Studio Code** с расширениями:
   - C# Dev Kit
   - .NET MAUI Extension

3. **.NET 8.0 SDK** или выше
   - Скачать: https://dotnet.microsoft.com/download/dotnet/8.0

4. **Android SDK**
   - API Level 21 (Android 5.0) - минимальный
   - API Level 34 (Android 14) - рекомендуемый

## Установка среды разработки

### Вариант 1: Visual Studio 2022 (Рекомендуется)

1. **Скачать Visual Studio 2022**
   - Перейти на https://visualstudio.microsoft.com/
   - Скачать Community Edition (бесплатно) или другую версию

2. **Установить необходимые компоненты**
   - Запустить установщик Visual Studio Installer
   - Выбрать workload: ".NET Multi-platform App UI development"
   - В разделе "Individual components" убедиться что выбраны:
     - Android SDK setup (API level 34)
     - Android SDK build-tools
     - Android Emulator

3. **Установить .NET MAUI**
   ```bash
   dotnet workload install maui
   ```

### Вариант 2: Visual Studio Code

1. **Установить .NET SDK**
   ```bash
   # Windows (через winget)
   winget install Microsoft.DotNet.SDK.8

   # macOS (через homebrew)
   brew install dotnet

   # Linux
   # См. инструкции на https://dotnet.microsoft.com/download
   ```

2. **Установить MAUI workload**
   ```bash
   dotnet workload install maui
   ```

3. **Установить расширения VS Code**
   - Открыть VS Code
   - Установить расширения:
     - C# Dev Kit
     - .NET MAUI Extension

4. **Установить Android SDK**
   ```bash
   # Через командную строку .NET
   dotnet tool install -g android-sdk-manager
   android-sdk-manager "platforms;android-34"
   android-sdk-manager "build-tools;34.0.0"
   ```

## Настройка Android устройства

### Физическое устройство:

1. **Включить режим разработчика**
   - Открыть "Настройки" → "О телефоне"
   - Нажать 7 раз на "Номер сборки"
   - Вернуться в настройки, открыть "Для разработчиков"

2. **Включить отладку по USB**
   - В разделе "Для разработчиков"
   - Включить "Отладка по USB"

3. **Подключить устройство к компьютеру**
   - Использовать USB кабель
   - Разрешить отладку на устройстве (появится запрос)

### Android эмулятор:

1. **Создать эмулятор через Visual Studio**
   - Открыть "Tools" → "Android" → "Android Device Manager"
   - Нажать "New Device"
   - Выбрать устройство (например, Pixel 5)
   - Выбрать system image (API 34, x86_64)
   - Создать и запустить эмулятор

2. **Или через командную строку**
   ```bash
   # Список доступных образов
   avdmanager list avd

   # Создать новый эмулятор
   avdmanager create avd -n Pixel5API34 -k "system-images;android-34;google_apis;x86_64" -d pixel_5

   # Запустить эмулятор
   emulator -avd Pixel5API34
   ```

## Сборка проекта

### Через Visual Studio 2022:

1. **Открыть проект**
   - Запустить Visual Studio 2022
   - File → Open → Project/Solution
   - Выбрать `MedicineReminder.csproj`

2. **Восстановить пакеты**
   - Build → Rebuild Solution
   - Или: `dotnet restore` в терминале

3. **Выбрать целевое устройство**
   - В панели инструментов выбрать:
     - Конфигурацию: Debug или Release
     - Целевое устройство: Android Emulator или физическое устройство

4. **Запустить приложение**
   - Нажать F5 или кнопку "Start Debugging"
   - Или: Debug → Start Debugging

### Через командную строку:

1. **Перейти в директорию проекта**
   ```bash
   cd MedicineReminder
   ```

2. **Восстановить пакеты**
   ```bash
   dotnet restore
   ```

3. **Собрать проект**
   ```bash
   # Debug сборка
   dotnet build -f net8.0-android

   # Release сборка
   dotnet build -f net8.0-android -c Release
   ```

4. **Установить на устройство**
   ```bash
   # Убедиться что устройство подключено
   adb devices

   # Установить APK
   dotnet build -f net8.0-android -c Release -t:Install
   ```

### Через Visual Studio Code:

1. **Открыть проект**
   ```bash
   code MedicineReminder
   ```

2. **Открыть терминал** (Ctrl+`)

3. **Восстановить и собрать**
   ```bash
   dotnet restore
   dotnet build -f net8.0-android
   ```

4. **Запустить**
   - Нажать F5
   - Или использовать команду:
   ```bash
   dotnet build -f net8.0-android -t:Run
   ```

## Создание подписанного APK

Для публикации в Google Play или распространения нужно создать подписанный APK:

### 1. Создать keystore:

```bash
keytool -genkey -v -keystore medicine-reminder.keystore -alias medicinereminder -keyalg RSA -keysize 2048 -validity 10000
```

### 2. Настроить подпись в проекте:

Отредактировать `MedicineReminder.csproj`:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <AndroidKeyStore>true</AndroidKeyStore>
    <AndroidSigningKeyStore>medicine-reminder.keystore</AndroidSigningKeyStore>
    <AndroidSigningKeyAlias>medicinereminder</AndroidSigningKeyAlias>
    <AndroidSigningKeyPass>ваш_пароль</AndroidSigningKeyPass>
    <AndroidSigningStorePass>ваш_пароль</AndroidSigningStorePass>
</PropertyGroup>
```

### 3. Собрать подписанный APK:

```bash
dotnet publish -f net8.0-android -c Release
```

Готовый APK будет в: `bin/Release/net8.0-android/publish/`

## Возможные проблемы и решения

### Проблема: "Android SDK not found"

**Решение:**
```bash
# Установить переменную окружения
# Windows
setx ANDROID_HOME "C:\Program Files (x86)\Android\android-sdk"

# macOS/Linux
export ANDROID_HOME=~/Library/Android/sdk
export PATH=$PATH:$ANDROID_HOME/tools:$ANDROID_HOME/platform-tools
```

### Проблема: "No Android device found"

**Решение:**
```bash
# Проверить подключенные устройства
adb devices

# Перезапустить adb сервер
adb kill-server
adb start-server

# Для эмулятора - запустить его заново
```

### Проблема: "Build failed - package restore error"

**Решение:**
```bash
# Очистить кэш NuGet
dotnet nuget locals all --clear

# Удалить bin и obj
rm -rf bin obj

# Восстановить пакеты заново
dotnet restore
```

### Проблема: "Deployment failed"

**Решение:**
```bash
# Удалить приложение с устройства
adb uninstall com.medicinereminder.app

# Переустановить
dotnet build -f net8.0-android -t:Install
```

### Проблема: Уведомления не работают на Android 13+

**Решение:**
- Убедиться что разрешение POST_NOTIFICATIONS запрошено
- Проверить настройки уведомлений в системе
- Предоставить разрешение вручную в настройках приложения

### Проблема: Slow emulator performance

**Решение:**
- Включить Hardware Acceleration (HAXM для Intel, WHPX для Windows)
- Выбрать x86_64 образ вместо ARM
- Увеличить RAM эмулятора (рекомендуется 4GB+)

## Отладка

### Просмотр логов:

```bash
# Все логи приложения
adb logcat | grep MedicineReminder

# Только ошибки
adb logcat *:E

# Очистить логи
adb logcat -c
```

### Отладка в Visual Studio:

1. Установить breakpoints в коде
2. Запустить в режиме Debug (F5)
3. Использовать Debug панель для просмотра переменных

### Hot Reload:

.NET MAUI поддерживает Hot Reload:
- Сохраните изменения в .razor или .cs файле
- Изменения применятся автоматически без перезапуска

## Производительность

### Оптимизация сборки:

```xml
<!-- Добавить в .csproj для Release -->
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <AndroidLinkMode>SdkOnly</AndroidLinkMode>
    <AndroidEnableProfiledAot>true</AndroidEnableProfiledAot>
    <RunAOTCompilation>true</RunAOTCompilation>
</PropertyGroup>
```

### Измерение размера APK:

```bash
# Размер подписанного APK
ls -lh bin/Release/net8.0-android/publish/*.apk

# Анализ содержимого APK
unzip -l your-app-Signed.apk
```

## Полезные команды

```bash
# Проверить версию .NET
dotnet --version

# Проверить установленные workloads
dotnet workload list

# Список целевых платформ
dotnet build --help | grep TargetFramework

# Очистить проект
dotnet clean

# Проверить подключенные устройства
adb devices

# Скриншот с устройства
adb shell screencap -p /sdcard/screen.png
adb pull /sdcard/screen.png

# Видео с экрана
adb shell screenrecord /sdcard/demo.mp4
```

## Документация

- [.NET MAUI Documentation](https://docs.microsoft.com/dotnet/maui/)
- [Blazor Hybrid Documentation](https://docs.microsoft.com/aspnet/core/blazor/hybrid)
- [Android Developer Guide](https://developer.android.com/)
- [Visual Studio MAUI Guide](https://docs.microsoft.com/visualstudio/mac/maui)

## Поддержка

При возникновении проблем:
1. Проверьте логи через `adb logcat`
2. Убедитесь что все SDK установлены корректно
3. Попробуйте пересобрать проект с нуля
4. Проверьте версии компонентов
