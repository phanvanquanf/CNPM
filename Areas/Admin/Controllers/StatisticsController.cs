using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using hotels.Models;
using hotels.Areas.Admin.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace hotels.Areas.Admin.Controllers;

[Area("Admin")]
public class StatisticsController : Controller
{
    private readonly DataContext _context;

    public StatisticsController(DataContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        ViewBag.Title = "Báo cáo thống kê";
        return View();
    }

    [HttpGet]
    public JsonResult GetRevenueStatistics(string period = "month")
    {
        var today = DateTime.Now;
        DateTime startDate;
        DateTime endDate = today;

        switch (period.ToLower())
        {
            case "day":
                startDate = today.Date;
                break;
            case "week":
                startDate = today.AddDays(-7);
                break;
            case "month":
                startDate = new DateTime(today.Year, today.Month, 1);
                break;
            case "year":
                startDate = new DateTime(today.Year, 1, 1);
                break;
            default:
                startDate = new DateTime(today.Year, today.Month, 1);
                break;
        }

        var bookings = _context.DatPhongs
            .Where(dp => dp.NgayTao >= startDate && dp.NgayTao <= endDate && dp.TrangThai >= 1)
            .Include(dp => dp.CTDatPhongs!)
                .ThenInclude(ct => ct.Phong!)
            .ToList();

        var revenue = bookings.Sum(dp =>
        {
            if (dp.CTDatPhongs == null || !dp.CTDatPhongs.Any())
                return 0;

            var nights = (dp.NgayDi - dp.NgayDen)?.Days ?? 1;
            return dp.CTDatPhongs.Sum(ct => (ct.Phong?.GiaPhong ?? 0) * nights);
        });

        var previousPeriodStart = period.ToLower() switch
        {
            "day" => startDate.AddDays(-1),
            "week" => startDate.AddDays(-7),
            "month" => startDate.AddMonths(-1),
            "year" => startDate.AddYears(-1),
            _ => startDate.AddMonths(-1)
        };

        var previousPeriodEnd = startDate.AddDays(-1);

        var previousBookings = _context.DatPhongs
            .Where(dp => dp.NgayTao >= previousPeriodStart && dp.NgayTao <= previousPeriodEnd && dp.TrangThai >= 1)
            .Include(dp => dp.CTDatPhongs!)
                .ThenInclude(ct => ct.Phong!)
            .ToList();

        var previousRevenue = previousBookings.Sum(dp =>
        {
            if (dp.CTDatPhongs == null || !dp.CTDatPhongs.Any())
                return 0;

            var nights = (dp.NgayDi - dp.NgayDen)?.Days ?? 1;
            return dp.CTDatPhongs.Sum(ct => (ct.Phong?.GiaPhong ?? 0) * nights);
        });

        var growthRate = previousRevenue > 0 
            ? ((revenue - previousRevenue) / (double)previousRevenue * 100) 
            : 0;

        return Json(new
        {
            revenue = revenue,
            previousRevenue = previousRevenue,
            growthRate = Math.Round(growthRate, 2),
            bookingCount = bookings.Count,
            previousBookingCount = previousBookings.Count
        });
    }

    [HttpGet]
    public JsonResult GetBookingStatistics()
    {
        var totalBookings = _context.DatPhongs.Count();
        var confirmedBookings = _context.DatPhongs.Count(dp => dp.TrangThai == 1);
        var pendingBookings = _context.DatPhongs.Count(dp => dp.TrangThai == 0);
        var completedBookings = _context.DatPhongs.Count(dp => dp.TrangThai == 3);
        var cancelledBookings = _context.DatPhongs.Count(dp => dp.TrangThai == 4);

        var today = DateTime.Now.Date;
        var todayBookings = _context.DatPhongs.Count(dp => dp.NgayTao != null && dp.NgayTao.Value.Date == today);

        var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var thisMonthBookings = _context.DatPhongs.Count(dp => dp.NgayTao >= thisMonth);

        return Json(new
        {
            total = totalBookings,
            confirmed = confirmedBookings,
            pending = pendingBookings,
            completed = completedBookings,
            cancelled = cancelledBookings,
            today = todayBookings,
            thisMonth = thisMonthBookings
        });
    }

    [HttpGet]
    public JsonResult GetRoomStatistics()
    {
        var totalRooms = _context.Phongs.Count();
        var availableRooms = _context.Phongs.Count(p => p.TrangThai == 0);
        var occupiedRooms = _context.Phongs.Count(p => p.TrangThai == 1);
        var maintenanceRooms = _context.Phongs.Count(p => p.TrangThai == 2);

        var occupancyRate = totalRooms > 0 
            ? Math.Round((occupiedRooms / (double)totalRooms) * 100, 2) 
            : 0;

        var roomTypes = _context.Phongs
            .Include(p => p.LoaiPhong)
            .GroupBy(p => p.LoaiPhong != null ? p.LoaiPhong.LoaiPhong : "Chưa phân loại")
            .Select(g => new
            {
                type = g.Key,
                count = g.Count(),
                available = g.Count(p => p.TrangThai == 0),
                occupied = g.Count(p => p.TrangThai == 1)
            })
            .ToList();

        return Json(new
        {
            total = totalRooms,
            available = availableRooms,
            occupied = occupiedRooms,
            maintenance = maintenanceRooms,
            occupancyRate = occupancyRate,
            roomTypes = roomTypes
        });
    }

    [HttpGet]
    public JsonResult GetCustomerStatistics()
    {
        var totalCustomers = _context.KhachHangs.Count();
        var activeCustomers = _context.KhachHangs.Count(k => k.TrangThai == 0);

        // Count new customers based on first booking date
        var today = DateTime.Now.Date;
        var newCustomersToday = _context.KhachHangs
            .Where(k => _context.DatPhongs
                .Where(dp => dp.IDKhachHang == k.IDKhachHang)
                .Any(dp => dp.NgayTao != null && dp.NgayTao.Value.Date == today))
            .Count();

        var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var newCustomersThisMonth = _context.KhachHangs
            .Where(k => _context.DatPhongs
                .Where(dp => dp.IDKhachHang == k.IDKhachHang)
                .Any(dp => dp.NgayTao >= thisMonth))
            .Count();

        var topCustomers = _context.KhachHangs
            .Select(k => new
            {
                id = k.IDKhachHang,
                name = k.HoTen,
                email = k.Email,
                phone = k.SDT,
                bookingCount = _context.DatPhongs.Count(dp => dp.IDKhachHang == k.IDKhachHang)
            })
            .OrderByDescending(c => c.bookingCount)
            .Take(10)
            .ToList();

        return Json(new
        {
            total = totalCustomers,
            active = activeCustomers,
            newToday = newCustomersToday,
            newThisMonth = newCustomersThisMonth,
            topCustomers = topCustomers
        });
    }

    [HttpGet]
    public JsonResult GetServiceStatistics()
    {
        var totalServices = _context.DichVus.Count();
        var activeServices = _context.DichVus.Count(dv => dv.TrangThai == 0);

        var serviceBookings = _context.DatDVs
            .Include(dv => dv.DichVu)
            .Where(dv => dv.TrangThai >= 1)
            .GroupBy(dv => dv.DichVu != null ? dv.DichVu.DichVu : "Không xác định")
            .Select(g => new
            {
                serviceName = g.Key,
                bookingCount = g.Count(),
                totalRevenue = g.Sum(dv => dv.ThanhTien),
                serviceId = 0 // Not needed for display
            })
            .OrderByDescending(s => s.bookingCount)
            .Take(10)
            .ToList();

        var totalServiceRevenue = _context.DatDVs
            .Where(dv => dv.TrangThai >= 1)
            .Sum(dv => dv.ThanhTien);

        var today = DateTime.Now.Date;
        var todayServiceBookings = _context.DatDVs.Count(dv => dv.NgaySuDung.Date == today && dv.TrangThai >= 1);

        return Json(new
        {
            total = totalServices,
            active = activeServices,
            serviceBookings = serviceBookings,
            totalRevenue = totalServiceRevenue,
            todayBookings = todayServiceBookings
        });
    }

    [HttpGet]
    public JsonResult GetRevenueByMonth(int year)
    {
        if (year == 0) year = DateTime.Now.Year;

        var monthlyRevenue = new List<object>();

        for (int month = 1; month <= 12; month++)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var bookings = _context.DatPhongs
                .Where(dp => dp.NgayTao >= startDate && dp.NgayTao <= endDate && dp.TrangThai >= 1)
                .Include(dp => dp.CTDatPhongs!)
                    .ThenInclude(ct => ct.Phong!)
                .ToList();

            var revenue = bookings.Sum(dp =>
            {
                if (dp.CTDatPhongs == null || !dp.CTDatPhongs.Any())
                    return 0;

                var nights = (dp.NgayDi - dp.NgayDen)?.Days ?? 1;
                return dp.CTDatPhongs.Sum(ct => (ct.Phong?.GiaPhong ?? 0) * nights);
            });

            monthlyRevenue.Add(new
            {
                month = month,
                monthName = new DateTime(year, month, 1).ToString("MMM"),
                revenue = revenue,
                bookingCount = bookings.Count
            });
        }

        return Json(monthlyRevenue);
    }

    [HttpGet]
    public JsonResult GetRevenueByDay(int days = 30)
    {
        var endDate = DateTime.Now.Date;
        var startDate = endDate.AddDays(-days);

        var dailyRevenue = new List<object>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var bookings = _context.DatPhongs
                .Where(dp => dp.NgayTao != null && dp.NgayTao.Value.Date == date && dp.TrangThai >= 1)
                .Include(dp => dp.CTDatPhongs!)
                    .ThenInclude(ct => ct.Phong!)
                .ToList();

            var revenue = bookings.Sum(dp =>
            {
                if (dp.CTDatPhongs == null || !dp.CTDatPhongs.Any())
                    return 0;

                var nights = (dp.NgayDi - dp.NgayDen)?.Days ?? 1;
                if (nights <= 0) nights = 1;
                return dp.CTDatPhongs.Sum(ct => (ct.Phong?.GiaPhong ?? 0) * nights);
            });

            dailyRevenue.Add(new
            {
                date = date.ToString("dd/MM"),
                revenue = revenue,
                bookingCount = bookings.Count
            });
        }

        return Json(dailyRevenue);
    }

    [HttpGet]
    public JsonResult GetRevenueByRoomType()
    {
        try
        {
            var roomTypeRevenue = _context.CTDatPhongs
                .Include(ct => ct.Phong!)
                    .ThenInclude(p => p.LoaiPhong)
                .Include(ct => ct.DatPhong!)
                .Where(ct => ct.DatPhong != null && ct.DatPhong.TrangThai >= 1 && 
                             ct.DatPhong.NgayDen != null && ct.DatPhong.NgayDi != null)
                .ToList()
                .GroupBy(ct => ct.Phong != null && ct.Phong.LoaiPhong != null 
                    ? ct.Phong.LoaiPhong.LoaiPhong 
                    : "Chưa phân loại")
                .Select(g => new
                {
                    roomType = g.Key ?? "Chưa phân loại",
                    revenue = g.Sum(ct =>
                    {
                        if (ct.DatPhong == null || ct.DatPhong.NgayDen == null || ct.DatPhong.NgayDi == null)
                            return 0;
                        var nights = (ct.DatPhong.NgayDi.Value - ct.DatPhong.NgayDen.Value).Days;
                        if (nights <= 0) nights = 1;
                        return (ct.Phong?.GiaPhong ?? 0) * nights;
                    }),
                    bookingCount = g.Count()
                })
                .Where(r => r.revenue > 0)
                .OrderByDescending(r => r.revenue)
                .Take(5)
                .ToList();

            return Json(roomTypeRevenue);
        }
        catch
        {
            return Json(new List<object>());
        }
    }

    [HttpGet]
    public JsonResult GetBookingTrend(int months = 6)
    {
        var endDate = DateTime.Now;
        var startDate = endDate.AddMonths(-months);

        var monthlyTrend = new List<object>();

        // Đảm bảo bắt đầu từ tháng đầu tiên
        var firstMonth = new DateTime(startDate.Year, startDate.Month, 1);
        
        for (int i = 0; i <= months; i++)
        {
            var monthStart = firstMonth.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            
            // Đảm bảo không vượt quá ngày hiện tại
            if (monthEnd > endDate)
                monthEnd = endDate;

            var bookings = _context.DatPhongs
                .Where(dp => dp.NgayTao != null && 
                             dp.NgayTao >= monthStart && 
                             dp.NgayTao <= monthEnd)
                .ToList();

            monthlyTrend.Add(new
            {
                month = monthStart.ToString("MM/yyyy"),
                monthName = monthStart.ToString("MMM yyyy"),
                total = bookings.Count,
                confirmed = bookings.Count(dp => dp.TrangThai == 1),
                completed = bookings.Count(dp => dp.TrangThai == 3),
                cancelled = bookings.Count(dp => dp.TrangThai == 4)
            });
        }

        return Json(monthlyTrend);
    }

    [HttpGet]
    public JsonResult GetTopServicesChart()
    {
        try
        {
            var topServices = _context.DatDVs
                .Include(dv => dv.DichVu)
                .Where(dv => dv.TrangThai >= 1)
                .ToList()
                .GroupBy(dv => dv.DichVu != null && !string.IsNullOrEmpty(dv.DichVu.DichVu) 
                    ? dv.DichVu.DichVu 
                    : "Không xác định")
                .Select(g => new
                {
                    serviceName = g.Key ?? "Không xác định",
                    bookingCount = g.Count(),
                    totalRevenue = g.Sum(dv => dv.ThanhTien)
                })
                .Where(s => s.totalRevenue > 0)
                .OrderByDescending(s => s.totalRevenue)
                .Take(5)
                .ToList();

            return Json(topServices);
        }
        catch
        {
            return Json(new List<object>());
        }
    }

    [HttpGet]
    public JsonResult GetBookingByHour()
    {
        var today = DateTime.Now.Date;
        var bookings = _context.DatPhongs
            .Where(dp => dp.NgayTao != null && dp.NgayTao.Value.Date == today)
            .ToList();

        var hourlyData = new List<object>();

        for (int hour = 0; hour < 24; hour++)
        {
            var hourStart = today.AddHours(hour);
            var hourEnd = hourStart.AddHours(1);
            
            // Nếu là giờ hiện tại, chỉ tính đến thời điểm hiện tại
            if (hourEnd > DateTime.Now)
                hourEnd = DateTime.Now;

            var count = bookings.Count(dp => 
                dp.NgayTao != null &&
                dp.NgayTao >= hourStart && 
                dp.NgayTao < hourEnd);

            hourlyData.Add(new
            {
                hour = hour.ToString("00") + ":00",
                hourValue = hour,
                count = count
            });
        }

        return Json(hourlyData);
    }

    [HttpGet]
    public IActionResult ExportToExcel(string period = "month")
    {
        // EPPlus 8+ không cần set license trong code nếu đã có license file
        // Hoặc có thể set trong Program.cs hoặc appsettings
        
        using (var package = new ExcelPackage())
        {
            var today = DateTime.Now;
            DateTime startDate;
            DateTime endDate = today;

            switch (period.ToLower())
            {
                case "day":
                    startDate = today.Date;
                    break;
                case "week":
                    startDate = today.AddDays(-7);
                    break;
                case "month":
                    startDate = new DateTime(today.Year, today.Month, 1);
                    break;
                case "year":
                    startDate = new DateTime(today.Year, 1, 1);
                    break;
                default:
                    startDate = new DateTime(today.Year, today.Month, 1);
                    break;
            }

            // Sheet 1: Tổng quan
            var summarySheet = package.Workbook.Worksheets.Add("Tổng quan");
            CreateSummarySheet(summarySheet, startDate, endDate, period);

            // Sheet 2: Doanh thu theo tháng
            var revenueSheet = package.Workbook.Worksheets.Add("Doanh thu theo tháng");
            CreateRevenueByMonthSheet(revenueSheet, today.Year);

            // Sheet 3: Doanh thu theo loại phòng
            var roomTypeSheet = package.Workbook.Worksheets.Add("Doanh thu theo loại phòng");
            CreateRevenueByRoomTypeSheet(roomTypeSheet);

            // Sheet 4: Đặt phòng
            var bookingSheet = package.Workbook.Worksheets.Add("Thống kê đặt phòng");
            CreateBookingSheet(bookingSheet, startDate, endDate);

            // Sheet 5: Dịch vụ
            var serviceSheet = package.Workbook.Worksheets.Add("Thống kê dịch vụ");
            CreateServiceSheet(serviceSheet);

            // Sheet 6: Khách hàng
            var customerSheet = package.Workbook.Worksheets.Add("Khách hàng thân thiết");
            CreateCustomerSheet(customerSheet);

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"BaoCaoThongKe_{period}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(), 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                fileName);
        }
    }

    private void CreateSummarySheet(ExcelWorksheet sheet, DateTime startDate, DateTime endDate, string period)
    {
        // Header
        sheet.Cells[1, 1].Value = "BÁO CÁO THỐNG KÊ KHÁCH SẠN";
        sheet.Cells[1, 1, 1, 4].Merge = true;
        sheet.Cells[1, 1].Style.Font.Size = 16;
        sheet.Cells[1, 1].Style.Font.Bold = true;
        sheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        sheet.Cells[2, 1].Value = $"Kỳ báo cáo: {GetPeriodName(period)}";
        sheet.Cells[2, 1, 2, 4].Merge = true;
        sheet.Cells[2, 1].Style.Font.Size = 12;
        sheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        sheet.Cells[3, 1].Value = $"Từ ngày: {startDate:dd/MM/yyyy} đến {endDate:dd/MM/yyyy}";
        sheet.Cells[3, 1, 3, 4].Merge = true;
        sheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        // Thống kê chính
        int row = 5;
        sheet.Cells[row, 1].Value = "CHỈ TIÊU";
        sheet.Cells[row, 2].Value = "GIÁ TRỊ";
        sheet.Cells[row, 1].Style.Font.Bold = true;
        sheet.Cells[row, 2].Style.Font.Bold = true;
        sheet.Cells[row, 1, row, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
        sheet.Cells[row, 1, row, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
        row++;

        var bookings = _context.DatPhongs
            .Where(dp => dp.NgayTao >= startDate && dp.NgayTao <= endDate && dp.TrangThai >= 1)
            .Include(dp => dp.CTDatPhongs!)
                .ThenInclude(ct => ct.Phong!)
            .ToList();

        var revenue = bookings.Sum(dp =>
        {
            if (dp.CTDatPhongs == null || !dp.CTDatPhongs.Any())
                return 0;
            var nights = (dp.NgayDi - dp.NgayDen)?.Days ?? 1;
            if (nights <= 0) nights = 1;
            return dp.CTDatPhongs.Sum(ct => (ct.Phong?.GiaPhong ?? 0) * nights);
        });

        sheet.Cells[row, 1].Value = "Tổng doanh thu";
        sheet.Cells[row, 2].Value = revenue;
        sheet.Cells[row, 2].Style.Numberformat.Format = "#,##0";
        row++;

        sheet.Cells[row, 1].Value = "Tổng số đặt phòng";
        sheet.Cells[row, 2].Value = bookings.Count;
        row++;

        var totalRooms = _context.Phongs.Count();
        var occupiedRooms = _context.Phongs.Count(p => p.TrangThai == 1);
        var occupancyRate = totalRooms > 0 ? Math.Round((occupiedRooms / (double)totalRooms) * 100, 2) : 0;

        sheet.Cells[row, 1].Value = "Tỷ lệ lấp đầy";
        sheet.Cells[row, 2].Value = occupancyRate / 100;
        sheet.Cells[row, 2].Style.Numberformat.Format = "0.00%";
        row++;

        var totalCustomers = _context.KhachHangs.Count();
        sheet.Cells[row, 1].Value = "Tổng số khách hàng";
        sheet.Cells[row, 2].Value = totalCustomers;
        row++;

        // Định dạng cột
        sheet.Column(1).Width = 25;
        sheet.Column(2).Width = 20;
    }

    private void CreateRevenueByMonthSheet(ExcelWorksheet sheet, int year)
    {
        sheet.Cells[1, 1].Value = "Tháng";
        sheet.Cells[1, 2].Value = "Doanh thu";
        sheet.Cells[1, 3].Value = "Số đơn";
        sheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;
        sheet.Cells[1, 1, 1, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
        sheet.Cells[1, 1, 1, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

        int row = 2;
        for (int month = 1; month <= 12; month++)
        {
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var bookings = _context.DatPhongs
                .Where(dp => dp.NgayTao >= monthStart && dp.NgayTao <= monthEnd && dp.TrangThai >= 1)
                .Include(dp => dp.CTDatPhongs!)
                    .ThenInclude(ct => ct.Phong!)
                .ToList();

            var revenue = bookings.Sum(dp =>
            {
                if (dp.CTDatPhongs == null || !dp.CTDatPhongs.Any())
                    return 0;
                var nights = (dp.NgayDi - dp.NgayDen)?.Days ?? 1;
                if (nights <= 0) nights = 1;
                return dp.CTDatPhongs.Sum(ct => (ct.Phong?.GiaPhong ?? 0) * nights);
            });

            sheet.Cells[row, 1].Value = monthStart.ToString("MM/yyyy");
            sheet.Cells[row, 2].Value = revenue;
            sheet.Cells[row, 2].Style.Numberformat.Format = "#,##0";
            sheet.Cells[row, 3].Value = bookings.Count;
            row++;
        }

        sheet.Column(1).Width = 15;
        sheet.Column(2).Width = 20;
        sheet.Column(3).Width = 15;
    }

    private void CreateRevenueByRoomTypeSheet(ExcelWorksheet sheet)
    {
        sheet.Cells[1, 1].Value = "Loại phòng";
        sheet.Cells[1, 2].Value = "Doanh thu";
        sheet.Cells[1, 3].Value = "Số đơn";
        sheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;
        sheet.Cells[1, 1, 1, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
        sheet.Cells[1, 1, 1, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

        var roomTypeRevenue = _context.CTDatPhongs
            .Include(ct => ct.Phong!)
                .ThenInclude(p => p.LoaiPhong)
            .Include(ct => ct.DatPhong!)
            .Where(ct => ct.DatPhong != null && ct.DatPhong.TrangThai >= 1 && 
                         ct.DatPhong.NgayDen != null && ct.DatPhong.NgayDi != null)
            .ToList()
            .GroupBy(ct => ct.Phong != null && ct.Phong.LoaiPhong != null 
                ? ct.Phong.LoaiPhong.LoaiPhong 
                : "Chưa phân loại")
            .Select(g => new
            {
                roomType = g.Key ?? "Chưa phân loại",
                revenue = g.Sum(ct =>
                {
                    if (ct.DatPhong == null || ct.DatPhong.NgayDen == null || ct.DatPhong.NgayDi == null)
                        return 0;
                    var nights = (ct.DatPhong.NgayDi.Value - ct.DatPhong.NgayDen.Value).Days;
                    if (nights <= 0) nights = 1;
                    return (ct.Phong?.GiaPhong ?? 0) * nights;
                }),
                bookingCount = g.Count()
            })
            .Where(r => r.revenue > 0)
            .OrderByDescending(r => r.revenue)
            .Take(10)
            .ToList();

        int row = 2;
        foreach (var item in roomTypeRevenue)
        {
            sheet.Cells[row, 1].Value = item.roomType;
            sheet.Cells[row, 2].Value = item.revenue;
            sheet.Cells[row, 2].Style.Numberformat.Format = "#,##0";
            sheet.Cells[row, 3].Value = item.bookingCount;
            row++;
        }

        sheet.Column(1).Width = 25;
        sheet.Column(2).Width = 20;
        sheet.Column(3).Width = 15;
    }

    private void CreateBookingSheet(ExcelWorksheet sheet, DateTime startDate, DateTime endDate)
    {
        sheet.Cells[1, 1].Value = "Trạng thái";
        sheet.Cells[1, 2].Value = "Số lượng";
        sheet.Cells[1, 1, 1, 2].Style.Font.Bold = true;
        sheet.Cells[1, 1, 1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
        sheet.Cells[1, 1, 1, 2].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

        var bookings = _context.DatPhongs
            .Where(dp => dp.NgayTao >= startDate && dp.NgayTao <= endDate)
            .ToList();

        int row = 2;
        sheet.Cells[row, 1].Value = "Tổng số";
        sheet.Cells[row, 2].Value = bookings.Count;
        row++;

        sheet.Cells[row, 1].Value = "Đã xác nhận";
        sheet.Cells[row, 2].Value = bookings.Count(dp => dp.TrangThai == 1);
        row++;

        sheet.Cells[row, 1].Value = "Chờ xác nhận";
        sheet.Cells[row, 2].Value = bookings.Count(dp => dp.TrangThai == 0);
        row++;

        sheet.Cells[row, 1].Value = "Hoàn thành";
        sheet.Cells[row, 2].Value = bookings.Count(dp => dp.TrangThai == 3);
        row++;

        sheet.Cells[row, 1].Value = "Đã hủy";
        sheet.Cells[row, 2].Value = bookings.Count(dp => dp.TrangThai == 4);
        row++;

        sheet.Column(1).Width = 20;
        sheet.Column(2).Width = 15;
    }

    private void CreateServiceSheet(ExcelWorksheet sheet)
    {
        sheet.Cells[1, 1].Value = "Dịch vụ";
        sheet.Cells[1, 2].Value = "Số đơn";
        sheet.Cells[1, 3].Value = "Doanh thu";
        sheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;
        sheet.Cells[1, 1, 1, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
        sheet.Cells[1, 1, 1, 3].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

        var topServices = _context.DatDVs
            .Include(dv => dv.DichVu)
            .Where(dv => dv.TrangThai >= 1)
            .ToList()
            .GroupBy(dv => dv.DichVu != null && !string.IsNullOrEmpty(dv.DichVu.DichVu) 
                ? dv.DichVu.DichVu 
                : "Không xác định")
            .Select(g => new
            {
                serviceName = g.Key ?? "Không xác định",
                bookingCount = g.Count(),
                totalRevenue = g.Sum(dv => dv.ThanhTien)
            })
            .Where(s => s.totalRevenue > 0)
            .OrderByDescending(s => s.totalRevenue)
            .Take(10)
            .ToList();

        int row = 2;
        foreach (var service in topServices)
        {
            sheet.Cells[row, 1].Value = service.serviceName;
            sheet.Cells[row, 2].Value = service.bookingCount;
            sheet.Cells[row, 3].Value = service.totalRevenue;
            sheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
            row++;
        }

        sheet.Column(1).Width = 30;
        sheet.Column(2).Width = 15;
        sheet.Column(3).Width = 20;
    }

    private void CreateCustomerSheet(ExcelWorksheet sheet)
    {
        sheet.Cells[1, 1].Value = "STT";
        sheet.Cells[1, 2].Value = "Tên khách hàng";
        sheet.Cells[1, 3].Value = "Email";
        sheet.Cells[1, 4].Value = "Số đơn";
        sheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;
        sheet.Cells[1, 1, 1, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
        sheet.Cells[1, 1, 1, 4].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

        var topCustomers = _context.KhachHangs
            .Select(k => new
            {
                id = k.IDKhachHang,
                name = k.HoTen,
                email = k.Email,
                phone = k.SDT,
                bookingCount = _context.DatPhongs.Count(dp => dp.IDKhachHang == k.IDKhachHang)
            })
            .OrderByDescending(c => c.bookingCount)
            .Take(20)
            .ToList();

        int row = 2;
        int stt = 1;
        foreach (var customer in topCustomers)
        {
            sheet.Cells[row, 1].Value = stt++;
            sheet.Cells[row, 2].Value = customer.name ?? "N/A";
            sheet.Cells[row, 3].Value = customer.email ?? "N/A";
            sheet.Cells[row, 4].Value = customer.bookingCount;
            row++;
        }

        sheet.Column(1).Width = 8;
        sheet.Column(2).Width = 30;
        sheet.Column(3).Width = 30;
        sheet.Column(4).Width = 12;
    }

    private string GetPeriodName(string period)
    {
        return period.ToLower() switch
        {
            "day" => "Hôm nay",
            "week" => "Tuần này",
            "month" => "Tháng này",
            "year" => "Năm nay",
            _ => "Tháng này"
        };
    }
}

