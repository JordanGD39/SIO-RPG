using Godot;
using System;

public class GameManager : Node
{
    [Export] private int voiceControl = 1;
    public int GetVoiceControl() {return voiceControl;}

    [Export] private int difficulty = 0;
    public int GetDifficulty() {return difficulty;}
}
