using Godot;
using System;

public class CharacterDamage : Node
{
    private Stats stats;
    private Stats attackerStats;
    private bool player = true;
    private bool chooseGuardDir = false;
    private bool choosing = false;
    private bool missed = false;
    private int guardDir = 0;
    private Timer timer;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        stats = GetParent().GetNode("Stats") as Stats;
        Player playerScript = GetParent() as Player;

        if (playerScript == null)
        {
            player = false;
        }
    }

    public void StartGuardSequence(Stats attackerStatsTemp)
    {
        attackerStats = attackerStatsTemp;
        float luk = stats.GetLuk() / 10;
        Random rand = new Random();
        float num = rand.Next() % 100;

        if (num <= luk)
        {
            missed = true;
        }
        else
        {
            missed = false;
        }

        GD.Print(missed + " " + luk + " " + num + " is player: " + player);
        timer = GetParent().GetNode<Timer>("GuardTimer");
        timer.Start();
        choosing = true;
        if (player)
        {
            chooseGuardDir = true;
        }        
        else
        {
            EnemyGuardChoose();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (chooseGuardDir && player)
        {
            GuardDirection();
        }

        if (choosing && timer.IsStopped())
        {
            choosing = false;
            ReceiveDamage();
        }
    }

    private void GuardDirection()
    {

        if (Input.IsActionPressed("ui_up"))
        {
            guardDir = 1;
        }
        else if (Input.IsActionPressed("ui_down"))
        {
            guardDir = -1;
        }
        else
        {
            guardDir = 0;
        } 
    }

    private void EnemyGuardChoose()
    {        
        Random rand = new Random();
        guardDir = rand.Next() % 3 - 1;

        GD.Print(guardDir);
    }

    private void ReceiveDamage()
    {
        EnemyGuardChoose();

        float damage = 0;

        if (!missed)
        {
            damage = attackerStats.GetAtk() - stats.GetDef();
        }         

        if (attackerStats.GetParent<Player>().GetAttackDirection() == guardDir)
        {
            damage /= 2;
        }
    }
}
