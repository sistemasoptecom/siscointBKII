using Microsoft.EntityFrameworkCore;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII
{
    public class AplicationDbContext : DbContext
    {
        

        public AplicationDbContext(DbContextOptions<AplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<permisos_usuII>().HasNoKey();
            modelBuilder.Entity<permisos_usuIII>().HasKey(x => x.id_permiso);
            modelBuilder.Entity<tipo_articulo>().HasKey(x => x.id);
        }


        public DbSet<usuario> usuario { get; set; }
        public DbSet<views> views { get; set; }
        public DbSet<permisos_usuII> permisos_usuII { get; set; }
        public DbSet<permisos_usuIII> permisos_usuIII { get; set; }
        public DbSet<tipo_usuario> tipo_usuario { get; set; }
        public DbSet<empresa> empresa { get; set; }
        public DbSet<area_ccosto> area_ccosto { get; set; }
        public DbSet<empleado> empleado { get; set; }
        public DbSet<tipo_articulo> tipo_articulo { get; set; }
        public DbSet<objeto> objeto { get; set; }
        public DbSet<articulos> articulos { get; set; }
        public DbSet<depreciacion> depreciacion { get; set; }
        public DbSet<articulos_af> articulos_af { get; set; }
        public DbSet<jefes> jefes { get; set; }
    }
}
