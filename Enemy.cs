using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FallenWorld.Player;
using static FallenWorld.Enemy;

namespace FallenWorld
{
    public partial class Enemy : UserControl
    {

        #region Global Variables

        #region StateEnemy
        public enum StateEnemy
        {
            Idle,
            Walk,
            Attack,
            Hurt,
            Parry,
            Death
        }
        #endregion

        #region Variáveis da spritesheet
        // Spritesheet do orc, que contém todas as animações.
        private Bitmap _spriteSheet;
        // Dimensões de cada frame
        private int FrameWidth = 112;
        private int FrameHeight = 112;
        #endregion

        #region Variáveis da animação
        // Controle da animação
        private StateEnemy _state = StateEnemy.Idle;
        private int _currentFrame = 0;
        private int _currentRow = 0;
        private int _frameCount = 0;
        private bool _facingRight = true;
        // Controla a animação visual das partículas de veneno.
        private int _poisonVisualTick = 0;
        #endregion

        #region Variáveis de vida
        // Vida do inimigo
        private const int VidaMaxima = 50;
        private int _vidaAtual = VidaMaxima;
        public int VidaAtual
        {
            get { return _vidaAtual; }
        }
        #endregion

        #region Variáveis de veneno e queimadura
        // Controle do efeito de veneno.
        private int _poisonDamagePerTick = 0;
        private int _poisonDurationRemaining = 0;
        private int _poisonTickRemaining = 0;
        private int _poisonTickInterval = 60;
        // Controle do efeito de queimadura.
        private int _burnDamagePerTick = 0;
        private int _burnDurationRemaining = 0;
        private int _burnTickRemaining = 0;
        private int _burnTickInterval = 60;
        private int _burnVisualTick = 0;
        #endregion

        #region Eventos
        // Avisa quando o golpe do Orc chega ao ponto de impacto.
        public event Action<Enemy> AttackImpact;
        private bool _attackImpactTriggered = false;
        #endregion

        #endregion

        #region Constructor
        public Enemy()
        {
            InitializeComponent();

            Size = new Size(FrameWidth, FrameHeight);

            Enemy_Load();
            SetAnimation(StateEnemy.Idle);

            // Carrega o primeiro frame da animação Idle.
            if (_spriteSheet != null && _frameCount > 0)
            {
                PBX_Enemy.Image = GetFrame(_currentRow,0);
            }
        }
        #endregion

        #region SpriteSheet Configuration
        private void Enemy_Load()
        {
            // Carrega a spritesheet do inimigo
            _spriteSheet = new Bitmap(Properties.Resources.Orc_Spritesheet);
        }
        #endregion

        #region Animations
        private void SetAnimation(StateEnemy state)
        {
            if (_spriteSheet == null)
                return;

            _state = state;
            _currentFrame = 0;

            // Permite um único impacto para cada novo ataque.
            if (state == StateEnemy.Attack)
                _attackImpactTriggered = false;

            switch (_state)
            {
                case StateEnemy.Idle:
                    _currentRow = _facingRight ? 9 : 10;
                    _frameCount = 9;
                    break;

                case StateEnemy.Walk:
                    _currentRow = _facingRight ? 5 : 6;
                    _frameCount = 9;
                    break;

                case StateEnemy.Attack:
                    _currentRow = _facingRight ? 11 : 12;
                    _frameCount = 9;
                    break;

                case StateEnemy.Parry:
                    _currentRow = _facingRight ? 3 : 4;
                    _frameCount = 9;
                    break;

                case StateEnemy.Hurt:
                    _currentRow = _facingRight ? 1 : 2;
                    _frameCount = 9;
                    break;

                case StateEnemy.Death:
                    _currentRow = _facingRight ? 7 : 8;
                    _frameCount = 9;
                    break;

                default:
                    return;
            }
        }

        private Bitmap GetFrame(int row, int column)
        {
            Rectangle sourceRectangle = new Rectangle(column * FrameWidth,row * FrameHeight,FrameWidth,FrameHeight);

            // Cria um frame com transparência e suporte a desenho.
            Bitmap frame = new Bitmap(FrameWidth,FrameHeight,System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(frame))
            {
                // O tamanho é igual ao frame original, preservando os pixels.
                g.DrawImage(_spriteSheet,new Rectangle(0, 0, FrameWidth, FrameHeight),sourceRectangle,GraphicsUnit.Pixel);
            }

            return frame;
        }

        private void TMR_Animation_Tick(object sender, EventArgs e)
        {
            if (_spriteSheet == null || _frameCount <= 0)
                return;

            // Exibe o frame atual
            Bitmap novoFrame = GetFrame(_currentRow, _currentFrame);

            // Desenha as partículas enquanto o Orc estiver envenenado.
            if (IsPoisoned && !IsDead)
            {
                DesenharEfeitoVeneno(novoFrame);
            }

            // Desenha as chamas enquanto o Orc estiver queimando.
            if (IsBurning && !IsDead)
            {
                DesenharEfeitoFogo(novoFrame);
            }

            Image frameAnterior = PBX_Enemy.Image;
            PBX_Enemy.Image = novoFrame;
            frameAnterior?.Dispose();

            // Registra o impacto uma única vez durante o ataque.
            if (_state == StateEnemy.Attack && !_attackImpactTriggered && _currentFrame == 4)
            {
                _attackImpactTriggered = true;
                AttackImpact?.Invoke(this);
            }

            // Avança enquanto ainda houver frames
            if (_currentFrame < _frameCount - 1)
            {
                _currentFrame++;
                return;
            }

            // A morte permanece no último frame.
            if (_state == StateEnemy.Death)
                return;

            // Estas animações terminam e retornam ao Idle.
            if (_state == StateEnemy.Attack || _state == StateEnemy.Parry || _state == StateEnemy.Hurt)
            {
                SetAnimation(StateEnemy.Idle);
                return;
            }

            // Idle e Walk ficam em loop.
            _currentFrame = 0;
        }
        #endregion

        #region Comandos de animacao
        public void SetIdle()
        {
            SetIdle(_facingRight);
        }

        public void SetIdle(bool facingRight)
        {
            if (_state == StateEnemy.Death || _state == StateEnemy.Attack || _state == StateEnemy.Hurt || _state == StateEnemy.Parry)
            {
                return;
            }

            bool directionChanged = _facingRight != facingRight;
            _facingRight = facingRight;

            if (_state != StateEnemy.Idle || directionChanged)
                SetAnimation(StateEnemy.Idle);
        }

        public void SetWalk(bool facingRight)
        {
            if (_state == StateEnemy.Death || _state == StateEnemy.Attack || _state == StateEnemy.Parry ||
                _state == StateEnemy.Hurt)
                return;

            bool directionChanged = _facingRight != facingRight;
            _facingRight = facingRight;

            if (_state != StateEnemy.Walk || directionChanged)
                SetAnimation(StateEnemy.Walk);
        }

        public void SetAttack(bool facingRight)
        {
            if (_state == StateEnemy.Death || _state == StateEnemy.Hurt || _state == StateEnemy.Parry || _state == StateEnemy.Attack)
            {
                return;
            }

            _facingRight = facingRight;
            SetAnimation(StateEnemy.Attack);
        }

        public void SetParry()
        {
            if (_state == StateEnemy.Death || _state == StateEnemy.Hurt)
                return;

            SetAnimation(StateEnemy.Parry);
        }

        public void SetHurt()
        {
            if (_state == StateEnemy.Death)
                return;

            SetAnimation(StateEnemy.Hurt);
        }

        public void SetDeath()
        {
            if (_state == StateEnemy.Death)
                return;

            // Encerra o efeito de veneno.
            _poisonDurationRemaining = 0;
            _poisonTickRemaining = 0;
            // Encerra o efeito de queimadura.
            _burnDurationRemaining = 0;
            _burnTickRemaining = 0;

            SetAnimation(StateEnemy.Death);
        }
        #endregion

        #region Enemy State
        public bool FacingRight
        {
            get { return _facingRight; }
        }

        public int DanoAtaque
        {
            get { return 10; }
        }

        public bool IsDead
        {
            get { return _state == StateEnemy.Death; }
        }

        public bool IsPerformingAction
        {
            get
            {
                return _state == StateEnemy.Attack || _state == StateEnemy.Parry || _state == StateEnemy.Hurt || _state == StateEnemy.Death;
            }
        }
        #endregion

        #region Visible Sprite Bounds
        public Rectangle GetVisibleSpriteBounds()
        {
            return CalculateVisibleBounds(PBX_Enemy);
        }

        private Rectangle CalculateVisibleBounds(PictureBox pictureBox)
        {
            Bitmap imagem = pictureBox.Image as Bitmap;

            if (imagem == null)
                return pictureBox.Bounds;

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
                return pictureBox.Bounds;

            float escalaX = 1f;
            float escalaY = 1f;
            float deslocamentoX = 0f;
            float deslocamentoY = 0f;

            switch (pictureBox.SizeMode)
            {
                case PictureBoxSizeMode.StretchImage:
                    escalaX = (float)pictureBox.ClientSize.Width / imagem.Width;
                    escalaY = (float)pictureBox.ClientSize.Height / imagem.Height;
                    break;

                case PictureBoxSizeMode.Zoom:
                    float escala = Math.Min(
                        (float)pictureBox.ClientSize.Width / imagem.Width,
                        (float)pictureBox.ClientSize.Height / imagem.Height
                    );

                    escalaX = escala;
                    escalaY = escala;

                    deslocamentoX = (pictureBox.ClientSize.Width - imagem.Width * escala) / 2f;
                    deslocamentoY = (pictureBox.ClientSize.Height - imagem.Height * escala) / 2f;
                    break;

                case PictureBoxSizeMode.CenterImage:
                    deslocamentoX = (pictureBox.ClientSize.Width - imagem.Width) / 2f;
                    deslocamentoY = (pictureBox.ClientSize.Height - imagem.Height) / 2f;
                    break;
            }

            Rectangle limites = Rectangle.FromLTRB(
                pictureBox.Left + (int)Math.Floor(deslocamentoX + minX * escalaX),
                pictureBox.Top + (int)Math.Floor(deslocamentoY + minY * escalaY),
                pictureBox.Left + (int)Math.Ceiling(deslocamentoX + (maxX + 1) * escalaX),
                pictureBox.Top + (int)Math.Ceiling(deslocamentoY + (maxY + 1) * escalaY)
            );

            return Rectangle.Intersect(limites, pictureBox.Bounds);
        }
        #endregion

        #region Hitbox
        public Rectangle GetHitboxAt(int x, int y)
        {
            int hitboxWidth = 34;
            int hitboxHeight = 48;

            return new Rectangle(x + (Width - hitboxWidth) / 2,y + Height - hitboxHeight,hitboxWidth,hitboxHeight);
        }
        #endregion

        #region Damage and Combat
        public int DanoContato
        {
            get { return 5; }
        }
        public void ReceberDano(int dano)
        {
            _vidaAtual = Math.Max(0, _vidaAtual - dano);

            if (_vidaAtual <= 0)
                SetDeath();
            else
                SetHurt();
        }
        #endregion

        #region Poison and Burn Effects
        private void DesenharEfeitoVeneno(Bitmap frame)
        {
            // Alterna a posição das partículas para criar movimento.
            int fase = _poisonVisualTick % 4;
            _poisonVisualTick++;

            using (Graphics g = Graphics.FromImage(frame))
            using (SolidBrush sombra = new SolidBrush(Color.FromArgb(90, 25, 150, 35)))
            using (SolidBrush veneno = new SolidBrush(Color.FromArgb(210, 55, 190, 45)))
            using (SolidBrush brilho = new SolidBrush(Color.FromArgb(235, 150, 240, 65)))
            {
                // Evita suavização das partículas pixel-art.
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                // Partículas ao redor do corpo do Orc.
                g.FillRectangle(sombra, 16, 38 + fase, 5, 5);
                g.FillRectangle(veneno, 18, 39 + fase, 3, 3);
                g.FillRectangle(brilho, 19, 40 + fase, 2, 2);

                g.FillRectangle(sombra, 87, 48 - fase, 5, 5);
                g.FillRectangle(veneno, 88, 49 - fase, 3, 3);
                g.FillRectangle(brilho, 89, 49 - fase, 2, 2);

                g.FillRectangle(sombra, 27 + fase, 76, 5, 5);
                g.FillRectangle(veneno, 28 + fase, 77, 3, 3);

                g.FillRectangle(veneno, 73 - fase, 29, 3, 3);
                g.FillRectangle(brilho, 74 - fase, 30, 2, 2);
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
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                // Chamas à esquerda do corpo.
                g.FillRectangle(sombra, 15, 48 + fase, 6, 9);
                g.FillRectangle(fogo, 17, 49 + fase, 4, 6);
                g.FillRectangle(brasa, 18, 51 + fase, 2, 3);

                // Chamas à direita do corpo.
                g.FillRectangle(sombra, 91, 45 - fase, 6, 9);
                g.FillRectangle(fogo, 92, 47 - fase, 4, 6);
                g.FillRectangle(brasa, 93, 49 - fase, 2, 3);

                // Brasas próximas aos pés.
                g.FillRectangle(fogo, 31 + fase, 85, 4, 4);
                g.FillRectangle(brasa, 32 + fase, 86, 2, 2);

                g.FillRectangle(fogo, 76 - fase, 91, 4, 4);
                g.FillRectangle(brasa, 77 - fase, 92, 2, 2);
            }
        }

        public bool IsPoisoned
        {
            get { return _poisonDurationRemaining > 0; }
        }

        public void AplicarVeneno(int danoPorTick,int duracaoTicks,int intervaloTicks)
        {
            if (IsDead || danoPorTick <= 0 || duracaoTicks <= 0 || intervaloTicks <= 0)
            {
                return;
            }

            // Não acumula venenos: renova o efeito existente.
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

            // Prepara a próxima aplicação.
            _poisonTickRemaining = _poisonTickInterval;

            // Aplica o dano sem reiniciar a animação Hurt a cada tick.
            _vidaAtual = Math.Max(
                0,
                _vidaAtual - _poisonDamagePerTick
            );

            if (_vidaAtual <= 0)
            {
                _poisonDurationRemaining = 0;
                SetDeath();
            }
        }

        public bool IsBurning
        {
            get { return _burnDurationRemaining > 0; }
        }

        public void AplicarQueimadura(int danoPorTick,int duracaoTicks,int intervaloTicks)
        {
            if (IsDead || danoPorTick <= 0 || duracaoTicks <= 0 || intervaloTicks <= 0)
            {
                return;
            }

            // Renova a queimadura em vez de acumular efeitos ilimitados.
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

            // Aplica dano sem reiniciar a animação Hurt a cada tick.
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
    }
}