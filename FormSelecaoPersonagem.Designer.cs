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
            this.BTN_MENU = new System.Windows.Forms.Button();
            this.BTN_EXIT = new System.Windows.Forms.Button();
            this.object_628b8b28_5e26_40ac_91b9_3afcb0fe528f = new System.Windows.Forms.Button();
            this.TMR_Duck_Mage = new System.Windows.Forms.Timer(this.components);
            this.TMR_Music_Knight = new System.Windows.Forms.Timer(this.components);
            this.TMR_Music_Princess = new System.Windows.Forms.Timer(this.components);
            this.TMR_Music_choose_charactes = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // RDB_Mage
            // 
            resources.ApplyResources(this.RDB_Mage, "RDB_Mage");
            this.RDB_Mage.BackColor = System.Drawing.Color.Transparent;
            this.RDB_Mage.BackgroundImage = global::FallenWorld.Properties.Resources.duckMageFrame;
            this.RDB_Mage.FlatAppearance.BorderSize = 0;
            this.RDB_Mage.Name = "RDB_Mage";
            this.RDB_Mage.TabStop = true;
            this.RDB_Mage.UseVisualStyleBackColor = false;
            // 
            // RDB_Knight
            // 
            resources.ApplyResources(this.RDB_Knight, "RDB_Knight");
            this.RDB_Knight.BackColor = System.Drawing.Color.Transparent;
            this.RDB_Knight.BackgroundImage = global::FallenWorld.Properties.Resources.knightFrame;
            this.RDB_Knight.FlatAppearance.BorderSize = 0;
            this.RDB_Knight.Name = "RDB_Knight";
            this.RDB_Knight.TabStop = true;
            this.RDB_Knight.UseVisualStyleBackColor = false;
            // 
            // RDB_Princess
            // 
            resources.ApplyResources(this.RDB_Princess, "RDB_Princess");
            this.RDB_Princess.BackColor = System.Drawing.Color.Transparent;
            this.RDB_Princess.BackgroundImage = global::FallenWorld.Properties.Resources.princessFrame;
            this.RDB_Princess.FlatAppearance.BorderSize = 0;
            this.RDB_Princess.Name = "RDB_Princess";
            this.RDB_Princess.TabStop = true;
            this.RDB_Princess.UseVisualStyleBackColor = false;
            // 
            // BTN_PLAY
            // 
            resources.ApplyResources(this.BTN_PLAY, "BTN_PLAY");
            this.BTN_PLAY.BackColor = System.Drawing.Color.Transparent;
            this.BTN_PLAY.FlatAppearance.BorderSize = 0;
            this.BTN_PLAY.Image = global::FallenWorld.Properties.Resources.Jogar;
            this.BTN_PLAY.Name = "BTN_PLAY";
            this.BTN_PLAY.UseVisualStyleBackColor = false;
            // 
            // BTN_MENU
            // 
            resources.ApplyResources(this.BTN_MENU, "BTN_MENU");
            this.BTN_MENU.BackColor = System.Drawing.Color.Transparent;
            this.BTN_MENU.FlatAppearance.BorderSize = 0;
            this.BTN_MENU.Name = "BTN_MENU";
            this.BTN_MENU.UseVisualStyleBackColor = false;
            // 
            // BTN_EXIT
            // 
            resources.ApplyResources(this.BTN_EXIT, "BTN_EXIT");
            this.BTN_EXIT.BackColor = System.Drawing.Color.Transparent;
            this.BTN_EXIT.FlatAppearance.BorderSize = 0;
            this.BTN_EXIT.Image = global::FallenWorld.Properties.Resources.Sair;
            this.BTN_EXIT.Name = "BTN_EXIT";
            this.BTN_EXIT.UseVisualStyleBackColor = false;
            // 
            // object_628b8b28_5e26_40ac_91b9_3afcb0fe528f
            // 
            resources.ApplyResources(this.object_628b8b28_5e26_40ac_91b9_3afcb0fe528f, "object_628b8b28_5e26_40ac_91b9_3afcb0fe528f");
            this.object_628b8b28_5e26_40ac_91b9_3afcb0fe528f.Name = "object_628b8b28_5e26_40ac_91b9_3afcb0fe528f";
            this.object_628b8b28_5e26_40ac_91b9_3afcb0fe528f.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Name = "panel1";
            // 
            // FormSelecaoPersonagem
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.BTN_EXIT);
            this.Controls.Add(this.BTN_MENU);
            this.Controls.Add(this.BTN_PLAY);
            this.Controls.Add(this.RDB_Princess);
            this.Controls.Add(this.RDB_Knight);
            this.Controls.Add(this.RDB_Mage);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormSelecaoPersonagem";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton RDB_Mage;
        private System.Windows.Forms.RadioButton RDB_Knight;
        private System.Windows.Forms.RadioButton RDB_Princess;
        private System.Windows.Forms.Button BTN_PLAY;
        private System.Windows.Forms.Button BTN_MENU;
        private System.Windows.Forms.Button BTN_EXIT;
        private System.Windows.Forms.Button object_628b8b28_5e26_40ac_91b9_3afcb0fe528f;
        private System.Windows.Forms.Timer TMR_Duck_Mage;
        private System.Windows.Forms.Timer TMR_Music_Knight;
        private System.Windows.Forms.Timer TMR_Music_Princess;
        private System.Windows.Forms.Timer TMR_Music_choose_charactes;
        private System.Windows.Forms.Panel panel1;
    }
}