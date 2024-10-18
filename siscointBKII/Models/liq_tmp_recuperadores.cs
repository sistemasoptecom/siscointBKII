using System;

namespace siscointBKII.Models
{
    public class liq_tmp_recuperadores
    {
        public Int64 id {  get; set; }
        public string cedula_recuperador { get; set; }
        public string cedula_supervisor { get; set; }
        public string zona {  get; set; }
        public Int32 codigo_tipo_esquema { get; set; }  
        public string periodo { get; set; }
        public string ciudad {  get; set; }
        public Boolean tipo_caluclo_fija { get; set; }
        public Boolean tipo_calculo_movil {  get; set; }
        public double numero_meta_fija { get; set; }
        public double numero_altas_fija { get; set; }
        public double ejecucion_fijas { get; set; } 
        public string porcentaje_ejecucion_fija { get; set; }
        public double numero_vendedores {  get; set; }
        public double vendendores_cumplen {  get; set; }
        public double ejecucion_vendedores_cumplen { get; set; }
        public string porcentaje_ejecucion_vendores { get; set; }
        public double numero_meta_movil { get; set; }
        public double numero_altas_movil { get; set; }
        public double ejecucion_altas_movil { get; set; }
        public string porcentaje_ejecucion_movil { get; set; }
        public string usuario {  get; set; }
        public Int32 estado { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
    }
}
