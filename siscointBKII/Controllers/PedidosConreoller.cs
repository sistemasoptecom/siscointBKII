
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
    public class PedidosConreoller : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;
        public PedidosConreoller(AplicationDbContext context, IConfiguration config)
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
    }
}
