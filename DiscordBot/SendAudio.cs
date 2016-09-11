using Discord.Audio;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class SendAudio
    {
        public async void SendAudioo(Discord.DiscordClient DC, string filePath)
        {

            for (;;)
            {
                //filePath = comboBox1.Text;
                var voiceChannel = DC.FindServers("Do_Konca|Gaming").FirstOrDefault().VoiceChannels.ElementAt(1);

                var _vClient = await DC.GetService<AudioService>().Join(voiceChannel);

                var channelCount = DC.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
                using (var MP3Reader = new Mp3FileReader(filePath)) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                {
                    resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                    int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                    byte[] buffer = new byte[blockSize];
                    int byteCount;

                    while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                    {
                        if (byteCount < blockSize)
                        {
                            // Incomplete Frame
                            for (int i = byteCount; i < blockSize; i++)
                                buffer[i] = 0;
                        }
                        _vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                    }

                    _vClient.Wait(); // Wait for the Voice Client to finish sending data, as ffMPEG may have already finished buffering out a song, and it is unsafe to return now.

                   
                }
            }

        }
    }
}
