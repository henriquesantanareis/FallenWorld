using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using NAudio.Wave;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;

namespace FallenWorld
{
    public partial class FormSelecaoPersonagem : Form
    {
        public FormSelecaoPersonagem()
        {
            InitializeComponent();
            // Desmarcar todos os radio buttons
            RDB_Mage.Checked = false;
            RDB_Knight.Checked = false;
            RDB_Princess.Checked = false;
        }

        #region Varriáveis globais
        private SoundPlayer musicChooseCharacters;
        private SoundPlayer selectionMusicMage;
        private SoundPlayer selectionMusicKnight;
        private SoundPlayer selectionMusicPrincess;
        private string pathMusicMenu = Path.Combine(Application.StartupPath, "Audio", "Musics", "Shadows_of_Three_Heroes.wav");
        //private string pathMusicMage = Path.Combine(Application.StartupPath, "Audio", "Musics", "musica_mago.wav");
        private string pathMusicKnight = Path.Combine(Application.StartupPath, "Audio", "Musics", "Rise_of_the_Ashen_Knight.wav");
        private string pathMusicPrincess = Path.Combine(Application.StartupPath, "Audio", "Musics", "Crown_of_Ashes_and_Dawn.wav");
        #endregion

        #region Load Form
        private void FormSelecaoPersonagem_Load(object sender, EventArgs e)
        {
            //music menu
            
            musicChooseCharacters = new SoundPlayer(pathMusicMenu);
            musicChooseCharacters.PlayLooping();   
        }
        #endregion

        #region selection music character
        private void TocarMusicaPersonagem(Personagem personagem)
        {
            switch (personagem)
            {
                case Personagem.Mago:
                    musicChooseCharacters.Stop();
                    //selectionMusicKnight.Stop();
                    //selectionMusicPrincess.Stop();
                    //string pathMusicMage = Path.Combine(Application.StartupPath, "Audio", "Musics", "musica_mago.wav");
                    //selectionMusicMage = new SoundPlayer(pathMusicMage);
                    break;

                case Personagem.Cavaleiro:
                    musicChooseCharacters.Stop();
                    //selectionMusicPrincess.Stop();
                    //selectionMusicMage.Stop();
                    selectionMusicKnight = new SoundPlayer(pathMusicKnight);
                    selectionMusicKnight.Play();
                    break;

                case Personagem.Princesa:
                    musicChooseCharacters.Stop();
                    //musicChooseCharacters.Dispose();
                    selectionMusicPrincess = new SoundPlayer(pathMusicPrincess);
                    selectionMusicPrincess.Play();
                    break;

                default:
                    musicChooseCharacters = new SoundPlayer(pathMusicMenu);
                    return;
            }

            //selectionMusicMage?.Play();
        }
        #endregion

        #region Personagem selection
        private void checkPersonagem(object sender, EventArgs e)
        {
            if(RDB_Mage.Checked == true)
            {
                RDB_Mage.Image = Properties.Resources.duckMage_Avatar_selected;
                RDB_Knight.Image = Properties.Resources.knight_Avatar_normal;
                RDB_Princess.Image = Properties.Resources.Princess_Avatar_normal;
                TipoPersonagem.EscolherPersonagem(Personagem.Mago);
                // reproduzir musica tema personagem 
                // mostrar a imagem do personagem selecionado
            }
            else if (RDB_Knight.Checked == true)
            {
                RDB_Knight.Image = Properties.Resources.knight_Avatar_selected;
                RDB_Mage.Image = Properties.Resources.duckMage_Avatar_normal;
                RDB_Princess.Image = Properties.Resources.Princess_Avatar_normal;
                TipoPersonagem.EscolherPersonagem(Personagem.Cavaleiro);
                TocarMusicaPersonagem(Personagem.Cavaleiro);
                // mostrar a imagem do personagem selecionado
            }
            else if (RDB_Princess.Checked == true)
            {
                RDB_Princess.Image = Properties.Resources.Princess_Avatar_selected;
                RDB_Knight.Image = Properties.Resources.knight_Avatar_normal;
                RDB_Mage.Image = Properties.Resources.duckMage_Avatar_normal;
                TipoPersonagem.EscolherPersonagem(Personagem.Princesa);
                TocarMusicaPersonagem(Personagem.Princesa);
                // mostrar a imagem do personagem selecionado
            }
            else
            {
                TipoPersonagem.EscolherPersonagem(Personagem.Nenhum);
            }                      
        }
        #endregion

        #region Choose character button states
        // Mago
        private void duckMage_MouseEnter(object sender, EventArgs e)
        {
            if (!RDB_Mage.Checked)
                RDB_Mage.Image = Properties.Resources.duckMage_Avatar_hover;
        }

        private void duckMage_MouseLeave(object sender, EventArgs e)
        {
            if (!RDB_Mage.Checked)
                RDB_Mage.Image = Properties.Resources.duckMage_Avatar_normal;
        }

        private void duckMage_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                RDB_Mage.Image = Properties.Resources.duckMafe_Avatar_pressed;
        }

        private void duckMage_MouseUp(object sender, MouseEventArgs e)
        {
            if (RDB_Mage.Checked)
            {
                RDB_Mage.Image = Properties.Resources.duckMage_Avatar_selected;
            }
            else if (RDB_Mage.ClientRectangle.Contains(
                RDB_Mage.PointToClient(Cursor.Position)))
            {
                RDB_Mage.Image = Properties.Resources.duckMage_Avatar_hover;
            }
            else
            {
                RDB_Mage.Image = Properties.Resources.duckMage_Avatar_normal;
            }
        }
        // Cavaleiro
        private void knight_MouseEnter(object sender, EventArgs e)
        {
            if (!RDB_Knight.Checked)
                RDB_Knight.Image = Properties.Resources.knight_Avatar_hover;
        }

        private void knight_MouseLeave(object sender, EventArgs e)
        {
            if (!RDB_Knight.Checked)
                RDB_Knight.Image = Properties.Resources.knight_Avatar_normal;
        }

        private void knight_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                RDB_Knight.Image = Properties.Resources.knight_Avatar_pressed;
        }

        private void knight_MouseUp(object sender, MouseEventArgs e)
        {
            if (RDB_Knight.Checked)
            {
                RDB_Knight.Image = Properties.Resources.knight_Avatar_selected;
            }
            else if (RDB_Knight.ClientRectangle.Contains(
                RDB_Knight.PointToClient(Cursor.Position)))
            {
                RDB_Knight.Image = Properties.Resources.knight_Avatar_hover;
            }
            else
            {
                RDB_Knight.Image = Properties.Resources.knight_Avatar_normal;
            }
        }
        // Princesa
        private void princess_MouseEnter(object sender, EventArgs e)
        {
            if (!RDB_Princess.Checked)
                RDB_Princess.Image = Properties.Resources.Princess_Avatar_hover;
        }

        private void princess_MouseLeave(object sender, EventArgs e)
        {
            if (!RDB_Princess.Checked)
                RDB_Princess.Image = Properties.Resources.Princess_Avatar_normal;
        }

        private void princess_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                RDB_Princess.Image = Properties.Resources.Princess_Avatar_Pressed;
        }

        private void princess_MouseUp(object sender, MouseEventArgs e)
        {
            if (RDB_Princess.Checked)
            {
                RDB_Princess.Image = Properties.Resources.Princess_Avatar_selected;
            }
            else if (RDB_Princess.ClientRectangle.Contains(
                RDB_Princess.PointToClient(Cursor.Position)))
            {
                RDB_Princess.Image = Properties.Resources.Princess_Avatar_hover;
            }
            else
            {
                RDB_Princess.Image = Properties.Resources.Princess_Avatar_normal;
            }
        }
        #endregion

        #region Play button
        private void BTN_Jogar_Click(object sender, EventArgs e)
        {
            if (TipoPersonagem.PersonagemEscolhido != Personagem.Nenhum)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Selecione um personagem para jogar!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BTN_PLAY_MouseEnter(object sender, EventArgs e)
        {
            BTN_PLAY.Image = Properties.Resources.Play_hover;
        }

        private void BTN_PLAY_MouseLeave(object sender, EventArgs e)
        {
            BTN_PLAY.Image = Properties.Resources.Play_normal;
        }

        private void BTN_PLAY_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                BTN_PLAY.Image = Properties.Resources.Play_pressed;
        }

        private void BTN_PLAY_MouseUp(object sender, MouseEventArgs e)
        {
            if (BTN_PLAY.ClientRectangle.Contains(
                BTN_PLAY.PointToClient(Cursor.Position)))
            {
                BTN_PLAY.Image = Properties.Resources.Play_hover;
            }
            else
            {
                BTN_PLAY.Image = Properties.Resources.Play_normal;
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
