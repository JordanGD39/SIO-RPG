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
        gameManager = GetParent().GetParent().GetParent() as GameManager;
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

        if (Input.IsActionJustPressed("print_list"))
        {
            GD.Print(learnList);
        }
    }

    public void MyTurn()
    {
        while (!ChooseSkill())
        {
            GD.Print("Searching skill");
        }        

        bool def = true;

        int num = 0;
        int count = battleManager.GetPlayers().Count;
        Random rand = new Random();

        List<Node> lowestDeforMagChar = new List<Node>();

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
                num = battleManager.GetPlayers().IndexOf(lowestDeforMagChar[0]);
            break;
        }       

        if (chosenSkill != null)
        {
            GD.Print("Skill: " + chosenSkill.GetAtk());
            stats.SetStamina(stats.GetStamina() - chosenSkill.GetStaminaDepletion());
            damageScript.GetStaminaBar().Value = (float)stats.GetStamina() / (float)stats.GetMaxStamina() * 100;

            if (chosenSkill.GetAttackAll())
            {
                for (int i = 0; i < battleManager.GetPlayers().Count; i++)
                {
                    battleManager.GetPlayers()[i].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);
                }                
            }
            else
            {
                battleManager.GetPlayers()[num].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);
            }
        }
        else
        {
            battleManager.GetPlayers()[num].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);
        }        

        damageScript.EnemyGuardChoose();
        timer = new Timer();
        timer.WaitTime = 0.4f;
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

        numSkill = rand.Next() % 101;

        if (chosenSkill.GetStaminaDepletion() >= stats.GetStamina() || gameManager.GetDifficulty() == 0 || (gameManager.GetDifficulty() == 1 && numSkill <= 30))
        {
            skillFound = false;
        }

        return skillFound;
    }
}
