using System;

namespace siscointBKII.ModelosQ
{
    public class listar_liq_comisiones_asesor_supervisor
    {
        public string cedula { get; set; }
        public string nombres { get; set; }
        public string tipo {  get; set; }
        public string ciudad { get; set; }
        public string zona { get; set; }

        public Int32 total_ftth { get; set; }
        public double valor_total_ftth { get; set; }
        public double valor_migracion { get; set; }
        public double total_valor_duos { get; set; }
        public double total_valor_trios { get; set; }
        public double total_valor_naked {  get; set; }
        public Int32 total_movil { get; set; }
        public double valor_total_movil { get; set; }

        public double total_valor_preferencial { get; set; }    
        public double total_valor_dedicado { get; set; }
        public double total_venta_base { get; set; }
        public double total_venta_c2c { get; set; }
        public double total_venta_alta_velocidad { get; set; }

        public double subtotal_comision { get; set; }
        public double total_nunca_pago_movil { get; set; }
        public double total_otros_conceptos { get; set; }
        public double nunca_pagos { get; set; }
        public double total_comision { get; set; }
       
       
        public string tipo_esquema { get; set; } 
        public string empresa { get; set; } 
        public string periodo { get; set; }
    }
}
