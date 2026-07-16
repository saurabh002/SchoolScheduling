using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SchoolScheduling.Endpoints
{
    public static class ReportExportEndpoints
    {
        public static void MapReportExportEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("api/reports/export", ExportMonthlyReport)
                .WithName("ExportMonthlyReport")
                .Produces<FileContentResult>(StatusCodes.Status200OK, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
 
        private static async Task<IResult> ExportMonthlyReport(
            string month, // format: "2026-06"
            SchoolDbContext db,
            CancellationToken ct)
        {
            if (!DateOnly.TryParseExact(month + "-01", "yyyy-MM-dd", out var firstDay))
                return Results.BadRequest("Invalid month format. Use yyyy-MM e.g. 2026-06");
    
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var monthLabel = firstDay.ToString("MMMM yyyy");
    
            // Absences with period + substitute details
            var absenceRows = await db.AbsencePeriods
                .Where(ap => ap.Absence.Date >= firstDay && ap.Absence.Date <= lastDay)
                .OrderBy(ap => ap.Absence.Date)
                .ThenBy(ap => ap.Absence.Teacher.Name)
                .Select(ap => new
                {
                    Date = ap.Absence.Date,
                    AbsentTeacher = ap.Absence.Teacher.Name,
                    Class = ap.TimetableEntry.ClassSection.Name,
                    Period = $"Period {ap.TimetableEntry.Period.Number} ({ap.TimetableEntry.Period.StartTime:hh\\:mm} - {ap.TimetableEntry.Period.EndTime:hh\\:mm})",
                    Substitute = ap.SubstituteTeacher != null ? ap.SubstituteTeacher.Name : "Unassigned",
                })
                .ToListAsync(ct);
    
            // Workload summary for the month
            var teachers = await db.Teachers
                .Select(t => new
                {
                    t.Name,
                    RegularLoad = t.TimetableEntries.Count(),
                    SubsTaken = db.AbsencePeriods.Count(ap =>
                        ap.SubstituteTeacherId == t.Id &&
                        ap.Absence.Date >= firstDay &&
                        ap.Absence.Date <= lastDay),
                    MissedClasses = db.AbsencePeriods.Count(ap =>
                        ap.Absence.TeacherId == t.Id &&
                        ap.Absence.Date >= firstDay &&
                        ap.Absence.Date <= lastDay),
                })
                .ToListAsync(ct);
    
            using var workbook = new XLWorkbook();
    
            // Sheet 1 — Absences
            var absenceSheet = workbook.Worksheets.Add("Absences");
    
            // Title
            absenceSheet.Cell(1, 1).Value = $"Monthly Absence Report — {monthLabel}";
            var titleRange = absenceSheet.Range(1, 1, 1, 5);
            titleRange.Merge();
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 13;
            titleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50");
            titleRange.Style.Font.FontColor = XLColor.White;
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
    
            // Headers
            var absenceHeaders = new[] { "Date", "Absent Teacher", "Class", "Period", "Substitute" };
            for (int i = 0; i < absenceHeaders.Length; i++)
            {
                var cell = absenceSheet.Cell(2, i + 1);
                cell.Value = absenceHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#f1f3f5");
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }
    
            // Data rows
            for (int i = 0; i < absenceRows.Count; i++)
            {
                var row = absenceRows[i];
                var r = i + 3;
                absenceSheet.Cell(r, 1).Value = row.Date.ToString("dd MMM yyyy");
                absenceSheet.Cell(r, 2).Value = row.AbsentTeacher;
                absenceSheet.Cell(r, 3).Value = row.Class;
                absenceSheet.Cell(r, 4).Value = row.Period;
                absenceSheet.Cell(r, 5).Value = row.Substitute;
    
                if (row.Substitute == "Unassigned")
                    absenceSheet.Cell(r, 5).Style.Font.FontColor = XLColor.Red;
    
                if (i % 2 == 1)
                {
                    absenceSheet.Range(r, 1, r, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#f9f9f9");
                }
            }
    
            absenceSheet.Columns().AdjustToContents();
    
            // Sheet 2 — Workload Summary
            var workloadSheet = workbook.Worksheets.Add("Workload Summary");
    
            workloadSheet.Cell(1, 1).Value = $"Workload Summary — {monthLabel}";
            var wTitleRange = workloadSheet.Range(1, 1, 1, 5);
            wTitleRange.Merge();
            wTitleRange.Style.Font.Bold = true;
            wTitleRange.Style.Font.FontSize = 13;
            wTitleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50");
            wTitleRange.Style.Font.FontColor = XLColor.White;
    
            var workloadHeaders = new[] { "Teacher", "Regular Load", "Subs Taken", "Missed Classes", "Effective Load" };
            for (int i = 0; i < workloadHeaders.Length; i++)
            {
                var cell = workloadSheet.Cell(2, i + 1);
                cell.Value = workloadHeaders[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#f1f3f5");
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }
    
            var sorted = teachers
                .OrderByDescending(t => t.RegularLoad + t.SubsTaken - t.MissedClasses)
                .ToList();
    
            for (int i = 0; i < sorted.Count; i++)
            {
                var t = sorted[i];
                var effectiveLoad = t.RegularLoad + t.SubsTaken - t.MissedClasses;
                var r = i + 3;
                workloadSheet.Cell(r, 1).Value = t.Name;
                workloadSheet.Cell(r, 2).Value = t.RegularLoad;
                workloadSheet.Cell(r, 3).Value = t.SubsTaken;
                workloadSheet.Cell(r, 4).Value = t.MissedClasses;
                workloadSheet.Cell(r, 5).Value = effectiveLoad;
    
                // Colour code effective load
                var loadColor = effectiveLoad >= 8 ? XLColor.FromHtml("#fde8e8")
                            : effectiveLoad >= 4 ? XLColor.FromHtml("#fff4e0")
                            : XLColor.FromHtml("#e6f4ea");
                workloadSheet.Cell(r, 5).Style.Fill.BackgroundColor = loadColor;
    
                if (i % 2 == 1)
                {
                    workloadSheet.Range(r, 1, r, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#f9f9f9");
                }
            }
    
            workloadSheet.Columns().AdjustToContents();
    
            // Stream to response
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var bytes = stream.ToArray();
            var fileName = $"SubstitutionReport_{firstDay:yyyy-MM}.xlsx";
    
            return Results.File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}