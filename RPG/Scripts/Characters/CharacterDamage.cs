using Godot;
using System;
using System.Collections.Generic;

public class CharacterDamage : Node
{
    private Stats stats;
    private Player player;
    private EnemyAI ai;
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
    public TextureProgress GetStaminaBar() {return staminaBar;} public void SetStaminaBar(TextureProgress a) {staminaBar = a;}
    private Sprite marker;
    private Skill skillThatAttackedMe;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {        
        ai = GetParent() as EnemyAI;
        player = GetParent() as Player;
        marker = GetParent().GetNode("Marker") as Sprite;
        stats = GetParent().GetNode("Stats") as Stats;
        stats.SetMaxHealth(stats.GetHealth());
        battleManager = GetParent().GetParent() as BattleManager;
        if (player == null)
        {
            playerControl = false;
        }
    }

    public void GetMyGuard()
    {
        guard = GetParent().GetNode("Guard") as AnimatedSprite;
        guard.Visible = false;
    }
    public void StartGuardSequence(Stats attackerStatsTemp, Skill skill, Node allSpecials)
    {
        skillThatAttackedMe = skill;

        marker.Visible = false;
        
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
        else
        {
            player.SetChooseAttackDir(true);            
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
        int atkOrMag = 0;

        if (skillThatAttackedMe != null)
        {
            if (skillThatAttackedMe.GetAtk() > 0)
            {
                atkOrMag = skillThatAttackedMe.GetAtk();
            }
            else if (skillThatAttackedMe.GetMag() > 0)
            {
                atkOrMag = skillThatAttackedMe.GetMag();
            }
        }        

        float attackerAtk = attackerStats.GetAtk() + atkOrMag;

        if (attackerStats.GetStamina() <= 0)
        {
            attackerAtk *= 0.6f;
            attackerAtk = Mathf.Round(attackerAtk);
        }

        float damage = 0;
        if (!missed)
        {
            GD.Print("Attacker atk: " + attackerAtk + " | Defender def: " + stats.GetDef());            
            damage = attackerAtk - stats.GetDef();
            if (damage < 0)
            {
                damage = 0;
            }
        }    
        else
        {
            GD.Print("Missed!");
        }     

        Player attacker = attackerStats.GetParent() as Player;
        if (attacker != null)
        {
            if (attackerStats.GetParent<Player>().GetAttackDirection() == guardDir)
            {
                damage *= 0.5f;
                GD.Print("You hit their shield!");
                attacker.SetChooseAttackDir(false);
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

        healthBar.Value = (float)stats.GetHealth() / (float)stats.GetMaxHealth() * 100;

        GD.Print(stats.GetCharName() + " has " + stats.GetHealth() + " HP left");
        guard.Visible = false;
        attackerStats.GetParent().GetNode<AnimatedSprite>("Guard").Visible = false;

        if (skillThatAttackedMe == null || !skillThatAttackedMe.GetAttackAll())
        {
           battleManager.NextTurn();
        }
        else
        {
            battleManager.SetAttacksForNextTurn(battleManager.GetAttacksForNextTurn() + 1);
            battleManager.CheckIfNextTurn(skillThatAttackedMe.GetTeam());
        }        

        if (stats.GetHealth() <= 0)
        {
            battleManager.TakeMeOutList(GetParent(), playerControl);
        }
    }
    public void Support(Skill skill, Node allSpecials)
    {
        if (marker != null)
        {
            marker.Visible = false;
        }

        if (skill.GetHeal() && stats.GetHealth() < stats.GetMaxHealth())
        {
            stats.SetHealth(stats.GetHealth() + skill.GetMag());
            if (stats.GetHealth() > stats.GetMaxHealth())
            {
                stats.SetHealth(stats.GetMaxHealth());
            }
            healthBar.Value = (float)stats.GetHealth() / (float)stats.GetMaxHealth() * 100;
            GD.Print("Health: " + stats.GetHealth());
        }        
        if (stats.GetAtk() <= stats.GetMaxAtk())
        {
           stats.SetAtk(stats.GetAtk() + skill.GetStatBonusAtk()); 
           stats.SetAtkCounter(3);
           GD.Print("ATK: " + stats.GetAtk());
        }
        if (stats.GetMag() <= stats.GetMaxMag())
        {
            stats.SetMag(stats.GetMag() + skill.GetStatBonusMag());
            GD.Print("MAG: " + stats.GetMag());
        }
        if (stats.GetDef() <= stats.GetMaxDef())
        {
            stats.SetDef(stats.GetDef() + skill.GetStatBonusDef());
            stats.SetDefCounter(3);
            GD.Print("DEF: " + stats.GetDef());
        }
        if (stats.GetRes() <= stats.GetMaxRes())
        {
            stats.SetRes(stats.GetRes() + skill.GetStatBonusRes());
            GD.Print("RES: " + stats.GetRes());
        }
        if (stats.GetLuk() <= stats.GetMaxLuk())
        {
            stats.SetLuk(stats.GetLuk() + skill.GetStatBonusLuk());
            GD.Print("LUK: " + stats.GetLuk());
        }
        if (stats.GetSpd() <= stats.GetMaxSpd())
        {
            stats.SetSpd(stats.GetSpd() + skill.GetStatBonusSpd());
            stats.SetSpdCounter(3);
            GD.Print("SPD: " + stats.GetSpd());
        }    
        marker.Visible = false;
        if (!skill.GetAttackAll())
        {
           battleManager.NextTurn(); 
        }
        else
        {
            battleManager.SetAttacksForNextTurn(battleManager.GetAttacksForNextTurn() + 1);
            battleManager.CheckIfNextTurn(skill.GetTeam());
        }
    }

    public void Debuff(Skill skill, Node allSpecials)
    {  
        if (marker != null)
        {
            marker.Visible = false;
        }

        if (stats.GetAtk() >= stats.GetMaxAtk())
        {
           stats.SetAtk(stats.GetAtk() + skill.GetStatBonusAtk()); 
           stats.SetAtkCounter(3);
           GD.Print("ATK: " + stats.GetAtk());
        }
        if (stats.GetMag() >= stats.GetMaxMag())
        {
            stats.SetMag(stats.GetMag() + skill.GetStatBonusMag());
            GD.Print("MAG: " + stats.GetMag());
        }
        if (stats.GetDef() >= stats.GetMaxDef())
        {
            stats.SetDef(stats.GetDef() + skill.GetStatBonusDef());
            stats.SetDefCounter(3);
            GD.Print("DEF: " + stats.GetDef());
        }
        if (stats.GetRes() >= stats.GetMaxRes())
        {
            stats.SetRes(stats.GetRes() + skill.GetStatBonusRes());
            GD.Print("RES: " + stats.GetRes());
        }
        if (stats.GetLuk() >= stats.GetMaxLuk())
        {
            stats.SetLuk(stats.GetLuk() + skill.GetStatBonusLuk());
            GD.Print("LUK: " + stats.GetLuk());
        }
        if (stats.GetSpd() >= stats.GetMaxSpd())
        {
            stats.SetSpd(stats.GetSpd() + skill.GetStatBonusSpd());
            stats.SetSpdCounter(3);
            GD.Print("SPD: " + stats.GetSpd());
        }        
        marker.Visible = false;

        if (!skill.GetAttackAll())
        {
           battleManager.NextTurn(); 
        }
        else
        {
            battleManager.SetAttacksForNextTurn(battleManager.GetAttacksForNextTurn() + 1);
            battleManager.CheckIfNextTurn(skill.GetTeam());
        }
    }
}
