using Godot;
using System;
using System.Collections.Generic;
using System.Speech.Recognition;

public class VoiceControl : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    private SpeechRecognitionEngine recEngine;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        recEngine = new SpeechRecognitionEngine();
        Choices voiceCommands = new Choices();
        voiceCommands.Add(new string[] { "Block", "Attack", "Parry", "Deflect", "Special" });
        GrammarBuilder builder = new GrammarBuilder();
        builder.Append(voiceCommands);
        Grammar grammar = new Grammar(builder);

        recEngine.LoadGrammarAsync(grammar);
        recEngine.SetInputToDefaultAudioDevice();
        recEngine.SpeechRecognized += RecEngine_SpeechRecognized;
        recEngine.RecognizeAsync(RecognizeMode.Multiple);
    }

    private void RecEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
        switch(e.Result.Text)
        {
            case "Block":
                GD.Print("Blocked");
                break;

            case "Attack":
                GD.Print("Attacking");
                break;

            case "Parry":
                GD.Print("PARRY");
                break;

            case "Deflect":
                GD.Print("Deflected");
                break;

            case "Special":
                GD.Print("Using special");
                break;
        }
    }
}
