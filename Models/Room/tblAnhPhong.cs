using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace hotels.Models
{
    [Table("tblAnhPhong")]
    public class tblAnhPhong
    {
        [Key]
        public long IDImage { get; set; }

        public long? IDPhong { get; set; }

        public string? Link { get; set; }

        public bool AnhChinh { get; set; } = false;

        [ForeignKey(nameof(IDPhong))]
        public virtual tblPhong? Phong { get; set; }
    }
}
