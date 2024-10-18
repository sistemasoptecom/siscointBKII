using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Vml;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.SecurityNamespace;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using siscointBKII.Interfaces;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VolanteController : ControllerBase
    {
        private AplicationDbContext _context;
        private readonly IRSAHelper RSAHelper;
        private IConfiguration _config;
        public VolanteController(AplicationDbContext context, IRSAHelper rSAHelper, IConfiguration config)
        {
            _config = config;
            _context = context;
            RSAHelper = rSAHelper;
        }
        [AllowAnonymous]
        [HttpPost("AccesoToken")]
        public IActionResult AccesoToken(dynamic data_recibe)
        {
            string cedula = "";
            string correo = "";
            string token = "";
            Int32 EnvioExitoso = 0;
            string mensaje = "";
            string tokenJ = "";
            var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
            var datObject = JObject.Parse(dataJson);
            cedula = Convert.ToString(datObject["cedula"]);
            correo = Convert.ToString(datObject["correo"]);
            token = Convert.ToString(datObject["token"]);
            //validamos los datos encryptados
            string cedula_v2 = RSAHelper.Decrypt(cedula);
            string correo_v2 = RSAHelper.Decrypt(correo);
            string token_v2 = RSAHelper.Decrypt(token);

            Int32 Token_int = Convert.ToInt32(token_v2);

            //validamos en el Token
            token_envia _token_envia_e = _context.token_envia.Where(x => x.cedula == cedula_v2
                                                                    && x.email == correo_v2
                                                                    && x.token == Token_int
                                                                    && x.estado == 1).FirstOrDefault();
            if(_token_envia_e == null)
            {
                mensaje = "EMPLEADO NO ENCONTRADO O CODIGO INVALIDO";
            }
            else
            {
                tokenJ = GenerateJwt(cedula);
            }
            
            if(!string.IsNullOrEmpty(tokenJ))
            {
                DesactivarToken(cedula_v2, correo_v2, Token_int);
                EnvioExitoso = 1;
            }
            string json = JsonConvert.SerializeObject(new { Resultado = EnvioExitoso, Mensaje = mensaje, Token = tokenJ });
            return Ok(json);
        }

        [HttpPost("ListarPeriodos")]
        //[Authorize(AuthenticationSchemes = "client2")]
        public IActionResult ListarPeriodos(dynamic data_recibe)
        {
            string cedula_empleado = "";
            string cargo = "";
            Int32 tipo_cargo = 0;
            //List<liq_comision_asesor> _liq_comision_asesor = new List<liq_comision_asesor>();
            var result_comision = new Object();
            try
            {
                //validar el cargo del empleado el cual trae los datos si es un asesor, supervisor, etc..
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                cedula_empleado = Convert.ToString(datObject["cedula_asesor"]);
                cargo = Convert.ToString(datObject["cargo"]);
                if(!string.IsNullOrEmpty (cargo)) 
                {
                    if (cargo.Contains("VENDEDOR"))
                    {
                        tipo_cargo = 1;
                    }else if (cargo.Contains("SUPERVISOR"))
                    {
                        tipo_cargo = 2;
                    }
                }
                //validamos en el tabla comision asesor
               
                switch (tipo_cargo)
                {
                    case 1:
                        result_comision = (from em in _context.empleado
                                            join ca in _context.liq_tmp_metas on em.cedula_emp equals ca.cedula_asesor
                                            join dc in _context.liq_comision_asesor on new { P = ca.periodo_importe, C = ca.cedula_asesor }
                                                                                    equals new { P = dc.periodo, C = dc.cedula_asesor }
                                                                                    //join emp in _context.empresa on em.empresa equals 
                                            where (em.cedula_emp == cedula_empleado)
                                            select new
                                            {
                                                id = dc.id,
                                                mes_comision = dc.mes_comision,
                                                codigo_tipo_escala = dc.codigo_tipo_escala,
                                                cedula_asesor = dc.cedula_asesor,
                                                cedula_supervisor = dc.cedula_supervisor,
                                                meta_asesor = dc.meta_asesor,
                                                cumplimiento_asesor = dc.cumplimiento_asesor,
                                                tabla_cumplimiento = dc.tabla_cumplimiento,
                                                nivel = dc.nivel,
                                                numero_cant_megas_1 = dc.numero_cant_megas_1,
                                                numero_cant_megas_2 = dc.numero_cant_megas_2,
                                                numero_cant_megas_3 = dc.numero_cant_megas_3,
                                                numero_cant_megas_4 = dc.numero_cant_megas_4,
                                                numero_duos = dc.numero_duos,
                                                numero_naked = dc.numero_naked,
                                                sub_total_comision = dc.sub_total_comision,
                                                numero_migracion = dc.numero_migracion,
                                                total_migracion = dc.total_migracion,
                                                numero_plan_movil = dc.numero_plan_movil,
                                                total_plan_movil = dc.total_plan_movil,
                                                ajustes = dc.ajustes,
                                                descripcion_nunca_pago = dc.descripcion_nunca_pago,
                                                total_nunca_pago = dc.total_nunca_pago,
                                                total_comision = dc.total_comision,
                                                estado = dc.estado,
                                                usuario = dc.usuario,
                                                fecha_creacion = dc.fecha_creacion,
                                                fecha_modificacion = dc.fecha_modificacion,
                                                meta_asesor_2 = dc.meta_asesor_2,
                                                periodo = dc.periodo,
                                                valor_mega_1 = dc.valor_mega_1,
                                                valor_mega_2 = dc.valor_mega_2,
                                                valor_mega_3 = dc.valor_mega_3,
                                                valor_mega_4 = dc.valor_mega_4,
                                                total_valor_mega_1 = dc.total_valor_mega_1,
                                                total_valor_mega_2 = dc.total_valor_mega_2,
                                                total_valor_mega_3 = dc.total_valor_mega_3,
                                                total_valor_mega_4 = dc.total_valor_mega_4,
                                                numero_trios = dc.numero_trios,
                                                valor_duos = dc.valor_duos,
                                                total_valor_duos = dc.total_valor_duos,
                                                valor_trios = dc.valor_trios,
                                                total_valor_trios = dc.total_valor_trios,
                                                valor_naked = dc.valor_naked,
                                                total_valor_naked = dc.total_valor_naked,
                                                zona = dc.zona,
                                                valor_plan_movil = dc.valor_plan_movil,
                                                valor_migracion = dc.valor_migracion,
                                                numero_cant_megas_5 = dc.numero_cant_megas_5,
                                                valor_mega_5 = dc.valor_mega_5,
                                                total_valor_mega_5 = dc.total_valor_mega_5,
                                                descripcion_otros_ajustes = dc.descripcion_otros_ajustes,
                                                otros_ajustes = dc.otros_ajustes,
                                                numero_cant_preferencial = dc.numero_cant_preferencial,
                                                valor_preferencial = dc.valor_preferencial,
                                                total_valor_preferencial = dc.total_valor_preferencial,
                                                numero_cant_decicado = dc.numero_cant_decicado,
                                                valor_dedicado = dc.valor_dedicado,
                                                total_valor_dedicado = dc.total_valor_dedicado,
                                                nombre_mega_1 = dc.nombre_mega_1,
                                                nombre_mega_2 = dc.nombre_mega_2,
                                                nombre_mega_3 = dc.nombre_mega_3,
                                                nombre_mega_4 = dc.nombre_mega_4,
                                                nombre_mega_5 = dc.nombre_mega_5,
                                                numero_venta_cobre_lb = dc.numero_venta_cobre_lb,
                                                valor_venta_cobre_lb = dc.valor_venta_cobre_lb,
                                                total_venta_cobre_lb = dc.total_venta_cobre_lb,
                                                numero_venta_fibra_lb = dc.numero_venta_fibra_lb,
                                                valor_venta_fibra_lb = dc.valor_venta_fibra_lb,
                                                total_venta_fibra_lb = dc.total_venta_fibra_lb,
                                                numero_venta_cobre_tv = dc.numero_venta_cobre_tv,
                                                valor_venta_cobre_tv = dc.valor_venta_cobre_tv,
                                                total_venta_cobre_tv = dc.total_venta_cobre_tv,
                                                numero_venta_fibra_tv = dc.numero_venta_fibra_tv,
                                                valor_venta_fibra_tv = dc.valor_venta_fibra_tv,
                                                total_venta_fibra_tv = dc.total_venta_fibra_tv,
                                                numero_cant_mega_6 = dc.numero_cant_mega_6,
                                                valor_mega_6 = dc.valor_mega_6,
                                                total_valor_mega_6 = dc.total_valor_mega_6,
                                                nombre_mega_6 = dc.nombre_mega_6,
                                                asesor_cumple = dc.asesor_cumple,
                                                meta_asesor_3 = dc.meta_asesor_3,
                                                numero_venta_base = dc.numero_venta_base,
                                                valor_venta_base = dc.valor_venta_base,
                                                total_venta_base = dc.total_venta_base,
                                                numero_venta_c2c = dc.numero_venta_c2c,
                                                valor_venta_c2c = dc.valor_venta_c2c,
                                                total_venta_c2c = dc.total_venta_c2c,
                                                numero_venta_alta_velocidad = dc.numero_venta_alta_velocidad,
                                                valor_venta_alta_velocidad = dc.valor_venta_alta_velocidad,
                                                total_venta_alta_velocidad = dc.total_venta_alta_velocidad,
                                                EsAsesorValido = dc.EsAsesorValido,
                                                total_nunca_pago_movil = dc.total_nunca_pago_movil,
                                                descripcion_nunca_pago_movil = dc.descripcion_nunca_pago_movil,
                                                total_otros_conceptos = dc.total_otros_conceptos,
                                                descripcion_otros_conceptos = dc.descripcion_otros_conceptos,
                                                nombre_asesor = General.getNombreCompletoEmpleado(dc.cedula_asesor, _config.GetConnectionString("conexionDbPruebas")),
                                                nombre_supervisor = General.getNombreCompletoEmpleado(dc.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas")),
                                                nombre_esquema = General.getNombreTipoEsquema(dc.codigo_tipo_escala, _config.GetConnectionString("conexionDbPruebas")),
                                                empresa_asesor = ca.empresa_contratante,
                                                ciudad_asesor = ca.ciudad,
                                                cargo_aseosr = em.cargo,

                                            });
                        break;
                    case 2:
                        result_comision = (from em in _context.empleado
                                           join ca in _context.liq_tmp_metas_supervisor on em.cedula_emp equals ca.cedula_supervisor
                                           join dc in _context.liq_comision_supervisor on new { P = ca.periodo_importe, C = ca.cedula_supervisor }
                                                                                       equals new { P = dc.periodo, C = dc.cedula_supervisor }
                                           where (em.cedula_emp == cedula_empleado)
                                           select new
                                           {
                                               id = dc.id,
                                               cedula_supervisor = dc.cedula_supervisor,
                                               mes_comision = dc.mes_comision,
                                               periodo = dc.periodo,
                                               codigo_tipo_esquema = dc.codigo_tipo_esquema,
                                               numero_meta_ftth = dc.numero_meta_ftth,
                                               numero_cumplimiento_asesor_ftth = dc.numero_cumplimiento_asesor_ftth,
                                               porcentaje_cumplimiento_asesor_ftth = dc.porcentaje_cumplimiento_asesor_ftth,
                                               homologa_porcentaje_ftth = dc.homologa_porcentaje_ftth,
                                               peso_cumpliento_ftth = dc.peso_cumpliento_ftth,
                                               homologa_peso_ftth = dc.homologa_peso_ftth,
                                               numero_meta_movil = dc.numero_meta_movil,
                                               numero_cumpliento_asesor_movil = dc.numero_cumpliento_asesor_movil,
                                               porcentaje_cumplimiento_asesor_movil = dc.porcentaje_cumplimiento_asesor_movil,
                                               homologa_porcentaje_movil = dc.homologa_porcentaje_movil,
                                               peso_cumplimiento_movil = dc.peso_cumplimiento_movil,
                                               homolog_peso_movil = dc.homolog_peso_movil,
                                               numero_cumplimiento_asesor_lb = dc.numero_cumplimiento_asesor_lb,
                                               porcentaje_cumplimiento_asesor_lb = dc.porcentaje_cumplimiento_asesor_lb,
                                               homologa_porcentaje_lb = dc.homologa_porcentaje_lb,
                                               numero_meta_lb = dc.numero_meta_lb,
                                               peso_cumplimiento_lb = dc.peso_cumplimiento_lb,
                                               homologa_peso_cumplieminto_lb = dc.homologa_peso_cumplieminto_lb,
                                               numero_cumplimiento_asesor_tv = dc.numero_cumplimiento_asesor_tv,
                                               porcentaje_cumplimiento_asesor_tv = dc.porcentaje_cumplimiento_asesor_tv,
                                               homologa_porcentaje_tv = dc.homologa_porcentaje_tv,
                                               numero_meta_tv = dc.numero_meta_tv,
                                               peso_cumplimiento_tv = dc.peso_cumplimiento_tv,
                                               homologa_peso_tv = dc.homologa_peso_tv,
                                               comision = dc.comision,
                                               factor_acelearion_desaceleracion = dc.factor_acelearion_desaceleracion,
                                               homologa_factor_aceleracion_desaceleracion = dc.homologa_factor_aceleracion_desaceleracion,
                                               total_comision = dc.total_comision,
                                               total_porcentaje_cumplimiento = dc.total_porcentaje_cumplimiento,
                                               total_homologa_cumplimiento = dc.total_homologa_cumplimiento,
                                               aceleracion_desaceleracion = dc.aceleracion_desaceleracion,
                                               numero_asesores_validos = dc.numero_asesores_validos,
                                               numero_cumplimiento_asesores = dc.numero_cumplimiento_asesores,
                                               cumplimiento_asesores = dc.cumplimiento_asesores,
                                               homologa_cumpliento_asesores = dc.homologa_cumpliento_asesores,
                                               nombre_supervisor = General.getNombreCompletoEmpleado(dc.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas")),
                                               nombre_esquema = General.getNombreTipoEsquema(dc.codigo_tipo_esquema, _config.GetConnectionString("conexionDbPruebas")),
                                               empresa_supervisor = em.empresa,
                                               zona = ca.zona

                                           });
                        break;
                }
                
                
            }
            catch (Exception e)
            {

            }
            return Ok(result_comision);
        }

        [HttpGet("getEmpresa")]
        public IActionResult getEmpresa()
        {
            List<empresa> empresa_e = _context.empresa.Where(x => x.estado ==1).ToList();
            return Ok(empresa_e);
        }
        [HttpPost("dataDetailsAsesor")]
        public IActionResult dataDetailsAsesor(dynamic data_recibe)
        {
            //List<liq_tmp_base_cierre> _liq_tmp_base_cierre = new List<liq_tmp_base_cierre>();
            //List<liq_tmp_altas_movil> _liq_tmp_altas_movil = new List<liq_tmp_altas_movil>();
            //List<liq_tmp_nunca_pagos_megas> _liq_tmp_nunca_pagos_megas = new List<liq_tmp_nunca_pagos_megas>();
            //List<liq_tmp_otros_conceptos> _liq_tmp_otros_conceptos = new List<liq_tmp_otros_conceptos>();
            var _liq_tmp_base_cierre = new Object();
            var _liq_tmp_altas_movil = new Object();
            var _liq_tmp_nunca_pagos_megas = new Object();
            var _liq_tmp_otros_conceptos = new Object();
            Int64 id = 0;
            Int32 cargo = 0;
            string cedula_asesor = "";
            string cedula_supervisor = "";
            string periodo = "";
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObejt = JObject.Parse(data);
                id = Convert.ToInt64(datObejt["id"]);
                cargo = Convert.ToInt32(datObejt["cargo"]);
                switch (cargo)
                {
                    case 1:
                        var _liq_comision_asesor_e = _context.liq_comision_asesor.Select(x => new { x.id, x.cedula_asesor, x.periodo })
                                                                                         .Where(y => y.id == id)
                                                                                         .FirstOrDefault();
                        if (_liq_comision_asesor_e != null)
                        {
                            cedula_asesor = _liq_comision_asesor_e.cedula_asesor;
                            periodo = _liq_comision_asesor_e.periodo;
                            _liq_tmp_base_cierre = _context.liq_tmp_base_cierre.Where(x => x.cedula_asesor == cedula_asesor
                                                                                      && x.periodo == periodo
                                                                                      && x.estado == 1)
                                                                               .Select(x => new
                                                                               {
                                                                                   cedula_asesor = x.cedula_asesor,
                                                                                   nombre_asesor = General.getNombreCompletoEmpleado(x.cedula_asesor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                   cedula_supervisor = x.cedula_supervisor,
                                                                                   nombre_supervisor = General.getNombreCompletoEmpleado(x.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                   producto = x.producto,
                                                                                   mes_seg = x.mes_seg,
                                                                                   unidad = x.unidad,
                                                                                   cod_peticion = x.cod_peticion,
                                                                                   velocidad = x.velocidad,
                                                                                   velocidad_ftth = x.velocidad_ftth_rango,
                                                                                   velocidad_pymes_rango = x.velocidad_pymes_rango,
                                                                                   empaqhomo = x.empaqhomo,
                                                                                   num_doc_cliente = x.num_doc_cliente,
                                                                                   observacion = x.observacion,
                                                                                   cod_tipo_esquema = x.cod_tipo_esquema,
                                                                                   nombre_esquema = General.getNombreTipoEsquema(x.cod_tipo_esquema, _config.GetConnectionString("conexionDbPruebas")),
                                                                                   migracion_otro = x.migracion_otro,
                                                                                   periodo = x.periodo,
                                                                                   tipo_campana = x.tipo_campana,
                                                                                   fecha_creacion = x.fecha_creacion

                                                                               })
                                                                               .ToList();
                            _liq_tmp_altas_movil = _context.liq_tmp_altas_movil.Where(x => x.cedula_asesor == cedula_asesor
                                                                                      && x.periodo == periodo
                                                                                      && x.estado == 1)
                                                                               .Select(x => new
                                                                               {
                                                                                   cedula_asesor = x.cedula_asesor,
                                                                                   nombre_asesor = General.getNombreCompletoEmpleado(x.cedula_asesor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                   cedula_supervisor = x.cedula_supervisor,
                                                                                   nombre_supervisor = General.getNombreCompletoEmpleado(x.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                   unidad = x.unidad,
                                                                                   valor = x.valor,
                                                                                   periodo = x.periodo,
                                                                                   observacion = x.observacion,
                                                                                   imei = x.imei,
                                                                                   celular = x.celular,
                                                                                   codigo_tipo_escala = x.codigo_tipo_escala,
                                                                                   nombre_esquema = General.getNombreTipoEsquema(x.codigo_tipo_escala, _config.GetConnectionString("conexionDbPruebas")),
                                                                                   fecha_creacion = x.fecha_creacion,
                                                                               })
                                                                               .ToList();
                            _liq_tmp_nunca_pagos_megas = _context.liq_tmp_nunca_pagos_megas.Where(x => x.cedula_asesor == cedula_asesor
                                                                                                  && x.periodo == periodo
                                                                                                  && x.estado == 1)
                                                                                           .Select(x => new
                                                                                           {
                                                                                               cedula_asesor = x.cedula_asesor,
                                                                                               nombre_asesor = General.getNombreCompletoEmpleado(x.cedula_asesor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                               cedula_supervisor = x.cedula_supervisor,
                                                                                               nombre_supervisor = General.getNombreCompletoEmpleado(x.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                               zona = x.zona,
                                                                                               periodo = x.periodo,
                                                                                               cod_tipo_esquema = x.cod_tipo_esquema,
                                                                                               nombre_esquema = General.getNombreTipoEsquema(x.cod_tipo_esquema, _config.GetConnectionString("conexionDbPruebas")),
                                                                                               observacion = x.observacion,
                                                                                               total = x.total,
                                                                                               tipo_operacion = x.tipo_operacion,
                                                                                               fecha_creacion = x.fecha_creacion,
                                                                                           })
                                                                                           .ToList();
                            _liq_tmp_otros_conceptos = _context.liq_tmp_otros_conceptos.Where(x => x.cedula_asesor == cedula_asesor
                                                                                              && x.periodo == periodo
                                                                                              && x.estado == 1)
                                                                                       .Select(x => new
                                                                                       {
                                                                                           cedula_asesor = x.cedula_asesor,
                                                                                           nombre_asesor = General.getNombreCompletoEmpleado(x.cedula_asesor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                           cedula_supervisor = x.cedula_supervisor,
                                                                                           nombre_supervisor = General.getNombreCompletoEmpleado(x.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                           zona = x.zona,
                                                                                           periodo = x.periodo,
                                                                                           cod_tipo_esquema = x.cod_tipo_esquema,
                                                                                           nombre_esquema = General.getNombreTipoEsquema(x.cod_tipo_esquema, _config.GetConnectionString("conexionDbPruebas")),
                                                                                           descripcion = x.descripcion,
                                                                                           total = x.total,
                                                                                           fecha_creacion = x.fecha_creacion
                                                                                       })
                                                                                       .ToList();
                        }
                        break;
                    case 2:
                        var comision_supervisor_e = _context.liq_comision_supervisor.Select(x => new { x.id, x.cedula_supervisor, x.periodo })
                                                                                    .Where(x => x.id == id)
                                                                                    .FirstOrDefault();
                        if(comision_supervisor_e != null)
                        {
                            cedula_supervisor = comision_supervisor_e.cedula_supervisor;
                            periodo = comision_supervisor_e.periodo;
                            _liq_tmp_base_cierre = _context.liq_tmp_base_cierre.Where(x => x.cedula_supervisor == cedula_supervisor
                                                                                      && x.periodo == periodo
                                                                                      && x.estado == 1)
                                                                               .Select(x => new
                                                                               {
                                                                                   cedula_asesor = x.cedula_asesor,
                                                                                   nombre_asesor = General.getNombreCompletoEmpleado(x.cedula_asesor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                   cedula_supervisor = x.cedula_supervisor,
                                                                                   nombre_supervisor = General.getNombreCompletoEmpleado(x.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                   producto = x.producto,
                                                                                   mes_seg = x.mes_seg,
                                                                                   unidad = x.unidad,
                                                                                   cod_peticion = x.cod_peticion,
                                                                                   velocidad = x.velocidad,
                                                                                   velocidad_ftth = x.velocidad_ftth_rango,
                                                                                   velocidad_pymes_rango = x.velocidad_pymes_rango,
                                                                                   empaqhomo = x.empaqhomo,
                                                                                   num_doc_cliente = x.num_doc_cliente,
                                                                                   observacion = x.observacion,
                                                                                   cod_tipo_esquema = x.cod_tipo_esquema,
                                                                                   nombre_esquema = General.getNombreTipoEsquema(x.cod_tipo_esquema, _config.GetConnectionString("conexionDbPruebas")),
                                                                                   migracion_otro = x.migracion_otro,
                                                                                   periodo = x.periodo,
                                                                                   tipo_campana = x.tipo_campana,
                                                                                   fecha_creacion = x.fecha_creacion

                                                                               })
                                                                               .ToList();
                            _liq_tmp_altas_movil = _context.liq_tmp_altas_movil.Where(x => x.cedula_supervisor == cedula_supervisor
                                                                                     && x.periodo == periodo
                                                                                     && x.estado == 1)
                                                                              .Select(x => new
                                                                              {
                                                                                  cedula_asesor = x.cedula_asesor,
                                                                                  nombre_asesor = General.getNombreCompletoEmpleado(x.cedula_asesor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                  cedula_supervisor = x.cedula_supervisor,
                                                                                  nombre_supervisor = General.getNombreCompletoEmpleado(x.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                  unidad = x.unidad,
                                                                                  valor = x.valor,
                                                                                  periodo = x.periodo,
                                                                                  observacion = x.observacion,
                                                                                  imei = x.imei,
                                                                                  celular = x.celular,
                                                                                  codigo_tipo_escala = x.codigo_tipo_escala,
                                                                                  nombre_esquema = General.getNombreTipoEsquema(x.codigo_tipo_escala, _config.GetConnectionString("conexionDbPruebas")),
                                                                                  fecha_creacion = x.fecha_creacion,
                                                                              })
                                                                              .ToList();
                            _liq_tmp_nunca_pagos_megas = _context.liq_tmp_nunca_pagos_megas.Where(x => x.cedula_supervisor == cedula_supervisor
                                                                                                  && x.periodo == periodo
                                                                                                  && x.estado == 1)
                                                                                           .Select(x => new
                                                                                           {
                                                                                               cedula_asesor = x.cedula_asesor,
                                                                                               nombre_asesor = General.getNombreCompletoEmpleado(x.cedula_asesor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                               cedula_supervisor = x.cedula_supervisor,
                                                                                               nombre_supervisor = General.getNombreCompletoEmpleado(x.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                               zona = x.zona,
                                                                                               periodo = x.periodo,
                                                                                               cod_tipo_esquema = x.cod_tipo_esquema,
                                                                                               nombre_esquema = General.getNombreTipoEsquema(x.cod_tipo_esquema, _config.GetConnectionString("conexionDbPruebas")),
                                                                                               observacion = x.observacion,
                                                                                               total = x.total,
                                                                                               tipo_operacion = x.tipo_operacion,
                                                                                               fecha_creacion = x.fecha_creacion,
                                                                                           })
                                                                                           .ToList();
                            _liq_tmp_otros_conceptos = _context.liq_tmp_otros_conceptos.Where(x => x.cedula_supervisor == cedula_supervisor
                                                                                              && x.periodo == periodo
                                                                                              && x.estado == 1)
                                                                                       .Select(x => new
                                                                                       {
                                                                                           cedula_asesor = x.cedula_asesor,
                                                                                           nombre_asesor = General.getNombreCompletoEmpleado(x.cedula_asesor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                           cedula_supervisor = x.cedula_supervisor,
                                                                                           nombre_supervisor = General.getNombreCompletoEmpleado(x.cedula_supervisor, _config.GetConnectionString("conexionDbPruebas")),
                                                                                           zona = x.zona,
                                                                                           periodo = x.periodo,
                                                                                           cod_tipo_esquema = x.cod_tipo_esquema,
                                                                                           nombre_esquema = General.getNombreTipoEsquema(x.cod_tipo_esquema, _config.GetConnectionString("conexionDbPruebas")),
                                                                                           descripcion = x.descripcion,
                                                                                           total = x.total,
                                                                                           fecha_creacion = x.fecha_creacion
                                                                                       })
                                                                                       .ToList();
                        }

                            
                        break;
                }
                
            }
            catch (Exception e) 
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "dataDetailsAsesor", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(new { DataMegas = _liq_tmp_base_cierre, 
                                                            DataMovil = _liq_tmp_altas_movil, 
                                                            DataNp = _liq_tmp_nunca_pagos_megas,
                                                            DataOc = _liq_tmp_otros_conceptos
                                                          });
            return Ok(json);
        }

        //validamos si el empleado es un asesor del area comercial
        //[HttpPost("getListarPeriodosComision")]


        public void DesactivarToken(string cedula, string correo, Int32 token)
        {
            
            token_envia _token_envia_e = _context.token_envia.Where(x => x.cedula == cedula
                                                                    && x.email == correo
                                                                    && x.token == token
                                                                    && x.estado == 1).FirstOrDefault();
            if(_token_envia_e != null)
            {
                _token_envia_e.estado = 0;
                _context.Update(_token_envia_e);
                _context.SaveChanges();
            }
            
        }

        //generamos el Token jwt
        public string GenerateJwt(string cedula)
        {
            string nombres = "";
            string cargo = "";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key2"]));
            var crediales = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            empleado _empleado_e = _context.empleado.Where(x => x.cedula_emp == cedula).FirstOrDefault();
            if(_empleado_e != null )
            {
                nombres = _empleado_e.nombre + " " + _empleado_e.snombre + " " + _empleado_e.ppellido + " " + _empleado_e.spellido;
                cargo = _empleado_e.cargo;
            }
            var claims = new[]
            {
                new Claim(ClaimTypes.SerialNumber, cedula),
                new Claim(ClaimTypes.Name, nombres),
                new Claim(ClaimTypes.NameIdentifier, cargo)
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                                             _config["Jwt:Audience"],
                                             claims,
                                             expires: DateTime.Now.AddMinutes(180),
                                             signingCredentials: crediales);

            return new JwtSecurityTokenHandler().WriteToken(token); 
                
        }

        //public string get_nombre_tipo_esquema(int codigoTipoEsquema)
        //{
        //    string nombreTipoEsquema = "";
        //    liq_tipo_esquema _liq_tipo_esquema_e = _context.liq_tipo_esquema.Where(x => x.codigo_valor == codigoTipoEsquema).FirstOrDefault();
        //    if (_liq_tipo_esquema_e != null)
        //    {
        //        nombreTipoEsquema = _liq_tipo_esquema_e.esquema;
        //    }

        //    return nombreTipoEsquema;
        //}
    }
}
