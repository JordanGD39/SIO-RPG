using Godot;
using System;
using System.Collections.Generic;
using System.Speech.Recognition;

public class VoiceControl : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();
        Choices voiceCommands = new Choices();
        voiceCommands.Add(new string[] { "Block", "Attack", "Parry", "Deflect", "Special" });
        GrammarBuilder builder = new GrammarBuilder();
        builder.Append(voiceCommands);
        Grammar grammar = new Grammar(builder);

        recEngine.LoadGrammarAsync(grammar);
        recEngine.SetInputToDefaultAudioDevice();
        recEngine.SpeechRecognized += RecEngine_SpeechRecognized;
    }

    private void RecEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
        switch(e.Result.Text)
        {
            case "Block":

                break;

            case "Attack":

                break;

            case "Parry":

                break;

            case "Deflect":

                break;

            case "Special":

                break;
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
       
  }
}
