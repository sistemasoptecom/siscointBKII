using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntradasController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;
        public EntradasController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("ObtenerJefes")]
        [Authorize]
        public IActionResult getJefes()
        {
            var data = new Object();
            try
            {
                data = _context.jefes.ToList();
                if(data == null)
                {
                    return null;
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "jefes", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return Ok(data);
        }
        [HttpGet("ObtnerObjeto/{id}")]
        [Authorize]
        public IActionResult getObjeto(int id)
        {
            var data = new Object();
            try
            {
                data = _context.objeto.Where(x => x.id == id).ToList();
                if(data == null)
                {
                    return null;
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "objeto", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }

            return Ok(data);
        }

        [HttpPost("AgregarEntrada")]
        [Authorize]
        public IActionResult crearEntreda(dynamic json)
        {
            string jsonResult = "";
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(json);
                //string tipoentrega = Convert.ToString(dataJson["tipoEntrega"]);
                var datObject = JObject.Parse(dataJson);
                string tipoEntregas = Convert.ToString(datObject["tipoEntrega"]);
                tipoEntrega TipoEntregas = JsonConvert.DeserializeObject<tipoEntrega>(tipoEntregas);
                if(TipoEntregas.entrega == "1" || TipoEntregas.prestamo == "5")
                {
                    //se crea una entrega
                    string Entregas = Convert.ToString(datObject["entrega"]);
                    string DetalleEntregas = Convert.ToString(datObject["detalleEntrega"]);
                    // validar que antes el objeto se encuentre disponible y no entregado
                    jsonResult = CrearEntrega(Entregas, DetalleEntregas);
                }
                else if(TipoEntregas.devolucion == "2")
                {
                    //se crea la devolucion
                    string devolucion = Convert.ToString(datObject["entrega"]);
                    string detalleDevolucion = Convert.ToString(datObject["detalleEntrega"]);
                    // validar que antes el objeto se encuentre entregado y no disponible
                    jsonResult = CrearDevolucion(devolucion, detalleDevolucion);
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "Entregas", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            
            return Ok(jsonResult);
        }
        [HttpPost("listarEntregas")]
        [Authorize]
        public IActionResult listarEntregas(dynamic tipo)
        {
            //var data = new Object();
            var data = System.Text.Json.JsonSerializer.Serialize(tipo);
            var datObejt = JObject.Parse(data);
            int tipo_articulo = Convert.ToInt32(datObejt["idTipoFormulario"]);
            int tipo_objeto = Convert.ToInt32(datObejt["idOpcion"]);
            List<reporte_objetos> rptObj = getQueryListarReportes(tipo_articulo, tipo_objeto);
            return Ok(rptObj);
            //return Ok();
        }

        [HttpGet("listarTipoReporte/{tipo_reporte_page}")]
        [Authorize]
        public IActionResult listarTipoReporte(int tipo_reporte_page)
        {
            var data = new Object();
            try
            {
                data = _context.tipo_reporte.Where(x => x.tipo_reporte_tabla == tipo_reporte_page).ToList();
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "tipo_reporte", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return Ok(data);
        }

        public List<reporte_objetos> getQueryListarReportes(int tipo_articulo, int tipo)
        {
            List<reporte_objetos> rptObj = new List<reporte_objetos>();
            string query = "";
            
            try
            {
                if(tipo != 0)
                {
                    
                    query = "select \n" +
                       "row_number() over(order by o.id desc) AS item, \n" +
                       "o.descripcion as Descripcion, \n" +
                        "case o.estado when 0 then 'ENTREGADO' when 1 then 'DISPONIBLE' when 3 then 'BAJA' when 4 then 'HURTO' when 5 then 'REPARACION' else 'NINGUNO' end as Estado, \n" +
                       "o.estado as EstadoNumber, \n" +
                       "o.af as AF, o.imei as Imei, \n" +
                       "concat(emp.nombre, ' ', emp.snombre, ' ', emp.ppellido, ' ', emp.spellido) as usuario, \n" +
                       "o.id as Id from objeto o \n" +
                       "left join detalle_entregaII de on o.imei = de.imei_inv \n" +
                       "left join entregas e on de.id_ent = e.id_ent \n" +
                       "left join empleado emp on e.ced_empl = emp.cedula_emp \n" +
                       "where tipo_articulo = @tipo_articulo and tipo = @tipo ";
                }
                else
                {
                    query = "select \n" +
                       "row_number() over(order by o.id desc) AS item, \n" +
                       "o.descripcion as Descripcion, \n" +
                        "case o.estado when 0 then 'ENTREGADO' when 1 then 'DISPONIBLE' when 3 then 'BAJA' when 4 then 'HURTO' when 5 then 'REPARACION' else 'NINGUNO' end as Estado, \n" +
                       "o.estado as EstadoNumber, \n" +
                       "o.af as AF, o.imei as Imei, \n" +
                       "concat(emp.nombre, ' ', emp.snombre, ' ', emp.ppellido, ' ', emp.spellido) as usuario, \n" +
                       "o.id as Id from objeto o \n" +
                       "left join detalle_entregaII de on o.imei = de.imei_inv \n" +
                       "left join entregas e on de.id_ent = e.id_ent \n" +
                       "left join empleado emp on e.ced_empl = emp.cedula_emp \n" +
                       "where tipo_articulo = @tipo_articulo";
                }
               
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexion")))
                {
                    using (SqlCommand cmd = new SqlCommand(query))
                    {
                        cmd.Parameters.Add(new SqlParameter("@tipo", System.Data.SqlDbType.Int));
                        cmd.Parameters.Add(new SqlParameter("@tipo_articulo", System.Data.SqlDbType.Int));
                        cmd.Parameters["@tipo"].Value = tipo;
                        cmd.Parameters["@tipo_articulo"].Value = tipo_articulo;
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                rptObj.Add(new reporte_objetos
                                {
                                    Item = Convert.ToInt64(sdr["item"]),
                                    Descripcion = sdr["Descripcion"]+"",
                                    Estado = sdr["Estado"]+"",
                                    EstadoNumber = Convert.ToInt32(sdr["EstadoNumber"]),
                                    Af = sdr["AF"]+"",
                                    Imei = sdr["Imei"]+"",
                                    usuario = sdr["usuario"]+"",
                                    Id = Convert.ToInt32(sdr["Id"])
                                });
                            }
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
                General.CrearLogError(sf.GetMethod().Name, "objeto", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return rptObj;
        }

        public string CrearEntrega(string Entregas, string detalleEntregas)
        {
            string data = "";
            string mensaje = "";
            int ResultEntregas = 0;
            int ResultDetalles = 0;
            try
            {
                entregas EntregasS = JsonConvert.DeserializeObject<entregas>(Entregas);
                _context.entregas.Add(EntregasS);
                ResultEntregas = _context.SaveChanges();
                if(ResultEntregas > 0)
                {
                    entregas entityEntrega = _context.entregas.OrderByDescending(x => x.id_ent).First();
                    int id_ent = entityEntrega.id_ent;
                    List<detalle_entregaII> detalleEntregasS = JsonConvert.DeserializeObject<List<detalle_entregaII>>(detalleEntregas);
                    foreach(detalle_entregaII det in detalleEntregasS)
                    {
                        try
                        {
                            det.id_ent = id_ent;
                            _context.detalle_entregaII.Add(det);
                           ResultDetalles =  _context.SaveChanges();
                            if(ResultDetalles > 0)
                            {
                                objeto obj = _context.objeto.Where(x => x.imei == det.imei_inv).First();
                                if(obj != null)
                                {
                                    obj.estado = 0;
                                    _context.objeto.Update(obj);
                                    _context.SaveChanges();
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            ResultDetalles = 0;
                            entregas ent = _context.entregas.Where(x => x.id_ent == id_ent).First();
                            _context.entregas.Remove(ent);
                            _context.SaveChanges();
                        }
                    }
                }

                if (ResultEntregas > 0 && ResultDetalles > 0)
                {
                    mensaje = "Registro Creado Exitosamente";
                }
                else
                {
                    mensaje = "Error al Crear el Registro";
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "entregas", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            
            data = JsonConvert.SerializeObject(new { Result = ResultEntregas, Mensaje = mensaje });
            return data;
        }

        public string CrearDevolucion(string Devolucion, string detalleDevolucion)
        {
            string data = "";
            string mensaje = "";
            int ResultDevoluciones = 0;
            int ResultDetallesDev = 0;
            try
            {
                devoluciones devoluciones = JsonConvert.DeserializeObject<devoluciones>(Devolucion);
                _context.devoluciones.Add(devoluciones);
                ResultDevoluciones = _context.SaveChanges();
                if(ResultDevoluciones > 0)
                {
                    devoluciones entityDevolucion = _context.devoluciones.OrderByDescending(x => x.id_dev).First();
                    int id_dev = entityDevolucion.id_dev;
                    List<detalle_devolucionII> detalleDevolucionesS = JsonConvert.DeserializeObject<List<detalle_devolucionII>>(detalleDevolucion);
                    foreach(detalle_devolucionII det in detalleDevolucionesS)
                    {
                        try
                        {
                            det.id_dev = id_dev;
                            _context.detalle_devolucionII.Add(det);
                            ResultDetallesDev = _context.SaveChanges();
                            if(ResultDetallesDev > 0)
                            {
                                objeto obj = _context.objeto.Where(x => x.imei == det.imei_inv).First();
                                if (obj != null)
                                {
                                    obj.estado = 1;
                                    _context.objeto.Update(obj);
                                    _context.SaveChanges();
                                }
                            }
                        }
                        catch(Exception e)
                        {
                            ResultDetallesDev = 0;
                            devoluciones dev = _context.devoluciones.Where(x => x.id_dev == id_dev).First();
                            _context.devoluciones.Remove(dev);
                            _context.SaveChanges();
                        }
                        //det.id_dev = id_dev;
                    }
                }
                if (ResultDevoluciones > 0 && ResultDetallesDev > 0)
                {
                    mensaje = "Registro Creado Exitosamente";
                }
                else
                {
                    mensaje = "Error al Crear el Registro";
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "devoluciones", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            data = JsonConvert.SerializeObject(new { Result = ResultDevoluciones, Mensaje = mensaje });
            return data;
        }
    }
}
