using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Vector3 fireflyDestination;
    public static GameManager Instance;
    [SerializeField] private Vector3 spawnPosition;
    public int questIndex;
    [SerializeField] GameObject StartButton;
    private float startTime = 0f;
    public float takenTime;

    public Transform pickupPoint;
    public Transform destinationPoint;

    public bool hasPackage = false;
    public bool taskCompleted = false;
    public GameObject hubParticle, destinationParticle, nacMeshGps;

    public GameObject canvas, menuPanel, healthBar, gameOverPanel;

    public int maxConfidence = 5;

    public event Action<int> OnConfidenceChanged;
    SessionManager sessionManager;
    public int currentConfidence, currentBounty, currentCoin, currentStar;
    void Awake()
    {
        Instance = this;

        // Load from PlayerPrefs
        //currentConfidence = PlayerPrefs.GetInt(AllStringConstant.CONFIDENCE, maxConfidence);
        currentConfidence = maxConfidence;
    }

    void Start()
    {
//        fireflyDestination = pickupPoint.position;
        questIndex = 0;
        startTime = Time.time;

        AudioManager.instance.play("NightCityAmbientBGM");
        sessionManager = SessionManager.Instance;

        //load the confidence bounty from session manager
        currentConfidence = sessionManager.saved_confidence;
        currentBounty = sessionManager.saved_bounty;
        currentCoin = sessionManager.saved_coin;
        currentStar = sessionManager.saved_star;
    }
    public void PlayerReachedPickup()
    {
        if (!hasPackage)
        {
            nacMeshGps.SetActive(true);
            hubParticle.SetActive(false);
            destinationParticle.SetActive(true);

            fireflyDestination = destinationPoint.position;
            hasPackage = true;
            Debug.Log("Package Picked! Now go to Destination.");
        }
    }

    public void PlayerReachedDestination()
    {
        if (hasPackage)
        {
            fireflyDestination = pickupPoint.position;
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

    public void GameOver()
    {
        Time.timeScale = 0;
        canvas.GetComponent<Animator>().Play("gameOverpanelOpen");
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

    public Vector3 getSpawnPosition()
    {
        return spawnPosition;
    }
    public void StartGame()
    {
        startTime = Time.time;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        StartButton.SetActive(false);
    }

    [SerializeField] private GameObject panel;
    public void OnReachingDestination()
    {
        AudioManager.instance.stop("NightCityAmbientBGM");
        AudioManager.instance.play("VictoryFinalSFX");
        panel.SetActive(true);

    }

    public void deactivatePanel()
    {
        panel.SetActive(false);
    }

    public void TakeHit(int amount)
    {
        AudioManager.instance.play("Cat Sad Meow");

        currentConfidence = Mathf.Clamp(currentConfidence - amount, 0, maxConfidence);

        // Fire event
        OnConfidenceChanged?.Invoke(currentConfidence);
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
}
