
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FallenWorld
{
    public class GothicQueenBoss : UserControl
    {
        #region Estados e ataques

        private enum Estado
        {
            Idle,
            Walk,
            Hurt,
            RoyalVenomSlash,
            EmeraldBladefall,
            Death
        }

        public enum TipoAtaque
        {
            RoyalVenomSlash,
            EmeraldBladefall
        }

        private Estado _estadoAtual = Estado.Idle;
        private TipoAtaque _tipoAtaqueAtual;

        #endregion

        #region Spritesheet

        private const int FrameWidth = 176;
        private const int FrameHeight = 196;

        private const int ColunasSpritesheet = 17;
        private const int LinhasSpritesheet = 13;

        private Bitmap _spritesheet;
        private readonly PictureBox _spriteBox;

        #endregion

        #region Animação

        private readonly Timer _animationTimer;

        private const int AnimationInterval = 100;

        private const int SlashImpactFrame = 7;
        private const int BladefallImpactFrame = 10;

        private int _currentFrame = 0;
        private int _currentRow = 7;
        private int _currentFrameCount = 9;

        private bool _facingRight = true;
        private bool _attackImpactTriggered = false;
        private bool _deathAnimationCompleted = false;

        public event Action<GothicQueenBoss, TipoAtaque> AttackImpact;

        public event Action<GothicQueenBoss> DeathAnimationCompleted;

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

        public GothicQueenBoss(int vidaMaxima)
        {
            if (vidaMaxima <= 0)
                throw new ArgumentOutOfRangeException(
                    "vidaMaxima",
                    "A vida máxima precisa ser maior que zero."
                );

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

            _spritesheet = new Bitmap(
                Properties.Resources.Fallen_Queen_sprite
            );

            if (_spritesheet.Width < FrameWidth * ColunasSpritesheet ||
                _spritesheet.Height < FrameHeight * LinhasSpritesheet)
            {
                _spritesheet.Dispose();
                _spritesheet = null;

                throw new ArgumentException(
                    "A spritesheet da Rainha Gótica possui dimensões inválidas."
                );
            }

            _animationTimer = new Timer
            {
                Interval = AnimationInterval
            };

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

            switch (novoEstado)
            {
                case Estado.Idle:
                    _currentRow = _facingRight ? 7 : 8;
                    _currentFrameCount = 9;
                    break;

                case Estado.Hurt:
                    _currentRow = _facingRight ? 1 : 2;
                    _currentFrameCount = 9;
                    break;

                case Estado.Death:
                    _currentRow = _facingRight ? 3 : 4;
                    _currentFrameCount = 17;
                    _deathAnimationCompleted = false;
                    break;

                case Estado.Walk:
                    _currentRow = _facingRight ? 5 : 6;
                    _currentFrameCount = 9;
                    break;

                case Estado.RoyalVenomSlash:
                    _currentRow = _facingRight ? 9 : 10;
                    _currentFrameCount = 13;
                    _attackImpactTriggered = false;
                    break;

                case Estado.EmeraldBladefall:
                    _currentRow = _facingRight ? 11 : 12;
                    _currentFrameCount = 17;
                    _attackImpactTriggered = false;
                    break;
            }

            ExibirFrameAtual();
        }

        #endregion

        #region Extração e exibição dos frames

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

                graphics.InterpolationMode =
                    System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                graphics.PixelOffsetMode =
                    System.Drawing.Drawing2D.PixelOffsetMode.Half;

                graphics.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.None;

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

            Image frameAnterior = _spriteBox.Image;

            _spriteBox.Image = novoFrame;

            frameAnterior?.Dispose();
        }

        #endregion

        #region Atualização da animação

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if ((_estadoAtual == Estado.RoyalVenomSlash ||
                 _estadoAtual == Estado.EmeraldBladefall) &&
                !_attackImpactTriggered)
            {
                int frameImpacto =
                    _estadoAtual == Estado.RoyalVenomSlash
                        ? SlashImpactFrame
                        : BladefallImpactFrame;

                if (_currentFrame == frameImpacto)
                {
                    _attackImpactTriggered = true;

                    AttackImpact?.Invoke(
                        this,
                        _tipoAtaqueAtual
                    );
                }
            }

            // Mantém a Rainha no último frame da morte.
            if (_estadoAtual == Estado.Death &&
                _currentFrame >= _currentFrameCount - 1)
            {
                if (!_deathAnimationCompleted)
                {
                    _deathAnimationCompleted = true;

                    DeathAnimationCompleted?.Invoke(this);
                }

                return;
            }

            if (_currentFrame < _currentFrameCount - 1)
            {
                _currentFrame++;

                ExibirFrameAtual();

                return;
            }

            // Hurt e ataques retornam ao Idle.
            if (_estadoAtual == Estado.Hurt ||
                _estadoAtual == Estado.RoyalVenomSlash ||
                _estadoAtual == Estado.EmeraldBladefall)
            {
                SetAnimation(Estado.Idle);
                return;
            }

            // Idle e Walk permanecem em loop.
            _currentFrame = 0;

            ExibirFrameAtual();
        }

        #endregion

        #region Movimento e comandos

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

            if (tipoAtaque == TipoAtaque.RoyalVenomSlash)
                SetAnimation(Estado.RoyalVenomSlash);
            else
                SetAnimation(Estado.EmeraldBladefall);

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
                return _estadoAtual == Estado.RoyalVenomSlash ||
                       _estadoAtual == Estado.EmeraldBladefall ||
                       _estadoAtual == Estado.Hurt ||
                       _estadoAtual == Estado.Death;
            }
        }

        #endregion

        #region Hitbox e limites visuais

        public Rectangle GetHitboxAt(int x, int y)
        {
            const int hitboxWidth = 56;
            const int hitboxHeight = 86;

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

            return Rectangle.FromLTRB(
                minX,
                minY,
                maxX + 1,
                maxY + 1
            );
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
