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

namespace WorkTest
{
    public partial class Form1 : Form
    {
        private void button1_Click(object sender, EventArgs e)
        {
            recEngine.RecognizeAsync(RecognizeMode.Multiple);
            button2.Enabled = true;
            button1.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            recEngine.RecognizeAsyncStop();
            button2.Enabled = false;
            button1.Enabled = true;
        }

        SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();
        SpeechSynthesizer speechSynth = new SpeechSynthesizer();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Choices voiceCommands = new Choices();
            voiceCommands.Add(new string[] { "Attack", "Deflect", "Parry", "Block up", "Block right", "Block down", "Special", "What do you think of or project?" });
            GrammarBuilder builder = new GrammarBuilder();
            builder.Append(voiceCommands);
            Grammar grammar = new Grammar(builder);

            recEngine.LoadGrammarAsync(grammar);
            recEngine.SetInputToDefaultAudioDevice();
            recEngine.SpeechRecognized += recEngine_SpeechRecognized;
        }

        private void recEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            speechSynth.Rate = 1;
            speechSynth.Volume = 60;

            foreach (object obj in speechSynth.GetInstalledVoices())
            {
                var voice = (InstalledVoice)obj;
            }

            switch (e.Result.Text)
            {
                case "Block up":
                    SendKeys.SendWait("{UP}");
                    speechSynth.SpeakAsync("UP");
                  //  MessageBox.Show("UP");
                    break;

                case "Block right":
                    SendKeys.SendWait("{RIGHT}");
                    MessageBox.Show("RIGHT");
                    break;

                case "Block down":
                    SendKeys.SendWait("{DOWN}");
                    MessageBox.Show("DOWN");
                    break;

                case "Attack":
                  //  SendKeys.SendWait("{ENTER}");
                    MessageBox.Show("Attacking");
                    break;

                case "Parry":
                    SendKeys.SendWait("(p)");
                    MessageBox.Show("PARIED!");
                    break;

                case "Deflect":
                    SendKeys.SendWait("(e)");
                    MessageBox.Show("DEFLECTING SINCE 1942");
                    break;

                case "Special":
                    SendKeys.SendWait("{TAB}");
                    MessageBox.Show("USING SPECIAL");
                    break;

                case "What do you think of or project?":
                    speechSynth.SpeakAsync("It is a very cool project, but i wouldn't expect less from my 2 favorite developers");
                    break;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.BackColor = Color.Black;
            button4.Enabled = true;
            button3.Enabled = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.BackColor = Color.White;
            button4.Enabled = false;
            button3.Enabled = true;
        }
    }
}
