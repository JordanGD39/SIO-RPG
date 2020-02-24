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
    private GUI gui;
    private List<Node> turnOrder = new List<Node>();
    private List<Node> players = new List<Node>();
    private List<Node> enemies = new List<Node>();

    public List<Node> GetEnemies() {return enemies;}

    private int currTurn = 0;

    public override void _Ready()
    {
        for (int i = 0; i < GetChildCount(); i++)
        {
            turnOrder.Add(GetChild(i));

            if (i > 3) { enemies.Add(GetChild(i));}
            else
            {
                players.Add(GetChild(i));

                Node2D playerTransform = turnOrder[i] as Node2D;
                
                switch (i)
                {
                    case 0:
                    playerTransform.Position = new Vector2(580, 455);
                    playerTransform.Scale = new Vector2(0.91f, 0.91f);
                    break;
                    case 1:
                    playerTransform.Position = new Vector2(530, 600);
                    playerTransform.Scale = new Vector2(0.94f, 0.94f);
                    break;
                    case 2:
                    playerTransform.Position = new Vector2(480, 750);
                    playerTransform.Scale = new Vector2(0.97f, 0.97f);
                    break;
                    case 3:
                    playerTransform.Position = new Vector2(430, 920);
                    break;
                }
            }           
        }

        GD.Print(enemies.Count);

        gui = GetParent().GetNode("UI") as GUI;
        gui.ChangeNames(turnOrder);

        SpeedCompare spdCompare = new SpeedCompare();

        turnOrder.Sort(spdCompare);
        turnOrder.Reverse();

        for (int i = 0; i < turnOrder.Count; i++)
        {
            Stats a = turnOrder[i].GetNode("Stats") as Stats;
            GD.Print(a.GetSpd() + " " + turnOrder[i].Name);
        }

        Player player = turnOrder[0] as Player;

        player.SetGoToMid(true);
    }

    public void AttackPressed()
    {
        GD.Print(turnOrder[currTurn].GetNode<Stats>("Stats").GetCharName() + " is going to attack!");
        gui.DissapearAttackMenu();
        Player player = turnOrder[0] as Player;
        player.SetTargetChoose(true);
        enemies[0].GetNode<Sprite>("Marker").Visible = true;
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
