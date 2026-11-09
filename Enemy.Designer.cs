namespace FallenWorld
{
    partial class Enemy
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
            this.PBX_Enemy = new System.Windows.Forms.PictureBox();
            this.TMR_Animation = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.PBX_Enemy)).BeginInit();
            this.SuspendLayout();
            // 
            // PBX_Enemy
            // 
            this.PBX_Enemy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PBX_Enemy.Location = new System.Drawing.Point(0, 0);
            this.PBX_Enemy.Name = "PBX_Enemy";
            this.PBX_Enemy.Size = new System.Drawing.Size(112, 112);
            this.PBX_Enemy.TabIndex = 0;
            this.PBX_Enemy.TabStop = false;
            // 
            // TMR_Animation
            // 
            this.TMR_Animation.Enabled = true;
            this.TMR_Animation.Tick += new System.EventHandler(this.TMR_Animation_Tick);
            // 
            // Enemy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.PBX_Enemy);
            this.DoubleBuffered = true;
            this.Name = "Enemy";
            this.Size = new System.Drawing.Size(112, 112);
            ((System.ComponentModel.ISupportInitialize)(this.PBX_Enemy)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox PBX_Enemy;
        private System.Windows.Forms.Timer TMR_Animation;
    }
}
