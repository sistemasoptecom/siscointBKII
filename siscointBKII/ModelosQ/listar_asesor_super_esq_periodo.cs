using System;

namespace siscointBKII.ModelosQ
{
    public class listar_asesor_super_esq_periodo
    {
        public Int64 id { get; set; }
        public string cedula_asesor { get; set; }   
        public string asesor { get; set; } 
        public Int32 meta_asesor { get; set; }  
        public Int32 meta_asesor_2 { get; set; }
        public Int32 meta_asesor_3 { get; set; }
        public Int32 cumplimiento_asesor { get; set; }
        public string tabla_cumplimiento { get; set; }  
        public Int32 nivel { get; set; }
        public decimal total_comision { get; set; } 
        public Int32 numero_cant_megas_1 { get; set; }
        public Int32 numero_cant_megas_2 { get; set; }
        public Int32 numero_cant_megas_3 { get; set; }
        public Int32 numero_cant_megas_4 { get; set; }
        public string zona { get; set; }
        public bool EsAsesorValido { get; set; } 
    }
}
