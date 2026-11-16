
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace FallenWorld
{
    public class PuzzleRune : UserControl
    {
        #region Estados

        public enum RuneState
        {
            Inactive,
            Activating,
            Active
        }

        private RuneState _state = RuneState.Inactive;

        public RuneState State
        {
            get { return _state; }
        }

        public bool IsActivated
        {
            get { return _state == RuneState.Active; }
        }

        public bool IsActivating
        {
            get { return _state == RuneState.Activating; }
        }

        public int RuneId { get; private set; }

        public event Action<PuzzleRune> ActivationCompleted;

        #endregion

        #region Spritesheet

        private Bitmap _inactiveFrame;
        private readonly List<Bitmap> _activationFrames = new List<Bitmap>();

        private int _currentFrame = 0;
        private int _frameElapsedMs = 0;

        // Tempo de exibição de cada frame.
        private const int FrameDurationMs = 100;

        #endregion

        #region Constructor

        public PuzzleRune(int runeId, Bitmap spriteSheet, string jsonMetadata)
        {
            if (runeId < 1 || runeId > 3)
                throw new ArgumentOutOfRangeException("runeId");

            if (spriteSheet == null)
                throw new ArgumentNullException("spriteSheet");

            RuneId = runeId;

            // Configura a renderização pixel art.
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor |
                ControlStyles.ResizeRedraw,
                true
            );

            BackColor = Color.Transparent;
            TabStop = false;

            CarregarSprites(spriteSheet, jsonMetadata);

            Name = "PuzzleRune_" + RuneId;
        }

        #endregion

        #region Load Sprites

        private void CarregarSprites(Bitmap spriteSheet,string jsonMetadata)
        {
            if (string.IsNullOrWhiteSpace(jsonMetadata))
                throw new ArgumentException("O JSON da runa está vazio.","jsonMetadata");

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            Dictionary<string, object> root = serializer.Deserialize<Dictionary<string, object>>(jsonMetadata);

            Dictionary<string, object> objeto = ObterDicionario(root, "object");

            Dictionary<string, object> tamanho = ObterDicionario(objeto, "size");

            Dictionary<string, object> spritesheet = ObterDicionario(root, "spritesheet");

            Dictionary<string, object> cellSize = ObterDicionario(spritesheet, "cell_size");

            int larguraObjeto = ObterInteiro(tamanho, "width");
            int alturaObjeto = ObterInteiro(tamanho, "height");

            int larguraFrame = ObterInteiro(cellSize, "width");
            int alturaFrame = ObterInteiro(cellSize, "height");

            object valorRows;

            if (!spritesheet.TryGetValue("rows", out valorRows))
                throw new Exception("Linhas do spritesheet não encontradas.");

            IEnumerable rows = valorRows as IEnumerable;

            if (rows == null)
                throw new Exception("Estrutura de frames inválida.");

            int linhaBase = -1;
            int linhaAnimacao = -1;
            int quantidadeFrames = 0;

            foreach (object item in rows)
            {
                Dictionary<string, object> linha = item as Dictionary<string, object>;

                if (linha == null)
                    continue;

                string tipo = Convert.ToString(linha["type"]);
                int numeroLinha = ObterInteiro(linha, "row");

                if (string.Equals(tipo, "rotations",StringComparison.OrdinalIgnoreCase))
                {
                    linhaBase = numeroLinha;
                }
                else if (string.Equals(tipo, "animation",StringComparison.OrdinalIgnoreCase))
                {
                    linhaAnimacao = numeroLinha;
                    quantidadeFrames = ObterInteiro(linha, "frame_count");
                }
            }

            if (linhaBase < 0 || linhaAnimacao < 0)
                throw new Exception("Não foi possível identificar as linhas da runa.");

            if (quantidadeFrames <= 0)
                throw new Exception("Quantidade de frames inválida.");

            // Verifica as dimensões antes de recortar.
            Rectangle areaBase = new Rectangle(
                0,
                linhaBase * alturaFrame,
                larguraFrame,
                alturaFrame
            );

            if (areaBase.Right > spriteSheet.Width || areaBase.Bottom > spriteSheet.Height)
            {
                throw new Exception("Imagem-base fora dos limites do spritesheet.");
            }

            _inactiveFrame = spriteSheet.Clone(areaBase,spriteSheet.PixelFormat);

            // Extrai os frames da animação.
            for (int i = 0; i < quantidadeFrames; i++)
            {
                Rectangle areaFrame = new Rectangle(
                    i * larguraFrame,
                    linhaAnimacao * alturaFrame,
                    larguraFrame,
                    alturaFrame
                );

                if (areaFrame.Right > spriteSheet.Width || areaFrame.Bottom > spriteSheet.Height)
                {
                    throw new Exception("Frame da animação fora dos limites.");
                }

                Bitmap frame = spriteSheet.Clone(areaFrame,spriteSheet.PixelFormat);

                _activationFrames.Add(frame);
            }

            // Define o tamanho visual do controle.
            Size = new Size(larguraObjeto, alturaObjeto);
            MinimumSize = Size;
            MaximumSize = Size;

            Invalidate();
        }

        private Dictionary<string, object> ObterDicionario(Dictionary<string, object> origem,string chave)
        {
            object valor;

            if (origem == null || !origem.TryGetValue(chave, out valor))
            {
                throw new Exception("Informação não encontrada no JSON: " + chave);
            }

            Dictionary<string, object> resultado = valor as Dictionary<string, object>;

            if (resultado == null)
                throw new Exception("Estrutura inválida no JSON: " + chave);

            return resultado;
        }

        private int ObterInteiro(Dictionary<string, object> origem,string chave)
        {
            object valor;

            if (!origem.TryGetValue(chave, out valor))
                throw new Exception("Valor não encontrado: " + chave);

            return Convert.ToInt32(valor);
        }

        #endregion

        #region Activation

        public bool StartActivation()
        {
            // Impede ativação duplicada.
            if (_state != RuneState.Inactive)
                return false;

            _state = RuneState.Activating;

            _currentFrame = 0;
            _frameElapsedMs = 0;

            Invalidate();

            return true;
        }

        public void UpdateAnimation(int elapsedMilliseconds)
        {
            if (_state != RuneState.Activating)
                return;

            if (elapsedMilliseconds <= 0)
                return;

            _frameElapsedMs += elapsedMilliseconds;

            bool frameChanged = false;

            while (_frameElapsedMs >= FrameDurationMs)
            {
                _frameElapsedMs -= FrameDurationMs;

                if (_currentFrame < _activationFrames.Count - 1)
                {
                    _currentFrame++;
                    frameChanged = true;
                }
                else
                {
                    // A animação terminou.
                    // A runa permanece no último frame.
                    _state = RuneState.Active;
                    _frameElapsedMs = 0;

                    frameChanged = true;

                    ActivationCompleted?.Invoke(this);

                    break;
                }
            }

            if (frameChanged)
                Invalidate();
        }

        #endregion

        #region Drawing

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Bitmap imagemAtual = _inactiveFrame;

            if (_state == RuneState.Activating || _state == RuneState.Active)
            {
                if (_activationFrames.Count > 0)
                {
                    imagemAtual = _activationFrames[_currentFrame];
                }
            }

            if (imagemAtual == null)
                return;

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            e.Graphics.DrawImage(
                imagemAtual,
                ClientRectangle,
                0,
                0,
                imagemAtual.Width,
                imagemAtual.Height,
                GraphicsUnit.Pixel
            );
        }

        #endregion
        
        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_inactiveFrame != null)
                {
                    _inactiveFrame.Dispose();
                    _inactiveFrame = null;
                }

                foreach (Bitmap frame in _activationFrames)
                {
                    if (frame != null)
                        frame.Dispose();
                }

                _activationFrames.Clear();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
