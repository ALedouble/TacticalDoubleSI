using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    public Action<int> OnAbilityClicked;

    private void Awake()
    {
        Instance = this;

        GetReferences();
    }

    private void Start()
    {
        SelectionManager.Instance.OnEntitySelect += UpdateEntityInfo;
    }

    TextMeshProUGUI HPTextPlayer;
    TextMeshProUGUI PATextPlayer;

    TextMeshProUGUI HPTextEnemy;
    TextMeshProUGUI PATextEnemy;

    void UpdateEntityInfo(EntityBehaviour entity)
    {
        TextMeshProUGUI HPtextMeshToUpdate = null;
        TextMeshProUGUI PAtextMeshToUpdate = null;

        switch (entity.data.alignement)
        {
            case Alignement.Enemy:

                HPtextMeshToUpdate = HPTextEnemy;
                PAtextMeshToUpdate = PATextEnemy;

                break;
            case Alignement.Player:

                HPtextMeshToUpdate = HPTextPlayer;
                PAtextMeshToUpdate = PATextPlayer;

                break;
            case Alignement.Neutral:
                break;
            default:
                break;
        }

        if (HPtextMeshToUpdate == null || PAtextMeshToUpdate == null) return;

        HPtextMeshToUpdate.text = entity.CurrentHealth.ToString() + "/" + entity.data.maxHealth;
        PAtextMeshToUpdate.text = entity.CurrentActionPoints.ToString() + "/" + entity.data.maxActionPoints;
    }

    HUDReferencer[] HUDReferences;
    void GetReferences()
    {
        HUDReferences = FindObjectsOfType<HUDReferencer>();

        for (int i = 0; i < HUDReferences.Length; i++)
        {
            string tag = HUDReferences[i].hudTag;

            if (GetAbilityReferences(tag, HUDReferences[i])) continue;

            if (GetEntityInfoReferences(tag, HUDReferences[i])) continue;
        }
    }

    bool GetAbilityReferences(string tag, HUDReferencer reference)
    {
        if (tag.Contains("Ability"))
        {
            tag = tag.Replace("Ability", "");
            int abilityNumber = Int32.Parse(tag);

            Button abilityButton = reference.GetComponent<Button>();

            abilityButton.onClick.AddListener(() =>
            {
                OnAbilityClicked.Invoke(abilityNumber);
            });

            return true;
        }

        return false;
    }

    bool GetEntityInfoReferences(string tag, HUDReferencer reference)
    {
        switch (tag)
        {
            case "HPTextPlayer":
                HPTextPlayer = reference.GetComponent<TextMeshProUGUI>();
                return true;
            case "PATextPlayer":
                PATextPlayer = reference.GetComponent<TextMeshProUGUI>();
                return true;
            case "HPTextEnemy":
                HPTextEnemy = reference.GetComponent<TextMeshProUGUI>();
                return true;
            case "PATextEnemy":
                PATextEnemy = reference.GetComponent<TextMeshProUGUI>();
                return true;
            default:
                return false;
        }
    }
}
