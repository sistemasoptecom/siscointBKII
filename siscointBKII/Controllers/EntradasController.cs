using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntradasController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        public EntradasController(AplicationDbContext context)
        {
            _context = context;
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
                    jsonResult = CrearEntrega(Entregas, DetalleEntregas);
                }
                else if(TipoEntregas.devolucion == "2")
                {
                    //se crea la devolucion
                    string devolucion = Convert.ToString(datObject["entrega"]);
                    string detalleDevolucion = Convert.ToString(datObject["detalleEntrega"]);
                    jsonResult = CrearDevolucion(devolucion, detalleDevolucion);
                }
            }
            catch(Exception e)
            {

            }
            
            return Ok(jsonResult);
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
                    int id_ent = Convert.ToInt32(_context.entregas.OrderByDescending(x => x.id_ent).First());
                    List<detalle_entregaII> detalleEntregasS = JsonConvert.DeserializeObject<List<detalle_entregaII>>(detalleEntregas);
                    foreach(detalle_entregaII det in detalleEntregasS)
                    {
                        try
                        {
                            det.id_ent = id_ent;
                            _context.detalleEntrega.Add(det);
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
                    int id_dev = Convert.ToInt32(_context.devoluciones.OrderByDescending(x => x.id_dev).First());
                    List<detalle_devolucionII> detalleDevolucionesS = JsonConvert.DeserializeObject<List<detalle_devolucionII>>(detalleDevolucion);
                    foreach(detalle_devolucionII det in detalleDevolucionesS)
                    {
                        try
                        {
                            det.id_dev = id_dev;
                            _context.detalleDevolucion.Add(det);
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
                
            }
            data = JsonConvert.SerializeObject(new { Result = ResultDevoluciones, Mensaje = mensaje });
            return data;
        }



    }
}
