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

        public Player()
        {
            InitializeComponent();
        }

        public Player(Personagem personagem)
        {
            InitializeComponent();
            _personagem = personagem;
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
                            break;
                        case StatePlayer.Attack:
                            break;
                        case StatePlayer.Death:
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

            return _spriteSheet.Clone(sourceRectangle,_spriteSheet.PixelFormat);
        }

        private void TMR_Animation_Tick(object sender, EventArgs e)
        {
            if (_spriteSheet == null)
                return;

            if (_currentFrame >= _frameCount)
            {
                if (_state == StatePlayer.Death)
                {
                    _currentFrame = _frameCount - 1;
                }
                else
                {
                    _currentFrame = 0;
                }
            }

            PBX_PLAYER.Image = GetFrame(_currentRow,_currentFrame);

            _currentFrame++;
        }
        #endregion

    }
}
