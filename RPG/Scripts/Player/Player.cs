using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    private Node specials;
    private bool someoneIsGuarding = false;
    private bool alreadyTargeted = false;
    private GameManager gameManager;
    private int permSkillIndex;
    private int permItemIndex;
    private bool useItem = false;
    private List<Node> targets = new List<Node>();
    private AnimationPlayer animation;
    public AnimationPlayer GetAnimationPlayer() {return animation;}
    private Sprite marker;

    private float failsafeTimer = 0;
    public override void _Ready()
    {
        specials = GetNode("Special Moves");
        gui = GetParent().GetParent().GetNode("UI") as GUI;
        battleManager = GetParent() as BattleManager;
        gameManager = battleManager.GetParent() as GameManager;
        stats = GetNode("Stats") as Stats;
        guard = GetNode("Guard") as AnimatedSprite;
        damageScript = GetNode("Damage") as CharacterDamage;
        animation = GetChild(0).GetChild(0) as AnimationPlayer;
        marker = GetNode("Marker") as Sprite;
    }
 // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (goToMid)
        {
            failsafeTimer += delta;
            GoToMiddle();
        }

        if (goToStartPos)
        {
            GoToStartPos();
        }

        if (targetChoose)
        {
            bool support = ListSupport();
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

        if (failsafeTimer > 5)
        {
            Position = target;
        }

        if (Position.x >= target.x)
        {      
            stats.SetStamina(stats.GetStamina() + 20); 
            
            stats.CheckStatBonus();   
            GD.Print("There");
            goToMid = false;
            failsafeTimer = 0;
            gui.ShowAttackMenu();
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

    public void ChooseSkill(int skillIndex)
    {        
        useItem = false;
        targetIndex = 0;       

        if (stats.GetStun() > 0)
        {
            Random rand = new Random();
            
            float stunNum = rand.Next() % 100;

            GD.Print("stun chance: " + stunNum);

            if (stunNum <= 50)
            {
                battleManager.NextTurn();
                GD.Print(stats.GetCharName() + " is stunned!");
                return;
            }
        }
        
        if (skillIndex >= 0)
        {
            chosenSkill = specials.GetChild(skillIndex) as Skill;

            bool support = ListSupport();
            ChooseTargetsList(support);

            if (chosenSkill.GetTeam())
            {                
                battleManager.GetPlayers()[0].GetNode<Sprite>("Marker").Visible = true;
                targetChoose = true;
                return;
            }
            else if (chosenSkill.GetGuardTeam())
            {
                if (stats.GetGuard() == 0)
                {
                    stats.SetGuard(3);       
                    GD.Print(stats.GetCharName() + " is guarding");                                
                }     

                stats.SetStamina(stats.GetStamina() - chosenSkill.GetStaminaDepletion());
                battleManager.NextTurn();                
                return;           
            }
            else if (chosenSkill.GetCounter())
            {
                stats.SetStamina(stats.GetStamina() - chosenSkill.GetStaminaDepletion());
                stats.SetCounter(true);
                battleManager.NextTurn();
                GD.Print(stats.GetCharName() + " is in counter mode!");
                return;
            }
        }
        else
        {
            ChooseTargetsList(false);

            chosenSkill = null;
        }

        someoneIsGuarding = false;

        for (int i = 0; i < battleManager.GetEnemies().Count; i++)
        {
            if (battleManager.GetEnemies()[i].GetNode<Stats>("Stats").GetGuard() > 0)
            {
                someoneIsGuarding = true;
                targetIndex = i;
                battleManager.GetEnemies()[i].GetNode<Sprite>("Marker").Visible = true;
                alreadyTargeted = false;
                targetChoose = true;
                return;
            }
        }
        permSkillIndex = skillIndex;
        battleManager.GetEnemies()[0].GetNode<Sprite>("Marker").Visible = true;
        alreadyTargeted = false;

        targetChoose = true;
    }

    private bool ListSupport()
    {
        bool support = useItem;

        if (chosenSkill != null)
        {
            support = chosenSkill.GetTeam();
        }

        return support;
    }

    public void ChooseItem(int itemIndex)
    {
        permItemIndex = itemIndex;
        useItem = true;
        chosenSkill = null;
        if (permItemIndex != 2)
        {
            battleManager.GetPlayers()[0].GetNode<Sprite>("Marker").Visible = true;
        }
        else
        {
            battleManager.GetDeadPlayers()[0].GetNode<Sprite>("Marker").Visible = true;
        }
        
        alreadyTargeted = false;
        ChooseTargetsList(true);
        targetChoose = true;
    }

    private void ChooseTargetsList(bool team)
    {
        if (team)
        {
            battleManager.SortPlayers();
            targets = battleManager.GetPlayers();  

            if (useItem && permItemIndex == 2)
            {
                targets = battleManager.GetDeadPlayers();
            }

            GD.Print("targets count: " + targets.Count + " permItemIndex: " + permItemIndex);
        }
        else
        {
            targets = battleManager.GetEnemies();
        }
    }

    private async void ChooseTarget(float delta, bool team)
    {
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

                    if (chosenSkill.GetStatChange() || chosenSkill.GetHeal())
                    {
                        if (animation.HasAnimation(stats.GetCharName() + "_Heal"))
                        {
                            animation.Play(stats.GetCharName() + "_Heal");
                            gameManager.GetAudioNode().GetChild<AudioStreamPlayer>(1).Play(0);
                            Task longRunningTask = gameManager.LongRunningOperationAsync((int)Math.Round(animation.GetAnimation(stats.GetCharName() + "_Heal").Length * 1000, MidpointRounding.AwayFromZero));
                            await longRunningTask;
                        }

                        if (!chosenSkill.GetAttackAll())
                        {
                            targets[targetIndex].GetNode<CharacterDamage>("Damage").Debuff(chosenSkill, stats);
                        }
                        else
                        {
                            for (int i = 0; i < targets.Count; i++)
                            {
                                targets[i].GetNode<CharacterDamage>("Damage").Debuff(chosenSkill, stats);
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
                if (animation.HasAnimation(stats.GetCharName() + "_Heal"))
                {
                    marker.Visible = false;
                    animation.Play(stats.GetCharName() + "_Heal");
                    gameManager.GetAudioNode().GetChild<AudioStreamPlayer>(1).Play(0);
                    Task longRunningTask = gameManager.LongRunningOperationAsync((int)Math.Round(animation.GetAnimation(stats.GetCharName() + "_Heal").Length * 1000, MidpointRounding.AwayFromZero));
                    await longRunningTask;
                }

                if (chosenSkill != null)
                {
                    stats.SetStamina(stats.GetStamina() - chosenSkill.GetStaminaDepletion());

                    targets[targetIndex].GetNode<CharacterDamage>("Damage").Support(chosenSkill, true, stats);
                    visible = false;
                }
                else
                {
                    targets[targetIndex].GetNode<CharacterDamage>("Damage").ReceiveItem(permItemIndex);
                    visible = false;
                }                
            }
            
            guard.Visible = visible;
            targetChoose = false;
            attackDir = 0;
            guard.Play("Forward");
            chooseAttackDir = true;
            guardDelay = 0;                  
        }

        if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (chosenSkill != null && chosenSkill.GetAttackAll())
            {
                for (int i = 0; i < targets.Count; i++)
                {                    
                    targets[i].GetNode<Sprite>("Marker").Visible = false;
                }
            }
            else
            {
                targets[targetIndex].GetNode<Sprite>("Marker").Visible = false;
            }
            
            targetIndex = 0;
            
            if (chosenSkill == null)
            {
                gui.ShowAttackMenu();
            }
            else
            {
                gui.ShowSpecialMenu(permSkillIndex);
            }

            targetChoose = false;
        }

        if (someoneIsGuarding || battleManager.GetEnemies().Count == 0 || (chosenSkill != null && chosenSkill.GetAttackAll())) 
        {   
            if(chosenSkill != null && chosenSkill.GetAttackAll() && !alreadyTargeted) 
            {
                for (int i = 0; i < targets.Count; i++)
                {                    
                    targets[i].GetNode<Sprite>("Marker").Visible = true;
                }
                alreadyTargeted = true;
            } 

            return;
        }

        if (Input.IsActionJustPressed("ui_up"))
        {
            if (team)
            {
                TargetDown();
            }
            else
            {
                TargetUp();
            }
        }
        else if (Input.IsActionJustPressed("ui_down"))
        {
            if (team)
            {
                TargetUp();
            }
            else
            {
                TargetDown();                
            }  
        }
    }

    private void TargetUp()
    {
        if (targets.Count <= 1)
        {
            return;
        }

        targets[targetIndex].GetNode<Sprite>("Marker").Visible = false;

        targetIndex++;

        if (targetIndex > targets.Count - 1)
        {
            targetIndex = 0;
        }

        targets[targetIndex].GetNode<Sprite>("Marker").Visible = true;   
    }

    private void TargetDown()
    {
        if (targets.Count <= 1)
        {
            return;
        }
        
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

        if (guardDelay > 0.25f * gameManager.GetVoiceControl() || stats.GetStamina() > 0)
        {
            //GD.Print("Block statement");
            
            if (Input.IsActionPressed("ui_up") || gameManager.GetVoiceControl() > 1 && Input.IsActionJustReleased("ui_up"))
            {
                attackDir = 1;
                guard.Play("Up");
                guardDelay = 0;
            }
            else if (Input.IsActionPressed("ui_down")|| gameManager.GetVoiceControl() > 1 && Input.IsActionJustReleased("ui_down"))
            {
                attackDir = -1;
                guard.Play("Down");
                guardDelay = 0;
            }
            else
            {
                if (gameManager.GetVoiceControl() == 1 || gameManager.GetVoiceControl() > 1 && Input.IsActionJustReleased("ui_right"))
                {                              
                    attackDir = 0;
                    guard.Play("Forward");
                    guardDelay = 0;
                }
            }      
        }         
    }
}
