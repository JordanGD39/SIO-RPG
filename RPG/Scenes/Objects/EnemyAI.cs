using Godot;
using System;

public class EnemyAI : KinematicBody2D
{
    private BattleManager battleManager;
    private Stats stats;
    private CharacterDamage damageScript;
    private Timer timer;

    private bool timerStarted = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        battleManager = GetParent() as BattleManager;
        stats = GetNode("Stats") as Stats;
        damageScript = GetNode("Damage") as CharacterDamage;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (timer != null && timerStarted)
        {
            GD.Print("Boss waiting... " + timer.TimeLeft);
            if (timer.TimeLeft == 0)
            {
                timerStarted = false;
                timer.Stop();
                timer.QueueFree();
                damageScript.EnemyGuardChoose();
            }            
        }
    }

    public void MyTurn()
    {
        int count = battleManager.GetPlayers().Count - 1;

        Random rand = new Random();
        int num = rand.Next() % count;

        battleManager.GetPlayers()[num].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats);
        damageScript.EnemyGuardChoose();
        timer = new Timer();
        timer.WaitTime = 0.6f;
        timer.OneShot = true;
        AddChild(timer);
        timer.Start();
        timerStarted = true;
    }
}