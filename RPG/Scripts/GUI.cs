using Godot;
using System;
using System.Collections.Generic;

public class GUI : Node
{    
    private BattleManager battleManager;
    private Node attackMenu;
    private MarginContainer attackMenuContainer;

    public override void _Ready()
    {
        battleManager = GetParent().GetNode("TurnQueue") as BattleManager;
        attackMenuContainer = GetNode("AttackMenu") as MarginContainer;
        attackMenu =  attackMenuContainer.GetChild(0).GetChild(0);
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

    public void ShowAttackMenu(Stats stats)
    {
        attackMenuContainer.Visible = true;
        Node panel = attackMenuContainer.GetChild(0);
        panel.GetChild<VBoxContainer>(0).Visible = true;
        panel.GetChild<VBoxContainer>(1).Visible = false;
        Button attackButton = attackMenu.GetChild(0) as Button;
        attackButton.GrabFocus();
    }

    public void DissapearAttackMenu()
    {
        attackMenuContainer.Visible = false;
    }

    public void ShowSpecialMenu()
    {
        Node specials = battleManager.GetTurnOrder()[battleManager.GetCurrTurn()].GetNode("Special Moves");
        Node panel = attackMenuContainer.GetChild(0);
        panel.GetChild<VBoxContainer>(0).Visible = false;
        panel.GetChild<VBoxContainer>(1).Visible = true;
        panel.GetChild(1).GetChild<Button>(0).GrabFocus();
        for (int i = 0; i < panel.GetChild(1).GetChildCount(); i++)
        {
            Button button = panel.GetChild(1).GetChild(i) as Button;
            button.Text = " " + specials.GetChild(i).Name;
            button.GetChild<Label>(0).Text = specials.GetChild<Skill>(i).GetStaminaDepletion() + " ST";
        }
    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
