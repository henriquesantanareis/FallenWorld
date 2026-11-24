
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FallenWorld
{
    public class QueenPoisonWave : UserControl
    {
        private int _direction;
        private int _animationTick;
        private int _explosionTick;

        public int WorldX { get; private set; }
        public int WorldY { get; private set; }

        public int Speed { get; set; } = 4;

        public bool IsExploding { get; private set; }
        public bool IsFinished { get; private set; }

        public QueenPoisonWave(int worldX, int worldY, int direction)
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor,
                true
            );

            BackColor = Color.Transparent;
            Size = new Size(96, 56);

            WorldX = worldX;
            WorldY = worldY;

            _direction = direction >= 0 ? 1 : -1;

            TabStop = false;
        }

        public Rectangle GetCollisionBounds()
        {
            if (IsExploding || IsFinished)
                return Rectangle.Empty;

            return new Rectangle(
                WorldX + 14,
                WorldY + 30,
                Width - 28,
                18
            );
        }

        public void UpdateWave(int playerCenterX)
        {
            if (IsFinished)
                return;

            if (IsExploding)
            {
                _explosionTick++;

                if (_explosionTick >= 14)
                    IsFinished = true;
            }
            else
            {
                // A onda acompanha a posição horizontal do jogador.
                int centroOnda = WorldX + Width / 2;
                int diferenca = playerCenterX - centroOnda;

                if (Math.Abs(diferenca) > 10)
                    _direction = diferenca > 0 ? 1 : -1;

                WorldX += _direction * Speed;

                _animationTick++;
            }

            Invalidate();
        }

        public void StartExplosion()
        {
            if (IsExploding || IsFinished)
                return;

            IsExploding = true;
            _explosionTick = 0;

            Invalidate();
        }

        private int AjustarX(int x, int width)
        {
            return _direction > 0
                ? x
                : Width - x - width;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            if (IsFinished)
                return;

            if (IsExploding)
            {
                DesenharExplosao(g);
                return;
            }

            DesenharOnda(g);
        }

        private void DesenharOnda(Graphics g)
        {
            int movimento = (_animationTick % 4) * 2;

            using (SolidBrush sombra = new SolidBrush(
                Color.FromArgb(190, 5, 35, 15)))
            using (SolidBrush verdeEscuro = new SolidBrush(
                Color.FromArgb(230, 10, 85, 30)))
            using (SolidBrush verde = new SolidBrush(
                Color.FromArgb(235, 35, 160, 55)))
            using (SolidBrush brilho = new SolidBrush(
                Color.FromArgb(240, 115, 255, 100)))
            {
                // Sombra junto ao chão.
                g.FillRectangle(sombra, 10, 42, 76, 8);

                // Corpo principal da onda.
                g.FillRectangle(
                    verdeEscuro,
                    AjustarX(16, 64),
                    30,
                    64,
                    12
                );

                g.FillRectangle(
                    verde,
                    AjustarX(23, 50),
                    22 - movimento / 2,
                    50,
                    12
                );

                g.FillRectangle(
                    verde,
                    AjustarX(31, 34),
                    14 + movimento / 2,
                    34,
                    10
                );

                // Ponta ascendente da energia.
                g.FillRectangle(
                    verdeEscuro,
                    AjustarX(65, 12),
                    24,
                    12,
                    14
                );

                // Núcleo luminoso.
                g.FillRectangle(
                    brilho,
                    AjustarX(30, 34),
                    28,
                    34,
                    5
                );

                g.FillRectangle(
                    brilho,
                    AjustarX(40, 18),
                    20,
                    18,
                    5
                );

                // Partículas da energia.
                g.FillRectangle(brilho, 23, 12 + movimento, 5, 5);
                g.FillRectangle(brilho, 65, 18 - movimento / 2, 5, 5);
                g.FillRectangle(brilho, 45, 8 + movimento / 2, 4, 4);
            }
        }

        private void DesenharExplosao(Graphics g)
        {
            int fase = _explosionTick;

            int tamanho = 12 + fase * 3;
            int centroX = Width / 2;
            int centroY = Height / 2;

            using (SolidBrush sombra = new SolidBrush(
                Color.FromArgb(180, 5, 55, 20)))
            using (SolidBrush verde = new SolidBrush(
                Color.FromArgb(220, 30, 180, 55)))
            using (SolidBrush brilho = new SolidBrush(
                Color.FromArgb(235, 145, 255, 100)))
            {
                g.FillRectangle(
                    sombra,
                    centroX - tamanho / 2 - 5,
                    centroY - tamanho / 2 - 5,
                    tamanho + 10,
                    tamanho + 10
                );

                g.FillRectangle(
                    verde,
                    centroX - tamanho / 2,
                    centroY - tamanho / 2,
                    tamanho,
                    tamanho
                );

                if (fase < 8)
                {
                    g.FillRectangle(
                        brilho,
                        centroX - tamanho / 4,
                        centroY - tamanho / 4,
                        tamanho / 2,
                        tamanho / 2
                    );
                }

                // Partículas externas.
                int deslocamento = fase * 3;

                g.FillRectangle(
                    brilho,
                    centroX - 30 - deslocamento,
                    centroY - 12,
                    6,
                    6
                );

                g.FillRectangle(
                    brilho,
                    centroX + 24 + deslocamento,
                    centroY + 8,
                    6,
                    6
                );

                g.FillRectangle(
                    verde,
                    centroX - 8,
                    centroY - 25 - deslocamento,
                    6,
                    6
                );
            }
        }
    }
}
