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
        private readonly Personagem _personagem;

        public enum StatePlayer
        {
            Idle,
            Walk,
            Attack,
            Death,
            Fireball
        }
        private StatePlayer _state = StatePlayer.Idle;

        private bool _facingRight = true;

        // Spritesheet
        private Bitmap _spriteSheet;

        // Controle da animação
        private int _currentFrame = 0;
        private int _currentRow = 0;
        private int _frameCount = 0;

        // Tamanho de cada frame
        private const int FrameWidth = 64;
        private const int FrameHeight = 64;
        #endregion

        public Player(Personagem personagem)
        {
            InitializeComponent();
            _personagem = personagem;
            this.Size = new Size(FrameWidth, FrameHeight);
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            configCharacter();
            SetAnimation(StatePlayer.Idle);
        }

        #region Load SpriteSheet
        private void configCharacter()
        {
            switch (_personagem)
            {
                case Personagem.Mago:
                    _spriteSheet = new Bitmap(Properties.Resources.duckMage_Spritesheet);
                    PBX_PLAYER.Image = GetFrame(9, 0);
                    break;
                case Personagem.Cavaleiro:
                    // vamo adicionar ainda
                    break;
                case Personagem.Princesa:
                    // vamo adicionar ainda
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
                            _currentRow = _facingRight ? 9 : 10;
                            _frameCount = 8;
                            break;
                        case StatePlayer.Walk:
                            _currentRow = _facingRight ? 11 : 12;
                            _frameCount = 9;
                            break;
                        case StatePlayer.Attack:
                            _currentRow = _facingRight ? 1 : 2;
                            _frameCount = 9;
                            break;
                        case StatePlayer.Death:
                            _currentRow = _facingRight ? 6 : 7;
                            _frameCount = 9;
                            break;
                        case StatePlayer.Fireball:
                            _currentRow = 8;
                            _frameCount = 6;
                            break;
                    }
                    break;
                case Personagem.Cavaleiro:
                    switch (_state)
                    {

                    }
                    break;
                case Personagem.Princesa:
                    switch (_state)
                    {

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

            return _spriteSheet.Clone(sourceRectangle, _spriteSheet.PixelFormat);
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

            // Ataque e Fireball terminam e voltam ao Idle
            if (_state == StatePlayer.Attack || _state == StatePlayer.Fireball)
            {
                SetAnimation(StatePlayer.Idle);
                return;
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
            if (_state == StatePlayer.Death || _state == StatePlayer.Attack || _state == StatePlayer.Fireball)
                return;

            if (_state != StatePlayer.Idle)
                SetAnimation(StatePlayer.Idle);
        }

        public void SetWalk(bool facingRight)
        {
            if (_state == StatePlayer.Death || _state == StatePlayer.Attack || _state == StatePlayer.Fireball)
                return;

            bool directionChanged = _facingRight != facingRight;

            _facingRight = facingRight;

            if (_state != StatePlayer.Walk || directionChanged)
                SetAnimation(StatePlayer.Walk);
        }

        public void SetAttack()
        {
            if (_state != StatePlayer.Death)
                SetAnimation(StatePlayer.Attack);
        }

        public void SetFireball()
        {
            if (_state != StatePlayer.Death)
                SetAnimation(StatePlayer.Fireball);
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
                return _state == StatePlayer.Attack || _state == StatePlayer.Fireball || _state == StatePlayer.Death;
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
            int hitboxWidth = 24;
            int hitboxHeight = 40;

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
