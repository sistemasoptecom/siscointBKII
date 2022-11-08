using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using siscointBKII.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace siscointBKII.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        public List<UsuariosModel> usuarios = new List<UsuariosModel>();
        private AplicationDbContext _context;
        private IConfiguration _config;
        public LoginController(IConfiguration config, AplicationDbContext context)
        {
            _config = config;
            _context = context;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            var user = Authenticate(userLogin);
            if(user == null)
                return NotFound("User not found");

            var token = Generate(user);
            return Ok(token);
        }

        [AllowAnonymous]
        [HttpPost("resetPassword")]
        public IActionResult resetPassword([FromBody] UserLogin userLogin)
        {
            Boolean EsUsuarioValido = validateUsuario(userLogin.Username);
            string data_mensaje = "";
            if (EsUsuarioValido)
            {
                usuario usuario = _context.usuario.Where(x => x.username == userLogin.Username).FirstOrDefault();
                if(usuario != null)
                {
                    string _nombre_usuario = usuario.nombre_usuario;
                    string nombre_usuario = System.Text.RegularExpressions.Regex.Replace(_nombre_usuario, @"\s", "");
                    string nombreCapa = "minombrees" + nombre_usuario + userLogin.Password;
                    //string nombreCapa = "mi nombre es " + usuario.nombre_usuario + userLogin.Password;
                    //usuario.pssword = General.EncriptarPassword(nombreCapa, userLogin.Password);
                    usuario.pssword = General.cifrarTextoAES(userLogin.Password, nombreCapa, nombreCapa, "SHA1", 22, "1234567891234567", 128);
                    _context.Update(usuario);
                    int saveChanges = _context.SaveChanges();
                    if(saveChanges > 0)
                    {
                        data_mensaje = "CAMBIO DE CONTRASEÑA EXITOSO!";
                    }

                }
                else
                {
                    data_mensaje = "USUARIO NO EXISTE!";
                }
            }
            else
            {
                data_mensaje = "USUARIO INVALIDO!";
            }
            return Ok(data_mensaje);
        }

        private Boolean validateUsuario(string usuario)
        {
            Boolean EsUsuarioValido = false;
            try
            {
                var _usuario = _context.usuario.Where(x => x.username == usuario).FirstOrDefault();
                if(_usuario != null)
                {
                    EsUsuarioValido = true;
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "login", e.Message, _config.GetConnectionString("conexion"));
            }
            return EsUsuarioValido;
        }

        private string Generate(usuario user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var crediales = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                
                new Claim(ClaimTypes.SerialNumber, user.codigo),
                new Claim(ClaimTypes.Name, user.nombre_usuario),
                new Claim(ClaimTypes.NameIdentifier, user.username)
            };
            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
                _config["Jwt:Audience"], 
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: crediales);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private usuario Authenticate(UserLogin userLogin)
        {
            var user = new usuario();
            Boolean EsUsuarioValido = false;
            string _nombre_usuario = "";
            string nombre_usuario = "";
            string nombreCapa = "";
            try
            {
                user = _context.usuario.FirstOrDefault(o => o.username == userLogin.Username);
                string passwordHash = "";
                if (user != null)
                {
                    passwordHash = user.pssword;
                    _nombre_usuario = user.nombre_usuario;
                    nombre_usuario = System.Text.RegularExpressions.Regex.Replace(_nombre_usuario, @"\s", "");
                    nombreCapa = "minombrees" + nombre_usuario + userLogin.Password;
                }


                //passwordHash = General.DesencriptarPassword(passwordHash, userLogin.Password);
                passwordHash = General.descifrarTextoAES(passwordHash, nombreCapa, nombreCapa, "SHA1", 22, "1234567891234567", 128);
                if (passwordHash != "")
                {
                    if(passwordHash.Contains(userLogin.Password))
                    {
                        EsUsuarioValido = true;
                    }
                }
            }
            catch (Exception e)
            {
                var st = new StackTrace();
                var sf = st.GetFrame(1);

                General.CrearLogError(sf.GetMethod().Name, "empleado", e.Message, _config.GetConnectionString("conexion"));
            }

            //var currenUser = _context.usuario.FirstOrDefault(o => o.username.ToLower() == userLogin.Username.ToLower() && o.pssword == userLogin.Password);
            //if(currenUser == null)
            //{
            //    return null;
            //}
            if (!EsUsuarioValido)
                return null;
            return user;

        }

       

    }
}
