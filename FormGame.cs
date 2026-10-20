using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.AxHost;
using static FallenWorld.Player;

namespace FallenWorld
{
    //, IMessageFilter
    public partial class FormGame : Form
    {
        #region Global Variables
        private readonly Personagem _personagem;
        private const int VelocidadePlayer = 4;
        private bool _attack0KeyHeld = false;
        private bool _attack1KeyHeld = false;
        private bool _attack2KeyHeld = false;
        private Player player;
        //private Enemy enemyOrc;
        private Maps map;
        private Bitmap _imageMap;
        #endregion

        public FormGame(Personagem personagem)
        {
            InitializeComponent();
            _personagem = personagem;
        }

        #region Form Load
        private void FormGame_Load(object sender, EventArgs e)
        {
            // Esconder o cursor.
            Cursor.Hide();

            // Bloquear os eventos do mouse.
            //Application.AddMessageFilter(this);
            Load_Map();
            switch (_personagem)
            {
                case Personagem.Mago:
                    PBX_Avatar.BackgroundImage = Properties.Resources.duckMage_Avatar_normal;
                    PBX_HealthBar.BackgroundImage = Properties.Resources.duckMage_HealthBar_Full;
                    PBX_MagicBar.BackgroundImage = Properties.Resources.duckMage_MagicBar_Full;
                    BTN_Skill1.BackgroundImage = Properties.Resources.duck_Mage_Skill1_disabled;
                    BTN_Skill2.BackgroundImage = Properties.Resources.duckMage_Skill2_disabled;
                    BTN_Skill3.BackgroundImage = Properties.Resources.duckMage_Skill3_disabled;
                    break;
                case Personagem.Cavaleiro:
                    PBX_Avatar.BackgroundImage = Properties.Resources.knight_Avatar_normal;
                    PBX_HealthBar.BackgroundImage = Properties.Resources.Knight_HealthBar_Full;
                    //BTN_Skill1.Image = Properties.Resources.knight_Skill1_disabled;
                    //BTN_Skill2.Image = Properties.Resources.knight_Skill2_disabled;
                    PBX_MagicBar.Visible = false; // Assuming the knight doesn't have a magic bar
                    BTN_Skill3.Visible = false; // Assuming the knight has only 2 skills
                    break;
                case Personagem.Princesa:
                    PBX_Avatar.BackgroundImage = Properties.Resources.Princess_Avatar_normal;
                    PBX_HealthBar.BackgroundImage = Properties.Resources.Princess_HealthBar_Full;
                    BTN_Skill1.BackgroundImage = Properties.Resources.princess_Skill1_disabled;
                    BTN_Skill2.BackgroundImage = Properties.Resources.princess_Skill2_disabled;
                    PBX_MagicBar.Visible = false; // Assuming the knight doesn't have a magic bar
                    BTN_Skill3.Visible = false; // Assuming the princess has only 2 skills
                    break;
            }
            player = new Player(_personagem);

            MapTile spawn = map.GetSpawnPlayer();

            if (spawn == null)
            {
                MessageBox.Show("Spawn do jogador não encontrado.");
                return;
            }

            Rectangle spriteBounds = player.GetVisibleSpriteBounds();

            int spawnX = spawn.x * map.tileSize;
            int spawnY = spawn.y * map.tileSize;

            // Centralizar o controle no tile.
            // Alinhar a parte inferior do controle ao topo do chão.
            int playerX = spawnX + (map.tileSize - player.Width) / 2;
            int playerY = spawnY - player.Height;

            player.Location = new Point(playerX, playerY);

            PNL_ViewPort.Controls.Add(player);
            player.BringToFront();
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
            // BASIC ATTACKS
            if (e.KeyCode == Core.Attack0 && !_attack0KeyHeld)
            {
                _attack0KeyHeld = true;
                Core.isAttacking0 = true;
            }
            // Skill 1
            if (e.KeyCode == Core.Attack1 && !_attack1KeyHeld)
            {
                _attack1KeyHeld = true;
                Core.isAttacking1 = true;
            }
            // Skill 2
            if (e.KeyCode == Core.Attack2 && !_attack2KeyHeld)
            {
                _attack2KeyHeld = true;
                Core.isAttacking2 = true;
            }
            // Skill 3
            if (e.KeyCode == Core.Exit)
            {
                BTN_EXIT.Image = Properties.Resources.Exit_pressed;
                return;
            }
        }
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Core.MoveLeft)
                Core.isMovingLeft = false;
            if (e.KeyCode == Core.MoveRight)
                Core.isMovingRight = false;
            if (e.KeyCode == Core.jump)
                Core.isJumping = false;
            if (e.KeyCode == Core.Attack0)
            {
                _attack0KeyHeld = false;
                Core.isAttacking0 = false;
            }
            if (e.KeyCode == Core.Attack1)
            {
                _attack1KeyHeld = false;
                Core.isAttacking1 = false;
            }
            if (e.KeyCode == Core.Attack2)
            {
                _attack2KeyHeld = false;
                Core.isAttacking2 = false;
            }
            if (e.KeyCode == Core.Exit)
            {
                BTN_EXIT.Image = Properties.Resources.Exit_normal;
                this.Close();
                return;
            }
        }

        private bool TentarMoverHorizontal(int deslocamento)
        {
            int proximoX = player.Left + deslocamento;

            Rectangle hitbox = player.GetHitboxAt(
                proximoX,
                player.Top
            );

            if (!map.PodeMover(hitbox))
                return false;

            player.Left = proximoX;
            return true;
        }
        #endregion

        #region Perda de vida e morte do personagem

        #endregion

        #region Skill buttons states
        private void SkillButtonStates(object sender, EventArgs e)
        {
            // Implement skill 1 logic here
        }
        #endregion

        #region Animations
        private void TMR_Game_Tick(object sender, EventArgs e)
        {
            if (player == null || player.IsDisposed)
                return;

            // Ataque básico
            if (Core.isAttacking0)
            {
                Core.isAttacking0 = false;
                player.SetAttack();
            }

            // Habilidade 1: Fireball
            if (Core.isAttacking1)
            {
                Core.isAttacking1 = false;
                player.SetFireball();
            }

            // Habilidade 2: ataque temporário
            if (Core.isAttacking2)
            {
                Core.isAttacking2 = false;
                player.SetAttack();
            }

            // Não movimenta o personagem durante uma ação.
            if (player.IsPerformingAction)
                return;

            // Movimentação
            if (Core.isMovingLeft && !Core.isMovingRight)
            {
                if (TentarMoverHorizontal(-VelocidadePlayer))
                    player.SetWalk(false);
                else
                    player.SetIdle();
            }
            else if (Core.isMovingRight && !Core.isMovingLeft)
            {
                if (TentarMoverHorizontal(VelocidadePlayer))
                    player.SetWalk(true);
                else
                    player.SetIdle();
            }
            else
            {
                player.SetIdle();
            }
        }
        #endregion

        #region Load Map
        private void Load_Map()
        {
            string mapJson = Properties.Resources.tutorial_Map_Js;
            JavaScriptSerializer readMapjson = new JavaScriptSerializer();

            map = readMapjson.Deserialize<Maps>(mapJson);

            if (map == null)
                throw new Exception("Não foi possível carregar o mapa.");

            // Preparar as colisões uma única vez.
            map.PrepararColisoes();

            Bitmap spritesheet = Properties.Resources.tutorial_Map_sprite;

            // A classe Maps monta a imagem completa do cenário.
            Bitmap novaImagem = map.CriarImagem(spritesheet);

            if (_imageMap != null)
                _imageMap.Dispose();

            _imageMap = novaImagem;

            PNL_ViewPort.BackgroundImage = _imageMap;
        }
        #endregion

        #region bloqueio do mouse
        // Bloquear mensagens do mouse enquanto o jogo estiver aberto.
        public bool PreFilterMessage(ref Message m)
        {
            // Mensagens de movimento e clique do mouse.
            if ((m.Msg >= 0x0200 && m.Msg <= 0x020E) || (m.Msg >= 0x00A0 && m.Msg <= 0x00AD))
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
