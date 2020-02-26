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
    public List<Node> GetPlayers() {return players;}

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
                Player playerScript = turnOrder[i] as Player;
                switch (i)
                {
                    case 0:
                    playerTransform.Position = new Vector2(580, 455);
                    playerScript.SetStartTarget(new Vector2(580, 455));
                    playerTransform.Scale = new Vector2(0.91f, 0.91f);
                    break;
                    case 1:
                    playerTransform.Position = new Vector2(530, 600);
                    playerScript.SetStartTarget(new Vector2(530, 600));
                    playerTransform.Scale = new Vector2(0.94f, 0.94f);
                    break;
                    case 2:
                    playerTransform.Position = new Vector2(480, 750);
                    playerScript.SetStartTarget(new Vector2(480, 750));
                    playerTransform.Scale = new Vector2(0.97f, 0.97f);
                    break;
                    case 3:
                    playerTransform.Position = new Vector2(430, 920);
                    playerScript.SetStartTarget(new Vector2(430, 920));
                    break;
                }
            }    

            GetChild(i).GetNode<CharacterDamage>("Damage").GetMyHealthBar(); 
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
        Player player = turnOrder[currTurn] as Player;
        player.SetTimer(0);
        player.SetTargetChoose(true);
        enemies[0].GetNode<Sprite>("Marker").Visible = true;
    }

    public void NextTurn(bool playerControl)
    {
        Player prevPlayer = turnOrder[currTurn] as Player;

        if (prevPlayer != null)
        {
            prevPlayer.SetGoToStartPos(true);
        }
        
        currTurn++;
        
        if (currTurn > turnOrder.Count - 1)
        {
            currTurn = 0;
        } 

        Player player = turnOrder[currTurn] as Player;

        //If enemy
        if(prevPlayer == null)
        {
            if (player != null)
            {
                player.SetGoToMid(true); 
            }
            else
            {
                EnemyAI enemy = turnOrder[currTurn] as EnemyAI;
                enemy.MyTurn();
            }                 
        }         
    }

    public void CurrCharacterGoesToMid()
    {
        Player player = turnOrder[currTurn] as Player;

        if (player != null)
        {
            player.SetGoToMid(true);
        }
        else
        {
            EnemyAI enemy = turnOrder[currTurn] as EnemyAI;
            enemy.MyTurn();
        }
    }

    public void TakeMeOutList(Node charachter, bool playerControl)
    {
        turnOrder.Remove(charachter);

        if (playerControl)
        {
            players.Remove(charachter);

            if (players.Count == 0)
            {
                GetTree().Quit();
            }
        }
        else
        {
            enemies.Remove(charachter);
            
            if (enemies.Count == 0)
            {
                GetTree().Quit();
            }
        }
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
