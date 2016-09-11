using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Discord.API;
using Discord.Audio;
using Discord.Commands;
using Discord.Modules;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.IO;
using VkNet;
using VkNet.Model.RequestParams;
using VkNet.Model.Attachments;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace DiscordBot
{

    public partial class Form1 : Form
    {
        public VkApi api = new VkApi();
        public string combobox;
        private Discord.DiscordClient DC;

        public Form1()
        {
            InitializeComponent();
            //System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            button1.Text = "Exec Audio Config";
            button2.Text = "Login";
            button3.Text = "Start Streaming";
            timer1.Enabled = false;
            timer1.Interval = 2 * 10 * 1000;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            string path = AppDomain.CurrentDomain.BaseDirectory;
            string[] files = System.IO.Directory.GetFiles(path, "*.mp3").OrderBy(d => new FileInfo(d).CreationTime).ToArray();

            //string[] files = System.IO.Directory.GetFiles(path, "*.mp3");

            foreach (string file in files)
            {
                comboBox1.Items.Add(Path.GetFileName(file));
            }

            //comboBox1.SelectedIndex = 0;


            string login = "";
            string pass = "";
            ulong appID = ;

            VkNet.Enums.Filters.Settings scope = VkNet.Enums.Filters.Settings.All;
            ApiAuthParams authorize = new ApiAuthParams();

            authorize.Login = login;
            authorize.Password = pass;
            authorize.ApplicationId = appID;
            authorize.Settings = scope;
            api.Authorize(authorize);

            DC = new Discord.DiscordClient();

            DC.Connect("tokengoeshere");

            DC.UsingCommands(x =>
           {
               x.PrefixChar = '!';
               x.AllowMentionPrefix = true;
           });

            CreateCommand();
            combobox = comboBox1.Text;
        }



        public void CreateCommand()
        {
            var cService = DC.GetService<CommandService>();
            
            cService.CreateCommand("test").Do(async (e) =>
            {
                await e.Channel.SendMessage("test");
            });

            cService.CreateCommand("song").Do(async (e) =>
            {
                await e.Channel.SendMessage(combobox);
            });
        }



        public void DownloadAudio()
        {
            MessagesGetParams param1 = new MessagesGetParams();

            param1.Out = VkNet.Enums.MessageType.Received;
            param1.Count = 1;

            VkNet.Model.MessagesGetObject messages = api.Messages.Get(param1);
            foreach (VkNet.Model.Message msg in messages.Messages)
            {
                if (msg.Body == "!req" && msg.ReadState == VkNet.Enums.MessageReadState.Unreaded)
                {
                    string html = "https://api.vk.com/method/messages.get?count=1&access_token=TOKEN&v=V";

                    string text = GET(html);

                    JObject o = JObject.Parse(text);
                    try
                    {
                        string link = (string)o.SelectToken("response[1].attachment.audio.url");
                        string artist = (string)o.SelectToken("response[1].attachment.audio.artist");
                        string title = (string)o.SelectToken("response[1].attachment.audio.title");
                        
                        if (!File.Exists(@"C:\Users\Igor\Documents\Visual Studio 2015\Projects\DiscordBot\DiscordBot\bin\Debug" + artist + " - " + title + ".mp3"))
                        {
                            WebClient wc = new WebClient();
                            wc.DownloadFile(link, artist + " - " + title + ".mp3");
                        }
                    }
                    catch (Exception ex)
                    {
                        richTextBox1.Text = richTextBox1.Text + Environment.NewLine + DateTime.Now.ToString() + ": \n" + ex.Message.ToString() + "\n";
                    }
                }
            }
        }
        public async Task SendAudio(string filePath)
        {

            /*if (this.InvokeRequired)
            {
                this.Invoke(new System.Action(() =>
                {
                    DownloadAudio();
                }));
            }*/
            
            for (;;)
            {

              

                if (this.InvokeRequired)
                {
                    this.Invoke(new System.Action(() =>
                    {
                        filePath = comboBox1.Text;
                        combobox = comboBox1.Text;
                    }));
                }

                if (this.InvokeRequired)
                {
                    this.Invoke(new System.Action(() =>
                    {
                        var server = DC.GetServer(198072128673677312);
                        var channel = server.GetChannel(198072128673677312);
                        channel.SendMessage("Now playing: " + comboBox1.Text);
                    }));
                }
               

                //filePath = comboBox1.Text;
                var voiceChannel = DC.FindServers("Do_Konca|Gaming").FirstOrDefault().VoiceChannels.ElementAt(1);

                var _vClient = await DC.GetService<AudioService>().Join(voiceChannel);

                var channelCount = DC.GetService<AudioService>().Config.Channels; 
                var OutFormat = new WaveFormat(48000, 16, channelCount); 
                using (var MP3Reader = new Mp3FileReader(filePath)) 
                using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat))
                {
                    resampler.ResamplerQuality = 60; 
                    int blockSize = OutFormat.AverageBytesPerSecond / 50;
                    byte[] buffer = new byte[blockSize];
                    int byteCount;

                    while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) 
                    {
                        if (byteCount < blockSize)
                        {
                           
                            for (int i = byteCount; i < blockSize; i++)
                                buffer[i] = 0;
                        }
                        _vClient.Send(buffer, 0, blockSize); 
                    }

                    _vClient.Wait();

                    if (this.InvokeRequired)
                    {
                        this.Invoke(new System.Action(() =>
                        {
                            string path = AppDomain.CurrentDomain.BaseDirectory;
                            string[] files = System.IO.Directory.GetFiles(path, "*.mp3").OrderBy(d => new FileInfo(d).CreationTime).ToArray();
                            comboBox1.Items.Clear();
                            foreach (string file in files)
                            {
                                comboBox1.Items.Add(Path.GetFileName(file));
                            }

                            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
                            combobox = comboBox1.Text;
                        }));

                    }
                }
            }

        }



        private void button1_Click(object sender, EventArgs e)
        {
            //DC.ExecuteAndWait(async () =>
            //{
            //    await DC.Disconnect();
            //});
            //DC.SetGame("Testing Bot");


            DC.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing; // Tells the AudioService that we will only be sending audio
            });

            //var voiceChannel = DC.FindServers("Do_Konca|Gaming").FirstOrDefault().VoiceChannels.ElementAt(1);

            //var _vClient = DC.GetService<AudioService>().Join(voiceChannel);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            combobox = comboBox1.Text;
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            string filePath = comboBox1.Text;
            await Task.Run(() => SendAudio(filePath));
        }

        private string GET(string Url)
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(Url);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.Stream stream = resp.GetResponseStream();
            System.IO.StreamReader sr = new System.IO.StreamReader(stream, Encoding.UTF8);
            string Out = sr.ReadToEnd();
            sr.Close();
            return Out;
        }

    private void button5_Click(object sender, EventArgs e)
        {
            DownloadAudio();
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string[] files = System.IO.Directory.GetFiles(path, "*.mp3").OrderBy(d => new FileInfo(d).CreationTime).ToArray();
            comboBox1.Items.Clear();
            foreach (string file in files)
            {
                comboBox1.Items.Add(Path.GetFileName(file));
            }

            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DownloadAudio();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }
    }
}
