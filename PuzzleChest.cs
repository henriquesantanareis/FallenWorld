using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FallenWorld
{
    public class PuzzleChest : UserControl
    {
        private const int FrameWidth = 68;
        private const int FrameHeight = 68;
        private const int FrameDurationMs = 100;
        private const int AnimationRow = 1;
        private const int AnimationFrameCount = 13;

        private Bitmap _closedFrame;
        private readonly List<Bitmap> _openingFrames = new List<Bitmap>();

        private int _currentFrame;
        private int _frameElapsedMs;

        private bool _isOpening;
        private bool _isOpen;

        public bool IsOpening
        {
            get { return _isOpening; }
        }

        public bool IsOpen
        {
            get { return _isOpen; }
        }

        public PuzzleChest(Bitmap spriteSheet)
        {
            if (spriteSheet == null)
                throw new ArgumentNullException("spriteSheet");

            if (spriteSheet.Width < FrameWidth * 13 || spriteSheet.Height < FrameHeight * 4)
            {
                throw new ArgumentException("A spritesheet do baú possui dimensões inválidas.");
            }

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true
            );

            BackColor = Color.Transparent;

            Width = FrameWidth;
            Height = FrameHeight;

            // Primeiro quadro da animação: baú fechado.
            _closedFrame = ExtrairFrame(spriteSheet, 0, AnimationRow);

            // Carrega os 13 quadros da animação de abertura.
            for (int i = 0; i < AnimationFrameCount; i++)
            {
                _openingFrames.Add(ExtrairFrame(spriteSheet, i, AnimationRow));
            }
        }

        private Bitmap ExtrairFrame(Bitmap spriteSheet,int coluna,int linha)
        {
            Rectangle area = new Rectangle(
                coluna * FrameWidth,
                linha * FrameHeight,
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
                    spriteSheet,
                    new Rectangle(0, 0, FrameWidth, FrameHeight),
                    area,
                    GraphicsUnit.Pixel
                );
            }

            return frame;
        }

        public void StartOpening()
        {
            if (_isOpening || _isOpen)
                return;

            _isOpening = true;
            _currentFrame = 0;
            _frameElapsedMs = 0;

            Invalidate();
        }

        public void UpdateAnimation(int elapsedMilliseconds)
        {
            if (!_isOpening || elapsedMilliseconds <= 0)
                return;

            _frameElapsedMs += elapsedMilliseconds;

            while (_frameElapsedMs >= FrameDurationMs &&
                   _currentFrame < _openingFrames.Count - 1)
            {
                _frameElapsedMs -= FrameDurationMs;
                _currentFrame++;
            }

            if (_currentFrame >= _openingFrames.Count - 1)
            {
                _isOpening = false;
                _isOpen = true;
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Bitmap frame;

            if (_isOpening || _isOpen)
                frame = _openingFrames[_currentFrame];
            else
                frame = _closedFrame;

            if (frame == null)
                return;

            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

            e.Graphics.DrawImage(
                frame,
                ClientRectangle,
                new Rectangle(0, 0, FrameWidth, FrameHeight),
                GraphicsUnit.Pixel
            );
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_closedFrame != null)
                {
                    _closedFrame.Dispose();
                    _closedFrame = null;
                }

                foreach (Bitmap frame in _openingFrames)
                {
                    if (frame != null)
                        frame.Dispose();
                }

                _openingFrames.Clear();
            }

            base.Dispose(disposing);
        }
    }
}