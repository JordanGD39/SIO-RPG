using Godot;
using System;
using System.Collections.Generic;

public class Player : KinematicBody2D
{
    private bool goToMid = false;
    private bool goToStartPos = false;
    private bool targetChoose = false;
    private int targetIndex = 0;

    public void SetTargetChoose(bool a) {targetChoose = a;}
    public void SetGoToMid(bool a) {goToMid = a;}
    public void SetGoToStartPos(bool a) {goToStartPos = a;}
    private Vector2 target = new Vector2(680, 700);
    private Vector2 startTarget;
    public void SetStartTarget(Vector2 a) {startTarget = a;}
    private GUI gui;
    private Stats stats;
    private BattleManager battleManager;
    private float speed = 39000;
    private float timer = 0;
    public void SetTimer(float a){timer = a;}
    private int attackDir = 0;
    public int GetAttackDirection() {return attackDir;}
    private bool chooseAttackDir = false;
    private AnimatedSprite guard;
    private int atkCounter = 3;
    public void SetAtkCounter(int a) {atkCounter = a;}
    private int spdCounter = 3;
    public void SetSpdCounter(int a) {spdCounter = a;}
    private int defCounter = 3;
    public void SetDefCounter(int a) {defCounter = a;}
    private Skill chosenSkill;
    public override void _Ready()
    {
        gui = GetParent().GetParent().GetNode("UI") as GUI;
        battleManager = GetParent() as BattleManager;
        stats = GetNode("Stats") as Stats;
        guard = GetNode("Guard") as AnimatedSprite;
    }
 // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (goToMid)
        {
            GoToMiddle(delta);
        }

        if (goToStartPos)
        {
            GoToStartPos(delta);
        }

        if (targetChoose)
        {
            bool support = false;

            if (chosenSkill != null)
            {
                support = chosenSkill.GetTeam();
            }
            ChooseTarget(delta, support);
        }   

        if (chooseAttackDir)
        {
            ChooseAttackDirection();
        }  
    }

    private void GoToMiddle(float delta)
    {
        Vector2 vector = (target - Position).Normalized();
        MoveAndSlide(vector * speed * delta);

        if (Position.x > target.x)
        {         
            CheckStatBonus();   
            GD.Print("There");
            goToMid = false;
            gui.ShowAttackMenu(stats);
        }
    }

    private void GoToStartPos(float delta)
    {
        Vector2 vector = (startTarget - Position).Normalized();
        MoveAndSlide(vector * speed * delta);

        if (Position.x < startTarget.x)
        {
            GD.Print("At startPos");
            goToStartPos = false;
            battleManager.CurrCharacterGoesToMid();
        }
    }

    public void ChooseSkill(int i)
    {
        if (i >= 0)
        {
            chosenSkill = GetNode("Special Moves").GetChild(i) as Skill;
            if (chosenSkill.GetTeam())
            {
                battleManager.GetPlayers()[0].GetNode<Sprite>("Marker").Visible = true;
                return;
            }
        }
        else
        {
            chosenSkill = null;
        }

        battleManager.GetEnemies()[0].GetNode<Sprite>("Marker").Visible = true;
    }

    private void CheckStatBonus()
    {
        if (stats.GetAtk() != stats.GetMaxAtk())
        {
            atkCounter--;
            if (atkCounter <= 0)
            {
                stats.SetAtk(stats.GetMaxAtk());
                stats.SetMag(stats.GetMaxMag());
                atkCounter = 3;
            }
        }

        if (stats.GetSpd() != stats.GetMaxSpd())
        {
            spdCounter--;
            if (spdCounter <= 0)
            {
                stats.SetSpd(stats.GetMaxSpd());
                stats.SetLuk(stats.GetMaxLuk());
                spdCounter = 3;
            }
        }

        if (stats.GetDef() != stats.GetMaxDef())
        {
            defCounter--;
            if (defCounter <= 0)
            {
                stats.SetDef(stats.GetMaxDef());
                stats.SetRes(stats.GetMaxRes());
                defCounter = 3;
            }
        }
    }
    private void ChooseTarget(float delta, bool team)
    {
        List<Node> targets = new List<Node>();

        if (team)
        {
            targets = battleManager.GetPlayers();
        }
        else
        {
            targets = battleManager.GetEnemies();
        }

        if (timer < 0.3f)
        {
            timer += delta;
        }        

        if (Input.IsActionJustPressed("ui_accept") && timer > 0.25f)
        {
            GD.Print(stats.GetCharName());
            bool visible = true;
            if (!team)
            {                
                if (chosenSkill != null)
                {
                    stats.SetStamina(stats.GetStamina() - chosenSkill.GetStaminaDepletion());
                    GetNode<CharacterDamage>("Damage").GetStaminaBar().Value = (float)stats.GetStamina() / (float)stats.GetMaxStamina() * 100;

                    if (chosenSkill.GetStatChange() || chosenSkill.GetHeal())
                    {
                        battleManager.GetEnemies()[targetIndex].GetNode<CharacterDamage>("Damage").Debuff(chosenSkill);
                        visible = false;
                    }
                    else
                    {
                        battleManager.GetEnemies()[targetIndex].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);
                    }
                }
                else
                {
                    battleManager.GetEnemies()[targetIndex].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);
                }
                  
            }
            else
            {
                battleManager.GetPlayers()[targetIndex].GetNode<CharacterDamage>("Damage").Support(chosenSkill);
                visible = false;
            }
            
            guard.Visible = visible;
            targetChoose = false;
            chooseAttackDir = true;
        }

        if (battleManager.GetEnemies().Count == 0) {return;}

        if (Input.IsActionJustPressed("ui_up"))
        {
            targets[targetIndex].GetNode<Sprite>("Marker").Visible = false;

            targetIndex++;

            if (targetIndex > targets.Count - 1)
            {
                targetIndex = 0;
            }

            targets[targetIndex].GetNode<Sprite>("Marker").Visible = true;            
        }
        else if (Input.IsActionJustPressed("ui_down"))
        {
            targets[targetIndex].GetNode<Sprite>("Marker").Visible = false;

            targetIndex--;

            if (targetIndex < 0)
            {
                targetIndex = targets.Count - 1;
            }
    
            targets[targetIndex].GetNode<Sprite>("Marker").Visible = true;            
        }
    }

    private void ChooseAttackDirection()
    {
        if (Input.IsActionPressed("ui_up"))
        {
            attackDir = 1;
            guard.Play("Up");
        }
        else if (Input.IsActionPressed("ui_down"))
        {
            attackDir = -1;
            guard.Play("Down");
        }
        else
        {
            attackDir = 0;
            guard.Play("Forward");
        } 
    }
}
