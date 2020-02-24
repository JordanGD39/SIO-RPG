using Godot;
using System;
using System.Collections.Generic;

public enum weakness{ SWORD, AXE, LANCE, STAFF, FIRE, ICE}

public class Stats : Node
{
     [Export] private string charName = "";
     public string GetCharName() { return charName;}
     [Export] private WeaponStats weapon;
     public WeaponStats GetWeapon() { return weapon;}
     [Export] private int health = 300;
     public void SetHealth(int h) {health = h;} public int GetHealth(){ return health;}
     [Export] private int stamina = 100;
     public void SetStamina(int s) {stamina = s;} public int GetStamina(){ return stamina;}
     [Export] private int atk = 80;
     public void SetAtk(int a) {atk = a;} public int GetAtk(){ return atk;}
     [Export] private int mag = 80;
     public void SetMag(int m) {mag = m;} public int GetMag(){ return mag;}
     [Export] private int def = 80;
     public void SetDef(int d) {def = d;} public int GetDef(){ return def;}
     [Export] private int res = 80;
     public void SetRes(int r) {res = r;} public int GetRes(){ return res;}
     [Export] private int spd = 80;
     public void SetSpd(int s) {spd = s;} public int GetSpd(){ return spd;}
     [Export] private int luk = 80;
     public void SetLuk(int l) {luk = l;} public int GetLuk(){ return luk;}

     [Export] private List<weakness> weaknesses = new List<weakness>();

     public List<weakness> GetWeaknesses() {return weaknesses;}
}
