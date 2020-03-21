using Godot;
using System;
using System.Collections.Generic;

public class GUI : Node
{    
    private BattleManager battleManager;
    private Node attackMenu;
    private MarginContainer attackMenuContainer;
    private VBoxContainer attackContainer;
    private VBoxContainer specialContainer;

    private float delayTimer = 0;

    public override void _Ready()
    {
        battleManager = GetParent().GetNode("TurnQueue") as BattleManager;
        attackMenuContainer = GetNode("AttackMenu") as MarginContainer;
        attackMenu =  attackMenuContainer.GetChild(0).GetChild(0);
        attackContainer = attackMenuContainer.GetChild(0).GetChild<VBoxContainer>(0);
        specialContainer = attackMenuContainer.GetChild(0).GetChild<VBoxContainer>(1);
        attackMenuContainer.Visible = false;
    }
    public void ChangeNames(List<Node> players, List<Node> enemies)
    {
        Node hpBars = GetChild(0).GetChild(0);

        for (int i = 0; i < hpBars.GetChildCount(); i++)
        {
            Stats stats = players[i].GetNode<Stats>("Stats");
            stats.SetHealthBar(hpBars.GetChild(i).GetNode<TextureProgress>("HealthBar"));
            stats.SetStaminaBar(hpBars.GetChild(i).GetNode<TextureProgress>("StaminaBar"));
            stats.SetHealthText(hpBars.GetChild(i).GetNode<Label>("HP"));
            stats.SetStaminaText(hpBars.GetChild(i).GetNode<Label>("ST"));
            Label label = hpBars.GetChild(i).GetChild(0) as Label;
            label.Text = stats.GetCharName();
            stats.UpdateHealth();
            stats.UpdateStamina();
        }

        hpBars = GetChild(1).GetChild(0);

        for (int i = 0; i < hpBars.GetChildCount(); i++)
        {
            Stats stats = enemies[i].GetNode<Stats>("Stats");
            stats.SetHealthBar(hpBars.GetChild(i).GetNode<TextureProgress>("HealthBar"));
            stats.SetStaminaBar(hpBars.GetChild(i).GetNode<TextureProgress>("StaminaBar"));
            stats.SetHealthText(hpBars.GetChild(i).GetNode<Label>("HP"));
            stats.SetStaminaText(hpBars.GetChild(i).GetNode<Label>("ST"));
            Label label = hpBars.GetChild(i).GetChild(0) as Label;
            label.Text = stats.GetCharName();
            stats.UpdateHealth();
            stats.UpdateStamina();
        }
    }

    public void ShowAttackMenu()
    {
        attackMenuContainer.Visible = true;
        attackContainer.Visible = true;
        specialContainer.Visible = false;
        Button attackButton = attackMenu.GetChild(0) as Button;
        attackButton.GrabFocus();
    }

    public void DissapearAttackMenu()
    {
        attackMenuContainer.Visible = false;
    }

    public void ShowSpecialMenu(int buttonFocus)
    {
        delayTimer = 0;
        Node specials = battleManager.GetTurnOrder()[battleManager.GetCurrTurn()].GetNode("Special Moves");
        attackContainer.Visible = false;
        attackMenuContainer.Visible = true;
        specialContainer.Visible = true;
        specialContainer.GetChild<Button>(buttonFocus).GrabFocus();
        
        for (int i = 0; i < specialContainer.GetChildCount(); i++)
        {
            Button button = specialContainer.GetChild(i) as Button;
            button.Text = " " + specials.GetChild(i).Name;
            button.GetChild<Label>(0).Text = specials.GetChild<Skill>(i).GetStaminaDepletion() + " ST";
        }
    }

    public void GiveUIToNewCharacter(Node enemy)
    {
        Stats stats = enemy.GetNode<Stats>("Stats");
        Node hpBars = GetChild(1).GetChild(0);        
        VBoxContainer firstHealthBar = hpBars.GetChild<VBoxContainer>(1);
        
        int i = 1; 

        if (firstHealthBar.Visible)
        {
            i = 2;
        }

        stats.SetHealthBar(hpBars.GetChild(i).GetNode<TextureProgress>("HealthBar"));
        stats.SetStaminaBar(hpBars.GetChild(i).GetNode<TextureProgress>("StaminaBar"));
        stats.SetHealthText(hpBars.GetChild(i).GetNode<Label>("HP"));
        stats.SetStaminaText(hpBars.GetChild(i).GetNode<Label>("ST"));
        Label label = hpBars.GetChild(i).GetChild(0) as Label;
        label.Text = stats.GetCharName();
        stats.UpdateHealth();
        stats.UpdateStamina();
        hpBars.GetChild<VBoxContainer>(i).Visible = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (delayTimer < 1)
        {
            delayTimer += delta;
        }        

        if (specialContainer.Visible && Input.IsActionJustPressed("ui_cancel") && delayTimer > 0.25f)
        {
            specialContainer.Visible = false;
            attackContainer.Visible = true;
            attackMenu.GetChild<Button>(1).GrabFocus();
        }

        if (Input.IsKeyPressed(16777237))
        {
           attackMenu.GetChild<Button>(0).ReleaseFocus(); 
        }        
    }
}
