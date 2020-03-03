using System;
using System.Linq;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using API.Hateoas;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly Data.ApplicationDbContext database;
        private Hateoas.Hateoas Hateoas;

        public CategoriasController(ApplicationDbContext database)
        {
            this.database = database;
            Hateoas = new Hateoas.Hateoas("localhost:5001/api/v1/Categorias");
            Hateoas.AddAction("GET_INFO", "GET");
            Hateoas.AddAction("DELETE_PRODUCT", "DELETE");
            Hateoas.AddAction("EDIT_PRODUCT", "PATCH");

        }

        [HttpGet]
        public IActionResult Get()
        {
            var categorias = database.Categorias.ToList();
            List<CategoriaContainer> categoriasHateoas = new List<CategoriaContainer>();
            foreach (var cat in categorias)
            {
                CategoriaContainer categoriaHateoas = new CategoriaContainer();
                categoriaHateoas.categoria = cat;
                categoriaHateoas.links = Hateoas.GetActions(cat.CategoriaId.ToString());
                categoriasHateoas.Add(categoriaHateoas);
            }
            return Ok(categoriasHateoas);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                Categoria categoria = database.Categorias.First(x => x.CategoriaId == id);
                CategoriaContainer categoriaHateoas = new CategoriaContainer();
                categoriaHateoas.categoria = categoria;
                categoriaHateoas.links = Hateoas.GetActions(categoria.CategoriaId.ToString());
                return Ok(categoriaHateoas);
            }
            catch (Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult("");
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] CategoriaTemp ctemp)
        {
            // validation
            if (ctemp.Nome.Length <= 3)
            {
                Response.StatusCode = 400;
                return new ObjectResult(new { msg = "Nome precisa de mais de 3 caracteres." });
            }

            if (ctemp.Descricao.Length <= 3)
            {
                Response.StatusCode = 400;
                return new ObjectResult(new { msg = "Descrição precisa de mais de 3 caracteres." });
            }

            Categoria c = new Categoria();

            c.Nome = ctemp.Nome;
            c.Descricao = ctemp.Descricao;
            database.Categorias.Add(c);
            database.SaveChanges();

            Response.StatusCode = 201;
            return new ObjectResult("");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                Categoria categoria = database.Categorias.First(x => x.CategoriaId == id);
                database.Categorias.Remove(categoria);
                database.SaveChanges();

                return Ok();
            }
            catch (Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult("");
            }
        }

        [HttpPatch]
        public IActionResult Patch([FromBody] Categoria categoria)
        {
            if (categoria.CategoriaId > 0)
            {
                try
                {
                    var c = database.Categorias.First(x => x.CategoriaId == categoria.CategoriaId);

                    if (c != null)
                    {
                        if (categoria.Nome.Length <= 3)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new { msg = "Nome precisa de mais de 3 caracteres." });
                        }

                        if (categoria.Descricao.Length <= 3)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new { msg = "Descrição precisa de mais de 3 caracteres." });
                        }
                        //Editar
                        c.Nome = categoria.Nome != null ? categoria.Nome : c.Nome;
                        c.Descricao = categoria.Descricao != null ? categoria.Descricao : c.Descricao;

                        database.SaveChanges();
                        return Ok();
                    }
                    else
                    {
                        Response.StatusCode = 400;
                        return new ObjectResult(new { msg = "Produto não encontrado" });
                    }
                }
                catch
                {
                    Response.StatusCode = 400;
                    return new ObjectResult(new { msg = "Produto não encontrado" });
                }
            }
            else
            {
                Response.StatusCode = 400;
                return new ObjectResult(new { msg = "Id do produto é inválido" });
            }
        }
    }

    public class CategoriaTemp
    {
        public string Nome { get; set; }
        public string Descricao { get; set; }
    }

    public class CategoriaContainer
    {
        public Categoria categoria { get; set; }
        public Link[] links { get; set; }
    }
}