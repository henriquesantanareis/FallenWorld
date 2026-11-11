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

namespace FallenWorld
{
    public partial class Player : UserControl
    {
        #region Variaveis Globais

        #region Player
        private readonly Personagem _personagem;
        private StatePlayer _state = StatePlayer.Idle;
        public enum StatePlayer
        {
            Idle,
            Walk,
            Jump,
            Attack,
            Recover_Mana,
            Fireball,
            Aura,
            Sword_Slash,
            Poison_Attack,
            Hurt,
            Death,
        }
        // Controle visual da Aura do Cavaleiro.
        private int _auraVisualTick = 0;
        // Controle da onda de energia ao ativar a Aura.
        private bool _auraBurstActive = false;
        private int _auraBurstTick = 0;
        // Duração da animação da onda em atualizações.
        private const int AuraBurstDurationFrames = 18;
        #endregion

        #region Sprite Sheet
        private int FrameWidth;
        private int FrameHeight;
        private Bitmap _spriteSheet;
        private int _currentFrame = 0;
        private int _currentRow = 0;
        private int _frameCount = 0;
        private bool _facingRight = true;
        #endregion

        #region Player Stats
        private const int VidaMaximaPlayer = 100;
        private int _vidaAtual = VidaMaximaPlayer;
        public event Action<StatePlayer> AttackImpact;
        private bool _attackImpactTriggered = false;
        // Sistema de mana do Mago.
        private const int ManaMaximaPlayer = 100;
        private int _manaAtual = ManaMaximaPlayer;
        public int ManaAtual
        {
            get { return _manaAtual; }
        }
        public int ManaMaxima
        {
            get { return ManaMaximaPlayer; }
        }
        // Verifica se o Mago está recuperando mana.
        public bool IsRecoveringMana
        {
            get
            {
                return _personagem == Personagem.Mago && _state == StatePlayer.Recover_Mana;
            }
        }
        #endregion

        #endregion

        #region Construtor
        public Player(Personagem personagem)
        {
            InitializeComponent();
            _personagem = personagem;
            configCharacter();
            this.Size = new Size(FrameWidth, FrameHeight);
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            SetAnimation(StatePlayer.Idle);
            // Exibe imediatamente o primeiro frame da animação.
            if (_spriteSheet != null && _frameCount > 0)
                PBX_PLAYER.Image = GetFrame(_currentRow, 0);
        }
        #endregion

        #region Character Configuration
        private void configCharacter()
        {
            switch (_personagem)
            {
                case Personagem.Mago:
                    FrameWidth = 64;
                    FrameHeight = 64;
                    _spriteSheet = new Bitmap(Properties.Resources.duckMage_Spritesheet);
                    break;
                case Personagem.Cavaleiro:
                    FrameWidth = 104;
                    FrameHeight = 104;
                    _spriteSheet = new Bitmap(Properties.Resources.knight_Spritesheet);
                    break;
                case Personagem.Princesa:
                    FrameWidth = 136;
                    FrameHeight = 136;
                    _spriteSheet = new Bitmap(Properties.Resources.princess_spritesheet);
                    break;
            }
        }
        #endregion

        #region Animations
        private void SetAnimation(StatePlayer state)
        {
            _state = state;
            _currentFrame = 0;
            _attackImpactTriggered = false;

            switch (_personagem)
            {
                case Personagem.Mago:
                    switch (_state)
                    {
                        case StatePlayer.Idle:
                            _currentRow = _facingRight ? 5 : 6;
                            _frameCount = 9;
                            break;
                        case StatePlayer.Walk:
                            _currentRow = _facingRight ? 15 : 16;
                            _frameCount = 9;
                            break;
                        case StatePlayer.Jump:
                            _currentRow = _facingRight ? 1 : 2;
                            _frameCount = 9;
                            break;
                        case StatePlayer.Attack:
                            _currentRow = _facingRight ? 3 : 4;
                            _frameCount = 9;
                            break;
                        case StatePlayer.Recover_Mana:
                            _currentRow = _facingRight ? 7 : 8;
                            _frameCount = 9;
                            break;
                        case StatePlayer.Fireball:
                            _currentRow = _facingRight ? 9 : 10;
                            _frameCount = 9;
                            break;
                        case StatePlayer.Hurt:
                            _currentRow = _facingRight ? 13 : 14;
                            _frameCount = 9;
                            break;
                        case StatePlayer.Death:
                            _currentRow = _facingRight ? 11 : 12;
                            _frameCount = 9;
                            break;
                        default:
                            return;
                    }
                    break;
                case Personagem.Cavaleiro:
                    switch (_state)
                    {
                        case StatePlayer.Idle:
                            _currentRow = _facingRight ? 1 : 2;
                            _frameCount = 8;
                            break;

                        case StatePlayer.Jump:
                            _currentRow = _facingRight ? 7 : 8;
                            _frameCount = 9;
                            break;

                        case StatePlayer.Attack:
                            _currentRow = _facingRight ? 9 : 10;
                            _frameCount = 9;
                            break;

                        case StatePlayer.Death:
                            _currentRow = _facingRight ? 11 : 12;
                            _frameCount = 9;
                            break;

                        case StatePlayer.Hurt:
                            _currentRow = _facingRight ? 13 : 14;
                            _frameCount = 7;
                            break;

                        case StatePlayer.Aura:
                            _currentRow = _facingRight ? 15 : 16;
                            _frameCount = 9;
                            break;

                        case StatePlayer.Walk:
                            _currentRow = _facingRight ? 17 : 18;
                            _frameCount = 9;
                            break;

                        default:
                            return;
                    }
                    break;
                case Personagem.Princesa:
                    switch (_state)
                    {
                        case StatePlayer.Idle:
                            _currentRow = _facingRight ? 5 : 6;
                            _frameCount = 8;
                            break;
                        case StatePlayer.Walk:
                            _currentRow = _facingRight ? 7 : 8;
                            _frameCount = 8;
                            break;
                        case StatePlayer.Jump:
                            _currentRow = _facingRight ? 15 : 16;
                            _frameCount = 8;
                            break;
                        case StatePlayer.Attack:
                            _currentRow = _facingRight ? 11 : 12;
                            _frameCount = 8;
                            break;
                        case StatePlayer.Poison_Attack:
                            _currentRow = _facingRight ? 1 : 2;
                            _frameCount = 8;
                            break;
                        case StatePlayer.Hurt:
                            _currentRow = _facingRight ? 13 : 14;
                            _frameCount = 8;
                            break;
                        case StatePlayer.Death:
                            _currentRow = _facingRight ? 9 : 10;
                            _frameCount = 8;
                            break;
                    }
                    break;
            }
        }

        private Bitmap GetFrame(int row, int column)
        {
            Rectangle sourceRectangle = new Rectangle(column * FrameWidth,row * FrameHeight,FrameWidth,FrameHeight);

            Bitmap frame = _spriteSheet.Clone(sourceRectangle, _spriteSheet.PixelFormat);

            return frame;
        }

        private void TMR_Animation_Tick(object sender, EventArgs e)
        {
            if (_spriteSheet == null || _frameCount <= 0)
                return;

            // Exibe o frame atual
            Bitmap novoFrame = GetFrame(_currentRow, _currentFrame);

            // Desenha a Aura enquanto o buff estiver ativo.
            if (_personagem == Personagem.Cavaleiro && AuraAtiva)
            {
                DesenharAura(novoFrame);
            }

            // Desenha a onda de energia da ativação.
            if (_personagem == Personagem.Cavaleiro && _auraBurstActive)
            {
                DesenharOndaAura(novoFrame);
            }

            Image frameAnterior = PBX_PLAYER.Image;
            PBX_PLAYER.Image = novoFrame;
            frameAnterior?.Dispose();

            // Define o frame de impacto de cada tipo de ataque.
            int impactFrame = _state == StatePlayer.Fireball ? _frameCount - 1 : 4;

            // Dispara o evento uma única vez no frame correto.
            if (!_attackImpactTriggered && IsDamageAttack(_state) && _currentFrame == impactFrame)
            {
                _attackImpactTriggered = true;
                AttackImpact?.Invoke(_state);
            }

            // Avança enquanto ainda houver frames
            if (_currentFrame < _frameCount - 1)
            {
                _currentFrame++;
                return;
            }

            // Recuperação de mana:
            // Executa os frames iniciais normalmente.
            // Depois permanece em loop entre os frames 5 e 8.
            if (_personagem == Personagem.Mago && _state == StatePlayer.Recover_Mana)
            {
                _currentFrame = 5;
                return;
            }

            switch (_personagem)
            {
                case Personagem.Mago:
                    // Ataque e Fireball terminam e voltam ao Idle
                    if (_state == StatePlayer.Attack || _state == StatePlayer.Fireball || _state == StatePlayer.Hurt || _state == StatePlayer.Recover_Mana)
                    {
                        SetAnimation(StatePlayer.Idle);
                        return;
                    }
                    break;
                case Personagem.Cavaleiro:
                    // Ataque e Aura terminam e voltam ao Idle
                    if (_state == StatePlayer.Attack || _state == StatePlayer.Aura || _state == StatePlayer.Hurt)
                    {
                        SetAnimation(StatePlayer.Idle);
                        return;
                    }
                    break;
                case Personagem.Princesa:
                    if (_state == StatePlayer.Attack || _state == StatePlayer.Sword_Slash || _state == StatePlayer.Poison_Attack || _state == StatePlayer.Hurt)
                    {
                        SetAnimation(StatePlayer.Idle);
                        return;
                    }
                    break;
            }

            // A animação de morte permanece no último frame
            if (_state == StatePlayer.Death)
                return;

            // Idle e Walk ficam em loop
            _currentFrame = 0;
        }
        #endregion

        #region Animation Commands
        public void SetIdle()
        {
            if (_state == StatePlayer.Death || _state == StatePlayer.Attack || _state == StatePlayer.Fireball || _state == StatePlayer.Hurt ||
                _state == StatePlayer.Jump || _state == StatePlayer.Recover_Mana)
            {
                return;
            }

            if (_state != StatePlayer.Idle)
                SetAnimation(StatePlayer.Idle);
        }

        public void SetWalk(bool facingRight)
        {
            // Preservar animações prioritárias.
            if (_state == StatePlayer.Death || _state == StatePlayer.Attack || _state == StatePlayer.Fireball || _state == StatePlayer.Hurt ||
                _state == StatePlayer.Recover_Mana || _state == StatePlayer.Jump)
            {
                return;
            }

            bool directionChanged = _facingRight != facingRight;
            _facingRight = facingRight;

            if (_state != StatePlayer.Walk || directionChanged)
                SetAnimation(StatePlayer.Walk);
        }

        public void SetJump(bool facingRight)
        {
            if (_state == StatePlayer.Death || _state == StatePlayer.Attack || _state == StatePlayer.Fireball || _state == StatePlayer.Hurt ||
                _state == StatePlayer.Recover_Mana)
            {
                return;
            }

            bool directionChanged = _facingRight != facingRight;
            _facingRight = facingRight;

            if (_state != StatePlayer.Jump || directionChanged)
                SetAnimation(StatePlayer.Jump);
        }

        public void SetJump()
        {
            // Mantém a direção atual do personagem.
            SetJump(_facingRight);
        }

        public void SetGrounded()
        {
            // Só encerra o pulo se a animação atual for Jump.
            if (_state == StatePlayer.Jump)
                SetAnimation(StatePlayer.Idle);
        }

        public void SetAttack()
        {
            if (_state == StatePlayer.Death || _state == StatePlayer.Hurt || _state == StatePlayer.Recover_Mana || _state == StatePlayer.Aura)
            {
                return;
            }

            SetAnimation(StatePlayer.Attack);
        }

        public bool SetFireball()
        {
            if (_personagem != Personagem.Mago)
                return false;

            if (_state == StatePlayer.Death || _state == StatePlayer.Hurt || _state == StatePlayer.Recover_Mana)
            {
                return false;
            }

            SetAnimation(StatePlayer.Fireball);
            return true;
        }

        public bool SetRecover_Mana()
        {
            if (_personagem != Personagem.Mago)
                return false;

            // Só permite iniciar estando parado.
            if (_state != StatePlayer.Idle)
                return false;

            if (_manaAtual >= ManaMaximaPlayer)
                return false;

            SetAnimation(StatePlayer.Recover_Mana);

            return true;
        }

        public bool SetAura()
        {
            if (_personagem != Personagem.Cavaleiro)
                return false;

            if (_state == StatePlayer.Death || _state == StatePlayer.Attack || _state == StatePlayer.Hurt || _state == StatePlayer.Jump || _state == StatePlayer.Aura)
            {
                return false;
            }

            SetAnimation(StatePlayer.Aura);

            return true;
        }

        public void SetPoisonAttack()
        {
            if (_personagem != Personagem.Princesa)
                return;

            if (_state == StatePlayer.Death)
                return;

            SetAnimation(StatePlayer.Poison_Attack);
        }

        public void SetHurt()
        {
            if (_state == StatePlayer.Death)
                return;

            SetAnimation(StatePlayer.Hurt);
        }

        public void SetDeath()
        {
            SetAnimation(StatePlayer.Death);
        }
        #endregion

        #region Character State
        public bool IsPerformingAction
        {
            get
            {
                switch (_personagem)
                {
                    case Personagem.Mago:
                        return _state == StatePlayer.Fireball || _state == StatePlayer.Death || _state == StatePlayer.Hurt || _state == StatePlayer.Recover_Mana;
                    case Personagem.Cavaleiro:
                        return _state == StatePlayer.Aura || _state == StatePlayer.Death || _state == StatePlayer.Hurt;
                    case Personagem.Princesa:
                        return _state == StatePlayer.Death || _state == StatePlayer.Poison_Attack || _state == StatePlayer.Hurt;
                    default:
                        return false;
                }
            }
        }
        public bool FacingRight
        {
            get { return _facingRight; }
        }
        public bool EstaRecebendoDano
        {
            get { return _state == StatePlayer.Hurt; }
        }
        public bool EstaMorto
        {
            get { return _state == StatePlayer.Death; }
        }
        public int VidaAtual
        {
            get { return _vidaAtual; }
        }
        public bool AuraAtiva { get; set; }
        #endregion

        #region Visible Sprite Bounds
        public Rectangle GetVisibleSpriteBounds()
        {
            return CalculateVisibleBounds(PBX_PLAYER);
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

                    deslocamentoX =
                        (pictureBox.ClientSize.Width - imagem.Width * escala) / 2f;

                    deslocamentoY =
                        (pictureBox.ClientSize.Height - imagem.Height * escala) / 2f;
                    break;

                case PictureBoxSizeMode.CenterImage:
                    deslocamentoX =
                        (pictureBox.ClientSize.Width - imagem.Width) / 2f;

                    deslocamentoY =
                        (pictureBox.ClientSize.Height - imagem.Height) / 2f;
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
            int hitboxWidth;
            int hitboxHeight;
            switch (_personagem)
            {
                case Personagem.Mago:
                    hitboxWidth = 24;
                    hitboxHeight = 40;
                    break;
                case Personagem.Cavaleiro:
                    hitboxWidth = 40;
                    hitboxHeight = 60;
                    break;
                case Personagem.Princesa:
                    hitboxWidth = 24;
                    hitboxHeight = 40;
                    break;
                default:
                    hitboxWidth = 30;
                    hitboxHeight = 56;
                    break;
            }

            return new Rectangle(
                x + (Width - hitboxWidth) / 2,
                y + Height - hitboxHeight,
                hitboxWidth,
                hitboxHeight
            );
        }
        #endregion

        #region Damage
        public bool ReceberDano(int dano)
        {
            if (dano <= 0 || EstaMorto)
                return false;

            _vidaAtual = Math.Max(0, _vidaAtual - dano);

            if (_vidaAtual <= 0)
                SetDeath();
            else
                SetHurt();

            return true;
        }
        private bool IsDamageAttack(StatePlayer state)
        {
            switch (_personagem)
            {
                case Personagem.Mago:
                    return state == StatePlayer.Attack || state == StatePlayer.Fireball;

                case Personagem.Cavaleiro:
                    return state == StatePlayer.Attack;

                case Personagem.Princesa:
                    return state == StatePlayer.Attack || state == StatePlayer.Poison_Attack;

                default:
                    return false;
            }
        }
        #endregion

        #region Mana
        public bool ConsumirMana(int quantidade)
        {
            if (_personagem != Personagem.Mago || quantidade <= 0 || _manaAtual < quantidade)
            {
                return false;
            }
            _manaAtual -= quantidade;
            return true;
        }

        public void RecuperarMana(int quantidade)
        {
            if (_personagem != Personagem.Mago || !IsRecoveringMana || quantidade <= 0)
            {
                return;
            }
            _manaAtual = Math.Min(ManaMaximaPlayer,_manaAtual + quantidade);
        }

        public void CancelarRecuperacaoMana()
        {
            if (_state != StatePlayer.Recover_Mana)
                return;

            SetAnimation(StatePlayer.Idle);
        }
        #endregion

        #region Buff Effects
        private void DesenharAura(Bitmap frame)
        {
            if (frame == null || !AuraAtiva)
                return;

            // Alterna as posições das partículas.
            int fase = _auraVisualTick % 8;
            _auraVisualTick++;

            // Intensidade da pulsação.
            int pulso = fase < 4 ? fase : 7 - fase;

            using (Graphics g = Graphics.FromImage(frame))
            using (Pen auraExterna = new Pen(Color.FromArgb(75, 255, 150, 20), 4))
            using (Pen auraInterna = new Pen(Color.FromArgb(170, 255, 210, 75), 2))
            using (SolidBrush energia = new SolidBrush(Color.FromArgb(190, 255, 160, 30)))
            using (SolidBrush brilho = new SolidBrush(Color.FromArgb(240, 255, 230, 120)))
            {
                // Preserva o estilo pixel-art.
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;

                // Contorno energético ao redor do Cavaleiro.
                g.DrawEllipse(auraExterna,7 + pulso,5 + pulso,frame.Width - 14 - pulso * 2,frame.Height - 10 - pulso * 2);
                g.DrawEllipse(auraInterna,11 + pulso,9 + pulso,frame.Width - 22 - pulso * 2,frame.Height - 18 - pulso * 2);

                // Partículas do lado esquerdo.
                int particulaEsquerda = 20 + fase * 7;

                g.FillRectangle(energia, 5, particulaEsquerda, 4, 4);
                g.FillRectangle(brilho, 6, particulaEsquerda + 1, 2, 2);

                // Partículas do lado direito.
                int particulaDireita = 78 - fase * 7;

                g.FillRectangle(energia, frame.Width - 9,particulaDireita, 4, 4);
                g.FillRectangle(brilho, frame.Width - 8,particulaDireita + 1, 2, 2);

                // Partículas na parte superior.
                g.FillRectangle(brilho, 25 + fase * 3, 5, 3, 3);
                g.FillRectangle(energia, 70 - fase * 3, 9, 4, 4);

                // Energia na parte inferior.
                g.FillRectangle(energia, 30 + fase * 2,frame.Height - 12, 4, 4);
                g.FillRectangle(brilho, 73 - fase * 2,frame.Height - 9, 3, 3);
            }
        }
        private void DesenharOndaAura(Bitmap frame)
        {
            if (frame == null || !_auraBurstActive)
                return;

            // Calcula o progresso da onda.
            float progresso = _auraBurstTick/(float)(AuraBurstDurationFrames - 1);

            // Raio aumenta conforme a onda se expande.
            float raio = 8 + (progresso * 45);

            // A transparência diminui durante a expansão.
            int alpha = (int)(230 * (1f - progresso));

            float centroX = frame.Width / 2f;
            float centroY = frame.Height / 2f;

            using (Graphics g = Graphics.FromImage(frame))
            using (Pen ondaExterna = new Pen(Color.FromArgb(alpha, 255, 180, 35), 3))
            using (Pen ondaInterna = new Pen(Color.FromArgb(Math.Max(0, alpha - 50), 255, 235, 130), 2))
            using (SolidBrush particula = new SolidBrush(Color.FromArgb(alpha, 255, 195, 55)))
            using (SolidBrush brilho = new SolidBrush(Color.FromArgb(alpha, 255, 245, 170)))
            {
                // Mantém o aspecto pixel-art.
                g.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.None;

                // Onda externa em expansão.
                g.DrawEllipse(ondaExterna,centroX - raio,centroY - raio,raio * 2,raio * 2);

                // Segunda onda, um pouco menor.
                float raioInterno = raio * 0.75f;

                g.DrawEllipse(ondaInterna,centroX - raioInterno,centroY - raioInterno,raioInterno * 2,raioInterno * 2);

                // Partículas distribuídas ao redor da onda.
                for (int i = 0; i < 8; i++)
                {
                    double angulo = i * Math.PI / 4 + progresso * 0.5;

                    int x = (int)(centroX + Math.Cos(angulo) * raio);

                    int y = (int)(centroY + Math.Sin(angulo) * raio);

                    // Alterna o tamanho das partículas.
                    int tamanho = i % 2 == 0 ? 4 : 3;

                    g.FillRectangle(particula,x - tamanho / 2,y - tamanho / 2,tamanho,tamanho);

                    // Pequeno brilho no centro da partícula.
                    if (tamanho == 4)
                    {
                        g.FillRectangle(brilho,x,y,2,2);
                    }
                }
            }

            // Avança a animação da onda.
            _auraBurstTick++;

            // Finaliza o efeito após a última atualização.
            if (_auraBurstTick >= AuraBurstDurationFrames)
            {
                _auraBurstActive = false;
                _auraBurstTick = 0;
            }
        }
        #endregion
    }
}
