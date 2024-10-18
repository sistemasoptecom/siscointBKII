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
        public DbSet<tmp_excel_bco_agrario> tmp_excel_bco_agrario { get; set; }
        public DbSet<excel_bco_agrario> excel_bco_agrario { get; set; }
        public DbSet<liq_valores_megabytes> liq_valores_megabytes { get; set; } 
        public DbSet<liq_escala_altas> liq_escala_altas { get; set; }
        public DbSet<liq_pap> liq_pap { get; set; }
        public DbSet<liq_tmp_metas> liq_tmp_metas { get; set; }
        public DbSet<liq_tipo_esquema> liq_tipo_esquema { get; set; }
        public DbSet<liq_comision_asesor> liq_comision_asesor { get; set; } 
        public DbSet<liq_periodo_comision> liq_periodo_comision { get; set; }
        public DbSet<liq_periodo_comision_v2> liq_periodo_comision_v2 { get; set; }
        public DbSet<liq_importes> liq_Importes { get; set; }
        public DbSet<liq_tmp_base_cierre> liq_tmp_base_cierre { get; set; }
        public DbSet<lote_importe> lote_importe { get; set; }
        public DbSet<liq_empaqhome> liq_empaqhome { get; set; }
        public DbSet<liq_tmp_altas_movil> liq_tmp_altas_movil { get; set; }
        public DbSet<liq_esquema_movil> liq_esquema_movil { get; set; }
        public DbSet<liq_tmp_nunca_pagos_megas> liq_tmp_nunca_pagos_megas { get; set; } 
        public DbSet<data_archivos> data_Archivos { get; set; } 
        public DbSet<liq_esquema_call> liq_esquema_call { get; set; }
        public DbSet<liq_tmp_metas_supervisor> liq_tmp_metas_supervisor { get; set; }
        public DbSet<liq_cumpliento_peso_v2> liq_cumpliento_peso_v2 { get; set; }
        public DbSet<liq_esquema_supervisores> liq_esquema_supervisores { get; set; }
        public DbSet<liq_super_esquema_acelerador> liq_super_esquema_acelerador { get; set; }
        public DbSet<liq_comision_supervisor> liq_comision_supervisor { get; set; }
        public DbSet<liq_base_cierre> liq_base_cierre { get; set; }
        public DbSet<data_valido_proceso> data_valido_proceso { get; set; }
        public DbSet<liq_tipo_empahome> liq_tipo_empahome { get; set; } 
        public DbSet<liq_pendientes_nunca_pagos> liq_pendientes_nunca_pagos { get; set; }
        public DbSet<token_autoriza> token_autoriza { get; set; }
        public DbSet<liq_tmp_otros_conceptos> liq_tmp_otros_conceptos { get; set; } 
        public DbSet<liq_consultas> liq_consultas { get; set; }
        public DbSet<liq_tmp_solicitud_np> liq_tmp_solicitud_np { get; set; }   
        public DbSet<variable> variable { get; set; }
        public DbSet<token_envia> token_envia { get; set; }
        public DbSet<liq_tmp_recuperadores> liq_tmp_recuperadores { get; set; } 
        public DbSet<liq_comision_recuperador> liq_comision_recuperador { get; set; }  
        public DbSet<liq_esquema_recuperador> liq_esquema_recuperador { get; set; }
        public DbSet<tmp_data_empleados> tmp_data_empleados { get; set; }
        public DbSet<liq_tec_conf_puntaje> liq_tec_conf_puntaje { get; set; }
        public DbSet<liq_tec_periodo_comision> liq_tec_periodo_comision { get; set; }
        public DbSet<tec_liq_periodo_comision> tec_liq_periodo_comision { get; set; }
        public DbSet<tec_liq_config_semana_comision> tec_liq_config_semana_comision { get; set; }
        public DbSet<tec_liq_config_semana_comision_detalle> tec_liq_config_semana_comision_detalle { get; set; }
        public DbSet<tec_liq_ciudades> tec_liq_ciudades { get; set; }
        public DbSet<tec_liq_config_bono_puntaje> tec_liq_config_bono_puntaje { get; set; }
        public DbSet<tec_liq_conf_penalizacion_inf> tec_liq_conf_penalizacion_inf { get; set; }

        public DbSet<tec_liq_tmp_importe_dia_tecnico> tec_liq_tmp_importe_dia_tecnico { get; set; }
        public DbSet<tec_liq_conf_items_valores> tec_liq_conf_items_valores { get; set; }
        public DbSet<tec_liq_base_cierre_semanal> tec_liq_base_cierre_semanal { get; set; }
        public DbSet<tec_liq_usuario_ciudad> tec_liq_usuario_ciudad { get; set; }
        public DbSet<temp_import_update_empleados_v1> temp_import_update_empleados_v1 { get; set; }
    }
}
