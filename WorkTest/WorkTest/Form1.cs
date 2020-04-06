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
        // Creates a new speech recognition engine needed for the recognition.
        SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();
        // Creates a new speech synthesizer needed for the system to talk back.
        SpeechSynthesizer speechSynth = new SpeechSynthesizer();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Makes the color of the form black when it loads.
            this.BackColor = Color.Black;
            // Creates a new library of words that the program understands.
            Choices voiceCommands = new Choices();
            // Adds the items below to the library of known commands.
            voiceCommands.Add(new string[] { "Attack", "Parry", "Block up", "Block right", "Block down", "Special 1", "Special 2", "Special 3", "Special 4", "What do you think of our project?", "Use stamina potion", "Use health potion", "Show items" });
            // Creates a new grammar builder for the program to be able to understand what te user is saying.
            GrammarBuilder builder = new GrammarBuilder();
            // Adds the library of known commands to the gammar builder so it can understand the user with these commands.
            builder.Append(voiceCommands);
            // Adds the gramar builder to the new grammar made in this program.
            Grammar grammar = new Grammar(builder);

            // Loads the grammar created async so the program runs more smoothly.
            recEngine.LoadGrammarAsync(grammar);
            // Sets the inut device to default, mine being my headset.
            recEngine.SetInputToDefaultAudioDevice();
            // Creates a new function where the speech is recognized.
            recEngine.SpeechRecognized += recEngine_SpeechRecognized;
        }

        // Function for using the commands
        private void recEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Sets the output speed of the program to 1, which is normal speed.
            speechSynth.Rate = 1;
            // Sets the output volume of the program to 60%.
            speechSynth.Volume = 60;

            // Foreach loop which checks the computer for all installed voices and sets it to the default.
            foreach (object obj in speechSynth.GetInstalledVoices())
            {
                var voice = (InstalledVoice)obj;
            }

            // Switch case which checks the result of the command and then does what it's told.
            switch (e.Result.Text)
            {
                // When the command is Block up.
                case "Block up":
                    SendKeys.SendWait("{UP}");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                // When the command is Block right.
                case "Block right":
                    SendKeys.SendWait("{RIGHT}");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                // When the command is Block down.
                case "Block down":
                    SendKeys.SendWait("{DOWN}");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                // When the command is Attack.
                case "Attack":
                    SendKeys.SendWait("(a)");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                // When the command is Parry.
                case "Parry":
                    SendKeys.SendWait("(p)");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                // When the command is Special 1.
                case "Special 1":
                    SendKeys.SendWait("(s)");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                // When the command is Special 2.
                case "Special 2":
                    SendKeys.SendWait("(d)");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                // When the command is Special 3.
                case "Special 3":
                    SendKeys.SendWait("(f)");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                // When the command is Special 4.
                case "Special 4":
                    SendKeys.SendWait("(g)");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                // When the command is Show items.
                case "Show items":
                    SendKeys.SendWait("(g)");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                // When the command is Use stamina potion.
                case "Use stamina potion":
                    richTextBox1.Text += "\n";
                    SendKeys.SendWait("(j)");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                // When the command is Use health potion.
                case "Use health potion":
                    SendKeys.SendWait("(h)");
                    richTextBox1.Text += "\n" + e.Result.Text;
                    break;

                // When the command is What do you think of our project.
                case "What do you think of our project?":
                    speechSynth.SpeakAsync("It is a very cool project, but i wouldn't expect less from my 2 favorite developers");
                    break;
            }
        }

        // Used as event when the Unmute button is clicked.
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            recEngine.RecognizeAsync(RecognizeMode.Multiple);
            pictureBox2.Enabled = true;
            pictureBox1.Enabled = false;
        }

        // Used as event when the mute button is clicked.
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            recEngine.RecognizeAsyncStop();
            pictureBox2.Enabled = false;
            pictureBox1.Enabled = true;
        }

        // Used as event when the dark theme button is clicked.
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.BackColor = Color.Black;
            pictureBox4.Enabled = true;
            pictureBox3.Enabled = false;
        }

        // Used as event when the light theme button is clicked.
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
