using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MapManager.Instance.Init();

        PlayerTeamManager.Instance.BeginPlacement();

        PlayerTeamManager.Instance.OnFinishPlacement += OnFinishedPlacement;
    }

    void OnFinishedPlacement()
    {
        PlayerTeamManager.Instance.OnFinishPlacement -= OnFinishedPlacement;

        RoundManager.Instance.StartRound();
    }
}
