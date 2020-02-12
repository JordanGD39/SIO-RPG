using Godot;
using System;

public class Player : KinematicBody2D
{
    private bool goToMid = false;
    private Vector2 target = new Vector2(680, 700);

    private GUI gui;
    private float speed = 10000;

    public override void _Ready()
    {
        gui = GetParent().GetParent().GetNode("UI") as GUI;
    }

    public void MyTurn()
    {
        goToMid = true;
    }

 // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (goToMid)
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
    }
}
