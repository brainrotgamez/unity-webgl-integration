using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using VeryRareVentures;
using UnityEngine.Networking;
using Newtonsoft.Json;

[System.Serializable]
public class JSONEquippedItem
{
    public string itemGlbUrl;
    public string itemImageUrl;
    public string itemName;
}

[System.Serializable]
public class ExternalSessionData
{
    public string gameId;
    public string gameToken;
}

public class SessionManager : MonoBehaviour
{
    [Header("Session Configuration")]
    [SerializeField] private string gameId = "68229057eb9f31092a6afc8";
    
    
    private string currentSessionId;
    private string gameToken;
    private bool sessionActive = false;
    

    
    // Events
    
    public static event Action<string> OnSessionStarted;
    public static event Action<string> OnSessionEnded;
    public static event Action<int> OnScoreReported;
    public static event Action<string> OnSessionError;
    public static event Action<string> OnSessionRetrieved;


    // Singleton pattern
    public static SessionManager Instance { get; private set; }
    
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void StartGameSession(string gameId);
    
    [DllImport("__Internal")]
    private static extern void ReportScore(string score, string metadata, bool complete);

    [DllImport("__Internal")]
    private static extern void GetSession(string gameId);

    [DllImport("__Internal")]
    private static extern void InitHooks();


    [DllImport("__Internal")]
    private static extern void SetEquippedItem();

#endif
    

    public void SetEquippedItemCall()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        SetEquippedItem();
        #else
        Debug.Log("SetEquippedItem not available in editor");
        #endif
    }

    public void OnEquippedItemChanged(string result)
    {
        Debug.Log("OnSetEquippedItemSuccessCallback: " + result);
        
        // parse the result to get glb image url and name
        // result is a json string

        Debug.Log("result: " + result);
        
        try
        {
            var jsonObject = JsonUtility.FromJson<JSONEquippedItem>(result);
            
            if (jsonObject == null)
            {
                Debug.LogError("JSON deserialization returned null object");
                return;
            }
            
            Debug.Log($"Deserialized JSON - itemGlbUrl: '{jsonObject.itemGlbUrl}', itemImageUrl: '{jsonObject.itemImageUrl}', itemName: '{jsonObject.itemName}'");
            
            var itemGlbUrl = jsonObject.itemGlbUrl;
            var itemImageUrl = jsonObject.itemImageUrl;
            var itemName = jsonObject.itemName;
            
            if (string.IsNullOrEmpty(itemGlbUrl) || string.IsNullOrEmpty(itemName))
            {
                Debug.LogError($"Missing required data - itemGlbUrl: '{itemGlbUrl}', itemName: '{itemName}'");
                return;
            }

            // create a plane data object
            Debug.Log("OnEquippedItemChanged: " + itemGlbUrl);
            Debug.Log("OnEquippedItemChanged: " + itemImageUrl);
            Debug.Log("OnEquippedItemChanged: " + itemName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing equipped item JSON: {e.Message}");
            Debug.LogError($"JSON content: {result}");
        }
    }

    public void OnSetEquippedItemSuccessCallback(string result)
    {
        Debug.Log("OnSetEquippedItemSuccessCallback: " + result);
        
        // parse the result to get glb image url and name
        // result is a json string

        Debug.Log("result: " + result);
        
        try
        {
            var jsonObject = JsonUtility.FromJson<JSONEquippedItem>(result);
            
            if (jsonObject == null)
            {
                Debug.LogError("JSON deserialization returned null object");
                return;
            }
            
            Debug.Log($"Deserialized JSON - itemGlbUrl: '{jsonObject.itemGlbUrl}', itemImageUrl: '{jsonObject.itemImageUrl}', itemName: '{jsonObject.itemName}'");
            
            var itemGlbUrl = jsonObject.itemGlbUrl;
            var itemImageUrl = jsonObject.itemImageUrl;
            var itemName = jsonObject.itemName;
            
            if (string.IsNullOrEmpty(itemGlbUrl) || string.IsNullOrEmpty(itemName))
            {
                Debug.LogError($"Missing required data - itemGlbUrl: '{itemGlbUrl}', itemName: '{itemName}'");
                return;
            }

            // create a plane data object
            Debug.Log("SetEquippedItem success: " + itemGlbUrl);
            Debug.Log("SetEquippedItem success: " + itemImageUrl);
            Debug.Log("SetEquippedItem success: " + itemName);


        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing equipped item JSON: {e.Message}");
            Debug.LogError($"JSON content: {result}");
        }
    }


    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            #if UNITY_WEBGL && !UNITY_EDITOR
            
            InitHooks();
            #endif
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void StartSessionManager()
    {
        // Auto-start session if needed
        if (!sessionActive)
        {
            StartSession();
            
        }
    }
    
    /// <summary>
    /// Starts a new game session
    /// </summary>
    public void StartSession()
    {
        if (sessionActive)
        {
            Debug.LogWarning("Session already active. End current session before starting a new one.");
            return;
        }
        
        Debug.Log($"Starting game session - Game ID: {gameId}");
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            StartGameSession(gameId);
            SetPlayerInventory();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error starting session: {e.Message}");
            OnSessionError?.Invoke($"Failed to start session: {e.Message}");
        }
#else
        // For editor testing, simulate session start
        SimulateSessionStart();
#endif
    }
    
    /// <summary>
    /// Reports a score for the current session
    /// </summary>
    /// <param name="score">The score to report</param>
    /// <param name="metadata">Optional metadata (can be empty)</param>
    /// <param name="complete">Whether this completes the session</param>
    public void CallReportScore(int score, string metadata = "{}", bool complete = false)
    {
        if (!sessionActive)
        {
            Debug.LogWarning("No active session. Start a session before reporting scores.");
            return;
        }
        
        if (string.IsNullOrEmpty(currentSessionId))
        {
            Debug.LogError("Session ID is null or empty. Cannot report score.");
            OnSessionError?.Invoke("Invalid session ID");
            return;
        }
        
        Debug.Log($"Reporting score: {score}, Complete: {complete}");
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            ReportScore(score.ToString(), metadata, complete);
            OnScoreReported?.Invoke(score);
            
            if (complete)
            {
                EndSession();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reporting score: {e.Message}");
            OnSessionError?.Invoke($"Failed to report score: {e.Message}");
        }
#else
        // For editor testing
        SimulateScoreReport(score, complete);
#endif
    }
    
    /// <summary>
    /// Ends the current session
    /// </summary>
    public void EndSession()
    {
        if (!sessionActive)
        {
            Debug.LogWarning("No active session to end.");
            return;
        }
        
        Debug.Log($"Ending session: {currentSessionId}");
        sessionActive = false;
        OnSessionEnded?.Invoke(currentSessionId);
        currentSessionId = null;
        gameToken = null; // Clear the token when session ends
    }

    /// <summary>
    /// Retrieves session information from external system
    /// This calls the window.getSession JavaScript function in WebGL builds
    /// Expected window.getSession signature: async function getSession(gameId) => Promise<{gameId: string, gameToken: string}>
    /// </summary>
    public void RetrieveSession()
    {
        Debug.Log($"Retrieving session for Game ID: {gameId}");
        
#if UNITY_WEBGL && !UNITY_EDITOR
        try
        {
            GetSession(gameId);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error retrieving session: {e.Message}");
            OnSessionError?.Invoke($"Failed to retrieve session: {e.Message}");
        }
#else
        // For editor testing, simulate session retrieval
        SimulateSessionRetrieval();
#endif
    }


    public void OnPauseGame()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = false;
        #endif
        Debug.Log("Game paused");
        GameController gameController = FindObjectOfType<GameController>();
        gameController.PauseGame();
    }

    public void OnResumeGame()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        WebGLInput.captureAllKeyboardInput = true;
        #endif
        GameController gameController = FindObjectOfType<GameController>();
        gameController.ResumeGame();
        Debug.Log("Game resumed");
    } 

    public void OnAdjustVolume(string volume)
    {
        float volumeFloat = float.Parse(volume);
        AudioListener.volume = volumeFloat;
        Debug.Log("Volume adjusted to: " + volume);
    }

    public void OnSendPlayerInventoryData(string inventoryData)
    {
        Debug.Log("Player inventory data sent: " + inventoryData);
    }

    
    /// <summary>
    /// Called from JavaScript when session starts successfully
    /// </summary>
    /// <param name="sessionId">The session ID returned from the API</param>
    public void OnSessionStartedCallback(string sessionId)
    {
        currentSessionId = sessionId;
        sessionActive = true;
        Debug.Log($"Session started successfully with ID: {sessionId}");
        OnSessionStarted?.Invoke(sessionId);
    }
    



    /// <summary>
    /// Called from JavaScript when there's an error
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    public void OnSessionErrorCallback(string errorMessage)
    {
        Debug.LogError($"Session error: {errorMessage}");
        sessionActive = false;
        OnSessionError?.Invoke(errorMessage);
    }



    
    public async System.Threading.Tasks.Task<Texture2D> LoadImageFromURL(string imageUrl)
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    return texture;
                }
                else
                {
                    Debug.LogError($"Failed to load image: {request.error}");
                    return null;
                }
            }
        }




        
    /// <summary>
    /// Parse inventory JSON data into a list of PlaneData objects
    /// </summary>
    


    /// <summary>
    /// Called from JavaScript when session is retrieved successfully
    /// </summary>
    /// <param name="sessionJson">The session data as JSON string</param>
    public void OnSessionRetrievedCallback(string sessionJson)
    {
        try
        {
            // Parse the session data to extract gameToken
            var sessionData = JsonUtility.FromJson<ExternalSessionData>(sessionJson);
            
            if (!string.IsNullOrEmpty(sessionData.gameToken))
            {
                gameToken = sessionData.gameToken;
                Debug.Log($"Game token stored in SessionManager");
            }
            
            if (!string.IsNullOrEmpty(sessionData.gameId))
            {
                gameId = sessionData.gameId;
            }
            
            Debug.Log($"Session retrieved: GameID={sessionData.gameId}, Token stored");
            OnSessionRetrieved?.Invoke(sessionJson);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse session data: {e.Message}");
            OnSessionError?.Invoke($"Failed to parse session data: {e.Message}");
        }
    }
    
    // Editor testing methods
    private void SimulateSessionStart()
    {
        currentSessionId = "test_session_" + System.DateTime.Now.Ticks;
        sessionActive = true;
        Debug.Log($"[EDITOR] Simulated session start with ID: {currentSessionId}");
        OnSessionStarted?.Invoke(currentSessionId);
    }
    
    private void SimulateScoreReport(int score, bool complete)
    {
        Debug.Log($"[EDITOR] Simulated score report: {score}, Complete: {complete}");
        OnScoreReported?.Invoke(score);
        
        if (complete)
        {
            EndSession();
        }
    }

    private void SimulateSessionRetrieval()
    {
        var simulatedToken = "sim_token_" + System.DateTime.Now.Ticks + "_" + UnityEngine.Random.Range(1000, 9999);
        
        var simulatedSession = new ExternalSessionData
        {
            gameId = gameId,
            gameToken = simulatedToken
        };
        
        // Store the simulated token
        gameToken = simulatedToken;
        
        string sessionJson = JsonUtility.ToJson(simulatedSession);
        Debug.Log($"[EDITOR] Simulated session retrieval: {sessionJson}");
        OnSessionRetrieved?.Invoke(sessionJson);
    }
    
    // Public getters
    public bool IsSessionActive => sessionActive;
    public string CurrentSessionId => currentSessionId;
    public string GameId => gameId;

    public string GameToken => gameToken;
    public bool HasValidToken => !string.IsNullOrEmpty(gameToken);
    
    // Public setters for configuration
    public void SetGameId(string newGameId)
    {
        if (!sessionActive)
        {
            gameId = newGameId;
            Debug.Log($"Game ID updated to: {gameId}");
        }
        else
        {
            Debug.LogWarning("Cannot change Game ID while session is active.");
        }
    }
    



} 