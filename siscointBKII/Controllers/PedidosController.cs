using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController  : ControllerBase
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
                            iva = i.porcentaje
                        }).First();
            }
            catch(Exception e)
            {

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
                            iva = i.porcentaje
                        }).First();
            }
            catch(Exception e)
            {

            }
            return Ok(data);
        }
    }
}

