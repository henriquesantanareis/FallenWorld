
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace FallenWorld
{
    public class GuardianRune : UserControl
    {
        #region Estados

        public enum RuneState
        {
            Inactive,
            Activating,
            Active,
            Error,
            Successful
        }

        private RuneState _state = RuneState.Inactive;

        public RuneState State
        {
            get { return _state; }
        }

        public bool IsActivated
        {
            get
            {
                return _state == RuneState.Active || _state == RuneState.Successful;
            }
        }

        public bool IsAnimating
        {
            get
            {
                return !_animationFinished &&
                       (_state == RuneState.Activating ||
                        _state == RuneState.Error ||
                        _state == RuneState.Successful);
            }
        }

        public int RuneId { get; private set; }

        public event Action<GuardianRune> ActivationCompleted;
        public event Action<GuardianRune> ErrorAnimationCompleted;
        public event Action<GuardianRune> SuccessAnimationCompleted;

        #endregion

        #region Sprites

        private Bitmap _inactiveFrame;

        private readonly List<Bitmap> _activationFrames = new List<Bitmap>();

        private readonly List<Bitmap> _errorFrames = new List<Bitmap>();

        private readonly List<Bitmap> _successFrames = new List<Bitmap>();

        private int _currentFrame;
        private int _frameElapsedMs;
        private bool _animationFinished;

        private const int FrameDurationMs = 100;

        #endregion

        #region Informações das animações

        private class AnimationInfo
        {
            public int Row;
            public int FrameCount;
        }

        #endregion

        #region Constructor

        public GuardianRune(int runeId,Bitmap spriteSheet,string jsonMetadata)
        {
            if (runeId < 1 || runeId > 3)
                throw new ArgumentOutOfRangeException("runeId");

            if (spriteSheet == null)
                throw new ArgumentNullException("spriteSheet");

            RuneId = runeId;

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

            Name = "GuardianRune_" + RuneId;
        }

        #endregion

        #region Carregamento dos sprites

        private void CarregarSprites(Bitmap spriteSheet,string jsonMetadata)
        {
            if (string.IsNullOrWhiteSpace(jsonMetadata))
                throw new ArgumentException("O JSON da runa está vazio.","jsonMetadata");

            JavaScriptSerializer serializer = new JavaScriptSerializer();

            Dictionary<string, object> root =
                serializer.Deserialize<Dictionary<string, object>>(jsonMetadata);

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
                throw new Exception("Estrutura de animações inválida.");

            int linhaBase = -1;

            AnimationInfo ativacao = null;
            AnimationInfo erro = null;
            AnimationInfo sucesso = null;

            foreach (object item in rows)
            {
                Dictionary<string, object> linha = item as Dictionary<string, object>;

                if (linha == null)
                    continue;

                string tipo = Convert.ToString(linha["type"]);

                int numeroLinha = ObterInteiro(linha, "row");

                if (string.Equals(tipo,"rotations",StringComparison.OrdinalIgnoreCase))
                {
                    linhaBase = numeroLinha;
                }
                else if (string.Equals(tipo,"animation",StringComparison.OrdinalIgnoreCase))
                {
                    string nomeAnimacao = Convert.ToString(linha["animation"]);

                    AnimationInfo info = new AnimationInfo();

                    info.Row = numeroLinha;
                    info.FrameCount = ObterInteiro(linha, "frame_count");

                    if (nomeAnimacao.IndexOf("ativ",StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ativacao = info;
                    }
                    else if (nomeAnimacao.IndexOf("error",StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        erro = info;
                    }
                    else if (nomeAnimacao.IndexOf("succes",StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        sucesso = info;
                    }
                }
            }

            if (linhaBase < 0 || ativacao == null || erro == null || sucesso == null)
            {
                throw new Exception("Não foi possível identificar todas as animações da runa.");
            }

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

            _activationFrames.AddRange(ExtrairFrames(spriteSheet, ativacao,larguraFrame, alturaFrame));

            _errorFrames.AddRange(ExtrairFrames(spriteSheet, erro,larguraFrame, alturaFrame));

            _successFrames.AddRange(ExtrairFrames(spriteSheet, sucesso,larguraFrame, alturaFrame));

            Size = new Size(larguraObjeto, alturaObjeto);

            MinimumSize = Size;
            MaximumSize = Size;

            Invalidate();
        }

        private List<Bitmap> ExtrairFrames(
            Bitmap spriteSheet,
            AnimationInfo animacao,
            int larguraFrame,
            int alturaFrame)
        {
            List<Bitmap> frames = new List<Bitmap>();

            if (animacao.FrameCount <= 0)
                throw new Exception("Quantidade de frames inválida.");

            for (int i = 0; i < animacao.FrameCount; i++)
            {
                Rectangle area = new Rectangle(
                    i * larguraFrame,
                    animacao.Row * alturaFrame,
                    larguraFrame,
                    alturaFrame
                );

                if (area.Right > spriteSheet.Width || area.Bottom > spriteSheet.Height)
                {
                    throw new Exception("Frame fora dos limites do spritesheet.");
                }

                Bitmap frame = spriteSheet.Clone(area,spriteSheet.PixelFormat);

                frames.Add(frame);
            }

            return frames;
        }

        private Dictionary<string, object> ObterDicionario(Dictionary<string, object> origem,string chave)
        {
            object valor;

            if (origem == null || !origem.TryGetValue(chave, out valor))
            {
                throw new Exception("Informação não encontrada: " + chave);
            }

            Dictionary<string, object> resultado = valor as Dictionary<string, object>;

            if (resultado == null)
                throw new Exception("Estrutura inválida: " + chave);

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

        #region Iniciar animações

        public bool StartActivation()
        {
            if (_state != RuneState.Inactive)
                return false;

            return IniciarAnimacao(RuneState.Activating);
        }

        public bool StartError()
        {
            if (_state == RuneState.Error)
                return false;

            return IniciarAnimacao(RuneState.Error);
        }

        public bool StartSuccess()
        {
            if (_state != RuneState.Active)
                return false;

            return IniciarAnimacao(RuneState.Successful);
        }

        private bool IniciarAnimacao(RuneState novoEstado)
        {
            _state = novoEstado;

            _currentFrame = 0;
            _frameElapsedMs = 0;
            _animationFinished = false;

            Invalidate();

            return true;
        }

        #endregion

        #region Atualização das animações

        public void UpdateAnimation(int elapsedMilliseconds)
        {
            if (_animationFinished ||
                elapsedMilliseconds <= 0)
                return;

            List<Bitmap> frames = ObterFramesAtuais();

            if (frames == null || frames.Count == 0)
                return;

            _frameElapsedMs += elapsedMilliseconds;

            bool frameChanged = false;

            while (_frameElapsedMs >= FrameDurationMs)
            {
                _frameElapsedMs -= FrameDurationMs;

                if (_currentFrame < frames.Count - 1)
                {
                    _currentFrame++;
                    frameChanged = true;
                }
                else
                {
                    _animationFinished = true;
                    _frameElapsedMs = 0;

                    frameChanged = true;

                    if (_state == RuneState.Activating)
                    {
                        _state = RuneState.Active;

                        ActivationCompleted?.Invoke(this);
                    }
                    else if (_state == RuneState.Error)
                    {
                        _state = RuneState.Inactive;
                        _currentFrame = 0;

                        ErrorAnimationCompleted?.Invoke(this);
                    }
                    else if (_state == RuneState.Successful)
                    {
                        SuccessAnimationCompleted?.Invoke(this);
                    }

                    break;
                }
            }

            if (frameChanged)
                Invalidate();
        }

        private List<Bitmap> ObterFramesAtuais()
        {
            switch (_state)
            {
                case RuneState.Activating:
                case RuneState.Active:
                    return _activationFrames;

                case RuneState.Error:
                    return _errorFrames;

                case RuneState.Successful:
                    return _successFrames;

                default:
                    return null;
            }
        }

        #endregion

        #region Reinicialização

        public void ResetToInactive()
        {
            _state = RuneState.Inactive;

            _currentFrame = 0;
            _frameElapsedMs = 0;
            _animationFinished = true;

            Invalidate();
        }

        #endregion

        #region Desenho

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Bitmap imagemAtual = _inactiveFrame;

            List<Bitmap> frames = ObterFramesAtuais();

            if (frames != null && frames.Count > 0)
            {
                if (_currentFrame >= 0 &&
                    _currentFrame < frames.Count)
                {
                    imagemAtual = frames[_currentFrame];
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

                LiberarFrames(_activationFrames);
                LiberarFrames(_errorFrames);
                LiberarFrames(_successFrames);
            }

            base.Dispose(disposing);
        }

        private void LiberarFrames(List<Bitmap> frames)
        {
            foreach (Bitmap frame in frames)
            {
                if (frame != null)
                    frame.Dispose();
            }

            frames.Clear();
        }

        #endregion
    }
}
