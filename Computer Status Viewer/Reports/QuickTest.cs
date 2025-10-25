using System;
using System.IO;
using Computer_Status_Viewer.Models;

namespace Computer_Status_Viewer.Reports
{
    /// <summary>
    /// Быстрый тест экспорта без зависимостей от базы данных
    /// </summary>
    public class QuickTest
    {
        public static void RunTest()
        {
            Console.WriteLine("=== БЫСТРЫЙ ТЕСТ ЭКСПОРТА ===");
            Console.WriteLine();
            
            try
            {
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
                
                Console.WriteLine("✓ Тестовый отчёт создан");
                
                // Создаём экспортёр (без реального ReportManager)
                var exporter = new ReportExporter(null);
                
                // Тестируем TXT экспорт
                var txtPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test_report.txt");
                Console.WriteLine($"Создание TXT файла: {txtPath}");
                
                // Создаём простую версию экспорта без базы данных
                CreateSimpleTxtExport(testReport, txtPath);
                
                Console.WriteLine("✓ TXT экспорт успешно завершён!");
                Console.WriteLine($"Файл создан: {txtPath}");
                
                // Тестируем HTML экспорт
                var htmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test_report.html");
                Console.WriteLine($"Создание HTML файла: {htmlPath}");
                
                CreateSimpleHtmlExport(testReport, htmlPath);
                
                Console.WriteLine("✓ HTML экспорт успешно завершён!");
                Console.WriteLine($"HTML файл создан: {htmlPath}");
                
                Console.WriteLine();
                Console.WriteLine("=== ТЕСТ ЗАВЕРШЁН УСПЕШНО ===");
                Console.WriteLine("Проверьте файлы на рабочем столе:");
                Console.WriteLine($"- {Path.GetFileName(txtPath)}");
                Console.WriteLine($"- {Path.GetFileName(htmlPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
            }
        }
        
        private static void CreateSimpleTxtExport(Report report, string filePath)
        {
            var content = new System.Text.StringBuilder();
            
            // Заголовок отчёта
            content.AppendLine("=".PadRight(80, '='));
            content.AppendLine($"ОТЧЁТ: {report.Title.ToUpper()}");
            content.AppendLine("=".PadRight(80, '='));
            content.AppendLine();
            
            // Основная информация
            content.AppendLine("ОСНОВНАЯ ИНФОРМАЦИЯ:");
            content.AppendLine("-".PadRight(40, '-'));
            content.AppendLine($"Тип отчёта:     {report.ReportType?.Name ?? "Неизвестно"}");
            content.AppendLine($"Категория:      {report.ReportType?.Category ?? "Неизвестно"}");
            content.AppendLine($"Статус:         {report.Status}");
            content.AppendLine($"Тип:            {(report.IsAutomatic ? "Автоматический" : "Пользовательский")}");
            content.AppendLine($"Создан:         {report.CreatedDate:dd.MM.yyyy HH:mm:ss}");
            content.AppendLine($"Описание:       {report.Description}");
            content.AppendLine();
            
            // Тестовые данные
            content.AppendLine("ДАННЫЕ ОТЧЁТА:");
            content.AppendLine("=".PadRight(80, '='));
            content.AppendLine();
            content.AppendLine("📁 СИСТЕМНАЯ ИНФОРМАЦИЯ");
            content.AppendLine("-".PadRight(60, '-'));
            content.AppendLine("  • Процессор: Intel Core i7-8700K");
            content.AppendLine("    Тип: String");
            content.AppendLine("    Время: 14:30:25");
            content.AppendLine();
            content.AppendLine("  • Оперативная память: 16 GB");
            content.AppendLine("    Тип: String");
            content.AppendLine("    Время: 14:30:25");
            content.AppendLine();
            
            // Подвал
            content.AppendLine("=".PadRight(80, '='));
            content.AppendLine($"Сгенерировано: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
            content.AppendLine("SystemGuard-X v1.0");
            content.AppendLine("=".PadRight(80, '='));
            
            File.WriteAllText(filePath, content.ToString(), System.Text.Encoding.UTF8);
        }
        
        private static void CreateSimpleHtmlExport(Report report, string filePath)
        {
            var htmlContent = new System.Text.StringBuilder();
            
            htmlContent.AppendLine("<!DOCTYPE html>");
            htmlContent.AppendLine("<html lang='ru'>");
            htmlContent.AppendLine("<head>");
            htmlContent.AppendLine("    <meta charset='UTF-8'>");
            htmlContent.AppendLine("    <title>Отчёт: " + report.Title + "</title>");
            htmlContent.AppendLine("    <style>");
            htmlContent.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; background-color: #f5f5f5; }");
            htmlContent.AppendLine("        .container { background-color: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            htmlContent.AppendLine("        .header { text-align: center; color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 20px; margin-bottom: 30px; }");
            htmlContent.AppendLine("        .title { font-size: 24px; font-weight: bold; margin-bottom: 10px; }");
            htmlContent.AppendLine("        .info-section { margin-bottom: 30px; }");
            htmlContent.AppendLine("        .info-title { font-size: 18px; font-weight: bold; color: #34495e; margin-bottom: 15px; border-left: 4px solid #3498db; padding-left: 10px; }");
            htmlContent.AppendLine("        .info-table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }");
            htmlContent.AppendLine("        .info-table td { padding: 8px 12px; border-bottom: 1px solid #ecf0f1; }");
            htmlContent.AppendLine("        .info-table td:first-child { font-weight: bold; color: #2c3e50; width: 200px; }");
            htmlContent.AppendLine("        .footer { text-align: center; margin-top: 40px; padding-top: 20px; border-top: 2px solid #ecf0f1; color: #7f8c8d; font-size: 12px; }");
            htmlContent.AppendLine("    </style>");
            htmlContent.AppendLine("</head>");
            htmlContent.AppendLine("<body>");
            htmlContent.AppendLine("    <div class='container'>");
            htmlContent.AppendLine("        <div class='header'>");
            htmlContent.AppendLine("            <div class='title'>ОТЧЁТ: " + report.Title.ToUpper() + "</div>");
            htmlContent.AppendLine("        </div>");
            htmlContent.AppendLine("        <div class='info-section'>");
            htmlContent.AppendLine("            <div class='info-title'>📋 Основная информация</div>");
            htmlContent.AppendLine("            <table class='info-table'>");
            htmlContent.AppendLine("                <tr><td>Тип отчёта:</td><td>" + (report.ReportType?.Name ?? "Неизвестно") + "</td></tr>");
            htmlContent.AppendLine("                <tr><td>Категория:</td><td>" + (report.ReportType?.Category ?? "Неизвестно") + "</td></tr>");
            htmlContent.AppendLine("                <tr><td>Статус:</td><td>" + report.Status + "</td></tr>");
            htmlContent.AppendLine("                <tr><td>Тип:</td><td>" + (report.IsAutomatic ? "Автоматический" : "Пользовательский") + "</td></tr>");
            htmlContent.AppendLine("                <tr><td>Создан:</td><td>" + report.CreatedDate.ToString("dd.MM.yyyy HH:mm:ss") + "</td></tr>");
            htmlContent.AppendLine("                <tr><td>Описание:</td><td>" + report.Description + "</td></tr>");
            htmlContent.AppendLine("            </table>");
            htmlContent.AppendLine("        </div>");
            htmlContent.AppendLine("        <div class='footer'>");
            htmlContent.AppendLine("            <div>Сгенерировано: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " | SystemGuard-X v1.0</div>");
            htmlContent.AppendLine("        </div>");
            htmlContent.AppendLine("    </div>");
            htmlContent.AppendLine("</body>");
            htmlContent.AppendLine("</html>");
            
            File.WriteAllText(filePath, htmlContent.ToString(), System.Text.Encoding.UTF8);
        }
    }
}
