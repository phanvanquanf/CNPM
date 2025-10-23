using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotels.Models
{
    [Table("tblPhong")]
    public class tblPhong
    {
        [Key]
        public long IDPhong { get; set; }

        public string? MaSoPhong { get; set; }

        public string? ViTri { get; set; }

        public int? GiaPhong { get; set; }

        public string? TongQuan { get; set; }

        public double? Rating { get; set; }

        public int? Reviewer { get; set; }

        public int TrangThai { get; set; } = 0;

        public long? IDLoaiPhong { get; set; }

        [ForeignKey(nameof(IDLoaiPhong))]
        public virtual tblLoaiPhong? LoaiPhong { get; set; }

        // 👇 Gộp bảng ảnh
        public virtual ICollection<tblAnhPhong>? AnhPhongs { get; set; }
    }
}
