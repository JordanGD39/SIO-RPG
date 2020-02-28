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
    private CharacterDamage damageScript;
    private BattleManager battleManager;
    private float speed = 500;
    private float timer = 0;
    private float guardDelay;
    public void SetTimer(float a){timer = a;}
    private int attackDir = 0;
    public int GetAttackDirection() {return attackDir;}
    private bool chooseAttackDir = false;
    public void SetChooseAttackDir(bool a) {chooseAttackDir = a;}
    private AnimatedSprite guard;    
    private Skill chosenSkill;
    public override void _Ready()
    {
        gui = GetParent().GetParent().GetNode("UI") as GUI;
        battleManager = GetParent() as BattleManager;
        stats = GetNode("Stats") as Stats;
        guard = GetNode("Guard") as AnimatedSprite;
        damageScript = GetNode<CharacterDamage>("Damage") as CharacterDamage;
    }
 // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (goToMid)
        {
            GoToMiddle();
        }

        if (goToStartPos)
        {
            GoToStartPos();
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
            ChooseAttackDirection(delta);
        }  
    }

    private void GoToMiddle()
    {
        Vector2 vector = (target - Position).Normalized();
        MoveAndSlide(vector * speed);

        if (Position.x >= target.x)
        {       
            if (stats.GetStamina() < 100)
            {
                stats.SetStamina(stats.GetStamina() + 15);
                if (stats.GetStamina() > 100)
                {
                    stats.SetStamina(100);
                }
                damageScript.GetStaminaBar().Value = (float)stats.GetStamina() / 100 * 100;
            }  
            
            stats.CheckStatBonus();   
            GD.Print("There");
            goToMid = false;
            gui.ShowAttackMenu(stats);
        }
    }

    private void GoToStartPos()
    {
        Vector2 vector = (startTarget - Position).Normalized();
        MoveAndSlide(vector * speed);

        if (Position.x <= startTarget.x)
        {
            GD.Print("At startPos");
            goToStartPos = false;
            battleManager.CurrCharacterGoesToMid();
        }
    }

    public void ChooseSkill(int i)
    {
        if (stats.GetStamina() <= 0)
        {
            stats.SetStamina(0);            
        }

        targetIndex = 0;
        
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
                        if (!chosenSkill.GetAttackAll())
                        {
                            targets[targetIndex].GetNode<CharacterDamage>("Damage").Debuff(chosenSkill);
                        }
                        else
                        {
                            for (int i = 0; i < targets.Count; i++)
                            {
                                targets[i].GetNode<CharacterDamage>("Damage").Debuff(chosenSkill);
                            }
                        }
                        
                        visible = false;
                    }
                    else
                    {
                        if (!chosenSkill.GetAttackAll())
                        {
                            targets[targetIndex].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);
                        }  
                        else
                        {
                            for (int i = 0; i < targets.Count; i++)
                            {
                                targets[i].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);
                            }
                        }
                    }
                }
                else
                {
                    targets[targetIndex].GetNode<CharacterDamage>("Damage").StartGuardSequence(stats, chosenSkill);                
                }
                  
            }
            else
            {
                targets[targetIndex].GetNode<CharacterDamage>("Damage").Support(chosenSkill);
                visible = false;
            }
            
            guard.Visible = visible;
            targetChoose = false;
            attackDir = 0; 
            chooseAttackDir = true;    
            guardDelay = 0;                  
        }

        if (battleManager.GetEnemies().Count == 0 || (chosenSkill != null && chosenSkill.GetAttackAll())) 
        {   
            if(chosenSkill.GetAttackAll() && timer < 0.25f) 
            {
                for (int i = 0; i < targets.Count; i++)
                {                    
                    targets[i].GetNode<Sprite>("Marker").Visible = true;
                }
            } 

            return;
        }

        if (Input.IsActionJustPressed("ui_up"))
        {
            if (team)
            {
                TargetDown(targets);
            }
            else
            {
                TargetUp(targets);
            }
        }
        else if (Input.IsActionJustPressed("ui_down"))
        {
            if (team)
            {
                TargetUp(targets);
            }
            else
            {
                TargetDown(targets);                
            }  
        }
    }

    private void TargetUp(List<Node> targets)
    {
        targets[targetIndex].GetNode<Sprite>("Marker").Visible = false;

        targetIndex++;

        if (targetIndex > targets.Count - 1)
        {
            targetIndex = 0;
        }

        targets[targetIndex].GetNode<Sprite>("Marker").Visible = true;   
    }

    private void TargetDown(List<Node> targets)
    {
        targets[targetIndex].GetNode<Sprite>("Marker").Visible = false;

        targetIndex--;

        if (targetIndex < 0)
        {
            targetIndex = targets.Count - 1;
        }

        targets[targetIndex].GetNode<Sprite>("Marker").Visible = true;         
    }

    private void ChooseAttackDirection(float delta)
    {
        guardDelay += delta;

        if (guardDelay > 0.25f || stats.GetStamina() > 0)
        {
            if (Input.IsActionPressed("ui_up"))
            {
                attackDir = 1;
                guard.Play("Up");
                guardDelay = 0;
            }
            else if (Input.IsActionPressed("ui_down"))
            {
                attackDir = -1;
                guard.Play("Down");
                guardDelay = 0;
            }
            else
            {
                attackDir = 0;
                guard.Play("Forward");
                guardDelay = 0;
            }      
        }            
    }
}
