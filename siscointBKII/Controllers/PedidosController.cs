using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;
        public PedidosController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpGet("obtenerContratoProvedor/{id}")]
        [Authorize]
        public IActionResult getContratoProvedor(int id)
        {
            var data = new Object();
            try
            {
                data = (from dp in _context.detalle_proveedor
                        join p in _context.proveedorII on dp.nit equals p.nit
                        where (dp.id == id)
                        select new
                        {
                            nit = dp.nit,
                            razon_social = p.razon_social,
                            contrato = dp.contrato
                        }).First();
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "detalle_proveedor", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return Ok(data);
        }
        [HttpGet("obtenerArticuloComprasId/{id}")]
        [Authorize]
        public IActionResult getArticuloComprasId(int id)
        {
            var data = new Object();
            try
            {
                data = (from ac in _context.compras_articulos
                        join i in _context.iva on ac.tipo_iva equals i.id + ""
                        where (ac.id == id)
                        select new
                        {
                            codigo = ac.codigo,
                            descripcion = ac.descripcion,
                            und = ac.und,
                            cuenta = ac.cuenta,
                            iva = i.porcentaje
                        }).First();
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "compras_articulos", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return Ok(data);
        }
        [HttpGet("obtenerArticuloFijoId/{id}")]
        [Authorize]
        public IActionResult getArticuloFijoId(int id)
        {
            var data = new Object();
            try
            {
                data = (from af in _context.articulos_af
                        join i in _context.iva on af.tipo_iva equals i.id + ""
                        where (af.id == id)
                        select new
                        {
                            codigo = af.codigo,
                            descripcion = af.descripcion,
                            und = af.und,
                            cuenta = af.cuenta,
                            iva = i.porcentaje
                        }).First();
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "articulos_af", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return Ok(data);
        }

        [HttpGet("getDirectivos")]
        [Authorize]
        public IActionResult getDirectivos()
        {
            var data = new Object();
            try
            {
                data = _context.directivos.ToList();
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
                General.CrearLogError(sf.GetMethod().Name, "directivos", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }
            return Ok(data);
        }

        [HttpPost("AgregarPedido")]
        [Authorize]
        public IActionResult agregarTipoPedido(dynamic data)
        {
            string resultado = "";
            string cadena_conexion = _config.GetConnectionString("conexion");
            try
            {
                var dataJson = System.Text.Json.JsonSerializer.Serialize(data);
                var datObject = JObject.Parse(dataJson);
                string tipoPedido = Convert.ToString(datObject["tipoPedido"]);
                string pedido = Convert.ToString(datObject["pedido"]);
                string detallePedido = Convert.ToString(datObject["detallePedido"]);

                pedidos pedidosAdd = JsonConvert.DeserializeObject<pedidos>(pedido);
                List<detalle_pedido> detallePedidos = JsonConvert.DeserializeObject<List<detalle_pedido>>(detallePedido);
            }
            catch(Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);
                MethodBase site = e.TargetSite;
                string methodName = site == null ? null : site.Name;
                General.CrearLogError(sf.GetMethod().Name, "pedidos", e.Message,e.Source,e.StackTrace,methodName, _config.GetConnectionString("conexion"));
            }

            return Ok(resultado);
        }

        public static Boolean CrearPedido(pedidos pedido, List<detalle_pedido> detallePedido, string tipoPedido, AplicationDbContext context, out int numero_pedido,string cadena_conexion)
        {
            Boolean PedidoCreado = false;
            int numero_ped = 0;
            //numero_pedido = 0;
            numero_pedido = Convert.ToInt32(PreloadNro_pedido(context));
            if(numero_pedido > 0)
            {
                pedido.nro_pedido = numero_pedido;
                pedido.aprobado_jefe = "0";
                context.pedidos.Add(pedido);
                int creoPedido = context.SaveChanges();
                if(creoPedido > 0)
                {
                    foreach(detalle_pedido detPedido in detallePedido)
                    {
                        detPedido.id_pedido = numero_pedido;
                        context.detalle_pedido.Add(detPedido);
                        int creoDetallePedido = context.SaveChanges();
                        
                    }

                    switch (tipoPedido)
                    {
                        case "ARTICULO_FIJO":
                            General.EjecutarProcedimientoAlmacenado_validatePresupuesto("validate_presupuesto_AF", numero_pedido + "", cadena_conexion);
                            numero_ped = numero_pedido;
                            var det_ped = context.detalle_pedido.Where(x => x.id_pedido == numero_ped).ToList();
                            if(det_ped != null)
                            {
                                foreach(detalle_pedido idata in det_ped)
                                {
                                    General.EjecutarProcedimientoAlmacenado_validateAFDiff("INSERT_DEPRECIACION", cadena_conexion, idata.id_pedido + "", idata.cantidad?? 0, idata.codigo_art);
                                }
                            }
                            break;
                        case "DIFERIDO":
                            General.EjecutarProcedimientoAlmacenado_validatePresupuesto("validate_presupuesto", numero_pedido + "", cadena_conexion);
                            break;
                        default:
                            General.EjecutarProcedimientoAlmacenado_validatePresupuesto("validate_presupuesto", numero_pedido + "", cadena_conexion);
                            break;
                    }

                }
            }


            return PedidoCreado;
        }

        public static string PreloadNro_pedido(AplicationDbContext context)
        {
            int nro_pedido = 0;
            pedidos p = context.pedidos.OrderByDescending(x => x.nro_pedido).FirstOrDefault();
            if(p != null)
            {
                nro_pedido = p.nro_pedido + 1;
            }
            return nro_pedido + "";
        }
    }
}

