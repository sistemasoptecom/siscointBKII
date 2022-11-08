using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using siscointBKII.Models;
using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AplicationDbContext _context;
        private readonly IConfiguration _config;

        public UsuariosController(AplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpGet("Usuarios")]
        [Authorize]
        public IActionResult Usuarios()
        {
            var currentUser = GetCurrentUser();

            //return Ok($"Hola {currentUser.nombre_usuario}, tu usuario es: {currentUser.username}");
            return Ok(currentUser);
        }
        [HttpGet("tipoUsuario")]
        [Authorize]
        public IActionResult tipoUsuario()
        {
            var tipoUsuario = _context.tipo_usuario.ToList();
            return Ok(tipoUsuario);
        }

        [HttpPost("getUsuario")]
        [Authorize]
        public IActionResult getUsuario([FromBody] usuario user)
        {
            List<dataBusquedaCompleta> busqueda = new List<dataBusquedaCompleta>();
            var usuarios = _context.usuario.Where(x =>  x.codigo == user.codigo || x.nombre_usuario.Contains(user.nombre_usuario) || x.username == user.username).ToList();
            if (usuarios == null)
                return null;

            foreach(usuario item in usuarios)
            {
                dataBusquedaCompleta busq = new dataBusquedaCompleta();
                busq.id = item.id;
                busq.valor1 = item.codigo;
                busq.valor2 = item.nombre_usuario;
                busq.valor3 = item.username;
                busq.valor4 = "";
                busq.valor5 = "";
                busqueda.Add(busq);
            }

            //List<usuario> userList = new List<usuario>();
            //userList.Add(usuarios.First());
            return Ok(busqueda);
        }

        [HttpPost("getUsuarioId")]
        [Authorize]
        public IActionResult getUsuarioId([FromBody] usuario user)
        {
            var usuarios = new Object();
            try
            {
                usuarios = _context.usuario.Where(x => x.id == user.id).ToList();
                if (usuarios == null)
                    return null;
            }
            catch(Exception e)
            {
                //log de errores
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "usuario", e.Message, _config.GetConnectionString("conexion"));
            }
            return Ok(usuarios);
        }


        //[HttpGet("Listar")]
        //[Authorize]
        //public IActionResult Listar()
        //{
        //    var usuarios = UsuariosConstantes.user.ToList();
        //    return Ok(usuarios);
        //}

        [HttpGet("ListarUsuarios")]
        public IActionResult listarUsuarios()
        {
            var usuarios = _context.usuario.ToList();
            return Ok(usuarios);
        }
        [HttpPost("AgregarUsuario")]
        [Authorize]
        public IActionResult Add(dynamic data)
        {
            var dato = new Object();
            try
            {
                string dat = System.Text.Json.JsonSerializer.Serialize(data);
                usuario usuario = JsonConvert.DeserializeObject<usuario>(dat);
                string nombreCapa = "mi nombre es " + usuario.nombre_usuario + usuario.pssword;
                //usuario.pssword = General.EncriptarPassword(nombreCapa, usuario.pssword);
                dato = _context.usuario.FirstOrDefault(x => x.codigo == usuario.codigo || x.username == usuario.username);
                //data = _context.usuario.FirstOrDefault(x => x.codigo == usuarios.codigo || x.username == usuarios.username);
                if (dato == null)
                {
                   
                    
                    _context.Add(usuario);
                    _context.SaveChanges();
                }
                else
                {
                    return BadRequest();
                }

            }
            catch(Exception e)
            {
                //log de errores
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "usuario", e.Message, _config.GetConnectionString("conexion"));
            }
            return Ok();
        }
        [HttpPut("EditarUsuario/{id}")]
        [Authorize]
        public IActionResult Edit(int id,[FromBody] usuario usuarios)
        {
            
            
            Boolean EsEditar = false;
            try
            {
                
                if (id != usuarios.id)
                {
                    return BadRequest();
                }
                string _nombre_usuario = usuarios.nombre_usuario;
                string nombre_usuario = Regex.Replace(_nombre_usuario, @"\s", "");
                string nombreCapa = "minombrees"+ nombre_usuario + usuarios.pssword;
                usuario users = new usuario();
                users.id = usuarios.id;
                users.codigo = usuarios.codigo;
                users.nombre_usuario = usuarios.nombre_usuario;
                users.username = usuarios.username;
                users.password = usuarios.password;
                //users.pssword = General.EncriptarPassword(nombreCapa, usuarios.pssword);
                users.id_tipo_usuario = usuarios.id_tipo_usuario;
                users.estado = usuarios.estado;
                users.cargo = usuarios.cargo;
                users.area = usuarios.area;
                users.modulo = usuarios.modulo;

                _context.Update(users);
                _context.SaveChanges();
               
            }
            catch (Exception e)
            {
                //logo de errores
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "usuario", e.Message, _config.GetConnectionString("conexion"));
            }
            return Ok();
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

        //public string ComputeHash(string input, HashAlgorithm algorithm)
        //{
        //    Byte[] inputBytes = Encoding.UTF8.GetBytes(input);

        //    Byte[] hashedBytes = algorithm.ComputeHash(inputBytes);

        //    return BitConverter.ToString(hashedBytes);
        //}

        public static string ToSha256(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            var sb = new StringBuilder();
            for(int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
