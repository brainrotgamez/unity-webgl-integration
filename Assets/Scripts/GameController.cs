using UnityEngine;
using UnityEngine.SceneManagement;
using VeryRareVentures;

public class GameController : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject pausePanel;
    public GameObject finishPanel;


    private bool isPaused = false; 
    public SessionManager sessionManager;

    void Start()
    {
        Time.timeScale = 1f; // Ensure the game starts unpaused
        startPanel.SetActive(true); // Show the start panel
        pausePanel.SetActive(false); // Hide the pause panel
        finishPanel.SetActive(false); // Hide the finish panel

        
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(!isPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    public void StartGame()
    {
        
        //GameObject plane = Instantiate(planePrefab, transform.position, transform.rotation);
        //GameObject.FindObjectOfType<PlaneSelector>().OnPlaneSpawned(plane.GetComponent<PlaneController>());


        Time.timeScale = 1f; // Resume the game time
        sessionManager.StartSessionManager();
        sessionManager.SetEquippedItemCall();
        startPanel.SetActive(false); // Hide the start panel

    }
    public void PauseGame()
    {
        Time.timeScale = 0f; // Pause the game time
        isPaused = true; // Set the pause state
        pausePanel.SetActive(true); // Show the pause panel

    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // Resume the game time
        isPaused = false; // Reset the pause state
        pausePanel.SetActive(false); // Hide the pause panel

    }
    public void FinishGame()
    {
        sessionManager.EndSession();
    }
    public void RestartGame()
    {
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnSelectButtonClicked()
    {
        SceneManager.LoadScene("StartScene");
    }

    public void OnBackButtonClicked()
    {
        SceneManager.LoadScene("StartScene");
    }
    public void ExitGame()
    {
        SceneManager.LoadScene("StartScene");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
        #endif
    }
}
