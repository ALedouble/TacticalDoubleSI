﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [HideInInspector] public Canvas canvas;

    public TileDescriptions tileDescriptions;

    public List<Color> abilityColors = new List<Color>();

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
        enemyInfoGroup.alpha = 0;
        tileInfoGroup.alpha = 0;
        roundHUDGroup.alpha = 0;
        abilityPopupGroup.alpha = 0;
        blastoutPopupGroup.alpha = 0;
        finishPlacementButton.DOScale(0, 0);
        abilityOutline.gameObject.SetActive(false);
        hpDisplay.transform.parent.gameObject.SetActive(false);
        PADisplay.gameObject.SetActive(false);

        SelectionManager.Instance.OnEntitySelect += UpdateEntityInfo;

        //SelectionManager.Instance.OnHoveredEntityChanged += UpdateEntityInfo;
        SelectionManager.Instance.OnCancel += () =>
        {
            inspectedEnemy = null;
            inspectedPlayer = null;

            UpdateEntityInfo(null);
        };

        SelectionManager.Instance.OnHoveredTileChanged += UpdateTileInfo;
        SelectionManager.Instance.OnHoveredTileChanged += UpdateHPDisplay;

        PlayerTeamManager.Instance.OnPlacedAllPlayers += ShowFinishPlacementButton;
        PlayerTeamManager.Instance.OnFinishPlacement += OnFinishPlacementConfirmed;

        RoundManager.Instance.OnPlayerTurn += ShowEndTurnButton;

        XPFill.fillAmount = 0;
        PlayerTeamManager.Instance.OnXPChanged += UpdateFill;
    }

    Image XPFill;

    void UpdateFill()
    {
        XPFill.DOFillAmount((float)PlayerTeamManager.Instance.teamXp / 10f, .1f);
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
        endTurnButtonTween = endTurnButton.DOScale(0, .2f).SetEase(Ease.InBack).OnComplete(() =>
        {
            OnEndTurnPressed?.Invoke();
        });
    }

    public void HideEndTurnButton()
    {
        endTurnButtonTween?.Kill();
        endTurnButtonTween = endTurnButton.DOScale(0, .2f).SetEase(Ease.InBack);
    }

    void UpdateHPDisplay(MapRaycastHit mapHit)
    {
        if (mapHit.tile == null || mapHit.tile.entities.Count <= 0 || mapHit.tile.entities[0].data.alignement == Alignement.Neutral)
        {
            hpDisplay.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            hpDisplay.transform.parent.gameObject.SetActive(true);

            currentHoveredTile = mapHit.tile;
            hpDisplay.transform.parent.GetComponent<RectTransform>().anchoredPosition = canvas.WorldToCanvas(new Vector3(mapHit.position.x, 2, mapHit.position.y));
        }
    }

    TileData currentHoveredTile;
    TextMeshProUGUI hpDisplay;

    [HideInInspector] public TextMeshProUGUI PADisplay;

    private void Update()
    {
        hpDisplay.text = currentHoveredTile != null ? 
            currentHoveredTile.entities.Count > 0 ?
            currentHoveredTile.entities[0].CurrentHealth.ToString() : "/" : "/";
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

        if (entity.data.alignement == Alignement.Neutral) return;

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

                int playerIndex = PlayerTeamManager.Instance.GetPlayerIndex(entity);
                Ability ability = null;

                for (int j = 0; j < 3; j++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        ability = PlayerTeamManager.Instance.playerProgression[playerIndex].abilityProgression[j].abilities[x];
                        abilityDescriptions[j, x] = ability.description;
                        abilityDescriptions[j, x] = abilityDescriptions[j, x].Replace("[damage]", (Mathf.Ceil(CombatUtils.ComputeDamage(entity, ability))).ToString());
                        abilityDescriptions[j, x] = abilityDescriptions[j, x].Replace("[heal]", CombatUtils.ComputeHeal(entity, ability).ToString());
                        abilityDescriptions[j, x] = abilityDescriptions[j, x].Replace("[armor]", "1");
                    }
                    abilityTitles[j] = PlayerTeamManager.Instance.playerProgression[playerIndex].abilityProgression[j].abilities[0].displayName;

                    abilityCosts[j].text = PlayerTeamManager.Instance.playerProgression[playerIndex].abilityProgression[j].abilities[0].cost.ToString();
                }

                for (int i = 0; i < 3; i++)
                {
                    abilitySprites[i].sprite = entity.data.abilities[i].displaySprite;
                    Color originalColor = abilitySprites[i].color;
                    if (entity.CurrentActionPoints < entity.data.abilities[i].cost) abilitySprites[i].color = new Color(originalColor.r, originalColor.g, originalColor.b, .2f);
                    else abilitySprites[i].color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                }

                ability = PlayerTeamManager.Instance.playerProgression[playerIndex].abilityProgression[3].abilities[0];
                blastoutDescription = ability.description;

                blastoutDescription = blastoutDescription.Replace("[damage]", (Mathf.Ceil(CombatUtils.ComputeDamage(entity, ability))).ToString());
                blastoutDescription = blastoutDescription.Replace("[heal]", CombatUtils.ComputeHeal(entity, ability).ToString());
                blastoutDescription = blastoutDescription.Replace("[armor]", "1");

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


    Tween abilityPopupFade;
    Tween blastoutPopupFade;
    void ShowAbilityPopup(int index)
    {
        if (index < 3)
        {
            for (int i = 0; i < 4; i++)
            {
                abilityDescriptionsText[i].text = abilityDescriptions[index, i];
            }
            abilityTitle.text = abilityTitles[index];
            abilityGlow.color = abilityColors[index];

            abilityPopupFade?.Kill();
            abilityPopupFade = abilityPopupGroup.DOFade(1, .1f);
        }
        else
        {
            blastoutDescriptionText.text = blastoutDescription;

            blastoutPopupFade?.Kill();
            blastoutPopupFade = blastoutPopupGroup.DOFade(1, .1f);
        }

    }
    void HideAbilityPopup(int i)
    {
        abilityPopupFade?.Kill();
        abilityPopupGroup.DOFade(0, .1f);

        blastoutPopupFade?.Kill();
        blastoutPopupFade = blastoutPopupGroup.DOFade(0, .1f);
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
            tilePreview.sprite = tileDescriptions.tileSprites[(int)mapHit.tile.TileType-1];
            tileName.text = tileDescriptions.tileNames[(int)mapHit.tile.TileType - 1];
            tileDescription.text = tileDescriptions.tileEffects[(int)mapHit.tile.TileType - 1];

            tileInfoFade?.Kill();
            tileInfoGroup.DOFade(1, .05f);
        }
    }

    RectTransform abilityOutline;

    HUDReferencer[] HUDReferences;
    void GetReferences()
    {
        HUDReferences = FindObjectsOfType<HUDReferencer>();

        for (int i = 0; i < 3; i++)
        {
            abilitySprites.Add(null);
        }

        for (int i = 0; i < HUDReferences.Length; i++)
        {
            string tag = HUDReferences[i].hudTag;

            if (tag == "AbilityOutline")
            {
                abilityOutline = HUDReferences[i].GetComponent<RectTransform>();
                continue;
            }
            if (tag == "PopupBlastoutGroup")
            {
                blastoutPopupGroup = HUDReferences[i].GetComponent<CanvasGroup>();
                continue;
            }
            if (tag == "BlastoutDescription")
            {
                blastoutDescriptionText = HUDReferences[i].GetComponent<TextMeshProUGUI>();
                continue;
            }
            if (tag == "HPDisplay")
            {
                hpDisplay = HUDReferences[i].GetComponentInChildren<TextMeshProUGUI>();
                continue;
            }
            if (tag == "PADisplay")
            {
                PADisplay = HUDReferences[i].GetComponent<TextMeshProUGUI>();
                continue;
            }
            if (tag == "XPFill")
            {
                XPFill = HUDReferences[i].GetComponent<Image>();
            }

            if (GetAbilityPopupReferences(tag, HUDReferences[i])) continue;

            if (GetAbilityReferences(tag, HUDReferences[i])) continue;

            if (GetEntityInfoReferences(tag, HUDReferences[i])) continue;

            if (GetGroupReferences(tag, HUDReferences[i])) continue;

            if (tag == "IconPlayer") playerIcon = HUDReferences[i].GetComponent<Image>();
            if (tag == "IconEnemy") enemyIcon = HUDReferences[i].GetComponent<Image>();

            if (GetButtonReferences(tag, HUDReferences[i])) continue;

            if (tag == "TilePreview") tilePreview = HUDReferences[i].GetComponent<Image>();
            if (tag == "TileName") tileName = HUDReferences[i].GetComponent<TextMeshProUGUI>();
            if (tag == "TileEffect") tileDescription = HUDReferences[i].GetComponent<TextMeshProUGUI>();

        }
    }

    List<Image> abilitySprites = new List<Image>();
    List<TextMeshProUGUI> abilityCosts = new List<TextMeshProUGUI>();

    bool GetAbilityReferences(string tag, HUDReferencer reference)
    {
        if (abilityCosts.Count == 0)
        {
            for (int i = 0; i < 3; i++)
            {
                abilityCosts.Add(null);
            }
        }

        if (tag.Contains("Ability"))
        {
            string numberTag = tag.Replace("Ability", "");
            int abilityNumber = -1;
                
            if (Int32.TryParse(numberTag, out abilityNumber))
            {
                Button abilityButton = reference.GetComponent<Button>();

                abilityButton.onClick.AddListener(() =>
                {
                    OnAbilityClicked?.Invoke(abilityNumber);
                });

                EventTrigger trigger = abilityButton.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((x) =>
                {
                    ShowAbilityPopup(abilityNumber);
                });
                trigger.triggers.Add(entry);
                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerExit;
                entry.callback.AddListener((x) =>
                {
                    HideAbilityPopup(abilityNumber);
                });
                trigger.triggers.Add(entry);


                if (abilityNumber <= 2)
                {
                    abilitySprites[abilityNumber] = reference.GetComponentInChildren<Image>();
                    abilityCosts[abilityNumber] = reference.GetComponentInChildren<TextMeshProUGUI>();
                }

                return true;
            }
            return false;
        }

        return false;
    }


    string[,] abilityDescriptions = new string[3, 4];
    List<TextMeshProUGUI> abilityDescriptionsText = new List<TextMeshProUGUI>();
    string[] abilityTitles = new string[3];
    TextMeshProUGUI abilityTitle;
    CanvasGroup abilityPopupGroup;
    Image abilityGlow;
    CanvasGroup blastoutPopupGroup;
    TextMeshProUGUI blastoutDescriptionText;
    string blastoutDescription;

    bool GetAbilityPopupReferences(string tag, HUDReferencer reference)
    {
        if (abilityDescriptionsText.Count == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                abilityDescriptionsText.Add(null);
            }
        }

        if (tag.Contains("PopupAbility"))
        {
            string numberTag = tag.Replace("PopupAbility", "");
            int abilityNumber = -1;

            if (Int32.TryParse(numberTag, out abilityNumber))
            {
                abilityDescriptionsText[abilityNumber] = reference.GetComponent<TextMeshProUGUI>();

                return true;
            }
            else
            {
                if (tag == "PopupAbilityTitle")
                {
                    abilityTitle = reference.GetComponent<TextMeshProUGUI>();
                }
                if (tag == "PopupAbilityGroup")
                {
                    abilityPopupGroup = reference.GetComponent<CanvasGroup>();
                }
                if (tag == "PopupAbilityGlow")
                {
                    abilityGlow = reference.GetComponent<Image>();
                }

                return true;
            }
            return false;
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

        rect.DOAnchorPosX(rect.anchoredPosition.x + 50, .5f);
        rect.DOAnchorPosY(rect.anchoredPosition.y + 1, .5f).SetEase(Ease.OutBack, 100);
        textMesh.DOFade(0, 1f).SetDelay(.5f).OnComplete(()=>
        {
            PoolManager.Recycle(rect.gameObject);
        });
    }

    public void SelectAbility(int abilityNumber)
    {
        abilityOutline.gameObject.SetActive(true);
        abilityOutline.anchoredPosition = abilitySprites[abilityNumber].rectTransform.parent.GetComponent<RectTransform>().anchoredPosition;
    }
    public void DeselectAbility()
    {
        abilityOutline.gameObject.SetActive(false);
    }
}
