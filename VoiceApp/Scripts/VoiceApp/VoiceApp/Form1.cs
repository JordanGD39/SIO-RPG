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
    public partial class VoiceApp : Form
    {
        private bool isSpeaking = false;
        SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();
        public VoiceApp()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isSpeaking = true;
            if (isSpeaking)
            {
                recEngine.RecognizeAsync(RecognizeMode.Multiple);
                MessageBox.Show("started");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            isSpeaking = false;
            if (!isSpeaking)
            {
                recEngine.RecognizeAsyncStop();
                MessageBox.Show("stoped");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Choices voiceCommands = new Choices();
            voiceCommands.Add(new string[] { "Block", "Attack", "Parry", "Deflect", "Special", "Start" });
            GrammarBuilder builder = new GrammarBuilder();
            builder.Append(voiceCommands);
            Grammar grammar = new Grammar(builder);

            recEngine.LoadGrammarAsync(grammar);
            recEngine.SetInputToDefaultAudioDevice();
            recEngine.SpeechRecognized += RecognizedSpeech;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VoiceApp());
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
                case "Block":
                //    rBox.Text = "Epic Block";
                    speechSynth.SpeakAsync("Epic block");
                    //   a.KeyValue = Keys.Up;
                    SendKeys.SendWait("{UP}");
                    break;

                case "Attack":
                    rBox.Text = "Attack is the best defence";
                    speechSynth.SpeakAsync("Attack is the best deffence");
                    break;

                case "Parry":
                    rBox.Text = "PARRY";
                    speechSynth.SpeakAsync("PARRY!");
                    break;

                case "Deflect":
                    rBox.Text = "Look at that deflect";
                    speechSynth.SpeakAsync("Look at that deflect");
                    break;

                case "Special":
                    rBox.Text = "You're special";
                    speechSynth.SpeakAsync("You're special");
                    break;

                case "Start":
                    SendKeys.SendWait("{ESC}");
                    rBox.Text = "Opening menu";
                    break;
            }
        }
        }
    } 
