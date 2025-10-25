using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Printing;
using Computer_Status_Viewer.Models;

namespace Computer_Status_Viewer.Reports
{
    /// <summary>
    /// Класс для экспорта отчётов в различные форматы
    /// </summary>
    public class ReportExporter
    {
        private readonly ReportManager _reportManager;

        public ReportExporter(ReportManager reportManager)
        {
            _reportManager = reportManager ?? throw new ArgumentNullException(nameof(reportManager));
        }

        /// <summary>
        /// Экспорт отчёта в TXT формат с красивым форматированием
        /// </summary>
        public void ExportToTxt(Report report, string filePath)
        {
            try
            {
                var content = new StringBuilder();
                
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
                
                if (report.ModifiedDate.HasValue)
                {
                    content.AppendLine($"Изменён:        {report.ModifiedDate.Value:dd.MM.yyyy HH:mm:ss}");
                }
                
                if (!string.IsNullOrEmpty(report.Description))
                {
                    content.AppendLine($"Описание:       {report.Description}");
                }
                
                if (!string.IsNullOrEmpty(report.FilePath))
                {
                    content.AppendLine($"Файл:           {Path.GetFileName(report.FilePath)}");
                }
                
                content.AppendLine();

                // Данные отчёта
                var reportData = _reportManager.GetReportData(report.Id);
                if (reportData.Any())
                {
                    content.AppendLine("ДАННЫЕ ОТЧЁТА:");
                    content.AppendLine("=".PadRight(80, '='));
                    content.AppendLine();

                    // Группировка данных по категориям
                    var groupedData = reportData.GroupBy(d => d.Category ?? "Без категории");
                    
                    foreach (var group in groupedData)
                    {
                        content.AppendLine($"📁 {group.Key.ToUpper()}");
                        content.AppendLine("-".PadRight(60, '-'));
                        
                        foreach (var data in group)
                        {
                            content.AppendLine($"  • {data.Key}: {data.Value}");
                            if (!string.IsNullOrEmpty(data.DataType))
                            {
                                content.AppendLine($"    Тип: {data.DataType}");
                            }
                            content.AppendLine($"    Время: {data.Timestamp:HH:mm:ss}");
                            content.AppendLine();
                        }
                    }
                }
                else
                {
                    content.AppendLine("ДАННЫЕ ОТЧЁТА:");
                    content.AppendLine("-".PadRight(40, '-'));
                    content.AppendLine("Данные отчёта отсутствуют");
                    content.AppendLine();
                }

                // Подвал
                content.AppendLine("=".PadRight(80, '='));
                content.AppendLine($"Сгенерировано: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
                content.AppendLine($"SystemGuard-X v1.0");
                content.AppendLine("=".PadRight(80, '='));

                // Сохранение в файл
                File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка экспорта в TXT: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Экспорт отчёта в PDF формат с красивым форматированием
        /// Использует HTML-подобный формат для совместимости
        /// </summary>
        public void ExportToPdf(Report report, string filePath)
        {
            try
            {
                // Создаём HTML-подобный документ, который можно открыть в браузере и сохранить как PDF
                var htmlContent = new StringBuilder();
                
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
                htmlContent.AppendLine("        .data-section { margin-top: 30px; }");
                htmlContent.AppendLine("        .category-header { font-size: 16px; font-weight: bold; color: #2980b9; margin: 20px 0 10px 0; padding: 10px; background-color: #ecf0f1; border-left: 4px solid #3498db; }");
                htmlContent.AppendLine("        .data-table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }");
                htmlContent.AppendLine("        .data-table th { background-color: #34495e; color: white; padding: 12px; text-align: left; }");
                htmlContent.AppendLine("        .data-table td { padding: 10px 12px; border-bottom: 1px solid #bdc3c7; }");
                htmlContent.AppendLine("        .data-table tr:nth-child(even) { background-color: #f8f9fa; }");
                htmlContent.AppendLine("        .footer { text-align: center; margin-top: 40px; padding-top: 20px; border-top: 2px solid #ecf0f1; color: #7f8c8d; font-size: 12px; }");
                htmlContent.AppendLine("        .no-data { text-align: center; color: #95a5a6; font-style: italic; padding: 40px; }");
                htmlContent.AppendLine("    </style>");
                htmlContent.AppendLine("</head>");
                htmlContent.AppendLine("<body>");
                htmlContent.AppendLine("    <div class='container'>");
                
                // Заголовок
                htmlContent.AppendLine("        <div class='header'>");
                htmlContent.AppendLine("            <div class='title'>ОТЧЁТ: " + report.Title.ToUpper() + "</div>");
                htmlContent.AppendLine("        </div>");
                
                // Основная информация
                htmlContent.AppendLine("        <div class='info-section'>");
                htmlContent.AppendLine("            <div class='info-title'>📋 Основная информация</div>");
                htmlContent.AppendLine("            <table class='info-table'>");
                htmlContent.AppendLine("                <tr><td>Тип отчёта:</td><td>" + (report.ReportType?.Name ?? "Неизвестно") + "</td></tr>");
                htmlContent.AppendLine("                <tr><td>Категория:</td><td>" + (report.ReportType?.Category ?? "Неизвестно") + "</td></tr>");
                htmlContent.AppendLine("                <tr><td>Статус:</td><td>" + report.Status + "</td></tr>");
                htmlContent.AppendLine("                <tr><td>Тип:</td><td>" + (report.IsAutomatic ? "Автоматический" : "Пользовательский") + "</td></tr>");
                htmlContent.AppendLine("                <tr><td>Создан:</td><td>" + report.CreatedDate.ToString("dd.MM.yyyy HH:mm:ss") + "</td></tr>");
                
                if (report.ModifiedDate.HasValue)
                {
                    htmlContent.AppendLine("                <tr><td>Изменён:</td><td>" + report.ModifiedDate.Value.ToString("dd.MM.yyyy HH:mm:ss") + "</td></tr>");
                }
                
                if (!string.IsNullOrEmpty(report.Description))
                {
                    htmlContent.AppendLine("                <tr><td>Описание:</td><td>" + report.Description + "</td></tr>");
                }
                
                if (!string.IsNullOrEmpty(report.FilePath))
                {
                    htmlContent.AppendLine("                <tr><td>Файл:</td><td>" + Path.GetFileName(report.FilePath) + "</td></tr>");
                }
                
                htmlContent.AppendLine("            </table>");
                htmlContent.AppendLine("        </div>");
                
                // Данные отчёта
                var reportData = _reportManager.GetReportData(report.Id);
                if (reportData.Any())
                {
                    htmlContent.AppendLine("        <div class='data-section'>");
                    htmlContent.AppendLine("            <div class='info-title'>📊 Данные отчёта</div>");
                    
                    var groupedData = reportData.GroupBy(d => d.Category ?? "Без категории");
                    
                    foreach (var group in groupedData)
                    {
                        htmlContent.AppendLine("            <div class='category-header'>📁 " + group.Key.ToUpper() + "</div>");
                        htmlContent.AppendLine("            <table class='data-table'>");
                        htmlContent.AppendLine("                <thead>");
                        htmlContent.AppendLine("                    <tr>");
                        htmlContent.AppendLine("                        <th>Параметр</th>");
                        htmlContent.AppendLine("                        <th>Значение</th>");
                        htmlContent.AppendLine("                        <th>Время</th>");
                        htmlContent.AppendLine("                    </tr>");
                        htmlContent.AppendLine("                </thead>");
                        htmlContent.AppendLine("                <tbody>");
                        
                        foreach (var data in group)
                        {
                            htmlContent.AppendLine("                    <tr>");
                            htmlContent.AppendLine("                        <td>" + data.Key + "</td>");
                            htmlContent.AppendLine("                        <td>" + data.Value + "</td>");
                            htmlContent.AppendLine("                        <td>" + data.Timestamp.ToString("HH:mm:ss") + "</td>");
                            htmlContent.AppendLine("                    </tr>");
                        }
                        
                        htmlContent.AppendLine("                </tbody>");
                        htmlContent.AppendLine("            </table>");
                    }
                    
                    htmlContent.AppendLine("        </div>");
                }
                else
                {
                    htmlContent.AppendLine("        <div class='no-data'>");
                    htmlContent.AppendLine("            <div>Данные отчёта отсутствуют</div>");
                    htmlContent.AppendLine("        </div>");
                }
                
                // Подвал
                htmlContent.AppendLine("        <div class='footer'>");
                htmlContent.AppendLine("            <div>Сгенерировано: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss") + " | SystemGuard-X v1.0</div>");
                htmlContent.AppendLine("        </div>");
                
                htmlContent.AppendLine("    </div>");
                htmlContent.AppendLine("</body>");
                htmlContent.AppendLine("</html>");
                
                // Сохраняем как HTML файл (можно открыть в браузере и сохранить как PDF)
                var htmlFilePath = filePath.Replace(".pdf", ".html");
                File.WriteAllText(htmlFilePath, htmlContent.ToString(), Encoding.UTF8);
                
                // Создаём простой текстовый файл с инструкциями
                var instructionsPath = filePath.Replace(".pdf", "_instructions.txt");
                var instructions = new StringBuilder();
                instructions.AppendLine("ИНСТРУКЦИИ ПО СОЗДАНИЮ PDF:");
                instructions.AppendLine("=".PadRight(50, '='));
                instructions.AppendLine();
                instructions.AppendLine("1. Откройте файл " + Path.GetFileName(htmlFilePath) + " в браузере");
                instructions.AppendLine("2. Нажмите Ctrl+P (Печать)");
                instructions.AppendLine("3. Выберите 'Сохранить как PDF' в качестве принтера");
                instructions.AppendLine("4. Нажмите 'Сохранить'");
                instructions.AppendLine();
                instructions.AppendLine("Альтернативно:");
                instructions.AppendLine("- Используйте онлайн конвертеры HTML в PDF");
                instructions.AppendLine("- Установите расширение браузера для сохранения как PDF");
                
                File.WriteAllText(instructionsPath, instructions.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка экспорта в PDF: {ex.Message}", ex);
            }
        }


        /// <summary>
        /// Экспорт отчёта в указанный формат
        /// </summary>
        public void ExportReport(Report report, string filePath, string format)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Путь к файлу не может быть пустым", nameof(filePath));

            switch (format.ToLower())
            {
                case "txt":
                    ExportToTxt(report, filePath);
                    break;
                case "pdf":
                    ExportToPdf(report, filePath);
                    break;
                default:
                    throw new ArgumentException($"Неподдерживаемый формат: {format}");
            }
        }
    }
}
