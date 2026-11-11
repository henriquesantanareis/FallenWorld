
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using LibVLCSharp.Shared;

namespace FallenWorld
{
    public partial class FormTransicaoPortal : Form
    {
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private Media _media;
        private string _caminhoVideo;
        private int _finalizacaoSolicitada = 0;

        public FormTransicaoPortal(string nomeVideo)
        {
            InitializeComponent();

            if (string.IsNullOrWhiteSpace(nomeVideo))
                throw new ArgumentException("Nome do vídeo inválido.");

            // Localiza o vídeo dentro da pasta Cutscenes.
            _caminhoVideo = Path.Combine(Application.StartupPath,"Cutscenes",nomeVideo);

            Shown += FormTransicaoPortal_Shown;
        }

        private void FormTransicaoPortal_Shown(object sender, EventArgs e)
        {
            // Verifica se o vídeo existe.
            if (!File.Exists(_caminhoVideo))
            {
                MessageBox.Show(
                    "Vídeo não encontrado:\n" + _caminhoVideo,
                    "Fallen World",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                DialogResult = DialogResult.Abort;
                Close();

                return;
            }

            try
            {
                // Inicializa o VLC.
                _libVLC = new LibVLC();

                // Cria o reprodutor.
                _mediaPlayer = new MediaPlayer(_libVLC);

                // Associa o reprodutor ao VideoView.
                videoView1.MediaPlayer = _mediaPlayer;

                // Detecta quando o vídeo termina.
                _mediaPlayer.EndReached +=
                    MediaPlayer_EndReached;

                // Detecta erros durante a reprodução.
                _mediaPlayer.EncounteredError +=
                    MediaPlayer_EncounteredError;

                // Carrega o vídeo da pasta Cutscenes.
                _media = new Media(_libVLC,_caminhoVideo,FromType.FromPath);

                // Inicia a reprodução.
                if (!_mediaPlayer.Play(_media))
                {
                    throw new Exception("Não foi possível iniciar o vídeo.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao reproduzir o portal:\n\n" + ex.Message,
                    "Fallen World",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                DialogResult = DialogResult.Abort;
                Close();
            }
        }

        // Executado quando o vídeo termina.
        private void MediaPlayer_EndReached(object sender, EventArgs e)
        {
            FinalizarTransicao(DialogResult.OK);
        }

        // Executado quando ocorre um erro no VLC.
        private void MediaPlayer_EncounteredError(object sender, EventArgs e)
        {
            FinalizarTransicao(DialogResult.Abort);
        }

        private void FinalizarTransicao(DialogResult resultado)
        {
            // Evita executar a finalização mais de uma vez.
            if (Interlocked.Exchange(ref _finalizacaoSolicitada, 1) != 0)
                return;

            if (IsDisposed || !IsHandleCreated)
                return;

            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (IsDisposed)
                        return;

                    if (resultado == DialogResult.Abort)
                    {
                        MessageBox.Show(
                            "Ocorreu um erro durante a reprodução do portal.",
                            "Fallen World",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }

                    DialogResult = resultado;
                    Close();
                }));
            }
            catch (InvalidOperationException)
            {
                // O formulário já está sendo fechado.
            }
        }

        // Libera os recursos ao fechar o formulário.
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.EndReached -= MediaPlayer_EndReached;

                _mediaPlayer.EncounteredError -= MediaPlayer_EncounteredError;

                _mediaPlayer.Stop();

                videoView1.MediaPlayer = null;

                _mediaPlayer.Dispose();
                _mediaPlayer = null;
            }

            if (_media != null)
            {
                _media.Dispose();
                _media = null;
            }

            if (_libVLC != null)
            {
                _libVLC.Dispose();
                _libVLC = null;
            }

            // Não excluímos o vídeo, pois ele é um arquivo
            // permanente da pasta Cutscenes.

            base.OnFormClosed(e);
        }
    }
}
