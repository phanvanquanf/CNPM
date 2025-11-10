using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotels.Areas.Admin.Models
{
    [Table("tblLoaiDichVu")]
    public class tblLoaiDichVu
    {
        [Key]
        public long IDLoaiDichVu { get; set; }

        public string? LoaiDichVu { get; set; }

        public string? MoTa { get; set; }

        public int TrangThai { get; set; } = 0;
        public virtual ICollection<tblDichVu>? DanhSachDichVu { get; set; }
    }
}
