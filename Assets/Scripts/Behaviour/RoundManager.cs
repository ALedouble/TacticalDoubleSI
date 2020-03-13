using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public enum RoundPhase
{
    Player,
    AI
}

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    public Sequence currentMovementSequence;

    public List<EntityBehaviour> roundEntities = new List<EntityBehaviour>();
    public int currentEntityTurn = 0;

    public RoundPhase phase;
    public int roundNumber;


    private void Awake()
    {
        Instance = this;
    }

    public Action OnPlayerTurn;

    public void StartRound()
    {
        phase = RoundPhase.Player;

        OnPlayerTurn?.Invoke();

        for (int i = 0; i < PlayerTeamManager.Instance.playerEntitybehaviours.Count; i++)
        {
            PlayerTeamManager.Instance.playerEntitybehaviours[i].channelingRoundsLeft--;


            if (PlayerTeamManager.Instance.playerEntitybehaviours[i].channelingRoundsLeft == 0)
            {
                PlayerTeamManager.Instance.playerEntitybehaviours[i].UseAbility(
                    PlayerTeamManager.Instance.playerEntitybehaviours[i].channelingAbility,
                    PlayerTeamManager.Instance.playerEntitybehaviours[i].currentTile);
            }

            PlayerTeamManager.Instance.playerEntitybehaviours[i].stasisRoundsLeft--;

            if (PlayerTeamManager.Instance.playerEntitybehaviours[i].stasisRoundsLeft == 0)
            {

            }
        }

        for (int i = 0; i < MapManager.GetListOfEntity().Count; i++)
        {
            // TODO : carry over a part of previous action points
            MapManager.GetListOfEntity()[i].CurrentActionPoints = Mathf.CeilToInt(MapManager.GetListOfEntity()[i].data.maxActionPoints);


        }

        SelectionManager.Instance.OnEntitySelect += RoundManager.Instance.StartPlayerTurn;

        HUDManager.Instance.OnEndTurnPressed += EndTurn;
    }

    public void StartPlayerTurn(EntityBehaviour entity)
    {
        if (entity.data.alignement != Alignement.Player) return;
        if (entity.IsChannelingBurst || entity.stasis) return;
        SelectionManager.Instance.OnEntitySelect -= StartPlayerTurn;

        entity.OnTurn();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            currentMovementSequence.Kill(true);
        }
    }

    public void EndTurn()
    {
        if (phase == RoundPhase.Player)
        {
            phase = RoundPhase.AI;

            SelectionManager.Instance.OnEntitySelect -= StartPlayerTurn;
            SelectionManager.Instance.OnEntitySelect -= StartPlayerTurn;

            HUDManager.Instance.OnEndTurnPressed -= EndTurn;

            // All player entities have played, make the ai play
            roundEntities[currentEntityTurn].OnTurn();
        }
        else
        {
            currentEntityTurn++;
            // if all AIs have made their turn, launch another round
            if (currentEntityTurn == roundEntities.Count)
            {
                currentEntityTurn = 0;

                StartRound();
            }
            else
            {
                // if not, launch the next ai
                roundEntities[currentEntityTurn].OnTurn();

            }

        }
    }

    public void CheckRemainingEntities()
    {
        Debug.Log(PlayerTeamManager.Instance.playerEntitybehaviours.Count);
        List<EntityBehaviour> ennemies = new List<EntityBehaviour>();
        List<EntityBehaviour> allies = new List<EntityBehaviour>();

        for (int i = 0; i < MapManager.GetListOfEntity().Count; i++)
        {
            if (MapManager.GetListOfEntity()[i].data.alignement == Alignement.Enemy)
            {
                ennemies.Add(MapManager.GetListOfEntity()[i]);
            }

            if (MapManager.GetListOfEntity()[i].data.alignement == Alignement.Player)
            {
                allies.Add(MapManager.GetListOfEntity()[i]);
            }

        }

       if(ennemies.Count <= 0)
        {
            //Win()
            Debug.Log("you win");
        }

       if(PlayerTeamManager.Instance.playerEntitybehaviours.Count <= 1)
       {
            //Loose()
            Debug.Log("you Loose");
        }
    }
}
