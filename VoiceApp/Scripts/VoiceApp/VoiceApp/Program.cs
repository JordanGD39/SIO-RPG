using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Diagnostics;
using System.ComponentModel;

namespace VoiceApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();
            Choices voiceCommands = new Choices();
            voiceCommands.Add(new string[] { "Block up", "Block right", "Block down",  "Next", "Previous",   "Attack", "Parry", "Deflect", "Special", "Start" });
            GrammarBuilder builder = new GrammarBuilder();
            builder.Append(voiceCommands);
            Grammar grammar = new Grammar(builder);

            recEngine.LoadGrammarAsync(grammar);
            recEngine.SetInputToDefaultAudioDevice();
            recEngine.SpeechRecognized += RecognizedSpeech;
         //   recEngine.RecognizeAsync(RecognizeMode.Multiple);
           
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BuggySpeech());
        }

        private static void RecognizedSpeech(object sender, SpeechRecognizedEventArgs e)
        {
        SpeechSynthesizer speechSynth = new SpeechSynthesizer();
        RichTextBox rBox = new RichTextBox();
            speechSynth.Rate = 1;
            speechSynth.Volume = 60;

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

                case "Next":
                    SendKeys.SendWait("{DOWN}");
                    break;

                case "Previous":
                    SendKeys.SendWait("{UP}");
                    break;

                case "Block down":
                    speechSynth.SpeakAsync("block down");
                    SendKeys.SendWait("{DOWN}");
                    break;

                case "Attack":
                    MessageBox.Show("Attack is the best deffence");
                    speechSynth.SpeakAsync("Attack is the best defence");
                    break;

                case "Parry":
                    MessageBox.Show("PARRY");
                    speechSynth.SpeakAsync("PARRY!");
                    break;

                case "Deflect":
                    MessageBox.Show("Look at that deflect");
                    speechSynth.SpeakAsync("Look at that deflect");
                    SendKeys.SendWait("e");
                    break;

                case "Special":
                    MessageBox.Show("You're special");
                    speechSynth.SpeakAsync("You're special");
                    break;

                case "Start":
                    MessageBox.Show("Opening menu");
                    SendKeys.SendWait("{ESC}");
                    break;
            }
        }
    }
}
