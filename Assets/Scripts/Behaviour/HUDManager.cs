using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

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
        enemyInfoGroup.alpha = 0;
        tileInfoGroup.alpha = 0;

        SelectionManager.Instance.OnEntitySelect += (x)=> 
        {
            switch (x.data.alignement)
            {
                case Alignement.Enemy:
                    inspectedEnemy = x;
                    break;
                case Alignement.Player:
                    inspectedPlayer = x;
                    break;
                default:
                    break;
            }
            UpdateEntityInfo(x);
        };
        SelectionManager.Instance.OnHoveredEntityChanged += UpdateEntityInfo;
        SelectionManager.Instance.OnCancel += () =>
        {
            inspectedEnemy = null;
            inspectedPlayer = null;

            UpdateEntityInfo(null);
            UpdateEntityInfo(null);
        };

        SelectionManager.Instance.OnHoveredTileChanged += UpdateTileInfo;
    }

    EntityBehaviour inspectedEnemy;
    EntityBehaviour inspectedPlayer;

    TextMeshProUGUI HPTextPlayer;
    TextMeshProUGUI PATextPlayer;

    TextMeshProUGUI HPTextEnemy;
    TextMeshProUGUI PATextEnemy;

    CanvasGroup enemyInfoGroup;
    Tween enemyInfoFade;
    CanvasGroup playerInfoGroup;
    Tween playerInfoFade;

    Image playerIcon;
    Image enemyIcon;

    void UpdateEntityInfo(EntityBehaviour entity)
    {
        if (entity == null)
        {
            if (inspectedEnemy == null)
            {
                enemyInfoFade?.Kill();
                enemyInfoGroup.DOFade(0, 0.05f);
            }
            if (inspectedPlayer == null)
            {
                playerInfoFade?.Kill();
                playerInfoGroup.DOFade(0, 0.05f);
            }

            if (inspectedEnemy != null) UpdateEntityInfo(inspectedEnemy);
            if (inspectedPlayer != null) UpdateEntityInfo(inspectedPlayer);

            return;
        }


        TextMeshProUGUI HPtextMesh = null;
        TextMeshProUGUI PAtextMesh = null;

        Image icon = null;
        CanvasGroup canvasGroup = null;
        Tween fade = null;


        switch (entity.data.alignement)
        {
            case Alignement.Enemy:

                HPtextMesh = HPTextEnemy;
                PAtextMesh = PATextEnemy;

                canvasGroup = enemyInfoGroup;
                fade = enemyInfoFade;

                icon = enemyIcon;

                break;
            case Alignement.Player:

                HPtextMesh = HPTextPlayer;
                PAtextMesh = PATextPlayer;

                canvasGroup = playerInfoGroup;
                fade = playerInfoFade;

                icon = playerIcon;

                break;
            default:
                break;
        }

        if (canvasGroup.alpha != 1)
        {
            fade?.Kill();
            canvasGroup.DOFade(1, 0.05f);
        }

        icon.sprite = entity.data.portrait;

        HPtextMesh.text = entity.CurrentHealth.ToString() + "/" + entity.data.maxHealth;
        PAtextMesh.text = entity.CurrentActionPoints.ToString() + "/" + entity.data.maxActionPoints;
    }

    CanvasGroup tileInfoGroup;
    Tween tileInfoFade;

    void UpdateTileInfo(MapRaycastHit mapHit)
    {
        if (!MapManager.IsInsideMap(mapHit.position))
        {
            tileInfoFade?.Kill();
            tileInfoGroup.DOFade(0, .05f);
            return;
        }
        else
        {
            // TODO : get tile type and display info

            tileInfoFade?.Kill();
            tileInfoGroup.DOFade(1, .05f);
        }
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

            if (GetGroupReferences(tag, HUDReferences[i])) continue;

            if (tag == "IconPlayer") playerIcon = HUDReferences[i].GetComponent<Image>();
            if (tag == "IconEnemy") enemyIcon = HUDReferences[i].GetComponent<Image>();
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
                OnAbilityClicked?.Invoke(abilityNumber);
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

    bool GetGroupReferences(string tag, HUDReferencer reference)
    {
        if (tag.Contains("Group"))
        {
            if (tag == "EnemyInfoGroup")
            {
                enemyInfoGroup = reference.GetComponent<CanvasGroup>();
            }
            if (tag == "PlayerInfoGroup")
            {
                playerInfoGroup = reference.GetComponent<CanvasGroup>();
            }
            if (tag == "TileInfoGroup")
            {
                tileInfoGroup = reference.GetComponent<CanvasGroup>();
            }

            return true;
        }
        return false;
    }
}
