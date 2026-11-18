
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
        private bool _deathAnimationCompleted = false;

        // Disparado quando a animação de morte termina.
        public event Action<MidBoss> DeathAnimationCompleted;
        #endregion

        #region Life and Damage

        private const int VidaMaxima = 100;

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

        #region Poison and Burn

        // Controle do veneno.
        private int _poisonDamagePerTick = 0;
        private int _poisonDurationRemaining = 0;
        private int _poisonTickRemaining = 0;
        private int _poisonTickInterval = 60;

        // Controle da queimadura.
        private int _burnDamagePerTick = 0;
        private int _burnDurationRemaining = 0;
        private int _burnTickRemaining = 0;
        private int _burnTickInterval = 60;

        // Animação visual dos efeitos.
        private int _poisonVisualTick = 0;
        private int _burnVisualTick = 0;

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

            // Desenha as partículas verdes do veneno.
            if (IsPoisoned && !IsDead)
            {
                DesenharEfeitoVeneno(novoFrame);
            }

            // Desenha as chamas da queimadura.
            if (IsBurning && !IsDead)
            {
                DesenharEfeitoFogo(novoFrame);
            }

            Image frameAnterior = _spriteBox.Image;

            _spriteBox.Image = novoFrame;

            frameAnterior?.Dispose();
        }

        private void DesenharEfeitoVeneno(Bitmap frame)
        {
            int fase = _poisonVisualTick % 4;
            _poisonVisualTick++;

            using (Graphics g = Graphics.FromImage(frame))
            using (SolidBrush sombra = new SolidBrush(Color.FromArgb(90, 25, 150, 35)))
            using (SolidBrush veneno = new SolidBrush(Color.FromArgb(210, 55, 190, 45)))
            using (SolidBrush brilho = new SolidBrush(Color.FromArgb(235, 150, 240, 65)))
            {
                g.SmoothingMode = SmoothingMode.None;

                g.FillRectangle(sombra, 17, 46 + fase, 6, 6);
                g.FillRectangle(veneno, 19, 47 + fase, 4, 4);
                g.FillRectangle(brilho, 20, 48 + fase, 2, 2);

                g.FillRectangle(sombra, 105, 56 - fase, 6, 6);
                g.FillRectangle(veneno, 106, 57 - fase, 4, 4);
                g.FillRectangle(brilho, 107, 58 - fase, 2, 2);

                g.FillRectangle(sombra, 30 + fase, 91, 6, 6);
                g.FillRectangle(veneno, 31 + fase, 92, 4, 4);

                g.FillRectangle(veneno, 86 - fase, 34, 4, 4);
                g.FillRectangle(brilho, 87 - fase, 35, 2, 2);
            }
        }

        private void DesenharEfeitoFogo(Bitmap frame)
        {
            int fase = _burnVisualTick % 4;
            _burnVisualTick++;

            using (Graphics g = Graphics.FromImage(frame))
            using (SolidBrush sombra = new SolidBrush(Color.FromArgb(100, 120, 25, 5)))
            using (SolidBrush fogo = new SolidBrush(Color.FromArgb(220, 240, 75, 15)))
            using (SolidBrush brasa = new SolidBrush(Color.FromArgb(240, 255, 185, 45)))
            {
                g.SmoothingMode = SmoothingMode.None;

                // Chamas nas laterais.
                g.FillRectangle(sombra, 17, 57 + fase, 7, 10);
                g.FillRectangle(fogo, 19, 58 + fase, 4, 7);
                g.FillRectangle(brasa, 20, 60 + fase, 2, 3);

                g.FillRectangle(sombra, 106, 54 - fase, 7, 10);
                g.FillRectangle(fogo, 108, 56 - fase, 4, 7);
                g.FillRectangle(brasa, 109, 58 - fase, 2, 3);

                // Brasas próximas aos pés.
                g.FillRectangle(fogo, 36 + fase, 104, 5, 5);
                g.FillRectangle(brasa, 37 + fase, 105, 3, 3);

                g.FillRectangle(fogo, 91 - fase, 108, 5, 5);
                g.FillRectangle(brasa, 92 - fase, 109, 3, 3);
            }
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
                // Avisa apenas uma vez que a animação terminou.
                if (!_deathAnimationCompleted)
                {
                    _deathAnimationCompleted = true;
                    DeathAnimationCompleted?.Invoke(this);
                }

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


        #region Poison and Burn Effects

        public bool IsPoisoned
        {
            get { return _poisonDurationRemaining > 0; }
        }

        public bool IsBurning
        {
            get { return _burnDurationRemaining > 0; }
        }

        public void AplicarVeneno(int danoPorTick, int duracaoTicks, int intervaloTicks)
        {
            if (IsDead || danoPorTick <= 0 ||
                duracaoTicks <= 0 || intervaloTicks <= 0)
            {
                return;
            }

            // Renova o efeito em vez de acumular venenos.
            _poisonDamagePerTick = danoPorTick;
            _poisonDurationRemaining = duracaoTicks;
            _poisonTickInterval = intervaloTicks;
            _poisonTickRemaining = intervaloTicks;
        }

        public void AtualizarVeneno()
        {
            if (IsDead || _poisonDurationRemaining <= 0)
                return;

            _poisonDurationRemaining--;
            _poisonTickRemaining--;

            if (_poisonTickRemaining > 0)
                return;

            _poisonTickRemaining = _poisonTickInterval;

            _vidaAtual = Math.Max(
                0,
                _vidaAtual - _poisonDamagePerTick
            );

            // Não reinicia a animação Hurt a cada aplicação.
            if (_vidaAtual <= 0)
            {
                _poisonDurationRemaining = 0;
                SetDeath();
            }
        }

        public void AplicarQueimadura(int danoPorTick, int duracaoTicks, int intervaloTicks)
        {
            if (IsDead || danoPorTick <= 0 ||
                duracaoTicks <= 0 || intervaloTicks <= 0)
            {
                return;
            }

            // Renova a queimadura existente.
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
