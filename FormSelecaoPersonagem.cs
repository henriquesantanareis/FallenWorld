using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FallenWorld
{
    public partial class FormSelecaoPersonagem : Form
    {
        public FormSelecaoPersonagem()
        {
            InitializeComponent();
        }

        // Global Variables
        private TipoPersonagem personagemSelecionado;


        private void checkPersonagem(object sender, EventArgs e)
        {
            if(RDB_Mage.Checked)
            {
                personagemSelecionado = TipoPersonagem.Mage;
                // mostrar a imagem do personagem selecionado
                // reproduzir musica tema personagem 
            }
            else if (RDB_Knight.Checked)
            {
                personagemSelecionado = TipoPersonagem.Knight;
                // mostrar a imagem do personagem selecionado
                // reproduzir musica tema personagem 
            }
            else if (RDB_Princess.Checked)
            {
                personagemSelecionado = TipoPersonagem.Princess;
                // mostrar a imagem do personagem selecionado
                // reproduzir musica tema personagem 
            }
            else
            {
                personagemSelecionado = TipoPersonagem.None;
            }
        }

        #region Estados botões
        // This method is responsible for enabling or disabling buttons based on the selected character.
        private void stateButtons()
        {

        }
        //try
        //{
        //    private void BTN_Jogar_Click(object sender, EventArgs e)
        //    {
            
        //        FormGame formGame = new FormGame();
        //        formGame.Show();
        //        this.Hide();
        //    }

        //}catch (Exception ex)
        //{
        //    MessageBox.Show("Nao foi possivel iniciar o jogo: " + ex.Message);
        //}
        #endregion

        public enum TipoPersonagem
        {
            Mage,
            Knight,
            Princess,
            // nenhum personagem selecionado
            None
        }
    }
}
