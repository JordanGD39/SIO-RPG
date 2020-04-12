using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CharacterDamage : Node
{
    private Stats stats;
    private Node FX;
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
    private GameManager gameManager;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {        
        ai = GetParent() as EnemyAI;
        player = GetParent() as Player;
        marker = GetParent().GetNode("Marker") as Sprite;
        stats = GetParent().GetNode("Stats") as Stats;
        stats.SetMaxHealth(stats.GetHealth());
        battleManager = GetParent().GetParent() as BattleManager;        
        if (battleManager != null)
        {
            gameManager = battleManager.GetParent() as GameManager;
            FX = gameManager.GetNode("FX") as Node;
        }

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
        if (battleManager != null)
        {
            battleManager = GetParent().GetParent() as BattleManager;
            gameManager = battleManager.GetParent() as GameManager;
        }

        skillThatAttackedMe = skill;

        marker.Visible = false;
        
        if (!playerControl)
        {
            CheckAttackToLearn(false, null);
        }

        int lessAcc = 0;

        if (skillThatAttackedMe != null)
        {
            lessAcc = skillThatAttackedMe.GetLessAcc();
        }

        attackerStats = attackerStatsTemp;
        float spd = stats.GetSpd() / 10;
        Random rand = new Random();
        float num = rand.Next() % 100;

        if (num <= spd + lessAcc)
        {
            missed = true;
        }
        else
        {
            missed = false;
        }

        GD.Print("Missed: " + missed + " " + (spd + lessAcc) + " " + num + " is player: " + playerControl);

        timer = new Timer();
        timer.WaitTime = 1f * gameManager.GetVoiceControl();
        timer.OneShot = true;
        AddChild(timer);
        timer.Start();

        if (guard == null)
        {
            GetMyGuard();
        }

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
            else if (timer.TimeLeft < 0.6f * gameManager.GetVoiceControl() && !playerControl)
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
        if (guard == null)
        {
            GetMyGuard();
        }

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

    private async void ReceiveDamage()
    {   
        float crit = 1;

        Random rand = new Random();
        float num = rand.Next() % 100;
        float luk = attackerStats.GetLuk() / 10;

        if (num <= luk)
        {
            crit = 2;
            GD.Print("CRIT!!!!!");
        }

        int atkOrMagUser = attackerStats.GetAtk();
        int atkOrMag = 0;
        int defOrRes = stats.GetDef();
        bool stun = false;
        float weaknessHit = 1;       

        if (skillThatAttackedMe != null)
        {
            if (skillThatAttackedMe.GetAtk() != 0)
            {
                atkOrMag = skillThatAttackedMe.GetAtk();
            }
            else if (skillThatAttackedMe.GetMag() != 0)
            {
                atkOrMagUser = attackerStats.GetMag();
                atkOrMag = skillThatAttackedMe.GetMag();
                defOrRes = stats.GetRes();
            }

            stun = skillThatAttackedMe.GetStun();

            if (stats.GetWeaknesses().Contains(skillThatAttackedMe.GetElement()))
            {
                weaknessHit = 1.5f;
                GD.Print("WEAKNESS HIT!!");
            }
        }
        else
        {
            if (stats.GetWeaknesses().Contains(attackerStats.GetAttackElement()))
            {
                weaknessHit = 1.5f;
                GD.Print("WEAKNESS HIT!!");
            }
        }

        float attackerAtk = atkOrMagUser + atkOrMag;

        if (attackerStats.GetStamina() <= 0)
        {
            attackerAtk *= 0.6f;
            attackerAtk = Mathf.Round(attackerAtk);
        }

        float damage = 0;
        if (!missed)
        {
            GD.Print("Attacker: " + attackerStats.GetCharName() +  " atk: " + attackerAtk + " | Defender def: " + defOrRes);           
            damage = (attackerAtk - defOrRes) * weaknessHit * crit;
            GD.Print("Without extra damage calc: " + (attackerAtk - defOrRes));
            int randExtraDamage = (rand.Next() % 12) - 2; 
            GD.Print("Random extra Damage: " + randExtraDamage);
            damage += randExtraDamage;
           
            if (damage < 0)
            {
                damage = 0;
            }

            if (weaknessHit > 1)
            {
                float stunChance = rand.Next() % 100;
                GD.Print("Stun chance: " + stunChance);
                if (stunChance <= 20)
                {
                    stun = true;
                }
            }
        }    
        else
        {
            GD.Print("Missed!");
        }

        guard.Visible = false;
        attackerStats.GetParent().GetNode<AnimatedSprite>("Guard").Visible = false;

        Player attackerIsPlayer = attackerStats.GetParent() as Player;
        bool hitShield = false;

        Sprite shield = GetParent().GetNode<Sprite>("Shield");

        if (attackerIsPlayer != null)
        {
            attackerIsPlayer.SetChooseAttackDir(false);            

            if (attackerIsPlayer.GetAttackDirection() == guardDir)
            {
                damage *= 0.5f;
                GD.Print("You hit their shield!");                
                stun = false;
                hitShield = true;
                if (stats.GetCounter())
                {
                    damage = 0;
                }
                else
                {
                    shield.Visible = true;
                }
            } 
        }
        else
        {            

            if (attackerStats.GetParent().GetNode<CharacterDamage>("Damage").GetEnemyAttackDirection() == GetParent<Player>().GetAttackDirection())
            {
                damage *= 0.5f;
                GD.Print("You hit their shield!");
                stun = false;
                hitShield = true;
                if (stats.GetCounter())
                {
                    damage = 0;
                }   
                else
                {
                    shield.Visible = true;
                }          
            } 
        }       

        int damageInt = Mathf.RoundToInt(damage);
        
        GD.Print(attackerStats.GetCharName() + " did " + damageInt + " damage to " + stats.GetCharName());        
        int animDurationMS = 0;

        if (attackerIsPlayer != null)
        {
            attackerIsPlayer.GetAnimationPlayer().Play(attackerStats.GetCharName() + "_Attack");

            PlayElementAnim();
            animDurationMS = (int)Math.Round(attackerIsPlayer.GetAnimationPlayer().GetAnimation(attackerStats.GetCharName() + "_Attack").Length * 1000, MidpointRounding.AwayFromZero);
            Task longRunningTask = gameManager.LongRunningOperationAsync(attackerStats.GetHitHappened());
            await longRunningTask;

            animDurationMS -= attackerStats.GetHitHappened() + 200;        
        }
        else
        {
            attackerStats.GetParent<EnemyAI>().GetAnimationPlayer().Play(attackerStats.GetCharName() + "_Attack");

            PlayElementAnim();
            animDurationMS = (int)Math.Round(attackerStats.GetParent<EnemyAI>().GetAnimationPlayer().GetAnimation(attackerStats.GetCharName() + "_Attack").Length * 1000, MidpointRounding.AwayFromZero);
            Task longRunningTask = gameManager.LongRunningOperationAsync(attackerStats.GetHitHappened());
            await longRunningTask;

            animDurationMS -= attackerStats.GetHitHappened() + 200;
        }

        Sprite sprite = GetParent().GetChild<Sprite>(0);

        if (damageInt > 0)
        {            
            if (crit > 1)
            {
                sprite.Modulate = new Color(3,3,3);
                gameManager.GetAudioNode().GetNode<AudioStreamPlayer>("Crit").Play();
            }
            else
            {
                if (hitShield)
                {
                    sprite.Modulate = new Color(1.25f,1.25f,1.25f);
                    gameManager.GetAudioNode().GetNode<AudioStreamPlayer>("Guard").Play();
                }
                else
                {
                    sprite.Modulate = new Color(2,2,2);
                    gameManager.GetAudioNode().GetChild<AudioStreamPlayer>(0).Play();
                }
               
            }            
        }
        else
        {
            if (missed)
            {
                sprite.Modulate = new Color(0.5f,0.5f,0.5f);
                gameManager.GetAudioNode().GetNode<AudioStreamPlayer>("Miss").Play();
            }
            else
            {
                sprite.Modulate = new Color(1.25f,1.25f,1.25f);
                gameManager.GetAudioNode().GetNode<AudioStreamPlayer>("NoDamage").Play();
            }
        }        

        if (skillThatAttackedMe != null)
        {
            PlayElementSFX();
        }        

        stats.SetHealth(stats.GetHealth() - damageInt);

        Task animTask = gameManager.LongRunningOperationAsync(200);
        await animTask;

        sprite.Modulate = new Color(1,1,1);

        if (stats.GetCounter() && hitShield)
        {
            attackerStats.GetParent().GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, null);
            stats.SetCounter(false);
            return;
        }

        animTask = gameManager.LongRunningOperationAsync(animDurationMS);
        await animTask;  

        shield.Visible = false;      

        if (stun && stats.GetStun() == 0 && damageInt > 0)
        {
            stats.SetStun(3);
            GD.Print(stats.GetCharName() + " is hit with a stun skill!");
            stats.GetStatChangesUI().GetNode<Sprite>("Stun").Visible = true;
        }
        
        if (stats.GetHealth() <= 0)
        {
            stats.SetHealth(0);
            battleManager.TakeMeOutList(GetParent(), playerControl);
        }

        GD.Print(stats.GetCharName() + " has " + stats.GetHealth() + " HP left");

        if (skillThatAttackedMe == null || !skillThatAttackedMe.GetAttackAll())
        {
           battleManager.NextTurn();
        }
        else
        {
            battleManager.CheckIfNextTurn(playerControl);
        }
    }

    public void Support(Skill skill, bool userIsPlayer, Stats characterStats)
    {
        skillThatAttackedMe = skill;

        marker.Visible = false;

        if (playerControl)
        {
            for (int i = 0; i < battleManager.GetEnemies().Count; i++)
            {
                battleManager.GetEnemies()[i].GetNode<CharacterDamage>("Damage").CheckAttackToLearn(true, skill);
            }            
        }

        float multiplier = 1;

        if (characterStats.GetStamina() <= 0)
        {
            multiplier = 0.25f;
        }
        GD.Print(stats.GetCharName() + " is getting supported!");
        if (skill.GetHeal() && stats.GetHealth() < stats.GetMaxHealth())
        {
            stats.SetHealth(stats.GetHealth() + Mathf.RoundToInt((float)skill.GetMag() * multiplier));
            if (stats.GetHealth() > stats.GetMaxHealth())
            {
                stats.SetHealth(stats.GetMaxHealth());
            }
            GD.Print("Health: " + stats.GetHealth());            
        }        
        if (stats.GetAtk() <= stats.GetMaxAtk())
        {
            stats.SetAtk(stats.GetAtk() + Mathf.RoundToInt((float)skill.GetStatBonusAtk() * multiplier)); 
            stats.SetAtkCounter(3);
            GD.Print("ATK: " + stats.GetAtk());
            stats.SetMag(stats.GetMag() + Mathf.RoundToInt((float)skill.GetStatBonusMag() * multiplier));
            GD.Print("MAG: " + stats.GetMag());
            AnimatedSprite atk =  stats.GetStatChangesUI().GetNode<AnimatedSprite>("Atk");
            if (stats.GetAtk() > stats.GetMaxAtk())
            {                
                atk.Play("AtkUp");
                atk.Visible = true;
            }
            else if(stats.GetAtk() == stats.GetMaxAtk())
            {
                atk.Visible = false;
                stats.SetAtkCounter(0);
            }  
        }
        if (stats.GetDef() <= stats.GetMaxDef())
        {
            stats.SetDef(stats.GetDef() + Mathf.RoundToInt((float)skill.GetStatBonusDef() * multiplier));
            stats.SetDefCounter(3);
            GD.Print("DEF: " + stats.GetDef());
            stats.SetRes(stats.GetRes() + Mathf.RoundToInt((float)skill.GetStatBonusRes() * multiplier));
            GD.Print("RES: " + stats.GetRes());
            AnimatedSprite def =  stats.GetStatChangesUI().GetNode<AnimatedSprite>("Def");
            if (stats.GetDef() > stats.GetMaxDef())
            {                
                def.Play("DefUp");
                def.Visible = true;
            }
            else if(stats.GetDef() == stats.GetMaxDef())
            {
                def.Visible = false;
                stats.SetDefCounter(0);
            }            
        }
        if (stats.GetSpd() <= stats.GetMaxSpd())
        {
            stats.SetSpd(stats.GetSpd() + Mathf.RoundToInt((float)skill.GetStatBonusSpd() * multiplier));
            stats.SetSpdCounter(3);
            GD.Print("SPD: " + stats.GetSpd());
            stats.SetLuk(stats.GetLuk() + Mathf.RoundToInt((float)skill.GetStatBonusLuk() * multiplier));
            GD.Print("LUK: " + stats.GetLuk());
            AnimatedSprite spd =  stats.GetStatChangesUI().GetNode<AnimatedSprite>("Spd");
            if (stats.GetSpd() > stats.GetMaxSpd())
            {                
                spd.Play("SpdUp");
                spd.Visible = true;
            }
            else if(stats.GetSpd() == stats.GetMaxSpd())
            {
                spd.Visible = false;
                stats.SetSpdCounter(0);
            }           
        }

        marker.Visible = false;

        if (!skill.GetAttackAll())
        {
           battleManager.NextTurn(); 
        }
        else
        {
            battleManager.CheckIfNextTurn(userIsPlayer);
        }
    }

    public void Debuff(Skill skill, Stats characterStats)
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

        GD.Print(stats.GetCharName() + " is getting debuffed!");

        float multiplier = 1;

        if (characterStats.GetStamina() <= 0)
        {
            multiplier = 0.25f;
        }

        if (stats.GetAtk() >= stats.GetMaxAtk())
        {
           stats.SetAtk(stats.GetAtk() + Mathf.RoundToInt((float)skill.GetStatBonusAtk() * multiplier)); 
           stats.SetAtkCounter(3);
           GD.Print("ATK: " + stats.GetAtk());
           stats.SetMag(stats.GetMag() + Mathf.RoundToInt((float)skill.GetStatBonusMag() * multiplier));
            GD.Print("MAG: " + stats.GetMag());
            AnimatedSprite atk =  stats.GetStatChangesUI().GetNode<AnimatedSprite>("Atk");
            if (stats.GetAtk() < stats.GetMaxAtk())
            {                
                atk.Play("AtkDown");
                atk.Visible = true;
            }
            else if(stats.GetAtk() == stats.GetMaxAtk())
            {
                atk.Visible = false;
                stats.SetAtkCounter(0);
            } 
        }
        if (stats.GetDef() >= stats.GetMaxDef())
        {
            stats.SetDef(stats.GetDef() + Mathf.RoundToInt((float)skill.GetStatBonusDef() * multiplier));
            stats.SetDefCounter(3);
            GD.Print("DEF: " + stats.GetDef());
            stats.SetRes(stats.GetRes() + Mathf.RoundToInt((float)skill.GetStatBonusRes() * multiplier));
            GD.Print("RES: " + stats.GetRes());
            AnimatedSprite def =  stats.GetStatChangesUI().GetNode<AnimatedSprite>("Def");
            if (stats.GetDef() < stats.GetMaxDef())
            {                
                def.Play("DefDown");
                def.Visible = true;
            }
            else if(stats.GetDef() == stats.GetMaxDef())
            {
                def.Visible = false;
                stats.SetDefCounter(0);
            }   
        }
        if (stats.GetSpd() >= stats.GetMaxSpd())
        {
            stats.SetSpd(stats.GetSpd() + Mathf.RoundToInt((float)skill.GetStatBonusSpd() * multiplier));
            stats.SetSpdCounter(3);
            GD.Print("SPD: " + stats.GetSpd());
            stats.SetLuk(stats.GetLuk() + Mathf.RoundToInt((float)skill.GetStatBonusLuk() * multiplier));
            GD.Print("LUK: " + stats.GetLuk());
            AnimatedSprite spd =  stats.GetStatChangesUI().GetNode<AnimatedSprite>("Spd");
            if (stats.GetSpd() < stats.GetMaxSpd())
            {                
                spd.Play("SpdDown");
                spd.Visible = true;
            }
            else if(stats.GetSpd() == stats.GetMaxSpd())
            {
                spd.Visible = false;
                stats.SetSpdCounter(0);
            }
        }      
        marker.Visible = false;

        if (!skill.GetAttackAll())
        {
           battleManager.NextTurn(); 
        }
        else
        {
            battleManager.CheckIfNextTurn(playerControl);
        }
    }

    public void ReceiveItem(int i)
    {
        marker.Visible = false;

        switch (i)
        {            
            case 0:
            stats.SetHealth(stats.GetHealth() + 100);
            if (stats.GetHealth() > stats.GetMaxHealth())
            {
                stats.SetHealth(stats.GetMaxHealth());
            }
            GD.Print("Health: " + stats.GetHealth());
            break;
            case 1:
            stats.SetStamina(stats.GetStamina() + 50);
            if (stats.GetStamina() > stats.GetMaxStamina())
            {
                stats.SetStamina(stats.GetMaxStamina());
            }
            break;
            case 2:
            stats.SetHealth(100);
            battleManager.PutMeInList(GetParent());
            break;
            case 3:
            if (stats.GetAtk() < stats.GetMaxAtk())
            {
                stats.SetAtk(stats.GetMaxAtk());
                stats.SetMag(stats.GetMaxMag());
                stats.GetStatChangesUI().GetNode<AnimatedSprite>("Atk").Visible = false;
            }
            if (stats.GetDef() < stats.GetMaxDef())
            {
                stats.SetDef(stats.GetMaxDef());
                stats.SetRes(stats.GetMaxRes());
                stats.GetStatChangesUI().GetNode<AnimatedSprite>("Def").Visible = false;
            }
            if (stats.GetSpd() < stats.GetMaxSpd())
            {
                stats.SetSpd(stats.GetMaxSpd());
                stats.SetLuk(stats.GetMaxLuk());
                stats.GetStatChangesUI().GetNode<AnimatedSprite>("Spd").Visible = false;
            }

            stats.SetStun(0);
            stats.GetStatChangesUI().GetNode<AnimatedSprite>("Stun").Visible = false;
            break;
        }
        battleManager.NextTurn();
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

    private void PlayElementAnim()
    {
        if (skillThatAttackedMe != null)
        {
            switch (skillThatAttackedMe.GetElement())
            {                
                case weakness.LIGHTNING:
                    if (!skillThatAttackedMe.GetAttackAll())
                    {
                        FX.GetChild<Sprite>(0).Position = new Vector2(GetParent<KinematicBody2D>().Position.x - 100, GetParent<KinematicBody2D>().Position.y - 250);
                    }
                    else
                    {
                        if (!playerControl)
                        {
                            FX.GetChild<Sprite>(0).Position = new Vector2(1200, 650);
                        }
                        else
                        {
                            FX.GetChild<Sprite>(0).Position = new Vector2(350, 650);
                        }
                        
                    }
                    FX.GetChild(0).GetChild<AnimationPlayer>(0).Play("Thunder");        
                break;
                case weakness.FIRE:
                if (!skillThatAttackedMe.GetAttackAll())
                    {
                        FX.GetChild<Sprite>(1).Position = GetParent<KinematicBody2D>().Position;
                        if (skillThatAttackedMe.GetMag() < 50)
                        {
                            FX.GetChild<Sprite>(1).Scale = new Vector2(2, 2);
                        }
                        else
                        {
                            FX.GetChild<Sprite>(1).Scale = new Vector2(4, 4);
                        }
                    }
                    else
                    {
                        if (!playerControl)
                        {
                            FX.GetChild<Sprite>(1).Position = new Vector2(1300, 650);
                        }
                        else
                        {
                            FX.GetChild<Sprite>(1).Position = new Vector2(450, 650);
                        }
                        FX.GetChild<Sprite>(1).Scale = new Vector2(5, 5);
                    }
                    FX.GetChild(1).GetChild<AnimationPlayer>(0).Play("Fire");        
                break;
            }   
        }
        else
        {
            if (attackerStats.GetAttackElement() == weakness.FIST)
            {
                if (!playerControl)
                {
                    FX.GetChild<Sprite>(2).Position = new Vector2(GetParent<KinematicBody2D>().Position.x - 100, GetParent<KinematicBody2D>().Position.y);
                    FX.GetChild<Sprite>(2).Scale = new Vector2(0.7f, 0.7f);
                }
                else
                {
                    FX.GetChild<Sprite>(2).Position = new Vector2(GetParent<KinematicBody2D>().Position.x + 100, GetParent<KinematicBody2D>().Position.y);
                    FX.GetChild<Sprite>(2).Scale = new Vector2(-0.7f, 0.7f);
                }
                
                FX.GetChild(2).GetChild<AnimationPlayer>(0).Play("Hand");
            }
        }
    }

    private void PlayElementSFX()
    {
        switch (skillThatAttackedMe.GetElement())
        {            
            case weakness.LIGHTNING:
                gameManager.GetAudioNode().GetChild<AudioStreamPlayer>(2).Play(1);             
            break;
            case weakness.FIRE:
                gameManager.GetAudioNode().GetChild<AudioStreamPlayer>(3).Play(0);
            break;
        }        
    }
}
