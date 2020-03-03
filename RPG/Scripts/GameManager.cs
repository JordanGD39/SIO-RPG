using Godot;
using System;

public class GameManager : Node
{
    [Export] private bool voiceControl = false;
    public bool GetVoiceControl() {return voiceControl;}

    [Export] private int difficulty = 0;
    public int GetDifficulty() {return difficulty;}
}
