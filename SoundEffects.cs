using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace FallenWorld
{
    public static class SoundEffects
    {
        #region Variáveis GLobais
        private static readonly object _sync = new object();
        private static readonly List<Playback> _activeSounds = new List<Playback>();
        // Armazena exclusivamente as vozes dos personagens.
        private static readonly List<Playback> _activeVoices = new List<Playback>();

        // Identifica solicitações de voz que foram canceladas.
        private static int _voicePlaybackRequestId = 0;
        private static Playback _fireballFlight;
        // Indica se o som de voo está sendo inicializado.
        private static bool _fireballFlightStarting;
        // Identifica solicitações de voo canceladas.
        private static int _fireballFlightRequestId;
        // Controle do som contínuo de recuperação de mana.
        private static Playback _manaRecovery;
        private static bool _manaRecoveryStarting;
        private static int _manaRecoveryRequestId;
        // Controle do som contínuo da Aura do Cavaleiro.
        private static Playback _knightAura;
        private static bool _knightAuraStarting;
        private static int _knightAuraRequestId;
        // Controle da música ambiente em WAV.
        private static Playback _wavLoop;
        private static bool _wavLoopStarting;
        private static int _wavLoopRequestId;
        // Controle do Audio Ducking.
        private static int _duckingSoundCount = 0;
        private static int _wavLoopFadeRequestId = 0;

        private static float _wavLoopBaseVolume = 0.26f;
        private static float _wavLoopCurrentVolume = 0.26f;

        private const float MusicDuckFactor = 0.5f;
        #endregion

        #region Play 
        public static void Play(Stream audioResource, float volume = 1f)
        {
            if (audioResource == null)
                return;

            // Carrega e inicializa o áudio em outra thread.
            Task.Run(() =>
            {
                Playback playback = CreatePlayback(
                    audioResource,
                    volume,
                    false,
                    true
                );

                if (playback == null)
                    return;

                lock (_sync)
                {
                    _activeSounds.Add(playback);
                }
                RegistrarSomDucking(playback);

                try
                {
                    playback.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    playback.ForceFinish();
                }
            });
        }

        public static void PlayVoice(Stream audioResource, float volume = 1f)
        {
            if (audioResource == null)
                return;

            int requestId;

            lock (_sync)
            {
                requestId = _voicePlaybackRequestId;
            }

            // Inicializa a voz fora da thread principal.
            Task.Run(() =>
            {
                Playback playback = CreatePlayback(
                    audioResource,
                    volume,
                    false
                );

                if (playback == null)
                    return;

                bool iniciar = false;

                lock (_sync)
                {
                    // Impede iniciar uma voz solicitada antes de StopVoices().
                    if (requestId == _voicePlaybackRequestId)
                    {
                        playback.EnableMusicDucking();

                        _activeVoices.Add(playback);
                        _activeSounds.Add(playback);

                        // Mantém a redução de volume da música ambiente.
                        RegistrarSomDucking(playback);

                        iniciar = true;

                        try
                        {
                            playback.Start();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(
                                "Erro ao reproduzir voz: " + ex.Message
                            );

                            playback.ForceFinish();
                        }
                    }
                }

                // Libera uma voz cuja reprodução foi cancelada durante a inicialização.
                if (!iniciar)
                    playback.ForceFinish();
            });
        }

        public static void StopVoices()
        {
            Playback[] vozesParaParar;

            lock (_sync)
            {
                // Cancela solicitações de voz que ainda estão sendo inicializadas.
                _voicePlaybackRequestId++;

                // Copia as vozes atualmente registradas.
                vozesParaParar = _activeVoices.ToArray();

                // Limpa o registro das vozes ativas.
                _activeVoices.Clear();
            }

            // Interrompe cada voz fora do bloqueio.
            foreach (Playback voz in vozesParaParar)
            {
                if (voz != null)
                    voz.Stop();
            }
        }

        public static void PlayWav(Stream audioResource, float volume = 1f)
        {
            if (audioResource == null)
                return;

            Task.Run(() =>
            {
                Playback playback = CreateWavPlayback(
                    audioResource,
                    volume,
                    false,
                    true
                );

                if (playback == null)
                    return;

                lock (_sync)
                {
                    _activeSounds.Add(playback);
                }
                RegistrarSomDucking(playback);

                try
                {
                    playback.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Erro ao reproduzir WAV: " + ex.Message);
                    playback.ForceFinish();
                }
            });
        }

        public static void StartWavLoop(Stream audioResource, float volume = 0.18f)
        {
            if (audioResource == null)
                return;

            int requestId;

            lock (_sync)
            {
                // Evita iniciar outra música enquanto uma já está tocando.
                if (_wavLoop != null || _wavLoopStarting)
                {
                    audioResource.Dispose();
                    return;
                }

                _wavLoopStarting = true;
                requestId = ++_wavLoopRequestId;
            }

            Task.Run(() =>
            {
                Playback playback = CreateWavPlayback(audioResource,volume,true);

                bool iniciar = false;

                lock (_sync)
                {
                    if (requestId == _wavLoopRequestId && _wavLoopStarting)
                    {
                        _wavLoopStarting = false;

                        if (playback != null)
                        {
                            _wavLoop = playback;

                            _wavLoopBaseVolume = volume;

                            _wavLoopCurrentVolume = _duckingSoundCount > 0
                                ? volume * MusicDuckFactor
                                : volume;

                            playback.SetVolume(_wavLoopCurrentVolume);

                            _activeSounds.Add(playback);

                            iniciar = true;
                        }
                    }
                }

                if (!iniciar)
                {
                    playback?.ForceFinish();
                    return;
                }

                try
                {
                    playback.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Erro ao iniciar música ambiente: " + ex.Message);
                    playback.ForceFinish();
                }
            });
        }

        public static void StartMp3Loop(Stream audioResource, float volume = 0.18f)
        {
            if (audioResource == null)
                return;

            int requestId;

            lock (_sync)
            {
                if (_wavLoop != null || _wavLoopStarting)
                {
                    audioResource.Dispose();
                    return;
                }

                _wavLoopStarting = true;
                requestId = ++_wavLoopRequestId;
            }

            Task.Run(() =>
            {
                // Utiliza CreatePlayback, que decodifica MP3.
                Playback playback = CreatePlayback(audioResource, volume, true);

                bool iniciar = false;

                lock (_sync)
                {
                    if (requestId == _wavLoopRequestId && _wavLoopStarting)
                    {
                        _wavLoopStarting = false;

                        if (playback != null)
                        {
                            _wavLoop = playback;
                            _wavLoopBaseVolume = volume;

                            _wavLoopCurrentVolume = _duckingSoundCount > 0
                                ? volume * MusicDuckFactor
                                : volume;

                            playback.SetVolume(_wavLoopCurrentVolume);

                            _activeSounds.Add(playback);

                            iniciar = true;
                        }
                    }
                }

                if (!iniciar)
                {
                    playback?.ForceFinish();
                    return;
                }

                try
                {
                    playback.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Erro ao iniciar música ambiente MP3: " + ex.Message);
                    playback.ForceFinish();
                }
            });
        }

        public static void StopWavLoop()
        {
            Playback playback;

            lock (_sync)
            {
                _wavLoopRequestId++;
                _wavLoopFadeRequestId++;
                _wavLoopStarting = false;
                playback = _wavLoop;
                _wavLoop = null;
            }

            playback?.Stop();
        }
        #endregion

        #region Fireball Sounds
        public static void StartFireballFlight(Stream audioResource,float volume = 0.30f)
        {
            if (audioResource == null)
                return;

            int requestId;

            lock (_sync)
            {
                // Não cria outro reprodutor se já existe um voo ativo.
                if (_fireballFlight != null || _fireballFlightStarting)
                {
                    audioResource.Dispose();
                    return;
                }

                _fireballFlightStarting = true;
                requestId = ++_fireballFlightRequestId;
            }

            // Inicializa o áudio fora da thread da interface.
            Task.Run(() =>
            {
                Playback playback = CreatePlayback(audioResource,volume,true);

                bool iniciar = false;

                lock (_sync)
                {
                    if (requestId == _fireballFlightRequestId && _fireballFlightStarting)
                    {
                        _fireballFlightStarting = false;

                        if (playback != null)
                        {
                            _fireballFlight = playback;
                            _activeSounds.Add(playback);
                            iniciar = true;
                        }
                    }
                }

                // A solicitação foi cancelada durante a inicialização.
                if (!iniciar)
                {
                    playback?.ForceFinish();
                    return;
                }

                try
                {
                    playback.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    playback.ForceFinish();
                }
            });
        }

        public static void StopFireballFlight()
        {
            Playback playback;

            lock (_sync)
            {
                // Cancela qualquer inicialização pendente.
                _fireballFlightRequestId++;
                _fireballFlightStarting = false;

                playback = _fireballFlight;
                _fireballFlight = null;
            }

            playback?.Stop();
        }
        #endregion

        #region Mana Recovery Sounds
        public static void StartManaRecovery(Stream audioResource,float volume = 0.25f)
        {
            if (audioResource == null)
                return;

            int requestId;

            lock (_sync)
            {
                if (_manaRecovery != null || _manaRecoveryStarting)
                {
                    audioResource.Dispose();
                    return;
                }

                _manaRecoveryStarting = true;
                requestId = ++_manaRecoveryRequestId;
            }

            // Carrega e inicializa o áudio fora da thread da interface.
            Task.Run(() =>
            {
                Playback playback = CreatePlayback(audioResource,volume,true);

                bool iniciar = false;

                lock (_sync)
                {
                    if (requestId == _manaRecoveryRequestId &&
                        _manaRecoveryStarting)
                    {
                        _manaRecoveryStarting = false;

                        if (playback != null)
                        {
                            _manaRecovery = playback;
                            _activeSounds.Add(playback);
                            iniciar = true;
                        }
                    }
                }

                if (!iniciar)
                {
                    playback?.ForceFinish();
                    return;
                }

                try
                {
                    playback.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Erro no som de recuperação de mana: " + ex.Message);
                    playback.ForceFinish();
                }
            });
        }

        public static void StopManaRecovery()
        {
            Playback playback;

            lock (_sync)
            {
                // Cancela uma inicialização que ainda esteja acontecendo.
                _manaRecoveryRequestId++;
                _manaRecoveryStarting = false;

                playback = _manaRecovery;
                _manaRecovery = null;
            }

            playback?.Stop();
        }
        #endregion

        #region Knight Aura Sounds
        public static void StartKnightAura(Stream audioResource,float volume = 0.25f)
        {
            if (audioResource == null)
                return;

            int requestId;

            lock (_sync)
            {
                if (_knightAura != null || _knightAuraStarting)
                {
                    audioResource.Dispose();
                    return;
                }

                _knightAuraStarting = true;
                requestId = ++_knightAuraRequestId;
            }

            Task.Run(() =>
            {
                Playback playback = CreatePlayback(audioResource,volume,true);

                bool iniciar = false;

                lock (_sync)
                {
                    if (requestId == _knightAuraRequestId && _knightAuraStarting)
                    {
                        _knightAuraStarting = false;

                        if (playback != null)
                        {
                            _knightAura = playback;
                            _activeSounds.Add(playback);
                            iniciar = true;
                        }
                    }
                }

                if (!iniciar)
                {
                    playback?.ForceFinish();
                    return;
                }

                try
                {
                    playback.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Erro no som da Aura: " + ex.Message);
                    playback.ForceFinish();
                }
            });
        }

        public static void StopKnightAura()
        {
            Playback playback;

            lock (_sync)
            {
                _knightAuraRequestId++;
                _knightAuraStarting = false;

                playback = _knightAura;
                _knightAura = null;
            }

            playback?.Stop();
        }
        #endregion

        // Controle do som contínuo da onda venenosa da Rainha.
        private static Playback _queenPoisonWave;
        private static bool _queenPoisonWaveStarting;
        private static int _queenPoisonWaveRequestId;

        #region Gothic Queen Poison Wave Sounds
        public static bool IsQueenPoisonWaveActive
        {
            get
            {
                lock (_sync)
                {
                    // Considera tanto a reprodução ativa quanto a inicialização.
                    return _queenPoisonWave != null || _queenPoisonWaveStarting;
                }
            }
        }
        public static void StartQueenPoisonWave(Stream audioResource,float volume = 0.32f)
        {
            if (audioResource == null)
                return;

            int requestId;

            lock (_sync)
            {
                if (_queenPoisonWave != null || _queenPoisonWaveStarting)
                {
                    audioResource.Dispose();
                    return;
                }

                _queenPoisonWaveStarting = true;
                requestId = ++_queenPoisonWaveRequestId;
            }

            Task.Run(() =>
            {
                Playback playback = CreatePlayback(
                    audioResource,
                    volume,
                    true
                );

                bool iniciar = false;

                lock (_sync)
                {
                    if (requestId == _queenPoisonWaveRequestId &&
                        _queenPoisonWaveStarting)
                    {
                        _queenPoisonWaveStarting = false;

                        if (playback != null)
                        {
                            _queenPoisonWave = playback;
                            _activeSounds.Add(playback);
                            iniciar = true;
                        }
                    }
                }

                if (!iniciar)
                {
                    playback?.ForceFinish();
                    return;
                }

                try
                {
                    playback.Start();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(
                        "Erro no som da onda venenosa: " + ex.Message
                    );

                    playback.ForceFinish();
                }
            });
        }

        public static void StopQueenPoisonWave()
        {
            Playback playback;

            lock (_sync)
            {
                _queenPoisonWaveRequestId++;
                _queenPoisonWaveStarting = false;

                playback = _queenPoisonWave;
                _queenPoisonWave = null;
            }

            playback?.Stop();
        }

        #endregion

        #region Playback Management
        private static Playback CreatePlayback(Stream audioResource,float volume,bool loop, bool ducksMusic = false)
        {
            if (audioResource == null)
                return null;

            MemoryStream buffer = new MemoryStream();
            Mp3FileReader reader = null;
            WaveOutEvent output = null;

            try
            {
                if (audioResource.CanSeek)
                    audioResource.Position = 0;

                audioResource.CopyTo(buffer);
                audioResource.Dispose();

                buffer.Position = 0;

                // Decodifica o MP3.
                reader = new Mp3FileReader(buffer);

                // Controla o volume.
                VolumeWaveProvider16 volumeProvider = new VolumeWaveProvider16(reader)
                {
                    Volume = Math.Max(0f,Math.Min(1f, volume))
                };

                output = new WaveOutEvent();
                output.Init(volumeProvider);

                return new Playback(reader, volumeProvider, output, loop, ducksMusic);
            }
            catch (Exception ex)
            {
                output?.Dispose();
                reader?.Dispose();

                if (reader == null)
                    buffer.Dispose();

                audioResource.Dispose();

                Debug.WriteLine("Erro ao carregar MP3: " + ex.Message);

                return null;
            }
        }

        private static Playback CreateWavPlayback(Stream audioResource,float volume,bool loop, bool ducksMusic = false)
        {
            if (audioResource == null)
                return null;

            MemoryStream buffer = new MemoryStream();
            WaveFileReader reader = null;
            WaveOutEvent output = null;

            try
            {
                if (audioResource.CanSeek)
                    audioResource.Position = 0;

                audioResource.CopyTo(buffer);
                audioResource.Dispose();

                buffer.Position = 0;

                // Decodifica o arquivo WAV.
                reader = new WaveFileReader(buffer);

                // Controla o volume.
                VolumeWaveProvider16 volumeProvider = new VolumeWaveProvider16(reader)
                {
                    Volume = Math.Max(0f, Math.Min(1f, volume))
                };

                output = new WaveOutEvent();
                output.Init(volumeProvider);

                return new Playback(reader, volumeProvider, output, loop, ducksMusic);
            }
            catch (Exception ex)
            {
                output?.Dispose();
                reader?.Dispose();

                if (reader == null)
                    buffer.Dispose();

                audioResource.Dispose();

                Debug.WriteLine("Erro ao carregar WAV: " + ex.Message);

                return null;
            }
        }

        private static void OnPlaybackFinished(Playback playback)
        {
            bool restaurarMusica = false;

            lock (_sync)
            {
                _activeSounds.Remove(playback);
                _activeVoices.Remove(playback);

                if (_fireballFlight == playback)
                    _fireballFlight = null;

                if (_manaRecovery == playback)
                    _manaRecovery = null;

                if (_knightAura == playback)
                    _knightAura = null;

                if (_queenPoisonWave == playback)
                    _queenPoisonWave = null;

                if (playback.DucksMusic && _duckingSoundCount > 0)
                {
                    _duckingSoundCount--;

                    restaurarMusica = _duckingSoundCount == 0;
                }
            }

            if (restaurarMusica)
                AtualizarVolumeMusicaAmbiente(false);
        }

        private sealed class Playback
        {
            private readonly WaveStream _reader;
            private readonly WaveOutEvent _output;
            private readonly VolumeWaveProvider16 _volumeProvider;
            private bool _ducksMusic;

            public bool DucksMusic
            {
                get { return _ducksMusic; }
            }
            public void EnableMusicDucking()
            {
                _ducksMusic = true;
            }
            private readonly bool _loop;

            private volatile bool _stopRequested;
            private int _finished;

            public Playback(WaveStream reader,VolumeWaveProvider16 volumeProvider,WaveOutEvent output,bool loop, bool ducksMusic)
            {
                _reader = reader;
                _volumeProvider = volumeProvider;
                _output = output;
                _loop = loop;
                _ducksMusic = ducksMusic;
                _output.PlaybackStopped += Output_PlaybackStopped;
            }

            public void SetVolume(float volume)
            {
                _volumeProvider.Volume = Math.Max(0f,Math.Min(1f, volume));
            }

            public void Start()
            {
                _output.Play();
            }

            public void Stop()
            {
                _stopRequested = true;

                try
                {
                    _output.Stop();
                }
                catch
                {
                    Finish();
                }
            }

            private void Output_PlaybackStopped(object sender,StoppedEventArgs e)
            {
                if (_loop && !_stopRequested && e.Exception == null)
                {
                    try
                    {
                        _reader.Position = 0;
                        _output.Play();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }

                Finish();
            }

            public void ForceFinish()
            {
                _stopRequested = true;
                Finish();
            }

            private void Finish()
            {
                if (Interlocked.Exchange(ref _finished, 1) != 0)
                    return;

                SoundEffects.OnPlaybackFinished(this);

                _output.PlaybackStopped -= Output_PlaybackStopped;

                _output.Dispose();
                _reader.Dispose();
            }
        }
        #endregion

        private static void RegistrarSomDucking(Playback playback)
        {
            if (playback == null || !playback.DucksMusic)
                return;

            bool reduzirMusica = false;

            lock (_sync)
            {
                _duckingSoundCount++;

                reduzirMusica = _duckingSoundCount == 1;
            }

            if (reduzirMusica)
                AtualizarVolumeMusicaAmbiente(true);
        }

        private static void AtualizarVolumeMusicaAmbiente(bool reduzir)
        {
            Playback musica;
            float volumeInicial;
            float volumeFinal;
            int requestId;

            lock (_sync)
            {
                musica = _wavLoop;

                if (musica == null)
                    return;

                requestId = ++_wavLoopFadeRequestId;

                volumeInicial = _wavLoopCurrentVolume;

                volumeFinal = reduzir
                    ? _wavLoopBaseVolume * MusicDuckFactor
                    : _wavLoopBaseVolume;
            }

            Task.Run(async () =>
            {
                const int etapas = 10;
                const int intervaloMs = 30;

                for (int i = 1; i <= etapas; i++)
                {
                    await Task.Delay(intervaloMs);

                    float novoVolume;

                    lock (_sync)
                    {
                        // Cancela uma transição antiga, se outra começou.
                        if (_wavLoop != musica || requestId != _wavLoopFadeRequestId)
                            return;

                        novoVolume = volumeInicial + (volumeFinal - volumeInicial) * i / etapas;

                        _wavLoopCurrentVolume = novoVolume;
                    }

                    musica.SetVolume(novoVolume);
                }
            });
        }
    }
}