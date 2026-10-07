namespace FallenWorld
{
    partial class FormSelecaoPersonagem
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSelecaoPersonagem));
            this.RDB_Mage = new System.Windows.Forms.RadioButton();
            this.RDB_Knight = new System.Windows.Forms.RadioButton();
            this.RDB_Princess = new System.Windows.Forms.RadioButton();
            this.BTN_PLAY = new System.Windows.Forms.Button();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.TMR_Duck_Mage = new System.Windows.Forms.Timer(this.components);
            this.TMR_Music_Knight = new System.Windows.Forms.Timer(this.components);
            this.TMR_Music_Princess = new System.Windows.Forms.Timer(this.components);
            this.TMR_Music_choose_charactes = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.PNL_Description_Character = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // RDB_Mage
            // 
            resources.ApplyResources(this.RDB_Mage, "RDB_Mage");
            this.RDB_Mage.BackColor = System.Drawing.Color.Transparent;
            this.RDB_Mage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.RDB_Mage.FlatAppearance.BorderSize = 0;
            this.RDB_Mage.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.RDB_Mage.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.RDB_Mage.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.RDB_Mage.Image = global::FallenWorld.Properties.Resources.duckMage_Avatar_normal;
            this.RDB_Mage.Name = "RDB_Mage";
            this.RDB_Mage.UseVisualStyleBackColor = false;
            this.RDB_Mage.CheckedChanged += new System.EventHandler(this.checkPersonagem);
            this.RDB_Mage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.duckMage_MouseDown);
            this.RDB_Mage.MouseEnter += new System.EventHandler(this.duckMage_MouseEnter);
            this.RDB_Mage.MouseLeave += new System.EventHandler(this.duckMage_MouseLeave);
            this.RDB_Mage.MouseUp += new System.Windows.Forms.MouseEventHandler(this.duckMage_MouseUp);
            // 
            // RDB_Knight
            // 
            resources.ApplyResources(this.RDB_Knight, "RDB_Knight");
            this.RDB_Knight.BackColor = System.Drawing.Color.Transparent;
            this.RDB_Knight.Cursor = System.Windows.Forms.Cursors.Hand;
            this.RDB_Knight.FlatAppearance.BorderSize = 0;
            this.RDB_Knight.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.RDB_Knight.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.RDB_Knight.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.RDB_Knight.Image = global::FallenWorld.Properties.Resources.knight_Avatar_normal;
            this.RDB_Knight.Name = "RDB_Knight";
            this.RDB_Knight.UseVisualStyleBackColor = false;
            this.RDB_Knight.CheckedChanged += new System.EventHandler(this.checkPersonagem);
            this.RDB_Knight.MouseDown += new System.Windows.Forms.MouseEventHandler(this.knight_MouseDown);
            this.RDB_Knight.MouseEnter += new System.EventHandler(this.knight_MouseEnter);
            this.RDB_Knight.MouseLeave += new System.EventHandler(this.knight_MouseLeave);
            this.RDB_Knight.MouseUp += new System.Windows.Forms.MouseEventHandler(this.knight_MouseUp);
            // 
            // RDB_Princess
            // 
            resources.ApplyResources(this.RDB_Princess, "RDB_Princess");
            this.RDB_Princess.BackColor = System.Drawing.Color.Transparent;
            this.RDB_Princess.Cursor = System.Windows.Forms.Cursors.Hand;
            this.RDB_Princess.FlatAppearance.BorderSize = 0;
            this.RDB_Princess.FlatAppearance.CheckedBackColor = System.Drawing.Color.Transparent;
            this.RDB_Princess.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.RDB_Princess.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.RDB_Princess.Image = global::FallenWorld.Properties.Resources.Princess_Avatar_normal;
            this.RDB_Princess.Name = "RDB_Princess";
            this.RDB_Princess.UseVisualStyleBackColor = true;
            this.RDB_Princess.CheckedChanged += new System.EventHandler(this.checkPersonagem);
            this.RDB_Princess.MouseDown += new System.Windows.Forms.MouseEventHandler(this.princess_MouseDown);
            this.RDB_Princess.MouseEnter += new System.EventHandler(this.princess_MouseEnter);
            this.RDB_Princess.MouseLeave += new System.EventHandler(this.princess_MouseLeave);
            this.RDB_Princess.MouseUp += new System.Windows.Forms.MouseEventHandler(this.princess_MouseUp);
            // 
            // BTN_PLAY
            // 
            this.BTN_PLAY.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.BTN_PLAY, "BTN_PLAY");
            this.BTN_PLAY.FlatAppearance.BorderSize = 0;
            this.BTN_PLAY.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.BTN_PLAY.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.BTN_PLAY.Image = global::FallenWorld.Properties.Resources.Play_normal;
            this.BTN_PLAY.Name = "BTN_PLAY";
            this.BTN_PLAY.TabStop = false;
            this.BTN_PLAY.UseVisualStyleBackColor = true;
            this.BTN_PLAY.Click += new System.EventHandler(this.BTN_Jogar_Click);
            this.BTN_PLAY.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BTN_PLAY_MouseDown);
            this.BTN_PLAY.MouseEnter += new System.EventHandler(this.BTN_PLAY_MouseEnter);
            this.BTN_PLAY.MouseLeave += new System.EventHandler(this.BTN_PLAY_MouseLeave);
            this.BTN_PLAY.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BTN_PLAY_MouseUp);
            // 
            // BTN_EXIT
            // 
            this.BTN_EXIT.BackColor = System.Drawing.Color.Transparent;
            this.BTN_EXIT.BackgroundImage = global::FallenWorld.Properties.Resources.Exit_normal;
            resources.ApplyResources(this.BTN_EXIT, "BTN_EXIT");
            this.BTN_EXIT.FlatAppearance.BorderSize = 0;
            this.BTN_EXIT.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.BTN_EXIT.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            this.BTN_EXIT.Click += new System.EventHandler(this.BTN_EXIT_Click);
            this.BTN_EXIT.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BTN_EXIT_MouseDown);
            this.BTN_EXIT.MouseEnter += new System.EventHandler(this.BTN_EXIT_MouseEnter);
            this.BTN_EXIT.MouseLeave += new System.EventHandler(this.BTN_EXIT_MouseLeave);
            this.BTN_EXIT.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BTN_EXIT_MouseUp);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // PNL_Description_Character
            // 
            resources.ApplyResources(this.PNL_Description_Character, "PNL_Description_Character");
            this.PNL_Description_Character.BackColor = System.Drawing.Color.Transparent;
            this.PNL_Description_Character.Name = "PNL_Description_Character";
            // 
            // FormSelecaoPersonagem
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.PNL_Description_Character);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.BTN_PLAY);
            this.Controls.Add(this.RDB_Princess);
            this.Controls.Add(this.RDB_Knight);
            this.Controls.Add(this.RDB_Mage);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSelecaoPersonagem";
            this.Load += new System.EventHandler(this.FormSelecaoPersonagem_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton RDB_Mage;
        private System.Windows.Forms.RadioButton RDB_Knight;
        private System.Windows.Forms.RadioButton RDB_Princess;
        private System.Windows.Forms.Button BTN_PLAY;
        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.Timer TMR_Duck_Mage;
        private System.Windows.Forms.Timer TMR_Music_Knight;
        private System.Windows.Forms.Timer TMR_Music_Princess;
        private System.Windows.Forms.Timer TMR_Music_choose_charactes;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel PNL_Description_Character;
    }
}
