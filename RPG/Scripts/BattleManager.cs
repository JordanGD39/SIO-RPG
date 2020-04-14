using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

class IndexCompare : IComparer<Node> 
{ 
    public int Compare(Node x, Node y) 
    { 
        if (x == null || y == null) 
        { 
            return 0; 
        } 

        return x.GetIndex().CompareTo(y.GetIndex());           
    } 
}
public class BattleManager : Node
{
    private GUI gui;
    private ItemHolder itemHolder;
    private List<Node> turnOrder = new List<Node>();
    private List<Node> players = new List<Node>();
    private List<Node> deadPlayers = new List<Node>();
    private List<Node> enemies = new List<Node>();
    public List<Node> GetTurnOrder() {return turnOrder;}
    public List<Node> GetEnemies() {return enemies;}
    public List<Node> GetPlayers() {return players;}
    public List<Node> GetDeadPlayers() {return deadPlayers;}
    private int currTurn = 0;
    public int GetCurrTurn() {return currTurn;}
    private int attacksForNextTurn = 0;

    private SpeedCompare spdCompare = new SpeedCompare();

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

            GetChild(i).GetNode<CharacterDamage>("Damage").GetMyGuard(); 
        }

        GD.Print(enemies.Count);

        gui = GetParent().GetNode("UI") as GUI;
        itemHolder = GetParent().GetNode("ItemHolder") as ItemHolder;
        gui.ChangeNames(players, enemies);

        SortTurnOrder();
         
        SortPlayers();
    }
    
    public void GiveFirstTurn()
    {
        Stats bossStats = enemies[0].GetNode<Stats>("Stats");

        if (GetParent<GameManager>().GetDifficulty() == 0)
        {
            bossStats.SetAtk(200);
            bossStats.SetMag(160);
        }
        else if (GetParent<GameManager>().GetDifficulty() == 1)
        {
            bossStats.SetAtk(225);
            bossStats.SetMag(185);
        }

        bossStats.SetMaxAtk(bossStats.GetAtk());
        bossStats.SetMaxMag(bossStats.GetMag());
        
        Player player = turnOrder[0] as Player;

        player.SetGoToMid(true);  
    }

    public void AttackPressed(int i)
    {
        GD.Print(turnOrder[currTurn].GetNode<Stats>("Stats").GetCharName() + " is going to attack! " + i);
        gui.DissapearAttackMenu();
        Player player = turnOrder[currTurn] as Player;
        player.SetTimer(0);
        player.ChooseSkill(i);
    }

    public void ItemUsed(int i)
    {
        switch (i)
        {
            case 0:
            if (itemHolder.GetPotions() > 0)
            {
                itemHolder.LowerPotions();
            }
            else
            {
                return;
            }
            break;
            case 1:
            if (itemHolder.GetStaminaPotions() > 0)
            {
                itemHolder.LowerStaminaPotions();
            }
            else
            {
                return;
            }
            break;
            case 2:
            if (itemHolder.GetRevives() > 0 && deadPlayers.Count > 0)
            {
                itemHolder.LowerRevives();
            }
            else
            {
                return;
            }
            break;
            case 3:
            if (itemHolder.GetNeutralizers() > 0)
            {
                itemHolder.LowerNeutralizers();
            }
            else
            {
                return;
            }
            break;
        }
        GD.Print(turnOrder[currTurn].GetNode<Stats>("Stats").GetCharName() + " is going to use an item! " + i);
        gui.DissapearAttackMenu();
        Player player = turnOrder[currTurn] as Player;
        player.SetTimer(0);
        player.ChooseItem(i);
    }

    public void ChangeDesOnSkill(int i)
    {
        gui.ChangeDescriptionText(turnOrder[currTurn].GetNode("Special Moves").GetChild<Skill>(i).GetDescription(), false);
    }

    public void NextTurn(Node prevCharacter)
    {
        gui.HideDescription();
        attacksForNextTurn = 0;

        if (currTurn > turnOrder.Count - 1)
        {
            currTurn--;
        } 

        Player prevPlayer = prevCharacter as Player;

        if (prevPlayer != null)
        {
            prevPlayer.SetGoToStartPos(true);
        }
        
        currTurn++;
        
        if (currTurn > turnOrder.Count - 1)
        {
            currTurn = 0;          
            SortTurnOrder();  
        } 

        Player player = turnOrder[currTurn] as Player;

        //If previous attacker is enemy
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

    private void SortTurnOrder()
    {
        turnOrder.Sort(spdCompare);
        turnOrder.Reverse();
        GD.Print("\n");
        for (int i = 0; i < turnOrder.Count; i++)
        {
            Stats a = turnOrder[i].GetNode("Stats") as Stats;
            GD.Print(a.GetSpd() + " " + turnOrder[i].Name);
        }
        GD.Print("\n");
    }

    public void SortPlayers()
    {
        IndexCompare indexCompare = new IndexCompare();
        players.Sort(indexCompare);
        deadPlayers.Sort(indexCompare);
    }

    public void CheckIfNextTurn(bool team, Node prevCharacter)
    {       
        attacksForNextTurn++;

        List<Node> targets = null;

        if (team)
        {
            targets = players;
        }
        else
        {
            targets = enemies;
        }

        if (attacksForNextTurn >= targets.Count)
        {
            NextTurn(prevCharacter);
            attacksForNextTurn = 0;
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

    public async void TakeMeOutList(Node charachter, bool playerControl)
    {
        turnOrder.Remove(charachter);
        charachter.GetNode<Stats>("Stats").ResetStats();
        GetParent<GameManager>().GetAudioNode().GetNode<AudioStreamPlayer>("Death").Play(0);
        if (playerControl)
        {
            players.Remove(charachter);
            AnimationPlayer anim = charachter.GetChild(0).GetChild<AnimationPlayer>(0);
            anim.Play("Death");
            deadPlayers.Add(charachter);

            if (players.Count == 0)
            {
                GetTree().Quit();
            }
        }
        else
        {
            enemies.Remove(charachter);
            Stats enemyStats = charachter.GetNode<Stats>("Stats");
            AnimationPlayer anim = charachter.GetChild(0).GetChild<AnimationPlayer>(0);
            anim.Play("Death");
            Task animDelay = GetParent<GameManager>().LongRunningOperationAsync((int)Math.Round(anim.GetAnimation("Death").Length * 1000, MidpointRounding.AwayFromZero));
            await animDelay;

            enemyStats.GetHealthBar().GetParent<VBoxContainer>().Visible = false;                
            charachter.QueueFree();

            if (enemies.Count == 0)
            {
                GetTree().Quit();
            }
        }
    }

    public void PutMeInList(Node charachter)
    {
        AnimationPlayer anim = charachter.GetChild(0).GetChild<AnimationPlayer>(0);
        anim.PlayBackwards("Death");
        turnOrder.Add(charachter);
        players.Add(charachter);
        deadPlayers.Remove(charachter);
        SortPlayers();
    }

 // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      if (Input.IsActionJustReleased("ui_accept"))
//         {
//             GD.Print("-----");
//             for (int i = 0; i < players.Count; i++)
//             {
//                 GD.Print(players[i].GetNode<Stats>("Stats").GetCharName());
//             }  
//         }
//  }
}
