using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hotels.Areas.Admin.Models;
using Microsoft.EntityFrameworkCore;

namespace hotels.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<tblMenu> Menus { get; set; }
        public DbSet<AdminMenu> AdminMenus { get; set; }
        public DbSet<tblNhanVien> NhanViens { get; set; }
        public DbSet<tblKhachHang> KhachHangs { get; set; }
        public DbSet<tblTaiKhoan> TaiKhoans { get; set; }
        public DbSet<tblDatPhong> DatPhongs { get; set; }
        public DbSet<tblLoaiPhong> LoaiPhongs { get; set; }
        public DbSet<tblPhong> Phongs { get; set; }
        public DbSet<tblCTDatPhong> CTDatPhongs { get; set; }
        public DbSet<tblTienNghi> TienNghis { get; set; }
        public DbSet<tblAnhPhong> AnhPhongs { get; set; }
    }
}