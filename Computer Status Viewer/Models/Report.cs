using System;

namespace Computer_Status_Viewer.Models
{
    /// <summary>
    /// Модель отчёта
    /// </summary>
    public class Report
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int ReportTypeId { get; set; }
        public bool IsAutomatic { get; set; }
        public string FilePath { get; set; }
        public string Status { get; set; } // "Создан", "В процессе", "Завершён", "Ошибка"
        
        // Навигационные свойства
        public ReportType ReportType { get; set; }
    }
}
