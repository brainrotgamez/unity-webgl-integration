mergeInto(LibraryManager.library, {

    StartGameSession: async function (gameIdPtr, leaderboardIdPtr) {
        var gameId = UTF8ToString(gameIdPtr);
        var leaderboardId = UTF8ToString(leaderboardIdPtr);
        
        console.log('Unity called StartGameSession with gameId:', gameId, 'leaderboardId:', leaderboardId);
        
        // Check if the external function exists
        if (typeof window.startGameSession === 'function') {
            try {
                // Call the external function
                const result = await window.startGameSession(gameId);
                console.log('StartGameSession result:', result);
                SendMessage('SessionManager', 'OnSessionStartedCallback', result.sessionId);
            } catch (error) {
                console.error('Error calling window.startGameSession:', error);
                // Send error back to Unity
                SendMessage('SessionManager', 'OnSessionErrorCallback', 'Failed to start session: ' + error.message);
            }
        } else {
            console.warn('window.startGameSession function not found. Make sure it is defined on the webpage.');
            // For testing, you can simulate a successful session start
            var simulatedSessionId = 'sim_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
            console.log('Simulating session start with ID:', simulatedSessionId);
            SendMessage('SessionManager', 'OnSessionStartedCallback', simulatedSessionId);
        }
    },

    ReportScore: async function (scorePtr, metadataPtr, complete) {
        var score = UTF8ToString(scorePtr);
        var metadata = UTF8ToString(metadataPtr);
        
        console.log('Unity called ReportScore with ','score:', score, 'metadata:', metadata, 'complete:', complete);
        
        // Check if the external function exists
        if (typeof window.reportScore === 'function') {
            try {
                // Parse metadata as JSON object
                var metadataObj = {};
                if (metadata && metadata !== '{}') {
                    try {
                        metadataObj = JSON.parse(metadata);
                    } catch (parseError) {
                        console.warn('Failed to parse metadata as JSON:', parseError);
                        metadataObj = {}; // Default to empty object on parse error
                    }
                }
                
                // Call the external async function
                const success = await window.reportScore(score, metadataObj, complete === 1);
                console.log('Score reported successfully via async window.reportScore');
                console.log(success);
                // If you need to send a confirmation back to Unity after successful async call:
                SendMessage('SessionManager', 'OnScoreReportedCallback', 'Score reported successfully');

            } catch (error) {
                console.error('Error calling window.reportScore:', error);
                // Send error back to Unity
                SendMessage('SessionManager', 'OnSessionErrorCallback', 'Failed to report score: ' + error.message);
            }
        } else {
            console.warn('window.reportScore function not found. Make sure it is defined on the webpage.');
            // Simulate or handle the case where the function is not found
            console.log('[SIMULATION] Score would be reported: score=' + score + ', metadata=' + JSON.stringify(metadataObj) + ', complete=' + (complete === 1));
            // If you need to send a simulated success back to Unity:
            // SendMessage('SessionManager', 'OnScoreReportedCallback', 'Score reported (simulated)');
        }
    },


    GetSession: async function (gameIdPtr) {
        var gameId = UTF8ToString(gameIdPtr);
        
        console.log('Unity called GetSession with gameId:', gameId);
        
        // Check if the external function exists
        if (typeof window.getSession === 'function') {
            try {
                // Call the external function - expects { gameId, gameToken }
                const result = await window.getSession(gameId);
                
                // Validate that we have the required fields
                if (!result.gameId || !result.gameToken) {
                    throw new Error('getSession must return an object with gameId and gameToken');
                }
                
                // Convert the session object to JSON string to send back to Unity
                var sessionJson = JSON.stringify({
                    gameId: result.gameId,
                    gameToken: result.gameToken
                });
                SendMessage('SessionManager', 'OnSessionRetrievedCallback', sessionJson);
            } catch (error) {
                console.error('Error calling window.getSession:', error);
                // Send error back to Unity
                SendMessage('SessionManager', 'OnSessionErrorCallback', 'Failed to get session: ' + error.message);
            }
        } else {
            console.warn('window.getSession function not found. Make sure it is defined on the webpage.');
            // For testing, simulate a session response with gameId and gameToken
            var simulatedSession = {
                gameId: gameId,
                gameToken: 'sim_token_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9)
            };
            console.log('Simulating session retrieval:', simulatedSession);
            SendMessage('SessionManager', 'OnSessionRetrievedCallback', JSON.stringify(simulatedSession));
        }
    },


    SetEquippedItem: async function () {

        console.log('Unity called SetEquippedItem');
        if (typeof window.setEquippedItem === 'function') {
            try {
                const result = await window.setEquippedItem();
                console.log('SetEquippedItem result:', result);
                SendMessage('SessionManager', 'OnSetEquippedItemSuccessCallback', result);
            } catch (error) {
                console.error('Error calling window.setEquippedItem:', error);
                SendMessage('SessionManager', 'OnSessionErrorCallback', 'Failed to set equipped item: ' + error.message);
            }
        } else {
            console.warn('window.setEquippedItem function not found. Make sure it is defined on the webpage.');
            SendMessage('SessionManager', 'OnSessionErrorCallback', 'Failed to set equipped item: function not found');
        }
    },

    
    SetPlayerInventory: async function () {

        

        
        if (typeof window.setPlayerInventory === 'function') {
            try {
                
                const returnedInventory = await window.setPlayerInventory();
                
                var inventoryString;
                if (typeof returnedInventory === 'string') {
                    inventoryString = returnedInventory;
                } else if (returnedInventory) {
                    inventoryString = JSON.stringify(returnedInventory);
                } else {
                    inventoryString = '';
                }

                SendMessage('SessionManager', 'OnSetPlayerInventorySuccessCallback', inventoryString);
            } catch (error) {
                console.error('Error during SetPlayerInventory:', error);
                SendMessage('SessionManager', 'OnSetPlayerInventoryErrorCallback', 'Failed to set inventory: ' + error.message);
            }
        } else {
            console.warn('window.setPlayerInventory function not found. For testing, falling back to simulation.');
            console.log('[SIMULATION] SetPlayerInventory with:');
            SendMessage('SessionManager', 'OnSetPlayerInventorySuccessCallback', '{"test": "test"}');
        }
    },

    InitHooks: async function () {
        if (typeof window.hookTick === 'function') { 
            var result;
            while (true){
                await new Promise(resolve => setTimeout(resolve, 100));
            try {
                result = await window.hookTick();
                
                if(result.success){

                }else{
                    console.error('Hook tick function failed');
                }
                if (result.pauseGame){
                    console.log(result);
                    SendMessage('SessionManager', 'OnPauseGame');
                }
                if (result.resumeGame){
                    console.log(result);
                    SendMessage('SessionManager', 'OnResumeGame');
                }
                if (result.adjustVolume){
                    console.log(result);
                    SendMessage('SessionManager', 'OnAdjustVolume', result.volume.toString());
                }
                if (result.sendPlayerInventoryData){
                    console.log(result);
                    SendMessage('SessionManager', 'OnSendPlayerInventoryData', result.inventoryData);
                }

                if (result.equippedItemChanged){
                    console.log(result);
                    SendMessage('SessionManager', 'OnEquippedItemChanged', result.equippedItem);
                }
            } catch (error) {
                    console.error('Error calling window.hookTick:', error);
                }
            }
        } else {
            console.warn('window.hookTick function not found. Make sure it is defined on the webpage.');
        }
    }



}); 