Quick-reference of the JavaScript hooks your Unity/WebGL game should use.  
When the game is implemented in brainrotgamez.com, it will use these functions.
If you'd like, you may use these function calls in your code to enable functionality with the site.
(They won't work in the itch webgl display, only on brainrotgamez.com)

--------------------------------------------------------------------
GLOBAL FUNCTIONS (window.*) FOR THE GAME TO CALL
--------------------------------------------------------------------

1. parent.window.startGameSession(gameId? = current, showToast? = true) → Promise<{ success, sessionId, gameId }>  
   • Creates (or re-uses) a backend game session, stores `window.currentGameSessionId`, and can trigger a toast.

2. parent.window.reportScore(score, metadata = {}, complete = false) → Promise<{ success, … }>  
   • Sends incremental or final scores to `/api/game-session/score-update`.  
   • Auto-starts a session if needed.  
   • Emits browser events:  
     – `score-submitted` (always)  
     – `questCompleted` / `questProgress` (when quests update)  
   • On `complete === true`, marks the session finished and may force a Unity refresh for specific games.

3. parent.window.setEquippedItem() → Promise<JSONStringified EquippedItem>  
   • Fetches the player’s currently equipped item (`/api/users/equipped/:gameId`) and returns it as a JSON string.

4. parent.window.hookTick() → Promise<{ …instructions… }>  
   • Call this each game “tick” (your desired cadence).  
   • Respond to any returned instructions:  
     – `pauseGame` / `resumeGame`  
     – `adjustVolume` { volume 0-1 }  
     – `equippedItemChanged` { equippedItem }

--------------------------------------------------------------------
RECOMMENDED CALL FLOW
--------------------------------------------------------------------
1. On first frame, call `startGameSession()`; keep the returned `sessionId`.  
2. Every frame (or on demand) call `hookTick()` and act on flags.  
3. When score/progress changes, call `reportScore(score, meta, completeFlag)`.  
4. After equipment changes, call `setEquippedItem()` to sync with the page.
