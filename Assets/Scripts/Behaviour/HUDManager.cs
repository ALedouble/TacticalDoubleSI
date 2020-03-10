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

    public TileDescriptions tileDescriptions;

    public Action<int> OnAbilityClicked;
    public Action OnFinishPlacement;

    private void Awake()
    {
        Instance = this;

        GetReferences();
    }

    private void Start()
    {
        enemyInfoGroup.alpha = 0;
        tileInfoGroup.alpha = 0;
        roundHUDGroup.alpha = 0;
        finishPlacementButton.DOScale(0, 0);

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

        PlayerTeamManager.Instance.OnPlacedAllPlayers += ShowFinishPlacementButton;
        PlayerTeamManager.Instance.OnFinishPlacement += OnFinishPlacementConfirmed;

        RoundManager.Instance.OnPlayerTurn += ShowEndTurnButton;
    }

    CanvasGroup roundHUDGroup;
    RectTransform finishPlacementButton;

    void ShowFinishPlacementButton()
    {
        PlayerTeamManager.Instance.OnPlacedAllPlayers -= ShowFinishPlacementButton;

        finishPlacementButton.DOScale(1, .1f).SetEase(Ease.OutBack);
    }

    void OnFinishPlacementConfirmed()
    {
        roundHUDGroup.DOFade(1, .2f);
        finishPlacementButton.DOScale(0, .2f).SetEase(Ease.InBack);
    }

    public Action OnEndTurnPressed;
    RectTransform endTurnButton;

    void ShowEndTurnButton()
    {
        endTurnButton.DOScale(1, .2f).SetEase(Ease.OutBack);
    }

    void OnEndTurn()
    {
        OnEndTurnPressed?.Invoke();
        endTurnButton.DOScale(0, .2f).SetEase(Ease.InBack);
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
            case Alignement.Enemy :

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
            case Alignement.Neutral:

                HPtextMesh = HPTextEnemy;
                PAtextMesh = PATextEnemy;

                canvasGroup = enemyInfoGroup;
                fade = enemyInfoFade;

                icon = enemyIcon;

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
    Image tilePreview;
    TextMeshProUGUI tileName;
    TextMeshProUGUI tileDescription;


    void UpdateTileInfo(MapRaycastHit mapHit)
    {
        if (mapHit.tile == null || mapHit.tile.TileType == TileType.Solid)
        {
            tileInfoFade?.Kill();
            tileInfoGroup.DOFade(0, .05f);
            return;
        }
        else
        {
            Debug.Log((int)mapHit.tile.TileType - 1);
            //tilePreview.sprite = tileDescriptions.tileSprites[(int)mapHit.tile.TileType-1];
            tileName.text = tileDescriptions.tileNames[(int)mapHit.tile.TileType-1];
            tileDescription.text = tileDescriptions.tileEffects[(int)mapHit.tile.TileType-1];

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

            if (GetButtonReferences(tag, HUDReferences[i])) continue;

            if (tag == "TilePreview") enemyIcon = HUDReferences[i].GetComponent<Image>();
            if (tag == "TileName") tileName = HUDReferences[i].GetComponent<TextMeshProUGUI>();
            if (tag == "TileEffect") tileDescription = HUDReferences[i].GetComponent<TextMeshProUGUI>();
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
            if (tag == "RoundHUDGroup")
            {
                roundHUDGroup = reference.GetComponent<CanvasGroup>();
            }
            return true;
        }
        return false;
    }

    bool GetButtonReferences(string tag, HUDReferencer reference)
    {
        if (tag == "FinishPlacementButton")
        {
            finishPlacementButton = reference.GetComponent<RectTransform>();
            reference.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnFinishPlacement?.Invoke();
            });

            return true;
        }
        if (tag == "EndTurnButton")
        {
            endTurnButton = reference.GetComponent<RectTransform>();
            reference.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnEndTurn();
            });

            return true;
        }

        return false;
    }
}
