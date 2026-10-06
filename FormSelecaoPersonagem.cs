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
            RDB_Mage.Checked = false;
            RDB_Knight.Checked = false;
            RDB_Princess.Checked = false;
        }


        private void checkPersonagem(object sender, EventArgs e)
        {
            if(RDB_Mage.Checked == true)
            {
                TipoPersonagem.EscolherPersonagem(Personagem.Mago);
                // mostrar a imagem do personagem selecionado
                // reproduzir musica tema personagem 
            }
            else if (RDB_Knight.Checked == true)
            {
                TipoPersonagem.EscolherPersonagem(Personagem.Cavaleiro);
                // mostrar a imagem do personagem selecionado
                // reproduzir musica tema personagem 
            }
            else if (RDB_Princess.Checked == true)
            {
                TipoPersonagem.EscolherPersonagem(Personagem.Princesa);
                // mostrar a imagem do personagem selecionado
                // reproduzir musica tema personagem 
            }
            else
            {
                TipoPersonagem.EscolherPersonagem(Personagem.Nenhum);
            }
        }

        #region Play button
        private void BTN_Jogar_Click(object sender, EventArgs e)
        {
            if (TipoPersonagem.PersonagemEscolhido != Personagem.Nenhum)
            {
                BTN_PLAY.DialogResult = DialogResult.OK;
            }
            else
            {
                
                MessageBox.Show("Selecione um personagem para jogar!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BTN_PLAY_MouseEnter(object sender, EventArgs e)
        {
            BTN_PLAY.BackgroundImage = Properties.Resources.Play_hover;
        }

        private void BTN_PLAY_MouseLeave(object sender, EventArgs e)
        {
            BTN_PLAY.BackgroundImage = Properties.Resources.Play_normal;
        }

        private void BTN_PLAY_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                BTN_PLAY.BackgroundImage = Properties.Resources.Play_pressed;
        }

        private void BTN_PLAY_MouseUp(object sender, MouseEventArgs e)
        {
            if (BTN_PLAY.ClientRectangle.Contains(
                BTN_PLAY.PointToClient(Cursor.Position)))
            {
                BTN_PLAY.BackgroundImage = Properties.Resources.Play_hover;
            }
            else
            {
                BTN_PLAY.BackgroundImage = Properties.Resources.Play_normal;
            }
        }
        #endregion

        #region Botão Sair
        private void BTN_EXIT_Click(object sender, EventArgs e)
        {
            BTN_EXIT.Image = Properties.Resources.Exit_pressed;
            this.Close();
        }
        
        private void BTN_EXIT_MouseEnter(object sender, EventArgs e)
        {
            BTN_EXIT.BackgroundImage = Properties.Resources.Exit_hover;
        }

        private void BTN_EXIT_MouseLeave(object sender, EventArgs e)
        {
            BTN_EXIT.BackgroundImage = Properties.Resources.Exit_normal;
        }

        private void BTN_EXIT_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                BTN_EXIT.BackgroundImage = Properties.Resources.Exit_pressed;
        }

        private void BTN_EXIT_MouseUp(object sender, MouseEventArgs e)
        {
            if (BTN_EXIT.ClientRectangle.Contains(
                BTN_EXIT.PointToClient(Cursor.Position)))
            {
                BTN_EXIT.BackgroundImage = Properties.Resources.Exit_hover;
            }
            else
            {
                BTN_EXIT.BackgroundImage = Properties.Resources.Exit_normal;
            }
        }
        #endregion
    }
}
