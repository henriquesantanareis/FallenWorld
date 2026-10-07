using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System;
using System.IO;
using System.Windows.Forms;

namespace FallenWorld
{
    public partial class FormIntro : Form
    {
        private LibVLC LibVLC;
        private LibVLCSharp.Shared.MediaPlayer mediaPlayer;
        private VideoView videoView;
        private Media media;
        public FormIntro()
        {
            InitializeComponent();

            LibVLCSharp.Shared.Core.Initialize();

            LibVLC = new LibVLC();
            mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(LibVLC);

            videoView = new VideoView
            {
                Dock = DockStyle.Fill,
                MediaPlayer = mediaPlayer
            };

            PNL_INTRO.Controls.Add(videoView);

            mediaPlayer.EndReached += MediaPlayer_EndReached;
        }

        
        #region Form Load
        private void FormIntro_Load(object sender, EventArgs e)
        {
            string videoPath = Path.Combine(Application.StartupPath,"Cutscenes", "Intro_game.mp4");
            if (!File.Exists(videoPath))
            {
                MessageBox.Show(
                    "Vídeo não encontrado:\n" + videoPath,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }
            media = new Media(LibVLC, new Uri(videoPath));

            mediaPlayer.Play(media);
        }
        #endregion

        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                AbrirSelecao();
            }));
        }

        #region Abre Seleção de Personagem
        private void AbrirSelecao()
        {
            mediaPlayer.Stop();

            this.Hide();

            using (var selecao = new FormSelecaoPersonagem())
            {
                if (selecao.ShowDialog() == DialogResult.OK)
                {
                    using (var game = new FormGame(TipoPersonagem.PersonagemEscolhido))
                    {
                        game.ShowDialog();
                    }
                }
            }

            this.Close();
        }
        #endregion

        #region Skip Intro
        protected override bool ProcessCmdKey(ref Message msg,Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                //AbrirSelecao();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            mediaPlayer?.Stop();
            mediaPlayer?.Dispose();
            LibVLC?.Dispose();
            base.OnFormClosed(e);
        }
        #endregion
    }
}
