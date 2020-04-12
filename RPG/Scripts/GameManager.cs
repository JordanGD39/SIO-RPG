using Godot;
using System;
using System.Threading.Tasks;

public class GameManager : Node
{
    [Export] private int voiceControl = 1;
    public int GetVoiceControl() {return voiceControl;}

    [Export] private int difficulty = 0;
    public int GetDifficulty() {return difficulty;}

    private Node audioNode;
    public Node GetAudioNode() {return audioNode;}

    public override void _Ready()
    {
        audioNode = GetNode("SFX");
    }
    public async Task LongRunningOperationAsync(int delay)
    {
        await Task.Delay(delay);
    }
}
