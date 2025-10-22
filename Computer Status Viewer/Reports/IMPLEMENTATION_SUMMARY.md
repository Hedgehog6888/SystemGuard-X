# Реализация системы отчётов в SystemGuard-X

## Что было сделано

### 1. Добавлен пакет SQLite
- Добавлен пакет `System.Data.SQLite` версии 1.0.118
- Обновлены файлы `packages.config` и `Computer Status Viewer.csproj`

### 2. Созданы модели данных
- **Report.cs** - основная модель отчёта
- **ReportType.cs** - типы отчётов
- **ReportData.cs** - данные отчётов

### 3. Создан DatabaseManager
- Полнофункциональный менеджер для работы с SQLite
- Автоматическое создание базы данных и таблиц
- CRUD операции для всех сущностей
- Вставка базовых типов отчётов

### 4. Создан ReportManager
- Высокоуровневый API для работы с отчётами
- Автоматическое создание системных отчётов
- Создание отчётов о производительности
- Создание пользовательских отчётов
- Генерация файлов отчётов в текстовом формате

### 5. Интеграция в MainWindow
- Добавлен ReportManager в MainWindow
- Созданы методы для демонстрации работы
- Добавлены примеры использования

## Структура базы данных

### Таблица ReportTypes
```sql
CREATE TABLE ReportTypes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    Category TEXT NOT NULL,
    IsAutomatic INTEGER NOT NULL DEFAULT 0,
    Template TEXT
)
```

### Таблица Reports
```sql
CREATE TABLE Reports (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT,
    CreatedDate DATETIME NOT NULL,
    ModifiedDate DATETIME,
    ReportTypeId INTEGER NOT NULL,
    IsAutomatic INTEGER NOT NULL DEFAULT 0,
    FilePath TEXT,
    Status TEXT NOT NULL DEFAULT 'Создан',
    FOREIGN KEY (ReportTypeId) REFERENCES ReportTypes(Id)
)
```

### Таблица ReportData
```sql
CREATE TABLE ReportData (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ReportId INTEGER NOT NULL,
    Key TEXT NOT NULL,
    Value TEXT,
    DataType TEXT NOT NULL DEFAULT 'string',
    Category TEXT,
    Timestamp DATETIME NOT NULL,
    FOREIGN KEY (ReportId) REFERENCES Reports(Id)
)
```

## Базовые типы отчётов

1. **Системная информация** - полная информация о системе
2. **Производительность** - отчёт о производительности системы
3. **Безопасность** - проверка безопасности системы
4. **Пользовательский отчёт** - пользовательские отчёты

## Возможности системы

### Автоматические отчёты
- Создаются программой автоматически
- Собирают системную информацию
- Генерируют файлы отчётов
- Сохраняются в базе данных

### Пользовательские отчёты
- Создаются пользователем вручную
- Могут содержать любые данные
- Гибкая структура данных
- Полная интеграция с системой

### Управление отчётами
- Просмотр всех отчётов
- Получение отчёта по ID
- Удаление отчётов
- Обновление статусов

## Примеры использования

### Создание системного отчёта
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
    { "Параметр", "Значение" }
};

int reportId = reportManager.CreateCustomReport(
    "Заголовок", 
    "Описание", 
    customData
);
```

### Получение всех отчётов
```csharp
var allReports = reportManager.GetAllReports();
```

## Файловая структура

```
Computer Status Viewer/
├── Models/
│   ├── Report.cs
│   ├── ReportType.cs
│   └── ReportData.cs
├── Database/
│   └── DatabaseManager.cs
├── Reports/
│   ├── ReportManager.cs
│   ├── README.md
│   └── IMPLEMENTATION_SUMMARY.md
└── Reports/ (папка для файлов отчётов)
    └── *.txt файлы отчётов
```

## Интеграция в проект

1. **DatabaseManager** - низкоуровневая работа с SQLite
2. **ReportManager** - высокоуровневый API для отчётов
3. **MainWindow** - интеграция в пользовательский интерфейс
4. **Models** - модели данных для типизации

## Расширение функциональности

Для добавления новых типов отчётов:
1. Добавьте новый тип в `ReportTypes`
2. Создайте метод сбора данных в `ReportManager`
3. Добавьте метод создания отчёта
4. Интегрируйте в UI

## Тестирование

Для тестирования системы отчётов:
1. Раскомментируйте строку `// DemonstrateReports();` в `LoadContentAsync`
2. Запустите приложение
3. Система создаст все типы отчётов автоматически
4. Проверьте папку `Reports` на наличие файлов отчётов
5. Проверьте базу данных `Reports.db` в папке приложения

## Заключение

Система отчётов полностью интегрирована в проект SystemGuard-X и готова к использованию. Она поддерживает как автоматические, так и пользовательские отчёты, с полной функциональностью CRUD операций и генерацией файлов отчётов.
