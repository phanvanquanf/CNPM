using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotels.Areas.Admin.Models
{
    [Table("tblKhachHang")]
    public class tblKhachHang
    {
        [Key]
        public long IDKhachHang { get; set; }

        public long? IDTaiKhoan { get; set; }

        [ForeignKey(nameof(IDTaiKhoan))]
        public virtual tblTaiKhoan? TaiKhoan { get; set; }

        public string? HoTen { get; set; }

        public string? GioiTinh { get; set; }

        public string? CCCD { get; set; }

        public string? SDT { get; set; }

        public string? Email { get; set; }

        public string? DiaChi { get; set; }

        public int TrangThai { get; set; } = 0;
    }
}
