using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotels.Areas.Admin.Models
{
    [Table("tblDatDV")]
    public class tblDatDV
    {
        [Key]
        public long IDDatDichVu { get; set; }
        public long IDTaiKhoan { get; set; }

        [ForeignKey(nameof(IDTaiKhoan))]
        public virtual tblTaiKhoan? TaiKhoan { get; set; }
        public long IDDichVu { get; set; }

        [ForeignKey(nameof(IDDichVu))]
        public virtual tblDichVu? DichVu { get; set; }
        public DateTime NgaySuDung { get; set; } = DateTime.Now;
        public TimeSpan? GioSuDung { get; set; }
        public string? DiemDon { get; set; }
        public int SoLuong { get; set; } = 1;
        public int? SoHanhKhach { get; set; }
        public int ThanhTien { get; set; }
        public int TrangThai { get; set; } = 0;
        public string? GhiChu { get; set; }
    }
    public class SelectedDishViewModel
    {
        public bool IsSelected { get; set; }
        public int Quantity { get; set; }
        public int ThanhTien { get; set; }
    }

    public class BookingServiceViewModel
    {
        public int IDTaiKhoan { get; set; }
        public DateTime NgaySuDung { get; set; }
        public int SoHanhKhach { get; set; }
        public string? GhiChu { get; set; }
        public TimeSpan? GioSuDung { get; set; }
        public Dictionary<long, SelectedDishViewModel> SelectedDishes { get; set; } = new();
    }

}
