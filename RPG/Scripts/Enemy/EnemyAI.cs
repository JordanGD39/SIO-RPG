using Godot;
using System;
using System.Collections.Generic;

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
    private Node specials;
    private List<AIskillTypes> learnList = new List<AIskillTypes>();
    public List<AIskillTypes> GetLearnList() {return learnList;}
    private BattleManager battleManager;
    private Stats stats;
    private CharacterDamage damageScript;
    private Timer timer;
    private bool timerStarted = false;
    private Skill chosenSkill;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        learnList.Add(AIskillTypes.ATTACK);
        gameManager = GetParent().GetParent() as GameManager;
        specials = GetNode("Special Moves");
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

        if (Input.IsActionJustPressed("print_list") && battleManager.GetEnemies().IndexOf(GetChild(0).GetParent()) == 0)
        {            
            for (int i = 0; i < learnList.Count; i++)
            {
                GD.Print(learnList[i]);
            }
        }
    }

    public void MyTurn()
    {
        stats.SetStamina(stats.GetStamina() + 10);
        bool skillFound = false;
        Random rand = new Random();

        stats.CheckStatBonus();

        if (stats.GetStun() > 0)
        {
            float stunNum = rand.Next() % 100;

            GD.Print("stun chance: " + stunNum);

            if (stunNum <= 50)
            {
                battleManager.NextTurn();
                GD.Print(stats.GetCharName() + " is stunned!");
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
            battleManager.NextTurn();
            GD.Print(stats.GetCharName() + " is guarding");
            return;
        }

        bool someoneIsGuarding = false;
        int num = 0;

        for (int i = 0; i < battleManager.GetEnemies().Count; i++)
        {
            if (battleManager.GetPlayers()[i].GetNode<Stats>("Stats").GetGuard() > 0)
            {
                someoneIsGuarding = true;
                num = i;
            }
        }

        bool def = true;
        
        int count = battleManager.GetPlayers().Count;

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

        if (chosenSkill != null)
        {
            GD.Print("Skill: " + (chosenSkill.GetAtk() + chosenSkill.GetMag()));
            stats.SetStamina(stats.GetStamina() - chosenSkill.GetStaminaDepletion());

            if (chosenSkill.GetAttackAll())
            {
                for (int i = 0; i < battleManager.GetPlayers().Count; i++)
                {
                    battleManager.GetPlayers()[i].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);
                }                
            }
            else
            {
                if (chosenSkill.GetStatChange() || chosenSkill.GetHeal())
                {
                    if (stats.GetMaxHealth() > 1000)
                    {
                        GetNode<CharacterDamage>("Damage").Support(chosenSkill, false);
                    }
                    else
                    {
                        battleManager.GetEnemies()[0].GetNode<CharacterDamage>("Damage").Support(chosenSkill, false);
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

        if (chosenSkill != null)
        {
            if (chosenSkill.GetStaminaDepletion() >= stats.GetStamina())
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
        }        

        return skillFound;
    }
}
