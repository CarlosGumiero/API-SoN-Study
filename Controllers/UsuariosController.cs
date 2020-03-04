using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly Data.ApplicationDbContext database;

        public UsuariosController(ApplicationDbContext database)
        {
            this.database = database;
        }

        //api/v1/usuarios/registro
        [HttpPost("registro")]
        public IActionResult Registro([FromBody] Usuario usuario)
        {
            //Verificar se as credenciais são válidas
            //Verificar se o e-mail já está cadastrado no banco
            //Encriptar a senha
            database.Add(usuario);
            database.SaveChanges();
            return Ok(new { msg = "Usuário cadastrado com sucesso!" });
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] Usuario credenciais)
        {
            //Buscar um usuario por email
            //Verificar se a senha está correta
            //Gerar um token JWT e retornar esse token para o usuário
            try
            {
                Usuario usuario = database.Usuarios.First(x => x.Email.Equals(credenciais.Email));

                if (usuario != null)
                {
                    //Achou um usuário com cadastro válido
                    if (usuario.Senha.Equals(credenciais.Senha))
                    {
                        //Usuário acertou a senha : logar
                        string chaveDeSeguranca = "A_barata_da_vizinha_ta_na_minha_cama.";  //Chave de segurança
                        var chaveSimetrica = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveDeSeguranca));
                        var credenciaisDeAcesso = new SigningCredentials(chaveSimetrica, SecurityAlgorithms.HmacSha256Signature);

                        var claims = new List<Claim>();
                        claims.Add(new Claim("id", usuario.UsuarioId.ToString()));
                        claims.Add(new Claim("email", usuario.Email));
                        claims.Add(new Claim(ClaimTypes.Role, "admin"));

                        var JWT = new JwtSecurityToken(
                            issuer: "MercadoAPI.com", // Quem está fornecendo o JWT para o usuário
                            expires: DateTime.Now.AddHours(1),
                            audience: "usuario",
                            signingCredentials: credenciaisDeAcesso,
                            claims: claims
                        );

                        return Ok(new JwtSecurityTokenHandler().WriteToken(JWT));
                    }
                    else
                    {
                        // Não existe nenhum usuário com este email
                        Response.StatusCode = 401; //Não autorizado
                        return new ObjectResult("");
                    }
                }
                else
                {
                    Response.StatusCode = 401;
                    return new ObjectResult("");
                }
            }
            catch (Exception)
            {
                Response.StatusCode = 401;
                return new ObjectResult("");
            }

        }
    }
}