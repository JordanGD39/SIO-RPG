using Godot;
using System;

public class ItemHolder : Node
{
    private int potions = 10;
    public int GetPotions(){return potions;} public void LowerPotions() {potions--;}
    private int staminaPotions = 8;
    public int GetStaminaPotions(){return staminaPotions;} public void LowerStaminaPotions() {staminaPotions--; }
    private int revives = 3;
    public int GetRevives(){return revives;} public void LowerRevives() {revives--;}
    private int neutralizers = 5;
    public int GetNeutralizers() {return neutralizers;} public void LowerNeutralizers() {neutralizers--;}
}
