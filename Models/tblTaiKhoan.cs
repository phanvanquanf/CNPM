using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotels.Areas.Admin.Models
{
    [Table("tblTaiKhoan")]
    public class tblTaiKhoan
    {
        [Key]
        public long IDTaiKhoan { get; set; }

        public string? TenDangNhap { get; set; }

        public string? MatKhau { get; set; }

        public bool TrangThai { get; set; } = true;

        public int VaiTro { get; set; } = 2;

        public string? Image { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
