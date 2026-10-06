using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FallenWorld
{
    public enum Personagem
    {
        Nenhum,
        Mago,
        Cavaleiro,
        Princesa
    }
    
    public class TipoPersonagem
    {
        private static Personagem _personagem;

        public static Personagem PersonagemEscolhido => _personagem;

        public static void EscolherPersonagem(Personagem escolhido)
        {
            _personagem = escolhido;
        }
        
    }
    
}
