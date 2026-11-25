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

        #region Inventory

        // Tipos de runas que podem ser adquiridas.
        private enum TipoRuna
        {
            Poder,
            Agilidade,
            Vida
        }

        // Armazena as runas adquiridas, permitindo repetições.
        private readonly List<TipoRuna> _runasAdquiridas = new List<TipoRuna>();
        #endregion

        #region Inventory Rewards
        private const int BonusVidaRuna = 20;
        private const int BonusResistenciaRunaPoder = 15;
        private bool _recompensaBauEntregue = false;
        private readonly Random _recompensaRandom = new Random();
        private const int MaxRunasInventario = 3;
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

        #region Mid-Boss

        private MidBoss _midBoss;

        // Posição real do chefe no mundo.
        private Point _midBossWorldPosition;

        // Ajuste visual para alinhar os pés ao chão.
        private int _midBossVisualOffsetY = 0;
        // Configurações da inteligência artificial.
        private const int MidBossDetectionRange = 300;
        private const int MidBossAttackRange = 85;
        private const int MidBossMoveSpeed = 2;
        // Tempo entre os ataques do chefe.
        private const int MidBossAttackCooldownTicks = 120;

        // Alcance horizontal dos golpes.
        private const int MidBossAttackReach = 72;

        // Tempo restante para o próximo ataque.
        private int _midBossAttackCooldownRemaining = 0;
        // Indica se a animação de morte terminou.
        private bool _midBossDefeated = false;
        // Indica se a música da batalha já começou.
        private bool _midBossBattleMusicStarted = false;
        #endregion

        #region Final Boss - Lich das Chamas Negras

        private LichBoss _lichBoss;

        // Posição real do Lich no mundo.
        private Point _lichBossWorldPosition;

        // Ajuste visual para alinhar os pés ao chão.
        private int _lichBossVisualOffsetY = 0;

        // Configurações da inteligência artificial do Lich.
        private const int LichDetectionRange = 500;
        private const int LichMoveSpeed = 2;

        // Configurações do Dark Flame Burst.
        private const int LichMeleeAttackRange = 150;
        private const int LichFlameBurstRadius = 130;
        private const int LichMeleeAttackDamage = 20;
        private const int LichAttackCooldownTicks = 120;

        // Configurações da queimadura.
        private const int LichBurnDamagePerTick = 2;
        private const int LichBurnDurationTicks = 360;
        private const int LichBurnIntervalTicks = 120;

        private int _lichBossAttackCooldownRemaining = 0;
        // Indica se a música da batalha do Lich já começou.
        private bool _lichBossBattleMusicStarted = false;
        private int _lichBurnDurationRemaining = 0;
        private int _lichBurnTickRemaining = 0;

        // Distância máxima para utilizar o Dark Fire Ball.
        private const int LichFireBallAttackRange = 420;

        #endregion

        #region final boss - Dullhan o Cavaleiro Sem Cabeça
        private DullahanBoss _dullahanBoss;

        // Posição real do Dullahan no mundo.
        private Point _dullahanBossWorldPosition;

        // Ajuste visual para alinhar os pés ao chão.
        private int _dullahanBossVisualOffsetY = 0;
        // Configurações da inteligência artificial.
        private const int DullahanDetectionRange = 420;
        private const int DullahanMoveSpeed = 2;

        // Alcance dos ataques.
        private const int DullahanAttack1Range = 100;
        private const int DullahanAttack2Range = 145;

        // Dano dos ataques.
        private const int DullahanAttack1Damage = 18;
        private const int DullahanAttack2Damage = 28;

        // Tempo entre os ataques.
        private const int DullahanAttack1CooldownTicks = 90;
        private const int DullahanAttack2CooldownTicks = 150;

        private int _dullahanBossAttackCooldownRemaining = 0;

        // Controla a alternância dos ataques.
        private bool _dullahanNextAttackHeavy = false;
        // Duração do bloqueio da Aura: aproximadamente 6 segundos.
        private const int DullahanAuraBlockDurationTicks = 360;

        // Tempo restante do bloqueio.
        private int _dullahanAuraBlockRemaining = 0;
        // Indica se a música da batalha do Dullahan já começou.
        private bool _dullahanBossBattleMusicStarted = false;
        #endregion

        #region Final Boss - Gothic Queen

        private GothicQueenBoss _gothicQueenBoss;

        // Posição real da Rainha no mundo.
        private Point _gothicQueenBossWorldPosition;

        // Ajuste visual para alinhar os pés ao chão.
        private int _gothicQueenBossVisualOffsetY = 0;

        // Inteligência artificial da Rainha.
        private const int GothicQueenDetectionRange = 420;
        private const int GothicQueenMoveSpeed = 2;

        // Royal Venom Slash.
        private const int RoyalVenomSlashRange = 110;
        private const int RoyalVenomSlashDamage = 18;
        private const int RoyalVenomSlashCooldownTicks = 100;

        // Probabilidade de envenenamento.
        private const int RoyalVenomSlashPoisonChance = 50;

        // Configurações do veneno.
        private const int QueenPoisonDamagePerTick = 2;
        private const int QueenPoisonDurationTicks = 360;
        private const int QueenPoisonIntervalTicks = 90;

        private int _gothicQueenAttackCooldownRemaining = 0;
        // Indica se a música da batalha da Rainha já começou.
        private bool _gothicQueenBattleMusicStarted = false;
        private int _queenPoisonDurationRemaining = 0;
        private int _queenPoisonTickRemaining = 0;

        // Sorteio independente para o veneno da Rainha.
        private readonly Random _gothicQueenRandom = new Random();
        // Contador de ataques da Rainha para alternar entre os ataques.
        private int _gothicQueenAttackCount = 0;
        // Ondas venenosas criadas pelo Emerald Bladefall.
        private readonly List<QueenPoisonWave> _queenPoisonWaves = new List<QueenPoisonWave>();
        // Distância para iniciar a explosão ao alcançar o jogador.
        private const int QueenPoisonWaveTriggerDistance = 36;
        // Dano direto do Emerald Bladefall.
        private const int EmeraldBladefallDamage = 18;
        // Indica se a animação de morte da Rainha já foi concluída.
        private bool _gothicQueenDefeated = false;
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

        #region Dark Fireball
        private readonly List<DarkFireballProjectile> _darkFireballs = new List<DarkFireballProjectile>();
        private const int LichFireBallDamage = 20;
        // Sorteio dos efeitos negativos da Dark Fireball.
        private readonly Random _darkFireballDebuffRandom = new Random();

        // Duração do bloqueio da habilidade.
        private const int LichSkillBlockDurationTicks = 360;

        // Slot da habilidade bloqueada: 0, 1 ou 2.
        // -1 significa que nenhuma habilidade está bloqueada.
        private int _lichBlockedSkillSlot = -1;

        // Tempo restante do bloqueio.
        private int _lichSkillBlockRemaining = 0;
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
        // Marcador de saída do mapa.
        private MapTile _nextStage;

        // Controle de proximidade e interação.
        private bool _nearNextStage = false;
        private bool _interactionKeyHeld = false;

        // Indicação visual da saída.
        private Label _stagePrompt;
        // Controle das transições entre mapas.
        private bool _transicaoEmAndamento = false;

        // Indica se o Santuário das Raízes já foi carregado.
        private bool _santuarioCarregado = false;
        // Indica se o Mapa 1 já foi carregado.
        private bool _mapa1Carregado = false;
        // Indica se as Ruínas do Guardião já foram carregadas.
        private bool _ruinasGuardiaoCarregadas = false;
        // Indica se o Mapa 2 já foi carregado.
        private bool _mapa2Carregado = false;
        // Indica se a Forja do Julgamento já foi carregada.
        private bool _judgmentForgeCarregado = false;
        // Indica se a arena dos chefes finais já foi carregada.
        private bool _finalBossArenaCarregada = false;
        #endregion

        #region Puzzle Runes
        // Armazena as runas do puzzle atual.
        private readonly List<PuzzleRune> _puzzleRunes = new List<PuzzleRune>();
        // Armazena as posições reais das runas no mapa.
        private readonly Dictionary<PuzzleRune, Point> _puzzleRuneWorldPositions = new Dictionary<PuzzleRune, Point>();
        private const int DistanciaInteracaoRuna = 80;
        private PuzzleRune _runaProxima;
        private bool _puzzleCompleto = false;
        private PuzzleChest _puzzleChest;
        private Point _puzzleChestWorldPosition;
        private bool _puzzleChestAoAlcance = false;
        // Tempo limite do Julgamento das Runas.
        private const int JudgmentForgeTempoLimiteMs = 5000;

        // Tempo restante para ativar as três runas.
        private int _judgmentForgeTempoRestanteMs = JudgmentForgeTempoLimiteMs;

        // Acumulador para contabilizar os milissegundos.
        private int _judgmentForgeTempoAcumuladoMs = 0;

        // Indica se a contagem regressiva está ativa.
        private bool _judgmentForgeContagemAtiva = false;

        // Impede que a penalidade seja aplicada mais de uma vez.
        private bool _judgmentForgeContagemFalhou = false;

        // Indica que as três runas já começaram a ativação.
        private bool _judgmentForgeTodasRunasIniciadas = false;

        // Indicador visual da contagem regressiva.
        private Label _judgmentForgeTempoLabel;
        // Indica se a música da Forja já foi iniciada.
        private bool _judgmentForgeMusicaIniciada = false;
        // Controla se Vharzun já avisou sobre a saída da Forja.
        private bool _vharzunAvisoSaidaReproduzido = false;
        // Controla se Sylvara já avisou sobre a saída do Santuário.
        private bool _sylvaraAvisoSaidaReproduzido = false;
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
                SoundEffects.StopQueenPoisonWave();
                SoundEffects.StopVoices();
                SoundEffects.StopWavLoop();
                LiberarIconesInventario();
            };
        }

        private void FormGame_Load(object sender, EventArgs e)
        {
            // Esconder o cursor.
            Cursor.Hide();
            // Bloquear os eventos do mouse.
            Application.AddMessageFilter(this);
            InicializarInventario();
            Load_Map(Properties.Resources.tutorial_Map_Js, Properties.Resources.tutorial_Map_sprite);
            _nextStage = map.GetNextStage();
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
            // O baú do tutorial permanece interativo, mas não entrega recompensa.
            LoadPuzzleChest(true, false);
            CriarPromptStage();
        }
        #endregion

        #region Inventory
        // Extrai o primeiro quadro de uma spritesheet para usar como ícone.
        private Bitmap CriarIconeInventario(Bitmap spritesheet,int larguraFrame,int alturaFrame)
        {
            if (spritesheet == null)
                throw new ArgumentNullException("spritesheet");

            if (spritesheet.Width < larguraFrame || 
                spritesheet.Height < alturaFrame)
            {
                throw new ArgumentException(
                    "A spritesheet possui dimensões inválidas."
                );
            }

            Bitmap icone = new Bitmap(larguraFrame, alturaFrame);

            using (Graphics g = Graphics.FromImage(icone))
            {
                g.Clear(Color.Transparent);

                g.DrawImage(
                    spritesheet,
                    new Rectangle(0, 0, larguraFrame, alturaFrame),
                    new Rectangle(0, 0, larguraFrame, alturaFrame),
                    GraphicsUnit.Pixel
                );
            }

            return icone;
        }

        private Bitmap CriarIconeRuna(TipoRuna runa)
        {
            switch (runa)
            {
                case TipoRuna.Poder:
                    return CriarIconeInventario(
                        Properties.Resources.Rune_Force_sprite, 68, 68
                    );

                case TipoRuna.Agilidade:
                    return CriarIconeInventario(
                        Properties.Resources.Rune_Agil_sprite, 68, 68
                    );

                case TipoRuna.Vida:
                    return CriarIconeInventario(
                        Properties.Resources.Rune_Life_sprite, 68, 68
                    );

                default:
                    throw new ArgumentOutOfRangeException("runa");
            }
        }

        // Configura os ícones iniciais do inventário.
        private void InicializarInventario()
        {
            
            PBX_Rune1.SizeMode = PictureBoxSizeMode.Zoom;
            PBX_Rune2.SizeMode = PictureBoxSizeMode.Zoom;
            PBX_Rune3.SizeMode = PictureBoxSizeMode.Zoom;

            // Mantém o fundo transparente no inventário.
            FLP_Inventory.BackColor = Color.Transparent;

            PBX_Rune1.BackColor = Color.Transparent;
            PBX_Rune2.BackColor = Color.Transparent;
            PBX_Rune3.BackColor = Color.Transparent;

            // As runas ficam escondidas até serem adquiridas.
            PBX_Rune1.Visible = false;
            PBX_Rune2.Visible = false;
            PBX_Rune3.Visible = false;

            AtualizarInventario();
        }

        // Atualiza a representação visual do inventário.
        private void AtualizarInventario()
        {
            PictureBox[] espacos = {PBX_Rune1,PBX_Rune2,PBX_Rune3};

            for (int i = 0; i < espacos.Length; i++)
            {
                PictureBox espaco = espacos[i];

                // Libera a imagem anterior.
                espaco.Image?.Dispose();
                espaco.Image = null;

                // Esconde os espaços ainda não utilizados.
                if (i >= _runasAdquiridas.Count)
                {
                    espaco.Visible = false;
                    continue;
                }

                // Mostra a runa correspondente à posição da coleção.
                espaco.Image = CriarIconeRuna(_runasAdquiridas[i]);
                espaco.Visible = true;
            }
        }

        // Libera as imagens criadas para o inventário.
        private void LiberarIconesInventario()
        {
            PBX_Rune1.Image?.Dispose();
            PBX_Rune2.Image?.Dispose();
            PBX_Rune3.Image?.Dispose();

            PBX_Rune1.Image = null;
            PBX_Rune2.Image = null;
            PBX_Rune3.Image = null;
        }
        #endregion

        #region Map and Environment
        private void Load_Map(string mapJson, Bitmap spritesheet)
        {
            if (string.IsNullOrWhiteSpace(mapJson))
                throw new ArgumentException("O JSON do mapa está vazio.");

            if (spritesheet == null)
                throw new ArgumentNullException("spritesheet");

            JavaScriptSerializer readMapjson = new JavaScriptSerializer();

            // Desserializa o novo mapa.
            Maps novoMapa = readMapjson.Deserialize<Maps>(mapJson);

            if (novoMapa == null)
                throw new Exception("Não foi possível carregar o mapa.");

            // Prepara as colisões do novo cenário.
            novoMapa.PrepararColisoes();

            // Monta a imagem completa do novo cenário.
            Bitmap novaImagem = novoMapa.CriarImagem(spritesheet);

            // Substitui o mapa e a imagem atuais.
            map = novoMapa;

            if (_imageMap != null)
                _imageMap.Dispose();

            _imageMap = novaImagem;

            world1.BackgroundImage = null;
            world1.Invalidate();
        }
        private bool CarregarSantuarioDasRaizes()
        {
            if (_transicaoEmAndamento || player == null || player.IsDisposed)
                return false;

            bool timerEstavaAtivo = TMR_Game.Enabled;

            _transicaoEmAndamento = true;
            TMR_Game.Stop();

            try
            {
                string mapJson = Properties.Resources.sanctuary_Map_Js;
                Bitmap spritesheet = Properties.Resources.sanctuary_Map_sprite;

                // Verifica se o novo mapa possui um Spawn válido.
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                Maps mapaTeste = serializer.Deserialize<Maps>(mapJson);

                if (mapaTeste == null)
                    throw new Exception("Não foi possível interpretar o JSON do Santuário.");

                MapTile spawnTeste = mapaTeste.GetSpawnPlayer();

                if (spawnTeste == null)
                    throw new Exception("O Spawn do jogador não foi encontrado no Santuário.");

                // Interrompe as vozes do mapa anterior antes da transição.
                SoundEffects.StopVoices();

                // Carrega o novo mapa e prepara suas colisões.
                Load_Map(mapJson, spritesheet);

                // Reinicia o aviso de saída da Sylvara ao entrar no Santuário.
                _sylvaraAvisoSaidaReproduzido = false;

                MapTile spawn = map.GetSpawnPlayer();

                // Interrompe a recuperação de mana, caso esteja acontecendo.
                player.CancelarRecuperacaoMana();
                PararSomRecuperacaoMana();
                _manaRecoveryTickCounter = 0;

                // Para e remove as bolas de fogo do mapa anterior.
                foreach (FireballProjectile fireball in _fireballs.ToList())
                {
                    if (fireball == null)
                        continue;

                    world1.Controls.Remove(fireball);
                    fireball.Dispose();
                }

                _fireballs.Clear();
                SoundEffects.StopFireballFlight();

                // Remove os inimigos do mapa anterior.
                foreach (Enemy enemy in _enemies.ToList())
                {
                    if (enemy == null)
                        continue;

                    enemy.AttackImpact -= Enemy_AttackImpact;

                    world1.Controls.Remove(enemy);
                    enemy.Dispose();
                }

                _enemies.Clear();
                _enemyAttackCooldowns.Clear();
                _enemyKnockbackRemaining.Clear();
                _enemyWorldPositions.Clear();
                _enemyVisualOffsetY.Clear();

                // Posiciona o mesmo jogador no Spawn do novo mapa.
                int spawnX = spawn.x * map.tileSize;
                int spawnY = spawn.y * map.tileSize;

                _playerWorldX = spawnX + (map.tileSize - player.Width) / 2;
                _playerWorldY = spawnY - player.Height;

                player.Location = new Point(
                    _playerWorldX,
                    _playerWorldY + _playerVisualOffsetY
                );

                // Reinicia a física e os comandos de movimento.
                _velocidadeVertical = 0f;
                _estaNoChao = TemChaoAbaixo();

                _pedidoPulo = false;
                _jumpKeyHeld = false;

                Core.isMovingLeft = false;
                Core.isMovingRight = false;
                Core.isJumping = false;

                Core.isAttacking0 = false;
                Core.isAttacking1 = false;
                Core.isAttacking2 = false;

                _attack0KeyHeld = false;
                _attack1KeyHeld = false;
                _attack2KeyHeld = false;

                SkillButtonStates(this, EventArgs.Empty);

                // Atualiza o marcador de saída do novo mapa.
                _nextStage = map.GetNextStage();
                _nearNextStage = false;

                if (_stagePrompt != null)
                    _stagePrompt.Visible = false;

                // Atualiza a câmera antes de criar os novos inimigos.
                AtualizarCamera();

                // Cria e posiciona as três runas do puzzle.
                LoadPuzzleRunes();

                // Cria e posiciona o baú do puzzle.
                LoadPuzzleChest(false);

                AtualizarPosicaoPlayerNaTela();
                player.BringToFront();

                // Atualiza o HUD usando os valores do mesmo jogador.
                AtualizarBarraVida();
                AtualizarBarraMagia();

                world1.Invalidate();

                // Inicia a música ambiente MP3 do Santuário.
                SoundEffects.StopWavLoop();

                SoundEffects.StartMp3Loop(Properties.Resources.RootShrine_Ambient,0.30f);

                SoundEffects.PlayVoice(Properties.Resources.Sylvara_RootsSanctuary_Intro,0.9f);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Não foi possível carregar o Santuário das Raízes.\n\n" + ex.Message,
                    "Erro na transição",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
            finally
            {
                _transicaoEmAndamento = false;

                if (timerEstavaAtivo)
                    TMR_Game.Start();
            }
        }

        private bool CarregarMapa1()
        {
            if (_transicaoEmAndamento || player == null || player.IsDisposed)
                return false;

            bool timerEstavaAtivo = TMR_Game.Enabled;

            _transicaoEmAndamento = true;
            TMR_Game.Stop();

            try
            {
                // Recursos do Mapa 1.
                string mapJson = Properties.Resources.map1_Map_Js;
                Bitmap spritesheet = Properties.Resources.map1_Map_sprite;

                // Verifica se o mapa possui um Spawn válido.
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                Maps mapaTeste = serializer.Deserialize<Maps>(mapJson);

                if (mapaTeste == null)
                    throw new Exception("Não foi possível interpretar o JSON do Mapa 1.");

                MapTile spawnTeste = mapaTeste.GetSpawnPlayer();

                if (spawnTeste == null)
                    throw new Exception("O Spawn do jogador não foi encontrado no Mapa 1.");

                // Interrompe as vozes do mapa anterior antes da transição.
                SoundEffects.StopVoices();

                // Remove os objetos do puzzle anterior.
                LimparPuzzleRunes();
                LimparPuzzleChest();

                _puzzleCompleto = false;
                _runaProxima = null;
                _puzzleChestAoAlcance = false;

                // Interrompe a música ambiente do Santuário.
                SoundEffects.StopWavLoop();

                // Carrega o novo mapa e suas colisões.
                Load_Map(mapJson, spritesheet);

                MapTile spawn = map.GetSpawnPlayer();

                // Interrompe a recuperação de mana, se estiver ativa.
                player.CancelarRecuperacaoMana();
                PararSomRecuperacaoMana();
                _manaRecoveryTickCounter = 0;

                // Remove as bolas de fogo do mapa anterior.
                foreach (FireballProjectile fireball in _fireballs.ToList())
                {
                    if (fireball == null)
                        continue;

                    world1.Controls.Remove(fireball);
                    fireball.Dispose();
                }

                _fireballs.Clear();
                SoundEffects.StopFireballFlight();

                // Remove os inimigos do mapa anterior.
                foreach (Enemy enemy in _enemies.ToList())
                {
                    if (enemy == null)
                        continue;

                    enemy.AttackImpact -= Enemy_AttackImpact;

                    world1.Controls.Remove(enemy);
                    enemy.Dispose();
                }

                _enemies.Clear();
                _enemyAttackCooldowns.Clear();
                _enemyKnockbackRemaining.Clear();
                _enemyWorldPositions.Clear();
                _enemyVisualOffsetY.Clear();

                // Posiciona o jogador no Spawn do Mapa 1.
                int spawnX = spawn.x * map.tileSize;
                int spawnY = spawn.y * map.tileSize;

                _playerWorldX = spawnX + (map.tileSize - player.Width) / 2;
                _playerWorldY = spawnY - player.Height;

                player.Location = new Point(_playerWorldX,_playerWorldY + _playerVisualOffsetY);

                // Reinicia a física.
                _velocidadeVertical = 0f;
                _estaNoChao = TemChaoAbaixo();

                // Reinicia os comandos de movimento.
                _pedidoPulo = false;
                _jumpKeyHeld = false;

                Core.isMovingLeft = false;
                Core.isMovingRight = false;
                Core.isJumping = false;

                Core.isAttacking0 = false;
                Core.isAttacking1 = false;
                Core.isAttacking2 = false;

                _attack0KeyHeld = false;
                _attack1KeyHeld = false;
                _attack2KeyHeld = false;

                SkillButtonStates(this, EventArgs.Empty);

                // Atualiza o marcador de saída do Mapa 1.
                _nextStage = map.GetNextStage();
                _nearNextStage = false;

                if (_stagePrompt != null)
                    _stagePrompt.Visible = false;

                // Atualiza a câmera e carrega os inimigos.
                AtualizarCamera();
                LoadEnemies();

                AtualizarPosicaoPlayerNaTela();
                player.BringToFront();

                // Atualiza o HUD.
                AtualizarBarraVida();
                AtualizarBarraMagia();

                world1.Invalidate();

                // Inicia a música ambiente MP3 do Mapa 1.
                SoundEffects.StartMp3Loop(Properties.Resources.Map1_Ambient,0.30f);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Não foi possível carregar o Mapa 1.\n\n" + ex.Message,
                    "Erro na transição",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
            finally
            {
                _transicaoEmAndamento = false;

                if (timerEstavaAtivo)
                    TMR_Game.Start();
            }
        }

        private bool CarregarRuinasGuardiao()
        {
            if (_transicaoEmAndamento || player == null || player.IsDisposed)
                return false;

            bool timerEstavaAtivo = TMR_Game.Enabled;

            _transicaoEmAndamento = true;
            TMR_Game.Stop();

            try
            {
                // Recursos das Ruínas do Guardião.
                string mapJson = Properties.Resources.PuzzleGuardian_js;
                Bitmap spritesheet = Properties.Resources.PuzzleGuardian_spritesheet;

                // Valida o JSON e o Spawn.
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                Maps mapaTeste = serializer.Deserialize<Maps>(mapJson);

                if (mapaTeste == null)
                    throw new Exception("Não foi possível interpretar o mapa das Ruínas.");

                MapTile spawnTeste = mapaTeste.GetSpawnPlayer();

                if (spawnTeste == null)
                    throw new Exception("Spawn do jogador não encontrado nas Ruínas.");

                // Interrompe as vozes do mapa anterior antes da transição.
                SoundEffects.StopVoices();

                // Remove os objetos dos puzzles anteriores.
                LimparPuzzleRunes();
                LimparPuzzleChest();
                LimparGuardianRunes();

                _puzzleCompleto = false;
                _runaProxima = null;
                _puzzleChestAoAlcance = false;

                _guardianPuzzleCompleto = false;
                _guardianRuneErrorInProgress = false;
                _guardianSequenceIndex = 0;
                _guardianRuneSequence.Clear();

                // Carrega o cenário.
                Load_Map(mapJson, spritesheet);

                MapTile spawn = map.GetSpawnPlayer();

                // Cancela a recuperação de mana.
                player.CancelarRecuperacaoMana();
                PararSomRecuperacaoMana();
                _manaRecoveryTickCounter = 0;

                // Remove as bolas de fogo anteriores.
                foreach (FireballProjectile fireball in _fireballs.ToList())
                {
                    if (fireball == null)
                        continue;

                    world1.Controls.Remove(fireball);
                    fireball.Dispose();
                }

                _fireballs.Clear();
                SoundEffects.StopFireballFlight();

                // Remove inimigos do mapa anterior.
                foreach (Enemy enemy in _enemies.ToList())
                {
                    if (enemy == null)
                        continue;

                    enemy.AttackImpact -= Enemy_AttackImpact;

                    world1.Controls.Remove(enemy);
                    enemy.Dispose();
                }

                _enemies.Clear();
                _enemyAttackCooldowns.Clear();
                _enemyKnockbackRemaining.Clear();
                _enemyWorldPositions.Clear();
                _enemyVisualOffsetY.Clear();

                // Posiciona o jogador no Spawn das Ruínas.
                int spawnX = spawn.x * map.tileSize;
                int spawnY = spawn.y * map.tileSize;

                _playerWorldX = spawnX + (map.tileSize - player.Width) / 2;
                _playerWorldY = spawnY - player.Height;

                player.Location = new Point(
                    _playerWorldX,
                    _playerWorldY + _playerVisualOffsetY
                );

                // Reinicia a física.
                _velocidadeVertical = 0f;
                _estaNoChao = TemChaoAbaixo();

                // Reinicia os comandos.
                _pedidoPulo = false;
                _jumpKeyHeld = false;

                Core.isMovingLeft = false;
                Core.isMovingRight = false;
                Core.isJumping = false;

                Core.isAttacking0 = false;
                Core.isAttacking1 = false;
                Core.isAttacking2 = false;

                _attack0KeyHeld = false;
                _attack1KeyHeld = false;
                _attack2KeyHeld = false;

                SkillButtonStates(this, EventArgs.Empty);

                // Atualiza a saída do mapa.
                _nextStage = map.GetNextStage();
                _nearNextStage = false;

                if (_stagePrompt != null)
                    _stagePrompt.Visible = false;

                // Atualiza a câmera.
                AtualizarCamera();

                // Carrega as runas das Ruínas.
                LoadGuardianRunes();

                // Carrega o baú escondido até a conclusão do puzzle.
                LoadPuzzleChest(false);

                AtualizarPosicaoPlayerNaTela();
                player.BringToFront();

                // Atualiza o HUD.
                AtualizarBarraVida();
                AtualizarBarraMagia();

                world1.Invalidate();
                // Interrompe a música ambiente do Mapa 1.
                SoundEffects.StopWavLoop();
                // Inicia a música ambiente das Ruínas do Guardião.
                SoundEffects.StartWavLoop(Properties.Resources.GuardianRuins_Ambient,0.36f);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Não foi possível carregar as Ruínas do Guardião.\n\n" + ex.Message,
                    "Erro na transição",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
            finally
            {
                _transicaoEmAndamento = false;

                if (timerEstavaAtivo)
                    TMR_Game.Start();
            }
        }


        private bool CarregarMapa2()
        {
            if (_transicaoEmAndamento || player == null || player.IsDisposed)
                return false;

            bool timerEstavaAtivo = TMR_Game.Enabled;

            _transicaoEmAndamento = true;
            TMR_Game.Stop();

            try
            {
                // Recursos do Mapa 2.
                string mapJson = Properties.Resources.map2_Map_Js;
                Bitmap spritesheet = Properties.Resources.map2_Map_sprite;

                // Valida o JSON e o Spawn do jogador.
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                Maps mapaTeste = serializer.Deserialize<Maps>(mapJson);

                if (mapaTeste == null)
                    throw new Exception("Não foi possível interpretar o Mapa 2.");

                MapTile spawnTeste = mapaTeste.GetSpawnPlayer();

                if (spawnTeste == null)
                    throw new Exception("Spawn do jogador não encontrado no Mapa 2.");

                // Interrompe as vozes do mapa anterior antes da transição.
                SoundEffects.StopVoices();

                // Remove os objetos dos puzzles anteriores.
                LimparPuzzleRunes();
                LimparPuzzleChest();
                LimparGuardianRunes();

                _puzzleCompleto = false;
                _runaProxima = null;
                _puzzleChestAoAlcance = false;

                _guardianPuzzleCompleto = false;
                _guardianRuneErrorInProgress = false;
                _guardianSequenceIndex = 0;
                _guardianRuneSequence.Clear();

                // Interrompe a música das Ruínas do Guardião.
                SoundEffects.StopWavLoop();

                // Carrega o novo mapa e suas colisões.
                Load_Map(mapJson, spritesheet);

                MapTile spawn = map.GetSpawnPlayer();

                // Interrompe a recuperação de mana.
                player.CancelarRecuperacaoMana();
                PararSomRecuperacaoMana();
                _manaRecoveryTickCounter = 0;

                // Remove as bolas de fogo anteriores.
                foreach (FireballProjectile fireball in _fireballs.ToList())
                {
                    if (fireball == null)
                        continue;

                    world1.Controls.Remove(fireball);
                    fireball.Dispose();
                }

                _fireballs.Clear();
                SoundEffects.StopFireballFlight();

                // Remove um Mid-Boss anterior, caso exista.
                LimparMidBoss();
                _midBossDefeated = false;
                _midBossBattleMusicStarted = false;
                // Remove os inimigos do mapa anterior.
                foreach (Enemy enemy in _enemies.ToList())
                {
                    if (enemy == null)
                        continue;

                    enemy.AttackImpact -= Enemy_AttackImpact;

                    world1.Controls.Remove(enemy);
                    enemy.Dispose();
                }

                _enemies.Clear();
                _enemyAttackCooldowns.Clear();
                _enemyKnockbackRemaining.Clear();
                _enemyWorldPositions.Clear();
                _enemyVisualOffsetY.Clear();


                // Posiciona o jogador no Spawn do Mapa 2.
                int spawnX = spawn.x * map.tileSize;
                int spawnY = spawn.y * map.tileSize;

                _playerWorldX = spawnX + (map.tileSize - player.Width) / 2;
                _playerWorldY = spawnY - player.Height;

                player.Location = new Point(
                    _playerWorldX,
                    _playerWorldY + _playerVisualOffsetY
                );

                // Reinicia a física.
                _velocidadeVertical = 0f;
                _estaNoChao = TemChaoAbaixo();

                // Reinicia os comandos.
                _pedidoPulo = false;
                _jumpKeyHeld = false;

                Core.isMovingLeft = false;
                Core.isMovingRight = false;
                Core.isJumping = false;

                Core.isAttacking0 = false;
                Core.isAttacking1 = false;
                Core.isAttacking2 = false;

                _attack0KeyHeld = false;
                _attack1KeyHeld = false;
                _attack2KeyHeld = false;

                SkillButtonStates(this, EventArgs.Empty);

                // Atualiza o marcador de saída.
                _nextStage = map.GetNextStage();
                _nearNextStage = false;

                if (_stagePrompt != null)
                    _stagePrompt.Visible = false;

                // Atualiza a câmera.
                AtualizarCamera();

                // Carrega o Mid-Boss.
                LoadMidBoss();

                AtualizarPosicaoPlayerNaTela();
                player.BringToFront();

                // Atualiza o HUD.
                AtualizarBarraVida();
                AtualizarBarraMagia();

                world1.Invalidate();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Não foi possível carregar o Mapa 2.\n\n" + ex.Message,
                    "Erro na transição",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
            finally
            {
                _transicaoEmAndamento = false;

                if (timerEstavaAtivo)
                    TMR_Game.Start();
            }
        }


        private bool CarregarForjaDoJulgamento()
        {
            if (_transicaoEmAndamento || player == null || player.IsDisposed)
                return false;

            bool timerEstavaAtivo = TMR_Game.Enabled;

            _transicaoEmAndamento = true;
            TMR_Game.Stop();

            try
            {
                // Recursos da Forja do Julgamento.
                string mapJson = Properties.Resources.JudgmentForge_Map_js;
                Bitmap spritesheet = Properties.Resources.JudgmentForge_Map_sprite;

                // Valida o JSON e o Spawn do jogador.
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                Maps mapaTeste = serializer.Deserialize<Maps>(mapJson);

                if (mapaTeste == null)
                    throw new Exception("Não foi possível interpretar a Forja do Julgamento.");

                MapTile spawnTeste = mapaTeste.GetSpawnPlayer();

                if (spawnTeste == null)
                    throw new Exception("Spawn do jogador não encontrado na Forja do Julgamento.");

                // Interrompe as vozes do mapa anterior antes da transição.
                SoundEffects.StopVoices();

                // Remove os objetos dos puzzles anteriores.
                LimparPuzzleRunes();
                LimparPuzzleChest();
                LimparGuardianRunes();

                _puzzleCompleto = false;
                _runaProxima = null;
                _puzzleChestAoAlcance = false;

                _guardianPuzzleCompleto = false;
                _guardianRuneErrorInProgress = false;
                _guardianSequenceIndex = 0;
                _guardianRuneSequence.Clear();

                // Interrompe a música da arena anterior.
                SoundEffects.StopWavLoop();
                _judgmentForgeMusicaIniciada = false;
                _vharzunAvisoSaidaReproduzido = false;

                // Carrega o novo mapa e suas colisões.
                Load_Map(mapJson, spritesheet);

                MapTile spawn = map.GetSpawnPlayer();

                // Interrompe a recuperação de mana.
                player.CancelarRecuperacaoMana();
                PararSomRecuperacaoMana();
                _manaRecoveryTickCounter = 0;

                // Remove as bolas de fogo anteriores.
                foreach (FireballProjectile fireball in _fireballs.ToList())
                {
                    if (fireball == null)
                        continue;

                    world1.Controls.Remove(fireball);
                    fireball.Dispose();
                }

                _fireballs.Clear();
                SoundEffects.StopFireballFlight();

                // Remove o Mid-Boss da arena anterior.
                LimparMidBoss();
                _midBossBattleMusicStarted = false;

                // Mantém o registro da vitória sobre o Mid-Boss.

                // Remove os inimigos do mapa anterior.
                foreach (Enemy enemy in _enemies.ToList())
                {
                    if (enemy == null)
                        continue;

                    enemy.AttackImpact -= Enemy_AttackImpact;

                    world1.Controls.Remove(enemy);
                    enemy.Dispose();
                }

                _enemies.Clear();
                _enemyAttackCooldowns.Clear();
                _enemyKnockbackRemaining.Clear();
                _enemyWorldPositions.Clear();
                _enemyVisualOffsetY.Clear();

                // Posiciona o jogador no Spawn da Forja.
                int spawnX = spawn.x * map.tileSize;
                int spawnY = spawn.y * map.tileSize;

                _playerWorldX = spawnX + (map.tileSize - player.Width) / 2;
                _playerWorldY = spawnY - player.Height;

                player.Location = new Point(
                    _playerWorldX,
                    _playerWorldY + _playerVisualOffsetY
                );

                // Reinicia a física.
                _velocidadeVertical = 0f;
                _estaNoChao = TemChaoAbaixo();

                // Reinicia os comandos de movimento.
                _pedidoPulo = false;
                _jumpKeyHeld = false;

                Core.isMovingLeft = false;
                Core.isMovingRight = false;
                Core.isJumping = false;

                Core.isAttacking0 = false;
                Core.isAttacking1 = false;
                Core.isAttacking2 = false;

                _attack0KeyHeld = false;
                _attack1KeyHeld = false;
                _attack2KeyHeld = false;

                SkillButtonStates(this, EventArgs.Empty);

                // Atualiza o marcador de saída.
                _nextStage = map.GetNextStage();
                _nearNextStage = false;

                if (_stagePrompt != null)
                    _stagePrompt.Visible = false;

                // Atualiza a câmera.
                AtualizarCamera();

                // Carrega as três runas exclusivas da Forja.
                LoadJudgmentForgeRunes();

                // Reinicia o estado do Julgamento das Runas.
                _judgmentForgeTempoRestanteMs = JudgmentForgeTempoLimiteMs;
                _judgmentForgeTempoAcumuladoMs = 0;
                _judgmentForgeContagemAtiva = false;
                _judgmentForgeContagemFalhou = false;
                _judgmentForgeTodasRunasIniciadas = false;

                // Cria o indicador visual do tempo.
                CriarIndicadorTempoJulgamento();

                // Cria o baú inicialmente escondido.
                LoadPuzzleChest(false);

                AtualizarPosicaoPlayerNaTela();
                player.BringToFront();

                // Atualiza o HUD.
                AtualizarBarraVida();
                AtualizarBarraMagia();

                world1.Invalidate();

                // Vharzun anuncia o desafio ao jogador ao entrar na Forja.
                SoundEffects.PlayVoice(
                    Properties.Resources.Vharzun_Forge_Intro,
                    0.9f
                );

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Não foi possível carregar a Forja do Julgamento.\n\n" + ex.Message,
                    "Erro na transição",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
            finally
            {
                _transicaoEmAndamento = false;

                if (timerEstavaAtivo)
                    TMR_Game.Start();
            }
        }



        private bool CarregarArenaChefesFinais()
        {
            if (_transicaoEmAndamento || player == null || player.IsDisposed)
                return false;

            bool timerEstavaAtivo = TMR_Game.Enabled;

            _transicaoEmAndamento = true;
            TMR_Game.Stop();

            try
            {
                // Recursos da arena dos chefes finais.
                string mapJson = Properties.Resources.FinalBossMap_js;
                Bitmap spritesheet = Properties.Resources.FinalBossMap_sprite;

                // Valida o JSON e o Spawn do jogador.
                JavaScriptSerializer serializer = new JavaScriptSerializer();

                Maps mapaTeste = serializer.Deserialize<Maps>(mapJson);

                if (mapaTeste == null)
                    throw new Exception("Não foi possível interpretar a Arena dos Chefes Finais.");

                MapTile spawnTeste = mapaTeste.GetSpawnPlayer();

                if (spawnTeste == null)
                    throw new Exception("Spawn do jogador não encontrado na Arena dos Chefes Finais.");

                // Interrompe as vozes do mapa anterior antes da transição.
                SoundEffects.StopVoices();

                // Remove os objetos do puzzle anterior.
                LimparPuzzleRunes();
                LimparPuzzleChest();
                LimparGuardianRunes();

                _puzzleCompleto = false;
                _runaProxima = null;
                _puzzleChestAoAlcance = false;

                // Desativa o contador da Forja.
                _judgmentForgeContagemAtiva = false;
                _judgmentForgeContagemFalhou = false;
                _judgmentForgeTodasRunasIniciadas = false;
                _judgmentForgeMusicaIniciada = false;

                // Remove o indicador visual do Julgamento.
                if (_judgmentForgeTempoLabel != null)
                {
                    if (!_judgmentForgeTempoLabel.IsDisposed)
                    {
                        world1.Controls.Remove(_judgmentForgeTempoLabel);
                        _judgmentForgeTempoLabel.Dispose();
                    }

                    _judgmentForgeTempoLabel = null;
                }

                // Interrompe a música e os sons contínuos do mapa anterior.
                SoundEffects.StopWavLoop();
                SoundEffects.StopFireballFlight();

                SoundEffects.StopQueenPoisonWave();
                _gothicQueenBattleMusicStarted = false;

                // Carrega o novo mapa e suas colisões.
                Load_Map(mapJson, spritesheet);

                MapTile spawn = map.GetSpawnPlayer();

                // Interrompe somente a recuperação de mana.
                // O valor atual da mana permanece preservado.
                player.CancelarRecuperacaoMana();
                PararSomRecuperacaoMana();
                _manaRecoveryTickCounter = 0;

                // Remove as bolas de fogo anteriores.
                foreach (FireballProjectile fireball in _fireballs.ToList())
                {
                    if (fireball == null)
                        continue;

                    world1.Controls.Remove(fireball);
                    fireball.Dispose();
                }

                _fireballs.Clear();

                // Remove o Mid-Boss, caso ainda exista.
                LimparMidBoss();
                _midBossBattleMusicStarted = false;

                // Remove os inimigos do mapa anterior.
                foreach (Enemy enemy in _enemies.ToList())
                {
                    if (enemy == null)
                        continue;

                    enemy.AttackImpact -= Enemy_AttackImpact;

                    world1.Controls.Remove(enemy);
                    enemy.Dispose();
                }

                _enemies.Clear();
                _enemyAttackCooldowns.Clear();
                _enemyKnockbackRemaining.Clear();
                _enemyWorldPositions.Clear();
                _enemyVisualOffsetY.Clear();

                // Posiciona o mesmo jogador no Spawn da arena final.
                int spawnX = spawn.x * map.tileSize;
                int spawnY = spawn.y * map.tileSize;

                _playerWorldX = spawnX + (map.tileSize - player.Width) / 2;
                _playerWorldY = spawnY - player.Height;

                player.Location = new Point(
                    _playerWorldX,
                    _playerWorldY + _playerVisualOffsetY
                );

                // Reinicia a física.
                _velocidadeVertical = 0f;
                _estaNoChao = TemChaoAbaixo();

                // Reinicia os comandos de movimento.
                _pedidoPulo = false;
                _jumpKeyHeld = false;

                Core.isMovingLeft = false;
                Core.isMovingRight = false;
                Core.isJumping = false;

                Core.isAttacking0 = false;
                Core.isAttacking1 = false;
                Core.isAttacking2 = false;

                _attack0KeyHeld = false;
                _attack1KeyHeld = false;
                _attack2KeyHeld = false;

                SkillButtonStates(this, EventArgs.Empty);

                // A arena final não possui outra saída.
                _nextStage = map.GetNextStage();
                _nearNextStage = false;

                if (_stagePrompt != null)
                    _stagePrompt.Visible = false;

                // Atualiza a câmera.
                AtualizarCamera();

                // Cria e posiciona o Lich das Chamas Negras.
                CarregarChefeFinalDoPersonagem();

                // Atualiza as posições visuais.
                AtualizarPosicaoPlayerNaTela();

                player.BringToFront();

                // Atualiza o HUD sem alterar os atributos do jogador.
                AtualizarBarraVida();
                AtualizarBarraMagia();

                world1.Invalidate();

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Não foi possível carregar a Arena dos Chefes Finais.\n\n" + ex.Message,
                    "Erro na transição",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return false;
            }
            finally
            {
                _transicaoEmAndamento = false;

                if (timerEstavaAtivo)
                    TMR_Game.Start();
            }
        }


        private void LoadPuzzleRunes()
        {
            if (map == null)
                return;

            // Evita duplicar runas caso o método seja chamado novamente.
            LimparPuzzleRunes();
            _puzzleCompleto = false;

            List<MapTile> marcadores = map.GetPuzzleMarkers();

            foreach (MapTile marcador in marcadores)
            {
                if (marcador.attributes == null)
                    continue;

                string tipoRune = null;

                // Identifica o atributo Puzzle.
                foreach (KeyValuePair<string, string> atributo in marcador.attributes)
                {
                    if (string.Equals(atributo.Key, "Puzzle",StringComparison.OrdinalIgnoreCase))
                    {
                        tipoRune = atributo.Value;
                        break;
                    }
                }

                PuzzleRune rune = null;

                // Cria a runa correspondente ao marcador.
                if (string.Equals(tipoRune, "Rune1", StringComparison.OrdinalIgnoreCase))
                {
                    rune = new PuzzleRune(
                        1,
                        Properties.Resources.Puzzle_Rune_1_sprite,
                        Properties.Resources.Puzzle_Rune_1_js
                    );
                    rune.ActivationCompleted += OnRuneActivationCompleted;
                }
                else if (string.Equals(tipoRune, "Rune2", StringComparison.OrdinalIgnoreCase))
                {
                    rune = new PuzzleRune(
                        2,
                        Properties.Resources.Puzzle_Rune_2_sprite,
                        Properties.Resources.Puzzle_Rune_2_js
                    );
                    rune.ActivationCompleted += OnRuneActivationCompleted;
                }
                else if (string.Equals(tipoRune, "Rune3", StringComparison.OrdinalIgnoreCase))
                {
                    rune = new PuzzleRune(
                        3,
                        Properties.Resources.Puzzle_Rune_3_sprite,
                        Properties.Resources.Puzzle_Rune_3_js
                    );
                    rune.ActivationCompleted += OnRuneActivationCompleted;
                }

                // Ignora marcadores que não correspondem às três runas.
                if (rune == null)
                    continue;

                // Converte a posição do marcador para coordenadas do mundo.
                int worldX = marcador.x * map.tileSize + (map.tileSize - rune.Width) / 2;

                // Ajuste para alinhar a base visível da pedra ao chão.
                int worldY = marcador.y * map.tileSize - rune.Height + 4;

                Point worldPosition = new Point(worldX, worldY);

                _puzzleRunes.Add(rune);
                _puzzleRuneWorldPositions.Add(rune, worldPosition);

                // Converte para a posição atual da câmera.
                rune.Location = new Point(worldX - _cameraX,worldY - _cameraY);

                world1.Controls.Add(rune);
            }
            
            if (player != null && !player.IsDisposed)
            {
                player.BringToFront();
            }
        }

        private void LoadJudgmentForgeRunes()
        {
            if (map == null || world1 == null)
                return;

            // Evita duplicar runas caso o método seja chamado novamente.
            LimparPuzzleRunes();
            _puzzleCompleto = false;

            List<MapTile> marcadores = map.GetPuzzleMarkers();

            foreach (MapTile marcador in marcadores)
            {
                if (marcador.attributes == null)
                    continue;

                string tipoRune = null;

                // Identifica o atributo Puzzle.
                foreach (KeyValuePair<string, string> atributo in marcador.attributes)
                {
                    if (string.Equals(
                        atributo.Key,
                        "Puzzle",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        tipoRune = atributo.Value;
                        break;
                    }
                }

                PuzzleRune rune = null;

                // Runa de Fogo.
                if (string.Equals(tipoRune, "Rune1", StringComparison.OrdinalIgnoreCase))
                {
                    rune = new PuzzleRune(
                        1,
                        Properties.Resources.Puzzle_Rune_Flame_sprite,
                        Properties.Resources.Puzzle_Rune_Flame_js
                    );
                }
                // Runa da Montanha.
                else if (string.Equals(tipoRune, "Rune2", StringComparison.OrdinalIgnoreCase))
                {
                    rune = new PuzzleRune(
                        2,
                        Properties.Resources.Puzzle_Rune_Mountain_sprite,
                        Properties.Resources.Puzzle_Rune_Mountain_js
                    );
                }
                // Runa do Núcleo.
                else if (string.Equals(tipoRune, "Rune3", StringComparison.OrdinalIgnoreCase))
                {
                    rune = new PuzzleRune(
                        3,
                        Properties.Resources.Puzzle_Rune_Core_sprite,
                        Properties.Resources.Puzzle_Rune_Core_js
                    );
                }

                // Ignora marcadores que não correspondem às três runas.
                if (rune == null)
                    continue;

                // Registra o evento de conclusão da animação.
                rune.ActivationCompleted += OnRuneActivationCompleted;

                // Converte a posição do marcador para coordenadas do mundo.
                int worldX = marcador.x * map.tileSize + (map.tileSize - rune.Width) / 2;

                int worldY = marcador.y * map.tileSize - rune.Height + 4;

                Point worldPosition = new Point(worldX, worldY);

                _puzzleRunes.Add(rune);
                _puzzleRuneWorldPositions.Add(rune, worldPosition);

                // Converte para a posição atual da câmera.
                rune.Location = new Point(worldX - _cameraX,worldY - _cameraY);

                world1.Controls.Add(rune);
            }

            if (player != null && !player.IsDisposed)
            {
                player.BringToFront();
            }
        }
            
        private void LimparPuzzleRunes()
        {
            foreach (PuzzleRune rune in _puzzleRunes.ToList())
            {
                if (rune == null)
                    continue;

                world1.Controls.Remove(rune);
                rune.Dispose();
            }

            _puzzleRunes.Clear();
            _puzzleRuneWorldPositions.Clear();
        }


        #region Guardian Rune Loading

        private void LoadGuardianRunes()
        {
            if (map == null || world1 == null)
                return;

            // Remove eventuais runas antigas.
            LimparGuardianRunes();

            _guardianPuzzleCompleto = false;
            _guardianRuneErrorInProgress = false;
            _guardianSequenceIndex = 0;
            _guardianRuneSequence.Clear();

            // Busca os marcadores Puzzle no JSON.
            List<MapTile> marcadores = map.GetPuzzleMarkers();

            foreach (MapTile marcador in marcadores)
            {
                if (marcador.attributes == null)
                    continue;

                string tipoRune = null;

                foreach (KeyValuePair<string, string> atributo
                    in marcador.attributes)
                {
                    if (string.Equals(
                        atributo.Key,
                        "Puzzle",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        tipoRune = atributo.Value;
                        break;
                    }
                }

                GuardianRune rune = null;

                // Runa de Tharok - Triangular.
                if (string.Equals(
                    tipoRune, "Rune1",
                    StringComparison.OrdinalIgnoreCase))
                {
                    rune = new GuardianRune(
                        1,
                        Properties.Resources.Runa_de_Tharok,
                        Properties.Resources.Runa_de_Tharok_js
                    );
                }
                // Runa de Elyra - Circular.
                else if (string.Equals(
                    tipoRune, "Rune2",
                    StringComparison.OrdinalIgnoreCase))
                {
                    rune = new GuardianRune(
                        2,
                        Properties.Resources.Runa_de_Elyra,
                        Properties.Resources.Runa_de_Elyra_js
                    );
                }
                // Runa de Vaelis - Losangular.
                else if (string.Equals(
                    tipoRune, "Rune3",
                    StringComparison.OrdinalIgnoreCase))
                {
                    rune = new GuardianRune(
                        3,
                        Properties.Resources.Runa_de_Vaelis,
                        Properties.Resources.Runa_de_Vaelis_js
                    );
                }

                if (rune == null)
                    continue;

                // Registra os eventos das animações.
                rune.ActivationCompleted += OnGuardianRuneActivationCompleted;
                rune.ErrorAnimationCompleted += OnGuardianRuneErrorAnimationCompleted;
                rune.SuccessAnimationCompleted += OnGuardianRuneSuccessAnimationCompleted;

                // En nosso JSON, os marcadores estão na camada
                // background. Por isso, buscamos o chão na camada floor.
                // Posiciona a runa diretamente no marcador do Sprite Fusion.
                int worldX = marcador.x * map.tileSize + (map.tileSize - rune.Width) / 2;

                int worldY = marcador.y * map.tileSize + (map.tileSize - rune.Height) / 2;

                Point worldPosition = new Point(worldX, worldY);

                _guardianRunes.Add(rune);
                _guardianRuneWorldPositions.Add(rune, worldPosition);

                // Posiciona considerando a câmera.
                rune.Location = new Point(worldX - _cameraX,worldY - _cameraY);

                world1.Controls.Add(rune);
            }

            if (_guardianRunes.Count != 3)
            {
                LimparGuardianRunes();

                throw new Exception("Não foram encontradas as três runas das Ruínas do Guardião.");
            }

            GerarSequenciaGuardian();

            if (player != null && !player.IsDisposed)
                player.BringToFront();
        }


        private void GerarSequenciaGuardian()
        {
            _guardianRuneSequence.Clear();

            // IDs: 1 = Tharok, 2 = Elyra, 3 = Vaelis.
            _guardianRuneSequence.AddRange(new[] { 1, 2, 3 });

            // Embaralha as três runas.
            for (int i = _guardianRuneSequence.Count - 1; i > 0; i--)
            {
                int j = _guardianRandom.Next(i + 1);

                int temp = _guardianRuneSequence[i];
                _guardianRuneSequence[i] = _guardianRuneSequence[j];
                _guardianRuneSequence[j] = temp;
            }

            _guardianSequenceIndex = 0;
            _guardianPuzzleCompleto = false;

            // Reproduz a pista correspondente à sequência sorteada.
            string sequencia = string.Join("", _guardianRuneSequence);

            switch (sequencia)
            {
                case "123": // Tharok -> Elyra -> Vaelis
                    SoundEffects.PlayVoice(Properties.Resources.GuardianVoice_Sequence_01,0.9f);
                    break;

                case "132": // Tharok -> Vaelis -> Elyra
                    SoundEffects.PlayVoice(Properties.Resources.GuardianVoice_Sequence_02,0.9f);
                    break;

                case "213": // Elyra -> Tharok -> Vaelis
                    SoundEffects.PlayVoice(Properties.Resources.GuardianVoice_Sequence_03,0.9f);
                    break;

                case "231": // Elyra -> Vaelis -> Tharok
                    SoundEffects.PlayVoice(Properties.Resources.GuardianVoice_Sequence_04,0.9f);
                    break;

                case "312": // Vaelis -> Tharok -> Elyra
                    SoundEffects.PlayVoice(Properties.Resources.GuardianVoice_Sequence_05,0.9f);
                    break;

                case "321": // Vaelis -> Elyra -> Tharok
                    SoundEffects.PlayVoice(Properties.Resources.GuardianVoice_Sequence_06,0.9f);
                    break;
            }
            
            #if DEBUG
            // Mostra a sequência apenas na janela Output do Visual Studio.
            System.Diagnostics.Debug.WriteLine("Sequência das Ruínas: " + string.Join(" -> ", _guardianRuneSequence));
            #endif
        }


        private void LimparGuardianRunes()
        {
            foreach (GuardianRune rune in _guardianRunes.ToList())
            {
                if (rune == null)
                    continue;

                world1.Controls.Remove(rune);
                rune.Dispose();
            }

            _guardianRunes.Clear();
            _guardianRuneWorldPositions.Clear();

            _guardianRuneProxima = null;
        }

        #endregion

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
            if (e.KeyCode == Core.Attack0 && !_attack0KeyHeld && !HabilidadeBloqueada(0))
            {
                _attack0KeyHeld = true;
                Core.isAttacking0 = true;
            }
            // Skill 1
            if (e.KeyCode == Core.Attack1 && !_attack1KeyHeld && !HabilidadeBloqueada(1))
            {
                _attack1KeyHeld = true;
                Core.isAttacking1 = true;
            }
            // Skill 3
            if (e.KeyCode == Core.Attack2 && !_attack2KeyHeld && !HabilidadeBloqueada(2))
            {
                _attack2KeyHeld = true;
                Core.isAttacking2 = true;
            }
            // Exit game
            if (e.KeyCode == Core.Exit)
            {
                BTN_EXIT.Image = Properties.Resources.BTN_Exit_pressed;
                Core.isExiting = true;
                return;
            }
            // Interact
            if (e.KeyCode == Core.Interact)
            {
                if (!_interactionKeyHeld)
                {
                    _interactionKeyHeld = true;
                    Core.isInteracting = true;

                    // Atualiza a proximidade antes de interagir.
                    AtualizarRunaProxima();
                    AtualizarProximidadePuzzleChest();
                    AtualizarGuardianRuneProxima();

                    if (_puzzleChestAoAlcance)
                    {
                        if (!_puzzleChest.IsOpening && !_puzzleChest.IsOpen)
                        {
                            SoundEffects.PlayWav(Properties.Resources.TreasureChest_Open,0.8f);

                            _puzzleChest.StartOpening();
                        }
                    }
                    else if (_guardianRuneProxima != null)
                    {
                        TentarAtivarGuardianRune(_guardianRuneProxima);
                    }
                    else if (_runaProxima != null)
                    {
                        if (_runaProxima.StartActivation())
                        {
                            if (_judgmentForgeCarregado)
                            {
                                // Verifica se esta é a primeira runa ativada.
                                bool primeiraAtivacao = !_judgmentForgeMusicaIniciada;

                                // Inicia a música somente na primeira ativação.
                                if (primeiraAtivacao)
                                {
                                    SoundEffects.StartMp3Loop(
                                        Properties.Resources.Judgment_Forge,
                                        0.30f
                                    );

                                    _judgmentForgeMusicaIniciada = true;
                                }

                                SoundEffects.PlayWav(
                                    Properties.Resources.JudgmentRune_Activate,
                                    0.8f
                                );

                                // Vharzun anuncia o início do julgamento.
                                // Esta fala acontece somente na primeira ativação.
                                if (primeiraAtivacao)
                                {
                                    SoundEffects.PlayVoice(
                                        Properties.Resources.Vharzun_Forge_Trial_Start,
                                        0.9f
                                    );
                                }

                                // A contagem começa imediatamente após a primeira runa.
                                IniciarContagemJulgamento();
                            }
                            else
                            {
                                SoundEffects.PlayWav(
                                    Properties.Resources.RootRune_Activate,
                                    0.8f
                                );
                            }
                        }
                    }
                    else if (_nearNextStage)
                    {
                        AvancarStage();
                    }
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
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
                BTN_EXIT.Image = Properties.Resources.BTN_Exit_normal;
                Core.isExiting = false;
                this.Close();
                return;
            }
            if (e.KeyCode == Core.Interact)
            {
                _interactionKeyHeld = false;
                Core.isInteracting = false;
            }
            SkillButtonStates(sender, e);
        }

        private void SkillButtonStates(object sender, EventArgs e)
        {
            switch (_personagem)
            {
                case Personagem.Mago:

                    // Habilidade 1: Staff Attack.
                    BTN_Skill1.Image = HabilidadeBloqueada(0)
                        ? Properties.Resources.duck_Mage_Skill1_locked
                        : _attack0KeyHeld
                            ? Properties.Resources.duck_Mage_Skill1_pressed
                            : Properties.Resources.duck_Mage_Skill1;

                    // Habilidade 2: Fireball.
                    BTN_Skill2.Image = HabilidadeBloqueada(1)
                        ? Properties.Resources.duckMage_Skill2_locked
                        : _attack1KeyHeld
                            ? Properties.Resources.duckMage_Skill2_pressed
                            : Properties.Resources.duckMage_Skill2;

                    // Habilidade 3: Recover Mana.
                    BTN_Skill3.Image = HabilidadeBloqueada(2)
                        ? Properties.Resources.duckMage_Skill3_locked
                        : _attack2KeyHeld
                            ? Properties.Resources.duckMage_Skill3_pressed
                            : Properties.Resources.duckMage_Skill3;

                    break;

                case Personagem.Cavaleiro:

                    // Habilidade 1: Aura.
                    BTN_Skill1.Image = HabilidadeBloqueada(0)
                        ? Properties.Resources.knight_Skill1_locked
                        : _attack0KeyHeld
                            ? Properties.Resources.knight_Skill1_pressed
                            : Properties.Resources.knight_Skill1;

                    // Habilidade 2: Sword Slash.
                    BTN_Skill2.Image = HabilidadeBloqueada(1)
                        ? Properties.Resources.knight_Skill2_locked
                        : _attack1KeyHeld
                            ? Properties.Resources.knight_Skill2_pressed
                            : Properties.Resources.Knight_Skill2;

                    break;

                case Personagem.Princesa:

                    // Habilidade 1.
                    BTN_Skill1.Image = HabilidadeBloqueada(0)
                        ? Properties.Resources.princess_Skill1_locked
                        : _attack0KeyHeld
                            ? Properties.Resources.princess_Skill1_pressed
                            : Properties.Resources.princess_Skill1;

                    // Habilidade 2: Poison Attack.
                    BTN_Skill2.Image = HabilidadeBloqueada(1)
                        ? Properties.Resources.princess_Skill2_locked
                        : _attack1KeyHeld
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
            AtualizarPosicaoMidBossNaTela();
            AtualizarPosicaoLichBossNaTela();
            AtualizarPosicaoDullahanBossNaTela();
            AtualizarPosicaoGothicQueenBossNaTela();
            AtualizarPosicaoRunasNaTela();
            AtualizarPosicaoGuardianRunesNaTela();
            AtualizarPosicaoPuzzleChestNaTela();
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

        private void AtualizarPosicaoMidBossNaTela()
        {
            if (_midBoss == null || _midBoss.IsDisposed)
                return;

            _midBoss.Location = new Point(
                _midBossWorldPosition.X - _cameraX,
                _midBossWorldPosition.Y - _cameraY + _midBossVisualOffsetY
            );
        }

        private void AtualizarPosicaoRunasNaTela()
        {
            foreach (KeyValuePair<PuzzleRune, Point> item in _puzzleRuneWorldPositions)
            {
                PuzzleRune rune = item.Key;
                Point worldPosition = item.Value;

                if (rune == null || rune.IsDisposed)
                    continue;

                rune.Location = new Point(worldPosition.X - _cameraX,worldPosition.Y - _cameraY);
            }
        }

        private void AtualizarPosicaoGuardianRunesNaTela()
        {
            foreach (KeyValuePair<GuardianRune, Point> item
                in _guardianRuneWorldPositions)
            {
                GuardianRune rune = item.Key;
                Point worldPosition = item.Value;

                if (rune == null || rune.IsDisposed)
                    continue;

                rune.Location = new Point(worldPosition.X - _cameraX,worldPosition.Y - _cameraY);
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

            Rectangle hitboxJogador = player.GetHitboxAt(
                _playerWorldX,
                _playerWorldY
            );

            // Considera o contato mesmo quando a colisão
            // bloqueia o movimento alguns pixels antes.
            Rectangle areaContato = new Rectangle(
                hitboxJogador.Left - VelocidadePlayer,
                hitboxJogador.Top,
                hitboxJogador.Width + VelocidadePlayer * 2,
                hitboxJogador.Height
            );

            // Verifica contato com os Orcs.
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
                return;
            }

            // Verifica contato com o Mid-Boss.
            if (_midBoss != null && !_midBoss.IsDisposed && !_midBoss.IsDead)
            {
                Rectangle hitboxBoss = _midBoss.GetHitboxAt(_midBossWorldPosition.X,_midBossWorldPosition.Y);

                if (areaContato.IntersectsWith(hitboxBoss))
                {
                    AplicarDanoAoJogador(_midBoss.DanoContato);
                }
            }
        }

        private void AtualizarBarraVida()
        {
            if (player == null)
                return;

            int vidaAtual = player.VidaAtual;

            int vida = vidaAtual <= 0
                ? 0
                : Math.Max(5, (vidaAtual * 100) / player.VidaMaxima);

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

        private void AtualizarHUDLichBoss()
        {
            if (_lichBoss == null || _lichBoss.IsDisposed)
                return;

            int vidaPercentual = _lichBoss.VidaAtual <= 0
                ? 0
                : (_lichBoss.VidaAtual * 100) / _lichBoss.VidaMaxima;

            // Seleciona a imagem correspondente, em intervalos de 5%.
            int percentualBarra = vidaPercentual == 0
                ? 0
                : Math.Min(100, ((vidaPercentual + 4) / 5) * 5);

            string nomeRecurso = "LichBoss_HealthBar_HP" + percentualBarra;

            PBX_HealtBar_Final_Boss.Image = Properties.Resources.ResourceManager.GetObject(nomeRecurso) as Bitmap;
            // Troca o avatar quando o Lich é derrotado.
            if (_lichBoss.VidaAtual <= 0)
            {
                PBX_Avatar_Final_Boss.Image = Properties.Resources.LichBoss_Avatar_Death;
            }
        }

        private bool AplicarDanoAosInimigos(int dano, int alcance, bool aplicarVeneno = false)
        {
            if (player == null || player.EstaMorto)
                return false;

            Rectangle hitboxPlayer = player.GetHitboxAt(_playerWorldX, _playerWorldY);

            int alturaAtaque = Math.Max(12, hitboxPlayer.Height / 2);
            int ataqueY = hitboxPlayer.Top + (hitboxPlayer.Height - alturaAtaque) / 2;

            Rectangle hitboxAtaque;

            if (player.FacingRight)
            {
                hitboxAtaque = new Rectangle(hitboxPlayer.Right, ataqueY, alcance, alturaAtaque);
            }
            else
            {
                hitboxAtaque = new Rectangle(hitboxPlayer.Left - alcance, ataqueY, alcance, alturaAtaque);
            }

            bool atingiuInimigo = false;
            bool atingiuAlvo = false;

            foreach (KeyValuePair<Enemy, Point> item in _enemyWorldPositions)
            {
                Enemy enemy = item.Key;

                if (enemy == null || enemy.IsDisposed || enemy.IsDead)
                    continue;

                Point posicao = item.Value;

                Rectangle hitboxInimigo = enemy.GetHitboxAt(posicao.X, posicao.Y);

                if (!hitboxAtaque.IntersectsWith(hitboxInimigo))
                    continue;

                enemy.ReceberDano(dano);

                // Se sobreviveu ao golpe, inicia o recuo.
                if (!enemy.IsDead)
                    IniciarRecuoInimigo(enemy);

                // Aplica veneno se o golpe for da habilidade da Princesa.
                if (aplicarVeneno && !enemy.IsDead)
                {
                    enemy.AplicarVeneno(
                        danoPorTick: CalcularDanoComRunaPoder(3, 25),
                        duracaoTicks: 300,
                        intervaloTicks: 60
                    );
                }

                atingiuInimigo = true;
                atingiuAlvo = true;
                break;
            }

            bool atingiuChefe = false;

            // Verifica se o ataque atingiu o Mid-Boss.
            if (!atingiuInimigo && _midBoss != null && !_midBoss.IsDisposed && !_midBoss.IsDead)
            {
                Rectangle hitboxBoss = _midBoss.GetHitboxAt(
                    _midBossWorldPosition.X,
                    _midBossWorldPosition.Y
                );

                if (hitboxAtaque.IntersectsWith(hitboxBoss))
                {
                    _midBoss.ReceberDano(dano);
                    atingiuChefe = true;
                    atingiuAlvo = true;

                    // Aplica veneno quando o golpe for da habilidade da Princesa.
                    if (aplicarVeneno && !_midBoss.IsDead)
                    {
                        _midBoss.AplicarVeneno(
                            danoPorTick: CalcularDanoComRunaPoder(3, 25),
                            duracaoTicks: 300,
                            intervaloTicks: 60
                        );
                    }
                }
            }

            // Verifica se o ataque atingiu o chefe final: Lich.
            if (!atingiuInimigo && !atingiuChefe && _lichBoss != null && !_lichBoss.IsDisposed && !_lichBoss.IsDead)
            {
                Rectangle hitboxLich = _lichBoss.GetHitboxAt(
                    _lichBossWorldPosition.X,
                    _lichBossWorldPosition.Y
                );

                if (hitboxAtaque.IntersectsWith(hitboxLich))
                {
                    _lichBoss.ReceberDano(dano);
                    AtualizarHUDLichBoss();
                    atingiuAlvo = true;
                }
            }
            // Verifica se o ataque atingiu o Dullahan.
            if (!atingiuInimigo && !atingiuChefe && _dullahanBoss != null && !_dullahanBoss.IsDisposed && !_dullahanBoss.IsDead)
            {
                Rectangle hitboxDullahan = _dullahanBoss.GetHitboxAt(
                    _dullahanBossWorldPosition.X,
                    _dullahanBossWorldPosition.Y
                );

                if (hitboxAtaque.IntersectsWith(hitboxDullahan))
                {
                    _dullahanBoss.ReceberDano(dano);
                    AtualizarHUDDullahanBoss();
                    atingiuAlvo = true;
                }
            }
            // Verifica se o ataque atingiu a Rainha Gótica.
            if (!atingiuInimigo && !atingiuChefe && _gothicQueenBoss != null && !_gothicQueenBoss.IsDisposed && !_gothicQueenBoss.IsDead)
            {
                Rectangle hitboxRainha = _gothicQueenBoss.GetHitboxAt(
                    _gothicQueenBossWorldPosition.X,
                    _gothicQueenBossWorldPosition.Y
                );

                if (hitboxAtaque.IntersectsWith(hitboxRainha))
                {
                    // Aplica o dano recebido pela Rainha.
                    _gothicQueenBoss.ReceberDano(dano);
                    AtualizarHUDGothicQueenBoss();

                    // Registra a vida restante para teste.
                    System.Diagnostics.Debug.WriteLine(
                        "[Rainha Gótica] Vida restante: " +
                        _gothicQueenBoss.VidaAtual
                    );

                    atingiuChefe = true;
                    atingiuAlvo = true;
                }
            }
            return atingiuAlvo;
        }

        private void Player_AttackImpact(StatePlayer state)
        {
            switch (_personagem)
            {
                case Personagem.Mago:
                    if (state == StatePlayer.Attack)
                    {
                        // Som do cajado cortando o ar com magia de fogo.
                        SoundEffects.PlayWav(
                            Properties.Resources.Mage_Staff_Swing,
                            0.80f
                        );

                        // Aplica o dano e verifica se algum alvo foi atingido.
                        bool atingiuAlvo = AplicarDanoAosInimigos(10, 44);

                        // Som de impacto flamejante somente quando acertar.
                        if (atingiuAlvo)
                        {
                            SoundEffects.PlayWav(
                                Properties.Resources.Mage_Staff_Fire_Impact,
                                0.90f
                            );
                        }
                    }
                    else if (state == StatePlayer.Fireball)
                    {
                        CriarBolaDeFogo();
                    }
                    break;

                case Personagem.Cavaleiro:
                    if (state == StatePlayer.Attack)
                    {
                        int danoBase = _knightAuraRemaining > 0 
                            ? KnightAuraDamage 
                            : KnightBaseDamage;

                        int dano = CalcularDanoComRunaPoder(danoBase, 20);

                        AplicarDanoAosInimigos(dano, 52);
                    }
                    break;

                case Personagem.Princesa:

                    // Habilidade 1: Strong Sword Attack.
                    if (state == StatePlayer.Attack)
                    {
                        // Som do movimento da espada.
                        SoundEffects.PlayWav(
                            Properties.Resources.Princess_Sword_Swing,
                            0.85f
                        );

                        // Calcula o dano com o bônus das Runas de Poder.
                        int danoAtaque = CalcularDanoComRunaPoder(40, 25);

                        // Aplica o dano e verifica se algum alvo foi atingido.
                        bool atingiuAlvo = AplicarDanoAosInimigos(danoAtaque, 48);

                        // Som do impacto somente quando acertar.
                        if (atingiuAlvo)
                        {
                            SoundEffects.PlayWav(
                                Properties.Resources.Princess_Sword_Impact,
                                0.90f
                            );
                        }
                    }

                    // Habilidade 2: Poison Attack.
                    else if (state == StatePlayer.Poison_Attack)
                    {
                        // Som do ataque envenenado.
                        SoundEffects.Play(
                            Properties.Resources.Princess_Poison_Attack,
                            0.90f
                        );

                        int danoVeneno = CalcularDanoComRunaPoder(20, 25);

                        AplicarDanoAosInimigos(danoVeneno, 48, true);
                    }

                    break;
            }
        }

        private void Enemy_AttackImpact(Enemy enemy)
        {
            if (enemy == null || enemy.IsDisposed || enemy.IsDead)
                return;

            // Reproduz o som do golpe do Orc.
            SoundEffects.PlayWav(Properties.Resources.Orc_Attack_Swing, 0.8f);

            // Verifica se o jogador pode receber dano.
            if (player == null || player.IsDisposed || player.EstaMorto || _invulnerabilidadeRestante > 0)
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

        private void MidBoss_AttackImpact(MidBoss boss,MidBoss.BossAttackType attackType)
        {
            // Verifica se o Mid-Boss ainda pode executar o ataque.
            if (boss == null || boss.IsDisposed || boss.IsDead)
                return;

            // Reproduz o som correspondente ao ataque executado.
            switch (attackType)
            {
                case MidBoss.BossAttackType.Attack1:
                    SoundEffects.PlayWav(
                        Properties.Resources.MidBoss_Attack1_Swing,
                        0.85f
                    );
                    break;

                case MidBoss.BossAttackType.Attack2:
                    SoundEffects.PlayWav(
                        Properties.Resources.MidBoss_Attack2_Swing,
                        0.85f
                    );
                    break;
            }

            // Verifica se o jogador pode receber dano.
            if (player == null || player.IsDisposed || player.EstaMorto ||
                _invulnerabilidadeRestante > 0)
            {
                return;
            }

            // Obtém as hitboxes no mundo.
            Rectangle hitboxJogador = player.GetHitboxAt(_playerWorldX,_playerWorldY);

            Rectangle hitboxBoss = boss.GetHitboxAt(_midBossWorldPosition.X,_midBossWorldPosition.Y);

            // Define a altura da área atingida pelo golpe.
            int alturaAtaque = Math.Max(24, hitboxBoss.Height - 16);

            int ataqueY = hitboxBoss.Top + (hitboxBoss.Height - alturaAtaque) / 2;

            Rectangle areaAtaque;

            // Cria a área de ataque conforme a direção do chefe.
            if (boss.FacingRight)
            {
                areaAtaque = new Rectangle(hitboxBoss.Right,ataqueY,MidBossAttackReach,alturaAtaque);
            }
            else
            {
                areaAtaque = new Rectangle(hitboxBoss.Left - MidBossAttackReach,ataqueY,MidBossAttackReach,alturaAtaque);
            }

            // O golpe não causa dano se o jogador estiver fora da área.
            if (!areaAtaque.IntersectsWith(hitboxJogador))
                return;

            // Seleciona o dano de acordo com o ataque executado.
            int dano = attackType == MidBoss.BossAttackType.Attack1 ? boss.DanoAtaque1 : boss.DanoAtaque2;

            AplicarDanoAoJogador(dano);
        }

        private void CriarDarkFireBall()
        {
            if (_lichBoss == null ||
                _lichBoss.IsDisposed ||
                _lichBoss.IsDead ||
                player == null ||
                player.IsDisposed ||
                player.EstaMorto)
            {
                return;
            }

            bool facingRight = _lichBoss.FacingRight;

            // Limites visíveis do sprite atual do Lich.
            Rectangle spriteBounds = _lichBoss.GetVisibleSpriteBounds();

            int spriteWorldLeft = _lichBossWorldPosition.X + spriteBounds.Left;

            int spriteWorldRight = _lichBossWorldPosition.X + spriteBounds.Right;

            int spriteWorldTop = _lichBossWorldPosition.Y + _lichBossVisualOffsetY + spriteBounds.Top;

            // Posiciona o projétil próximo à extremidade do cajado.
            int projectileX = facingRight ? spriteWorldRight - 18 : spriteWorldLeft - 68 + 18;

            // Centraliza o projétil na altura do sprite.
            int projectileY = spriteWorldTop + (spriteBounds.Height - 68) / 2;

            DarkFireballProjectile projectile = new DarkFireballProjectile(facingRight,projectileX,projectileY);

            _darkFireballs.Add(projectile);

            // Converte as coordenadas do mundo para a tela.
            projectile.Location = new Point(projectile.WorldX - _cameraX,projectile.WorldY - _cameraY);

            world1.Controls.Add(projectile);

            projectile.BringToFront();
        }

        // Verifica se determinada habilidade está bloqueada.
        private bool HabilidadeBloqueada(int slot)
        {
            // Bloqueio aleatório aplicado pelo Lich.
            bool bloqueioLich =
                _lichSkillBlockRemaining > 0 &&
                _lichBlockedSkillSlot == slot;

            // Bloqueio específico da Aura aplicado pelo Dullahan.
            bool bloqueioDullahan =
                slot == 0 &&
                _dullahanAuraBlockRemaining > 0;

            return bloqueioLich || bloqueioDullahan;
        }

        private void AplicarBloqueioAuraDullahan()
        {
            if (_personagem != Personagem.Cavaleiro ||
                player == null ||
                player.IsDisposed ||
                player.EstaMorto)
            {
                return;
            }

            // Bloqueia a Aura por aproximadamente 6 segundos.
            _dullahanAuraBlockRemaining = DullahanAuraBlockDurationTicks;

            // Cancela a Aura caso esteja ativa.
            _knightAuraRemaining = 0;

            player.AuraAtiva = false;

            SoundEffects.StopKnightAura();

            // Cancela qualquer comando pendente da Aura.
            _attack0KeyHeld = false;
            Core.isAttacking0 = false;

            // Atualiza o efeito visual vermelho do Dullahan.
            AtualizarVisualDebuffJogador();

            // Atualiza a aparência do botão da habilidade.
            SkillButtonStates(this, EventArgs.Empty);
        }

        // Sorteia qual efeito será aplicado pela Dark Fireball.
        private void AplicarEfeitoAleatorioDarkFireball()
        {
            if (player == null || player.IsDisposed || player.EstaMorto)
                return;

            // 50% queimadura e 50% bloqueio de habilidade.
            if (_darkFireballDebuffRandom.Next(2) == 0)
            {
                // Reutiliza o sistema de queimadura existente.
                _lichBurnDurationRemaining = LichBurnDurationTicks;
                _lichBurnTickRemaining = LichBurnIntervalTicks;

                player.QueimaduraAtiva = true;
                AtualizarVisualDebuffJogador();
            }
            else
            {
                AplicarBloqueioAleatorioHabilidade();
            }
        }

        // Escolhe uma habilidade funcional do personagem atual.
        private void AplicarBloqueioAleatorioHabilidade()
        {
            // Se já existe uma habilidade bloqueada,
            // apenas renova sua duração.
            if (_lichSkillBlockRemaining > 0)
            {
                _lichSkillBlockRemaining = LichSkillBlockDurationTicks;
                AtualizarVisualDebuffJogador();
                return;
            }

            int[] habilidadesDisponiveis;

            switch (_personagem)
            {
                case Personagem.Mago:
                    habilidadesDisponiveis = new int[] { 0, 1, 2 };
                    break;

                case Personagem.Cavaleiro:
                    habilidadesDisponiveis = new int[] { 0, 1 };
                    break;

                case Personagem.Princesa:
                    habilidadesDisponiveis = new int[] { 0, 1 };
                    break;

                default:
                    return;
            }

            int indiceAleatorio = _darkFireballDebuffRandom.Next(habilidadesDisponiveis.Length);

            _lichBlockedSkillSlot = habilidadesDisponiveis[indiceAleatorio];

            _lichSkillBlockRemaining = LichSkillBlockDurationTicks;

            // Cancela qualquer comando pendente da habilidade bloqueada.
            switch (_lichBlockedSkillSlot)
            {
                case 0:
                    _attack0KeyHeld = false;
                    Core.isAttacking0 = false;
                    break;

                case 1:
                    _attack1KeyHeld = false;
                    Core.isAttacking1 = false;
                    break;

                case 2:
                    _attack2KeyHeld = false;
                    Core.isAttacking2 = false;
                    break;
            }
            SkillButtonStates(this, EventArgs.Empty);
        }

        // Controla o tempo restante do bloqueio.
        private void AtualizarBloqueioHabilidadeJogador()
        {
            bool bloqueioExpirou = false;

            // Atualiza o bloqueio de habilidade do Lich.
            if (_lichSkillBlockRemaining > 0)
            {
                _lichSkillBlockRemaining--;

                if (_lichSkillBlockRemaining <= 0)
                {
                    _lichSkillBlockRemaining = 0;
                    _lichBlockedSkillSlot = -1;

                    bloqueioExpirou = true;
                }
            }

            // Atualiza o bloqueio da Aura do Dullahan.
            if (_dullahanAuraBlockRemaining > 0)
            {
                _dullahanAuraBlockRemaining--;

                if (_dullahanAuraBlockRemaining <= 0)
                {
                    _dullahanAuraBlockRemaining = 0;

                    bloqueioExpirou = true;
                }
            }

            if (bloqueioExpirou)
            {
                // Mantém o efeito visual enquanto houver outro bloqueio.
                if (player != null && !player.IsDisposed)
                {
                    AtualizarVisualDebuffJogador();

                    SkillButtonStates(this, EventArgs.Empty);
                }

                SkillButtonStates(this, EventArgs.Empty);
            }
        }

        private void AtualizarVisualDebuffJogador()
        {
            if (player == null || player.IsDisposed)
                return;

            player.DebuffVisualAtivo =
                _lichBurnDurationRemaining > 0 ||
                _lichSkillBlockRemaining > 0 ||
                _dullahanAuraBlockRemaining > 0;
        }

        private void AtualizarDarkFireballs()
        {
            for (int i = _darkFireballs.Count - 1; i >= 0; i--)
            {
                DarkFireballProjectile projectile = _darkFireballs[i];

                if (projectile == null || projectile.IsDisposed)
                {
                    _darkFireballs.RemoveAt(i);
                    continue;
                }

                projectile.UpdateProjectile();

                // Remove somente depois que a animação de impacto terminar.
                if (projectile.IsFinished)
                {
                    world1.Controls.Remove(projectile);
                    _darkFireballs.RemoveAt(i);
                    projectile.Dispose();
                    continue;
                }

                // Atualiza a posição de acordo com a câmera.
                projectile.Location = new Point(
                    projectile.WorldX - _cameraX,
                    projectile.WorldY - _cameraY
                );

                // Durante a explosão, não verifica novas colisões.
                if (projectile.IsImpacting)
                    continue;

                Rectangle hitboxBola = projectile.GetCollisionBounds();

                bool iniciarImpacto = false;

                // Verifica se o projétil saiu dos limites do mapa.
                if (_imageMap == null ||
                    projectile.WorldX + projectile.Width < 0 ||
                    projectile.WorldX > _imageMap.Width ||
                    projectile.WorldY + projectile.Height < 0 ||
                    projectile.WorldY > _imageMap.Height)
                {
                    iniciarImpacto = true;
                }
                // Verifica colisão com o cenário.
                else if (map == null || !map.PodeMover(hitboxBola))
                {
                    iniciarImpacto = true;
                }
                else if (player != null && !player.IsDisposed && !player.EstaMorto)
                {
                    // Hitbox real do jogador.
                    Rectangle hitboxJogador = player.GetHitboxAt(_playerWorldX,_playerWorldY);

                    // Colisão entre a bola roxa e o jogador.
                    if (hitboxBola.IntersectsWith(hitboxJogador))
                    {
                        // Só aplica o efeito se o jogador puder receber dano.
                        if (_invulnerabilidadeRestante <= 0)
                        {
                            AplicarDanoAoJogador(LichFireBallDamage);

                            // Sorteia queimadura ou bloqueio de habilidade.
                            AplicarEfeitoAleatorioDarkFireball();
                        }

                        iniciarImpacto = true;
                    }
                }

                // Encerra o voo ao atingir a distância máxima.
                if (!iniciarImpacto && projectile.ExceededMaxDistance)
                {
                    iniciarImpacto = true;
                }

                // Reproduz o impacto quando a bola de fogo explode.
                if (iniciarImpacto)
                {
                    SoundEffects.Play(Properties.Resources.Lich_DarkFireball_Impact,0.95f);

                    projectile.StartImpact();
                }
            }
        }

        private void LichBoss_AttackImpact(LichBoss boss,LichBoss.TipoAtaque attackType)
        {
            if (boss == null || boss.IsDisposed || boss.IsDead || boss != _lichBoss)
            {
                return;
            }

            // Toca o impacto no momento correto da animação.
            if (attackType == LichBoss.TipoAtaque.DarkFlameBurst)
            {
                SoundEffects.Play(
                    Properties.Resources.Lich_DarkFlameBurst_Impact,
                    0.95f
                );
            }

            if (player == null || player.IsDisposed || player.EstaMorto || _invulnerabilidadeRestante > 0)
            {
                return;
            }

            // Cria a bola de fogo no momento do lançamento.
            if (attackType == LichBoss.TipoAtaque.DarkFireBall)
            {
                CriarDarkFireBall();
                return;
            }

            if (attackType != LichBoss.TipoAtaque.DarkFlameBurst)
                return;

            // O Dark Flame Burst não atinge jogadores no ar.
            if (!_estaNoChao)
                return;

            Rectangle hitboxJogador = player.GetHitboxAt(
                _playerWorldX,
                _playerWorldY
            );

            Rectangle hitboxBoss = boss.GetHitboxAt(
                _lichBossWorldPosition.X,
                _lichBossWorldPosition.Y
            );

            Rectangle areaAtaque = new Rectangle(
                hitboxBoss.Left - LichFlameBurstRadius,
                hitboxBoss.Top - 24,
                hitboxBoss.Width + (LichFlameBurstRadius * 2),
                hitboxBoss.Height + 24
            );

            if (!areaAtaque.IntersectsWith(hitboxJogador))
                return;

            AplicarDanoAoJogador(LichMeleeAttackDamage);

            if (player.EstaMorto)
                return;

            if (_darkFireballDebuffRandom.Next(100) < 50)
            {
                _lichBurnDurationRemaining = LichBurnDurationTicks;
                _lichBurnTickRemaining = LichBurnIntervalTicks;

                player.QueimaduraAtiva = true;

                AtualizarVisualDebuffJogador();
            }
        }

        private void DullahanBoss_AttackImpact(DullahanBoss boss,DullahanBoss.TipoAtaque attackType)
        {
            if (boss == null || boss.IsDisposed || boss.IsDead || boss != _dullahanBoss)
            {
                return;
            }

            // Reproduz o impacto correspondente ao ataque.
            // O som não depende de o jogador receber dano.
            if (attackType == DullahanBoss.TipoAtaque.Attack1)
            {
                SoundEffects.Play(Properties.Resources.Dullahan_Attack1_Impact,0.85f);
            }
            else if (attackType == DullahanBoss.TipoAtaque.Attack2)
            {
                SoundEffects.Play(Properties.Resources.Dullahan_Attack2_Impact,1.0f);
            }

            // Verifica se o jogador pode receber dano.
            if (player == null || player.IsDisposed || player.EstaMorto || _invulnerabilidadeRestante > 0)
            {
                return;
            }

            Rectangle hitboxJogador = player.GetHitboxAt(
                _playerWorldX,
                _playerWorldY
            );

            Rectangle hitboxBoss = boss.GetHitboxAt(
                _dullahanBossWorldPosition.X,
                _dullahanBossWorldPosition.Y
            );

            bool ataquePesado =
                attackType == DullahanBoss.TipoAtaque.Attack2;

            // O ataque pesado não atinge jogadores no ar.
            if (ataquePesado && !_estaNoChao)
                return;

            int alcance = ataquePesado
                ? DullahanAttack2Range
                : DullahanAttack1Range;

            int altura = ataquePesado
                ? hitboxBoss.Height
                : hitboxBoss.Height / 2;

            int ataqueY = ataquePesado
                ? hitboxBoss.Top
                : hitboxBoss.Top + hitboxBoss.Height / 2;

            Rectangle areaAtaque;

            if (boss.FacingRight)
            {
                areaAtaque = new Rectangle(
                    hitboxBoss.Right - 2,
                    ataqueY,
                    alcance,
                    altura
                );
            }
            else
            {
                areaAtaque = new Rectangle(
                    hitboxBoss.Left - alcance + 2,
                    ataqueY,
                    alcance,
                    altura
                );
            }

            if (!areaAtaque.IntersectsWith(hitboxJogador))
                return;

            int dano = ataquePesado
                ? DullahanAttack2Damage
                : DullahanAttack1Damage;

            AplicarDanoAoJogador(dano);

            if (ataquePesado && !player.EstaMorto)
            {
                AplicarBloqueioAuraDullahan();
            }
        }

        private void GothicQueenBoss_AttackImpact(GothicQueenBoss boss,GothicQueenBoss.TipoAtaque attackType)
        {
            if (boss == null || boss.IsDisposed || boss.IsDead || boss != _gothicQueenBoss)
            {
                return;
            }

            // Royal Venom Slash: impacto do golpe venenoso.
            if (attackType == GothicQueenBoss.TipoAtaque.RoyalVenomSlash)
            {
                SoundEffects.Play(
                    Properties.Resources.Fallen_Queen_RoyalVenomSlash_Impact,
                    0.95f
                );
            }
            // Emerald Bladefall: impacto das espadas no chão.
            else if (attackType == GothicQueenBoss.TipoAtaque.EmeraldBladefall)
            {
                SoundEffects.Play(
                    Properties.Resources.Fallen_Queen_EmeraldBladefall_SwordImpact,
                    0.95f
                );
            }

            if (player == null || player.IsDisposed || player.EstaMorto)
            {
                return;
            }

            // Emerald Bladefall: cria a onda venenosa.
            if (attackType == GothicQueenBoss.TipoAtaque.EmeraldBladefall)
            {
                CriarOndaVenenosaRainha(boss);
                return;
            }

            // O Royal Venom Slash respeita a invulnerabilidade.
            if (attackType != GothicQueenBoss.TipoAtaque.RoyalVenomSlash ||
                _invulnerabilidadeRestante > 0)
            {
                return;
            }

            Rectangle hitboxJogador = player.GetHitboxAt(
                _playerWorldX,
                _playerWorldY
            );

            Rectangle hitboxRainha = boss.GetHitboxAt(
                _gothicQueenBossWorldPosition.X,
                _gothicQueenBossWorldPosition.Y
            );

            Rectangle areaAtaque;

            if (boss.FacingRight)
            {
                areaAtaque = new Rectangle(
                    hitboxRainha.Right - 8,
                    hitboxRainha.Top - 15,
                    RoyalVenomSlashRange,
                    hitboxRainha.Height + 25
                );
            }
            else
            {
                areaAtaque = new Rectangle(
                    hitboxRainha.Left - RoyalVenomSlashRange + 8,
                    hitboxRainha.Top - 15,
                    RoyalVenomSlashRange,
                    hitboxRainha.Height + 25
                );
            }

            if (!areaAtaque.IntersectsWith(hitboxJogador))
                return;

            AplicarDanoAoJogador(RoyalVenomSlashDamage);

            if (player.EstaMorto)
                return;

            if (_gothicQueenRandom.Next(100) <
                RoyalVenomSlashPoisonChance)
            {
                AplicarVenenoDaRainha();
            }
        }

        private void CriarOndaVenenosaRainha(GothicQueenBoss boss)
        {
            if (boss == null ||
                boss.IsDisposed ||
                world1 == null)
                return;

            Rectangle hitboxRainha = boss.GetHitboxAt(
                _gothicQueenBossWorldPosition.X,
                _gothicQueenBossWorldPosition.Y
            );

            // A onda nasce à frente da Rainha.
            int waveX = boss.FacingRight
                ? hitboxRainha.Right - 10
                : hitboxRainha.Left - 96 + 10;

            // Procura o chão abaixo do ponto de nascimento da onda.
            int alturaChao = hitboxRainha.Bottom;

            if (map != null)
            {
                int limiteBusca = map.tileSize * 4;

                for (int deslocamento = 0;
                     deslocamento <= limiteBusca;
                     deslocamento++)
                {
                    Rectangle faixaChao = new Rectangle(
                        waveX + 14,
                        hitboxRainha.Bottom + deslocamento,
                        68,
                        1
                    );

                    // Quando não pode mover, encontrou uma superfície sólida.
                    if (!map.PodeMover(faixaChao))
                    {
                        alturaChao = hitboxRainha.Bottom + deslocamento;
                        break;
                    }
                }
            }

            // O desenho da onda ocupa 50 dos seus 56 pixels de altura.
            int waveY = alturaChao - 50;

            int direction = boss.FacingRight ? 1 : -1;

            QueenPoisonWave wave = new QueenPoisonWave(
                waveX,
                waveY,
                direction
            );

            _queenPoisonWaves.Add(wave);

            wave.Location = new Point(
                waveX - _cameraX,
                waveY - _cameraY
            );

            world1.Controls.Add(wave);

            player.BringToFront();
            boss.BringToFront();

            // A onda fica visualmente à frente dos personagens.
            wave.BringToFront();
        }

        private void AtualizarOndasVenenosasRainha()
        {
            if (player == null || player.IsDisposed || player.EstaMorto)
            {
                foreach (QueenPoisonWave wave in _queenPoisonWaves.ToList())
                {
                    if (wave != null && !wave.IsDisposed)
                    {
                        world1.Controls.Remove(wave);
                        wave.Dispose();
                    }
                }

                _queenPoisonWaves.Clear();

                AtualizarSomOndaVenenosaRainha();

                return;
            }

            Rectangle hitboxJogador = player.GetHitboxAt(
                _playerWorldX,
                _playerWorldY
            );

            int centroJogadorX =
                hitboxJogador.Left + hitboxJogador.Width / 2;

            for (int i = _queenPoisonWaves.Count - 1; i >= 0; i--)
            {
                QueenPoisonWave wave = _queenPoisonWaves[i];

                if (wave == null || wave.IsDisposed)
                {
                    _queenPoisonWaves.RemoveAt(i);
                    continue;
                }

                // Atualiza o movimento em direção ao jogador.
                wave.UpdateWave(centroJogadorX);

                // Explode quando chega próximo da Princesa.
                int centroOndaX = wave.WorldX + wave.Width / 2;

                if (!wave.IsExploding && Math.Abs(centroOndaX - centroJogadorX) <= QueenPoisonWaveTriggerDistance)
                {
                    // A Princesa precisa estar no chão para ser atingida.
                    if (_estaNoChao && _invulnerabilidadeRestante <= 0)
                    {
                        // Aplica o dano direto.
                        AplicarDanoAoJogador(EmeraldBladefallDamage);

                        // Aplica veneno garantido se a Princesa sobreviver.
                        if (!player.EstaMorto)
                        {
                            AplicarVenenoDaRainha();
                        }
                    }

                    // Reproduz a explosão quando a onda alcança o jogador.
                    SoundEffects.Play(Properties.Resources.Fallen_Queen_EmeraldBladefall_WaveExplosion,1.0f);

                    wave.StartExplosion();
                }

                // Remove depois que a animação da explosão termina.
                if (wave.IsFinished)
                {
                    world1.Controls.Remove(wave);
                    wave.Dispose();

                    _queenPoisonWaves.RemoveAt(i);
                    continue;
                }

                // Atualiza a posição considerando a câmera.
                wave.Location = new Point(
                    wave.WorldX - _cameraX,
                    wave.WorldY - _cameraY
                );
                wave.BringToFront();
            }
            AtualizarSomOndaVenenosaRainha();
        }

        private void AtualizarSomOndaVenenosaRainha()
        {
            bool existeOndaPerseguindo = _queenPoisonWaves.Any(
                wave =>
                    wave != null &&
                    !wave.IsDisposed &&
                    !wave.IsExploding &&
                    !wave.IsFinished
            );

            // Enquanto existir uma onda perseguindo, garante que o áudio esteja ativo.
            if (existeOndaPerseguindo)
            {
                if (!SoundEffects.IsQueenPoisonWaveActive)
                {
                    SoundEffects.StartQueenPoisonWave(
                        Properties.Resources.Fallen_Queen_EmeraldBladefall_Wave,
                        0.32f
                    );
                }
            }
            // Quando nenhuma onda estiver perseguindo, interrompe o áudio.
            else if (SoundEffects.IsQueenPoisonWaveActive)
            {
                SoundEffects.StopQueenPoisonWave();
            }
        }

        private void AplicarVenenoDaRainha()
        {
            if (player == null ||
                player.IsDisposed ||
                player.EstaMorto)
                return;

            // Renova a duração caso o jogador já esteja envenenado.
            _queenPoisonDurationRemaining = QueenPoisonDurationTicks;

            _queenPoisonTickRemaining = QueenPoisonIntervalTicks;

            // Ativa o efeito visual verde próprio do veneno.
            player.VenenoAtivo = true;
        }

        private void AtualizarVenenoJogadorRainha()
        {
            if (_queenPoisonDurationRemaining <= 0)
                return;

            if (player == null ||
                player.IsDisposed ||
                player.EstaMorto)
            {
                _queenPoisonDurationRemaining = 0;
                _queenPoisonTickRemaining = 0;

                if (player != null && !player.IsDisposed)
                    player.VenenoAtivo = false;

                return;
            }

            // Diminui a duração do envenenamento.
            _queenPoisonDurationRemaining--;

            if (_queenPoisonDurationRemaining <= 0)
            {
                _queenPoisonDurationRemaining = 0;
                _queenPoisonTickRemaining = 0;

                player.VenenoAtivo = false;
                return;
            }

            // Aguarda o próximo dano contínuo.
            if (_queenPoisonTickRemaining > 0)
            {
                _queenPoisonTickRemaining--;
                return;
            }

            // Aguarda o fim da invulnerabilidade.
            if (_invulnerabilidadeRestante > 0)
                return;

            AplicarDanoAoJogador(QueenPoisonDamagePerTick);

            _queenPoisonTickRemaining = QueenPoisonIntervalTicks;
        }

        private void AtualizarQueimaduraJogador()
        {
            if (_lichBurnDurationRemaining <= 0)
                return;

            if (player == null || player.IsDisposed || player.EstaMorto)
            {
                _lichBurnDurationRemaining = 0;
                _lichBurnTickRemaining = 0;
                return;
            }

            // Diminui a duração da queimadura.
            _lichBurnDurationRemaining--;

            if (_lichBurnDurationRemaining <= 0)
            {
                player.QueimaduraAtiva = false;
                _lichBurnTickRemaining = 0;

                AtualizarVisualDebuffJogador();

                return;
            }

            // Aguarda o intervalo entre os danos.
            if (_lichBurnTickRemaining > 0)
            {
                _lichBurnTickRemaining--;
                return;
            }

            // Aguarda o fim da invulnerabilidade do jogador.
            if (_invulnerabilidadeRestante > 0)
                return;

            // Aplica o dano contínuo.
            AplicarDanoAoJogador(LichBurnDamagePerTick);

            _lichBurnTickRemaining = LichBurnIntervalTicks;
        }

        private void AplicarDanoAoJogador(int dano)
        {
            if (player == null || player.IsDisposed || player.EstaMorto || _invulnerabilidadeRestante > 0)
            {
                return;
            }

            int danoFinal = dano;

            // Reduz o dano recebido enquanto a Aura do Cavaleiro estiver ativa.
            if (_personagem == Personagem.Cavaleiro && _knightAuraRemaining > 0)
            {
                danoFinal = Math.Max(1, (int)Math.Ceiling(danoFinal * 0.5));
            }

            // Aplica resistência proporcional à quantidade de Runas do Poder.
            int quantidadeRunasPoder = _runasAdquiridas.Count(r => r == TipoRuna.Poder);

            if (quantidadeRunasPoder > 0 && (_personagem == Personagem.Cavaleiro || _personagem == Personagem.Princesa))
            {
                int resistenciaTotal = BonusResistenciaRunaPoder * quantidadeRunasPoder;

                danoFinal = Math.Max(1,(int)Math.Ceiling(danoFinal * (100 - resistenciaTotal) / 100.0));
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
                    foreach (KeyValuePair<Enemy, Point> item in _enemyWorldPositions)
                    {
                        Enemy enemy = item.Key;

                        if (enemy == null || enemy.IsDisposed || enemy.IsDead)
                        {
                            continue;
                        }

                        Point posicao = item.Value;

                        Rectangle hitboxInimigo = enemy.GetHitboxAt(posicao.X, posicao.Y);

                        if (!hitboxBola.IntersectsWith(hitboxInimigo))
                            continue;

                        // Dano imediato da bola de fogo.
                        int danoFireball = CalcularDanoComRunaPoder(FireballDamage, 20);
                        enemy.ReceberDano(danoFireball);

                        if (!enemy.IsDead)
                        {
                            // Inicia a queimadura por aproximadamente 4 segundos.
                            enemy.AplicarQueimadura(danoPorTick: 2, duracaoTicks: 240, intervaloTicks: 60);
                            IniciarRecuoInimigo(enemy);
                        }

                        iniciarExplosao = true;
                        break;
                    }
                    // Colisão com o Mid-Boss.
                    if (!iniciarExplosao && _midBoss != null && !_midBoss.IsDisposed && !_midBoss.IsDead)
                    {
                        Rectangle hitboxBoss = _midBoss.GetHitboxAt(_midBossWorldPosition.X,_midBossWorldPosition.Y);

                        if (hitboxBola.IntersectsWith(hitboxBoss))
                        {
                            int danoFireball = CalcularDanoComRunaPoder(FireballDamage, 20);
                            _midBoss.ReceberDano(danoFireball);
                            // Aplica queimadura se o chefe sobreviver ao impacto.
                            if (!_midBoss.IsDead)
                            {
                                _midBoss.AplicarQueimadura(
                                    danoPorTick: 2,
                                    duracaoTicks: 240,
                                    intervaloTicks: 60
                                );
                            }
                            iniciarExplosao = true;
                        }
                    }
                    // Colisão com o chefe final: Lich.
                    if (!iniciarExplosao && _lichBoss != null && !_lichBoss.IsDisposed && !_lichBoss.IsDead)
                    {
                        Rectangle hitboxLich = _lichBoss.GetHitboxAt(
                            _lichBossWorldPosition.X,
                            _lichBossWorldPosition.Y
                        );

                        if (hitboxBola.IntersectsWith(hitboxLich))
                        {
                            int danoFireball = CalcularDanoComRunaPoder(FireballDamage, 20);

                            _lichBoss.ReceberDano(danoFireball);
                            // Aplica queimadura se o Lich sobreviver.
                            if (!_lichBoss.IsDead)
                            {
                                _lichBoss.AplicarQueimadura(
                                    danoPorTick: 2,
                                    duracaoTicks: 240,
                                    intervaloTicks: 60
                                );
                            }
                            AtualizarHUDLichBoss();
                            iniciarExplosao = true;
                        }
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

        private void LoadMidBoss()
        {
            if (map == null)
                return;

            // Procura o marcador Spawn=mid_boss.
            List<MapTile> spawns = map.GetEnemySpawns("mid_boss");

            if (spawns == null || spawns.Count == 0)
            {
                throw new Exception(
                    "O marcador Spawn=mid_boss não foi encontrado no Mapa 2."
                );
            }

            // Utiliza o primeiro marcador encontrado.
            MapTile spawn = spawns[0];

            // Cria o Mid-Boss.
            MidBoss boss = new MidBoss();

            // Calcula o ajuste visual dos pés.
            Rectangle bossBounds = boss.GetVisibleSpriteBounds();

            _midBossVisualOffsetY = Math.Max(0,boss.Height - bossBounds.Bottom);

            // Calcula a posição no mundo.
            int worldX = spawn.x * map.tileSize + (map.tileSize - boss.Width) / 2;

            int worldY = spawn.y * map.tileSize - boss.Height;

            _midBossWorldPosition = new Point(worldX, worldY);

            _midBoss = boss;

            boss.DeathAnimationCompleted += MidBoss_DeathAnimationCompleted;

            boss.AttackImpact += MidBoss_AttackImpact;
            _midBossAttackCooldownRemaining = 0;

            // Posiciona considerando a câmera.
            boss.Location = new Point(
                worldX - _cameraX,
                worldY - _cameraY + _midBossVisualOffsetY
            );

            // Adiciona o chefe ao cenário.
            world1.Controls.Add(boss);

            boss.BringToFront();
        }


        private void CarregarChefeFinalDoPersonagem()
        {
            switch (_personagem)
            {
                case Personagem.Mago:
                    LoadLichBoss();
                    break;

                case Personagem.Cavaleiro:
                    LoadDullahanBoss();
                    break;

                case Personagem.Princesa:
                    LoadGothicQueenBoss();
                    break;

                default:
                    throw new InvalidOperationException(
                        "Personagem selecionado sem chefe final correspondente."
                    );
            }
        }

        private void LoadGothicQueenBoss()
        {
            if (map == null)
                return;

            // Procura o marcador Spawn=final_boss.
            List<MapTile> spawns = map.GetEnemySpawns("final_boss");

            if (spawns == null || spawns.Count == 0)
            {
                throw new Exception(
                    "O marcador Spawn=final_boss não foi encontrado na Arena dos Chefes Finais."
                );
            }

            MapTile spawn = spawns[0];

            // Cria a Rainha com 100 pontos de vida.
            GothicQueenBoss boss = new GothicQueenBoss(100);

            // Calcula o ajuste visual dos pés.
            Rectangle bossBounds = boss.GetVisibleSpriteBounds();

            _gothicQueenBossVisualOffsetY = Math.Max(
                0,
                boss.Height - bossBounds.Bottom
            );

            // Calcula a posição no mundo.
            int worldX = spawn.x * map.tileSize + (map.tileSize - boss.Width) / 2;

            int worldY = spawn.y * map.tileSize - boss.Height;

            // Define a direção inicial olhando para o jogador.
            bool facingRight =
                (_playerWorldX + player.Width / 2) >
                (worldX + boss.Width / 2);

            boss.SetIdle(facingRight);

            _gothicQueenBossWorldPosition = new Point(worldX, worldY);

            _gothicQueenBoss = boss;

            boss.AttackImpact += GothicQueenBoss_AttackImpact;
            boss.DeathAnimationCompleted += GothicQueenBoss_DeathAnimationCompleted;

            // Configura a HUD inicial da Rainha.
            PBX_Avatar_Final_Boss.Image =
                Properties.Resources.ResourceManager.GetObject(
                    "Fallen_Queen_Avatar_Normal"
                ) as Bitmap;

            AtualizarHUDGothicQueenBoss();

            PBX_Avatar_Final_Boss.Visible = true;
            PBX_HealtBar_Final_Boss.Visible = true;

            // Posiciona considerando a câmera.
            AtualizarPosicaoGothicQueenBossNaTela();

            // Adiciona a Rainha ao cenário.
            world1.Controls.Add(boss);

            boss.BringToFront();
        }

        private void AtualizarPosicaoGothicQueenBossNaTela()
        {
            if (_gothicQueenBoss == null ||
                _gothicQueenBoss.IsDisposed)
                return;

            _gothicQueenBoss.Location = new Point(
                _gothicQueenBossWorldPosition.X - _cameraX,
                _gothicQueenBossWorldPosition.Y - _cameraY
                    + _gothicQueenBossVisualOffsetY
            );
        }

        private void LoadDullahanBoss()
        {
            if (map == null)
                return;

            // Procura o marcador Spawn=final_boss.
            List<MapTile> spawns = map.GetEnemySpawns("final_boss");

            if (spawns == null || spawns.Count == 0)
            {
                throw new Exception(
                    "O marcador Spawn=final_boss não foi encontrado na Arena dos Chefes Finais."
                );
            }

            MapTile spawn = spawns[0];

            // Cria o Dullahan com 100 pontos de vida.
            DullahanBoss boss = new DullahanBoss(100);

            // Calcula o ajuste visual dos pés.
            Rectangle bossBounds = boss.GetVisibleSpriteBounds();

            _dullahanBossVisualOffsetY = Math.Max(
                0,
                boss.Height - bossBounds.Bottom
            );

            // Calcula a posição no mundo.
            int worldX = spawn.x * map.tileSize + (map.tileSize - boss.Width) / 2;

            int worldY = spawn.y * map.tileSize - boss.Height;

            // Define a direção inicial olhando para o jogador.
            bool facingRight = (_playerWorldX + player.Width / 2) > (worldX + boss.Width / 2);

            boss.SetIdle(facingRight);

            _dullahanBossWorldPosition = new Point(worldX, worldY);

            _dullahanBoss = boss;

            boss.AttackImpact += DullahanBoss_AttackImpact;
            boss.DeathAnimationCompleted += DullahanBoss_DeathAnimationCompleted;

            // Configura a HUD inicial do chefe.
            PBX_Avatar_Final_Boss.Image = Properties.Resources.Dullahan_Avatar_Normal;

            AtualizarHUDDullahanBoss();

            PBX_Avatar_Final_Boss.Visible = true;
            PBX_HealtBar_Final_Boss.Visible = true;

            // Posiciona considerando a câmera.
            AtualizarPosicaoDullahanBossNaTela();

            // Adiciona o chefe ao cenário.
            world1.Controls.Add(boss);

            boss.BringToFront();
        }

        private void AtualizarPosicaoDullahanBossNaTela()
        {
            if (_dullahanBoss == null || _dullahanBoss.IsDisposed)
                return;

            _dullahanBoss.Location = new Point(
                _dullahanBossWorldPosition.X - _cameraX,
                _dullahanBossWorldPosition.Y - _cameraY + _dullahanBossVisualOffsetY
            );
        }

        private void AtualizarHUDDullahanBoss()
        {
            if (_dullahanBoss == null || _dullahanBoss.IsDisposed)
                return;

            int vidaPercentual = _dullahanBoss.VidaAtual <= 0
                ? 0
                : (_dullahanBoss.VidaAtual * 100) / _dullahanBoss.VidaMaxima;

            // Seleciona a imagem correspondente, em intervalos de 5%.
            int percentualBarra = vidaPercentual == 0
                ? 0
                : Math.Min(100, ((vidaPercentual + 4) / 5) * 5);

            string nomeRecurso = "Dullahan_HealthBar_HP" + percentualBarra;

            PBX_HealtBar_Final_Boss.Image =
                Properties.Resources.ResourceManager.GetObject(nomeRecurso) as Bitmap;

            // Troca o avatar quando o Dullahan é derrotado.
            if (_dullahanBoss.VidaAtual <= 0)
            {
                PBX_Avatar_Final_Boss.Image =
                    Properties.Resources.Dullahan_Avatar_Death;
            }
        }

        private void AtualizarHUDGothicQueenBoss()
        {
            if (_gothicQueenBoss == null ||_gothicQueenBoss.IsDisposed)
                return;

            int vidaPercentual = _gothicQueenBoss.VidaAtual <= 0
                ? 0
                : (_gothicQueenBoss.VidaAtual * 100) /
                  _gothicQueenBoss.VidaMaxima;

            // Seleciona a barra em intervalos de 5%.
            int percentualBarra = vidaPercentual == 0
                ? 0
                : Math.Min(100, ((vidaPercentual + 4) / 5) * 5);

            string nomeRecurso = "Fallen_Queen_HealthBar_HP" + percentualBarra;

            PBX_HealtBar_Final_Boss.Image =
                Properties.Resources.ResourceManager.GetObject(
                    nomeRecurso
                ) as Bitmap;

            // Troca o avatar quando a Rainha é derrotada.
            if (_gothicQueenBoss.VidaAtual <= 0)
            {
                PBX_Avatar_Final_Boss.Image =
                    Properties.Resources.ResourceManager.GetObject(
                        "Fallen_Queen_Avatar_Death"
                    ) as Bitmap;
            }
        }

        private void DullahanBoss_DeathAnimationCompleted(DullahanBoss boss)
        {
            if (boss == null || boss.IsDisposed || boss != _dullahanBoss)
            {
                return;
            }

            // Interrompe a música da batalha.
            SoundEffects.StopWavLoop();

            _dullahanBossBattleMusicStarted = false;
        }

        private void GothicQueenBoss_DeathAnimationCompleted(GothicQueenBoss boss)
        {
            if (boss == null || boss.IsDisposed || boss != _gothicQueenBoss || _gothicQueenDefeated)
                return;

            // Interrompe a música da batalha.
            SoundEffects.StopWavLoop();

            _gothicQueenBattleMusicStarted = false;

            // Registra que a Rainha foi derrotada.
            _gothicQueenDefeated = true;

            // Remove ondas venenosas que ainda estiverem no cenário.
            foreach (QueenPoisonWave wave in _queenPoisonWaves.ToList())
            {
                if (wave == null)
                    continue;

                if (!wave.IsDisposed)
                {
                    world1.Controls.Remove(wave);
                    wave.Dispose();
                }
            }

            _queenPoisonWaves.Clear();
            AtualizarSomOndaVenenosaRainha();

            // Mantém a HUD no estado final.
            AtualizarHUDGothicQueenBoss();

            world1.Invalidate();
        }

        private void LoadLichBoss()
        {
            if (map == null)
                return;

            // Procura o marcador Spawn=final_boss.
            List<MapTile> spawns = map.GetEnemySpawns("final_boss");

            if (spawns == null || spawns.Count == 0)
            {
                throw new Exception(
                    "O marcador Spawn=final_boss não foi encontrado na Arena dos Chefes Finais."
                );
            }

            // Utiliza o primeiro marcador encontrado.
            MapTile spawn = spawns[0];

            // Cria o Lich com 100 pontos de vida iniciais.
            // Esse valor será ajustado posteriormente no balanceamento.
            LichBoss boss = new LichBoss(100);

            // Calcula o ajuste visual dos pés.
            Rectangle bossBounds = boss.GetVisibleSpriteBounds();

            _lichBossVisualOffsetY = Math.Max(0,boss.Height - bossBounds.Bottom);

            // Calcula a posição do chefe no mundo.
            int worldX = spawn.x * map.tileSize + (map.tileSize - boss.Width) / 2;

            int worldY = spawn.y * map.tileSize - boss.Height;

            bool facingRight = (_playerWorldX + player.Width / 2) > (worldX + boss.Width / 2);

            boss.SetIdle(facingRight);

            _lichBossWorldPosition = new Point(worldX, worldY);

            _lichBoss = boss;

            // Configura a HUD inicial do chefe.
            PBX_Avatar_Final_Boss.Image = Properties.Resources.LichBoss_Avatar_Normal;

            AtualizarHUDLichBoss();

            PBX_Avatar_Final_Boss.Visible = true;
            PBX_HealtBar_Final_Boss.Visible = true;

            boss.AttackImpact += LichBoss_AttackImpact;
            boss.DeathAnimationCompleted += LichBoss_DeathAnimationCompleted;

            // Posiciona considerando a câmera.
            AtualizarPosicaoLichBossNaTela();

            // Adiciona o chefe ao cenário.
            world1.Controls.Add(boss);

            boss.BringToFront();
        }

        private void AtualizarPosicaoLichBossNaTela()
        {
            if (_lichBoss == null || _lichBoss.IsDisposed)
                return;

            _lichBoss.Location = new Point(
                _lichBossWorldPosition.X - _cameraX,
                _lichBossWorldPosition.Y - _cameraY + _lichBossVisualOffsetY
            );
        }

        private void MidBoss_DeathAnimationCompleted(MidBoss boss)
        {
            if (boss == null || boss.IsDisposed || boss != _midBoss || _midBossDefeated)
            {
                return;
            }

            // Interrompe a música da batalha.
            SoundEffects.StopWavLoop();

            _midBossBattleMusicStarted = false;

            // Registra a derrota do chefe.
            _midBossDefeated = true;

            // Mantém o jogador à frente dos objetos.
            if (player != null && !player.IsDisposed)
            {
                player.BringToFront();
            }

            world1.Invalidate();
        }

        private void LichBoss_DeathAnimationCompleted(LichBoss boss)
        {
            if (boss == null ||  boss.IsDisposed || boss != _lichBoss)
            {
                return;
            }

            // Interrompe a música da batalha.
            SoundEffects.StopWavLoop();

            _lichBossBattleMusicStarted = false;
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


        private void AtualizarIAMidBoss()
        {
            if (_midBoss == null || _midBoss.IsDisposed || _midBoss.IsDead)
                return;

            if (player == null || player.IsDisposed || player.EstaMorto || map == null)
                return;

            if (_midBossAttackCooldownRemaining > 0)
                _midBossAttackCooldownRemaining--;

            // Obtém as hitboxes do jogador e do chefe.
            Rectangle hitboxJogador = player.GetHitboxAt(_playerWorldX,_playerWorldY);

            Rectangle hitboxBoss = _midBoss.GetHitboxAt(_midBossWorldPosition.X,_midBossWorldPosition.Y);

            // Calcula a distância horizontal entre os personagens.
            int centroJogadorX = hitboxJogador.Left + hitboxJogador.Width / 2;
            int centroBossX = hitboxBoss.Left + hitboxBoss.Width / 2;

            int diferencaX = centroJogadorX - centroBossX;
            int distanciaX = Math.Abs(diferencaX);

            bool facingRight = diferencaX >= 0;

            // Verifica se estão aproximadamente na mesma altura.
            bool mesmaAltura = Math.Abs(hitboxJogador.Bottom - hitboxBoss.Bottom) <= 28;

            // Não interrompe animações de ataque, dano ou morte.
            if (_midBoss.IsPerformingAction)
                return;

            // Fora do alcance de percepção, permanece parado.
            if (distanciaX > MidBossDetectionRange || !mesmaAltura)
            {
                _midBoss.SetIdle(facingRight);
                return;
            }

            // Inicia a música quando o Mid-Boss detecta o jogador.
            if (!_midBossBattleMusicStarted)
            {
                SoundEffects.StartMp3Loop(Properties.Resources.MidBoss_Battle,0.38f);

                _midBossBattleMusicStarted = true;
            }

            // Quando estiver próximo, tenta iniciar um ataque.
            if (distanciaX <= MidBossAttackRange)
            {
                if (_midBossAttackCooldownRemaining <= 0)
                {
                    if (_midBoss.SetRandomAttack(facingRight))
                    {
                        _midBossAttackCooldownRemaining = MidBossAttackCooldownTicks;
                    }
                }
                else
                {
                    _midBoss.SetIdle(facingRight);
                }

                return;
            }

            // Calcula o próximo movimento.
            int movimento = diferencaX > 0
                ? MidBossMoveSpeed
                : -MidBossMoveSpeed;

            int proximoX = _midBossWorldPosition.X + movimento;

            Rectangle hitboxProxima = _midBoss.GetHitboxAt(proximoX,_midBossWorldPosition.Y);

            // Verifica se existe chão à frente.
            Rectangle faixaDosPes = new Rectangle(
                hitboxProxima.Left + 2,
                hitboxProxima.Bottom,
                Math.Max(1, hitboxProxima.Width - 4),
                1
            );

            // Não permite atravessar paredes ou cair de plataformas.
            if (!map.PodeMover(hitboxProxima) || map.PodeMover(faixaDosPes))
            {
                _midBoss.SetIdle(facingRight);
                return;
            }

            // Atualiza a posição real do chefe.
            _midBossWorldPosition = new Point(
                proximoX,
                _midBossWorldPosition.Y
            );

            // Atualiza sua posição visível considerando a câmera.
            _midBoss.Location = new Point(
                _midBossWorldPosition.X - _cameraX,
                _midBossWorldPosition.Y - _cameraY + _midBossVisualOffsetY
            );

            _midBoss.SetWalk(facingRight);
        }

        private void AtualizarIALichBoss()
        {
            if (_lichBoss == null || _lichBoss.IsDisposed || _lichBoss.IsDead)
                return;

            if (player == null || player.IsDisposed || player.EstaMorto || map == null)
                return;

            // Atualiza o tempo de recarga do ataque.
            if (_lichBossAttackCooldownRemaining > 0)
                _lichBossAttackCooldownRemaining--;

            // Obtém as hitboxes do jogador e do Lich.
            Rectangle hitboxJogador = player.GetHitboxAt(
                _playerWorldX,
                _playerWorldY
            );

            Rectangle hitboxLich = _lichBoss.GetHitboxAt(
                _lichBossWorldPosition.X,
                _lichBossWorldPosition.Y
            );

            // Calcula a distância horizontal entre os personagens.
            int centroJogadorX = hitboxJogador.Left + hitboxJogador.Width / 2;
            int centroLichX = hitboxLich.Left + hitboxLich.Width / 2;

            int diferencaX = centroJogadorX - centroLichX;
            int distanciaX = Math.Abs(diferencaX);

            // Define a direção para a qual o Lich deve olhar.
            bool facingRight = diferencaX >= 0;

            // Verifica se estão aproximadamente na mesma altura.
            bool mesmaAltura = Math.Abs(hitboxJogador.Bottom - hitboxLich.Bottom) <= 28;

            // Não interrompe ataques, animação de dano ou morte.
            if (_lichBoss.IsPerformingAction)
                return;

            // Fora do alcance ou em alturas diferentes, permanece parado.
            if (distanciaX > LichDetectionRange || !mesmaAltura)
            {
                _lichBoss.SetIdle(facingRight);
                return;
            }

            // Inicia a música exclusiva quando o Lich detecta o jogador.
            if (!_lichBossBattleMusicStarted)
            {
                SoundEffects.StopWavLoop();

                SoundEffects.StartMp3Loop(Properties.Resources.Lich_Battle_Music,0.38f);

                _lichBossBattleMusicStarted = true;
            }

            // Utiliza o ataque à distância quando estiver
            // fora do alcance do Dark Flame Burst e sem recarga.
            if (distanciaX > LichMeleeAttackRange &&
                distanciaX <= LichFireBallAttackRange &&
                _lichBossAttackCooldownRemaining <= 0)
            {
                if (_lichBoss.SetAttack(LichBoss.TipoAtaque.DarkFireBall,facingRight))
                {
                    SoundEffects.Play(Properties.Resources.Lich_DarkFireball_Cast,0.85f);

                    _lichBossAttackCooldownRemaining = LichAttackCooldownTicks;
                    return;
                }
            }

            // Quando estiver próximo, tenta executar o ataque corpo a corpo.
            if (distanciaX <= LichMeleeAttackRange)
            {
                if (_lichBossAttackCooldownRemaining <= 0)
                {
                    if (_lichBoss.SetAttack(LichBoss.TipoAtaque.DarkFlameBurst,facingRight))
                    {
                        SoundEffects.Play(Properties.Resources.Lich_DarkFlameBurst_Cast,0.85f);

                        _lichBossAttackCooldownRemaining = LichAttackCooldownTicks;
                    }
                }
                else
                {
                    _lichBoss.SetIdle(facingRight);
                }

                return;
            }

            // Calcula o próximo movimento.
            int movimento = diferencaX > 0 ? LichMoveSpeed : -LichMoveSpeed;

            int proximoX = _lichBossWorldPosition.X + movimento;

            Rectangle hitboxProxima = _lichBoss.GetHitboxAt(proximoX,_lichBossWorldPosition.Y);

            // Verifica se existe chão à frente.
            Rectangle faixaDosPes = new Rectangle(
                hitboxProxima.Left + 2,
                hitboxProxima.Bottom,
                Math.Max(1, hitboxProxima.Width - 4),
                1
            );

            // Não permite atravessar paredes ou cair de plataformas.
            if (!map.PodeMover(hitboxProxima) || map.PodeMover(faixaDosPes))
            {
                _lichBoss.SetIdle(facingRight);
                return;
            }

            // Atualiza a posição real do Lich.
            _lichBossWorldPosition = new Point(proximoX,_lichBossWorldPosition.Y);

            // Atualiza a posição visual considerando a câmera.
            AtualizarPosicaoLichBossNaTela();

            // Executa a animação de caminhada.
            _lichBoss.SetWalk(facingRight);
        }

        private void AtualizarIADullahanBoss()
        {
            if (_dullahanBoss == null || _dullahanBoss.IsDisposed || _dullahanBoss.IsDead)
                return;

            if (player == null || player.IsDisposed || player.EstaMorto || map == null)
                return;

            // Atualiza a recarga dos ataques.
            if (_dullahanBossAttackCooldownRemaining > 0)
                _dullahanBossAttackCooldownRemaining--;

            Rectangle hitboxJogador = player.GetHitboxAt(_playerWorldX,_playerWorldY);

            Rectangle hitboxDullahan = _dullahanBoss.GetHitboxAt(
                _dullahanBossWorldPosition.X,
                _dullahanBossWorldPosition.Y
            );

            int centroJogadorX = hitboxJogador.Left + hitboxJogador.Width / 2;
            int centroDullahanX = hitboxDullahan.Left + hitboxDullahan.Width / 2;

            int diferencaX = centroJogadorX - centroDullahanX;
            int distanciaX = Math.Abs(diferencaX);

            bool facingRight = diferencaX >= 0;

            // Verifica se estão aproximadamente na mesma altura.
            bool mesmaAltura =
                Math.Abs(hitboxJogador.Bottom - hitboxDullahan.Bottom) <= 28;

            // Não interrompe ataques, dano ou morte.
            if (_dullahanBoss.IsPerformingAction)
                return;

            // Fora do alcance de percepção.
            if (distanciaX > DullahanDetectionRange || !mesmaAltura)
            {
                _dullahanBoss.SetIdle(facingRight);
                return;
            }

            // Inicia a música exclusiva quando o Dullahan detecta o jogador.
            if (!_dullahanBossBattleMusicStarted)
            {
                SoundEffects.StopWavLoop();

                SoundEffects.StartMp3Loop(Properties.Resources.Dullahan_Battle_Music,0.38f);

                _dullahanBossBattleMusicStarted = true;
            }

            // Escolhe o próximo ataque.
            DullahanBoss.TipoAtaque proximoAtaque =
                _dullahanNextAttackHeavy
                    ? DullahanBoss.TipoAtaque.Attack2
                    : DullahanBoss.TipoAtaque.Attack1;

            int alcanceAtaque = _dullahanNextAttackHeavy
                ? DullahanAttack2Range
                : DullahanAttack1Range;

            // Verifica se já pode atacar.
            if (distanciaX <= alcanceAtaque)
            {
                if (_dullahanBossAttackCooldownRemaining <= 0)
                {
                    if (_dullahanBoss.SetAttack(proximoAtaque, facingRight))
                    {
                        _dullahanBossAttackCooldownRemaining =
                            _dullahanNextAttackHeavy
                                ? DullahanAttack2CooldownTicks
                                : DullahanAttack1CooldownTicks;

                        // Alterna para o próximo ataque.
                        _dullahanNextAttackHeavy = !_dullahanNextAttackHeavy;
                    }
                }
                else
                {
                    _dullahanBoss.SetIdle(facingRight);
                }

                return;
            }

            // Aproxima o Dullahan do jogador.
            int movimento = diferencaX > 0
                ? DullahanMoveSpeed 
                : -DullahanMoveSpeed;

            int proximoX = _dullahanBossWorldPosition.X + movimento;

            Rectangle hitboxProxima = _dullahanBoss.GetHitboxAt(
                proximoX,
                _dullahanBossWorldPosition.Y
            );

            // Verifica se existe chão à frente.
            Rectangle faixaDosPes = new Rectangle(
                hitboxProxima.Left + 2,
                hitboxProxima.Bottom,
                Math.Max(1, hitboxProxima.Width - 4),
                1
            );

            // Não atravessa paredes nem cai de plataformas.
            if (!map.PodeMover(hitboxProxima) || map.PodeMover(faixaDosPes))
            {
                _dullahanBoss.SetIdle(facingRight);
                return;
            }

            // Atualiza a posição real.
            _dullahanBossWorldPosition = new Point(
                proximoX,
                _dullahanBossWorldPosition.Y
            );

            // Atualiza a posição visual.
            AtualizarPosicaoDullahanBossNaTela();

            // Executa a animação de caminhada.
            _dullahanBoss.SetWalk(facingRight);
        }

        private void AtualizarIAGothicQueenBoss()
        {
            if (_gothicQueenBoss == null || _gothicQueenBoss.IsDisposed || _gothicQueenBoss.IsDead)
                return;

            if (player == null || player.IsDisposed || player.EstaMorto || map == null)
                return;

            // Atualiza a recarga do ataque.
            if (_gothicQueenAttackCooldownRemaining > 0)
                _gothicQueenAttackCooldownRemaining--;

            Rectangle hitboxJogador = player.GetHitboxAt(
                _playerWorldX,
                _playerWorldY
            );

            Rectangle hitboxRainha = _gothicQueenBoss.GetHitboxAt(
                _gothicQueenBossWorldPosition.X,
                _gothicQueenBossWorldPosition.Y
            );

            int centroJogadorX = hitboxJogador.Left + hitboxJogador.Width / 2;

            int centroRainhaX = hitboxRainha.Left + hitboxRainha.Width / 2;

            int diferencaX = centroJogadorX - centroRainhaX;

            int distanciaX = Math.Abs(diferencaX);

            bool facingRight = diferencaX >= 0;

            // Verifica se estão aproximadamente na mesma altura.
            bool mesmaAltura = Math.Abs(hitboxJogador.Bottom - hitboxRainha.Bottom) <= 28;

            // Não interrompe ataques, dano ou morte.
            if (_gothicQueenBoss.IsPerformingAction)
                return;

            // Fora do alcance de percepção.
            if (distanciaX > GothicQueenDetectionRange || !mesmaAltura)
            {
                _gothicQueenBoss.SetIdle(facingRight);
                return;
            }

            // Inicia a música exclusiva quando a Rainha detecta a Princesa.
            if (!_gothicQueenBattleMusicStarted)
            {
                SoundEffects.StopWavLoop();

                SoundEffects.StartMp3Loop(Properties.Resources.Fallen_Queen_Battle_Music,0.38f);

                _gothicQueenBattleMusicStarted = true;
            }

            // Ataca quando a Princesa está próxima.
            if (distanciaX <= RoyalVenomSlashRange)
            {
                if (_gothicQueenAttackCooldownRemaining <= 0)
                {
                    _gothicQueenAttackCount++;

                    GothicQueenBoss.TipoAtaque ataqueEscolhido =
                        (_gothicQueenAttackCount % 3 == 0)
                        ? GothicQueenBoss.TipoAtaque.EmeraldBladefall
                        : GothicQueenBoss.TipoAtaque.RoyalVenomSlash;

                    if (_gothicQueenBoss.SetAttack(ataqueEscolhido, facingRight))
                    {
                        // O som das espadas começa junto da animação Emerald Bladefall.
                        if (ataqueEscolhido == GothicQueenBoss.TipoAtaque.EmeraldBladefall)
                        {
                            SoundEffects.Play(
                                Properties.Resources.Fallen_Queen_EmeraldBladefall_Swords,
                                0.90f
                            );
                        }

                        _gothicQueenAttackCooldownRemaining =
                            ataqueEscolhido == GothicQueenBoss.TipoAtaque.EmeraldBladefall
                            ? 180
                            : RoyalVenomSlashCooldownTicks;
                    }
                }
                else
                {
                    _gothicQueenBoss.SetIdle(facingRight);
                }

                return;
            }

            // Aproxima a Rainha da Princesa.
            int movimento = diferencaX > 0
                ? GothicQueenMoveSpeed
                : -GothicQueenMoveSpeed;

            int proximoX =
                _gothicQueenBossWorldPosition.X + movimento;

            Rectangle hitboxProxima = _gothicQueenBoss.GetHitboxAt(
                proximoX,
                _gothicQueenBossWorldPosition.Y
            );

            // Verifica se existe chão à frente.
            Rectangle faixaDosPes = new Rectangle(
                hitboxProxima.Left + 2,
                hitboxProxima.Bottom,
                Math.Max(1, hitboxProxima.Width - 4),
                1
            );

            // Não atravessa paredes nem cai de plataformas.
            if (!map.PodeMover(hitboxProxima) ||
                map.PodeMover(faixaDosPes))
            {
                _gothicQueenBoss.SetIdle(facingRight);
                return;
            }

            // Atualiza a posição real da Rainha.
            _gothicQueenBossWorldPosition = new Point(
                proximoX,
                _gothicQueenBossWorldPosition.Y
            );

            // Atualiza a posição visual.
            AtualizarPosicaoGothicQueenBossNaTela();

            _gothicQueenBoss.SetWalk(facingRight);
        }

        private void AtualizarOrdemDesenhoRainha()
        {
            if (player == null ||
                player.IsDisposed ||
                _gothicQueenBoss == null ||
                _gothicQueenBoss.IsDisposed)
                return;

            Rectangle hitboxJogador = player.GetHitboxAt(
                _playerWorldX,
                _playerWorldY
            );

            Rectangle hitboxRainha = _gothicQueenBoss.GetHitboxAt(
                _gothicQueenBossWorldPosition.X,
                _gothicQueenBossWorldPosition.Y
            );

            // Quem estiver mais abaixo no cenário aparece na frente.
            if (hitboxJogador.Bottom >= hitboxRainha.Bottom)
            {
                player.BringToFront();
            }
            else
            {
                _gothicQueenBoss.BringToFront();
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
            // Atualiza o veneno do Mid-Boss.
            if (_midBoss != null && !_midBoss.IsDisposed)
            {
                _midBoss.AtualizarVeneno();
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
            // Atualiza a queimadura do Mid-Boss.
            if (_midBoss != null && !_midBoss.IsDisposed)
            {
                _midBoss.AtualizarQueimadura();
            }
            // Atualiza a queimadura do Lich.
            if (_lichBoss != null && !_lichBoss.IsDisposed)
            {
                int vidaAnterior = _lichBoss.VidaAtual;

                _lichBoss.AtualizarQueimadura();

                // Atualiza a HUD somente quando houver perda de vida.
                if (_lichBoss.VidaAtual != vidaAnterior)
                {
                    AtualizarHUDLichBoss();
                }
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
                    if (_manaRecoveryTickCounter >= ObterIntervaloRecuperacaoMana())
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
                                _knightAuraCooldownRemaining = ObterCooldownAura();
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

                    // Habilidade 1: Strong Sword Attack.
                    if (Core.isAttacking0)
                    {
                        Core.isAttacking0 = false;
                        player.SetAttack();
                    }

                    // Habilidade 2: Poison Attack.
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
            int velocidadeMovimento = ObterVelocidadeMovimento();
            if (!player.IsPerformingAction || player.EstaRecebendoDano)
            {
                if (Core.isMovingLeft && !Core.isMovingRight)
                {
                    if (TentarMoverHorizontal(-velocidadeMovimento))
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
                    if (TentarMoverHorizontal(velocidadeMovimento))
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
            // Atualiza a inteligência artificial do Mid-Boss.
            AtualizarIAMidBoss();
            // Atualiza a inteligência artificial do Lich.
            AtualizarIALichBoss();
            // Atualiza a inteligência artificial do Dullahan.
            AtualizarIADullahanBoss();
            // Atualiza a inteligência artificial da Gothic Queen.
            AtualizarIAGothicQueenBoss();
            // Atualiza a ordem de desenho entre a Princesa e a Gothic Queen.
            AtualizarOrdemDesenhoRainha();
            // Atualiza a onda venenosa da Rainha e verifica colisões com o jogador.
            AtualizarOndasVenenosasRainha();
            // Atualiza a posição das dark fireballs e verifica colisões.
            AtualizarDarkFireballs();
            // Atualiza o dano contínuo do veneno.
            AtualizarVenenoInimigos();
            // Atualiza o dano contínuo da queimadura.
            AtualizarQueimaduraInimigos();
            // Atualiza a queimadura causada pelo Lich no jogador.
            AtualizarQueimaduraJogador();
            // Atualiza a duração do bloqueio de habilidade.
            AtualizarBloqueioHabilidadeJogador();
            // Atualiza a posição das bolas de fogo e verifica colisões.
            AtualizarBolasDeFogo();
            // Atualiza a posição da runa mais próxima do jogador.
            AtualizarPuzzleRunes();
            // Verifica se o jogador está próximo do baú e se ele pode ser aberto.
            VerificarRecompensaBau();
            // Atualiza a contagem regressiva da Forja do Julgamento.
            AtualizarContagemJulgamento();
            // Atualiza a runa do Guardião mais próxima do jogador.
            AtualizarGuardianRunes();
            // Atualiza a barra de vida e mana do jogador.
            VerificarDanoContatoInimigos();
            // Atualiza a barra de vida do jogador.
            AtualizarInteracaoStage();
        }
        #endregion

        #region Collision detection between player and enemies

        private bool BloqueiaMovimentoPorInimigo(Rectangle hitboxAtual,Rectangle hitboxProxima)
        {
            // Verifica a colisão com os Orcs.
            foreach (KeyValuePair<Enemy, Point> item in _enemyWorldPositions)
            {
                Enemy enemy = item.Key;

                if (enemy == null || enemy.IsDisposed || enemy.IsDead)
                    continue;

                Point posicao = item.Value;

                Rectangle hitboxInimigo = enemy.GetHitboxAt(posicao.X,posicao.Y);

                if (!hitboxProxima.IntersectsWith(hitboxInimigo))
                    continue;

                if (!hitboxAtual.IntersectsWith(hitboxInimigo))
                    return true;

                int centroAtual = hitboxAtual.Left + hitboxAtual.Width / 2;
                int centroProximo = hitboxProxima.Left + hitboxProxima.Width / 2;
                int centroInimigo = hitboxInimigo.Left + hitboxInimigo.Width / 2;

                int distanciaAtual = Math.Abs(centroAtual - centroInimigo);
                int distanciaProxima = Math.Abs(centroProximo - centroInimigo);

                if (distanciaProxima <= distanciaAtual)
                    return true;
            }

            // Verifica a colisão com o Mid-Boss.
            if (_midBoss != null && !_midBoss.IsDisposed && !_midBoss.IsDead)
            {
                Rectangle hitboxBoss = _midBoss.GetHitboxAt(_midBossWorldPosition.X,_midBossWorldPosition.Y);

                if (hitboxProxima.IntersectsWith(hitboxBoss))
                {
                    if (!hitboxAtual.IntersectsWith(hitboxBoss))
                        return true;

                    int centroAtual = hitboxAtual.Left + hitboxAtual.Width / 2;
                    int centroProximo = hitboxProxima.Left + hitboxProxima.Width / 2;
                    int centroBoss = hitboxBoss.Left + hitboxBoss.Width / 2;

                    int distanciaAtual = Math.Abs(centroAtual - centroBoss);
                    int distanciaProxima = Math.Abs(centroProximo - centroBoss);

                    // Permite se afastar, mas não avançar para dentro do chefe.
                    if (distanciaProxima <= distanciaAtual)
                        return true;
                }
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

        #region Stage Transition
        private void CriarPromptStage()
        {
            _stagePrompt = new Label();

            _stagePrompt.Text = "Pressione E para avançar";
            _stagePrompt.AutoSize = true;
            _stagePrompt.BackColor = Color.FromArgb(25, 32, 29);
            _stagePrompt.ForeColor = Color.White;
            _stagePrompt.Font = new Font("Arial", 10, FontStyle.Bold);
            _stagePrompt.Padding = new Padding(10, 7, 10, 7);
            _stagePrompt.BorderStyle = BorderStyle.FixedSingle;
            _stagePrompt.Visible = false;

            _stagePrompt.Location = new Point(
                (world1.ClientSize.Width - _stagePrompt.Width) / 2,
                world1.ClientSize.Height - _stagePrompt.Height - 24
            );

            world1.Controls.Add(_stagePrompt);
            _stagePrompt.BringToFront();
        }
        private void AtualizarInteracaoStage()
        {
            if (player == null || map == null ||
                _nextStage == null || player.EstaMorto)
            {
                _nearNextStage = false;

                if (_stagePrompt != null)
                    _stagePrompt.Visible = false;

                return;
            }

            // Área de interação ao redor do marcador.
            Rectangle areaSaida = new Rectangle(
                _nextStage.x * map.tileSize,
                _nextStage.y * map.tileSize,
                map.tileSize,
                map.tileSize
            );

            // Aumenta a área para permitir aproximação.
            areaSaida.Inflate(map.tileSize * 2, map.tileSize);

            // Hitbox real do jogador no mundo.
            Rectangle hitboxPlayer = player.GetHitboxAt(
                _playerWorldX,
                _playerWorldY
            );

            _nearNextStage = areaSaida.IntersectsWith(hitboxPlayer);

            _stagePrompt.Visible = _nearNextStage;

            if (_nearNextStage)
                _stagePrompt.BringToFront();
        }

        private void AvancarStage()
        {
            if (!_nearNextStage || _transicaoEmAndamento)
                return;

            // Impede sair da Forja sem concluir o Julgamento.
            if (_judgmentForgeCarregado && !_finalBossArenaCarregada && !_puzzleCompleto)
            {
                // Reproduz o aviso de Vharzun somente uma vez.
                if (!_vharzunAvisoSaidaReproduzido)
                {
                    _vharzunAvisoSaidaReproduzido = true;


                    SoundEffects.PlayVoice(
                        Properties.Resources.Vharzun_Forge_ExitWarning,
                        0.9f
                    );
                }

                return;
            }

            // Impede sair da arena enquanto o Mid-Boss estiver vivo.
            if (_mapa2Carregado && !_midBossDefeated)
            {
                MessageBox.Show(
                    "Você precisa derrotar o Mid-Boss antes de sair da arena.",
                    "Arena do Mid-Boss",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            // Impede sair das Ruínas sem concluir o puzzle.
            if (_ruinasGuardiaoCarregadas && !_mapa2Carregado  && !_guardianPuzzleCompleto)
            {
                MessageBox.Show(
                    "Você precisa resolver o enigma das três runas antes de continuar.",
                    "Ruínas do Guardião",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            // Impede sair do Santuário sem ativar as três runas.
            if (_santuarioCarregado && !_mapa1Carregado && !_puzzleCompleto)
            {
                // Reproduz o aviso de Sylvara somente uma vez.
                if (!_sylvaraAvisoSaidaReproduzido)
                {
                    _sylvaraAvisoSaidaReproduzido = true;


                    SoundEffects.PlayVoice(
                        Properties.Resources.Sylvara_RootsSanctuary_ExitWarning,
                        0.9f
                    );
                }

                return;
            }

            // Primeira transição: Tutorial -> Santuário.
            if (!_santuarioCarregado)
            {
                if (CarregarSantuarioDasRaizes())
                    _santuarioCarregado = true;

                return;
            }

            // Segunda transição: Santuário -> Mapa 1.
            if (!_mapa1Carregado)
            {
                if (CarregarMapa1())
                    _mapa1Carregado = true;

                return;
            }

            // Terceira transição: Mapa 1 -> Ruínas do Guardião.
            if (!_ruinasGuardiaoCarregadas)
            {
                if (CarregarRuinasGuardiao())
                    _ruinasGuardiaoCarregadas = true;

                return;
            }

            // Quarta transição: Ruínas do Guardião -> Mapa 2.
            if (!_mapa2Carregado)
            {
                if (CarregarMapa2())
                    _mapa2Carregado = true;

                return;
            }

            // Quinta transição: Mapa 2 -> Forja do Julgamento.
            if (!_judgmentForgeCarregado)
            {
                if (CarregarForjaDoJulgamento())
                    _judgmentForgeCarregado = true;

                return;
            }

            // Sexta transição: Forja do Julgamento -> Arena dos Chefes Finais.
            if (!_finalBossArenaCarregada)
            {
                if (CarregarArenaChefesFinais())
                    _finalBossArenaCarregada = true;

                return;
            }

            MessageBox.Show(
                "O próximo mapa ainda não está configurado.",
                "Fallen World",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        #endregion

        #region Puzzle Runes
        private void AtualizarRunaProxima()
        {
            _runaProxima = null;

            if (player == null || player.IsDisposed)
                return;

            // Centro do jogador no mundo.
            double jogadorCentroX = _playerWorldX + player.Width / 2.0;
            double jogadorCentroY = _playerWorldY + player.Height / 2.0;

            double menorDistancia = double.MaxValue;

            foreach (PuzzleRune rune in _puzzleRunes)
            {
                if (rune == null || rune.IsDisposed)
                    continue;

                // Não considerar runas que já foram ativadas
                // ou que estão executando sua animação.
                if (rune.IsActivated || rune.IsActivating)
                    continue;

                Point posicaoRuna;

                if (!_puzzleRuneWorldPositions.TryGetValue(rune, out posicaoRuna))
                    continue;

                // Centro da runa no mundo.
                double runaCentroX = posicaoRuna.X + rune.Width / 2.0;
                double runaCentroY = posicaoRuna.Y + rune.Height / 2.0;

                double diferencaX = jogadorCentroX - runaCentroX;
                double diferencaY = jogadorCentroY - runaCentroY;

                // Distância entre os centros, ao quadrado.
                double distancia =
                    diferencaX * diferencaX +
                    diferencaY * diferencaY;

                // Verifica se está dentro do alcance de interação.
                if (distancia <= DistanciaInteracaoRuna * DistanciaInteracaoRuna && distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    _runaProxima = rune;
                }
            }
        }
        private void AtualizarPuzzleRunes()
        {
            foreach (PuzzleRune rune in _puzzleRunes)
            {
                if (rune == null || rune.IsDisposed)
                    continue;

                rune.UpdateAnimation(TMR_Game.Interval);
            }

            if (_puzzleChest != null && !_puzzleChest.IsDisposed)
            {
                _puzzleChest.UpdateAnimation(TMR_Game.Interval);
            }
            AtualizarProximidadePuzzleChest();
            AtualizarRunaProxima();
        }
        private async void OnRuneActivationCompleted(PuzzleRune rune)
        {
            if (_puzzleCompleto)
                return;

            if (_judgmentForgeCarregado && (_judgmentForgeContagemFalhou || !_judgmentForgeContagemAtiva))
            {
                return;
            }

            if (_puzzleRunes.Count != 3)
                return;

            foreach (PuzzleRune runa in _puzzleRunes)
            {
                if (runa == null || !runa.IsActivated)
                    return;
            }

            // Tratamento exclusivo da conclusão da Forja.
            if (_judgmentForgeCarregado)
            {
                _puzzleCompleto = true;

                // Interrompe definitivamente a contagem regressiva.
                _judgmentForgeContagemAtiva = false;
                _judgmentForgeTodasRunasIniciadas = true;

                // Interrompe a música da Forja.
                SoundEffects.StopWavLoop();

                // Atualiza o indicador visual.
                if (_judgmentForgeTempoLabel != null && !_judgmentForgeTempoLabel.IsDisposed)
                {
                    _judgmentForgeTempoLabel.Text = "JULGAMENTO CONCLUÍDO!";
                    _judgmentForgeTempoLabel.ForeColor = Color.Gold;
                }

                SoundEffects.PlayWav(Properties.Resources.RootMechanism_Open, 0.8f);

                await Task.Delay(2800);

                if (IsDisposed || Disposing)
                    return;

                // Vharzun reconhece a conclusão do julgamento.
                SoundEffects.PlayVoice(
                    Properties.Resources.Vharzun_Forge_Success,
                    0.9f
                );

                // Revela o baú após a abertura do mecanismo.
                if (_puzzleChest != null && !_puzzleChest.IsDisposed)
                {
                    _puzzleChest.Visible = true;

                    if (player != null && !player.IsDisposed)
                        player.BringToFront();
                }

                return;
            }

            // As três runas foram ativadas.
            _puzzleCompleto = true;

            // Reproduz o som de liberação do mecanismo ancestral.
            SoundEffects.PlayWav(Properties.Resources.RootMechanism_Open,0.8f);

            // Aguarda o mecanismo terminar de abrir.
            await Task.Delay(2800);

            // Evita acessar controles caso o formulário tenha sido fechado.
            if (IsDisposed || Disposing)
                return;

            // Sylvara celebra a conclusão do puzzle do Santuário.
            SoundEffects.PlayVoice(
                Properties.Resources.Sylvara_RootsSanctuary_Success,
                0.9f
            );

            // Revela o baú após o som.
            if (_puzzleChest != null && !_puzzleChest.IsDisposed)
            {
                _puzzleChest.Visible = true;

                if (player != null && !player.IsDisposed)
                    player.BringToFront();
            }
        }
        #endregion

        #region Guardian Rune Puzzle

        // Armazena as três runas das Ruínas do Guardião.
        private readonly List<GuardianRune> _guardianRunes = new List<GuardianRune>();

        // Armazena as posições das runas no mundo.
        private readonly Dictionary<GuardianRune, Point> _guardianRuneWorldPositions = new Dictionary<GuardianRune, Point>();

        // Distância necessária para interagir com uma runa.
        private const int DistanciaInteracaoGuardianRune = 80;

        // Runa mais próxima do jogador.
        private GuardianRune _guardianRuneProxima;

        // Sequência aleatória que o jogador deverá seguir.
        private readonly List<int> _guardianRuneSequence = new List<int>();

        // Posição atual do jogador na sequência.
        private int _guardianSequenceIndex = 0;

        // Controle de conclusão do puzzle.
        private bool _guardianPuzzleCompleto = false;

        // Controle da animação de erro.
        private bool _guardianRuneErrorInProgress = false;
        private static readonly Random _guardianRandom = new Random();
        private int _guardianSuccessAnimationsCompleted = 0;

        private void TentarAtivarGuardianRune(GuardianRune rune)
        {
            if (rune == null || _guardianPuzzleCompleto ||
                _guardianRuneErrorInProgress)
                return;

            if (_guardianSequenceIndex >= _guardianRuneSequence.Count)
                return;

            if (rune.State != GuardianRune.RuneState.Inactive)
                return;

            int idEsperado = _guardianRuneSequence[_guardianSequenceIndex];

            if (rune.RuneId == idEsperado)
            {
                // A runa está na posição correta da sequência
                SoundEffects.PlayWav(Properties.Resources.GuardianRune_Activate, 0.7f);
                // Sequência correta: inicia a ativação.
                rune.StartActivation();
            }
            else
            {
                // Sequência incorreta: inicia o efeito vermelho.
                _guardianRuneErrorInProgress = true;
                _guardianRuneProxima = null;

                // Reproduz o som de erro.
                SoundEffects.PlayWav(Properties.Resources.GuardianRune_Error,0.7f);
                rune.StartError();
            }
        }

        private void OnGuardianRuneActivationCompleted(GuardianRune rune)
        {
            if (_guardianPuzzleCompleto || _guardianRuneErrorInProgress)
                return;

            _guardianSequenceIndex++;

            _guardianRuneProxima = null;

            // Ainda faltam runas na sequência.
            if (_guardianSequenceIndex < _guardianRuneSequence.Count)
                return;

            // As três runas foram ativadas corretamente.
            _guardianPuzzleCompleto = true;

            _guardianSuccessAnimationsCompleted = 0;

            // Reproduz o som de conclusão do puzzle.
            SoundEffects.PlayWav(Properties.Resources.GuardianPuzzle_Complete,0.8f);

            // Inicia o efeito de sucesso nas três simultaneamente.
            foreach (GuardianRune guardianRune in _guardianRunes)
            {
                guardianRune.StartSuccess();
            }
        }

        private void OnGuardianRuneErrorAnimationCompleted(GuardianRune rune)
        {
            // Reinicia visualmente as três runas.
            foreach (GuardianRune guardianRune in _guardianRunes)
            {
                guardianRune.ResetToInactive();
            }

            // Reinicia o progresso, mas preserva a sequência sorteada.
            _guardianSequenceIndex = 0;
            _guardianRuneErrorInProgress = false;
            _guardianRuneProxima = null;
        }

        private async void OnGuardianRuneSuccessAnimationCompleted(GuardianRune rune)
        {
            if (!_guardianPuzzleCompleto)
                return;

            _guardianSuccessAnimationsCompleted++;

            // Aguarda as três animações terminarem.
            if (_guardianSuccessAnimationsCompleted < 3)
                return;

            // Reproduz o som do mecanismo antigo.
            SoundEffects.PlayWav(Properties.Resources.GuardianMechanism_Open,0.8f);

            // Aguarda o início do efeito sonoro antes de revelar o baú.
            await Task.Delay(1200);

            if (IsDisposed || Disposing)
                return;

            // Revela o baú.
            if (_puzzleChest != null && !_puzzleChest.IsDisposed)
            {
                _puzzleChest.Visible = true;

                if (player != null && !player.IsDisposed)
                    player.BringToFront();
            }

            world1.Invalidate();
        }

        private void AtualizarGuardianRunes()
        {
            foreach (GuardianRune rune in _guardianRunes)
            {
                if (rune == null || rune.IsDisposed)
                    continue;

                rune.UpdateAnimation(TMR_Game.Interval);
            }

            AtualizarGuardianRuneProxima();
        }

        private void AtualizarGuardianRuneProxima()
        {
            _guardianRuneProxima = null;

            if (player == null || player.IsDisposed ||
                _guardianPuzzleCompleto || _guardianRuneErrorInProgress)
                return;

            // Impede outra interação durante uma animação.
            if (_guardianRunes.Any(r => r != null && r.IsAnimating))
                return;

            double jogadorCentroX = _playerWorldX + player.Width / 2.0;
            double jogadorCentroY = _playerWorldY + player.Height / 2.0;

            double menorDistancia = double.MaxValue;

            foreach (GuardianRune rune in _guardianRunes)
            {
                if (rune == null || rune.IsDisposed)
                    continue;

                if (rune.State != GuardianRune.RuneState.Inactive)
                    continue;

                Point posicao;

                if (!_guardianRuneWorldPositions.TryGetValue(rune, out posicao))
                    continue;

                double runaCentroX = posicao.X + rune.Width / 2.0;
                double runaCentroY = posicao.Y + rune.Height / 2.0;

                double dx = jogadorCentroX - runaCentroX;
                double dy = jogadorCentroY - runaCentroY;

                double distancia = dx * dx + dy * dy;

                if (distancia <= DistanciaInteracaoGuardianRune * DistanciaInteracaoGuardianRune && distancia < menorDistancia)
                {
                    menorDistancia = distancia;
                    _guardianRuneProxima = rune;
                }
            }
        }
        #endregion

        #region Puzzle Chest
        private void LoadPuzzleChest(bool visivelInicialmente,bool entregarRecompensa = true)
        {
            _recompensaBauEntregue = !entregarRecompensa;

            if (map == null)
                return;

            LimparPuzzleChest();

            List<MapTile> marcadores = map.GetTreasureMarkers();

            foreach (MapTile marcador in marcadores)
            {
                if (marcador.attributes == null)
                    continue;

                string tipoBaú = null;

                foreach (KeyValuePair<string, string> atributo
                    in marcador.attributes)
                {
                    if (string.Equals(atributo.Key,"Treasure",StringComparison.OrdinalIgnoreCase))
                    {
                        tipoBaú = atributo.Value;
                        break;
                    }
                }

                if (!string.Equals(tipoBaú,"chest",StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                PuzzleChest chest = new PuzzleChest(Properties.Resources.Enygma_Chest_sprite);

                // Centraliza o baú no marcador do mapa.
                int worldX = marcador.x * map.tileSize + (map.tileSize - chest.Width) / 2;

                // Corrige os 10 pixels transparentes na parte inferior.
                int worldY = marcador.y * map.tileSize - chest.Height + 10;

                _puzzleChest = chest;

                _puzzleChestWorldPosition = new Point(worldX,worldY);

                chest.Location = new Point(worldX - _cameraX,worldY - _cameraY);

                // O baú começa escondido.
                chest.Visible = visivelInicialmente;

                world1.Controls.Add(chest);

                break;
            }

            // Mantém o jogador à frente do baú.
            if (player != null && !player.IsDisposed)
            {
                player.BringToFront();
            }
        }

        private void SortearRecompensaBau()
        {
            AdicionarRunaAleatoria();
        }

        private int CalcularDanoComRunaPoder(int dano, int bonusPercentual)
        {
            int quantidadeRunas = _runasAdquiridas.Count(r => r == TipoRuna.Poder);

            if (quantidadeRunas == 0)
                return dano;

            int bonusTotal = bonusPercentual * quantidadeRunas;

            return (int)Math.Ceiling(dano * (100 + bonusTotal) / 100.0);
        }

        private void AplicarEfeitoRuna(TipoRuna runa)
        {
            switch (runa)
            {
                case TipoRuna.Vida:

                    if (player == null || player.IsDisposed)
                        return;

                    player.AumentarVidaMaxima(BonusVidaRuna);

                    AtualizarBarraVida();

                    break;
            }
        }

        private int ObterIntervaloRecuperacaoMana()
        {
            int quantidadeRunas = _runasAdquiridas.Count(
                r => r == TipoRuna.Agilidade
            );

            if (_personagem != Personagem.Mago || quantidadeRunas == 0)
                return MageManaRecoveryInterval;

            switch (quantidadeRunas)
            {
                case 1:
                    return 4;
                case 2:
                    return 3;
                default:
                    return 2;
            }
        }

        private int ObterCooldownAura()
        {
            int quantidadeRunas = _runasAdquiridas.Count(r => r == TipoRuna.Agilidade);

            if (_personagem != Personagem.Cavaleiro || quantidadeRunas == 0)
                return KnightAuraCooldownTicks;

            int reducaoPercentual = 25 * quantidadeRunas;

            return KnightAuraCooldownTicks * (100 - reducaoPercentual) / 100;
        }

        private int ObterVelocidadeMovimento()
        {
            int quantidadeRunas = _runasAdquiridas.Count(r => r == TipoRuna.Agilidade);

            if (_personagem != Personagem.Princesa || quantidadeRunas == 0)
                return VelocidadePlayer;

            return (int)Math.Ceiling(VelocidadePlayer * (1 + 0.25 * quantidadeRunas));
        }

        private void AdicionarRunaAleatoria()
        {
            // Impede que o jogador ultrapasse três runas.
            if (_runasAdquiridas.Count >= MaxRunasInventario)
                return;

            // Todas as runas podem ser sorteadas, inclusive as repetidas.
            TipoRuna[] todasRunas = (TipoRuna[])Enum.GetValues(typeof(TipoRuna));

            // Sorteia uma das três runas disponíveis.
            TipoRuna runaSorteada = todasRunas[
                _recompensaRandom.Next(todasRunas.Length)
            ];

            // Registra a runa no inventário.
            _runasAdquiridas.Add(runaSorteada);

            // Aplica o efeito da runa adquirida.
            AplicarEfeitoRuna(runaSorteada);

            AtualizarInventario();
        }

        private void VerificarRecompensaBau()
        {
            if (_recompensaBauEntregue)
                return;

            if (_puzzleChest == null || _puzzleChest.IsDisposed)
                return;

            if (!_puzzleChest.IsOpen)
                return;

            // Impede que a recompensa seja entregue novamente.
            _recompensaBauEntregue = true;

            // Todos os baús entregam uma runa aleatória.
            SortearRecompensaBau();
        }

        private void LimparPuzzleChest()
        {
            if (_puzzleChest == null)
                return;

            world1.Controls.Remove(_puzzleChest);

            _puzzleChest.Dispose();
            _puzzleChest = null;
        }
        private void AtualizarPosicaoPuzzleChestNaTela()
        {
            if (_puzzleChest == null || _puzzleChest.IsDisposed)
                return;

            _puzzleChest.Location = new Point(
                _puzzleChestWorldPosition.X - _cameraX,
                _puzzleChestWorldPosition.Y - _cameraY
            );
        }
        private void AtualizarProximidadePuzzleChest()
        {
            _puzzleChestAoAlcance = false;

            if (player == null || player.IsDisposed ||
                _puzzleChest == null || _puzzleChest.IsDisposed ||
                !_puzzleChest.Visible)
            {
                return;
            }

            double jogadorCentroX = _playerWorldX + player.Width / 2.0;
            double jogadorCentroY = _playerWorldY + player.Height / 2.0;

            double bauCentroX = _puzzleChestWorldPosition.X + _puzzleChest.Width / 2.0;

            double bauCentroY = _puzzleChestWorldPosition.Y + _puzzleChest.Height / 2.0;

            double dx = jogadorCentroX - bauCentroX;
            double dy = jogadorCentroY - bauCentroY;

            double distancia = dx * dx + dy * dy;

            _puzzleChestAoAlcance = distancia <= DistanciaInteracaoRuna * DistanciaInteracaoRuna;
        }
        #endregion

        #region Mid-Boss
        private void LimparMidBoss()
        {
            if (_midBoss == null)
                return;

            world1.Controls.Remove(_midBoss);

            _midBoss.AttackImpact -= MidBoss_AttackImpact;
            _midBoss.DeathAnimationCompleted -= MidBoss_DeathAnimationCompleted;
            _midBoss.Dispose();
            _midBoss = null;
            _midBossAttackCooldownRemaining = 0;
            _midBossWorldPosition = Point.Empty;
            _midBossVisualOffsetY = 0;
        }
        #endregion

        #region Judgment Forge Countdown
        private void CriarIndicadorTempoJulgamento()
        {
            if (_judgmentForgeTempoLabel != null && !_judgmentForgeTempoLabel.IsDisposed)
            {
                _judgmentForgeTempoLabel.Text = "TEMPO: 5s";
                _judgmentForgeTempoLabel.Visible = false;
                return;
            }

            _judgmentForgeTempoLabel = new Label();

            _judgmentForgeTempoLabel.Name = "LBL_JudgmentForgeTimer";
            _judgmentForgeTempoLabel.Text = "TEMPO: 5s";
            _judgmentForgeTempoLabel.AutoSize = true;

            _judgmentForgeTempoLabel.Font = new Font("Arial", 14, FontStyle.Bold);

            _judgmentForgeTempoLabel.ForeColor = Color.Gold;
            _judgmentForgeTempoLabel.BackColor = Color.FromArgb(220, 35, 20, 12);

            _judgmentForgeTempoLabel.Padding = new Padding(14, 8, 14, 8);

            _judgmentForgeTempoLabel.BorderStyle = BorderStyle.FixedSingle;

            _judgmentForgeTempoLabel.Visible = false;

            // Posiciona o cronômetro acima da caixa de avanço do mapa.
            int topoPromptStage = (_stagePrompt != null && !_stagePrompt.IsDisposed)
                ? _stagePrompt.Top
                : world1.ClientSize.Height - 24;

            int posicaoY = topoPromptStage - _judgmentForgeTempoLabel.Height - 10;

            _judgmentForgeTempoLabel.Location = new Point(
                (world1.ClientSize.Width - _judgmentForgeTempoLabel.Width) / 2,
                posicaoY
            );

            world1.Controls.Add(_judgmentForgeTempoLabel);
            _judgmentForgeTempoLabel.BringToFront();
        }

        private void IniciarContagemJulgamento()
        {
            if (!_judgmentForgeCarregado || _judgmentForgeContagemAtiva || _judgmentForgeContagemFalhou)
                return;

            _judgmentForgeTempoRestanteMs = JudgmentForgeTempoLimiteMs;
            _judgmentForgeTempoAcumuladoMs = 0;

            _judgmentForgeContagemAtiva = true;

            if (_judgmentForgeTempoLabel != null && !_judgmentForgeTempoLabel.IsDisposed)
            {
                _judgmentForgeTempoLabel.Text = "TEMPO: 5s";
                _judgmentForgeTempoLabel.Visible = true;
                _judgmentForgeTempoLabel.BringToFront();
            }
        }

        private void AtualizarContagemJulgamento()
        {
            if (!_judgmentForgeCarregado || !_judgmentForgeContagemAtiva || _judgmentForgeContagemFalhou)
                return;

            _judgmentForgeTempoAcumuladoMs += TMR_Game.Interval;

            _judgmentForgeTempoRestanteMs = JudgmentForgeTempoLimiteMs - _judgmentForgeTempoAcumuladoMs;

            // Atualiza o indicador a cada segundo.
            int segundosRestantes = (int)Math.Ceiling(_judgmentForgeTempoRestanteMs / 1000.0);

            if (_judgmentForgeTempoLabel != null && !_judgmentForgeTempoLabel.IsDisposed)
            {
                _judgmentForgeTempoLabel.Text = "TEMPO: " + Math.Max(0, segundosRestantes) + "s";

                // Destaca os últimos segundos.
                _judgmentForgeTempoLabel.ForeColor = segundosRestantes <= 3 ? Color.Red : Color.Gold;
            }

            // O tempo acabou.
            if (_judgmentForgeTempoRestanteMs <= 0)
            {
                _judgmentForgeTempoRestanteMs = 0;

                _judgmentForgeContagemAtiva = false;
                _judgmentForgeContagemFalhou = true;

                AplicarPenalidadeJulgamento();
            }
        }

        private void AplicarPenalidadeJulgamento()
        {
            // Interrompe a música da Forja.
            SoundEffects.StopWavLoop();

            // Vharzun repreende o jogador por falhar no julgamento.
            SoundEffects.PlayVoice(
                Properties.Resources.Vharzun_Forge_Failure,
                0.9f
            );

            if (player == null || player.IsDisposed || player.EstaMorto)
                return;

            // A descarga mágica elimina toda a vida do jogador.
            player.ReceberDano(player.VidaAtual);

            // Atualiza o HUD.
            AtualizarBarraVida();

            // Esconde o indicador.
            if (_judgmentForgeTempoLabel != null && !_judgmentForgeTempoLabel.IsDisposed)
            {
                _judgmentForgeTempoLabel.Text = "JULGAMENTO FALHOU!";
                _judgmentForgeTempoLabel.ForeColor = Color.Red;
            }
        }
        #endregion
    }
}