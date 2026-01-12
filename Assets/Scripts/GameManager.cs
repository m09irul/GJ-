using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public PlayerController player;
    [HideInInspector] public Transform target; // next destination

    public int questIndex;
    private float startTime = 0f;
    public float takenTime;

    public bool hasPackage = false;
    public bool taskCompleted = false;
    public GameObject hubParticle, destinationParticle;

    public int maxConfidence = 5;

    public event Action<int> OnConfidenceChanged;
    SessionManager sessionManager;
    public int currentConfidence, currentBounty, currentCoin, currentFarmableItem;
    public int currentPackageQuality = 3; //1 - 3 : worst, mid, perfect
    public bool isPlayerDetected = false;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        OnConfidenceChanged += UIManager.Instance.UpdateConfidenceUI;

        if (SceneManager.GetActiveScene().buildIndex > 5)
        {
            AudioManager.instance.play("in game  bg", 5f);
            AudioManager.instance.play("game start cutscene", 3f);
            CinemachineController.Instance.PlayCamera(AllStringConstant.HUB_CAMERA, Ease.InCirc, () =>
            {
                // Dialogue starts here
                DialogueManager.instance.StartDialogue(AllStringConstant.HUB_DIALOUGE_NODE_ID, () =>
                {
                    // Called only after dialogue exits
                    CinemachineController.Instance.PlayCamera(AllStringConstant.DEST_CAMERA, Ease.InOutCirc, () =>
                    {
                        DialogueManager.instance.StartDialogue(AllStringConstant.DEST_DIALOUGE_NODE_ID, () =>
                        {
                            CinemachineController.Instance.StopCamera();
                            AudioManager.instance.stop("game start cutscene", 4f);
                        });
                    });
                });
            });
        }

        startTime = Time.time;

        sessionManager = SessionManager.Instance;

        //load the confidence bounty from session manager
        currentConfidence = sessionManager.saved_confidence;
        currentBounty = sessionManager.saved_bounty;
        currentCoin = sessionManager.saved_coin;
        currentFarmableItem = sessionManager.saved_Farming_item;
    }
    public void ThrowItem()
    {
        var inv = InventoryManager.Instance;
        if (inv.SelectedItem == null) return;

        InventoryItemInfo itemInfo = inv.SelectedItem.GetItemInfo();

        GameObject prefab =
            PrefabDatabase.Instance.GetPrefab(itemInfo.itemID);

        if (!prefab) return;

        player.StopThrowPreview();
        player.ThrowItem(prefab);

        inv.RemoveItem(itemInfo.itemID, 1);
        inv.DeselectCurrent();
    }

    public void PlayerReachedPickup()
    {
        hubParticle.SetActive(false);
        destinationParticle.SetActive(true);

        hasPackage = true;
        player.ToggleParcel(true);
        AudioManager.instance.play("CatBellSFX");
    }

    public void PlayerReachedDestination()
    {
        takenTime = Time.time - startTime;
        hasPackage = false;
        taskCompleted = true;
        player.ToggleParcel(false);
        currentCoin += 10;
        OnReachingDestination();

    }

    void GameOver()
    {
        AudioManager.instance.play("GameOverSFX");
        UIManager.Instance.UpdateGameOverUI();

    }

    public void OnRestartPress()
    {
        Time.timeScale = 1;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void OnMainMenuPress()
    {
        Time.timeScale = 1;

        SceneManager.LoadScene(1);
    }

    public void OnReachingDestination()
    {
        player.GetComponent<CharacterController>().enabled = false;

        AudioManager.instance.play("VictoryFinalSFX");

        LevelSaveManager.SaveLevel(SceneManager.GetActiveScene().buildIndex - 1, CalculateStar(), currentPackageQuality);
        UIManager.Instance.UpdateLevelCompletionUI(currentConfidence, currentBounty, CalculateStar(), currentPackageQuality, ConvertTimeToString());
        SessionManager.Instance.SaveTotalData(currentConfidence, currentBounty, currentCoin);
    }
    public void OnGameCompletePress()
    {
        SessionManager.Instance.OnLevelComplete();

    }
    int CalculateStar()
    {
        //now consider only package quality, and time in next build
        return currentPackageQuality;

    }
    string ConvertTimeToString()
    {
        int hours = Mathf.FloorToInt(takenTime / 3600f);
        int minutes = Mathf.FloorToInt((takenTime % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(takenTime % 60f);

        string timeString = $"{hours:00}:{minutes:00}:{seconds:00}";
        return timeString;
    }
    public void TakeHit(int amount)
    {
        AudioManager.instance.play("Cat Sad Meow");

        currentConfidence = Mathf.Clamp(currentConfidence - amount, 0, maxConfidence);

        // Fire event
        OnConfidenceChanged?.Invoke(currentConfidence);

        if(currentConfidence < 3)
        AudioManager.instance.play("PlayerConfLowSFX");
        
        if (currentConfidence <= 0)
            GameOver();

    }

    public void ResetConfidence()
    {
        currentConfidence = maxConfidence;
        OnConfidenceChanged?.Invoke(currentConfidence);
    }
    public void ButtonAudioPlay()
    {
        AudioManager.instance.play(AllStringConstant.BUTTON_CLICK_SFX);
    }
    public void OnSceneComplete()
    {

        UIManager.Instance.hudPanel.SetActive(true);
        player.canMove = true;
    }
    public void OnSceneStart()
    {
        player.movementStick.ResetJoystick();
        UIManager.Instance.hudPanel.SetActive(false);
        player.canMove = false;
    }
}
