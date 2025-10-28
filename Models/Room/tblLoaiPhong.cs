using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotels.Models
{
    [Table("tblLoaiPhong")]
    public class tblLoaiPhong
    {
        [Key]
        public long IDLoaiPhong { get; set; }

        public string? LoaiPhong { get; set; }

        public double? DienTich { get; set; }

        public int? SucChua { get; set; }

        public string? KieuGiuong { get; set; }

        public string? MoTa { get; set; }

        public virtual ICollection<tblTienNghi>? TienNghis { get; set; }
    }
}
