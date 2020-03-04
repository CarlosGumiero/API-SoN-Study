using System;
using System.Linq;
using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using API.Hateoas;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly Data.ApplicationDbContext database;
        private Hateoas.Hateoas Hateoas;

        public ProdutosController(ApplicationDbContext database)
        {
            this.database = database;
            Hateoas = new Hateoas.Hateoas("localhost:5001/api/v1/Produtos");
            Hateoas.AddAction("GET_INFO", "GET");
            Hateoas.AddAction("DELETE_PRODUCT", "DELETE");
            Hateoas.AddAction("EDIT_PRODUCT", "PATCH");

        }

        [HttpGet]
        public IActionResult Get()
        {
            database.Categorias.ToList();
            var produtos = database.Produtos.ToList();
            List<ProdutoContainer> produtosHateoas = new List<ProdutoContainer>();
            foreach (var prod in produtos)
            {
                ProdutoContainer produtoHateoas = new ProdutoContainer();
                produtoHateoas.produto = prod;
                produtoHateoas.links = Hateoas.GetActions(prod.ProdutoId.ToString());
                produtosHateoas.Add(produtoHateoas);
            }
            return Ok(produtosHateoas);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            try
            {
                Produto produto = database.Produtos.First(x => x.ProdutoId == id);
                database.Categorias.ToList();
                ProdutoContainer produtoHatoeas = new ProdutoContainer();
                produtoHatoeas.produto = produto;
                produtoHatoeas.links = Hateoas.GetActions(produto.ProdutoId.ToString());
                return Ok(produtoHatoeas);
            }
            catch (Exception)
            {
                Response.StatusCode = 404;
                return new ObjectResult("");
            }

        }

        [HttpPost]
        public IActionResult Post([FromBody] ProdutoTemp pTemp)
        {
            // validation
            if (pTemp.Preco <= 0)
            {
                Response.StatusCode = 400;
                return new ObjectResult(new { msg = "Preço não pode ser negativo nem 0." });
            }

            if (pTemp.Nome.Length <= 1)
            {
                Response.StatusCode = 400;
                return new ObjectResult(new { msg = "Nome precisa de mais de 1 caracter." });
            }

            Produto p = new Produto();

            database.Categorias.ToList();
            p.Nome = pTemp.Nome;
            p.Preco = pTemp.Preco;
            p.Categoria = database.Categorias.First(x => x.CategoriaId == pTemp.categoria.CategoriaId);
            database.Produtos.Add(p);
            database.SaveChanges();

            Response.StatusCode = 201;
            return new ObjectResult("");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            try
            {
                Produto produto = database.Produtos.First(x => x.ProdutoId == id);
                database.Produtos.Remove(produto);
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
        public IActionResult Patch([FromBody] Produto produto)
        {
            if (produto.ProdutoId > 0)
            {
                try
                {
                    var p = database.Produtos.First(x => x.ProdutoId == produto.ProdutoId);
                    database.Categorias.ToList();

                    if (p != null)
                    {
                        if (produto.Preco <= 0)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new { msg = "Preço não pode ser negativo nem 0." });
                        }

                        if (produto.Nome.Length <= 1)
                        {
                            Response.StatusCode = 400;
                            return new ObjectResult(new { msg = "Nome precisa de mais de 1 caracter." });
                        }
                        //Editar
                        p.Nome = produto.Nome != null ? produto.Nome : p.Nome;
                        p.Preco = produto.Preco != 0 ? produto.Preco : p.Preco;
                        p.Categoria = database.Categorias.First(x => x.CategoriaId == produto.Categoria.CategoriaId);

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

        public class ProdutoTemp
        {
            public string Nome { get; set; }
            public float Preco { get; set; }
            public Categoria categoria { get; set; }
        }

        public class ProdutoContainer
        {
            public Produto produto { get; set; }
            public Link[] links { get; set; }
        }
    }
}