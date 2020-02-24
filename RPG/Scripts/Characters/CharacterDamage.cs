using Godot;
using System;

public class CharacterDamage : Node
{
    private Stats stats;
    private bool player = true;
    private bool chooseGuardDir = false;
    private int guardDir = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        stats = GetParent().GetNode("Stats") as Stats;
        
        if (stats.GetWeapon().GetAtk() > 20)
        {
            player = false;
        }
    }

    public void ReceiveDamage(Stats attackerStats)
    {
        float luk = stats.GetLuk() / 10;
        Random rand = new Random();
        float num = rand.Next() % 100;

        bool missed = false;

        if (num <= luk)
        {
            missed = true;
        }
        else
        {
            missed = false;
        }

        GD.Print(missed + " " + luk + " " + num);

        chooseGuardDir = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (chooseGuardDir)
        {
            GuardDirection();
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
}
