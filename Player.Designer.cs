namespace FallenWorld
{
    partial class Player
    {
        /// <summary> 
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Designer de Componentes

        /// <summary> 
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.PBX_PLAYER = new System.Windows.Forms.PictureBox();
            this.TMR_Animation = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.PBX_PLAYER)).BeginInit();
            this.SuspendLayout();
            // 
            // PBX_PLAYER
            // 
            this.PBX_PLAYER.BackColor = System.Drawing.Color.Transparent;
            this.PBX_PLAYER.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PBX_PLAYER.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PBX_PLAYER.Location = new System.Drawing.Point(0, 0);
            this.PBX_PLAYER.Name = "PBX_PLAYER";
            this.PBX_PLAYER.Size = new System.Drawing.Size(64, 64);
            this.PBX_PLAYER.TabIndex = 0;
            this.PBX_PLAYER.TabStop = false;
            // 
            // TMR_Animation
            // 
            this.TMR_Animation.Enabled = true;
            this.TMR_Animation.Tick += new System.EventHandler(this.TMR_Animation_Tick);
            // 
            // Player
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.PBX_PLAYER);
            this.DoubleBuffered = true;
            this.Name = "Player";
            this.Size = new System.Drawing.Size(64, 64);
            ((System.ComponentModel.ISupportInitialize)(this.PBX_PLAYER)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox PBX_PLAYER;
        private System.Windows.Forms.Timer TMR_Animation;
    }
}
