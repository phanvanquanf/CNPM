using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotels.Models
{
    [Table("tblTienNghi")]
    public class tblTienNghi
    {
        [Key]
        public long IDTienNghi { get; set; }
        public string TienNghi { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public long? IDLoaiPhong { get; set; }

        [ForeignKey(nameof(IDLoaiPhong))]
        public virtual tblLoaiPhong? LoaiPhong { get; set; }
    }
}
