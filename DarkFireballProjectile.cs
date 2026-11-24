
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FallenWorld
{
    public class DarkFireballProjectile : UserControl
    {
        private const int FrameWidth = 68;
        private const int FrameHeight = 68;

        // Frames 0 a 4: voo.
        private const int TravelFrameCount = 5;

        // Frames 5 a 8: impacto.
        private const int ImpactStartFrame = 5;
        private const int ImpactFrameCount = 4;

        private const int TicksPerFrame = 2;
        private const int ImpactTicksPerFrame = 3;

        private readonly Bitmap _spriteSheet;

        private int _currentFrame = 0;
        private int _animationTick = 0;
        private int _impactTickCounter = 0;
        private int _spriteRow;

        private bool _isImpacting = false;
        private bool _isFinished = false;

        public int WorldX { get; private set; }
        public int WorldY { get; private set; }

        public bool FacingRight { get; private set; }

        public int Speed { get; set; } = 6;
        public int MaxDistance { get; set; } = 480;

        private int _startX;

        public bool IsImpacting
        {
            get { return _isImpacting; }
        }

        public bool IsFinished
        {
            get { return _isFinished; }
        }

        public bool ExceededMaxDistance
        {
            get
            {
                return Math.Abs(WorldX - _startX) >= MaxDistance;
            }
        }

        public DarkFireballProjectile(
            bool facingRight,
            int worldX,
            int worldY)
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true
            );

            BackColor = Color.Transparent;
            Size = new Size(FrameWidth, FrameHeight);
            TabStop = false;

            FacingRight = facingRight;
            WorldX = worldX;
            WorldY = worldY;

            _startX = worldX;

            // Utiliza a variação roxa da spritesheet.
            // Linha 3: direita.
            // Linha 4: esquerda.
            _spriteRow = FacingRight ? 3 : 4;

            _spriteSheet = new Bitmap(Properties.Resources.Dark_fireball_sprite);

            if (_spriteSheet.Width < 612 || _spriteSheet.Height < 340)
            {
                throw new InvalidOperationException(
                    "A spritesheet Dark_fireball possui dimensões inesperadas."
                );
            }

            Invalidate();
        }

        // Hitbox menor que o sprite, centralizada no projétil.
        public Rectangle GetCollisionBounds()
        {
            return new Rectangle(WorldX + 22,WorldY + 22,24,24);
        }

        public void UpdateProjectile()
        {
            if (IsDisposed || _isFinished)
                return;

            // Durante o impacto, o projétil fica parado.
            if (_isImpacting)
            {
                _impactTickCounter++;

                if (_impactTickCounter >= ImpactTicksPerFrame)
                {
                    _impactTickCounter = 0;
                    _currentFrame++;

                    if (_currentFrame >= ImpactStartFrame + ImpactFrameCount)
                    {
                        _isFinished = true;
                        return;
                    }

                    Invalidate();
                }

                return;
            }

            // Movimento horizontal.
            WorldX += FacingRight ? Speed : -Speed;

            // Anima somente os frames de voo.
            _animationTick++;

            if (_animationTick >= TicksPerFrame)
            {
                _animationTick = 0;
                _currentFrame++;

                if (_currentFrame >= TravelFrameCount)
                    _currentFrame = 0;

                Invalidate();
            }
        }

        // Inicia a animação roxa de impacto.
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

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            e.Graphics.SmoothingMode = SmoothingMode.None;

            Rectangle sourceRectangle = new Rectangle(
                _currentFrame * FrameWidth,
                _spriteRow * FrameHeight,
                FrameWidth,
                FrameHeight
            );

            Rectangle destinationRectangle = new Rectangle(
                0,
                0,
                FrameWidth,
                FrameHeight
            );

            e.Graphics.DrawImage(
                _spriteSheet,
                destinationRectangle,
                sourceRectangle,
                GraphicsUnit.Pixel
            );
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _spriteSheet != null)
                _spriteSheet.Dispose();

            base.Dispose(disposing);
        }
    }
}
