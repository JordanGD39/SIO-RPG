using Godot;
using System;
using System.Collections.Generic;
//using System.Collections.;

class SpeedCompare : IComparer<Node> 
{ 
    public int Compare(Node x, Node y) 
    { 
        if (x == null || y == null) 
        { 
            return 0; 
        } 

        Stats p1 = x.GetNode("Stats") as Stats;
        Stats p2 = y.GetNode("Stats") as Stats;

        return p1.GetSpd().CompareTo(p2.GetSpd());           
    } 
} 

public class BattleManager : Node
{
    [Export] private List<Node> turnOrder = new List<Node>();

    public override void _Ready()
    {
        for (int i = 0; i < GetChildCount(); i++)
        {
            turnOrder.Add(GetChild(i));
        }

        SpeedCompare spdCompare = new SpeedCompare();

        turnOrder.Sort(spdCompare);
        turnOrder.Reverse();

        // for (int i = 0; i < turnOrder.Count; i++)
        // {
        //     Stats a = turnOrder[i].GetNode("Stats") as Stats;
        //     GD.Print(a.GetSpd());
        // }       
        
         
    }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
