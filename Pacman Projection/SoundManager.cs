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

    internal class SoundManager
    {
        // Declare dictionaries to keep track of all sounds and their current states
        internal Dictionary<string, Sound> activeSounds = new Dictionary<string, Sound>();
        internal Dictionary<string, Sound> pausedSounds = new Dictionary<string, Sound>();
        internal Dictionary<string, byte[]> soundData = new Dictionary<string, byte[]>();

        public bool toPlaySounds { get; set; } = true;

        public const int loopingLatency = 70; // 70ms latency for looping sounds

        public SoundManager()
        {
            // Add all sound resources to soundData and load them
            soundData.Add("menuMusic", null);   
            soundData.Add("buttonReady", null);
            soundData.Add("pacman_beginning", null);
            soundData.Add("pacman_chomp", null);
            soundData.Add("pacman_eatFruit", null);
            soundData.Add("pacman_eatGhost", null);
            soundData.Add("pacman_death", null);
            soundData.Add("ghost_scared", null);
            soundData.Add("ghost_return", null);
            soundData.Add("ghost_scatter", null);
            soundData.Add("ghost_chase1", null);
            soundData.Add("ghost_chase2", null);
            soundData.Add("ghost_chase3", null);


            /*   ENSURE ALL SOUNDS TO BE LODADED HAVE 'Build Action' SET TO 'Embedded Resources'   */

            LoadSound("menuMusic", "Pacman_Projection.Resources.menuMusic.wav");
            LoadSound("buttonReady", "Pacman_Projection.Resources.buttonReady.wav");
            LoadSound("pacman_beginning", "Pacman_Projection.Resources.pacman_beginning.wav");
            LoadSound("pacman_chomp", "Pacman_Projection.Resources.pacman_chomp.wav");
            LoadSound("pacman_eatFruit", "Pacman_Projection.Resources.pacman_eatfruit.wav");
            LoadSound("pacman_eatGhost", "Pacman_Projection.Resources.pacman_eatghost.wav");
            LoadSound("pacman_death", "Pacman_Projection.Resources.pacman_death.wav");
            LoadSound("ghost_scared", "Pacman_Projection.Resources.ghost_scared.wav");
            LoadSound("ghost_return", "Pacman_Projection.Resources.ghost_return.wav");
            LoadSound("ghost_scatter", "Pacman_Projection.Resources.ghost_scatter.wav");
            LoadSound("ghost_chase1", "Pacman_Projection.Resources.ghost_chase1.wav");
            LoadSound("ghost_chase2", "Pacman_Projection.Resources.ghost_chase2.wav");
            LoadSound("ghost_chase3", "Pacman_Projection.Resources.ghost_chase3.wav");
        }

        private void LoadSound(string soundName, string resourcePath)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);

            if (stream != null)
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                soundData[soundName] = buffer;
            }
        }

        public void PlaySound(string soundName, bool loop)
        {
            if (toPlaySounds)
            {
                Task.Run(async () =>
                { 
                    if (soundData.ContainsKey(soundName))
                    {
                        MemoryStream memoryStream = new MemoryStream(soundData[soundName]);
                        WaveFileReader reader = new WaveFileReader(memoryStream);
                        WaveOutEvent waveOut = null;
                        WaveStream waveStream = null;

                        if (loop && !await CheckForSound(soundName)) // Don't loop sound again if it's already looping
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

                        if (!await CheckForSound(soundName))
                        {
                            activeSounds.Add(soundName, new Sound(soundName, loop, memoryStream, reader, waveOut, waveStream));
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
                                activeSounds?.Remove(soundName);
                            };
                        }
                    }
                    else
                    {
                        MessageBox.Show("Sound not found and/or cannot be played: \n" + soundName + ".\n POSSIBLE SOLUTION: " +
                        "\n1. Ensure soundData contains the sound-file. " +
                        "\n2. Check for spelling errors at relevant locations and methods in used code. " +
                        "\n3. Set sound-property 'Build Action' => 'Embedded Resources'");
                    }
                });
            }
        }

        public void UnpauseSound(string soundName)
        {
            if (pausedSounds.ContainsKey(soundName))
            {
                if (pausedSounds[soundName].looping)
                {
                    var sound = pausedSounds[soundName];
                    sound.waveStream.Position = sound.pausedPosition; // Restore the position to its last paused position
                    sound.waveOut.Play();
                    activeSounds.Add(soundName, sound);
                    pausedSounds.Remove(soundName);
                }
                else
                {
                    var sound = pausedSounds[soundName];
                    sound.memoryStream.Position = sound.pausedPosition; // Restore the position to its last paused position
                    sound.waveOut.Play();
                    activeSounds.Add(soundName, sound);
                    pausedSounds.Remove(soundName);
                }
            }
        }   

        private Task<bool> CheckForSound(string soundName)
        {
            return Task.FromResult(activeSounds.ContainsKey(soundName));
        }

        public void StopSound(string soundName)
        {
            if (soundData.ContainsKey(soundName) && activeSounds.ContainsKey(soundName))
            {
                activeSounds[soundName].waveOut.Stop();
                activeSounds[soundName].memoryStream.Dispose();
                activeSounds[soundName].reader.Dispose();
                activeSounds[soundName].waveStream?.Dispose();
                activeSounds.Remove(soundName);
            }
        }

        public void StopAllSounds()
        {
            if (activeSounds.Count > 0)
            {
                int index = activeSounds.Count - 1;
                while (index >= 0)
                {
                    try
                    {
                        activeSounds.ElementAt(index).Value.waveOut.Stop();
                        activeSounds.ElementAt(index).Value.memoryStream.Dispose();
                        activeSounds.ElementAt(index).Value.reader.Dispose();
                        activeSounds.ElementAt(index).Value.waveStream?.Dispose();
                        activeSounds.Remove(activeSounds.ElementAt(index).Key);
                        index--;
                    }
                    catch (Exception) 
                    { 
                        index--; 
                    }
                }
            }
        }

        public void PauseSound(string soundName)
        {
            if (activeSounds.ContainsKey(soundName))
            {
                var sound = activeSounds[soundName];
                sound.waveOut.Pause();
                sound.pausedPosition = sound.memoryStream.Position; // Save the current position of the sound
                pausedSounds.Add(soundName, sound);
                activeSounds.Remove(soundName);
            }
        }

        public void PauseLoopedSound(string soundName)
        {
            if (activeSounds.ContainsKey(soundName))
            {
                var sound = activeSounds[soundName];
                sound.waveOut.Pause();
                sound.pausedPosition = sound.waveStream.Position; // Save the current position of the sound
                pausedSounds.Add(soundName, sound);
                activeSounds.Remove(soundName);
            }
        }
    }
}
