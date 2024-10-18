using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using siscointBKII.ModelosQ;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class liqTecConfController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;
        public liqTecConfController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("tecUsuarioCiudad")]
        [Authorize]
        public IActionResult tecUsuarioCiudad(dynamic data_recibe)
        {
            string usuario = "";
            List<tec_liq_usuario_ciudad> _tec_liq_usuario_ciudad = new List<tec_liq_usuario_ciudad>();
            var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
            var datObject = JObject.Parse(dataJson);
            usuario = Convert.ToString(datObject["usuario"]);
            _tec_liq_usuario_ciudad = _context.tec_liq_usuario_ciudad.Where(x => x.usuario == usuario
                                                                            && x.estado == 1)
                                                                     .ToList();

            return Ok(_tec_liq_usuario_ciudad);
        }
        [HttpGet("getAAAA")]
        [Authorize]
        public IActionResult getAAAA()
        {
            string aaaa = "";
            variable _variable_e = _context.variable.Where(x => x.codigo_variable == "ano_presupuestal"
                                                           && x.estado == 1).FirstOrDefault();
            if (_variable_e != null)
            {
                aaaa = _variable_e.valor_variable.ToString();
            }
            return Ok(aaaa);
        }
        [HttpGet("getTopePuntos")]
        [Authorize]
        public IActionResult getTopePuntos()
        {
            string puntosTecnica = "";
            variable _variable_e = _context.variable.Where(x => x.codigo_variable == "puntos_comisiones_tecnica"
                                                           && x.estado == 1).FirstOrDefault();
            if (_variable_e != null)
            {
                puntosTecnica = _variable_e.valor_variable;
            }
            return Ok(puntosTecnica);
        }

        [HttpGet("listarPeriodosComision")]
        [Authorize]
        public IActionResult listarPeriodosComision()
        {
            List<tec_liq_periodo_comision> tec_liq_periodo_comision_ = _context.tec_liq_periodo_comision.Where(x => x.estado == 1)
                                                                                                        .OrderByDescending(x => x.id)
                                                                                                        .ToList();

            List<tec_liq_config_semana_comision> tec_liq_config_semana_comision_ = _context.tec_liq_config_semana_comision.Where(x => x.estado == 1)
                                                                                                                          .OrderByDescending(x => x.id)
                                                                                                                          .ToList();

            List<tec_liq_config_semana_comision_detalle> tec_liq_config_semana_comision_detalle_ = _context.tec_liq_config_semana_comision_detalle
                                                                                                   .Where(x => x.estado == 1)
                                                                                                   .OrderByDescending(x => x.id)
                                                                                                   .ToList();



            string json = JsonConvert.SerializeObject(new
            {
                periodo = tec_liq_periodo_comision_,
                semana = tec_liq_config_semana_comision_,
                dias = tec_liq_config_semana_comision_detalle_
            });

            return Ok(json);
        }

        [HttpPost("addPuntajeComisionTecnica")]
        [Authorize]
        public async Task<IActionResult> addPuntajeComisionTecnica(dynamic data_recibe)
        {
            string usuario = "";
            string periodo_comision = "";
            string periodo_detalle = "";
            string codigo_ciudad = "";
            string usuario_ = "";
            string mensaje = "";
            Int32 estado = 0;
            Int32 esValidoParaAdd = 0;
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                periodo_comision = Convert.ToString(datObject["periodo_comision"]);
                periodo_detalle = Convert.ToString(datObject["periodo_detalle"]);
                usuario_ = Convert.ToString(datObject["usuario"]);

                tec_liq_periodo_comision _liq_tec_periodo_comision_e_1 = JsonConvert.DeserializeObject<tec_liq_periodo_comision>(periodo_comision);
                List<listar_tec_liq_config_semana_comision> _tec_liq_config_semana_comision_ = JsonConvert.DeserializeObject<List<listar_tec_liq_config_semana_comision>>(periodo_detalle);

                if (_liq_tec_periodo_comision_e_1 != null)
                {
                    //agregamos el periodo
                    //validamos que el periodo no exista
                    tec_liq_periodo_comision _tec_liq_periodo_comision_e_v = _context.tec_liq_periodo_comision.Where(x => x.periodo == _liq_tec_periodo_comision_e_1.periodo
                                                                                                                     && x.estado == 1)
                                                                                                              .FirstOrDefault();
                    //esValidoParaAdd = _tec_liq_periodo_comision_e_v.escerradometas;

                    if (_tec_liq_periodo_comision_e_v == null)
                    {
                        esValidoParaAdd = 0;
                        tec_liq_periodo_comision _liq_tec_periodo_comision_e_2 = new tec_liq_periodo_comision();
                        _liq_tec_periodo_comision_e_2 = _liq_tec_periodo_comision_e_1;
                        _liq_tec_periodo_comision_e_2.estado = 1;
                        _liq_tec_periodo_comision_e_2.fecha_creacion = DateTime.Now;

                        await _context.tec_liq_periodo_comision.AddAsync(_liq_tec_periodo_comision_e_2);
                    }
                    else
                    {
                        esValidoParaAdd = _tec_liq_periodo_comision_e_v.escerradometas;
                    }
                }

                //aqui se sigue con el proceso
                //agregamos el detalle del periodo
                if (esValidoParaAdd == 0)
                {
                    foreach (listar_tec_liq_config_semana_comision item in _tec_liq_config_semana_comision_)
                    {
                        Int64 codigo_semana_comision = item.numero_semana;
                        string periodo_comision_aux = item.periodo;
                        codigo_ciudad = item.cod_ciudad;
                        tec_liq_config_semana_comision _tec_liq_config_semana_comision_e = new tec_liq_config_semana_comision();
                        _tec_liq_config_semana_comision_e.numero_semana = item.numero_semana;
                        _tec_liq_config_semana_comision_e.cod_ciudad = item.cod_ciudad;
                        _tec_liq_config_semana_comision_e.mm_comision = item.mm_comision;
                        _tec_liq_config_semana_comision_e.aaaa_comision = item.aaaa_comision;
                        _tec_liq_config_semana_comision_e.periodo = item.periodo;
                        _tec_liq_config_semana_comision_e.puntaje_semana = item.puntaje_semana;
                        _tec_liq_config_semana_comision_e.usuario = item.usuario;
                        _tec_liq_config_semana_comision_e.estado = 1;
                        _tec_liq_config_semana_comision_e.fecha_creacion = DateTime.Now;
                        _tec_liq_config_semana_comision_e.fecha_modificacion = DateTime.Now;
                        string[] dias_enum = item.dias.Split(";");
                        //var b = await _context.tec_liq_config_semana_comision.AddAsync(_tec_liq_config_semana_comision_e).State;
                        await _context.tec_liq_config_semana_comision.AddRangeAsync(_tec_liq_config_semana_comision_e);

                        foreach (string item_f in dias_enum)
                        {
                            if (!string.IsNullOrEmpty(item_f))
                            {
                                DateTime dia_comision = Convert.ToDateTime(item_f);
                                tec_liq_config_semana_comision_detalle _tec_liq_config_semana_comision_detalle_e = new tec_liq_config_semana_comision_detalle();
                                _tec_liq_config_semana_comision_detalle_e.cod_semana_comision = codigo_semana_comision;
                                _tec_liq_config_semana_comision_detalle_e.cod_ciudad = codigo_ciudad;
                                _tec_liq_config_semana_comision_detalle_e.periodo_comision = periodo_comision_aux;
                                _tec_liq_config_semana_comision_detalle_e.dia_comision = dia_comision;
                                _tec_liq_config_semana_comision_detalle_e.usuario = usuario_;
                                _tec_liq_config_semana_comision_detalle_e.estado = 1;
                                _tec_liq_config_semana_comision_detalle_e.fecha_creacion = DateTime.Now;
                                _tec_liq_config_semana_comision_detalle_e.fecha_modificacion = DateTime.Now;
                                //_context.tec_liq_config_semana_comision_detalle.Add(_tec_liq_config_semana_comision_detalle_e);
                                await _context.tec_liq_config_semana_comision_detalle.AddRangeAsync(_tec_liq_config_semana_comision_detalle_e);
                            }
                        }

                    }
                }
                //aqui
                if (esValidoParaAdd == 0)
                {
                    await _context.SaveChangesAsync();
                    estado = 1;
                    mensaje = "PERIODO AGREGADO O MODIFICADO CORRECTAMENTE";
                }
                else
                {
                    estado = 0;
                    mensaje = "NO SE LE PUEDE HACER MODFICACIONES";
                }


            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "addPuntajeComisionTecnica", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            string json = JsonConvert.SerializeObject(new { Estado = estado, Mensaje = mensaje });
            return Ok(json);
        }

        //metodo para traer los agrupados
        [HttpPost("listarPeriodoCiudad")]
        [Authorize]
        public IActionResult listarPeriodoCiudad(dynamic data_recibe)
        {
            var listarCiudadPeriodos = new object();
            string periodo = "";
            string usuario = "";
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObejt = JObject.Parse(data);
                periodo = Convert.ToString(datObejt["periodo"]);
                usuario = Convert.ToString(datObejt["usuario"]);

                List<tec_liq_usuario_ciudad> _tec_liq_usuario_ciudad = _context.tec_liq_usuario_ciudad.Where(x => x.usuario == usuario
                                                                                                         && x.estado == 1)
                                                                                                   .ToList();

                if(_tec_liq_usuario_ciudad.Count > 0)
                {
                    var ciudades = _tec_liq_usuario_ciudad.Select(y => y.cod_ciudad).ToList();

                    listarCiudadPeriodos = (from tec_liq_sem in _context.tec_liq_config_semana_comision
                                            join ciudad in _context.tec_liq_ciudades on tec_liq_sem.cod_ciudad equals ciudad.cod_ciudad
                                            where tec_liq_sem.periodo == periodo && ciudades.Contains(ciudad.cod_ciudad)
                                            group tec_liq_sem by new { tec_liq_sem.cod_ciudad, tec_liq_sem.periodo, ciudad.nombre } into g
                                            select new
                                            {
                                                ciudad = g.Key.cod_ciudad,
                                                nombre = g.Key.nombre,
                                                periodo = g.Key.periodo,
                                                metas = g.Sum(tec_liq_sem => tec_liq_sem.puntaje_semana)
                                            });
                }

                
            }
            catch (Exception e)
            {

            }
            return Ok(listarCiudadPeriodos);
        }

        [HttpPost("getListCiudades")]
        [Authorize]
        public IActionResult getListCiudades(dynamic data_recibe)
        {
            string usuario = "";
            List<tec_liq_ciudades> tec_liq_ciudades = new List<tec_liq_ciudades>();
            var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
            var datObejt = JObject.Parse(data);
            usuario = Convert.ToString(datObejt["usuario"]);
            //validamos que el usuario este en la relacion usuario / ciudad
            List<tec_liq_usuario_ciudad> _tec_liq_usuario_ciudad = _context.tec_liq_usuario_ciudad.Where(x => x.usuario == usuario
                                                                                                         && x.estado == 1)
                                                                                                   .ToList();
            if(_tec_liq_usuario_ciudad.Count > 0)
            {
                var ciudades = _tec_liq_usuario_ciudad.Select(y => y.cod_ciudad).ToList();
                tec_liq_ciudades = _context.tec_liq_ciudades.Where(x => x.estado == 1
                                                                   && ciudades.Contains(x.cod_ciudad)).ToList();
            }
                                                                                                           
            
            return Ok(tec_liq_ciudades);
        }

        [HttpPost("getListarConfBonoCiudad")]
        [Authorize]
        public IActionResult getListarConfBonoCiudad(dynamic data_recibe)
        {
            string resultado = "";
            string cod_ciudad = "";
            List<tec_liq_config_bono_puntaje> _liq_tec_conf_puntaje = new List<tec_liq_config_bono_puntaje>();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                cod_ciudad = Convert.ToString(datObject["cod_ciudad"]);
                _liq_tec_conf_puntaje = _context.tec_liq_config_bono_puntaje.Where(x => x.cod_ciudad == cod_ciudad
                                                                                   && x.estado == 1).ToList();
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                resultado = e.Message + " " + e.Source + " " + e.StackTrace + " " + methodName;
                General.CrearLogError(sf.GetMethod().Name, "getListarConfBonoCiudad", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(_liq_tec_conf_puntaje);
        }

        [HttpPost("enviarConfBonos")]
        [Authorize]
        public IActionResult enviarConfBonos(dynamic data_recibe)
        {
            string mensaje = "";
            string base_ = "";
            //List<tec_liq_config_bono_puntaje> _tec_liq_config_bono_puntaje = new List<tec_liq_config_bono_puntaje>();
            tec_liq_config_bono_puntaje _tec_liq_config_bono_puntaje_e_1 = new tec_liq_config_bono_puntaje();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["formConfig"]);
                //_tec_liq_config_bono_puntaje 
                _tec_liq_config_bono_puntaje_e_1 = JsonConvert.DeserializeObject<tec_liq_config_bono_puntaje>(base_);
                tec_liq_config_bono_puntaje _tec_liq_config_bono_puntaje_e_2 = new tec_liq_config_bono_puntaje();
                _tec_liq_config_bono_puntaje_e_2 = _tec_liq_config_bono_puntaje_e_1;
                _tec_liq_config_bono_puntaje_e_2.estado = 1;
                _tec_liq_config_bono_puntaje_e_2.fecha_creacion = DateTime.Now;
                _tec_liq_config_bono_puntaje_e_2.fecha_modificacion = DateTime.Now;
                _context.tec_liq_config_bono_puntaje.Add(_tec_liq_config_bono_puntaje_e_2);
                _context.SaveChanges();
                mensaje = "AGREGADO DE FORMA CORRECTA";
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "enviarConfBonos", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(mensaje);
        }

        [HttpPost("editConfBono")]
        [Authorize]
        public IActionResult editConfBono(dynamic data_recibe)
        {
            string mensaje = "";
            string usuario = "";
            Int64 id = 0;
            string rango_puntaje = "";
            double valor = 0;
            Int32 state = 0;
            tec_liq_config_bono_puntaje _tec_liq_config_bono_puntaje_e = new tec_liq_config_bono_puntaje();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                usuario = Convert.ToString(datObject["usuario"]);
                id = Convert.ToInt64(datObject["id"]);
                rango_puntaje = Convert.ToString(datObject["rango_puntaje"]);
                valor = Convert.ToDouble(datObject["valor"]);
                if (!string.IsNullOrEmpty(rango_puntaje))
                {
                    _tec_liq_config_bono_puntaje_e = _context.tec_liq_config_bono_puntaje.Where(x => x.id == id
                                                                                                && x.estado == 1)
                                                                                         .FirstOrDefault();
                    if (_tec_liq_config_bono_puntaje_e != null)
                    {
                        _tec_liq_config_bono_puntaje_e.rango_puntaje = rango_puntaje;
                        _tec_liq_config_bono_puntaje_e.valor = valor;
                        _tec_liq_config_bono_puntaje_e.usuario = usuario;
                        _tec_liq_config_bono_puntaje_e.fecha_modificacion = DateTime.Now;
                        _context.Update(_tec_liq_config_bono_puntaje_e);
                        state = 1;
                    }

                }
                if (state == 1)
                {
                    _context.SaveChanges();
                    mensaje = "ACTUALIZADO CORRECTAMENTE";
                }
                else
                {
                    mensaje = "ERROR AL ACTUALIZAR";
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "editConfBono", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(mensaje);
        }

        [HttpPost("enviarConfPenalizacion")]
        [Authorize]
        public IActionResult enviarConfPenalizacion(dynamic data_recibe)
        {
            string mensaje = "";
            string base_ = "";
            tec_liq_conf_penalizacion_inf _tec_liq_conf_penalizacion_inf_e_1 = new tec_liq_conf_penalizacion_inf();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["data"]);
                _tec_liq_conf_penalizacion_inf_e_1 = JsonConvert.DeserializeObject<tec_liq_conf_penalizacion_inf>(base_);
                if (_tec_liq_conf_penalizacion_inf_e_1 != null)
                {
                    tec_liq_conf_penalizacion_inf _tec_liq_conf_penalizacion_inf_e_2 = new tec_liq_conf_penalizacion_inf();
                    _tec_liq_conf_penalizacion_inf_e_2.porcentaje_infancia = _tec_liq_conf_penalizacion_inf_e_1.porcentaje_infancia;
                    string[] sub_homologa_infancia_1 = _tec_liq_conf_penalizacion_inf_e_1.porcentaje_infancia.Split('%');
                    string[] sub_homologa_infancia_2 = sub_homologa_infancia_1[0].Split('-');
                    string homologa_infancia = sub_homologa_infancia_2[0] + "," + sub_homologa_infancia_2[1];
                    _tec_liq_conf_penalizacion_inf_e_2.homologacion_infancia = homologa_infancia;
                    _tec_liq_conf_penalizacion_inf_e_2.porcentaje_afectacion = _tec_liq_conf_penalizacion_inf_e_1.porcentaje_afectacion;
                    string[] sub_homologa_afectacion = _tec_liq_conf_penalizacion_inf_e_1.porcentaje_afectacion.Split('%');
                    _tec_liq_conf_penalizacion_inf_e_2.homologacion_afectacion = sub_homologa_afectacion[0];
                    _tec_liq_conf_penalizacion_inf_e_2.cod_ciudad = _tec_liq_conf_penalizacion_inf_e_1.cod_ciudad;
                    _tec_liq_conf_penalizacion_inf_e_2.usuario = _tec_liq_conf_penalizacion_inf_e_1.usuario;
                    _tec_liq_conf_penalizacion_inf_e_2.estado = 1;
                    _tec_liq_conf_penalizacion_inf_e_2.fecha_creacion = DateTime.Now;
                    _tec_liq_conf_penalizacion_inf_e_2.fecha_modifacion = DateTime.Now;
                    _context.tec_liq_conf_penalizacion_inf.Add(_tec_liq_conf_penalizacion_inf_e_2);
                    int Estate = _context.SaveChanges();
                    if (Estate == 1)
                    {
                        mensaje = "CREADO DE FORMA CORRECTA";
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "enviarConfPenalizacion", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(mensaje);
        }
        [HttpPost("editConfPenalizacion")]
        [Authorize]
        public IActionResult editConfPenalizacion(dynamic data_recibe)
        {
            string mensaje = "";
            Int64 id = 0;
            string base_ = "";
            tec_liq_conf_penalizacion_inf _tec_liq_conf_penalizacion_inf_e_1 = new tec_liq_conf_penalizacion_inf();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["data"]);
                _tec_liq_conf_penalizacion_inf_e_1 = JsonConvert.DeserializeObject<tec_liq_conf_penalizacion_inf>(base_);
                if (_tec_liq_conf_penalizacion_inf_e_1 != null)
                {
                    tec_liq_conf_penalizacion_inf _tec_liq_conf_penalizacion_inf_e_2 = _context.tec_liq_conf_penalizacion_inf.Where(x => x.id == id
                                                                                                                                    && x.estado == 1)
                                                                                                                             .FirstOrDefault();
                    if(_tec_liq_conf_penalizacion_inf_e_2 != null)
                    {
                        _tec_liq_conf_penalizacion_inf_e_2.porcentaje_infancia = _tec_liq_conf_penalizacion_inf_e_1.porcentaje_infancia;
                        string[] sub_homologa_infancia_1 = _tec_liq_conf_penalizacion_inf_e_1.porcentaje_infancia.Split('%');
                        string[] sub_homologa_infancia_2 = sub_homologa_infancia_1[0].Split('-');
                        string homologa_infancia = sub_homologa_infancia_2[0] + "," + sub_homologa_infancia_2[1];
                        _tec_liq_conf_penalizacion_inf_e_2.homologacion_infancia = homologa_infancia;
                        _tec_liq_conf_penalizacion_inf_e_2.porcentaje_afectacion = _tec_liq_conf_penalizacion_inf_e_1.porcentaje_afectacion;
                        string[] sub_homologa_afectacion = _tec_liq_conf_penalizacion_inf_e_1.porcentaje_afectacion.Split('%');
                        _tec_liq_conf_penalizacion_inf_e_2.homologacion_afectacion = sub_homologa_afectacion[0];
                        _tec_liq_conf_penalizacion_inf_e_2.cod_ciudad = _tec_liq_conf_penalizacion_inf_e_1.cod_ciudad;
                        _tec_liq_conf_penalizacion_inf_e_2.usuario = _tec_liq_conf_penalizacion_inf_e_1.usuario;
                        _tec_liq_conf_penalizacion_inf_e_2.fecha_modifacion = DateTime.Now;
                        _context.tec_liq_conf_penalizacion_inf.Update(_tec_liq_conf_penalizacion_inf_e_2);
                        int Estate = _context.SaveChanges();
                        if (Estate == 1)
                        {
                            mensaje = "ACTUALIZADO DE FORMA CORRECTA";
                        }
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "editConfPenalizacion", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(mensaje);
        }

        [HttpPost("getListarConfPenalizaCiudad")]
        [Authorize]
        public IActionResult getListarConfPenalizaCiudad(dynamic data_recibe)
        {
            string cod_ciudad = "";
            List<tec_liq_conf_penalizacion_inf> tec_liq_conf_penalizacion_inf_ = new List<tec_liq_conf_penalizacion_inf>();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                cod_ciudad = Convert.ToString(datObject["cod_ciudad"]);
                tec_liq_conf_penalizacion_inf_ = _context.tec_liq_conf_penalizacion_inf.Where(x => x.cod_ciudad == cod_ciudad
                                                                                              && x.estado == 1).ToList();
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "getListarConfPenalizaCiudad", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(tec_liq_conf_penalizacion_inf_);
        }

        [HttpPost("setImporteDiaTecnico")]
        [Authorize]
        public async Task<IActionResult> setImporteDiaTecnico(dynamic data_recibe)
        {
            string resultado = "";
            string base_ = "";
            string usuario = "";
            string periodo = "";
            DateTime dia_comision = DateTime.Now;
            Int32 semana_comision = 0;
            string cod_ciudad = "";
            List<listar_tmp_importe_dia_tecnico> listar_tmp_importe_dia_tecnico_ = new List<listar_tmp_importe_dia_tecnico>();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["base"]);
                periodo = Convert.ToString(datObject["periodo"]);
                semana_comision = Convert.ToInt32(datObject["semana_comision"]);
                dia_comision = Convert.ToDateTime(datObject["dia_comision"]);
                usuario = Convert.ToString(datObject["usuario"]);
                cod_ciudad = Convert.ToString(datObject["cod_ciudad"]);
                listar_tmp_importe_dia_tecnico_ = JsonConvert.DeserializeObject<List<listar_tmp_importe_dia_tecnico>>(base_);

                if (listar_tmp_importe_dia_tecnico_.Count() > 0)
                {
                    foreach (listar_tmp_importe_dia_tecnico item in listar_tmp_importe_dia_tecnico_)
                    {
                        string nombre_tecnico_lider = "";
                        string nombre_tecnico_auxiliar = "";
                        tec_liq_tmp_importe_dia_tecnico _tec_liq_tmp_importe_dia_tecnico_e = new tec_liq_tmp_importe_dia_tecnico();
                        _tec_liq_tmp_importe_dia_tecnico_e.periodo_comision = periodo;
                        _tec_liq_tmp_importe_dia_tecnico_e.cod_semana_comision = semana_comision;
                        _tec_liq_tmp_importe_dia_tecnico_e.cod_ciudad = cod_ciudad;
                        _tec_liq_tmp_importe_dia_tecnico_e.dia_comision = dia_comision;
                        Boolean ExisteTecLider = validarExisteEmpleado(item.CEDULA_TECNICO_1);
                        Boolean ExisteTecAux = validarExisteEmpleado(item.CEDULA_TECNICO_2);
                        if (!ExisteTecLider)
                        {
                            string[] resul_nombre = item.NOMBRE_TECNICO_1.Split(" ");
                            int count = resul_nombre.Count();
                            foreach (string s in resul_nombre)
                            {
                                nombre_tecnico_lider += s + " ";
                            }
                            _tec_liq_tmp_importe_dia_tecnico_e.cedula_tec_lider = item.CEDULA_TECNICO_1;
                            string mensaje1 = await General.crearEmpleadosV2(item.CEDULA_TECNICO_1, nombre_tecnico_lider, item.CARGO_TECNICO_1, item.EMPRESA_TECNICO_1,"TECNICA" , _config.GetConnectionString("conexionDbPruebas"));
                        }
                        else
                        {
                            _tec_liq_tmp_importe_dia_tecnico_e.cedula_tec_lider = item.CEDULA_TECNICO_1;
                        }

                        if (!ExisteTecAux)
                        {
                            string[] resul_nombre_2 = item.NOMBRE_TECNICO_2.Split(" ");
                            int count2 = resul_nombre_2.Count();
                            foreach (string s in resul_nombre_2)
                            {
                                nombre_tecnico_auxiliar += s + " ";
                            }
                            _tec_liq_tmp_importe_dia_tecnico_e.cedula_tec_aux = item.CEDULA_TECNICO_2;
                            string mensaje2 = await General.crearEmpleadosV2(item.CEDULA_TECNICO_2, nombre_tecnico_auxiliar, item.CARGO_TECNICO_2, item.CARGO_TECNICO_2, "TECNICA", _config.GetConnectionString("conexionDbPruebas"));
                        }
                        else
                        {
                            _tec_liq_tmp_importe_dia_tecnico_e.cedula_tec_aux = item.CECO_TECNICO_2;
                        }
                        _tec_liq_tmp_importe_dia_tecnico_e.tipo = item.TIPO;
                        _tec_liq_tmp_importe_dia_tecnico_e.estado = 1;
                        _tec_liq_tmp_importe_dia_tecnico_e.usuario = usuario;
                        _tec_liq_tmp_importe_dia_tecnico_e.fecha_creacion = DateTime.Now;
                        _tec_liq_tmp_importe_dia_tecnico_e.fecha_modificacion = DateTime.Now;
                        await _context.tec_liq_tmp_importe_dia_tecnico.AddRangeAsync(_tec_liq_tmp_importe_dia_tecnico_e);

                    }
                }
                await _context.SaveChangesAsync();
                resultado = "CREADO DE FORMA CORRECTA";
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "setImporteDiaTecnico", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(resultado);
        }
        [HttpPost("getListarDetalleImporteTecnicos")]
        [Authorize]
        public IActionResult getListarDetalleImporteTecnicos(dynamic data_recibe)
        { 
            var datObjec = new Object();
            List<listar_tmp_importe_dia_tecnico> _listar_tmp_importe_dia_tecnico = new List<listar_tmp_importe_dia_tecnico>();
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObejt = JObject.Parse(data);
                string periodo = Convert.ToString(datObejt["periodo"]);
                string cod_ciudad = Convert.ToString(datObejt["cod_ciudad"]);
                Int32 cod_semana = Convert.ToInt32(datObejt["cod_semana"]);
                DateTime dia_semana = Convert.ToDateTime(datObejt["dia_semana"]);
                var dia_semana_convert = dia_semana.Date.ToString("yyyy-MM-dd HH:mm:ss");
                
                //aqui la consulta
                string query = "select tec.cedula_tec_lider, isnull((select concat(nombre,' ',snombre,' ',ppellido,' ',spellido) from empleado where cedula_emp = tec.cedula_tec_lider),'N/A') " +
                               " as nombre_tec_lider, tec.cedula_tec_aux,isnull((select concat(nombre,' ',snombre,' ',ppellido,' ',spellido) from empleado where cedula_emp = tec.cedula_tec_aux),'N/A') " +
                               " as nombre_tec_auxiliar from tec_liq_tmp_importe_dia_tecnico tec  where tec.periodo_comision = '"+periodo+"' and tec.cod_ciudad = '"+cod_ciudad+"' " +
                               " and tec.cod_semana_comision = '"+cod_semana+"' and tec.dia_comision = '"+ dia_semana_convert + "' ";
               using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexionDbPruebas")))
               {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                _listar_tmp_importe_dia_tecnico.Add(new listar_tmp_importe_dia_tecnico
                                {
                                    CEDULA_TECNICO_1 = sdr["cedula_tec_lider"] + "",
                                    NOMBRE_TECNICO_1 = sdr["nombre_tec_lider"] + "",
                                    CEDULA_TECNICO_2 = sdr["cedula_tec_aux"] + "",
                                    NOMBRE_TECNICO_2 = sdr["nombre_tec_auxiliar"] + ""
                                });
                            }
                        }
                        con.Close();
                    }
               }
            }
            catch (Exception e) 
            { 

            }
            datObjec = _listar_tmp_importe_dia_tecnico;
            return Ok(datObjec);
        }

        [HttpPost("setEnviarItemsValores")]
        [Authorize]
        public IActionResult setEnviarItemsValores(dynamic data_recibe)
        {
            string resultado = "";
            string usuario = "";
            string base_ = "";
            List<listar_tec_liq_tmp_valores_items> _listar_tec_liq_tmp_valores_items = new List<listar_tec_liq_tmp_valores_items>();

            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["data"]);
                usuario = Convert.ToString(datObject["usuario"]);
                _listar_tec_liq_tmp_valores_items = JsonConvert.DeserializeObject<List<listar_tec_liq_tmp_valores_items>>(base_);
                if(_listar_tec_liq_tmp_valores_items.Count > 0)
                {
                    foreach(listar_tec_liq_tmp_valores_items item in _listar_tec_liq_tmp_valores_items)
                    {
                        //validar que no se repitan el item con la ciudad
                        int existe = _context.tec_liq_conf_items_valores.Where(x => x.cod_ciudad == item.CODIGO_CIUDAD
                                                                               && x.codigo_orden == item.CODIGO_ORDEN
                                                                               && x.estado == 1)
                                                                        .Count();
                        if(existe == 0)
                        {
                            tec_liq_conf_items_valores _tec_liq_conf_items_valores_e = new tec_liq_conf_items_valores();
                            _tec_liq_conf_items_valores_e.cod_ciudad = item.CODIGO_CIUDAD;
                            _tec_liq_conf_items_valores_e.codigo_orden = item.CODIGO_ORDEN;
                            //_tec_liq_conf_items_valores_e.valor = Convert.ToDouble(item.VALOR);
                           
                            CultureInfo usCulture = new CultureInfo("en-US");
                            NumberFormatInfo dbNumberFormat = usCulture.NumberFormat;
                            decimal data_temp_1 = decimal.Parse(item.VALOR, dbNumberFormat);
                            double data_temp_2 = double.Parse(item.BONIFICACION, dbNumberFormat);
                            _tec_liq_conf_items_valores_e.valor = data_temp_1;
                            _tec_liq_conf_items_valores_e.punto_bonificacion = data_temp_2;

                            
                            
                            _tec_liq_conf_items_valores_e.productividad = Convert.ToInt32(item.PRODUCTIVIDAD);
                            _tec_liq_conf_items_valores_e.esConfiguracionDeco = false;
                            _tec_liq_conf_items_valores_e.cantidad_deco = 0;
                            _tec_liq_conf_items_valores_e.valor_punto_adicional = 0;
                            _tec_liq_conf_items_valores_e.usuario = usuario;
                            _tec_liq_conf_items_valores_e.estado = 1;
                            _tec_liq_conf_items_valores_e.fecha_creacion = DateTime.Now;
                            _tec_liq_conf_items_valores_e.fecha_modificacion = DateTime.Now;
                            _context.tec_liq_conf_items_valores.AddRange(_tec_liq_conf_items_valores_e);
                        }
                        

                    }
                    int res = _context.SaveChanges();
                    if(res > 0)
                    {
                        resultado = "ITEMS CREADOS";
                    }
                    
                    //tec_liq_conf_items_valores _tec_liq_conf_items_valores_e = new tec_liq_conf_items_valores();
                    //_tec_liq_conf_items_valores_e.codigo_orden = 
                }
                else
                {
                    resultado = "NO HAY PARA PROCESAR";
                }
            }
            catch (Exception e) 
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "setEnviarItemsValores", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(resultado);
        }

        [HttpPost("getListarItemsCiudad")]
        [Authorize]
        public IActionResult getListarItemsCiudad(dynamic data_recibe)
        {
            string cod_ciudad = "";
            List<tec_liq_conf_items_valores> _tec_liq_conf_items_valores = new List<tec_liq_conf_items_valores>();

            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                cod_ciudad = Convert.ToString(datObject["cod_ciudad"]);
                _tec_liq_conf_items_valores = _context.tec_liq_conf_items_valores.Where(x => x.cod_ciudad == cod_ciudad
                                                                                        && x.estado == 1)
                                                                                 .ToList();
                                                                                        
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "getListarItemsCiudad", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok(_tec_liq_conf_items_valores);
        }

        [HttpPost("setEnviarDataImporteBaseSemanal")]
        [Authorize]
        public IActionResult setEnviarDataImporteBaseSemanal(dynamic data_recibe)
        {
            string resultado = "";
            string usuario = "";
            string base_ = "";
            List<listar_tec_importe_base_semanal> _listar_tec_importe_base_semanal = new List<listar_tec_importe_base_semanal>();
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data_recibe);
                var datObject = JObject.Parse(dataJson);
                base_ = Convert.ToString(datObject["baseCierre"]);
                usuario = Convert.ToString(datObject["usuario"]);
                _listar_tec_importe_base_semanal = JsonConvert.DeserializeObject<List<listar_tec_importe_base_semanal>>(base_);
                if (_listar_tec_importe_base_semanal.Count > 0) 
                { 
                    //recorremos el array
                    foreach(listar_tec_importe_base_semanal item in _listar_tec_importe_base_semanal)
                    {
                        //se validan los registros repetidos 
                        string aux_periodo = item.PERIODO;
                        string aux_cod_ciudad = item.COD_CIUDAD;
                        Int32 aux_codigo_semana = Convert.ToInt32(item.SEMANA);
                        string aux_buckect_t = item.BUCKET_T1;
                        string aux_tecnologia = item.TECNOLOGIA;
                        string aux_sub_tipo_orden = item.SUBTIPO_DE_ORDEN;
                        string aux_pet_atis = item.PET_ATIS;
                        Int32 aux_cantidad_decos = Convert.ToInt32(item.CANTI_DECOS);
                        DateTime aux_fecha_comision = Convert.ToDateTime(item.FECHA_DE_COMISION);
                        string aux_obsevaciones = item.OBSERACIONES;

                        int existe = _context.tec_liq_base_cierre_semanal.Where(x => x.periodo_importe == item.PERIODO
                                                                                && x.cod_ciudad == item.COD_CIUDAD
                                                                                && x.cod_semana == aux_codigo_semana
                                                                                && x.bucket_t1 == aux_buckect_t
                                                                                && x.tecnologia == aux_tecnologia
                                                                                && x.subtipo_orden == aux_sub_tipo_orden
                                                                                && x.pet_atis == aux_pet_atis
                                                                                && x.cantidad_decos == aux_cantidad_decos
                                                                                && x.fecha_comision == aux_fecha_comision
                                                                                && x.observaciones == aux_obsevaciones)
                                                                         .Count();
                        //aqui se valida que no exista o que no este repetido
                        if(existe == 0)
                        {
                            tec_liq_base_cierre_semanal _base_cierre_semanal_e = new tec_liq_base_cierre_semanal();
                            _base_cierre_semanal_e.periodo_importe = item.PERIODO;
                            _base_cierre_semanal_e.cod_ciudad = item.COD_CIUDAD;
                            _base_cierre_semanal_e.cod_semana = aux_codigo_semana;
                            _base_cierre_semanal_e.bucket_t1 = item.BUCKET_T1;
                            _base_cierre_semanal_e.tecnologia = aux_tecnologia;
                            _base_cierre_semanal_e.subtipo_orden = aux_sub_tipo_orden;
                            _base_cierre_semanal_e.pet_atis = aux_pet_atis;
                            _base_cierre_semanal_e.cantidad_decos = aux_cantidad_decos;


                            //_tec_liq_tmp_importe_dia_tecnico_e
                            //valido la cantidad decos
                            decimal adicional_punto = 0;
                            decimal valor_punto = 0;
                            decimal valor_item = 0;
                            tec_liq_conf_items_valores _tec_liq_conf_items_e = new tec_liq_conf_items_valores();
                            _tec_liq_conf_items_e = _context.tec_liq_conf_items_valores.Where(x => x.codigo_orden == aux_sub_tipo_orden
                                                                                              && x.estado == 1)
                                                                                       .FirstOrDefault();
                            if (_tec_liq_conf_items_e != null)
                            {
                                valor_punto = (decimal)_tec_liq_conf_items_e.punto_bonificacion;
                                valor_item = _tec_liq_conf_items_e.valor;
                                //paremetrizar el valor 3
                                if (aux_cantidad_decos >= 3 && (_tec_liq_conf_items_e.esPuntoAdicional))
                                {
                                    //escogemos la cantidad y la restamos sobre los 3
                                    int result = aux_cantidad_decos - 3;
                                    if (result > 0)
                                    {
                                        adicional_punto = (decimal)(result * _tec_liq_conf_items_e.valor_punto_adicional);

                                    }
                                }
                            }

                            //fecha comision
                            _base_cierre_semanal_e.fecha_comision = aux_fecha_comision;
                            int aux_mes_comision = aux_fecha_comision.Month;
                            int aux_dia_comision = aux_fecha_comision.Day;
                            _base_cierre_semanal_e.mes_comision = aux_mes_comision;
                            _base_cierre_semanal_e.dia_comision = aux_dia_comision;
                            //validacion de los tecnicos
                            tec_liq_tmp_importe_dia_tecnico _tec_liq_tmp_importe_dia_tecnico_e = new tec_liq_tmp_importe_dia_tecnico();
                            _tec_liq_tmp_importe_dia_tecnico_e = _context.tec_liq_tmp_importe_dia_tecnico.Where(x => x.periodo_comision == aux_periodo
                                                                                                                && x.cod_ciudad == aux_cod_ciudad
                                                                                                                && x.cod_semana_comision == aux_codigo_semana
                                                                                                                && x.dia_comision.Date == aux_fecha_comision.Date
                                                                                                                && x.cedula_tec_lider == aux_buckect_t
                                                                                                                && x.estado == 1)
                                                                                                         .FirstOrDefault();
                            if(_tec_liq_tmp_importe_dia_tecnico_e != null)
                            {
                                if(!string.IsNullOrEmpty(_tec_liq_tmp_importe_dia_tecnico_e.cedula_tec_lider))
                                {
                                    _base_cierre_semanal_e.cedula_tecnico_1 = _tec_liq_tmp_importe_dia_tecnico_e.cedula_tec_lider;
                                    _base_cierre_semanal_e.puntos_tecnico_1 = valor_punto + adicional_punto;
                                }
                                if(!string.IsNullOrEmpty(_tec_liq_tmp_importe_dia_tecnico_e.cedula_tec_aux))
                                {
                                    _base_cierre_semanal_e.cedula_tecnico_2 = _tec_liq_tmp_importe_dia_tecnico_e.cedula_tec_aux;
                                    _base_cierre_semanal_e.puntos_tecnico_2 = valor_punto + adicional_punto;
                                }
                                
                                //_base_cierre_semanal_e.cedula_tecnico_2 = 
                            }
                            _base_cierre_semanal_e.facturado = valor_item;
                            _base_cierre_semanal_e.observaciones = aux_obsevaciones;
                            _base_cierre_semanal_e.estado = 1;
                            _base_cierre_semanal_e.usuario = usuario;
                            _base_cierre_semanal_e.fecha_creacion = DateTime.Now;
                            _base_cierre_semanal_e.fecha_modificacion = DateTime.Now;
                            _context.tec_liq_base_cierre_semanal.AddRange(_base_cierre_semanal_e);
                        }
                    }
                    _context.SaveChanges();
                }   
            }
            catch (Exception e) 
            {
                //string res = "";
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                //res = e.Message + " " + e.Source + " " + e.StackTrace + " " + methodName;
                General.CrearLogError(sf.GetMethod().Name, "setEnviarDataImporteBaseSemanal", e.Message, e.Source, e.StackTrace, methodName, _config.GetConnectionString("conexionDbPruebas"));
            }
            return Ok();
        }



        //METODOS
        public System.Boolean validarExisteEmpleado(string cedula)
        {
            System.Boolean Exite = false;
            int contar_empleado = _context.empleado.Where(x => x.cedula_emp == cedula).Count();
            if (contar_empleado > 0)
            {
                Exite = true;
            }
            return Exite;
        }

        //public IActionResult Index()

        //{
        //    return View();
        //}
    }
}
