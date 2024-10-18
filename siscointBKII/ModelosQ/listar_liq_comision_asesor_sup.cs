using System;

namespace siscointBKII.ModelosQ
{
    public class listar_liq_comision_asesor_sup
    {
        public Int64 id { get; set; }
        public Int32 mes_comision { get; set; }
        public Int32 codigo_tipo_escala { get; set; }
        public string cedula_asesor { get; set; }
        public string cedula_supervisor { get; set; }
        public Int32 meta_asesor { get; set; }
        public Int32 cumplimiento_asesor { get; set; }
        public string tabla_cumplimiento { get; set; }
        public Int32 nivel { get; set; }
        public Int32 numero_cant_megas_1 { get; set; }
        public Int32 numero_cant_megas_2 { get; set; }
        public Int32 numero_cant_megas_3 { get; set; }
        public Int32 numero_cant_megas_4 { get; set; }
        public Int32 numero_duos { get; set; }
        public Int32 numero_naked { get; set; }
        public double sub_total_comision { get; set; }
        public Int32 numero_migracion { get; set; }
        public double total_migracion { get; set; }
        public Int32 numero_plan_movil { get; set; }
        public double total_plan_movil { get; set; }
        public double ajustes { get; set; }
        public string descripcion_nunca_pago { get; set; }
        public double total_nunca_pago { get; set; }
        public double total_comision { get; set; }
        public Int32 estado { get; set; }
        public string usuario { get; set; }
        public DateTime fecha_creacion { get; set; }
        public DateTime fecha_modificacion { get; set; }
        public Int32 meta_asesor_2 { get; set; }
        public string periodo { get; set; }
        public double valor_mega_1 { get; set; }
        public double valor_mega_2 { get; set; }
        public double valor_mega_3 { get; set; }
        public double valor_mega_4 { get; set; }
        public double total_valor_mega_1 { get; set; }
        public double total_valor_mega_2 { get; set; }
        public double total_valor_mega_3 { get; set; }
        public double total_valor_mega_4 { get; set; }
        public Int32 numero_trios { get; set; }
        public double valor_duos { get; set; }
        public double total_valor_duos { get; set; }
        public double valor_trios { get; set; }
        public double total_valor_trios { get; set; }
        public double valor_naked { get; set; }
        public double total_valor_naked { get; set; }
        public string zona { get; set; }
        public string nombreAsesor { get; set; }
        public string nombreSupervisor { get; set; }
        public string esquema { get; set; }
    }
}
