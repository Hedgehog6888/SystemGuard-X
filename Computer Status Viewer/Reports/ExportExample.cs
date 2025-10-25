using System;
using System.IO;
using Computer_Status_Viewer.Models;

namespace Computer_Status_Viewer.Reports
{
    /// <summary>
    /// Пример использования ReportExporter
    /// </summary>
    public class ExportExample
    {
        public static void DemonstrateExport()
        {
            try
            {
                // Создаём менеджер отчётов
                var reportManager = new ReportManager();
                
                // Создаём экспортёр
                var exporter = new ReportExporter(reportManager);
                
                // Создаём тестовый отчёт
                var testReport = new Report
                {
                    Id = 1,
                    Title = "Тестовый отчёт системы",
                    Description = "Демонстрационный отчёт для тестирования экспорта",
                    Status = "Завершён",
                    CreatedDate = DateTime.Now,
                    IsAutomatic = true,
                    ReportType = new ReportType
                    {
                        Name = "Системная информация",
                        Category = "Система"
                    }
                };
                
                // Пути для экспорта
                var txtPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test_report.txt");
                var pdfPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test_report.pdf");
                
                Console.WriteLine("Демонстрация экспорта отчётов:");
                Console.WriteLine($"TXT файл: {txtPath}");
                Console.WriteLine($"PDF файл: {pdfPath}");
                
                // Экспорт в TXT
                exporter.ExportToTxt(testReport, txtPath);
                Console.WriteLine("✓ TXT экспорт завершён");
                
                // Экспорт в PDF
                exporter.ExportToPdf(testReport, pdfPath);
                Console.WriteLine("✓ PDF экспорт завершён");
                
                Console.WriteLine("Экспорт успешно завершён!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
