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
    public partial class FormGame : Form, IMessageFilter
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
        private int _playerWorldX;
        private int _playerWorldY;
        private int _cameraX;
        private int _cameraY;
        // Física vertical do personagem
        private const float ForcaPulo = -10.0f;
        private const float Gravidade = 0.55f;

        private float _velocidadeVertical = 0f;
        private bool _estaNoChao = false;

        // Impede que o W pressionado continuamente
        // dispare vários pulos automaticamente.
        private bool _jumpKeyHeld = false;
        private bool _pedidoPulo = false;
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
            Application.AddMessageFilter(this);
            Load_Map();
            switch (_personagem)
            {
                case Personagem.Mago:
                    PBX_Avatar.Image = Properties.Resources.duckMage_Avatar_normal;
                    PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_Full;
                    PBX_MagicBar.Image = Properties.Resources.duckMage_MagicBar_Full;
                    BTN_Skill1.Image = Properties.Resources.duck_Mage_Skill1_disabled;
                    BTN_Skill2.Image = Properties.Resources.duckMage_Skill2_disabled;
                    BTN_Skill3.Image = Properties.Resources.duckMage_Skill3_disabled;
                    break;
                case Personagem.Cavaleiro:
                    PBX_Avatar.Image = Properties.Resources.knight_Avatar_normal;
                    PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_Full;
                    BTN_Skill1.Image = Properties.Resources.knight_Skill1_disabled;
                    BTN_Skill2.Image = Properties.Resources.knight_Skill2_disabled;
                    PBX_MagicBar.Visible = false;
                    BTN_Skill3.Visible = false;
                    break;
                case Personagem.Princesa:
                    PBX_Avatar.Image = Properties.Resources.Princess_Avatar_normal;
                    PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_Full;
                    BTN_Skill1.Image = Properties.Resources.princess_Skill1_disabled;
                    BTN_Skill2.Image = Properties.Resources.princess_Skill2_disabled;
                    PBX_MagicBar.Visible = false;
                    BTN_Skill3.Visible = false;
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

            _playerWorldX = playerX;
            _playerWorldY = playerY;

            player.Location = new Point(playerX, playerY);

            world1.Controls.Add(player);
            _velocidadeVertical = 0f;
            _estaNoChao = TemChaoAbaixo();
            AtualizarCamera();
            AtualizarPosicaoPlayerNaTela();
            player.BringToFront();
        }
        #endregion

        #region Movement
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Core.MoveLeft)
                Core.isMovingLeft = true;
            if (e.KeyCode == Core.MoveRight)
                Core.isMovingRight = true;
            if (e.KeyCode == Core.jump)
            {
                Core.isJumping = true;
                _jumpKeyHeld = true;
                _pedidoPulo = true;
            }
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
            // Skill 3
            if (e.KeyCode == Core.Attack2 && !_attack2KeyHeld)
            {
                _attack2KeyHeld = true;
                Core.isAttacking2 = true;
            }
            // Exit game
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
            {
                Core.isJumping = false;
                _jumpKeyHeld = false;
                _pedidoPulo = false;
            }
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
            // Calcula a próxima posição no mundo.
            int proximoX = _playerWorldX + deslocamento;

            // Verifica a colisão usando as coordenadas do mundo.
            Rectangle hitbox = player.GetHitboxAt(
                proximoX,
                _playerWorldY
            );

            if (!map.PodeMover(hitbox))
                return false;

            // Atualiza a posição real do personagem.
            _playerWorldX = proximoX;

            // A câmera acompanha o personagem.
            AtualizarCamera();

            // Atualiza a posição visual e solicita o redesenho.
            AtualizarPosicaoPlayerNaTela();

            return true;
        }

        private bool TentarMoverVertical(int deslocamento)
        {
            if (deslocamento == 0)
                return true;

            int direcao = Math.Sign(deslocamento);
            int quantidadePassos = Math.Abs(deslocamento);

            for (int i = 0; i < quantidadePassos; i++)
            {
                int proximoY = _playerWorldY + direcao;

                Rectangle hitbox = player.GetHitboxAt(
                    _playerWorldX,
                    proximoY
                );

                if (!map.PodeMover(hitbox))
                {
                    // Parou ao tocar o teto ou o chão.
                    _velocidadeVertical = 0f;

                    // Colisão durante a descida significa aterrissagem.
                    if (direcao > 0)
                        _estaNoChao = true;

                    return false;
                }

                _playerWorldY = proximoY;
            }

            return true;
        }

        private bool TemChaoAbaixo()
        {
            Rectangle hitbox = player.GetHitboxAt(
                _playerWorldX,
                _playerWorldY
            );

            // Uma faixa de 1 pixel na parte inferior da hitbox.
            Rectangle faixaDosPes = new Rectangle(
                hitbox.Left + 2,
                hitbox.Bottom,
                Math.Max(1, hitbox.Width - 4),
                1
            );

            return !map.PodeMover(faixaDosPes);
        }

        private void AtualizarFisicaVertical()
        {
            if (player == null || map == null)
                return;

            int yAnterior = _playerWorldY;
            bool estavaNoChao = _estaNoChao;

            // Se saiu da borda de uma plataforma, começa a cair.
            if (_estaNoChao && !TemChaoAbaixo())
            {
                _estaNoChao = false;
                player.SetJump();
            }

            // Enquanto não estiver apoiado no chão, aplica gravidade.
            if (!_estaNoChao)
            {
                _velocidadeVertical += Gravidade;

                int deslocamentoY = (int)Math.Round(
                    _velocidadeVertical
                );

                if (deslocamentoY != 0)
                    TentarMoverVertical(deslocamentoY);
            }

            // Ao aterrissar, encerra a animação de pulo.
            if (!estavaNoChao && _estaNoChao)
                player.SetGrounded();

            // Atualiza câmera e desenho se a posição vertical mudou.
            if (_playerWorldY != yAnterior)
            {
                AtualizarCamera();
                AtualizarPosicaoPlayerNaTela();
            }
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

            switch (_personagem)
            {
                case Personagem.Mago:
                    // Habilidade 0: Staff Attack
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

                    // Habilidade 2: Recover Mana
                    if (Core.isAttacking2)
                    {
                        Core.isAttacking2 = false;
                        player.SetRecover_Mana();
                    }
                    break;
                case Personagem.Cavaleiro:
                    // Habilidade 1: Reforço
                    if (Core.isAttacking0)
                    {
                        Core.isAttacking0 = false;
                        player.SetAura();
                    }

                    // Habilidade 2: Sword Slash
                    if (Core.isAttacking1)
                    {
                        Core.isAttacking1 = false;
                        player.SetAttack();
                    }
                    break;
                case Personagem.Princesa:
                    /*if (Core.isAttack0)
                    {
                        
                    }*/
                    //Habilidade 2: Poison Attack
                    if (Core.isAttacking1)
                    {
                        Core.isAttacking1 = false;
                        player.SetPoisonAttack();
                    }
                    break;
            }
            

            // Guarda a solicitação e consome o pressionamento.
            bool solicitouPulo = _pedidoPulo;
            _pedidoPulo = false;

            // Só inicia o pulo se o jogador estiver no chão
            // e não estiver executando uma ação bloqueante.
            if (!player.IsPerformingAction &&
                solicitouPulo &&
                _estaNoChao)
            {
                _velocidadeVertical = ForcaPulo;
                _estaNoChao = false;

                player.SetJump();
            }

            // O movimento horizontal continua permitido no ar.
            if (!player.IsPerformingAction)
            {
                if (Core.isMovingLeft && !Core.isMovingRight)
                {
                    if (TentarMoverHorizontal(-VelocidadePlayer))
                    {
                        if (_estaNoChao)
                            player.SetWalk(false);
                        else
                            player.SetJump(false);
                    }
                    else if (_estaNoChao)
                    {
                        player.SetIdle();
                    }
                }
                else if (Core.isMovingRight && !Core.isMovingLeft)
                {
                    if (TentarMoverHorizontal(VelocidadePlayer))
                    {
                        if (_estaNoChao)
                            player.SetWalk(true);
                        else
                            player.SetJump(true);
                    }
                    else if (_estaNoChao)
                    {
                        player.SetIdle();
                    }
                }
                else
                {
                    if (_estaNoChao)
                        player.SetIdle();
                    else
                        player.SetJump();
                }
            }

            // A física deve continuar mesmo durante animações de ataque.
            AtualizarFisicaVertical();
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
            // Remove o uso da imagem como BackgroundImage.
            world1.BackgroundImage = null;
            world1.Invalidate();
        }
        private void world1_Paint(object sender, PaintEventArgs e)
        {
            if (_imageMap == null)
                return;

            // Desenha o mapa na posição inicial.
            // O painel exibirá apenas a parte que cabe na tela.
            e.Graphics.DrawImageUnscaled(_imageMap, -_cameraX, -_cameraY);
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

        #region Camera and player position
        private void AtualizarCamera()
        {
            if (_imageMap == null || player == null)
                return;

            int larguraTela = world1.ClientSize.Width;
            int alturaTela = world1.ClientSize.Height;

            int centroPlayerX = _playerWorldX + player.Width / 2;
            int centroPlayerY = _playerWorldY + player.Height / 2;

            int maxCameraX = Math.Max(
                0, _imageMap.Width - larguraTela
            );

            int maxCameraY = Math.Max(
                0, _imageMap.Height - alturaTela
            );

            _cameraX = Math.Max(
                0,
                Math.Min(centroPlayerX - larguraTela / 2, maxCameraX)
            );

            _cameraY = Math.Max(
                0,
                Math.Min(centroPlayerY - alturaTela / 2, maxCameraY)
            );
        }

        private void AtualizarPosicaoPlayerNaTela()
        {
            if (player == null)
                return;

            player.Location = new Point(
                _playerWorldX - _cameraX,
                _playerWorldY - _cameraY
            );

            world1.Invalidate();
        }
        #endregion
    }
}