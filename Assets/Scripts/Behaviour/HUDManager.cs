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

    [HideInInspector] public Canvas canvas;

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
        canvas = FindObjectOfType<Canvas>();

        playerInfoGroup.alpha = 0;
        //enemyInfoGroup.alpha = 0;
        tileInfoGroup.alpha = 0;
        roundHUDGroup.alpha = 0;
        finishPlacementButton.DOScale(0, 0);

        SelectionManager.Instance.OnEntitySelect += UpdateEntityInfo;

        //SelectionManager.Instance.OnHoveredEntityChanged += UpdateEntityInfo;
        SelectionManager.Instance.OnCancel += () =>
        {
            inspectedEnemy = null;
            inspectedPlayer = null;

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
    Tween endTurnButtonTween;

    public void ShowEndTurnButton()
    {
        endTurnButtonTween?.Kill();
        endTurnButtonTween = endTurnButton.DOScale(1, .2f).SetEase(Ease.OutBack);
    }

    void OnEndTurn()
    {
        endTurnButtonTween?.Kill();
        endTurnButtonTween = endTurnButton.DOScale(0, .2f).SetEase(Ease.InBack).OnComplete(()=>
        {
            OnEndTurnPressed?.Invoke();
        });
    }

    public void HideEndTurnButton()
    {
        endTurnButtonTween?.Kill();
        endTurnButtonTween = endTurnButton.DOScale(0, .2f).SetEase(Ease.InBack);
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

    public void UpdateEntityInfo(EntityBehaviour entity)
    {
        if (entity == null)
        {
            if (inspectedEnemy == null)
            {
                //enemyInfoFade?.Kill();
                //enemyInfoGroup.DOFade(0, 0.05f);
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

        switch (entity.data.alignement)
        {
            case Alignement.Enemy:
                inspectedEnemy = entity;
                break;
            case Alignement.Player:
                inspectedPlayer = entity;
                break;
            default:
                break;
        }

        TextMeshProUGUI HPtextMesh = null;
        TextMeshProUGUI PAtextMesh = null;

        Image icon = null;
        CanvasGroup canvasGroup = null;
        Tween fade = null;


        switch (entity.data.alignement)
        {
            case Alignement.Enemy :

                return;
                
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

                //if (inspectedPlayer != null) return;

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
        icon.preserveAspect = true;

        string healthString = "<size=50><color=#59c23d>" + entity.CurrentHealth + "</color></size>" + "/" + entity.data.maxHealth;
        string paString = "<size=50><color=#FFF32F>" + entity.CurrentActionPoints + "</color></size>" + "/" + entity.data.maxActionPoints;

        HPtextMesh.text = healthString;
        PAtextMesh.text = paString;
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

    public GameObject displayValueHUDPrefab;

    public static void DisplayValue(string value, Color color, Vector3 position)
    {
        RectTransform rect = PoolManager.InstantiatePooled(HUDManager.Instance.displayValueHUDPrefab, Vector3.zero).GetComponent<RectTransform>();

        rect.parent = Instance.transform;
        rect.localScale = Vector3.one * 1;
        rect.anchoredPosition = Instance.canvas.WorldToCanvas(position);

        TextMeshProUGUI textMesh = rect.GetComponent<TextMeshProUGUI>();
        textMesh.text = value;
        textMesh.color = color;

        rect.DOAnchorPosY(rect.anchoredPosition.y + 1, .5f).SetEase(Ease.OutBack, 100);
        textMesh.DOFade(0, 1f).SetDelay(.5f).OnComplete(()=>
        {
            PoolManager.Recycle(rect.gameObject);
        });
    }
}
