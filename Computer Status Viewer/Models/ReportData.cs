using System;

namespace Computer_Status_Viewer.Models
{
    /// <summary>
    /// Данные отчёта (метрики, параметры)
    /// </summary>
    public class ReportData
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string Key { get; set; } // Название параметра/метрики
        public string Value { get; set; } // Значение
        public string DataType { get; set; } // "string", "int", "double", "datetime", "json"
        public string Category { get; set; } // Категория данных
        public DateTime Timestamp { get; set; }
        
        // Навигационные свойства
        public Report Report { get; set; }
    }
}
