using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance;

    public Sequence currentMovementSequence;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            currentMovementSequence.Kill(true);
        }
    }

    internal static void EndTurn(EntityBehaviour minion)
    {
        throw new NotImplementedException();
    }
}
