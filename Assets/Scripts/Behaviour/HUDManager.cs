using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    public Action<int> OnAbilityClicked;

    private void Awake()
    {
        Instance = this;

        GetReferences();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetReferences()
    {
        HUDReferencer[] HUDReferences = FindObjectsOfType<HUDReferencer>();

        for (int i = 0; i < HUDReferences.Length; i++)
        {
            string tag = HUDReferences[i].hudTag;

            if (tag.Contains("Ability"))
            {
                tag = tag.Replace("Ability","");
                int abilityNumber = Int32.Parse(tag);

                Button abilityButton = HUDReferences[i].GetComponent<Button>();

                abilityButton.onClick.AddListener(() =>
                {
                    OnAbilityClicked?.Invoke(abilityNumber);
                });
            }
        }
    }
}
