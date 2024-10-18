using System;

namespace siscointBKII.Models
{
    public class liq_comision_recuperador
    {
        public Int64 id {  get; set; }
        public string cedula_recuperdor { get; set; }
        public string periodo { get; set; } 
        public Int32 codigo_tipo_esquema {  get; set; } 
        public double total_meta_fija { get; set; }
        public double total_altas_fija { get; set; }
        public double total_ejecucion_fija { get; set; }
        public string porcentaje_ejecucion_fija { get; set; }
        public double valor_pagar_fija { get; set; }
        public double total_vendedores {  get; set; }
        public double sum_vendedores_cumplen { get; set; }
        public double vendedores_cumplen { get; set; }
        public string porcentaje_vendores_cumplen { get; set; }
        public double valor_pagar_vendedores { get; set; }
        public double total_meta_movil {  get; set; }
        public double total_altas_movil { get; set; }
        public double total_ejecucion_movil { get; set; }
        public string porcentaje_ejecucion_movil { get; set; }
        public double valor_pagar_movil { get; set; }
        public double sub_total_comision { get; set; }
        public string aceleracion_desaceleracion { get; set; }
        public double total_comision { get; set; }
        public Int32 estado {  get; set; }
        public string usuario { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
    }
}
