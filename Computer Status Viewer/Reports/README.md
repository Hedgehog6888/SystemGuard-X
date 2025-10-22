# Система отчётов SystemGuard-X

## Обзор

Система отчётов позволяет создавать, хранить и управлять отчётами о состоянии системы. Поддерживаются как автоматические отчёты, создаваемые программой, так и пользовательские отчёты.

## Структура базы данных

### Таблица ReportTypes
Хранит типы отчётов:
- `Id` - Уникальный идентификатор
- `Name` - Название типа отчёта
- `Description` - Описание типа
- `Category` - Категория (Система, Производительность, Безопасность, Пользовательский)
- `IsAutomatic` - Автоматический ли тип отчёта
- `Template` - Шаблон для автоматических отчётов

### Таблица Reports
Хранит основные данные отчётов:
- `Id` - Уникальный идентификатор
- `Title` - Заголовок отчёта
- `Description` - Описание отчёта
- `CreatedDate` - Дата создания
- `ModifiedDate` - Дата изменения
- `ReportTypeId` - Ссылка на тип отчёта
- `IsAutomatic` - Автоматический ли отчёт
- `FilePath` - Путь к файлу отчёта
- `Status` - Статус отчёта (Создан, В процессе, Завершён, Ошибка)

### Таблица ReportData
Хранит данные отчётов:
- `Id` - Уникальный идентификатор
- `ReportId` - Ссылка на отчёт
- `Key` - Ключ параметра
- `Value` - Значение параметра
- `DataType` - Тип данных (string, int, double, datetime, json)
- `Category` - Категория данных
- `Timestamp` - Время создания записи

## Использование

### Создание автоматического отчёта о системе
```csharp
var reportManager = new ReportManager();
int reportId = reportManager.CreateSystemReport();
```

### Создание отчёта о производительности
```csharp
int reportId = reportManager.CreatePerformanceReport();
```

### Создание пользовательского отчёта
```csharp
var customData = new Dictionary<string, string>
{
    { "Параметр1", "Значение1" },
    { "Параметр2", "Значение2" }
};

int reportId = reportManager.CreateCustomReport(
    "Заголовок отчёта", 
    "Описание отчёта", 
    customData
);
```

### Получение всех отчётов
```csharp
var allReports = reportManager.GetAllReports();
```

### Получение отчёта по ID
```csharp
var report = reportManager.GetReportById(reportId);
```

### Удаление отчёта
```csharp
reportManager.DeleteReport(reportId);
```

## Автоматические отчёты

### Системный отчёт
Собирает следующую информацию:
- Информация о процессоре
- Информация об операционной системе
- Информация о памяти
- Информация о пользователе
- Информация о дисках
- Время работы системы

### Отчёт о производительности
Собирает следующую информацию:
- Загрузка процессора
- Использование памяти
- Количество процессов
- Время отклика системы

## Файлы отчётов

Отчёты сохраняются в папке `Reports` в формате текстовых файлов с расширением `.txt`. Каждый файл содержит:
- Заголовок и описание отчёта
- Дату создания
- Тип отчёта
- Статус
- Данные, сгруппированные по категориям

## Примеры использования

### В MainWindow.xaml.cs
```csharp
// Создание системного отчёта
public void CreateSystemReport()
{
    try
    {
        int reportId = _reportManager.Value.CreateSystemReport();
        MessageBox.Show($"Отчёт создан! ID: {reportId}");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Ошибка: {ex.Message}");
    }
}
```

### Демонстрация всех возможностей
```csharp
public void DemonstrateReports()
{
    // Создаём все типы отчётов
    CreateSystemReport();
    CreatePerformanceReport();
    CreateCustomReport();
    
    // Получаем статистику
    var allReports = _reportManager.Value.GetAllReports();
    MessageBox.Show($"Всего отчётов: {allReports.Count}");
}
```

## Настройка

База данных SQLite создаётся автоматически при первом запуске в папке приложения с именем `Reports.db`. Все таблицы создаются автоматически с базовыми типами отчётов.

## Расширение функциональности

Для добавления новых типов отчётов:
1. Добавьте новый тип в таблицу `ReportTypes`
2. Создайте метод сбора данных в `ReportManager`
3. Добавьте метод создания отчёта в `ReportManager`
4. Интегрируйте в пользовательский интерфейс
