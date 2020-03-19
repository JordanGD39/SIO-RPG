using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;

namespace VoiceApp
{
    public partial class BuggySpeech : Form
    {
        SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();
        private bool isSpeaking = false;


        public BuggySpeech()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.Show();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            recEngine.RecognizeAsync(RecognizeMode.Multiple);
            button2.Enabled = true;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            recEngine.RecognizeAsyncStop();
            button2.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Choices voiceCommands = new Choices();
            voiceCommands.Add(new string[] { "Block up", "Block right", "Block down",  " Attack", "Parry", "Deflect", "Special", "Start" });
            GrammarBuilder builder = new GrammarBuilder();
            builder.Append(voiceCommands);
            Grammar grammar = new Grammar(builder);

            recEngine.LoadGrammarAsync(grammar);
            recEngine.SetInputToDefaultAudioDevice();
            recEngine.SpeechRecognized += RecognizedSpeech;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BuggySpeech());
        }

        private static void RecognizedSpeech(object sender, SpeechRecognizedEventArgs e)
        {
            RichTextBox rBox = new RichTextBox();
            SpeechSynthesizer speechSynth = new SpeechSynthesizer();
            speechSynth.Rate = 1;
            speechSynth.Volume = 60;

            foreach (object obj in speechSynth.GetInstalledVoices())
            {
                var voice = (InstalledVoice)obj;
            }

            switch (e.Result.Text)
            {
                case "Block up":
                    speechSynth.SpeakAsync("block up");
                    SendKeys.SendWait("{UP}");
                    break;

                case "Block right":
                    speechSynth.SpeakAsync("block right");
                    SendKeys.SendWait("{RIGHT}");
                    break;

                case "Block down":
                    speechSynth.SpeakAsync("block down");
                    SendKeys.SendWait("{DOWN}");
                    break;

                case "Attack":
                    speechSynth.SpeakAsync("Attack is the best defence");
                    break;

                case "Parry":
                    speechSynth.SpeakAsync("PARRY!");
                    break;

                case "Deflect":
                    speechSynth.SpeakAsync("Look at that deflect");
                    break;

                case "Special":
                    speechSynth.SpeakAsync("You're special");
                    break;

                case "Start":
                    SendKeys.SendWait("{ESC}");
                    break;
            }
        }
    }
  } 
