
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FallenWorld
{
    public class MidBoss : UserControl
    {
        #region Estados

        private enum BossState
        {
            Idle,
            Walk,
            Hurt,
            Attack1,
            Attack2,
            Death
        }

        public enum BossAttackType
        {
            Attack1,
            Attack2
        }

        private BossState _state = BossState.Idle;

        private BossAttackType _currentAttack;

        #endregion

        #region Spritesheet

        private const int FrameWidth = 132;
        private const int FrameHeight = 132;

        private Bitmap _spriteSheet;

        private readonly PictureBox _spriteBox;

        #endregion

        #region Animation

        private readonly Timer _animationTimer;

        private int _currentFrame = 0;
        private int _currentRow = 7;
        private int _frameCount = 9;

        private bool _facingRight = true;

        // Quadro inicial do impacto dos ataques.
        private const int AttackImpactFrame = 4;

        private bool _attackImpactTriggered = false;

        public event Action<MidBoss, BossAttackType> AttackImpact;

        #endregion

        #region Life and Damage

        private const int VidaMaxima = 250;

        private int _vidaAtual = VidaMaxima;

        public int VidaAtual
        {
            get { return _vidaAtual; }
        }

        public int DanoAtaque1
        {
            get { return 15; }
        }

        public int DanoAtaque2
        {
            get { return 20; }
        }

        public int DanoContato
        {
            get { return 10; }
        }

        #endregion

        #region Constructor

        public MidBoss()
        {
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

            // Carrega o spritesheet do Mid-Boss.
            _spriteSheet = new Bitmap(Properties.Resources.MidBoss_Spritesheet);

            // Verifica as dimensões esperadas.
            if (_spriteSheet.Width < 2244 || _spriteSheet.Height < 1716)
            {
                throw new ArgumentException(
                    "O spritesheet do Mid-Boss possui dimensões inválidas."
                );
            }

            // Configura o temporizador das animações.
            _animationTimer = new Timer();

            _animationTimer.Interval = 100;
            _animationTimer.Tick += AnimationTimer_Tick;

            SetAnimation(BossState.Idle);

            ExibirFrameAtual();

            _animationTimer.Start();
        }

        #endregion

        #region Animation Configuration

        private void SetAnimation(BossState state)
        {
            _state = state;
            _currentFrame = 0;

            if (state == BossState.Attack1 || state == BossState.Attack2)
            {
                _attackImpactTriggered = false;
            }

            switch (state)
            {
                case BossState.Death:
                    _currentRow = _facingRight ? 1 : 2;
                    _frameCount = 17;
                    break;

                case BossState.Walk:
                    _currentRow = _facingRight ? 3 : 4;
                    _frameCount = 9;
                    break;

                case BossState.Hurt:
                    _currentRow = _facingRight ? 5 : 6;
                    _frameCount = 17;
                    break;

                case BossState.Idle:
                    _currentRow = _facingRight ? 7 : 8;
                    _frameCount = 9;
                    break;

                case BossState.Attack2:
                    _currentRow = _facingRight ? 9 : 10;
                    _frameCount = 9;
                    break;

                case BossState.Attack1:
                    _currentRow = _facingRight ? 11 : 12;
                    _frameCount = 9;
                    break;
            }

            ExibirFrameAtual();
        }

        #endregion

        #region Frame Extraction

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

            using (Graphics g = Graphics.FromImage(frame))
            {
                g.DrawImage(
                    _spriteSheet,
                    new Rectangle(0, 0, FrameWidth, FrameHeight),
                    sourceRectangle,
                    GraphicsUnit.Pixel
                );
            }

            return frame;
        }

        private void ExibirFrameAtual()
        {
            Bitmap novoFrame = GetFrame(_currentRow,_currentFrame);

            Image frameAnterior = _spriteBox.Image;

            _spriteBox.Image = novoFrame;

            frameAnterior?.Dispose();
        }

        #endregion

        #region Animation Update

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // Registra o impacto do ataque uma única vez.
            if ((_state == BossState.Attack1 ||
                 _state == BossState.Attack2) &&
                !_attackImpactTriggered &&
                _currentFrame == AttackImpactFrame)
            {
                _attackImpactTriggered = true;

                AttackImpact?.Invoke(this, _currentAttack);
            }

            // A animação de morte permanece no último quadro.
            if (_state == BossState.Death && _currentFrame >= _frameCount - 1)
            {
                return;
            }

            if (_currentFrame < _frameCount - 1)
            {
                _currentFrame++;

                ExibirFrameAtual();

                return;
            }

            // Ataque e Hurt retornam ao Idle.
            if (_state == BossState.Attack1 ||
                _state == BossState.Attack2 ||
                _state == BossState.Hurt)
            {
                SetAnimation(BossState.Idle);

                return;
            }

            // Idle e Walk permanecem em loop.
            _currentFrame = 0;

            ExibirFrameAtual();
        }

        #endregion

        #region Movement Commands

        public void SetIdle(bool facingRight)
        {
            if (IsPerformingAction)
                return;

            bool directionChanged = _facingRight != facingRight;

            _facingRight = facingRight;

            if (_state != BossState.Idle || directionChanged)
                SetAnimation(BossState.Idle);
        }

        public void SetWalk(bool facingRight)
        {
            if (IsPerformingAction)
                return;

            bool directionChanged = _facingRight != facingRight;

            _facingRight = facingRight;

            if (_state != BossState.Walk || directionChanged)
                SetAnimation(BossState.Walk);
        }

        #endregion

        #region Random Attacks

        private static readonly Random _random = new Random();

        public bool SetRandomAttack(bool facingRight)
        {
            if (IsPerformingAction)
                return false;

            _facingRight = facingRight;

            // Escolhe aleatoriamente um dos dois ataques.
            _currentAttack = _random.Next(0, 2) == 0
                ? BossAttackType.Attack1
                : BossAttackType.Attack2;

            if (_currentAttack == BossAttackType.Attack1)
            {
                SetAnimation(BossState.Attack1);
            }
            else
            {
                SetAnimation(BossState.Attack2);
            }

            return true;
        }

        #endregion

        #region Damage and Death

        public void SetHurt()
        {
            if (IsDead)
                return;

            SetAnimation(BossState.Hurt);
        }

        public void SetDeath()
        {
            if (IsDead)
                return;

            SetAnimation(BossState.Death);
        }

        public void ReceberDano(int dano)
        {
            if (IsDead || dano <= 0)
                return;

            _vidaAtual = Math.Max(0, _vidaAtual - dano);

            if (_vidaAtual <= 0)
            {
                SetDeath();
            }
            else
            {
                SetHurt();
            }
        }

        #endregion

        #region Boss Properties

        public bool FacingRight
        {
            get { return _facingRight; }
        }

        public bool IsDead
        {
            get { return _state == BossState.Death; }
        }

        public bool IsPerformingAction
        {
            get
            {
                return _state == BossState.Attack1 ||
                       _state == BossState.Attack2 ||
                       _state == BossState.Hurt ||
                       _state == BossState.Death;
            }
        }

        public BossAttackType CurrentAttack
        {
            get { return _currentAttack; }
        }

        #endregion

        #region Hitbox

        public Rectangle GetHitboxAt(int x, int y)
        {
            int hitboxWidth = 62;
            int hitboxHeight = 80;

            return new Rectangle(
                x + (Width - hitboxWidth) / 2,
                y + Height - hitboxHeight,
                hitboxWidth,
                hitboxHeight
            );
        }

        #endregion

        #region Visible Sprite Bounds

        public Rectangle GetVisibleSpriteBounds()
        {
            Bitmap imagem = _spriteBox.Image as Bitmap;

            if (imagem == null)
                return ClientRectangle;

            int minX = imagem.Width;
            int minY = imagem.Height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < imagem.Height; y++)
            {
                for (int x = 0; x < imagem.Width; x++)
                {
                    if (imagem.GetPixel(x, y).A > 10)
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
                _animationTimer.Stop();
                _animationTimer.Tick -= AnimationTimer_Tick;
                _animationTimer.Dispose();

                if (_spriteBox.Image != null)
                {
                    _spriteBox.Image.Dispose();
                    _spriteBox.Image = null;
                }

                if (_spriteSheet != null)
                {
                    _spriteSheet.Dispose();
                    _spriteSheet = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
