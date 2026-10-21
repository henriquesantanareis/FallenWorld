namespace FallenWorld
{
    partial class FormGame
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

        #region Código gerado pelo Windows Form Designer

        /// <summary>
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGame));
            this.TMR_Game = new System.Windows.Forms.Timer(this.components);
            this.world1 = new FallenWorld.World();
            this.PBX_Avatar = new System.Windows.Forms.PictureBox();
            this.BTN_Skill3 = new System.Windows.Forms.Button();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.BTN_Skill2 = new System.Windows.Forms.Button();
            this.PBX_MagicBar = new System.Windows.Forms.PictureBox();
            this.PBX_HealthBar = new System.Windows.Forms.PictureBox();
            this.BTN_Skill1 = new System.Windows.Forms.Button();
            this.world1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PBX_Avatar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBX_MagicBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBX_HealthBar)).BeginInit();
            this.SuspendLayout();
            // 
            // TMR_Game
            // 
            this.TMR_Game.Enabled = true;
            this.TMR_Game.Interval = 16;
            this.TMR_Game.Tick += new System.EventHandler(this.TMR_Game_Tick);
            // 
            // world1
            // 
            this.world1.Controls.Add(this.PBX_Avatar);
            this.world1.Controls.Add(this.BTN_Skill3);
            this.world1.Controls.Add(this.BTN_EXIT);
            this.world1.Controls.Add(this.BTN_Skill2);
            this.world1.Controls.Add(this.PBX_MagicBar);
            this.world1.Controls.Add(this.PBX_HealthBar);
            this.world1.Controls.Add(this.BTN_Skill1);
            this.world1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.world1.Location = new System.Drawing.Point(0, 0);
            this.world1.Name = "world1";
            this.world1.Size = new System.Drawing.Size(784, 561);
            this.world1.TabIndex = 5;
            this.world1.Paint += new System.Windows.Forms.PaintEventHandler(this.world1_Paint);
            // 
            // PBX_Avatar
            // 
            this.PBX_Avatar.BackColor = System.Drawing.Color.Transparent;
            this.PBX_Avatar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PBX_Avatar.ErrorImage = null;
            this.PBX_Avatar.InitialImage = null;
            this.PBX_Avatar.Location = new System.Drawing.Point(3, 3);
            this.PBX_Avatar.Name = "PBX_Avatar";
            this.PBX_Avatar.Size = new System.Drawing.Size(103, 87);
            this.PBX_Avatar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.PBX_Avatar.TabIndex = 3;
            this.PBX_Avatar.TabStop = false;
            // 
            // BTN_Skill3
            // 
            this.BTN_Skill3.AutoSize = true;
            this.BTN_Skill3.BackColor = System.Drawing.Color.Transparent;
            this.BTN_Skill3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BTN_Skill3.FlatAppearance.BorderSize = 0;
            this.BTN_Skill3.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.BTN_Skill3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.BTN_Skill3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_Skill3.Location = new System.Drawing.Point(500, 12);
            this.BTN_Skill3.Name = "BTN_Skill3";
            this.BTN_Skill3.Size = new System.Drawing.Size(88, 23);
            this.BTN_Skill3.TabIndex = 2;
            this.BTN_Skill3.TabStop = false;
            this.BTN_Skill3.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_Skill3.UseVisualStyleBackColor = false;
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.AutoSize = true;
            this.BTN_EXIT.BackColor = System.Drawing.Color.Transparent;
            this.BTN_EXIT.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BTN_EXIT.FlatAppearance.BorderSize = 0;
            this.BTN_EXIT.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.BTN_EXIT.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.BTN_EXIT.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_EXIT.Image = global::FallenWorld.Properties.Resources.Exit_normal;
            this.BTN_EXIT.Location = new System.Drawing.Point(672, 4);
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.Size = new System.Drawing.Size(109, 46);
            this.BTN_EXIT.TabIndex = 4;
            this.BTN_EXIT.TabStop = false;
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.BTN_EXIT.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnKeyUp);
            // 
            // BTN_Skill2
            // 
            this.BTN_Skill2.AutoSize = true;
            this.BTN_Skill2.BackColor = System.Drawing.Color.Transparent;
            this.BTN_Skill2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BTN_Skill2.FlatAppearance.BorderSize = 0;
            this.BTN_Skill2.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.BTN_Skill2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.BTN_Skill2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_Skill2.Location = new System.Drawing.Point(419, 12);
            this.BTN_Skill2.Name = "BTN_Skill2";
            this.BTN_Skill2.Size = new System.Drawing.Size(75, 23);
            this.BTN_Skill2.TabIndex = 1;
            this.BTN_Skill2.TabStop = false;
            this.BTN_Skill2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_Skill2.UseVisualStyleBackColor = false;
            // 
            // PBX_MagicBar
            // 
            this.PBX_MagicBar.BackColor = System.Drawing.Color.Transparent;
            this.PBX_MagicBar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PBX_MagicBar.ErrorImage = null;
            this.PBX_MagicBar.InitialImage = null;
            this.PBX_MagicBar.Location = new System.Drawing.Point(112, 45);
            this.PBX_MagicBar.Name = "PBX_MagicBar";
            this.PBX_MagicBar.Size = new System.Drawing.Size(220, 40);
            this.PBX_MagicBar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.PBX_MagicBar.TabIndex = 1;
            this.PBX_MagicBar.TabStop = false;
            // 
            // PBX_HealthBar
            // 
            this.PBX_HealthBar.BackColor = System.Drawing.Color.Transparent;
            this.PBX_HealthBar.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.PBX_HealthBar.ErrorImage = null;
            this.PBX_HealthBar.InitialImage = null;
            this.PBX_HealthBar.Location = new System.Drawing.Point(112, 10);
            this.PBX_HealthBar.Name = "PBX_HealthBar";
            this.PBX_HealthBar.Size = new System.Drawing.Size(220, 40);
            this.PBX_HealthBar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.PBX_HealthBar.TabIndex = 2;
            this.PBX_HealthBar.TabStop = false;
            // 
            // BTN_Skill1
            // 
            this.BTN_Skill1.AutoSize = true;
            this.BTN_Skill1.BackColor = System.Drawing.Color.Transparent;
            this.BTN_Skill1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.BTN_Skill1.FlatAppearance.BorderSize = 0;
            this.BTN_Skill1.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.BTN_Skill1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.BTN_Skill1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_Skill1.Location = new System.Drawing.Point(338, 10);
            this.BTN_Skill1.Name = "BTN_Skill1";
            this.BTN_Skill1.Size = new System.Drawing.Size(75, 25);
            this.BTN_Skill1.TabIndex = 0;
            this.BTN_Skill1.TabStop = false;
            this.BTN_Skill1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.BTN_Skill1.UseVisualStyleBackColor = false;
            // 
            // FormGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.ControlBox = false;
            this.Controls.Add(this.world1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "FormGame";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Jogo";
            this.Load += new System.EventHandler(this.FormGame_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnKeyUp);
            this.world1.ResumeLayout(false);
            this.world1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PBX_Avatar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBX_MagicBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PBX_HealthBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.PictureBox PBX_MagicBar;
        private System.Windows.Forms.PictureBox PBX_HealthBar;
        private System.Windows.Forms.Button BTN_Skill3;
        private System.Windows.Forms.Button BTN_Skill2;
        private System.Windows.Forms.Button BTN_Skill1;
        private System.Windows.Forms.PictureBox PBX_Avatar;
        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.Timer TMR_Game;
        private World world1;
    }
}

