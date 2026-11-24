using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FallenWorld
{
    /// <summary>
    /// Representa o Lich das Chamas Negras.
    /// Esta classe controla somente o visual, as animações e o estado básico
    /// de vida do chefe. A inteligência artificial e o projétil serão
    /// integrados posteriormente pelo FormGame.
    /// </summary>
    public class LichBoss : UserControl
    {
        #region Estados e ataques

        private enum Estado
        {
            Idle,
            Walk,
            Hurt,
            DarkFlameBurst,
            DarkFireBall,
            Death
        }

        public enum TipoAtaque
        {
            DarkFlameBurst,
            DarkFireBall
        }

        private Estado _estadoAtual = Estado.Idle;
        private TipoAtaque _tipoAtaqueAtual;

        #endregion

        #region Spritesheet

        // Dimensões reais de cada célula, conforme Litch.json.
        private const int FrameWidth = 196;
        private const int FrameHeight = 152;
        private const int ColunasSpritesheet = 9;
        private const int LinhasSpritesheet = 13;

        private Bitmap _spritesheet;
        private readonly PictureBox _spriteBox;

        #endregion

        #region Animação

        private readonly Timer _animationTimer;
        private const int AnimationInterval = 100;
        private const int FrameCount = 9;
        private const int AttackImpactFrame = 4;

        private int _currentFrame;
        private int _currentRow = 1;
        private bool _facingRight = true;
        private bool _attackImpactTriggered;
        private bool _deathAnimationCompleted;

        // Controle do efeito de queimadura.
        private int _burnDamagePerTick = 0;
        private int _burnDurationRemaining = 0;
        private int _burnTickRemaining = 0;
        private int _burnTickInterval = 60;
        private int _burnVisualTick = 0;

        /// <summary>
        /// Disparado uma única vez no quadro de impacto de cada ataque.
        /// O FormGame usará este evento para executar o ataque correspondente.
        /// </summary>
        public event Action<LichBoss, TipoAtaque> AttackImpact;

        /// <summary>
        /// Disparado uma única vez quando a animação de morte termina.
        /// </summary>
        public event Action<LichBoss> DeathAnimationCompleted;

        #endregion

        #region Vida

        private int _vidaAtual;

        public int VidaMaxima { get; private set; }

        public int VidaAtual
        {
            get { return _vidaAtual; }
        }

        #endregion

        #region Construtor

        public LichBoss(int vidaMaxima)
        {
            if (vidaMaxima <= 0)
                throw new ArgumentOutOfRangeException("vidaMaxima", "A vida máxima precisa ser maior que zero.");

            VidaMaxima = vidaMaxima;
            _vidaAtual = vidaMaxima;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true
            );

            BackColor = Color.Transparent;
            Size = new Size(FrameWidth, FrameHeight);

            _spriteBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.Normal,
                TabStop = false
            };

            Controls.Add(_spriteBox);

            // Nome real registrado em Properties/Resources.resx.
            _spritesheet = new Bitmap(Properties.Resources.LitchBoss_sprite);

            if (_spritesheet.Width < FrameWidth * ColunasSpritesheet ||
                _spritesheet.Height < FrameHeight * LinhasSpritesheet)
            {
                _spritesheet.Dispose();
                _spritesheet = null;

                throw new ArgumentException(
                    "O spritesheet do Lich possui dimensões diferentes das descritas em Litch.json."
                );
            }

            _animationTimer = new Timer();
            _animationTimer.Interval = AnimationInterval;
            _animationTimer.Tick += AnimationTimer_Tick;

            SetAnimation(Estado.Idle);
            _animationTimer.Start();
        }

        #endregion

        #region Configuração das animações

        private void SetAnimation(Estado novoEstado)
        {
            _estadoAtual = novoEstado;
            _currentFrame = 0;

            if (novoEstado == Estado.DarkFlameBurst || novoEstado == Estado.DarkFireBall)
            {
                _attackImpactTriggered = false;
            }

            switch (novoEstado)
            {
                case Estado.Idle:
                    _currentRow = _facingRight ? 1 : 2;
                    break;

                case Estado.Death:
                    _currentRow = _facingRight ? 3 : 4;
                    _deathAnimationCompleted = false;
                    break;

                case Estado.DarkFlameBurst:
                    _currentRow = _facingRight ? 5 : 6;
                    break;

                case Estado.Walk:
                    _currentRow = _facingRight ? 7 : 8;
                    break;

                case Estado.DarkFireBall:
                    _currentRow = _facingRight ? 9 : 10;
                    break;

                case Estado.Hurt:
                    _currentRow = _facingRight ? 11 : 12;
                    break;
            }

            ExibirFrameAtual();
        }

        #endregion

        #region Extração e exibição dos quadros

        private Bitmap GetFrame(int row, int column)
        {
            Rectangle sourceRectangle = new Rectangle(
                column * FrameWidth,
                row * FrameHeight,
                FrameWidth,
                FrameHeight
            );

            Bitmap frame = new Bitmap(
                FrameWidth,
                FrameHeight,
                PixelFormat.Format32bppArgb
            );

            using (Graphics graphics = Graphics.FromImage(frame))
            {
                graphics.Clear(Color.Transparent);
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                graphics.SmoothingMode = SmoothingMode.None;

                graphics.DrawImage(
                    _spritesheet,
                    new Rectangle(0, 0, FrameWidth, FrameHeight),
                    sourceRectangle,
                    GraphicsUnit.Pixel
                );
            }

            return frame;
        }

        private void ExibirFrameAtual()
        {
            Bitmap novoFrame = GetFrame(_currentRow, _currentFrame);

            // Desenha as chamas enquanto o Lich estiver queimando.
            if (IsBurning && !IsDead)
            {
                DesenharEfeitoQueimadura(novoFrame);
            }

            Image frameAnterior = _spriteBox.Image;
            _spriteBox.Image = novoFrame;
            frameAnterior?.Dispose();
        }

        private void DesenharEfeitoQueimadura(Bitmap frame)
        {
            int fase = _burnVisualTick % 4;
            _burnVisualTick++;

            using (Graphics g = Graphics.FromImage(frame))
            using (SolidBrush sombra = new SolidBrush(Color.FromArgb(100, 120, 25, 5)))
            using (SolidBrush fogo = new SolidBrush(Color.FromArgb(220, 240, 75, 15)))
            using (SolidBrush brasa = new SolidBrush(Color.FromArgb(240, 255, 185, 45)))
            {
                g.SmoothingMode = SmoothingMode.None;

                // Chamas à esquerda.
                g.FillRectangle(sombra, 27, 78 + fase, 10, 18);
                g.FillRectangle(fogo, 30, 80 + fase, 6, 13);
                g.FillRectangle(brasa, 32, 83 + fase, 3, 6);

                // Chamas à direita.
                g.FillRectangle(sombra, 159, 78 - fase, 10, 18);
                g.FillRectangle(fogo, 162, 80 - fase, 6, 13);
                g.FillRectangle(brasa, 164, 83 - fase, 3, 6);

                // Brasas próximas aos pés.
                g.FillRectangle(fogo, 55 + fase, 135, 5, 5);
                g.FillRectangle(brasa, 56 + fase, 136, 2, 2);

                g.FillRectangle(fogo, 135 - fase, 130, 5, 5);
                g.FillRectangle(brasa, 136 - fase, 131, 2, 2);
            }
        }

        #endregion

        #region Atualização da animação

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // O impacto do ataque é comunicado uma única vez no quadro definido.
            if ((_estadoAtual == Estado.DarkFlameBurst ||
                 _estadoAtual == Estado.DarkFireBall) &&
                !_attackImpactTriggered &&
                _currentFrame == AttackImpactFrame)
            {
                _attackImpactTriggered = true;
                AttackImpact?.Invoke(this, _tipoAtaqueAtual);
            }

            // A morte permanece no último quadro e avisa o FormGame uma vez.
            if (_estadoAtual == Estado.Death && _currentFrame >= FrameCount - 1)
            {
                if (!_deathAnimationCompleted)
                {
                    _deathAnimationCompleted = true;
                    DeathAnimationCompleted?.Invoke(this);
                }

                return;
            }

            if (_currentFrame < FrameCount - 1)
            {
                _currentFrame++;
                ExibirFrameAtual();
                return;
            }

            // Hurt e ataques retornam ao Idle depois de terminar.
            if (_estadoAtual == Estado.Hurt ||
                _estadoAtual == Estado.DarkFlameBurst ||
                _estadoAtual == Estado.DarkFireBall)
            {
                SetAnimation(Estado.Idle);
                return;
            }

            // Idle e Walk permanecem em loop.
            _currentFrame = 0;
            ExibirFrameAtual();
        }

        #endregion

        #region Comandos de movimento e animação

        public void SetIdle(bool facingRight)
        {
            if (IsPerformingAction)
                return;

            bool directionChanged = _facingRight != facingRight;
            _facingRight = facingRight;

            if (_estadoAtual != Estado.Idle || directionChanged)
                SetAnimation(Estado.Idle);
        }

        public void SetWalk(bool facingRight)
        {
            if (IsPerformingAction)
                return;

            bool directionChanged = _facingRight != facingRight;
            _facingRight = facingRight;

            if (_estadoAtual != Estado.Walk || directionChanged)
                SetAnimation(Estado.Walk);
        }

        public bool SetAttack(TipoAtaque tipoAtaque, bool facingRight)
        {
            if (IsPerformingAction)
                return false;

            _facingRight = facingRight;
            _tipoAtaqueAtual = tipoAtaque;

            if (tipoAtaque == TipoAtaque.DarkFlameBurst)
                SetAnimation(Estado.DarkFlameBurst);
            else
                SetAnimation(Estado.DarkFireBall);

            return true;
        }

        #endregion

        #region Dano e morte

        public void SetHurt()
        {
            if (IsDead)
                return;

            SetAnimation(Estado.Hurt);
        }

        public void SetDeath()
        {
            if (IsDead)
                return;

            SetAnimation(Estado.Death);
        }

        public void ReceberDano(int dano)
        {
            if (IsDead || dano <= 0)
                return;

            _vidaAtual = Math.Max(0, _vidaAtual - dano);

            if (_vidaAtual <= 0)
                SetDeath();
            else
                SetHurt();
        }

        public bool IsBurning
        {
            get { return _burnDurationRemaining > 0; }
        }

        public void AplicarQueimadura(int danoPorTick, int duracaoTicks, int intervaloTicks)
        {
            if (IsDead || danoPorTick <= 0 || duracaoTicks <= 0 || intervaloTicks <= 0)
            {
                return;
            }

            // Renova a queimadura em vez de acumular efeitos.
            _burnDamagePerTick = danoPorTick;
            _burnDurationRemaining = duracaoTicks;
            _burnTickInterval = intervaloTicks;
            _burnTickRemaining = intervaloTicks;
        }

        public void AtualizarQueimadura()
        {
            if (IsDead || _burnDurationRemaining <= 0)
                return;

            _burnDurationRemaining--;
            _burnTickRemaining--;

            if (_burnTickRemaining > 0)
                return;

            _burnTickRemaining = _burnTickInterval;

            _vidaAtual = Math.Max(
                0,
                _vidaAtual - _burnDamagePerTick
            );

            if (_vidaAtual <= 0)
            {
                _burnDurationRemaining = 0;
                SetDeath();
            }
        }

        #endregion

        #region Propriedades do chefe

        public bool FacingRight
        {
            get { return _facingRight; }
        }

        public bool IsDead
        {
            get { return _estadoAtual == Estado.Death; }
        }

        public bool IsPerformingAction
        {
            get
            {
                return _estadoAtual == Estado.DarkFlameBurst ||
                       _estadoAtual == Estado.DarkFireBall ||
                       _estadoAtual == Estado.Hurt ||
                       _estadoAtual == Estado.Death;
            }
        }

        #endregion

        #region Hitbox e limites visuais

        public Rectangle GetHitboxAt(int x, int y)
        {
            const int hitboxWidth = 64;
            const int hitboxHeight = 90;

            return new Rectangle(
                x + (Width - hitboxWidth) / 2,
                y + Height - hitboxHeight,
                hitboxWidth,
                hitboxHeight
            );
        }

        public Rectangle GetVisibleSpriteBounds()
        {
            Bitmap image = _spriteBox.Image as Bitmap;

            if (image == null)
                return ClientRectangle;

            int minX = image.Width;
            int minY = image.Height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (image.GetPixel(x, y).A > 10)
                    {
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }

            if (maxX < 0 || maxY < 0)
                return ClientRectangle;

            return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_animationTimer != null)
                {
                    _animationTimer.Stop();
                    _animationTimer.Tick -= AnimationTimer_Tick;
                    _animationTimer.Dispose();
                }

                if (_spriteBox != null && _spriteBox.Image != null)
                {
                    _spriteBox.Image.Dispose();
                    _spriteBox.Image = null;
                }

                if (_spritesheet != null)
                {
                    _spritesheet.Dispose();
                    _spritesheet = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
