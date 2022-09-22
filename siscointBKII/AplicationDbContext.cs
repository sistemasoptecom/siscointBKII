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
            modelBuilder.Entity<entregas>().HasKey(x => x.id_ent);
            modelBuilder.Entity<devoluciones>().HasKey(x => x.id_dev);
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
        public DbSet<entregas> entregas { get; set; }
        public DbSet<detalle_entregaII> detalle_entregaII { get; set; }
        public DbSet<devoluciones> devoluciones { get; set; }
        public DbSet<detalle_devolucionII> detalle_devolucionII { get; set; }
        public DbSet<tipo_reporte> tipo_reporte { get; set; }
        public DbSet<proveedorII> proveedorII { get; set; }
        public DbSet<detalle_proveedor> detalle_proveedor { get; set; }
        public DbSet<compras_articulos> compras_articulos { get; set; }
        public DbSet<iva> iva { get; set; }
        public DbSet<pedidos> pedidos { get; set; }
        public DbSet<detalle_pedido> detalle_pedido { get; set; }
        public DbSet<directivos> directivos { get; set; }
    }
}
