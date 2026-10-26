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
        private int FrameWidth;
        private int FrameHeight;
        private Bitmap _spriteSheet;
        private int _currentFrame = 0;
        private int _currentRow = 0;
        private int _frameCount = 0;
        private StatePlayer _state = StatePlayer.Idle;
        private bool _facingRight = true;
        private readonly Personagem _personagem;
        #endregion

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

        #region Load SpriteSheet
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

                        //case StatePlayer.Stab:
                        //    _currentRow = _facingRight ? 3 : 4;
                        //    _frameCount = 9;
                        //    break;

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
            Rectangle sourceRectangle = new Rectangle(
                column * FrameWidth,
                row * FrameHeight,
                FrameWidth,
                FrameHeight
            );

            Bitmap frame = _spriteSheet.Clone(sourceRectangle,_spriteSheet.PixelFormat);

            return frame;
        }

        private void TMR_Animation_Tick(object sender, EventArgs e)
        {
            if (_spriteSheet == null || _frameCount <= 0)
                return;

            // Exibe o frame atual
            Bitmap novoFrame = GetFrame(_currentRow, _currentFrame);

            Image frameAnterior = PBX_PLAYER.Image;
            PBX_PLAYER.Image = novoFrame;
            frameAnterior?.Dispose();

            // Avança enquanto ainda houver frames
            if (_currentFrame < _frameCount - 1)
            {
                _currentFrame++;
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

        #region Comandos de animação
        public void SetIdle()
        {
            if (_state == StatePlayer.Death || 
                _state == StatePlayer.Attack || 
                _state == StatePlayer.Fireball || 
                _state == StatePlayer.Hurt || 
                _state == StatePlayer.Jump ||  
                _state == StatePlayer.Recover_Mana)
            {
                return;
            }

            if (_state != StatePlayer.Idle)
                SetAnimation(StatePlayer.Idle);
        }

        public void SetWalk(bool facingRight)
        {
            // Preservar animações prioritárias.
            if (_state == StatePlayer.Death ||
                _state == StatePlayer.Attack ||
                _state == StatePlayer.Fireball ||
                _state == StatePlayer.Hurt ||
                _state == StatePlayer.Recover_Mana ||
                _state == StatePlayer.Jump)
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
            if (_state == StatePlayer.Death ||
                _state == StatePlayer.Attack ||
                _state == StatePlayer.Fireball ||
                _state == StatePlayer.Hurt ||
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
            if (_state == StatePlayer.Death ||
                _state == StatePlayer.Hurt ||
                _state == StatePlayer.Recover_Mana ||
                _state == StatePlayer.Aura)
            {
                return;
            }

            SetAnimation(StatePlayer.Attack);
        }

        public void SetFireball()
        {
            if(_personagem != Personagem.Mago)
                return;

            if (_state == StatePlayer.Death ||
                _state == StatePlayer.Hurt ||
                _state == StatePlayer.Recover_Mana)
            {
                return;
            }

            SetAnimation(StatePlayer.Fireball);
        }

        public void SetRecover_Mana()
        {
            if (_personagem != Personagem.Mago)
                return;

            if (_state == StatePlayer.Death ||
                _state == StatePlayer.Attack ||
                _state == StatePlayer.Fireball ||
                _state == StatePlayer.Hurt ||
                _state == StatePlayer.Jump)
            {
                return;
            }

            SetAnimation(StatePlayer.Recover_Mana);
        }

        public void SetAura()
        {
            if (_personagem != Personagem.Cavaleiro)
                return;

            if (_state == StatePlayer.Death ||
                _state == StatePlayer.Attack ||
                _state == StatePlayer.Hurt ||
                _state == StatePlayer.Jump)
            {
                return;
            }

            SetAnimation(StatePlayer.Aura);
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

        public bool IsPerformingAction
        {
            get
            {
                switch(_personagem)
                {
                    case Personagem.Mago:
                        return _state == StatePlayer.Fireball ||
                               _state == StatePlayer.Death ||
                               _state == StatePlayer.Hurt ||
                               _state == StatePlayer.Recover_Mana;
                    case Personagem.Cavaleiro:
                        return _state == StatePlayer.Aura ||
                               _state == StatePlayer.Death ||
                               _state == StatePlayer.Hurt;
                    case Personagem.Princesa:
                        return _state == StatePlayer.Poison_Attack ||
                               _state == StatePlayer.Death ||
                               _state == StatePlayer.Poison_Attack ||
                               _state == StatePlayer.Hurt;
                    default:
                        return false;
                }                
            }
        }

        public Rectangle GetVisibleSpriteBounds()
        {
            Bitmap imagem = PBX_PLAYER.Image as Bitmap;

            if (imagem == null)
                return new Rectangle(0, 0, Width, Height);

            int minX = imagem.Width;
            int minY = imagem.Height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < imagem.Height; y++)
            {
                for (int x = 0; x < imagem.Width; x++)
                {
                    Color pixel = imagem.GetPixel(x, y);

                    if (pixel.A > 10)
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (maxX < 0 || maxY < 0)
                return new Rectangle(0, 0, Width, Height);

            return Rectangle.FromLTRB(minX, minY, maxX + 1, maxY + 1);
        }

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
    }
}
