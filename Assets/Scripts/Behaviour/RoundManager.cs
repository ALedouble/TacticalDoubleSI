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

        for (int i = 0; i < MapManager.GetListOfEntity().Count; i++)
        {
            // TODO : carry over a part of previous action points
            MapManager.GetListOfEntity()[i].CurrentActionPoints = MapManager.GetListOfEntity()[i].data.maxActionPoints;
        }

        SelectionManager.Instance.OnEntitySelect += StartPlayerTurn;

        HUDManager.Instance.OnEndTurnPressed += EndTurn;
    }

    void StartPlayerTurn(EntityBehaviour entity)
    {
        if (entity.data.alignement != Alignement.Player) return;

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
}
