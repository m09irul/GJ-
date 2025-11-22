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

    public GameObject canvas, menuPanel, healthBar, gameOverPanel;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        fireflyDestination = pickupPoint.position;
        questIndex = 0;
        startTime = Time.time;
    }
    public void PlayerReachedPickup()
    {
        if (!hasPackage)
        {
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
        panel.SetActive(true);

    }

    public void deactivatePanel()
    {
        panel.SetActive(false);
    }
}
