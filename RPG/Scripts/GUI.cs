using Godot;
using System;
using System.Collections.Generic;

public class GUI : Node
{    
    private Node playerHP;
    private Node attackMenu;
    private MarginContainer attackMenuContainer;

    public override void _Ready()
    {
        attackMenuContainer = GetNode("AttackMenu") as MarginContainer;
        attackMenu =  attackMenuContainer.GetChild(0).GetChild(0);
        attackMenuContainer.Visible = false;
    }
    public void ChangeNames(List<Node> players)
    {
        playerHP = GetChild(0).GetChild(0);

        for (int i = 0; i < playerHP.GetChildCount(); i++)
        {
            CharacterDamage damageScript = players[i].GetNode<CharacterDamage>("Damage");
            damageScript.SetHealthBar(playerHP.GetChild(i).GetNode<TextureProgress>("HealthBar"));
            damageScript.SetStaminaBar(playerHP.GetChild(i).GetNode<TextureProgress>("StaminaBar"));
            Label label = playerHP.GetChild(i).GetChild(0) as Label;
            Stats stats = players[i].GetNode("Stats") as Stats;
            label.Text = stats.GetCharName();
        }
    }

    public void ShowAttackMenu(Stats stats)
    {
        attackMenuContainer.Visible = true;
        Button attackButton = attackMenu.GetChild(0) as Button;
        attackButton.GrabFocus();
    }

    public void DissapearAttackMenu()
    {
        attackMenuContainer.Visible = false;
    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
