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
    public void StartGuardSequence(Stats attackerStatsTemp, Skill skill)
    {
        skillThatAttackedMe = skill;

        marker.Visible = false;
        
        if (!playerControl)
        {
            CheckAttackToLearn(false, null);
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
        int defOrRes = stats.GetDef();

        if (skillThatAttackedMe != null)
        {
            if (skillThatAttackedMe.GetAtk() > 0)
            {
                atkOrMag = skillThatAttackedMe.GetAtk();
            }
            else if (skillThatAttackedMe.GetMag() > 0)
            {
                atkOrMag = skillThatAttackedMe.GetMag();
                defOrRes = stats.GetRes();
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
            GD.Print("Attacker atk: " + attackerAtk + " | Defender def: " + defOrRes);            
            damage = attackerAtk - defOrRes;
            Random rand = new Random();
            damage += (rand.Next() % 15) - 10;            
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

        int damageInt = Mathf.RoundToInt(damage);
        
        GD.Print(attackerStats.GetCharName() + " did " + damageInt + " damage to " + stats.GetCharName());        

        stats.SetHealth(stats.GetHealth() - damageInt);
        if (stats.GetHealth() <= 0)
        {
            stats.SetHealth(0);
            battleManager.TakeMeOutList(GetParent(), playerControl);
        }

        stats.UpdateHealth();

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
            battleManager.CheckIfNextTurn(playerControl);
        }
    }
    public void Support(Skill skill)
    {
        skillThatAttackedMe = skill;

        marker.Visible = false;

        if (playerControl)
        {
            battleManager.GetEnemies()[0].GetNode<CharacterDamage>("Damage").CheckAttackToLearn(true, skill);
        }

        if (skill.GetHeal() && stats.GetHealth() < stats.GetMaxHealth())
        {
            stats.SetHealth(stats.GetHealth() + skill.GetMag());
            if (stats.GetHealth() > stats.GetMaxHealth())
            {
                stats.SetHealth(stats.GetMaxHealth());
            }
            stats.UpdateHealth();
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

    public void Debuff(Skill skill)
    {  
        skillThatAttackedMe = skill;

        if (marker != null)
        {
            marker.Visible = false;
        }

        if (!playerControl)
        {
            CheckAttackToLearn(false, null);
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
            battleManager.CheckIfNextTurn(playerControl);
        }
    }

    public void CheckAttackToLearn(bool playerCalled, Skill skill)
    {
        if (playerCalled)
        {
            skillThatAttackedMe = skill;
        }

        if (skillThatAttackedMe == null || !skillThatAttackedMe.GetStatChange() && !skillThatAttackedMe.GetAttackAll() && skillThatAttackedMe.GetMag() <= 20 && skillThatAttackedMe.GetAtk() <= 20)
        {
            ai.GetLearnList().Add(AIskillTypes.ATTACK);
            return;
        }
        else
        {
            if (skillThatAttackedMe.GetAttackAll())
            {
                ai.GetLearnList().Add(AIskillTypes.EVERYONE);
                return;
            }
            else if (skillThatAttackedMe.GetStatChange())
            {
                ai.GetLearnList().Add(AIskillTypes.SUPPORT);
                return;
            }
            //Checking if skill has highest atk of all
            else if (skillThatAttackedMe.GetAtk() > 20 || skillThatAttackedMe.GetMag() > 20)
            {
                ai.GetLearnList().Add(AIskillTypes.HIGHESTATK);
                return;
            }
        }                
    }
}
