# 🖥️ Computer Status Viewer (SystemGuard-X)

<div align="center">

![Logo](Computer%20Status%20Viewer/Ico/Logo.png)

**Мощный инструмент для мониторинга состояния компьютера в реальном времени**

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework)
[![WPF](https://img.shields.io/badge/WPF-Windows%20Presentation%20Foundation-green.svg)](https://docs.microsoft.com/en-us/dotnet/framework/wpf/)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![GitHub](https://img.shields.io/badge/GitHub-Hedgehog6888-black.svg)](https://github.com/Hedgehog6888)

</div>

## 🚀 Описание

**Computer Status Viewer** — это профессиональное WPF-приложение для комплексного мониторинга состояния вашего компьютера. Приложение предоставляет детальную информацию о всех компонентах системы, включая процессор, память, материнскую плату, BIOS, операционную систему и многое другое.

## ✨ Основные возможности

### 🔧 Системная информация
- **Процессор**: Детальная информация о CPU, частотах, температуре, нагрузке
- **Память**: Мониторинг RAM, использование памяти, статистика
- **Материнская плата**: Информация о чипсете, BIOS, разъёмах
- **Операционная система**: Версия Windows, обновления, региональные настройки
- **BIOS**: Версия, дата, производитель, настройки

### 📊 Мониторинг производительности
- **Время работы**: Статистика работы системы, время включения/выключения
- **Эффективность**: Графики производительности в реальном времени
- **Сетевые подключения**: Мониторинг сетевой активности
- **Дисковые операции**: Статистика чтения/записи дисков

### 🎨 Современный интерфейс
- **Адаптивный дизайн**: Современный Material Design интерфейс
- **Тёмная/светлая тема**: Переключение между темами
- **Виджеты**: Компактные виджеты для быстрого доступа к информации
- **Графики**: Интерактивные графики производительности

### 🔍 Детальная диагностика
- **Сводка системы**: Общая информация о конфигурации
- **Региональные настройки**: Язык, валюта, формат даты
- **Устройства**: Список подключённых устройств
- **Сенсоры**: Температура, напряжение, скорость вентиляторов

## 🛠️ Технологии

- **.NET Framework 4.7.2** - Основная платформа
- **WPF (Windows Presentation Foundation)** - Пользовательский интерфейс
- **Hardware.Info** - Получение информации о железе
- **LibreHardwareMonitorLib** - Мониторинг датчиков
- **LiveCharts.Wpf** - Интерактивные графики
- **DynamicDataDisplayWpf** - Дополнительные графики

## 📦 Установка

### Требования
- Windows 10/11 (x64)
- .NET Framework 4.7.2 или выше
- Права администратора (для доступа к некоторым системным данным)

### Сборка из исходного кода

1. **Клонируйте репозиторий:**
   ```bash
   git clone https://github.com/Hedgehog6888/SystemGuard-X.git
   cd SystemGuard-X
   ```

2. **Откройте решение в Visual Studio:**
   ```bash
   start "Computer Status Viewer.sln"
   ```

3. **Восстановите NuGet пакеты:**
   ```bash
   nuget restore
   ```

4. **Соберите проект:**
   ```bash
   msbuild "Computer Status Viewer.sln" /p:Configuration=Release
   ```

5. **Запустите приложение:**
   ```bash
   "Computer Status Viewer\bin\Release\Computer Status Viewer.exe"
   ```

## 🎯 Использование

### Основное окно
- **Навигация**: Используйте боковое меню для перехода между разделами
- **Обновление**: Данные обновляются автоматически каждые несколько секунд
- **Экспорт**: Сохраняйте отчёты в различных форматах

### Виджеты
- **Системный монитор**: Компактный виджет с основной информацией
- **Производительность**: Графики CPU, памяти, диска в реальном времени
- **Температуры**: Мониторинг температур компонентов

### Настройки
- **Темы**: Переключение между светлой и тёмной темой
- **Обновления**: Настройка частоты обновления данных
- **Уведомления**: Настройка предупреждений о критических температурах

## 📁 Структура проекта

```
Computer Status Viewer/
├── Categories/           # Менеджеры категорий информации
│   ├── BIOSManager.cs
│   ├── CPUManager.cs
│   ├── MemoryManager.cs
│   ├── MotherboardManager.cs
│   ├── OSManager.cs
│   └── ...
├── Efficiency/          # Модули производительности
│   ├── ChartHelper.cs
│   ├── PerformanceManager.cs
│   └── ...
├── Widget/             # Виджеты и мониторинг
│   ├── SystemMonitor.cs
│   └── WidgetManager.cs
├── Main/               # Основная навигация
│   ├── NavigationManager.cs
│   └── SubcategoryManager.cs
├── Ico/                # Иконки и ресурсы
└── Properties/         # Настройки приложения
```

## 🔧 Конфигурация

### Настройки приложения
Файл `App.config` содержит основные настройки:
- Интервалы обновления данных
- Пути к ресурсам
- Настройки логирования

### Темы
Поддерживаются две темы:
- **Light Theme** - Светлая тема
- **Dark Theme** - Тёмная тема

## 🤝 Вклад в проект

Мы приветствуем вклад в развитие проекта! 

### Как внести вклад:
1. **Fork** репозитория
2. Создайте **feature branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit** изменения (`git commit -m 'Add some AmazingFeature'`)
4. **Push** в branch (`git push origin feature/AmazingFeature`)
5. Откройте **Pull Request**

### Требования к коду:
- Следуйте стилю кодирования C#
- Добавляйте комментарии к новому коду
- Тестируйте изменения перед отправкой PR

## 📋 Roadmap

- [ ] **Версия 2.0**
  - [ ] Поддержка Linux (через .NET Core)
  - [ ] Веб-интерфейс для удалённого мониторинга
  - [ ] Мобильное приложение
  - [ ] API для интеграции с другими системами

- [ ] **Улучшения**
  - [ ] Больше графиков и диаграмм
  - [ ] Экспорт в PDF/Excel
  - [ ] Плагинная архитектура
  - [ ] Уведомления в системном трее

## 🐛 Известные проблемы

- Некоторые функции требуют права администратора
- На старых системах может быть ограниченный доступ к датчикам
- Высокое потребление CPU при активном мониторинге

## 📄 Лицензия

Этот проект распространяется под лицензией MIT. См. файл [LICENSE](LICENSE) для подробностей.

## 👨‍💻 Автор

**Hedgehog6888**
- GitHub: [@Hedgehog6888](https://github.com/Hedgehog6888)

## 🙏 Благодарности

- [Hardware.Info](https://github.com/jinjinov/Hardware.Info) - за отличную библиотеку для работы с железом
- [LibreHardwareMonitor](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor) - за мониторинг датчиков
- [LiveCharts](https://github.com/Live-Charts/Live-Charts) - за красивые графики

---

<div align="center">

**⭐ Если проект вам понравился, поставьте звезду! ⭐**

[![GitHub stars](https://img.shields.io/github/stars/Hedgehog6888/SystemGuard-X.svg?style=social&label=Star)](https://github.com/Hedgehog6888/SystemGuard-X)
[![GitHub forks](https://img.shields.io/github/forks/Hedgehog6888/SystemGuard-X.svg?style=social&label=Fork)](https://github.com/Hedgehog6888/SystemGuard-X/fork)

</div>
