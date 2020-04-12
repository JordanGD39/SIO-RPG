using Godot;
using System;
using System.Collections.Generic;

public enum weakness{ SWORD, AXE, STAFF, FIST, FIRE, LIGHTNING, DARK}

public class Stats : Node
{
     [Export] private string charName = "";
     public string GetCharName() { return charName;}
     [Export] private int health = 300;
     public void SetHealth(int h) {if(h <= 0) {h = 0;} health = h; UpdateHealth();} public int GetHealth(){ return health;}
     [Export] private int maxHealth = 300;
     public void SetMaxHealth(int h) {maxHealth = h;} public int GetMaxHealth(){ return maxHealth;}
     [Export] private int stamina = 100;
     public void SetStamina(int s) { if(s > 100) {s = 100;}else if(s < 0) { s= 0;} stamina = s; UpdateStamina();} public int GetStamina(){ return stamina;}
     [Export] private int maxStamina = 100;
     public void SetMaxStamina(int s) {maxStamina = s;} public int GetMaxStamina(){ return maxStamina;}
     [Export] private int atk = 80;
     public void SetAtk(int a) {atk = a;} public int GetAtk(){ return atk;}
     [Export] private int maxAtk = 80;
     public void SetMaxAtk(int a) {maxAtk = a;} public int GetMaxAtk(){ return maxAtk;}
     [Export] private int mag = 80;
     public void SetMag(int m) {mag = m;} public int GetMag(){ return mag;}
     [Export] private int maxMag = 80;
     public void SetMaxMag(int m) {maxMag = m;} public int GetMaxMag(){ return maxMag;}
     [Export] private int def = 80;
     public void SetDef(int d) {def = d;} public int GetDef(){ return def;}
     [Export] private int maxDef = 80;
     public void SetMaxDef(int d) {maxDef = d;} public int GetMaxDef(){ return maxDef;}
     [Export] private int res = 80;
     public void SetRes(int r) {res = r;} public int GetRes(){ return res;}
     [Export] private int maxRes = 80;
     public void SetMaxRes(int r) {maxRes = r;} public int GetMaxRes(){ return maxRes;}
     [Export] private int spd = 80;
     public void SetSpd(int s) {spd = s;} public int GetSpd(){ return spd;}
     [Export] private int maxSpd = 80;
     public void SetMaxSpd(int s) {maxSpd = s;} public int GetMaxSpd(){ return maxSpd;}
     [Export] private int luk = 80;
     public void SetLuk(int l) {luk = l;} public int GetLuk(){ return luk;}
     [Export] private int maxLuk = 80;
     public void SetMaxLuk(int l) {maxLuk = l;} public int GetMaxLuk(){ return maxLuk;}
     [Export] private List<weakness> weaknesses = new List<weakness>();
     public List<weakness> GetWeaknesses() {return weaknesses;} public void SetWeaknesses(List<weakness> a) {weaknesses = a;}
     [Export] private weakness attackElement;
     public weakness GetAttackElement() {return attackElement;}
     private int atkCounter = 3;
     public void SetAtkCounter(int a) {atkCounter = a;}
     private int spdCounter = 3;
     public void SetSpdCounter(int a) {spdCounter = a;}
     private int defCounter = 3;
     public void SetDefCounter(int a) {defCounter = a;}
     private int guardCounter = 0;
     public int GetGuard(){return guardCounter;} public void SetGuard(int a){guardCounter = a;}
     private int stunCounter = 0;
     public int GetStun(){return stunCounter;} public void SetStun(int a){stunCounter = a;}
     private bool countering = false;
     public bool GetCounter(){return countering;} public void SetCounter(bool a){countering = a;}
     private TextureProgress healthBar;
     public TextureProgress GetHealthBar() {return healthBar;} public void SetHealthBar(TextureProgress a) {healthBar = a;}
     private TextureProgress staminaBar;
     public TextureProgress GetStaminaBar() {return staminaBar;} public void SetStaminaBar(TextureProgress a) { staminaBar = a;}
     private Label healthText;
     public void SetHealthText(Label a) {healthText = a;}
     private Label staminaText;
     public void SetStaminaText(Label a) {staminaText = a;}
     [Export] private int hitHappened = 1450; // In miliseconds
     public int GetHitHappened() {return hitHappened;}

     private Node statChangesUI;
     public Node GetStatChangesUI() {return statChangesUI;}

     public override void _Ready()
     {
          statChangesUI = GetParent().GetNode("Stat Changes");
     }

     public void CheckStatBonus()
     {
          if (guardCounter > 0)
          {
              guardCounter--;     
              if (guardCounter <= 0)
              {
                  statChangesUI.GetNode<AnimatedSprite>("Guard").Visible = false;
              }         
          }

          if (stunCounter > 0)
          {
              stunCounter--;     
              if (stunCounter <= 0)
              {
                  statChangesUI.GetNode<AnimatedSprite>("Stun").Visible = false;
              }         
          }
          
          if (atk != maxAtk)
          {
               atkCounter--;
               if (atkCounter <= 0)
               {
                    atk = maxAtk;
                    mag = maxMag;
                    atkCounter = 3;
                    GD.Print("ATK & MAG RESET");
                    statChangesUI.GetNode<AnimatedSprite>("Atk").Visible = false;
               }
          }

          if (spd != maxSpd)
          {
               spdCounter--;
               if (spdCounter <= 0)
               {
                    spd = maxSpd;
                    luk = maxLuk;
                    spdCounter = 3;
                    GD.Print("SPD & LUK RESET");
                    statChangesUI.GetNode<AnimatedSprite>("Spd").Visible = false;
               }
          }

          if (def != maxDef)
          {
               defCounter--;
               if (defCounter <= 0)
               {
                    def = maxDef;
                    res = maxRes;
                    defCounter = 3;
                    GD.Print("DEF & RES RESET");
                    statChangesUI.GetNode<AnimatedSprite>("Def").Visible = false;
               }
          }
     }

     public void UpdateStamina()
     {          
          staminaBar.Value = (float)stamina / (float)maxStamina * 100;
          staminaText.Text = "ST: " + stamina + "/" + maxStamina;
     }
     public void UpdateHealth()
     {
          healthBar.Value = (float)health / (float)maxHealth * 100;
          healthText.Text = "HP: " + health + "/" + maxHealth;
     }
}
