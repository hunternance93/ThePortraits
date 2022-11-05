using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Transform PlayerHead = null;
    public Transform PlayerLeftSide = null;
    public Transform PlayerRightSide = null;
    public Transform PlayerChest = null;
    public PlayerCameraSwitcher PCS = null;
    public FirstPersonController FPSController = null;

    public GameObject standingMesh = null;
    public GameObject crouchingMesh = null;

    [Tooltip("If player is below this height then they should be killed (void zone / out of bounds)")]
    public float deathHeight = 0;
    public bool canSightjackStun = false;
    [Tooltip("For final scene")]
    public Flash TrueHeirloomFlash = null;
    [Tooltip("For final scene")]
    public Flash NormalHeirloomFlash = null;
    [HideInInspector] public bool isSightJacking = false;
    [HideInInspector] public bool CanSightJack = true;
    [HideInInspector] public bool isInLight = false;
    public List<string> Inventory = new List<string>();
    public List<JournalEntry> JournalEntries = new List<JournalEntry>();
    [HideInInspector] public int EnemyAggroCount = 0;
    [HideInInspector] public bool HasOverlaySwappedColor = false;

    private const string statueItemName = "missing statue head";
    //Only counts lights setup specifically for this purpose (in cave level)
    private int numberOfLightsOnPlayer = 0;

    private const float _stunCooldown = 25;
    private const float _stunCooldownForTrueHeirloom = 10;
    private float stunCooldownTimer = _stunCooldown;

    private Vector3 originalScale;

    private void Start()
    {
        if (Inventory == null || Inventory.Count == 0) {
            //SaveGameManager.Instance.ReloadDataFromFile(GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore);
            //SaveGameManager.Instance.LoadAllData();
        }
        if (Inventory == null) Inventory = new List<string>();
        else if (InventoryContains(statueItemName)) FPSController.SetEnabledSprinting(false);

        originalScale = transform.localScale;

        if (JournalEntries == null) JournalEntries = new List<JournalEntry>();
    }

    private void Update()
    {
        if (transform.position.y <= deathHeight)
        {
            GameManager.instance.FellOutOfMap();
        }
        if (canSightjackStun && !CanStun(true))
        {
            stunCooldownTimer += Time.deltaTime;
            if (CanStun(true))
            {
                if (!GameManager.instance.GameEnding.IsGameOver) {    
                    if (InventoryContains("True Family Heirloom")) StartCoroutine(TrueHeirloomFlash.FlashScreen(.25f, true));
                    else StartCoroutine(NormalHeirloomFlash.FlashScreen(.25f, true));
                }
            }
        }
        if (isSightJacking)
        {
            SetCrouchedOrStandingModel();
        }
   
    }

    public void SwapOverlayColor()
    {
        if (!HasOverlaySwappedColor)
        {
            PCS.StunAbilityOverlay.GetComponent<Image>().color = TrueHeirloomFlash.targetColor;
            PCS.AccessibleStunAbilityOverlay.GetComponent<Image>().color = TrueHeirloomFlash.targetColor;
            HasOverlaySwappedColor = true;
        }
    }

    public void updateLightCount(int change)
    {
        numberOfLightsOnPlayer += change;
        isInLight = numberOfLightsOnPlayer > 0;
        //Debug.Log("Light Count: " + numberOfLightsOnPlayer);
    }

    public bool InventoryContains(string item)
    {
        return Inventory.Contains(item);
    }

    public bool PlayerHasMoreThanOneCoin()
    {
        int count = 0;
        foreach (string s in Inventory)
        {
            if (s.ToLower().Contains("coin"))
            {
                count++;
                if (count >= 2) return true;
            }
        }
        return false;
    }

    public void AddItem(string item)
    {
        Inventory.Add(item);
        if (item.Equals(statueItemName)) FPSController.SetEnabledSprinting(false);
    }

    public void RemoveAllCoins()
    {
        Inventory.RemoveAll(ContainsCoin);
    }

    private static bool ContainsCoin(string s)
    {
        return s.ToLower().Contains("coin");
    }

    public void RemoveItem(string item)
    {
        Inventory.Remove(item);
        if (item.Equals(statueItemName)) FPSController.SetEnabledSprinting(true);
    }

    public void SetPlayerAngle(float angle)
    {
        FPSController.SetAngle(angle);
    }

    //Check if current sightjack cam list is the exact same list we're trying to change to (ignore order)
    public bool SightJackCamsAre(GameObject[] compareCamObjs)
    {
        foreach (GameObject g in compareCamObjs) {
            bool match = false;
            foreach (GameObject f in PCS.GhostPOVs)
            {
                if (g == f) match = true;
            }
            if (!match) return false;
        }
        return true;
    }

    public void SetSightJackCams(GameObject[] newCams)
    {
        PCS.SetGhostPOVs(newCams);
        PCS.ResetGhostCamIndex();
    }

    public void IndicateNewSightjackCams(GameObject visionCam, float extraBuildUpToVision = 0, bool empoweredHeirloomScene = false)
    {
        if (visionCam)
        {
            PCS.ShowVision(visionCam, extraBuildUpToVision, empoweredHeirloomScene);
        }
    }

    public void SetCrouchedOrStandingModel()
    {
        if (FPSController.IsCrouched())
        {
            crouchingMesh.SetActive(true);
//            crouchingMesh.transform.localScale = new Vector3(1f, 2.5f, 1f);

            if (crouchingMesh.transform.localScale.y < 1.0f)
            {
                crouchingMesh.transform.localScale = Vector3.one;
            }

            standingMesh.SetActive(false);
        }
        else
        {
            standingMesh.SetActive(true);
            crouchingMesh.SetActive(false);
        }

    }

    public void PutStunOnCooldown()
    {
        stunCooldownTimer = 0;
    }

    public bool CanStun(bool dontShowMessage = false)
    {
        if (canSightjackStun)
        {
            if (InventoryContains("True Family Heirloom"))
            {
                if (stunCooldownTimer >= _stunCooldownForTrueHeirloom)
                {
                    return true;
                }
                else if (!dontShowMessage)
                {
                    GameManager.instance.DisplayMessage("The heirloom needs time to recharge to be used again.");
                }
            }
            else
            {
                if (stunCooldownTimer >= _stunCooldown)
                {
                    return true;
                }
                else if (!dontShowMessage)
                {
                    GameManager.instance.DisplayMessage("The heirloom needs time to recharge to be used again.");
                }
            }
        }
        return false;
    }
}
