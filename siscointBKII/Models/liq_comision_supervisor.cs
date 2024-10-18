using System;

namespace siscointBKII.Models
{
    public class liq_comision_supervisor
    {
        public Int64 id {  get; set; }
        public Int32 mes_comision { get; set; }
        public string periodo { get; set; }
        public Int32 codigo_tipo_esquema { get; set; } 
        public string cedula_supervisor { get; set; }
        public Int32 numero_meta_ftth { get; set; }
        public Int32 numero_cumplimiento_asesor_ftth { get; set; }
        public string porcentaje_cumplimiento_asesor_ftth { get; set; }
        public double homologa_porcentaje_ftth { get; set; }
        public string peso_cumpliento_ftth { get; set; }
        public double homologa_peso_ftth { get; set; }
        public Int32 numero_meta_movil {  get; set; }
        public Int32 numero_cumpliento_asesor_movil { get; set; }
        public string porcentaje_cumplimiento_asesor_movil { get; set; }
        public double homologa_porcentaje_movil { get; set; }
        public string peso_cumplimiento_movil { get; set; }
        public double homolog_peso_movil { get; set; }
        public Int32 numero_cumplimiento_asesor_lb {  get; set; }
        public string porcentaje_cumplimiento_asesor_lb { get; set; }
        public double homologa_porcentaje_lb { get; set; }
        public Int32 numero_meta_lb { get; set; }
        public string peso_cumplimiento_lb { get; set; }
        public double homologa_peso_cumplieminto_lb { get; set; }
        public Int32 numero_cumplimiento_asesor_tv {  get; set; }
        public string porcentaje_cumplimiento_asesor_tv { get; set; }
        public double homologa_porcentaje_tv { get; set; }
        public Int32 numero_meta_tv { get; set; }
        public string peso_cumplimiento_tv { get; set; }
        public double homologa_peso_tv { get; set; }
        public double comision {  get; set; }
        public string factor_acelearion_desaceleracion { get; set; }
        public double homologa_factor_aceleracion_desaceleracion { get; set; }
        public double total_comision { get; set; }
        public Int32 estado {  get; set; }
        public string usuario { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public string total_porcentaje_cumplimiento { get; set; }
        public double total_homologa_cumplimiento { get; set; }
        public string aceleracion_desaceleracion { get; set; }
        public double numero_asesores_validos { get; set; }
        public double numero_cumplimiento_asesores { get; set; }
        public string cumplimiento_asesores { get; set; }
        public double homologa_cumpliento_asesores { get; set; }
    }
}
