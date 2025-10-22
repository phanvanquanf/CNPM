using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotels.Models
{
    [Table("tblDatPhong")]
    public class tblDatPhong
    {
        [Key]
        public long IDMaDatPhong { get; set; }

        public long? IDKhachHang { get; set; }

        public DateTime? NgayDen { get; set; }

        public DateTime? NgayDi { get; set; }

        public int? TongSoKhach { get; set; }

        public int? TongSoPhong { get; set; }

        public int TrangThai { get; set; } = 0;

        public DateTime? NgayTao { get; set; } = DateTime.Now;

        public string? GhiChu { get; set; }
    }
}
