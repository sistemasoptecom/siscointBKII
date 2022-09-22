using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticuloController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;

        public ArticuloController(AplicationDbContext contex, IConfiguration config)
        {
            _context = contex;
            _config = config;
        }

        [HttpGet("tipoArticulo")]
        [Authorize]
        public IActionResult tipoArticulo()
        {
            var data = _context.tipo_articulo.ToList();
            if (data == null)
                return null;

            return Ok(data);
        }

        [HttpGet("validarImei/{imei}")]
        [Authorize]
        public IActionResult validarImei(string imei)
        {
            Boolean ExisteObjeto = false;
            string mensaje = "";
            try
            {
                var data = _context.objeto.FirstOrDefault(x => x.imei == imei);
                if(data != null)
                {
                    ExisteObjeto = true;
                    mensaje = "Imei Existente";
                }

            }
            catch (Exception e)
            {
                //log de eventos
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "Objeto", e.Message, _config.GetConnectionString("conexion"));
            }
            string json = JsonConvert.SerializeObject(new { Result = ExisteObjeto, Mensaje = mensaje });
            return Ok(json);
        }

        [HttpGet("validarArticulo/{id}")]
        [Authorize]
        public IActionResult validarArticuloXId(string id)
        {
            var data = new Object();
            try
            {
                data = _context.articulos.FirstOrDefault(x => x.id == Convert.ToInt32(id));

            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "articulos", e.Message, _config.GetConnectionString("conexion"));
            }
            return Ok(data);
        }
        [HttpPost("validarArticuloDevolutivo")]
        [Authorize]
        public IActionResult validarArticuloDevolutivo([FromBody] articulos Articulo)
        {
            var data = new Object();
            try
            {
                data = _context.articulos.Where(x => x.codigo == Articulo.codigo).First();
                if(data == null)
                {
                    return null;
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "articulos", e.Message, _config.GetConnectionString("conexion"));
            }
            return Ok(data);
        }

        [HttpGet("ValidarArticuloFijo/{id}")]
        [Authorize]
        public IActionResult validarArticuloFijoXId(string id)
        {
            var data = getArticuloFijo(id);
            if(data == null)
            {
                return null;
            }
            return Ok(data);
        }

        [HttpPost("AgregarObjeto/{idDepreciacion}")]
        [Authorize]
        public IActionResult AgregarObjeto(int idDepreciacion, dynamic data)
        {
            int result = 0;
            int result2 = 0;
            string mensaje = "";
            try
            {
                string dat = System.Text.Json.JsonSerializer.Serialize(data);
                objeto Objeto = JsonConvert.DeserializeObject<objeto>(dat);
                if(Objeto.tipo == 0)
                {
                    int tipo = TipoArticulo(Convert.ToInt32(Objeto.cod_articulo));
                    Objeto.tipo = tipo;
                }
                _context.objeto.Add(Objeto);
                result = _context.SaveChanges();
                if(result > 0)
                {
                    if(Objeto.tipo_articulo == 2)
                    {
                        depreciacion datDepreciacion = _context.depreciacion.Where(x => x.id == idDepreciacion).First();
                        if(datDepreciacion != null)
                        {
                            try
                            {
                                datDepreciacion.inventario = 1;
                                datDepreciacion.placa_af = Objeto.af;
                                datDepreciacion.cuota = Convert.ToInt32(Objeto.valor) / datDepreciacion.v_util;
                                _context.depreciacion.Update(datDepreciacion);
                                result2 = _context.SaveChanges();
                                
                               
                            }
                            catch(Exception e)
                            {
                                //si hay error hacer un rollback
                                var st = new StackTrace();
                                var sf = st.GetFrame(1);

                                General.CrearLogError(sf.GetMethod().Name, "depreciacion", e.Message, _config.GetConnectionString("conexion"));
                            }
                        }
                    }
                }
                //_context.objeto.Add(Objeto);
                //result = _context.SaveChanges();
                if (result > 0 && result2 > 0)
                {
                    //se realiza el paso de depreciacion
                    mensaje = "Registro Creado Exitosamente";
                }
                else
                {
                    if(result > 0)
                    {
                        mensaje = "Registro Creado Exitosamente";
                    }
                    else
                    {
                        mensaje = "Error al Crear el Registro";
                    }
                    
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "objeto", e.Message, _config.GetConnectionString("conexion"));
            }
            string json = JsonConvert.SerializeObject(new { Result = result, Mensaje = mensaje });
            return Ok(json);
        }

        [HttpGet("EditarObjeto/{Id}")]
        [Authorize]
        public IActionResult Edit(int Id, [FromBody] objeto Objeto)
        {
            int result = 0;
            string mensaje = "";
            try
            {
                if(Id == Objeto.id)
                {
                    _context.objeto.Update(Objeto);
                    result = _context.SaveChanges();
                    if (result > 0)
                    {
                        mensaje = "Datos Actualizados !";
                    }
                    else
                    {
                        mensaje = "Error al Actualizar";
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "objeto", e.Message, _config.GetConnectionString("conexion"));
            }
            string json = JsonConvert.SerializeObject(new { Result = result, Mensaje = mensaje });
            return Ok(json);
        }

        [HttpPost("busquedaObjeto")]
        [Authorize]
        public IActionResult busquedaObjeto([FromBody] objeto ObjetoB)
        {
            List<dataBusquedaCompleta> busqueda = new List<dataBusquedaCompleta>();
            try
            {
                var data = _context.objeto.Where(x => x.af == ObjetoB.af ||
                                                      x.imei == ObjetoB.imei ||
                                                      x.descripcion.Contains(ObjetoB.descripcion)).ToList();
                if(data == null)
                {
                    return null;
                }
                foreach(objeto item in data)
                {
                    dataBusquedaCompleta busq = new dataBusquedaCompleta();
                    busq.id = item.id;
                    busq.valor1 = item.descripcion;
                    busq.valor2 = item.af;
                    busq.valor3 = item.imei;
                    busq.valor4 = "";
                    busq.valor5 = "";

                    busqueda.Add(busq);
                }

            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "objeto", e.Message, _config.GetConnectionString("conexion"));
            }
            
            return Ok(busqueda);
        }
        [HttpPost("getObjetoArticuloId")]
        [Authorize]
        public IActionResult getObjeto([FromBody] objeto objetoB)
        {
            var data = new Object();
            try
            {
                data = _context.objeto.Where(x => x.id == objetoB.id).First();
                if(data == null)
                {
                    return null;
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "objeto", e.Message, _config.GetConnectionString("conexion"));
            }
            return Ok(data);
        }

        public List<articulosBusquedaFijo> getArticuloFijo(string id) 
        {
            List<articulosBusquedaFijo> articulosFijos = new List<articulosBusquedaFijo>();
            string qwery = "";
            try
            {
                qwery = "SELECT depreciacion.id as Id,"+
                        "depreciacion.id_pedido as Id_ped,"+
                        "depreciacion.cod_art as Codigo,"+
                        "pedidos.usuario as Usuario,"+
                        "depreciacion.descripcion as Descripcion,"+
                        "articulos_af.grupo as Grupo,"+
                        " depreciacion.valor as Valor,"+
                        "depreciacion.ccosto as CECO,"+
                        "depreciacion.v_util as V_util \n"+
                        "FROM depreciacion \n"+
                        "INNER JOIN  articulos_af on depreciacion.cod_art = articulos_af.codigo \n"+
                        "INNER JOIN  pedidos on depreciacion.id_pedido = pedidos.nro_pedido \n"+
                        "where depreciacion.id = '"+ id + "'";
                using (SqlConnection con = new SqlConnection(_config.GetConnectionString("conexion")))
                {
                    using (SqlCommand cmd = new SqlCommand(qwery))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader srd = cmd.ExecuteReader())
                        {
                            while (srd.Read())
                            {
                                articulosFijos.Add(new articulosBusquedaFijo
                                {
                                    Id = Convert.ToInt32(srd["Id"]),
                                    Id_ped = Convert.ToInt32(srd["Id_ped"]),
                                    Codigo = srd["Codigo"] + "",
                                    Usuario = srd["Usuario"] + "",
                                    Descripcion = srd["Descripcion"] + "",
                                    Grupo = srd["Grupo"] + "",
                                    Valor = Convert.ToDouble(srd["Valor"]),
                                    CECO = srd["CECO"] + "",
                                    V_util = Convert.ToInt32(srd["V_util"])
                                });
                            }
                        }
                        con.Close();
                    }
                }
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "depreciacion", e.Message, _config.GetConnectionString("conexion"));
            }
            return articulosFijos;
        }

        public int TipoArticulo(int codigoArticulo)
        {
            int tipo = 0;
            var data = _context.articulos_af.Where(x => x.codigo == codigoArticulo).First();
            if(data != null)
            {
                switch (data.grupo)
                {
                    case "COMPUTO":
                        tipo = 5;
                        break;
                    case "COMUNICACION":
                        tipo = 7;
                        break;
                    case "EDIFICACION":
                        tipo = 8;
                        break;
                    case "LICENCIA":
                        tipo = 9;
                        break;
                    case "MAQUINARIA":
                        tipo = 4;
                        break;
                    case "MYE":
                        tipo = 6;
                        break;
                    default:
                        tipo = 0;
                        break;
                }
            }
            return tipo;
        }
    }
}
