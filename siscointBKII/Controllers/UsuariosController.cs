using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        public UsuariosController(AplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet("Usuarios")]
        [Authorize]
        public IActionResult Usuarios()
        {
            var currentUser = GetCurrentUser();

            //return Ok($"Hola {currentUser.nombre_usuario}, tu usuario es: {currentUser.username}");
            return Ok(currentUser);
        }
        [HttpPost("getUsuario")]
        [Authorize]
        public IActionResult getUsuario([FromBody] usuario user)
        {
            var usuarios = _context.usuario.FirstOrDefault(x => x.codigo == user.codigo || x.nombre_usuario.Contains(user.nombre_usuario) || x.username == user.username);
            if (usuarios == null)
                return null;

            List<usuario> userList = new List<usuario>();
            userList.Add(usuarios);
            return Ok(userList);
        }

        [HttpGet("Listar")]
        [Authorize]
        public IActionResult Listar()
        {
            var usuarios = UsuariosConstantes.user.ToList();
            return Ok(usuarios);
        }

        [HttpGet("ListarUsuarios")]
        public IActionResult listarUsuarios()
        {
            var usuarios = _context.usuario.ToList();
            return Ok(usuarios);
        }


        private UsuariosModel GetCurrentUser()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity == null)
                return null;

            var userclaims = identity.Claims;
            return new UsuariosModel
            {
                codigo = userclaims.FirstOrDefault(o => o.Type == ClaimTypes.SerialNumber)?.Value,
                nombre_usuario = userclaims.FirstOrDefault(o => o.Type == ClaimTypes.Name)?.Value,
                username = userclaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value
            };
        }
    }
}
