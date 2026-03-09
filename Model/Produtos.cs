using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Model
{
    public class Produtos
    {
        public int Id{get;set;}
        public string Nome{get;set;} = string.Empty;
        public string Codigo{get;set;} = string.Empty;
        public int Quantidade{get;set;}
        public double Preco{get;set;}
        public int EstoqueMinimo{get;set;}

    }
}