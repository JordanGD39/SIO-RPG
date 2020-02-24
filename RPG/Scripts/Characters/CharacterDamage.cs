using Godot;
using System;
using System.Collections.Generic;

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
    private bool alreadyChosen = false;
    private AnimatedSprite guard;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        stats = GetParent().GetNode("Stats") as Stats;

        if (stats.GetCharName() == "Boss")
        {
            guard = GetParent().GetNode("Guard") as AnimatedSprite;
            guard.Visible = false;
        } 

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
        guard.Visible = true;    
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

        if (choosing)
        {
            GD.Print("Busy..." + timer.TimeLeft);
            if (timer.TimeLeft < 0.1f)
            {
                if (!alreadyChosen)
                {
                    EnemyGuardChoose();
                    alreadyChosen = true;
                }                
            }
            if (timer.TimeLeft == 0)
            {
                timer.Stop();
                choosing = false;
                ReceiveDamage();
            }            
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

        switch (guardDir)
        {            
            case -1:
            guard.Play("Down");
            break;
            case 0:
            guard.Play("Forward");
            break;
            case 1:
            guard.Play("Up");
            break;
        }
    }

    private void EnemyGuardChoose()
    {        
        Random rand = new Random();
        guardDir = rand.Next() % 3 - 1;

        GD.Print(guardDir);

        switch (guardDir)
        {            
            case -1:
            guard.Play("Down");
            break;
            case 0:
            guard.Play("Forward");
            break;
            case 1:
            guard.Play("Up");
            break;
        }
    }

    private void ReceiveDamage()
    {
        float damage = 0;

        if (!missed)
        {
            damage = attackerStats.GetAtk() - stats.GetDef();
        }         

        if (attackerStats.GetParent<Player>().GetAttackDirection() == guardDir)
        {
            damage /= 2;
            GD.Print("You hit their shield!");
        }

        GD.Print(attackerStats.GetCharName() + " did " + damage + " damage to " + stats.GetCharName());
    }
}
