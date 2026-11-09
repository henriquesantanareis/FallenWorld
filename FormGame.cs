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

        #region Player and Character
        // personagem selecionado pelo jogador
        private Player player;
        private readonly Personagem _personagem;
        private int _playerVisualOffsetY = 0;
        #endregion

        #region Input Handling
        // Impede que a tecla de ataque seja registrada várias vezes enquanto estiver pressionada.
        private bool _attack0KeyHeld = false;
        private bool _attack1KeyHeld = false;
        private bool _attack2KeyHeld = false;
        private bool _jumpKeyHeld = false;
        private bool _pedidoPulo = false;
        #endregion

        #region Enemys
        //Enemy Orc
        private readonly List<Enemy> _enemies = new List<Enemy>();
        // Controle da inteligência artificial dos Orcs.
        private readonly Dictionary<Enemy, int> _enemyAttackCooldowns = new Dictionary<Enemy, int>();
        private const int EnemyDetectionRange = 220;
        private const int EnemyAttackRange = 62;
        private const int EnemyMoveSpeed = 2;
        private const int EnemyAttackCooldownTicks = 90;
        private const int EnemyAttackReach = 44;
        // Controle do recuo dos Orcs ao receber dano.
        private readonly Dictionary<Enemy, int> _enemyKnockbackRemaining = new Dictionary<Enemy, int>();
        private readonly Dictionary<Enemy, Point> _enemyWorldPositions = new Dictionary<Enemy, Point>();
        private readonly Dictionary<Enemy, int> _enemyVisualOffsetY = new Dictionary<Enemy, int>();
        private const int EnemyKnockbackDistance = 16;
        private const int EnemyKnockbackSpeed = 2;
        #endregion
        
        #region Knight Aura

        // Duração da Aura: aproximadamente 5 segundos.
        private const int KnightAuraDurationTicks = 300;

        // Recarga da habilidade: aproximadamente 15 segundos.
        private const int KnightAuraCooldownTicks = 900;

        // Tempo restante de efeito.
        private int _knightAuraRemaining = 0;

        // Tempo restante para reutilizar a habilidade.
        private int _knightAuraCooldownRemaining = 0;

        // Dano do ataque básico do Cavaleiro.
        private const int KnightBaseDamage = 20;

        // Dano do ataque básico durante a Aura.
        private const int KnightAuraDamage = 30;

        #endregion

        #region Fireball
        // Bolas de fogo ativas no cenário.
        private readonly List<FireballProjectile> _fireballs = new List<FireballProjectile>();
        // Dano inicial da bola de fogo.
        private const int FireballDamage = 20;
        #endregion

        #region Mage Mana
        // Cada Fireball consome 50 pontos de mana.
        private const int MageFireballManaCost = 50;
        // Recupera 1 ponto a cada 6 atualizações do jogo.
        private const int MageManaRecoveryInterval = 6;
        // Quantidade recuperada por intervalo.
        private const int MageManaRecoveryAmount = 1;
        // Contador do intervalo de recuperação.
        private int _manaRecoveryTickCounter = 0;
        // Indica se o som de recuperação está ativo.
        private bool _manaRecoverySoundActive = false;
        #endregion

        #region Map and Physics
        // Mapa do jogo
        private Maps map;
        private Bitmap _imageMap;
        private int _playerWorldX;
        private int _playerWorldY;
        private int _cameraX;
        private int _cameraY;
        // Física vertical do personagem
        private const int VelocidadePlayer = 4;
        private const float ForcaPulo = -10.0f;
        private const float Gravidade = 0.55f;

        private float _velocidadeVertical = 0f;
        private bool _estaNoChao = false;
        #endregion

        #region Damage and Invulnerability
        // Impede que o W pressionado continuamente dispare vários pulos automaticamente.
        private const int IntervaloInvulnerabilidadeTicks = 100;
        private int _invulnerabilidadeRestante = 0;
        #endregion

        #endregion

        #region Constructor and Initialization
        public FormGame(Personagem personagem)
        {
            InitializeComponent();
            _personagem = personagem;
            FormClosed += (s, e) => 
            {
                PararSomRecuperacaoMana();
                SoundEffects.StopFireballFlight();
                SoundEffects.StopManaRecovery();
                SoundEffects.StopKnightAura();
            };
        }

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
                    PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP100;
                    PBX_MagicBar.Image = Properties.Resources.duckMage_MagicBar_Full;
                    BTN_Skill1.Image = Properties.Resources.duck_Mage_Skill1;
                    BTN_Skill2.Image = Properties.Resources.duckMage_Skill2;
                    BTN_Skill3.Image = Properties.Resources.duckMage_Skill3;
                    break;
                case Personagem.Cavaleiro:
                    PBX_Avatar.Image = Properties.Resources.knight_Avatar_normal;
                    PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP100;
                    BTN_Skill1.Image = Properties.Resources.knight_Skill1;
                    BTN_Skill2.Image = Properties.Resources.Knight_Skill2;
                    PBX_MagicBar.Visible = false;
                    BTN_Skill3.Visible = false;
                    break;
                case Personagem.Princesa:
                    PBX_Avatar.Image = Properties.Resources.Princess_Avatar_normal;
                    PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP100;
                    BTN_Skill1.Image = Properties.Resources.princess_Skill1;
                    BTN_Skill2.Image = Properties.Resources.princess_Skill2;
                    PBX_MagicBar.Visible = false;
                    BTN_Skill3.Visible = false;
                    break;
            }
            player = new Player(_personagem);

            Rectangle playerBounds = player.GetVisibleSpriteBounds();

            _playerVisualOffsetY = Math.Max(0,player.Height - playerBounds.Bottom);

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

            player.Location = new Point(playerX,playerY + _playerVisualOffsetY);
            player.AttackImpact += Player_AttackImpact;
            world1.Controls.Add(player);
            _velocidadeVertical = 0f;
            _estaNoChao = TemChaoAbaixo();
            LoadEnemies();
            AtualizarCamera();
            AtualizarPosicaoPlayerNaTela();
            player.BringToFront();
        }
        #endregion

        #region Map and Environment
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

        #region Input and Controls
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
            SkillButtonStates(sender, e);
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
            SkillButtonStates(sender, e);
        }
        private void SkillButtonStates(object sender, EventArgs e)
        {
            switch (_personagem)
            {
                case Personagem.Mago:

                    // Ataque 0: Staff Attack
                    BTN_Skill1.Image = _attack0KeyHeld
                        ? Properties.Resources.duck_Mage_Skill1_pressed
                        : Properties.Resources.duck_Mage_Skill1;

                    // Ataque 1: Fireball
                    BTN_Skill2.Image = _attack1KeyHeld
                        ? Properties.Resources.duckMage_Skill2_pressed
                        : Properties.Resources.duckMage_Skill2;

                    // Ataque 2: Recuperar mana
                    BTN_Skill3.Image = _attack2KeyHeld
                        ? Properties.Resources.duckMage_Skill3_pressed
                        : Properties.Resources.duckMage_Skill3;

                    break;

                case Personagem.Cavaleiro:

                    // Ataque 0: Reforço físico
                    BTN_Skill1.Image = _attack0KeyHeld
                        ? Properties.Resources.knight_Skill1_pressed
                        : Properties.Resources.knight_Skill1;

                    // Ataque 1: Sword Slash
                    BTN_Skill2.Image = _attack1KeyHeld
                        ? Properties.Resources.knight_Skill2_pressed
                        : Properties.Resources.Knight_Skill2;

                    break;

                case Personagem.Princesa:

                    // Ataque 0: primeiro botão de habilidade
                    BTN_Skill1.Image = _attack0KeyHeld
                        ? Properties.Resources.princess_Skill1_pressed
                        : Properties.Resources.princess_Skill1;

                    // Ataque 1: Poison Attack
                    BTN_Skill2.Image = _attack1KeyHeld
                        ? Properties.Resources.princess_Skill2_pressed
                        : Properties.Resources.princess_Skill2;

                    break;
            }
        }
        #endregion

        #region Movement and Physics
        private bool TentarMoverHorizontal(int deslocamento)
        {
            // Calcula a próxima posição no mundo.
            int proximoX = _playerWorldX + deslocamento;

            // Verifica a colisão usando as coordenadas do mundo.
            Rectangle hitboxAtual = player.GetHitboxAt(_playerWorldX,_playerWorldY);

            Rectangle hitboxProxima = player.GetHitboxAt(proximoX,_playerWorldY);

            if (!map.PodeMover(hitboxProxima))
                return false;

            if (BloqueiaMovimentoPorInimigo(hitboxAtual, hitboxProxima))
            {
                return false;
            }

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

                Rectangle hitbox = player.GetHitboxAt(_playerWorldX,proximoY);

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
            Rectangle hitbox = player.GetHitboxAt(_playerWorldX,_playerWorldY);

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

        #region Camera and Coordinates
        private void AtualizarCamera()
        {
            if (_imageMap == null || player == null)
                return;

            int larguraTela = world1.ClientSize.Width;
            int alturaTela = world1.ClientSize.Height;

            int centroPlayerX = _playerWorldX + player.Width / 2;
            int centroPlayerY = _playerWorldY + player.Height / 2;

            int maxCameraX = Math.Max(0, _imageMap.Width - larguraTela);

            int maxCameraY = Math.Max(0, _imageMap.Height - alturaTela);

            _cameraX = Math.Max(0,Math.Min(centroPlayerX - larguraTela / 2, maxCameraX));

            _cameraY = Math.Max(0,Math.Min(centroPlayerY - alturaTela / 2, maxCameraY));
        }

        private void AtualizarPosicaoPlayerNaTela()
        {
            if (player == null)
                return;

            player.Location = new Point(_playerWorldX - _cameraX, _playerWorldY - _cameraY + _playerVisualOffsetY);

            AtualizarPosicaoInimigosNaTela();
            world1.Invalidate();
        }

        private void AtualizarPosicaoInimigosNaTela()
        {
            foreach (KeyValuePair<Enemy, Point> item
                in _enemyWorldPositions)
            {
                Enemy enemy = item.Key;
                Point worldPosition = item.Value;

                if (enemy == null || enemy.IsDisposed)
                    continue;

                int visualOffset = 0;

                _enemyVisualOffsetY.TryGetValue(enemy, out visualOffset);

                enemy.Location = new Point(worldPosition.X - _cameraX, worldPosition.Y - _cameraY + visualOffset);
            }
        }
        #endregion

        #region Comabt and damage
        private void VerificarDanoContatoInimigos()
        {
            if (player == null || player.IsDisposed || player.EstaMorto || _invulnerabilidadeRestante > 0)
            {
                return;
            }

            Rectangle hitboxJogador = player.GetHitboxAt(_playerWorldX,_playerWorldY);

            // Considera contato mesmo quando o movimento
            // é bloqueado poucos pixels antes da colisão.
            Rectangle areaContato = new Rectangle(
                hitboxJogador.Left - VelocidadePlayer,
                hitboxJogador.Top,
                hitboxJogador.Width + VelocidadePlayer * 2,
                hitboxJogador.Height
            );

            foreach (KeyValuePair<Enemy, Point> item in _enemyWorldPositions)
            {
                Enemy enemy = item.Key;

                if (enemy == null || enemy.IsDisposed || enemy.IsDead)
                    continue;

                Point posicao = item.Value;

                Rectangle hitboxInimigo = enemy.GetHitboxAt(posicao.X,posicao.Y);

                if (!areaContato.IntersectsWith(hitboxInimigo))
                    continue;

                AplicarDanoAoJogador(enemy.DanoContato);

                // Apenas um inimigo aplica dano neste ciclo.
                return;
            }
        }

        private void AtualizarBarraVida()
        {
            if (player == null)
                return;

            int vida = player.VidaAtual;

            switch (_personagem)
            {
                case Personagem.Mago:
                    if (vida <= 0)
                    {
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP0;
                        PBX_Avatar.Image = Properties.Resources.duckMage_Avatar_death;
                    }
                    else if (vida <= 5)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP5;
                    else if (vida <= 10)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP10;
                    else if (vida <= 15)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP15;
                    else if (vida <= 20)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP20;
                    else if (vida <= 25)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP25;
                    else if (vida <= 30)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP30;
                    else if (vida <= 35)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP35;
                    else if (vida <= 40)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP40;
                    else if (vida <= 45)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP45;
                    else if (vida <= 50)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP50;
                    else if (vida <= 55)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP55;
                    else if (vida <= 60)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP60;
                    else if (vida <= 65)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP65;
                    else if (vida <= 70)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP70;
                    else if (vida <= 75)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP75;
                    else if (vida <= 80)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP80;
                    else if (vida <= 85)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP85;
                    else if (vida <= 90)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP90;
                    else if (vida <= 95)
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP95;
                    else
                        PBX_HealthBar.Image = Properties.Resources.duckMage_HealthBar_HP100;
                    break;

                case Personagem.Cavaleiro:
                    if (vida <= 0)
                    {
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP0;
                        PBX_Avatar.Image = Properties.Resources.knight_Avatar_death;
                    }
                    else if (vida <= 5)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP5;
                    else if (vida <= 10)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP10;
                    else if (vida <= 15)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP15;
                    else if (vida <= 20)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP20;
                    else if (vida <= 25)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP25;
                    else if (vida <= 30)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP30;
                    else if (vida <= 35)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP35;
                    else if (vida <= 40)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP40;
                    else if (vida <= 45)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP45;
                    else if (vida <= 50)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP50;
                    else if (vida <= 55)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP55;
                    else if (vida <= 60)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP60;
                    else if (vida <= 65)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP65;
                    else if (vida <= 70)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP70;
                    else if (vida <= 75)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP75;
                    else if (vida <= 80)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP80;
                    else if (vida <= 85)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP85;
                    else if (vida <= 90)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP90;
                    else if (vida <= 95)
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP95;
                    else
                        PBX_HealthBar.Image = Properties.Resources.Knight_HealthBar_HP100;
                    break;

                case Personagem.Princesa:
                    if (vida <= 0)
                    {
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP0;
                        PBX_Avatar.Image = Properties.Resources.Princess_Avatar_death;
                    }
                    else if (vida <= 5)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP5;
                    else if (vida <= 10)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP10;
                    else if (vida <= 15)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP15;
                    else if (vida <= 20)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP20;
                    else if (vida <= 25)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP25;
                    else if (vida <= 30)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP30;
                    else if (vida <= 35)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP35;
                    else if (vida <= 40)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP40;
                    else if (vida <= 45)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP45;
                    else if (vida <= 50)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP50;
                    else if (vida <= 55)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP55;
                    else if (vida <= 60)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP60;
                    else if (vida <= 65)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP65;
                    else if (vida <= 70)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP70;
                    else if (vida <= 75)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP75;
                    else if (vida <= 80)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP80;
                    else if (vida <= 85)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP85;
                    else if (vida <= 90)
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP90;
                    else
                        PBX_HealthBar.Image = Properties.Resources.Princess_HealthBar_HP100;
                    break;
            }
        }

        private void AtualizarBarraMagia()
        {
            if (_personagem != Personagem.Mago || player == null || player.IsDisposed)
            {
                return;
            }

            int mana = player.ManaAtual;

            if (mana >= player.ManaMaxima)
            {
                PBX_MagicBar.Image = Properties.Resources.duckMage_MagicBar_Full;
            }
            else if (mana >= MageFireballManaCost)
            {
                PBX_MagicBar.Image = Properties.Resources.duckMage_MagicBar_Half;
            }
            else
            {
                PBX_MagicBar.Image = Properties.Resources.duckMage_MagicBar_Empty;
            }
        }

        private void AplicarDanoAosInimigos(int dano, int alcance, bool aplicarVeneno = false)
        {
            if (player == null || player.EstaMorto)
                return;

            Rectangle hitboxPlayer = player.GetHitboxAt(_playerWorldX,_playerWorldY);

            int alturaAtaque = Math.Max(12, hitboxPlayer.Height / 2);
            int ataqueY = hitboxPlayer.Top + (hitboxPlayer.Height - alturaAtaque) / 2;

            Rectangle hitboxAtaque;

            if (player.FacingRight)
            {
                hitboxAtaque = new Rectangle(hitboxPlayer.Right,ataqueY,alcance,alturaAtaque);
            }
            else
            {
                hitboxAtaque = new Rectangle(hitboxPlayer.Left - alcance,ataqueY,alcance,alturaAtaque);
            }

            foreach (KeyValuePair<Enemy, Point> item in _enemyWorldPositions)
            {
                Enemy enemy = item.Key;

                if (enemy == null || enemy.IsDisposed || enemy.IsDead)
                    continue;

                Point posicao = item.Value;

                Rectangle hitboxInimigo = enemy.GetHitboxAt(posicao.X,posicao.Y);

                if (!hitboxAtaque.IntersectsWith(hitboxInimigo))
                    continue;

                enemy.ReceberDano(dano);

                // Se sobreviveu ao golpe, inicia o recuo.
                if (!enemy.IsDead)
                    IniciarRecuoInimigo(enemy);

                // Aplica veneno se o golpe for da habilidade da Princesa.
                if (aplicarVeneno && !enemy.IsDead)
                {
                    enemy.AplicarVeneno(danoPorTick: 3,duracaoTicks: 300,intervaloTicks: 60);
                }

                break;
            }
        }
        private void Player_AttackImpact(StatePlayer state)
        {
            switch (_personagem)
            {
                case Personagem.Mago:
                    if (state == StatePlayer.Attack)
                    { 
                        AplicarDanoAosInimigos(10, 44);
                    }
                    else if (state == StatePlayer.Fireball)
                    {
                        CriarBolaDeFogo();
                    }
                    break;

                case Personagem.Cavaleiro:
                    if (state == StatePlayer.Attack)
                    {
                        int dano = _knightAuraRemaining > 0 ? KnightAuraDamage : KnightBaseDamage;
                        AplicarDanoAosInimigos(dano, 52);
                    }
                    break;

                case Personagem.Princesa:
                    if (state == StatePlayer.Attack)
                        AplicarDanoAosInimigos(15, 48);
                    else if (state == StatePlayer.Poison_Attack)
                    {
                        // Som do ataque envenenado.
                        SoundEffects.Play(Properties.Resources.Princess_Poison_Attack,0.90f);
                        AplicarDanoAosInimigos(20, 48, true);
                    }
                    break;
            }
        }

        private void Enemy_AttackImpact(Enemy enemy)
        {
            if (player == null || player.IsDisposed || player.EstaMorto || enemy == null || enemy.IsDisposed || enemy.IsDead ||
                _invulnerabilidadeRestante > 0)
            {
                return;
            }

            Point posicao;

            if (!_enemyWorldPositions.TryGetValue(enemy, out posicao))
                return;

            Rectangle hitboxJogador = player.GetHitboxAt(_playerWorldX, _playerWorldY);

            Rectangle hitboxInimigo = enemy.GetHitboxAt(posicao.X, posicao.Y);

            int alturaAtaque = Math.Max(20, hitboxInimigo.Height - 12);

            int ataqueY = hitboxInimigo.Top + (hitboxInimigo.Height - alturaAtaque) / 2;

            Rectangle areaAtaque;

            if (enemy.FacingRight)
            {
                areaAtaque = new Rectangle(hitboxInimigo.Right,ataqueY,EnemyAttackReach,alturaAtaque);
            }
            else
            {
                areaAtaque = new Rectangle(hitboxInimigo.Left - EnemyAttackReach,ataqueY,EnemyAttackReach,alturaAtaque);
            }

            // Se o jogador não estiver na área do golpe, o ataque erra.
            if (!areaAtaque.IntersectsWith(hitboxJogador))
                return;

            AplicarDanoAoJogador(enemy.DanoAtaque);
        }

        private void AplicarDanoAoJogador(int dano)
        {
            if (player == null || player.IsDisposed || player.EstaMorto || _invulnerabilidadeRestante > 0)
            {
                return;
            }

            int danoFinal = dano;

            // Reduz o dano recebido enquanto a Aura estiver ativa.
            if (_personagem == Personagem.Cavaleiro && _knightAuraRemaining > 0)
            {
                danoFinal = Math.Max(1,(int)Math.Ceiling(dano * 0.5));
            }

            if (player.ReceberDano(danoFinal))
            {
                _invulnerabilidadeRestante = IntervaloInvulnerabilidadeTicks;
                AtualizarBarraVida();
            }
        }
        #endregion

        #region Projectiles and Skills
        private void CriarBolaDeFogo()
        {
            if (_personagem != Personagem.Mago || player == null || player.IsDisposed || player.EstaMorto)
            {
                return;
            }

            bool facingRight = player.FacingRight;

            // Limites visíveis do sprite atual do Mago.
            Rectangle spriteBounds = player.GetVisibleSpriteBounds();

            // Posição real dos limites visíveis no mundo.
            int spriteWorldLeft = _playerWorldX + spriteBounds.Left;

            int spriteWorldRight = _playerWorldX + spriteBounds.Right;

            int spriteWorldTop = _playerWorldY + _playerVisualOffsetY + spriteBounds.Top;

            // Posição horizontal aproximada da saída do cajado.
            int fireballX = facingRight ? spriteWorldRight - 12 : spriteWorldLeft - 20;

            // Centraliza a bola verticalmente no sprite visível.
            int fireballY = spriteWorldTop + (spriteBounds.Height - 32) / 2;

            FireballProjectile fireball = new FireballProjectile(facingRight,fireballX,fireballY);

            _fireballs.Add(fireball);

            // Converte a posição do mundo para as coordenadas da tela.
            fireball.Location = new Point(fireball.WorldX - _cameraX,fireball.WorldY - _cameraY);

            world1.Controls.Add(fireball);

            // Som contínuo durante o voo.
            SoundEffects.StartFireballFlight(Properties.Resources.duckMage_Fireball_Flight,0.30f);
            fireball.BringToFront();
        }
        private void AtualizarBolasDeFogo()
        {
            for (int i = _fireballs.Count - 1; i >= 0; i--)
            {
                FireballProjectile fireball = _fireballs[i];

                if (fireball == null || fireball.IsDisposed)
                {
                    _fireballs.RemoveAt(i);
                    continue;
                }

                fireball.UpdateProjectile();

                // Remove somente depois que a explosão terminar.
                if (fireball.IsFinished)
                {
                    world1.Controls.Remove(fireball);
                    _fireballs.RemoveAt(i);
                    fireball.Dispose();
                    continue;
                }

                // Atualiza a posição na tela conforme a câmera.
                fireball.Location = new Point(fireball.WorldX - _cameraX, fireball.WorldY - _cameraY);

                // Durante a explosão, não movimenta nem testa colisões.
                if (fireball.IsImpacting)
                    continue;

                // Hitbox menor que o sprite, centralizada na bola de fogo.
                Rectangle hitboxBola = new Rectangle(fireball.WorldX + 8, fireball.WorldY + 8, 16, 16);

                bool iniciarExplosao = false;

                // Limites do mapa.
                if (_imageMap == null ||
                    fireball.WorldX + fireball.Width < 0 ||
                    fireball.WorldX > _imageMap.Width ||
                    fireball.WorldY + fireball.Height < 0 ||
                    fireball.WorldY > _imageMap.Height)
                {
                    iniciarExplosao = true;
                }
                // Colisão com o cenário.
                else if (!map.PodeMover(hitboxBola))
                {
                    iniciarExplosao = true;
                }
                else
                {
                    // Colisão com os Orcs.
                    foreach (KeyValuePair<Enemy, Point> item
                        in _enemyWorldPositions)
                    {
                        Enemy enemy = item.Key;

                        if (enemy == null ||
                            enemy.IsDisposed ||
                            enemy.IsDead)
                        {
                            continue;
                        }

                        Point posicao = item.Value;

                        Rectangle hitboxInimigo = enemy.GetHitboxAt(posicao.X, posicao.Y);

                        if (!hitboxBola.IntersectsWith(hitboxInimigo))
                            continue;

                        // Dano imediato da bola de fogo.
                        enemy.ReceberDano(FireballDamage);

                        if (!enemy.IsDead)
                        {
                            // Inicia a queimadura por aproximadamente 4 segundos.
                            enemy.AplicarQueimadura(danoPorTick: 2, duracaoTicks: 240, intervaloTicks: 60);
                            IniciarRecuoInimigo(enemy);
                        }

                        iniciarExplosao = true;
                        break;
                    }
                }

                // O alcance máximo também encerra o voo.
                if (!iniciarExplosao && fireball.ExceededMaxDistance)
                    iniciarExplosao = true;

                if (iniciarExplosao)
                {
                    fireball.StartImpact();

                    SoundEffects.Play(Properties.Resources.duckMage_Fireball_Explosion, 0.95f);
                }
            }
            // Verifica se ainda existe alguma bola voando.
            bool existeBolaVoando = _fireballs.Exists(f => f != null && !f.IsDisposed && !f.IsFinished && !f.IsImpacting);

            // Para o som quando não houver projéteis em voo.
            if (!existeBolaVoando)
            {
                SoundEffects.StopFireballFlight();
            }
        }
        #endregion

        #region Enemy and AI
        private void LoadEnemies()
        {
            if (map == null)
                return;

            List<MapTile> spawns = map.GetEnemySpawns("orc");

            foreach (MapTile spawn in spawns)
            {
                Enemy enemy = new Enemy();

                Rectangle enemyBounds = enemy.GetVisibleSpriteBounds();

                int visualOffset = Math.Max(0, enemy.Height - enemyBounds.Bottom);

                _enemyVisualOffsetY[enemy] = visualOffset;

                enemy.AttackImpact += Enemy_AttackImpact;
                _enemyAttackCooldowns[enemy] = 0;
                _enemyKnockbackRemaining[enemy] = 0;

                // Coordenadas do marcador no mundo.
                int spawnX = spawn.x * map.tileSize;
                int spawnY = spawn.y * map.tileSize;

                // Centraliza o orc no tile e alinha seus pés ao chão.
                int enemyWorldX = spawnX + (map.tileSize - enemy.Width) / 2;

                int enemyWorldY = spawnY - enemy.Height;

                // Guarda a posição real do inimigo
                _enemyWorldPositions[enemy] = new Point(enemyWorldX, enemyWorldY);

                _enemies.Add(enemy);

                // Converte as coordenadas do mundo para a tela.
                enemy.Location = new Point(enemyWorldX - _cameraX, enemyWorldY - _cameraY + visualOffset);

                world1.Controls.Add(enemy);
                enemy.BringToFront();
            }
        }

        private void AtualizarIAInimigos()
        {
            AtualizarRecuoInimigos();

            if (player == null || player.IsDisposed || player.EstaMorto || map == null)
            {
                return;
            }

            Rectangle hitboxJogador = player.GetHitboxAt(_playerWorldX, _playerWorldY);

            int centroJogadorX = hitboxJogador.Left + hitboxJogador.Width / 2;

            foreach (Enemy enemy in _enemies)
            {
                if (enemy == null || enemy.IsDisposed || enemy.IsDead)
                {
                    continue;
                }

                // Atualiza o tempo restante até o próximo ataque.
                int cooldown;

                if (_enemyAttackCooldowns.TryGetValue(enemy, out cooldown)
                    && cooldown > 0)
                {
                    cooldown--;
                    _enemyAttackCooldowns[enemy] = cooldown;
                }

                // Recupera a posição real do Orc no mundo.
                Point posicao;

                if (!_enemyWorldPositions.TryGetValue(enemy, out posicao))
                    continue;

                Rectangle hitboxInimigo = enemy.GetHitboxAt(posicao.X, posicao.Y);

                int centroInimigoX = hitboxInimigo.Left + hitboxInimigo.Width / 2;

                int diferencaX = centroJogadorX - centroInimigoX;
                int distanciaX = Math.Abs(diferencaX);

                bool facingRight = diferencaX >= 0;

                // Verifica se o jogador está aproximadamente no mesmo nível.
                bool mesmaAltura = Math.Abs(hitboxJogador.Bottom - hitboxInimigo.Bottom) <= 28;

                // Não interrompe uma animação de ataque, dano ou morte.
                if (enemy.IsPerformingAction)
                    continue;

                // Não retoma a perseguição enquanto o Orc estiver recuando.
                int recuoRestante;

                if (_enemyKnockbackRemaining.TryGetValue(enemy, out recuoRestante) && recuoRestante != 0)
                    continue;

                // Fora do alcance de percepção, o Orc permanece parado.
                if (distanciaX > EnemyDetectionRange || !mesmaAltura)
                {
                    enemy.SetIdle(facingRight);
                    continue;
                }

                // Se o jogador estiver perto, tenta atacar.
                if (distanciaX <= EnemyAttackRange)
                {
                    if (cooldown <= 0)
                    {
                        enemy.SetAttack(facingRight);

                        _enemyAttackCooldowns[enemy] = EnemyAttackCooldownTicks;
                    }
                    else
                    {
                        enemy.SetIdle(facingRight);
                    }

                    continue;
                }

                // O jogador está visível, mas ainda longe para atacar.
                int movimento = diferencaX > 0 ? EnemyMoveSpeed : -EnemyMoveSpeed;

                int proximoX = posicao.X + movimento;

                Rectangle hitboxProxima = enemy.GetHitboxAt(proximoX, posicao.Y);

                // Faixa abaixo dos pés para evitar caminhar para fora de uma plataforma.
                Rectangle faixaDosPes = new Rectangle(hitboxProxima.Left + 2,hitboxProxima.Bottom,Math.Max(1, hitboxProxima.Width - 4),1);

                // Não avança contra paredes nem quando não há chão à frente.
                if (!map.PodeMover(hitboxProxima) || map.PodeMover(faixaDosPes))
                {
                    enemy.SetIdle(facingRight);
                    continue;
                }

                // Atualiza a posição do Orc no mundo.
                Point novaPosicao = new Point(proximoX, posicao.Y);

                _enemyWorldPositions[enemy] = novaPosicao;
                int visualOffset = 0;
                _enemyVisualOffsetY.TryGetValue(enemy, out visualOffset);

                // Converte a posição do mundo para a posição visível na tela.
                enemy.Location = new Point(novaPosicao.X - _cameraX, novaPosicao.Y - _cameraY + visualOffset);

                enemy.SetWalk(facingRight);
            }
        }

        private void IniciarRecuoInimigo(Enemy enemy)
        {
            if (enemy == null || enemy.IsDisposed || enemy.IsDead)
                return;

            Point posicaoInimigo;

            if (!_enemyWorldPositions.TryGetValue(enemy, out posicaoInimigo))
                return;

            Rectangle hitboxJogador = player.GetHitboxAt(_playerWorldX,_playerWorldY);

            Rectangle hitboxInimigo = enemy.GetHitboxAt(posicaoInimigo.X,posicaoInimigo.Y);

            int centroJogador = hitboxJogador.Left + hitboxJogador.Width / 2;
            int centroInimigo = hitboxInimigo.Left + hitboxInimigo.Width / 2;

            // Valor positivo: empurra para a direita.
            // Valor negativo: empurra para a esquerda.
            int direcao = centroInimigo >= centroJogador ? 1 : -1;

            _enemyKnockbackRemaining[enemy] = direcao * EnemyKnockbackDistance;
        }

        private void AtualizarRecuoInimigos()
        {
            if (map == null)
                return;

            foreach (Enemy enemy in _enemies)
            {
                if (enemy == null || enemy.IsDisposed)
                    continue;

                int restante;

                if (!_enemyKnockbackRemaining.TryGetValue(enemy, out restante) || restante == 0)
                {
                    continue;
                }

                // Orcs mortos não precisam sofrer recuo.
                if (enemy.IsDead)
                {
                    _enemyKnockbackRemaining[enemy] = 0;
                    continue;
                }

                Point posicao;

                if (!_enemyWorldPositions.TryGetValue(enemy, out posicao))
                    continue;

                int passo = Math.Sign(restante) * Math.Min(EnemyKnockbackSpeed, Math.Abs(restante));

                int proximoX = posicao.X + passo;

                Rectangle hitboxProxima = enemy.GetHitboxAt(proximoX,posicao.Y);

                // Verifica se há chão abaixo da nova posição.
                Rectangle faixaDosPes = new Rectangle(hitboxProxima.Left + 2,hitboxProxima.Bottom,Math.Max(1, hitboxProxima.Width - 4),1);

                // Não atravessa paredes nem empurra o Orc para fora de uma plataforma.
                if (!map.PodeMover(hitboxProxima) || map.PodeMover(faixaDosPes))
                {
                    _enemyKnockbackRemaining[enemy] = 0;
                    continue;
                }

                Point novaPosicao = new Point(proximoX, posicao.Y);

                _enemyWorldPositions[enemy] = novaPosicao;

                int visualOffset = 0;

                _enemyVisualOffsetY.TryGetValue(enemy, out visualOffset);

                enemy.Location = new Point(novaPosicao.X - _cameraX, novaPosicao.Y - _cameraY + visualOffset);

                // Diminui a distância que ainda falta percorrer.
                _enemyKnockbackRemaining[enemy] = restante - passo;
            }
        }

        private void AtualizarVenenoInimigos()
        {
            foreach (Enemy enemy in _enemies)
            {
                if (enemy == null || enemy.IsDisposed)
                    continue;

                enemy.AtualizarVeneno();
            }
        }

        private void AtualizarQueimaduraInimigos()
        {
            foreach (Enemy enemy in _enemies)
            {
                if (enemy == null || enemy.IsDisposed)
                    continue;

                enemy.AtualizarQueimadura();
            }
        }
        #endregion

        #region Game Loop and Animations
        private void TMR_Game_Tick(object sender, EventArgs e)
        {
            if (player == null || player.IsDisposed)
                return;
            
            if (_invulnerabilidadeRestante > 0)
                _invulnerabilidadeRestante--;

            // Gerencia a recuperação de mana.
            if (player.IsRecoveringMana)
            {
                // Se tentar movimentar ou pular, cancela a recuperação.
                if (Core.isMovingLeft || Core.isMovingRight || _pedidoPulo || !_estaNoChao)
                {
                    player.CancelarRecuperacaoMana();
                    _manaRecoveryTickCounter = 0;
                    PararSomRecuperacaoMana();  
                }
                else
                {
                    _manaRecoveryTickCounter++;

                    // Recupera mana no intervalo configurado.
                    if (_manaRecoveryTickCounter >= MageManaRecoveryInterval)
                    {
                        _manaRecoveryTickCounter = 0;
                        player.RecuperarMana(MageManaRecoveryAmount);
                        // A atualização visual da barra será feita aqui.
                        AtualizarBarraMagia();
                        // Para automaticamente quando a mana estiver cheia.
                        if (player.ManaAtual >= player.ManaMaxima)
                        {
                            player.CancelarRecuperacaoMana();
                            PararSomRecuperacaoMana();
                        }
                    }
                }
            }
            else
            {
                _manaRecoveryTickCounter = 0;
                // Também interrompe o som caso o Mago tenha recebido dano.
                PararSomRecuperacaoMana();
            }

            // Atualiza a duração da Aura.
            if (_knightAuraRemaining > 0)
            {
                _knightAuraRemaining--;

                // Para o som quando o buff termina.
                if (_knightAuraRemaining == 0)
                {
                    SoundEffects.StopKnightAura();
                }
            }

            // Atualiza a recarga da Aura.
            if (_knightAuraCooldownRemaining > 0)
                _knightAuraCooldownRemaining--;

            // Sincroniza o efeito visual com a duração da Aura.
            player.AuraAtiva = _personagem == Personagem.Cavaleiro && _knightAuraRemaining > 0;

            switch (_personagem)
            {
                case Personagem.Mago:
                    // Habilidade 1: Staff Attack
                    if (Core.isAttacking0)
                    {
                        Core.isAttacking0 = false;
                        player.SetAttack();
                    }

                    // Habilidade 2: Fireball.
                    if (Core.isAttacking1)
                    {
                        Core.isAttacking1 = false;

                        // Verifica se existe mana suficiente.
                        if (player.ManaAtual >= MageFireballManaCost)
                        {
                            if (player.SetFireball())
                            {
                                // Consome mana somente se a animação iniciar.
                                player.ConsumirMana(MageFireballManaCost);

                                AtualizarBarraMagia();

                                // Som de preparação da magia.
                                SoundEffects.Play(Properties.Resources.duckMage_Fireball_Casting,0.85f);
                            }
                        }
                    }

                    // Habilidade 3: Recover Mana.
                    if (Core.isAttacking2)
                    {
                        Core.isAttacking2 = false;

                        bool jogadorParado = _estaNoChao && !Core.isMovingLeft && !Core.isMovingRight && !_pedidoPulo;

                        if (jogadorParado && player.SetRecover_Mana())
                        {
                            _manaRecoveryTickCounter = 0;
                            // Inicia o som contínuo de recuperação.
                            SoundEffects.StartManaRecovery(Properties.Resources.duckMage_RecoveryMana, 0.25f);
                            _manaRecoverySoundActive = true;
                        }
                    }
                    break;
                case Personagem.Cavaleiro:
                    // Habilidade 1: Aura
                    if (Core.isAttacking0)
                    {
                        Core.isAttacking0 = false;
                        // Verifica se a habilidade está disponível.
                        if (_knightAuraCooldownRemaining <= 0)
                        {
                            // Tenta executar a animação.
                            if (player.SetAura())
                            {
                                // Ativa o efeito temporário.
                                _knightAuraRemaining = KnightAuraDurationTicks;
                                // Inicia a recarga.
                                _knightAuraCooldownRemaining = KnightAuraCooldownTicks;
                                // Ativa imediatamente o efeito visual.
                                player.AuraAtiva = true;
                                // Som de ativação: toca uma única vez.
                                SoundEffects.Play(Properties.Resources.Knight_Aura_Activation,0.90f);
                                // Inicia o som contínuo da Aura.
                                SoundEffects.StartKnightAura(Properties.Resources.Knight_Aura,0.25f);
                            }
                        }
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
            if (!player.IsPerformingAction && solicitouPulo && _estaNoChao)
            {
                _velocidadeVertical = ForcaPulo;
                _estaNoChao = false;

                player.SetJump();
            }

            // O movimento horizontal continua permitido no ar.
            if (!player.IsPerformingAction || player.EstaRecebendoDano)
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
            // Atualiza perseguição e ataques dos Orcs.
            AtualizarIAInimigos();
            // Atualiza o dano contínuo do veneno.
            AtualizarVenenoInimigos();
            // Atualiza o dano contínuo da queimadura.
            AtualizarQueimaduraInimigos();
            // Atualiza a posição das bolas de fogo e verifica colisões.
            AtualizarBolasDeFogo();
            VerificarDanoContatoInimigos();
        }
        #endregion
        
        #region Collision detection between player and enemies
        private bool BloqueiaMovimentoPorInimigo(Rectangle hitboxAtual,Rectangle hitboxProxima)
        {
            foreach (KeyValuePair<Enemy, Point> item in _enemyWorldPositions)
            {
                Enemy enemy = item.Key;

                if (enemy == null || enemy.IsDisposed || enemy.IsDead)
                    continue;

                Point posicao = item.Value;

                Rectangle hitboxInimigo = enemy.GetHitboxAt(posicao.X,posicao.Y);

                // O próximo movimento não toca este inimigo.
                if (!hitboxProxima.IntersectsWith(hitboxInimigo))
                    continue;

                // Se não havia colisão antes, bloqueia a entrada.
                if (!hitboxAtual.IntersectsWith(hitboxInimigo))
                    return true;

                // Já existia sobreposição: permite afastamento.
                int centroAtual = hitboxAtual.Left + hitboxAtual.Width / 2;

                int centroProximo = hitboxProxima.Left + hitboxProxima.Width / 2;

                int centroInimigo = hitboxInimigo.Left + hitboxInimigo.Width / 2;

                int distanciaAtual = Math.Abs(centroAtual - centroInimigo);

                int distanciaProxima = Math.Abs(centroProximo - centroInimigo);

                // Não permite avançar mais para dentro do inimigo.
                if (distanciaProxima <= distanciaAtual)
                    return true;
            }
            return false;
        }
        #endregion
        
        #region Mouse Control
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

        #region Sound Effects
        private void PararSomRecuperacaoMana()
        {
            if (!_manaRecoverySoundActive)
                return;

            _manaRecoverySoundActive = false;

            SoundEffects.StopManaRecovery();
        }
        #endregion
    }
}