using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotels.Areas.Admin.Models
{
    [Table("tblDichVu")]
    public class tblDichVu
    {
        [Key]
        public long IDDichVu { get; set; }

        public string? DichVu { get; set; }
        public int GiaDichVu { get; set; }

        public int? GiaUuDai { get; set; }

        public string? MoTa { get; set; }

        public int? ThoiLuong { get; set; }

        public string? DacDiem { get; set; }

        public string? Image { get; set; }

        public int TrangThai { get; set; } = 0;

        public long IDLoaiDichVu { get; set; }

        [ForeignKey(nameof(IDLoaiDichVu))]
        public virtual tblLoaiDichVu? LoaiDichVu { get; set; }

        // Navigation
        public virtual ICollection<tblDatDV>? DanhSachDatDV { get; set; }
    }
}
