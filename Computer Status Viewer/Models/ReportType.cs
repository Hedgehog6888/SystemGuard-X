namespace Computer_Status_Viewer.Models
{
    /// <summary>
    /// Тип отчёта
    /// </summary>
    public class ReportType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // "Система", "Производительность", "Безопасность", "Пользовательский"
        public bool IsAutomatic { get; set; }
        public string Template { get; set; } // Шаблон для автоматических отчётов
    }
}
