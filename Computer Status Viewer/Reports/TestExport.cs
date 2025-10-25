using System;
using System.IO;
using Computer_Status_Viewer.Models;

namespace Computer_Status_Viewer.Reports
{
    /// <summary>
    /// Простой тест экспорта без зависимостей от базы данных
    /// </summary>
    public class TestExport
    {
        public static void TestTxtExport()
        {
            try
            {
                Console.WriteLine("Тестирование TXT экспорта...");
                
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
                
                // Создаём простой ReportManager для тестирования
                var reportManager = new ReportManager();
                var exporter = new ReportExporter(reportManager);
                
                // Путь для тестового файла
                var testPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test_export.txt");
                
                Console.WriteLine($"Создание TXT файла: {testPath}");
                exporter.ExportToTxt(testReport, testPath);
                
                Console.WriteLine("✓ TXT экспорт успешно завершён!");
                Console.WriteLine($"Файл создан: {testPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка TXT экспорта: {ex.Message}");
            }
        }
        
        public static void TestHtmlExport()
        {
            try
            {
                Console.WriteLine("Тестирование HTML экспорта...");
                
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
                
                // Создаём простой ReportManager для тестирования
                var reportManager = new ReportManager();
                var exporter = new ReportExporter(reportManager);
                
                // Путь для тестового файла
                var testPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test_export.pdf");
                
                Console.WriteLine($"Создание HTML файла: {testPath}");
                exporter.ExportToPdf(testReport, testPath);
                
                Console.WriteLine("✓ HTML экспорт успешно завершён!");
                Console.WriteLine($"HTML файл создан: {testPath.Replace(".pdf", ".html")}");
                Console.WriteLine($"Инструкции созданы: {testPath.Replace(".pdf", "_instructions.txt")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка HTML экспорта: {ex.Message}");
            }
        }
        
        public static void RunAllTests()
        {
            Console.WriteLine("=== ТЕСТИРОВАНИЕ ЭКСПОРТА ОТЧЁТОВ ===");
            Console.WriteLine();
            
            TestTxtExport();
            Console.WriteLine();
            TestHtmlExport();
            
            Console.WriteLine();
            Console.WriteLine("=== ТЕСТИРОВАНИЕ ЗАВЕРШЕНО ===");
        }
    }
}
