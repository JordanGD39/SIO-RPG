using Godot;
using System;
using System.Threading.Tasks;

public class GameManager : Node
{
    [Export] private int voiceControl = 1;
    public int GetVoiceControl() {return voiceControl;} public void SetVoiceControl(bool a) {if(a) {voiceControl = 4;} else {voiceControl = 1;}}

    [Export] private int difficulty = 0;
    public int GetDifficulty() {return difficulty;} public void SetDifficulty(int a) {difficulty = a; GetNode<GUI>("UI").HideSetupMenu();}

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
