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
                    false
                );

                if (playback == null)
                    return;

                lock (_sync)
                {
                    _activeSounds.Add(playback);
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

        #region Playback Management
        private static Playback CreatePlayback(Stream audioResource,float volume,bool loop)
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

                return new Playback(reader, output, loop);
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

        private static void OnPlaybackFinished(Playback playback)
        {
            lock (_sync)
            {
                _activeSounds.Remove(playback);

                if (_fireballFlight == playback)
                    _fireballFlight = null;

                if (_manaRecovery == playback)
                    _manaRecovery = null;

                if (_knightAura == playback)
                    _knightAura = null;
            }
        }

        private sealed class Playback
        {
            private readonly Mp3FileReader _reader;
            private readonly WaveOutEvent _output;
            private readonly bool _loop;

            private volatile bool _stopRequested;
            private int _finished;

            public Playback(Mp3FileReader reader,WaveOutEvent output,bool loop)
            {
                _reader = reader;
                _output = output;
                _loop = loop;

                _output.PlaybackStopped += Output_PlaybackStopped;
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
    }
}