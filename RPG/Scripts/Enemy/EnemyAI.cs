using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public enum AIskillTypes {HIGHESTATK, SUPPORT, EVERYONE, ATTACK};

class DefCompare : IComparer<Node> 
{ 
    public int Compare(Node x, Node y) 
    { 
        if (x == null || y == null) 
        { 
            return 0; 
        } 

        Stats p1 = x.GetNode("Stats") as Stats;
        Stats p2 = y.GetNode("Stats") as Stats;

        return p1.GetDef().CompareTo(p2.GetDef());           
    } 
} 

class MagCompare : IComparer<Node> 
{ 
    public int Compare(Node x, Node y) 
    { 
        if (x == null || y == null) 
        { 
            return 0; 
        } 

        Stats p1 = x.GetNode("Stats") as Stats;
        Stats p2 = y.GetNode("Stats") as Stats;

        return p1.GetMag().CompareTo(p2.GetMag());           
    } 
} 

public class EnemyAI : KinematicBody2D
{
    private GameManager gameManager;
    private GUI gui;
    private Node specials;
    private List<AIskillTypes> learnList = new List<AIskillTypes>();
    public List<AIskillTypes> GetLearnList() {return learnList;}
    private BattleManager battleManager;
    private Stats stats;
    private CharacterDamage damageScript;
    private Timer timer;
    private bool timerStarted = false;
    private Skill chosenSkill;
    private List<Node> minions = new List<Node>();
    private int spawnMinionChance = -20;
    private AnimationPlayer animation;
    public AnimationPlayer GetAnimationPlayer() {return animation;}

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        animation = GetChild(0).GetChild(0) as AnimationPlayer;
        learnList.Add(AIskillTypes.ATTACK);
        gameManager = GetParent().GetParent() as GameManager;
        specials = GetNode("Special Moves");
        battleManager = GetParent() as BattleManager;
        stats = GetNode("Stats") as Stats;
        damageScript = GetNode("Damage") as CharacterDamage;
        gui = GetParent().GetParent().GetNode("UI") as GUI;

        Node minionHolder = gameManager.GetNode("MinionHolder");

        for (int i = 0; i < minionHolder.GetChildCount(); i++)
        {
            minions.Add(minionHolder.GetChild(i));
        }
        //If Boss
        if (stats.GetMaxHealth() > 1000)
        {
            List<weakness> newWeaknesses = new List<weakness>();
            Random rand = new Random();
            int num = rand.Next() % Enum.GetNames(typeof(weakness)).Length - 1;
            newWeaknesses.Add((weakness)num);
            num = rand.Next() % Enum.GetNames(typeof(weakness)).Length - 1;
            newWeaknesses.Add((weakness)num);
            while (newWeaknesses[0] == newWeaknesses[1])
            {
                newWeaknesses.RemoveAt(1);
                num = rand.Next() % Enum.GetNames(typeof(weakness)).Length;
                newWeaknesses.Add((weakness)num);
            }
            stats.SetWeaknesses(newWeaknesses);           
        }
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

        if (Input.IsActionJustPressed("print_list") && battleManager.GetEnemies().IndexOf(GetChild(0).GetParent()) == 0)
        {            
            for (int i = 0; i < learnList.Count; i++)
            {
                GD.Print(learnList[i]);
            }
        }
    }

    public async void MyTurn()
    {
        stats.SetStamina(stats.GetStamina() + 20);

        bool skillFound = false;
        Random rand = new Random();

        stats.CheckStatBonus();

        if (stats.GetStun() > 0)
        {
            float stunNum = rand.Next() % 100;

            GD.Print("stun chance: " + stunNum);

            if (stunNum <= 50)
            {
                battleManager.NextTurn(GetChild(0).GetParent());
                GD.Print(stats.GetCharName() + " is stunned!");
                animation.Play("Stunned");
                Task animStunDelay = gameManager.LongRunningOperationAsync((int)Math.Round(animation.GetAnimation("Stunned").Length * 100, MidpointRounding.AwayFromZero));
                await animStunDelay;
                return;
            }
        }

        if (stats.GetMaxHealth() > 1000 && battleManager.GetEnemies().Count < 3)
        {
            spawnMinionChance += 20;
            float spawnMinionRand = rand.Next() % 100;
            GD.Print("Spawning minion outcome: " + spawnMinionRand + " Chance: " + spawnMinionChance);
            if (spawnMinionRand <= spawnMinionChance)
            {
                int minionIndex = rand.Next() % minions.Count;
                Node minion = minions[minionIndex].Duplicate();
                KinematicBody2D minionTransform = minion as KinematicBody2D;
                GetParent().AddChild(minion);
                battleManager.GetEnemies().Add(minion);
                battleManager.GetTurnOrder().Add(minion);
                gui.GiveUIToNewCharacter(minion);
                if (battleManager.GetEnemies().Count <= 2)
                {
                    minionTransform.Position = new Vector2(1200, 500);
                    minionTransform.ZIndex = -1;
                }
                else
                {
                    Node2D otherMinion = battleManager.GetEnemies()[1] as Node2D;
                    if (otherMinion.Position == new Vector2(1200, 930))
                    {                        
                        minionTransform.Position = new Vector2(1200, 500);
                        battleManager.GetEnemies()[1] = minion;
                        battleManager.GetEnemies()[2] = otherMinion;
                        minionTransform.ZIndex = -1;
                    }
                    else
                    {
                        minionTransform.Position = new Vector2(1200, 930);
                        minionTransform.ZIndex = 1;
                    }                    
                }
                AnimationPlayer anim =  minion.GetChild(0).GetChild(0) as AnimationPlayer;
                anim.PlayBackwards("Death");
                Task animDelay = gameManager.LongRunningOperationAsync((int)Math.Round(animation.GetAnimation("Death").Length * 100, MidpointRounding.AwayFromZero));
                await animDelay;
                spawnMinionChance = -20;
                battleManager.NextTurn(GetChild(0).GetParent());
                return;
            }
        }

        for (int i = 0; i < 10; i++)
        {
            skillFound = ChooseSkill();
            GD.Print("Searching skill");

            if (skillFound)
            {
                break;
            }
        }
        
        if (!skillFound)
        {
            chosenSkill = null;
        }

        if (chosenSkill != null && chosenSkill.GetGuardTeam())
        {
            stats.SetGuard(3);
            stats.SetStamina(stats.GetStamina() - chosenSkill.GetStaminaDepletion());
            battleManager.NextTurn(GetChild(0).GetParent());
            GD.Print(stats.GetCharName() + " is guarding");
            stats.GetStatChangesUI().GetNode<Sprite>("Guard").Visible = true;
            return;
        }

        bool someoneIsGuarding = false;
        int num = 0;

        for (int i = 0; i < battleManager.GetPlayers().Count; i++)
        {
            if (battleManager.GetPlayers()[i].GetNode<Stats>("Stats").GetGuard() > 0)
            {
                someoneIsGuarding = true;
                num = i;
            }
        }

        bool def = true;
        
        int count = battleManager.GetPlayers().Count;

        if (chosenSkill != null && chosenSkill.GetMag() > 0)
        {
            def = false;
        }

        List<Node> lowestDeforMagChar = battleManager.GetPlayers();
        if((chosenSkill == null || !chosenSkill.GetStatChange() && !chosenSkill.GetHeal()) && !someoneIsGuarding)
        {
            if (gameManager.GetDifficulty() != 0)
            {
                if (def)
                {
                    DefCompare defCompare = new DefCompare();
                    lowestDeforMagChar.Sort(defCompare);
                }
                else
                {
                    MagCompare magCompare = new MagCompare();
                    lowestDeforMagChar.Sort(magCompare);
                }
            }

            switch (gameManager.GetDifficulty())
            {            
                case 0:               
                    num = rand.Next() % count;
                break;
                case 1:                
                    int randNum = rand.Next() % 3;
                    num = battleManager.GetPlayers().IndexOf(lowestDeforMagChar[randNum]);
                break;
                case 2:         
                    GD.Print("Searching index of lowest def");
                    num = battleManager.GetPlayers().IndexOf(lowestDeforMagChar[0]);
                break;
            }  
        }  
        else
        {
            if (chosenSkill != null && chosenSkill.GetStatChange() && !chosenSkill.GetTeam() && !someoneIsGuarding)
            {
                if (gameManager.GetDifficulty() > 0)
                {
                    int j = -1;
                    for (int i = 0; i < 10; i++)
                    {
                        GD.Print("Choosing player to debuff");
                        j = ChoosePlayerToDebuff(chosenSkill);

                        if (j > 0)
                        {
                            num = j;
                            break;
                        }
                    }   

                    if (num < 0)
                    {
                        num = rand.Next() % count;
                    }
                }
                else
                {
                    num = rand.Next() % count;
                }                              
            }
        }   

        if (chosenSkill != null)
        {
            GD.Print("Skill: " + (chosenSkill.GetAtk() + chosenSkill.GetMag()));
            stats.SetStamina(stats.GetStamina() - chosenSkill.GetStaminaDepletion());

            if (chosenSkill.GetAttackAll())
            {
                if (chosenSkill.GetStatChange())
                {                    
                    Task animDelay = gameManager.LongRunningOperationAsync(3000);
                    await animDelay;
                    gui.HideDescription();
                    for (int i = 0; i < battleManager.GetPlayers().Count; i++)
                    {
                        gui.ChangeDescriptionText(chosenSkill.Name + " on " +  battleManager.GetPlayers()[i].GetNode<Stats>("Stats").GetCharName(), true);
                        battleManager.GetPlayers()[i].GetNode<CharacterDamage>("Damage").Debuff(chosenSkill, stats);
                    }                    
                    return;
                }
                else
                {
                    for (int i = 0; i < battleManager.GetPlayers().Count; i++)
                    {
                        battleManager.GetPlayers()[i].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);
                    } 
                }                               
            }
            else
            {
                if (chosenSkill.GetStatChange() || chosenSkill.GetHeal())
                {                 
                    if (chosenSkill.GetTeam())
                    {   
                        gui.ChangeDescriptionText(chosenSkill.Name + " on " +  battleManager.GetEnemies()[0].GetNode<Stats>("Stats").GetCharName(), false);
                        Task animDelay = gameManager.LongRunningOperationAsync(3000);
                        await animDelay;
                        gui.HideDescription();
                        if (stats.GetMaxHealth() > 1000)
                        {
                            GetNode<CharacterDamage>("Damage").Support(chosenSkill, false, stats);
                        }
                        else
                        {
                            battleManager.GetEnemies()[0].GetNode<CharacterDamage>("Damage").Support(chosenSkill, false, stats);
                        }
                        return;
                    }
                    else
                    {
                        gui.ChangeDescriptionText(chosenSkill.Name + " on " +  battleManager.GetPlayers()[num].GetNode<Stats>("Stats").GetCharName(), false);
                        Task animDelay = gameManager.LongRunningOperationAsync(3000);
                        await animDelay;
                        gui.HideDescription();
                        battleManager.GetPlayers()[num].GetNode<CharacterDamage>("Damage").Debuff(chosenSkill, stats);
                        return;
                    }                   
                }
                else
                {
                    battleManager.GetPlayers()[num].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);
                }                
            }
        }
        else
        {
            battleManager.GetPlayers()[num].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);
        }        

        damageScript.EnemyGuardChoose();
        timer = new Timer();
        timer.WaitTime = 0.4f * gameManager.GetVoiceControl();
        timer.OneShot = true;
        AddChild(timer);
        timer.Start();
        timerStarted = true;
    }

    private bool ChooseSkill()
    {
        bool skillFound = true;

        Random rand = new Random();

        int numSkill = rand.Next() % learnList.Count;

        switch (learnList[numSkill])
        {            
            case AIskillTypes.ATTACK:
            int numAtk = rand.Next() % 2;

            if (numAtk == 0)
            {
                chosenSkill = null;
            }
            else
            {
                chosenSkill = specials.GetChild(0) as Skill;
            }

            break;
            case AIskillTypes.EVERYONE:
            chosenSkill = specials.GetChild(1) as Skill;
            break;
            case AIskillTypes.SUPPORT:
            chosenSkill = specials.GetChild(3) as Skill;
            break;
            case AIskillTypes.HIGHESTATK:
            chosenSkill = specials.GetChild(2) as Skill;
            break;
        }        

        int staminaBonusCheck = 0;
    
        if (gameManager.GetDifficulty() == 2 && learnList[numSkill] != AIskillTypes.HIGHESTATK)
        {
            staminaBonusCheck = 15;
        }

        if (chosenSkill != null)
        {
            if (chosenSkill.GetStaminaDepletion() + staminaBonusCheck >= stats.GetStamina())
            {
                switch (gameManager.GetDifficulty())
                {                
                    case 1:
                    numSkill = rand.Next() % 101;

                    if (numSkill <= 70)
                    {
                        skillFound = false;
                    }
                    break;
                    case 2:
                    skillFound = false;
                    break;
                }
            }   
            //If boss
            if (stats.GetMaxHealth() > 1000 && chosenSkill.GetStatChange() && stats.GetSpd() > stats.GetMaxSpd())
            {
                skillFound = false;
            }         
        }        

        return skillFound;
    }

    private int ChoosePlayerToDebuff(Skill skill)
    {
        Random rand = new Random();

        int numPlayer = rand.Next() % battleManager.GetPlayers().Count;

        Stats playerStats = battleManager.GetPlayers()[numPlayer].GetNode("Stats") as Stats;

        int whichDebuff = 0;

        if (skill.GetStatBonusDef() < 0)
        {
            whichDebuff = 1;
        }
        else if (skill.GetStatBonusSpd() < 0)
        {
            whichDebuff = 2;
        }

        switch (whichDebuff)
        {
            case 0:
            if (playerStats.GetAtk() > playerStats.GetMaxAtk())
            {
                numPlayer = -1;
            }
            break;
            case 1:
            if (playerStats.GetDef() > playerStats.GetMaxDef())
            {
                numPlayer = -1;
            }
            break;
            case 2:
            if (playerStats.GetSpd() > playerStats.GetMaxSpd())
            {
                numPlayer = -1;
            }
            break;
        }

        return numPlayer;
    }
}
