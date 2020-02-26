using Godot;
using System;

public class EnemyAI : KinematicBody2D
{
    private BattleManager battleManager;
    private Stats stats;
    private CharacterDamage damageScript;
    private Timer timer;
    private bool timerStarted = false;
    private Skill chosenSkill;

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
        int count = battleManager.GetPlayers().Count;

        Random rand = new Random();
        int num = rand.Next() % count;

        battleManager.GetPlayers()[num].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);
        damageScript.EnemyGuardChoose();
        timer = new Timer();
        timer.WaitTime = 0.4f;
        timer.OneShot = true;
        AddChild(timer);
        timer.Start();
        timerStarted = true;
    }
}
