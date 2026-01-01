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
    [SerializeField] GameObject StartButton;
    private float startTime = 0f;
    public float takenTime;

    public Transform pickupPoint;
    public Transform destinationPoint;

    public bool hasPackage = false;
    public bool taskCompleted = false;
    public GameObject hubParticle, destinationParticle;

    public int maxConfidence = 5;

    public event Action<int> OnConfidenceChanged;
    SessionManager sessionManager;
    public int currentConfidence, currentBounty, currentCoin, currentStar, currentFarmableItem;
    public GuidingFlutterBlySpawner guidingFlutterBlySpawner;
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
        target = pickupPoint.transform;
        OnConfidenceChanged += UIManager.Instance.UpdateConfidenceUI;

        // CinemachineController.Instance.PlayCamera(AllStringConstant.HUB_CAMERA, Ease.InCirc, () =>
        // {
        //     // Dialogue starts here
        //     DialogueManager.instance.StartDialogue(AllStringConstant.HUB_DIALOUGE_NODE_ID, () =>
        //     {
        //         // Called only after dialogue exits
        //         CinemachineController.Instance.PlayCamera(AllStringConstant.DEST_CAMERA, Ease.InOutCirc, () =>
        //         {
        //             DialogueManager.instance.StartDialogue(AllStringConstant.DEST_DIALOUGE_NODE_ID, () =>
        //             {
        //                 CinemachineController.Instance.StopCamera(()=>
        //                 {
        //                     DialogueManager.instance.StartDialogue(AllStringConstant.PAN_DIALOUGE_NODE_ID, () =>
        //                     {
        //                         //StartCoroutine(GuidePlayer());
        //                     });
        //                 });
        //             });
        //         });
        //     });
        // });

        questIndex = 0;
        startTime = Time.time;

        sessionManager = SessionManager.Instance;

        //load the confidence bounty from session manager
        currentConfidence = sessionManager.saved_confidence;
        currentBounty = sessionManager.saved_bounty;
        currentCoin = sessionManager.saved_coin;
        currentStar = sessionManager.saved_star;
        currentFarmableItem = sessionManager.saved_Farming_item;
    }
    public IEnumerator GuidePlayer()
    {
        yield return new WaitForSeconds(3);
        Debug.Log("working");

        guidingFlutterBlySpawner.Spawn();

        yield return new WaitForSeconds(2);

        DialogueManager.instance.StartDialogue(AllStringConstant.FUTTER_BLY_DIALOUGE_NODE_ID);
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
        if (!hasPackage)
        {
            hubParticle.SetActive(false);
            destinationParticle.SetActive(true);

            hasPackage = true;
            Debug.Log("Package Picked! Now go to Destination.");
        }
    }

    public void PlayerReachedDestination()
    {
        if (hasPackage)
        {
            takenTime = Time.time - startTime;
            Debug.Log("Time Taken: " + takenTime + " seconds.");
            hasPackage = false;
            taskCompleted = true;
            Debug.Log("Delivery Completed!");
            OnReachingDestination();
        }
        else
        {
            Debug.Log("You don't have any package.");
        }
    }

    void GameOver()
    {
        Time.timeScale = 0;
        AudioManager.instance.stop("NightCityAmbientBGM");
        AudioManager.instance.play("GameOverSFX");

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

    public void StartGame()
    {
        startTime = Time.time;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        StartButton.SetActive(false);
    }

    public void OnReachingDestination()
    {
        AudioManager.instance.stop("NightCityAmbientBGM");
        AudioManager.instance.play("VictoryFinalSFX");

    }
    public void OnGameComplete()
    {
        LevelSaveManager.SaveLevel(1, 3, 0);
        LevelLoader.instance.loadLevelWithIndex(1);

    }

    public void TakeHit(int amount)
    {
        AudioManager.instance.play("Cat Sad Meow");

        currentConfidence = Mathf.Clamp(currentConfidence - amount, 0, maxConfidence);

        // Fire event
        OnConfidenceChanged?.Invoke(currentConfidence);

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
