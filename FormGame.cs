using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static FallenWorld.Player;
using static System.Windows.Forms.AxHost;

namespace FallenWorld
{
    public partial class FormGame : Form
    {
        #region Global Variables
        private readonly Personagem _personagem;
        private Player player;
        //private readonly Personagem _infoCharactes;
        #endregion
        
        public FormGame(Personagem personagem)
        {
            InitializeComponent();
            _personagem = personagem;
        }


        #region Form Load
        private void FormGame_Load(object sender, EventArgs e)
        {
            switch (_personagem)
            {
                case Personagem.Mago:
                    PBX_Avatar.BackgroundImage = Properties.Resources.duckMage_Avatar_normal;
                    PBX_HealthBar.BackgroundImage = Properties.Resources.duckMage_HealthBar_Full;
                    PBX_MagicBar.BackgroundImage = Properties.Resources.duckMage_MagicBar_Full;
                    BTN_Skill1.Image = Properties.Resources.duck_Mage_Skill1_disabled;
                    BTN_Skill2.Image = Properties.Resources.duckMage_Skill2_disabled;
                    BTN_Skill3.Image = Properties.Resources.duckMage_Skill3_disabled;
                    player = new Player(_personagem);
                    player.Location = new Point(100, 400);
                    PNL_ViewPort.Controls.Add(player);
                    player.BringToFront();
                    break;
                case Personagem.Cavaleiro:
                    PBX_Avatar.BackgroundImage = Properties.Resources.knight_Avatar_normal;
                    PBX_HealthBar.BackgroundImage = Properties.Resources.Knight_HealthBar_Full;
                    //BTN_Skill1.Image = Properties.Resources.knight_Skill1_disabled;
                    //BTN_Skill2.Image = Properties.Resources.knight_Skill2_disabled;
                    PBX_MagicBar.Visible = false; // Assuming the knight doesn't have a magic bar
                    BTN_Skill3.Visible = false; // Assuming the knight has only 2 skills
                    player = new Player(_personagem);
                    player.BringToFront();
                    break;
                case Personagem.Princesa:
                    PBX_Avatar.BackgroundImage = Properties.Resources.Princess_Avatar_normal;
                    PBX_HealthBar.BackgroundImage = Properties.Resources.Princess_HealthBar_Full;
                    BTN_Skill1.Image = Properties.Resources.princess_Skill1_disabled;
                    BTN_Skill2.Image = Properties.Resources.princess_Skill2_disabled;
                    PBX_MagicBar.Visible = false; // Assuming the knight doesn't have a magic bar
                    BTN_Skill3.Visible = false; // Assuming the princess has only 2 skills
                    player = new Player(_personagem);
                    player.BringToFront();
                    break;
            }
            
        }
        #endregion

        #region Movement and attack logic
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Core.MoveLeft)
                Core.isMovingLeft = true;
            if (e.KeyCode == Core.MoveRight)
                Core.isMovingRight = true;
            if (e.KeyCode == Core.jump)
                Core.isJumping = true;
            if (e.KeyCode == Core.Attack0)
                Core.isAttacking0 = true;
            if (e.KeyCode == Core.Attack1)
                Core.isAttacking1 = true;
            if (e.KeyCode == Core.Attack2)
                Core.isAttacking2 = true;
            // Add logic for skill buttons if needed
            if (e.KeyCode == Core.Exit)
                BTN_EXIT.Image = Properties.Resources.Exit_pressed;
                this.Close();
        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {

        }
        #endregion

        #region cmmands temporarios
        //public void SetIdle()
        //{
        //    if (_state != StatePlayer.Idle)
        //        SetAnimation(StatePlayer.Idle);
        //}
        //public void SetWalk(bool facingRight)
        //{
        //    _facingRight = facingRight;

        //    if (_state != StatePlayer.Walk)
        //        SetAnimation(StatePlayer.Walk);
        //}
        //public void SetAttack()
        //{
        //    SetAnimation(StatePlayer.Attack);
        //}
        //public void SetDeath()
        //{
        //    SetAnimation(StatePlayer.Death);
        //}
        #endregion

        #region Perda de vida e morte do personagem

        #endregion

        #region Skill buttons states
        private void SkillButtonStates(object sender, EventArgs e)
        {
            // Implement skill 1 logic here
        }
        #endregion        
    }
}
