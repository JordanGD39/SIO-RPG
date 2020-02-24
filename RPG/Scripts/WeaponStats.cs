using Godot;
using System;

public class WeaponStats : Node
{
    [Export] private string weaponName = "";
     public string GetWeaponName() { return weaponName;}
     [Export] private int atk = 10;
     public void SetAtk(int a) {atk = a;} public int GetAtk(){ return atk;}
     [Export] private int mag = 10;
     public void SetMag(int m) {mag = m;} public int GetMag(){ return mag;}
}
