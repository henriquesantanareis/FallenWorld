using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FallenWorld.Properties;

namespace FallenWorld
{
    public partial class FireballProjectile : UserControl
    {
        #region Global Variables
        // Cada frame da bola de fogo possui 32x32 pixels.
        private const int FrameWidth = 32;
        private const int FrameHeight = 32;
        // Frames 0 a 9: movimento da bola.
        private const int TravelFrameCount = 10;
        // Frames 10 a 12: animação de impacto.
        private const int ImpactStartFrame = 10;
        private const int ImpactFrameCount = 3;
        private bool _isImpacting = false;
        private bool _isFinished = false;
        private int _impactTickCounter = 0;
        public bool IsImpacting
        {
            get { return _isImpacting; }
        }
        public bool IsFinished
        {
            get { return _isFinished; }
        }
        // Mantém cada frame da explosão visível por 3 atualizações.
        private const int ImpactTicksPerFrame = 3;
        private Bitmap _spriteSheet;
        private int _currentFrame = 0;
        private int _spriteRow;
        // Posição do projétil nas coordenadas do mundo.
        public int WorldX { get; private set; }
        public int WorldY { get; private set; }
        // Direção em que a bola está viajando.
        public bool FacingRight { get; private set; }
        // Velocidade e alcance máximo do projétil.
        public int Speed { get; set; } = 6;
        public int MaxDistance { get; set; } = 280;
        // Guarda a posição inicial para calcular o alcance.
        private int _startX;
        // Informa se a bola já percorreu a distância máxima.
        public bool ExceededMaxDistance
        {
            get
            {
                return Math.Abs(WorldX - _startX) >= MaxDistance;
            }
        }
        #endregion

        #region Constructor
        public FireballProjectile(bool facingRight, int worldX, int worldY)
        {
            InitializeComponent();

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;
            Size = new Size(FrameWidth, FrameHeight);

            FacingRight = facingRight;
            WorldX = worldX;
            WorldY = worldY;
            _startX = worldX;

            // A linha 1 é a animação para leste/direita.
            // A linha 2 é a animação para oeste/esquerda.
            _spriteRow = FacingRight ? 1 : 2;
            _spriteSheet = Properties.Resources.Fireball;

            if (_spriteSheet == null)
                throw new InvalidOperationException("O recurso Fireball não foi carregado.");

            if (_spriteSheet.Width < FrameWidth * (ImpactStartFrame + ImpactFrameCount) || _spriteSheet.Height < FrameHeight * 3)
            {
                throw new InvalidOperationException("A spritesheet Fireball possui dimensões inesperadas.");
            }
            Invalidate();
        }
        #endregion

        #region Update Logic
        // Chamado pelo FormGame a cada atualização do jogo.
        public void UpdateProjectile()
        {
            if (IsDisposed || _isFinished)
                return;

            // Durante a explosão, a bola para de se mover.
            if (_isImpacting)
            {
                _impactTickCounter++;

                if (_impactTickCounter >= ImpactTicksPerFrame)
                {
                    _impactTickCounter = 0;
                    _currentFrame++;
                    // A explosão terminou.
                    if (_currentFrame >= ImpactStartFrame + ImpactFrameCount)
                    {
                        _isFinished = true;
                        return;
                    }
                    Invalidate();
                }
                return;
            }

            // Movimento normal do projétil.
            WorldX += FacingRight ? Speed : -Speed;

            // Anima somente os frames de voo (0 a 9).
            _currentFrame++;

            if (_currentFrame >= TravelFrameCount)
                _currentFrame = 0;

            Invalidate();
        }
        #endregion

        #region Rendering Logic
        public void StartImpact()
        {
            if (_isImpacting || _isFinished)
                return;

            _isImpacting = true;
            _currentFrame = ImpactStartFrame;
            _impactTickCounter = 0;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_spriteSheet == null)
                return;

            // Mantém os pixels nítidos.
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.SmoothingMode = SmoothingMode.None;

            Rectangle sourceRectangle = new Rectangle(_currentFrame * FrameWidth,_spriteRow * FrameHeight,FrameWidth,FrameHeight);
            Rectangle destinationRectangle = new Rectangle(0,0,FrameWidth,FrameHeight);
            e.Graphics.DrawImage(_spriteSheet,destinationRectangle,sourceRectangle,GraphicsUnit.Pixel);
        }
        #endregion
    }
}
