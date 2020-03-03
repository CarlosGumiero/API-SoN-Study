namespace API.Models
{
    public class Produto
    {
        public int ProdutoId {get; set;}
        public string Nome { get; set; }
        public float Preco { get; set; }
        public Categoria Categoria {get; set; }
    }
}