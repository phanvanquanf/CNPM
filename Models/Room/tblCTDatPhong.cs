using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotels.Models
{
    [Table("tblCTDatPhong")]
    public class tblCTDatPhong
    {
        [Key]
        public long IDDatPhong { get; set; }

        public long? IDPhong { get; set; }

        public int? SoKhach { get; set; }

        public long? IDMaDatPhong { get; set; }

        [ForeignKey(nameof(IDPhong))]
        public virtual tblPhong? Phong { get; set; }

        [ForeignKey(nameof(IDMaDatPhong))]
        public virtual tblDatPhong? DatPhong { get; set; }
    }
}
