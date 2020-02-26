using Godot;
using System;
using System.Collections.Generic;

public class CharacterDamage : Node
{
    private Stats stats;
    private Stats attackerStats;
    private bool playerControl = true;
    private bool chooseGuardDir = false;
    private bool choosing = false;
    private bool missed = false;
    private int guardDir = 0;
    public int GetEnemyAttackDirection() {return guardDir;}
    private Timer timer;
    private bool alreadyChosen = false;
    private AnimatedSprite guard;
    private BattleManager battleManager;
    private TextureProgress healthBar;
    public void SetHealthBar(TextureProgress a) {healthBar = a;}
    private TextureProgress staminaBar;
    public void SetStaminaBar(TextureProgress a) {staminaBar = a;}
    private Sprite marker;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {        
        marker = GetParent().GetNode("Marker") as Sprite;
        stats = GetParent().GetNode("Stats") as Stats;
        stats.SetMaxHealth(stats.GetHealth());
        battleManager = GetParent().GetParent() as BattleManager;
    }

    public void GetMyHealthBar()
    {
        Node ui = GetParent().GetParent().GetParent().GetNode("UI");

        Player playerScript = GetParent() as Player;

        GD.Print(stats.GetCharName());
        guard = GetParent().GetNode("Guard") as AnimatedSprite;
        guard.Visible = false;

        if (playerScript == null)
        {
            playerControl = false;            
            healthBar = ui.GetNode("BossHP").GetChild(0).GetChild((GetIndex() - (battleManager.GetPlayers().Count - 1)) - 1).GetNode("HealthBar") as TextureProgress;
            staminaBar = healthBar.GetParent().GetNode("StaminaBar") as TextureProgress;
        }
    }
    public void StartGuardSequence(Stats attackerStatsTemp)
    {
        if (marker != null)
        {
            marker.Visible = false;
        }
        
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

        GD.Print("Missed: " + missed + " " + luk + " " + num + " is player: " + playerControl);

        timer = new Timer();
        timer.WaitTime = 1f;
        timer.OneShot = true;
        AddChild(timer);
        timer.Start();

        choosing = true;
        guard.Visible = true;    
        if (!playerControl)
        {
            EnemyGuardChoose();
        }            
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

        if (choosing)
        {            
            if (timer.TimeLeft == 0)
            {
                timer.Stop();
                timer.QueueFree();
                choosing = false;
                alreadyChosen = false;
                ReceiveDamage();
            } 
            else if (timer.TimeLeft < 0.6f && !playerControl)
            {
                if (!alreadyChosen)
                {
                    EnemyGuardChoose();
                    alreadyChosen = true;
                }                
            }           
        }
    }

    public void EnemyGuardChoose()
    {        
        guard.Visible = true;
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
            GD.Print("Attacker atk: " + attackerStats.GetAtk() + " | Defender def: " + stats.GetDef());            
            damage = attackerStats.GetAtk() - stats.GetDef();
            if (damage < 0)
            {
                damage = 0;
            }
        }    
        else
        {
            GD.Print("Missed!");
        }     
        Player player = attackerStats.GetParent() as Player;
        if (player != null)
        {
            if (attackerStats.GetParent<Player>().GetAttackDirection() == guardDir)
            {
                damage *= 0.5f;
                GD.Print("You hit their shield!");
            } 
        }
        else
        {
            if (attackerStats.GetParent().GetNode<CharacterDamage>("Damage").GetEnemyAttackDirection() == GetParent<Player>().GetAttackDirection())
            {
                damage *= 0.5f;
                GD.Print("You hit their shield!");
            } 
        }
        
        GD.Print(attackerStats.GetCharName() + " did " + damage + " damage to " + stats.GetCharName());
        
        stats.SetHealth(stats.GetHealth() - Mathf.RoundToInt(damage));        

        float maxHealth = stats.GetMaxHealth();
        float health = stats.GetHealth();

        healthBar.Value = health / maxHealth * 100;

        GD.Print(stats.GetCharName() + " has " + stats.GetHealth() + " HP left");
        guard.Visible = false;
        attackerStats.GetParent().GetNode<AnimatedSprite>("Guard").Visible = false;
        battleManager.NextTurn(playerControl);

        if (stats.GetHealth() <= 0)
        {
            battleManager.TakeMeOutList(GetParent(), playerControl);
        }
    }
}
