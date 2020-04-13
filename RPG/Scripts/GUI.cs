using Godot;
using System;
using System.Collections.Generic;

public class GUI : Node
{    
    private BattleManager battleManager;
    private ItemHolder itemHolder;
    private Node attackMenu;
    private MarginContainer attackMenuContainer;
    private VBoxContainer attackContainer;
    private VBoxContainer specialContainer;
    private VBoxContainer itemContainer;
    private MarginContainer descriptionBox;
    private MarginContainer menu;
    private Label description;
    private float delayTimer = 0;
    private int attackCounter = 0;

    public override void _Ready()
    {
        battleManager = GetParent().GetNode("TurnQueue") as BattleManager;
        itemHolder = GetParent().GetNode("ItemHolder") as ItemHolder;
        attackMenuContainer = GetNode("AttackMenu") as MarginContainer;
        attackMenu =  attackMenuContainer.GetChild(0).GetChild(0);
        attackContainer = attackMenuContainer.GetChild(0).GetChild<VBoxContainer>(0);
        specialContainer = attackMenuContainer.GetChild(0).GetChild<VBoxContainer>(1);
        itemContainer = attackMenuContainer.GetChild(0).GetChild<VBoxContainer>(2);
        attackMenuContainer.Visible = false;
        descriptionBox = GetNode("Description") as MarginContainer;
        description = descriptionBox.GetChild(0).GetChild(0) as Label;
        menu = GetNode("Setup") as MarginContainer;
        menu.GetChild(0).GetChild<Button>(1).GrabFocus();
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

    public void HideSetupMenu()
    {
        menu.Visible = false;
        Button attackButton = attackMenu.GetChild(0) as Button;
        attackButton.GrabFocus();
    }

    public void ShowAttackMenu()
    {
        attackMenuContainer.Visible = true;
        attackContainer.Visible = true;
        specialContainer.Visible = false;
        itemContainer.Visible = false;
        if (!menu.Visible)
        {
            Button attackButton = attackMenu.GetChild(0) as Button;
            attackButton.GrabFocus();
        }        
    }

    public void DissapearAttackMenu()
    {
        attackMenuContainer.Visible = false;
    }

    public void HideDescription()
    {
        attackCounter = 0;
        descriptionBox.Visible = false;
    }

    public void ShowSpecialMenu(int buttonFocus)
    {
        if (buttonFocus < 0)
        {
            buttonFocus = 0;
        }
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

    public void ShowItemMenu()
    {
        attackContainer.Visible = false;
        attackMenuContainer.Visible = true;
        itemContainer.Visible = true;
        itemContainer.GetChild<Button>(0).GrabFocus();
        
        itemContainer.GetChild(0).GetChild<Label>(0).Text = "x " + itemHolder.GetPotions();
        itemContainer.GetChild(1).GetChild<Label>(0).Text = "x " + itemHolder.GetStaminaPotions();
        itemContainer.GetChild(2).GetChild<Label>(0).Text = "x " + itemHolder.GetRevives();
        itemContainer.GetChild(3).GetChild<Label>(0).Text = "x " + itemHolder.GetNeutralizers();
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

    public void ChangeDescriptionText(string text, bool plus)
    {
        descriptionBox.Visible = true;        

        if (plus && attackCounter > 0)
        {
            description.Text += "\n" + text;
        }
        else
        {
            description.Text = text;
        }        

        if (plus)
        {
            attackCounter++;
        }
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
        else if (itemContainer.Visible && Input.IsActionJustPressed("ui_cancel") && delayTimer > 0.25f)
        {
            itemContainer.Visible = false;
            attackContainer.Visible = true;
            attackMenu.GetChild<Button>(2).GrabFocus();
        }     
    }
}
