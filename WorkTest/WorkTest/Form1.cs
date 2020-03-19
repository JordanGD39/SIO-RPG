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
                    MessageBox.Show("UP");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                case "Block right":
                    SendKeys.SendWait("{RIGHT}");
                    MessageBox.Show("RIGHT");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                case "Block down":
                    SendKeys.SendWait("{DOWN}");
                    MessageBox.Show("DOWN");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                case "Attack":
                  //  SendKeys.SendWait("{ENTER}");
                    MessageBox.Show("Attacking");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                case "Parry":
                    SendKeys.SendWait("(p)");
                    MessageBox.Show("PARIED!");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                case "Deflect":
                    SendKeys.SendWait("(e)");
                    MessageBox.Show("DEFLECTING SINCE 1942");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                case "Special":
                    SendKeys.SendWait("{TAB}");
                    MessageBox.Show("USING SPECIAL");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                case "What do you think of or project?":
                    speechSynth.SpeakAsync("It is a very cool project, but i wouldn't expect less from my 2 favorite developers");
                    break;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            recEngine.RecognizeAsync(RecognizeMode.Multiple);
            pictureBox2.Enabled = true;
            pictureBox1.Enabled = false;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            recEngine.RecognizeAsyncStop();
            pictureBox2.Enabled = false;
            pictureBox1.Enabled = true;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.BackColor = Color.Black;
            pictureBox4.Enabled = true;
            pictureBox3.Enabled = false;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.BackColor = Color.White;
            pictureBox4.Enabled = false;
            pictureBox3.Enabled = true;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            
        }
    }
}
