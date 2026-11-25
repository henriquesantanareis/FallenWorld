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

        #region Variáveis globais
        private SoundPlayer musicChooseCharacters;
        private SoundPlayer selectionMusicMage;
        private SoundPlayer selectionMusicKnight;
        private SoundPlayer selectionMusicPrincess;

        // Prévia animada do personagem selecionado.
        private Player _previewPersonagem;
        // Identifica qual personagem está sendo exibido.
        private Personagem _personagemPreviewAtual = Personagem.Nenhum;
        // Área reservada para exibir o personagem selecionado.
        private readonly Rectangle _areaPreviewPersonagem = new Rectangle(317, 216, 171, 220);
        #endregion

        #region Load Form
        private void FormSelecaoPersonagem_Load(object sender, EventArgs e)
        {
            //music menu
            musicChooseCharacters = new SoundPlayer(Properties.Resources.Shadows_of_Three_Heroes);
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
                    selectionMusicMage = new SoundPlayer(Properties.Resources.Cinderwick_s_Crooked_Grin);
                    selectionMusicMage.Play();
                    break;

                case Personagem.Cavaleiro:
                    musicChooseCharacters.Stop();
                    selectionMusicKnight = new SoundPlayer(Properties.Resources.Rise_of_the_Ashen_Knight);
                    selectionMusicKnight.Play();
                    break;

                case Personagem.Princesa:
                    musicChooseCharacters.Stop();
                    selectionMusicPrincess = new SoundPlayer(Properties.Resources.Crown_of_Ashes_and_Dawn);
                    selectionMusicPrincess.Play();
                    break;

                default:
                    musicChooseCharacters = new SoundPlayer(Properties.Resources.Shadows_of_Three_Heroes);
                    return;
            }
        }
        #endregion

        private void MostrarPreviewPersonagem(Personagem personagem)
        {
            // Evita recriar a prévia do mesmo personagem.
            if (_personagemPreviewAtual == personagem &&
                _previewPersonagem != null &&
                !_previewPersonagem.IsDisposed)
            {
                return;
            }

            // Remove e libera a prévia anterior.
            if (_previewPersonagem != null)
            {
                this.Controls.Remove(_previewPersonagem);
                _previewPersonagem.Dispose();
                _previewPersonagem = null;
            }

            // Cria o personagem com sua animação Idle.
            _previewPersonagem = new Player(personagem);

            // Ajusta o tamanho visual de cada personagem na seleção.
            if (personagem == Personagem.Mago)
            {
                _previewPersonagem.DefinirEscalaVisual(3f);
            }
            else if (personagem == Personagem.Cavaleiro)
            {
                _previewPersonagem.DefinirEscalaVisual(2f);
            }
            else if (personagem == Personagem.Princesa)
            {
                _previewPersonagem.DefinirEscalaVisual(1.5f);
            }

            // Obtém os limites dos pixels realmente visíveis.
            Rectangle spriteBounds = _previewPersonagem.GetVisibleSpriteBounds();

            // Centraliza o personagem na área reservada.
            int posicaoX =
                _areaPreviewPersonagem.X +
                (_areaPreviewPersonagem.Width - spriteBounds.Width) / 2 -
                spriteBounds.Left;

            // Alinha os pés do personagem à base da área.
            int posicaoY = _areaPreviewPersonagem.Bottom - spriteBounds.Bottom;

            _previewPersonagem.Location = new Point(posicaoX, posicaoY);

            // Adiciona diretamente ao formulário.
            this.Controls.Add(_previewPersonagem);

            _previewPersonagem.BringToFront();

            // Mantém o botão Jogar sempre na frente.
            BTN_PLAY.BringToFront();

            _personagemPreviewAtual = personagem;
        }

        #region Personagem selection
        private void checkPersonagem(object sender, EventArgs e)
        {
            if(RDB_Mage.Checked == true)
            {
                RDB_Mage.Image = Properties.Resources.duckMage_Avatar_selected;
                RDB_Knight.Image = Properties.Resources.knight_Avatar_normal;
                RDB_Princess.Image = Properties.Resources.Princess_Avatar_normal;
                TipoPersonagem.EscolherPersonagem(Personagem.Mago);
                MostrarPreviewPersonagem(Personagem.Mago);
                TocarMusicaPersonagem(Personagem.Mago);
                // mostrar a imagem do personagem selecionado
            }
            else if (RDB_Knight.Checked == true)
            {
                RDB_Knight.Image = Properties.Resources.knight_Avatar_selected;
                RDB_Mage.Image = Properties.Resources.duckMage_Avatar_normal;
                RDB_Princess.Image = Properties.Resources.Princess_Avatar_normal;
                TipoPersonagem.EscolherPersonagem(Personagem.Cavaleiro);
                MostrarPreviewPersonagem(Personagem.Cavaleiro);
                TocarMusicaPersonagem(Personagem.Cavaleiro);
                // mostrar a imagem idle do personagem selecionado
            }
            else if (RDB_Princess.Checked == true)
            {
                RDB_Princess.Image = Properties.Resources.Princess_Avatar_selected;
                RDB_Knight.Image = Properties.Resources.knight_Avatar_normal;
                RDB_Mage.Image = Properties.Resources.duckMage_Avatar_normal;
                TipoPersonagem.EscolherPersonagem(Personagem.Princesa);
                MostrarPreviewPersonagem(Personagem.Princesa);
                TocarMusicaPersonagem(Personagem.Princesa);
                // mostrar a imagem idle do personagem selecionado
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
            else if (RDB_Mage.ClientRectangle.Contains(RDB_Mage.PointToClient(Cursor.Position)))
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
            else if (RDB_Knight.ClientRectangle.Contains(RDB_Knight.PointToClient(Cursor.Position)))
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
            else if (RDB_Princess.ClientRectangle.Contains(RDB_Princess.PointToClient(Cursor.Position)))
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
                musicChooseCharacters.Stop();
                musicChooseCharacters.Dispose();
                using (FormTransicaoPortal portal = new FormTransicaoPortal("Celestial_Galaxy_Portal.mp4"))
                {
                    DialogResult resultado = portal.ShowDialog();

                    if (resultado != DialogResult.OK)
                        return;
                }
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
            if (BTN_PLAY.ClientRectangle.Contains(BTN_PLAY.PointToClient(Cursor.Position)))
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
        private void BTN_EXIT_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                BTN_EXIT.Image = Properties.Resources.Exit_pressed;
                this.Close();
            }
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
            if (BTN_EXIT.ClientRectangle.Contains(BTN_EXIT.PointToClient(Cursor.Position)))
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
