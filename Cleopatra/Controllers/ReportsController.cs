using Cleopatra.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Cleopatra.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly PdfReportService _pdfReportService;

        public ReportsController(PdfReportService pdfReportService)
        {
            _pdfReportService = pdfReportService;
        }

        // GET: api/reports/employee/{id}
        [HttpGet("employee/{id}")]
        public IActionResult GetEmployeeScheduleReport(int id)
        {
            try
            {
                var pdfBytes = _pdfReportService.GenerateEmployeeScheduleReport(id);
                return File(pdfBytes, "application/pdf", $"Employee_Schedule_{id}.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}