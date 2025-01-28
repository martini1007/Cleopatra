using Cleopatra.Data;
using Cleopatra.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;

namespace Cleopatra.Services;
 
public class PdfReportService 
{
        private readonly AppDbContext _context;

        public PdfReportService(AppDbContext context)
        {
            _context = context;
        }

        public byte[] GenerateEmployeeScheduleReport(int employeeId)
        {
            var schedules = _context.Schedules
                .Where(s => s.EmployeeId == employeeId)
                .OrderBy(s => s.StartDateTime)
                .ToList();

            if (!schedules.Any())
            {
                throw new Exception("No schedules found for this employee.");
            }

            var employeeName = _context.Employees
                .Where(e => e.EmployeeId == employeeId)
                .Select(e => e.Name)
                .FirstOrDefault();

            if (employeeName == null)
            {
                throw new Exception("Employee not found.");
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    // Title with added spacing below it
                    page.Header().Text($"Schedule Report for {employeeName}")
                        .FontSize(20)
                        .SemiBold()
                        .AlignCenter();
                        
                    
                    // Table with borders
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(150);  // Date column
                            columns.ConstantColumn(100);  // Start Time column
                            columns.ConstantColumn(100);  // End Time column
                        });
                        

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Date").SemiBold();
                            header.Cell().Element(CellStyle).Text("Start Time").SemiBold();
                            header.Cell().Element(CellStyle).Text("End Time").SemiBold();
                        });

                        foreach (var schedule in schedules)
                        {
                            table.Cell().Element(CellStyle).Text(schedule.StartDateTime.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(schedule.StartDateTime.ToString("HH:mm"));
                            table.Cell().Element(CellStyle).Text(schedule.EndDateTime.ToString("HH:mm"));
                        }
                    });

                    // Footer with generation timestamp
                    page.Footer().Text(text =>
                    {
                        text.Span("Generated on: ").SemiBold();
                        text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    });
                });
            });

            return document.GeneratePdf();
        }

// Helper method to apply cell styling with borders
static IContainer CellStyle(IContainer container)
{
    return container
        .Border(1)
        .Padding(5)
        .AlignLeft();
}

}

