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
            voiceCommands.Add(new string[] { "Block", "Attack", "Parry", "Deflect", "Special", "Start" });
            GrammarBuilder builder = new GrammarBuilder();
            builder.Append(voiceCommands);
            Grammar grammar = new Grammar(builder);

            recEngine.LoadGrammarAsync(grammar);
            recEngine.SetInputToDefaultAudioDevice();
            recEngine.SpeechRecognized += RecognizedSpeech;
            recEngine.RecognizeAsync(RecognizeMode.Multiple);
           
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VoiceApp());
        }

        private static void RecognizedSpeech(object sender, SpeechRecognizedEventArgs e)
        {
        SpeechSynthesizer speechSynth = new SpeechSynthesizer();
        RichTextBox rBox = new RichTextBox();
            speechSynth.Rate = 1;
            speechSynth.Volume = 60;

            switch (e.Result.Text)
            {
                case "Block":
                //    MessageBox.Show("Epic Block");
                    speechSynth.SpeakAsync("Epic block");
                    SendKeys.SendWait("{UP}");
                    SendKeys.SendWait("(e)");
                    break;

                case "Attack":
                    MessageBox.Show("Attack is the best deffence");
                    speechSynth.SpeakAsync("Attack is the best deffence");
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
