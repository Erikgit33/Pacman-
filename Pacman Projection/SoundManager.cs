using NAudio.Wave;
using Pacman_Projection.Properties;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pacman_Projection
{
    public enum Sounds
    {
        menuMusic,
        buttonPress,
        pacman_beginning,
        pacman_chomp,
        pacman_eatFruit,
        pacman_eatGhost,
        pacman_death,
        pacman_win,
        ghost_scared,
        ghost_return,
        ghost_scatter,
        ghost_chase1,
        ghost_chase2,
        ghost_chase3
    }
    public class SoundManager
    {
        // Declare dictionaries to keep track of all sounds and their current states
        internal Dictionary<Sounds, Sound> activeSounds = new Dictionary<Sounds, Sound>();
        internal Dictionary<Sounds, Sound> pausedSounds = new Dictionary<Sounds, Sound>();
        internal Dictionary<Sounds, byte[]> soundData = new Dictionary<Sounds, byte[]>();

        public bool toPlaySounds { get; set; } = true;

        public const int loopingLatency = 70; // 70ms latency for looping sounds

        public SoundManager()
        {
            // Add all sound resources to soundData and load them
            // Every instance of soundManager will have access to these sounds
            soundData.Add(Sounds.menuMusic, null);   
            soundData.Add(Sounds.buttonPress, null);
            soundData.Add(Sounds.pacman_beginning, null);
            soundData.Add(Sounds.pacman_chomp, null);
            soundData.Add(Sounds.pacman_eatFruit, null);
            soundData.Add(Sounds.pacman_eatGhost, null);
            soundData.Add(Sounds.pacman_death, null);
            soundData.Add(Sounds.ghost_scared, null);
            soundData.Add(Sounds.ghost_return, null);
            soundData.Add(Sounds.ghost_scatter, null);
            soundData.Add(Sounds.ghost_chase1, null);
            soundData.Add(Sounds.ghost_chase2, null);
            soundData.Add(Sounds.ghost_chase3, null);


            /*   ENSURE ALL SOUNDS TO BE LODADED HAVE 'Build Action' SET TO 'Embedded Resources'   */

            LoadSound(Sounds.menuMusic, "Pacman_Projection.Resources.menuMusic.wav");
            LoadSound(Sounds.buttonPress, "Pacman_Projection.Resources.buttonPress.wav");
            LoadSound(Sounds.pacman_beginning, "Pacman_Projection.Resources.pacman_beginning.wav");
            LoadSound(Sounds.pacman_chomp, "Pacman_Projection.Resources.pacman_chomp.wav");
            LoadSound(Sounds.pacman_eatFruit, "Pacman_Projection.Resources.pacman_eatFruit.wav");
            LoadSound(Sounds.pacman_eatGhost, "Pacman_Projection.Resources.pacman_eatGhost.wav");
            LoadSound(Sounds.pacman_death, "Pacman_Projection.Resources.pacman_death.wav");
            LoadSound(Sounds.pacman_win, "Pacman_Projection.Resources.pacman_win.wav");
            LoadSound(Sounds.ghost_scared, "Pacman_Projection.Resources.ghost_scared.wav");
            LoadSound(Sounds.ghost_return, "Pacman_Projection.Resources.ghost_return.wav");
            LoadSound(Sounds.ghost_scatter, "Pacman_Projection.Resources.ghost_scatter.wav");
            LoadSound(Sounds.ghost_chase1, "Pacman_Projection.Resources.ghost_chase1.wav");
            LoadSound(Sounds.ghost_chase2, "Pacman_Projection.Resources.ghost_chase2.wav");
            LoadSound(Sounds.ghost_chase3, "Pacman_Projection.Resources.ghost_chase3.wav");
        }

        private void LoadSound(Sounds soundEnum, string resourcePath)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);

            if (stream != null)
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                soundData[soundEnum] = buffer;
            }
        }

        /// <summary>
        /// Plays a sound.
        /// </summary>
        public async void PlaySound(Sounds soundEnum, bool loop)
        {
            if (toPlaySounds)
            {
                if (soundData.ContainsKey(soundEnum))
                {
                    if (pausedSounds.ContainsKey(soundEnum))
                    {
                        pausedSounds.Remove(soundEnum); // Remove from paused sounds if it exists there
                    }
                    
                    MemoryStream memoryStream = new MemoryStream(soundData[soundEnum]);
                    WaveFileReader reader = new WaveFileReader(memoryStream);
                    WaveOutEvent waveOut = null;
                    WaveStream waveStream = null;

                    if (loop && !CheckForSound(soundEnum)) // Don't loop sound again if it's already looping
                    {
                        waveStream = loop ? new LoopStream(reader) : (WaveStream)reader;
                        waveOut = new WaveOutEvent();
                        // if looping, set the desired latency to loopingLatency (70ms)
                        waveOut.DesiredLatency = loopingLatency;

                        waveOut.Init(waveStream);
                        waveOut.Play();
                    }
                    else
                    {
                        waveOut = new WaveOutEvent();

                        waveOut.Init(reader);
                        waveOut.Play();
                    }

                    // If the sound doesn't exist in activeSounds, add it
                    if (!CheckForSound(soundEnum))
                    {
                        activeSounds.Add(soundEnum, new Sound(nameof(soundEnum), loop, memoryStream, reader, waveOut, waveStream));
                    }

                    if (!loop)
                    {
                        waveOut.PlaybackStopped += (sender, e) =>
                        {
                            // Dispose if not null, crash-preventing
                            waveOut?.Dispose();
                            waveStream?.Dispose();
                            memoryStream?.Dispose();
                            reader?.Dispose();
                            activeSounds?.Remove(soundEnum);
                        };
                    }
                }
            }
        }
        
        /// <summary>
        /// Plays a sound with the option of limiting it to the only playin instance of it.  
        /// </summary>
        public async void PlaySound(Sounds soundEnum, bool loop, bool limitToOneSimultaneous)
        {
            if (toPlaySounds)
            {
                if (soundData.ContainsKey(soundEnum))
                {
                    bool play = false;
                    if (CheckForSound(soundEnum))
                    {
                        if (!limitToOneSimultaneous)
                        {
                            play = true;
                        }
                    }
                    else
                    {
                        play = true;
                    }

                   
                    if (play)
                    {
                        if (pausedSounds.ContainsKey(soundEnum))
                        {
                            pausedSounds.Remove(soundEnum); // Remove from paused sounds if it exists there
                        }

                        MemoryStream memoryStream = new MemoryStream(soundData[soundEnum]);
                        WaveFileReader reader = new WaveFileReader(memoryStream);
                        WaveOutEvent waveOut = null;
                        WaveStream waveStream = null;

                        if (loop && !CheckForSound(soundEnum)) // Don't loop sound again if it's already looping
                        {
                            waveStream = loop ? new LoopStream(reader) : (WaveStream)reader;
                            waveOut = new WaveOutEvent();
                            // if looping, set the desired latency to loopingLatency (70ms)
                            waveOut.DesiredLatency = loopingLatency;

                            waveOut.Init(waveStream);
                            waveOut.Play();
                        }
                        else
                        {
                            waveOut = new WaveOutEvent();

                            waveOut.Init(reader);
                            waveOut.Play();
                        }

                        if (!CheckForSound(soundEnum))
                        {
                            activeSounds.Add(soundEnum, new Sound(nameof(soundEnum), loop, memoryStream, reader, waveOut, waveStream));
                        }

                        if (!loop)
                        {
                            waveOut.PlaybackStopped += (sender, e) =>
                            {
                                // Dispose if not null, crash-preventing
                                waveOut?.Dispose();
                                waveStream?.Dispose();
                                memoryStream?.Dispose();
                                reader?.Dispose();
                                activeSounds?.Remove(soundEnum);
                            };
                        }
                    }   
                }
            }
        }

        /// <summary>
        /// Unpauses a sound.
        /// </summary>
        public void UnpauseSound(Sounds soundEnum)
        {
            if (pausedSounds.ContainsKey(soundEnum))
            {
                if (pausedSounds[soundEnum].Looping)
                {
                    var sound = pausedSounds[soundEnum];
                    sound.WaveStream.Position = sound.PausedPosition; // Restore the position to its last paused position
                    sound.WaveOut.Play();
                    activeSounds.Add(soundEnum, sound);
                    pausedSounds.Remove(soundEnum);
                }
                else
                {
                    var sound = pausedSounds[soundEnum];
                    sound.MemoryStream.Position = sound.PausedPosition; // Restore the position to its last paused position
                    sound.WaveOut.Play();
                    activeSounds.Add(soundEnum, sound);
                    pausedSounds.Remove(soundEnum);
                }
            }
        }

        /// <summary>
        /// Unpauses all paused sounds.
        /// </summary>
        public void UnpauseAllSounds()
        {
            if (pausedSounds.Count > 0)
            {
                int index = pausedSounds.Count - 1;
                while (index >= 0)
                {
                    try
                    {
                        PauseSound(pausedSounds.ElementAt(index).Key);
                        index--;
                    }
                    catch
                    {
                        index--;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a sound is currently active (playing).
        /// </summary>
        private bool CheckForSound(Sounds sound)
        {
            return activeSounds.ContainsKey(sound);
        }

        /// <summary>
        /// Stops a sound.
        /// </summary>
        public void StopSound(Sounds soundEnum)
        {
            if (soundData.ContainsKey(soundEnum) && activeSounds.ContainsKey(soundEnum))
            {
                activeSounds[soundEnum].WaveOut.Stop();
                activeSounds[soundEnum].MemoryStream.Dispose();
                activeSounds[soundEnum].Reader.Dispose();
                activeSounds[soundEnum].WaveStream?.Dispose();
                activeSounds.Remove(soundEnum);
            }
        }

        /// <summary>
        /// Stops all sounds.
        /// </summary>
        public void StopAllSounds()
        {
            if (activeSounds.Count > 0)
            {
                int index = activeSounds.Count - 1;
                while (index >= 0)
                {
                    try
                    {
                        activeSounds.ElementAt(index).Value.WaveOut.Stop();
                        activeSounds.ElementAt(index).Value.MemoryStream.Dispose();
                        activeSounds.ElementAt(index).Value.Reader.Dispose();
                        activeSounds.ElementAt(index).Value.WaveStream?.Dispose();
                        activeSounds.Remove(activeSounds.ElementAt(index).Key);
                        index--;
                    }
                    catch 
                    { 
                        index--; 
                    }
                }
            }
        }

        /// <summary>
        /// Pause a non-looping sound.
        /// </summary>
        public void PauseSound(Sounds soundEnum)
        {
            if (activeSounds.ContainsKey(soundEnum))
            {
                var sound = activeSounds[soundEnum];
                sound.WaveOut.Pause();
                sound.PausedPosition = sound.MemoryStream.Position; // Save the current position of the sound
                pausedSounds.Add(soundEnum, sound);
                activeSounds.Remove(soundEnum);
            }
        }

        /// <summary>
        /// Pauses a looping sound.
        /// </summary>
        public void PauseLoopedSound(Sounds soundEnum)
        {
            if (activeSounds.ContainsKey(soundEnum))
            {
                var sound = activeSounds[soundEnum];
                sound.WaveOut.Pause();
                sound.PausedPosition = sound.WaveStream.Position; // Save the current position of the sound
                pausedSounds.Add(soundEnum, sound);
                activeSounds.Remove(soundEnum);
            }
        }
    }
}
