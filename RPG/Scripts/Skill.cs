using Godot;
using System;

public class Skill : Node
{
    [Export] private weakness element;
    public weakness GetElement() {return element;}
    [Export] private int lessAcc = 0;
    public int GetLessAcc() {return lessAcc;}
    [Export] private int atk = 0;
    public int GetAtk() {return atk;}
    [Export] private int mag = 0;
    public int GetMag() {return mag;}
    [Export] private int staminaDepletion = 0;
    public int GetStaminaDepletion() {return staminaDepletion;}
    [Export] private bool attackAll = false;
    public bool GetAttackAll() {return attackAll;}
    [Export] private bool counter = false;
    public bool GetCounter() {return counter;}
    [Export] private bool stun = false;
    public bool GetStun() {return stun;}
    [Export] private bool guardTeam = false;
    public bool GetGuardTeam() {return guardTeam;}
    [Export] private bool heal = false;
    public bool GetHeal() {return heal;}
    [Export] private bool team = false;
    public bool GetTeam() {return team;}
    [Export] private bool statChange = false;
    public bool GetStatChange() {return statChange;}
    [Export] private int statBonusAtk = 0;
    public int GetStatBonusAtk() {return statBonusAtk;}
    [Export] private int statBonusMag = 0;
    public int GetStatBonusMag() {return statBonusMag;}
    [Export] private int statBonusDef = 0;
    public int GetStatBonusDef() {return statBonusDef;}
    [Export] private int statBonusRes = 0;
    public int GetStatBonusRes() {return statBonusRes;}
    [Export] private int statBonusSpd = 0;
    public int GetStatBonusSpd() {return statBonusSpd;}
    [Export] private int statBonusLuk = 0;
    public int GetStatBonusLuk() {return statBonusLuk;}
    [Export] private string descriptionText;
    public string GetDescription() {return descriptionText;}
}
