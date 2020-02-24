using Godot;
using System;

public class Player : KinematicBody2D
{
    private bool goToMid = false;

    private bool targetChoose = false;
    private int targetIndex = 0;

    public void SetTargetChoose(bool a) {targetChoose = a;}

    public void SetGoToMid(bool a) {goToMid = a;}
    private Vector2 target = new Vector2(680, 700);
    private GUI gui;
    private Stats stats;
    private BattleManager battleManager;
    private float speed = 10000;

    private float timer = 0;

    public override void _Ready()
    {
        gui = GetParent().GetParent().GetNode("UI") as GUI;
        battleManager = GetParent() as BattleManager;
        stats = GetNode("Stats") as Stats;
    }

 // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (goToMid)
        {
            GoToMiddle(delta);
        }

        if (targetChoose)
        {
            ChooseTarget(delta);
        }     
    }

    private void GoToMiddle(float delta)
    {
        Vector2 vector = (target - Position).Normalized();
        MoveAndSlide(vector * speed * delta);

        if (Position.x > target.x)
        {
            GD.Print("There");
            goToMid = false;
            gui.ShowAttackMenu(GetNode<Stats>("Stats"));
        }
    }

    private void ChooseTarget(float delta)
    {
        if (timer < 0.3f)
        {
            timer += delta;
        }        

        if (Input.IsActionJustPressed("ui_accept") && timer > 0.25f)
        {
            battleManager.GetEnemies()[targetIndex].GetNode<CharacterDamage>("Damage").ReceiveDamage(stats);
        }

        if (battleManager.GetEnemies().Count == 0) {return;}

        if (Input.IsActionJustPressed("ui_up"))
        {
            battleManager.GetEnemies()[targetIndex].GetNode<Sprite>("Marker").Visible = false;

            targetIndex++;

            if (targetIndex > battleManager.GetEnemies().Count - 1)
            {
                targetIndex = 0;
            }

            battleManager.GetEnemies()[targetIndex].GetNode<Sprite>("Marker").Visible = true;            
        }
        else if (Input.IsActionJustPressed("ui_down"))
        {
            battleManager.GetEnemies()[targetIndex].GetNode<Sprite>("Marker").Visible = false;

            targetIndex--;

            if (targetIndex < 0)
            {
                targetIndex = battleManager.GetEnemies().Count - 1;
            }
    
            battleManager.GetEnemies()[targetIndex].GetNode<Sprite>("Marker").Visible = true;            
        }
    }
}
