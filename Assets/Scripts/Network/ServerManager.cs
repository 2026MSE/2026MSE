using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviour
{
    public static ServerManager instance { get; private set; }

    [Header("Server Settings")]
    public bool isUsingServer = false;
    public string serverUrl = "http://localhost:8080";
    public float pollInterval = 1.0f;
    public bool is_debugging = false;

    [Header("Debug Object")]
    public GameObject test_plane;

    private CancellationTokenSource game_state_polling_cts;

    private PlayerManager playerManager;
    private MainGameManager mainGameManager;

    public enum RoomActionType
    {
        Create,
        Join,
        Start,
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ServerManager] Awake - instance created.");
        }
        else
        {
            Debug.LogWarning("[ServerManager] Duplicate instance destroyed.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ResolveManagers();
        Debug.Log("[ServerManager] Start - managers resolved.");
    }

    private void OnDestroy()
    {
        StopGameStatePolling();
    }

    private void ResolveManagers()
    {
        if (playerManager == null)
            playerManager = PlayerManager.instance;

        if (mainGameManager == null)
            mainGameManager = MainGameManager.instance;
    }
    private void MoveToMainHallAfterGameStart()
    {
        if (MainGameManager.instance == null)
        {
            Debug.LogWarning("[MoveToMainHallAfterGameStart] MainGameManager.instance is null.");
            return;
        }

        // 클라이언트 화면 상태 설정
        MainGameManager.instance.currentClientScene = ClientScene.IN_GAME;
        MainGameManager.instance.currentGameViewScene = GameViewScene.MAIN_HALL;

        // LoadingSceneManager가 읽을 값 설정
        MainGameManager.instance.nextSceneName = "MainHall";
        MainGameManager.instance.nextSceneLoadMode = ClientSceneLoadMode.Single;

        Debug.Log("[MoveToMainHallAfterGameStart] Move to LoadingScene -> MainHall");

        // RoomCreate가 Additive로 올라와 있었다면 Single 로딩으로 정리됨
        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);
    }

    // =========================================================
    // Polling Control
    // =========================================================

    public void GameStart()
    {
        Debug.Log("[ServerManager] GameStart called.");
        StartGameStatePolling();
    }

    public void GameStop()
    {
        Debug.Log("[ServerManager] GameStop called.");
        StopGameStatePolling();
    }

    public void RoomStart()
    {
        Debug.Log("[ServerManager] RoomStart called.");
        StartGameStatePolling();
    }

    public void RoomStop()
    {
        Debug.Log("[ServerManager] RoomStop called.");
        StopGameStatePolling();
    }

    public void StartGameStatePolling()
    {
        ResolveManagers();

        if (!CanPollGameState())
        {
            Debug.LogWarning("[ServerManager] Cannot start game state polling yet.");
            return;
        }

        if (game_state_polling_cts != null)
        {
            Debug.Log("[ServerManager] Game state polling is already running.");
            return;
        }

        game_state_polling_cts = new CancellationTokenSource();
        PollRoomStateServer(game_state_polling_cts.Token).Forget();

        Debug.Log("[ServerManager] Game state polling started.");
    }

    public void StopGameStatePolling()
    {
        if (game_state_polling_cts != null)
        {
            Debug.Log("[ServerManager] Game state polling stopped.");

            game_state_polling_cts.Cancel();
            game_state_polling_cts.Dispose();
            game_state_polling_cts = null;
        }
    }

    private async UniTaskVoid PollRoomStateServer(CancellationToken token)
    {
        Debug.Log("[ServerManager] PollRoomStateServer loop entered.");

        while (!token.IsCancellationRequested)
        {
            try
            {
                ResolveManagers();

                if (!CanPollGameState())
                {
                    if (is_debugging)
                    {
                        Debug.Log("[PollRoomStateServer] Cannot poll yet. Waiting for playerId and roomId...");
                    }

                    await UniTask.Delay((int)(pollInterval * 1000), cancellationToken: token);
                    continue;
                }

                string roomId = playerManager.currentRoom.roomId;
                string playerId = playerManager.this_player.id;

                string url =
                    serverUrl + "/game/state?roomId=" + UnityWebRequest.EscapeURL(roomId)
                    + "&playerId=" + UnityWebRequest.EscapeURL(playerId);

                Debug.Log("[Polling URI] " + url);

                string json = await GetPollingRequest(url, token);

                if (!string.IsNullOrEmpty(json))
                {
                    Debug.Log("[PollRoomStateServer Response] " + json);

                    ApiResponse<GameStateResponse> response =
                        JsonConvert.DeserializeObject<ApiResponse<GameStateResponse>>(json);

                    if (response != null && response.success && response.data != null)
                    {
                        if (MainGameManager.instance != null)
                        {
                            MainGameManager.instance.ApplyGameState(response.data);
                        }
                        else
                        {
                            Debug.LogWarning("[PollRoomStateServer] MainGameManager.instance is null.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[PollRoomStateServer Failed] " + response?.message);
                        Debug.LogWarning("[PollRoomStateServer ErrorCode] " + response?.errorCode);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[PollRoomStateServer] Polling canceled.");
                return;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[PollRoomStateServer Exception] " + e.Message);
                Debug.LogException(e);
            }

            try
            {
                await UniTask.Delay((int)(pollInterval * 1000), cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[PollRoomStateServer] Delay canceled.");
                return;
            }
        }

        Debug.Log("[ServerManager] PollRoomStateServer loop exited.");
    }

    private bool CanPollGameState()
    {
        ResolveManagers();

        if (playerManager == null)
        {
            if (is_debugging) Debug.LogWarning("[CanPollGameState] playerManager is null.");
            return false;
        }

        if (playerManager.this_player == null)
        {
            if (is_debugging) Debug.LogWarning("[CanPollGameState] this_player is null.");
            return false;
        }

        if (string.IsNullOrEmpty(playerManager.this_player.id))
        {
            if (is_debugging) Debug.LogWarning("[CanPollGameState] this_player.id is null or empty.");
            return false;
        }

        if (playerManager.currentRoom == null)
        {
            if (is_debugging) Debug.LogWarning("[CanPollGameState] currentRoom is null.");
            return false;
        }

        if (string.IsNullOrEmpty(playerManager.currentRoom.roomId))
        {
            if (is_debugging) Debug.LogWarning("[CanPollGameState] currentRoom.roomId is null or empty.");
            return false;
        }

        if (MainGameManager.instance == null)
        {
            if (is_debugging) Debug.LogWarning("[CanPollGameState] MainGameManager.instance is null.");
            return false;
        }

        return true;
    }

    // =========================================================
    // Room
    // =========================================================

    public async UniTaskVoid RoomRequest(GameActionRequest request, RoomActionType actionType)
    {
        ResolveManagers();

        if (request == null)
        {
            Debug.LogWarning("[RoomRequest] request is null.");
            return;
        }

        if (playerManager == null)
        {
            Debug.LogWarning("[RoomRequest] playerManager is null.");
            return;
        }

        if (playerManager.this_player == null)
        {
            Debug.LogWarning("[RoomRequest] this_player is null. Create player first.");
            return;
        }

        if (string.IsNullOrEmpty(request.playerId))
        {
            request.playerId = playerManager.this_player.id;
        }

        string endpoint = actionType switch
        {
            RoomActionType.Create => "/room/create",
            RoomActionType.Join => "/room/join",
            RoomActionType.Start => "/room/start",
            _ => throw new ArgumentException("Invalid RoomActionType")
        };

        string json = JsonConvert.SerializeObject(request);

        Debug.Log("[RoomRequest]");
        Debug.Log("Endpoint: " + endpoint);
        Debug.Log("Request JSON: " + json);

        string result = await SendJsonToServer(serverUrl + endpoint, json);

        if (string.IsNullOrEmpty(result))
        {
            Debug.LogWarning("[RoomRequest] Server response is empty.");
            return;
        }

        Debug.Log("[RoomRequest Response] " + result);

        ApiResponse<RoomInfo> response = null;

        try
        {
            response = JsonConvert.DeserializeObject<ApiResponse<RoomInfo>>(result);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[RoomRequest] Failed to parse response.");
            Debug.LogException(e);
            return;
        }

        if (response == null)
        {
            Debug.LogWarning("[RoomRequest] response is null.");
            return;
        }

        if (!response.success || response.data == null)
        {
            Debug.LogWarning("[RoomRequest Failed] " + response.message);
            Debug.LogWarning("[RoomRequest ErrorCode] " + response.errorCode);
            return;
        }

        Debug.Log($"[RoomRequest Success] {response.message} RoomID: {response.data.roomId}");

        playerManager.currentRoom = response.data;

        switch (actionType)
        {
            case RoomActionType.Create:
                Debug.Log("[RoomRequest] Room created. Do not move scene yet.");
                // 방 생성 후에는 RoomCreate 로비에 그대로 있음
                break;

            case RoomActionType.Join:
                Debug.Log("[RoomRequest] Room joined. Do not move scene yet.");
                // 방 입장 후에도 로비에 그대로 있음
                break;

            case RoomActionType.Start:
                Debug.Log("[RoomRequest] Room started. Start polling and move to MainHall.");

                StartGameStatePolling();
                MoveToMainHallAfterGameStart();

                break;
        }
    }

    // =========================================================
    // Player / Avatar
    // =========================================================

    public async UniTaskVoid PlayerRequest(string name, string style)
    {
        ResolveManagers();

        if (playerManager == null)
        {
            Debug.LogWarning("[PlayerRequest] playerManager is null.");
            return;
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.LogWarning("[PlayerRequest] name is empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(style))
        {
            style = "adventurer";
        }

        string url =
            serverUrl + "/api/avatar/player?name=" + UnityWebRequest.EscapeURL(name.Trim())
            + "&style=" + UnityWebRequest.EscapeURL(style.Trim());

        Debug.Log("[PlayerRequest URL] " + url);

        string result = await FetchDataFromServer(url);

        if (string.IsNullOrEmpty(result))
        {
            Debug.LogWarning("[PlayerRequest] Server response is empty.");
            return;
        }

        Debug.Log("[PlayerRequest Response] " + result);

        ApiResponse<Player> response = null;

        try
        {
            response = JsonConvert.DeserializeObject<ApiResponse<Player>>(result);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[PlayerRequest] Failed to parse response.");
            Debug.LogException(e);
            return;
        }

        if (response == null || !response.success || response.data == null)
        {
            Debug.LogWarning("[PlayerRequest Failed] " + response?.message);
            Debug.LogWarning("[PlayerRequest ErrorCode] " + response?.errorCode);
            return;
        }

        Player player = response.data;

        Debug.Log($"[PlayerRequest Success] {response.message}: {player.name} / ID: {player.id}");

        if (playerManager.this_player == null)
        {
            playerManager.this_player = player;
            Debug.Log("[PlayerRequest] this_player assigned.");
        }
        else
        {
            playerManager.debug_players.Add(player);
            Debug.Log("[PlayerRequest] debug player added.");
        }
    }

    public async UniTaskVoid TextureRequest()
    {
        Debug.Log("[TextureRequest] Start.");

        Texture2D result = await FetchTextureFromServer("https://api.dicebear.com/9.x/bottts/png");

        if (result != null)
        {
            Debug.Log("[TextureRequest] Texture received.");

            if (test_plane != null)
            {
                // test_plane.GetComponent<Renderer>().material.mainTexture = result;
            }
        }
        else
        {
            Debug.LogWarning("[TextureRequest] Texture result is null.");
        }
    }

    // =========================================================
    // Turn / Yut
    // =========================================================

    public async UniTask ThrowYutRequest()
    {
        ResolveManagers();

        if (!CanSendGameAction("ThrowYutRequest"))
            return;

        var req = new GameActionRequest
        {
            roomId = playerManager.currentRoom.roomId,
            playerId = playerManager.this_player.id
        };

        string json = JsonConvert.SerializeObject(req);

        Debug.Log("[ThrowYutRequest]");
        Debug.Log("Request JSON: " + json);

        string result = await SendJsonToServer(serverUrl + "/turn/throw", json);

        if (string.IsNullOrEmpty(result))
        {
            Debug.LogWarning("[ThrowYutRequest] Server response is empty.");
            return;
        }

        Debug.Log("[ThrowYutRequest Response] " + result);

        ApiResponse<ThrowResponse> response = null;

        try
        {
            response = JsonConvert.DeserializeObject<ApiResponse<ThrowResponse>>(result);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[ThrowYutRequest] Failed to parse response.");
            Debug.LogException(e);
            return;
        }

        if (response != null && response.success)
        {
            if (mainGameManager != null)
            {
                mainGameManager.throwResponse = response.data;
            }

            Debug.Log("[Throw Success] " + response.message);
        }
        else
        {
            Debug.LogWarning("[Throw Failed] " + response?.message);
            Debug.LogWarning("[Throw ErrorCode] " + response?.errorCode);
        }
    }

    public async UniTask EndTurnRequest()
    {
        ResolveManagers();

        if (!CanSendGameAction("EndTurnRequest"))
            return;

        var req = new GameActionRequest
        {
            roomId = playerManager.currentRoom.roomId,
            playerId = playerManager.this_player.id
        };

        string json = JsonConvert.SerializeObject(req);

        Debug.Log("[EndTurnRequest]");
        Debug.Log("Request JSON: " + json);

        string result = await SendJsonToServer(serverUrl + "/turn/end", json);

        if (string.IsNullOrEmpty(result))
        {
            Debug.LogWarning("[EndTurnRequest] Server response is empty.");
            return;
        }

        Debug.Log("[EndTurnRequest Response] " + result);

        ApiResponse<object> response = null;

        try
        {
            response = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[EndTurnRequest] Failed to parse response.");
            Debug.LogException(e);
            return;
        }

        if (response != null && response.success)
        {
            Debug.Log("[EndTurn Success] " + response.message);
        }
        else
        {
            Debug.LogWarning("[EndTurn Failed] " + response?.message);
            Debug.LogWarning("[EndTurn ErrorCode] " + response?.errorCode);
        }
    }

    // =========================================================
    // Board / Move
    // =========================================================

    public async UniTask MoveListRequest()
    {
        ResolveManagers();

        if (!CanSendGameAction("MoveListRequest"))
            return;

        string url =
            serverUrl + "/board/moveList?roomId=" + UnityWebRequest.EscapeURL(playerManager.currentRoom.roomId)
            + "&playerId=" + UnityWebRequest.EscapeURL(playerManager.this_player.id);

        Debug.Log("[MoveListRequest URL] " + url);

        string result = await FetchDataFromServer(url);

        if (string.IsNullOrEmpty(result))
        {
            Debug.LogWarning("[MoveListRequest] Server response is empty.");
            return;
        }

        Debug.Log("[MoveListRequest Response] " + result);

        ApiResponse<MoveListResponse> response = null;

        try
        {
            response = JsonConvert.DeserializeObject<ApiResponse<MoveListResponse>>(result);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[MoveListRequest] Failed to parse response.");
            Debug.LogException(e);
            return;
        }

        if (response != null && response.success)
        {
            if (mainGameManager != null)
            {
                mainGameManager.moveListResponse = response.data;
            }

            Debug.Log("[MoveList Success] " + response.message);
        }
        else
        {
            Debug.LogWarning("[MoveList Failed] " + response?.message);
            Debug.LogWarning("[MoveList ErrorCode] " + response?.errorCode);
        }
    }

    public async UniTask MovePieceRequest(string pieceId, int yutResultIndex)
    {
        ResolveManagers();

        if (!CanSendGameAction("MovePieceRequest"))
            return;

        if (string.IsNullOrEmpty(pieceId))
        {
            Debug.LogWarning("[MovePieceRequest] pieceId is null or empty.");
            return;
        }

        var req = new MoveRequest
        {
            roomId = playerManager.currentRoom.roomId,
            playerId = playerManager.this_player.id,
            pieceId = pieceId,
            yutResultIndex = yutResultIndex
        };

        string json = JsonConvert.SerializeObject(req);

        Debug.Log("[MovePieceRequest]");
        Debug.Log("Request JSON: " + json);

        string result = await SendJsonToServer(serverUrl + "/board/move", json);

        if (string.IsNullOrEmpty(result))
        {
            Debug.LogWarning("[MovePieceRequest] Server response is empty.");
            return;
        }

        Debug.Log("[MovePieceRequest Response] " + result);

        ApiResponse<MoveResultResponse> response = null;

        try
        {
            response = JsonConvert.DeserializeObject<ApiResponse<MoveResultResponse>>(result);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[MovePieceRequest] Failed to parse response.");
            Debug.LogException(e);
            return;
        }

        if (response != null && response.success)
        {
            Debug.Log("[Move Success] " + response.message);
        }
        else
        {
            Debug.LogWarning("[Move Failed] " + response?.message);
            Debug.LogWarning("[Move ErrorCode] " + response?.errorCode);
        }
    }

    // =========================================================
    // Hall / Declare / Challenge
    // =========================================================

    public async UniTask DeclareRequest(StickSide[] declareSticks)
    {
        ResolveManagers();

        if (!CanSendGameAction("DeclareRequest"))
            return;

        if (declareSticks == null || declareSticks.Length < 2)
        {
            Debug.LogWarning("[DeclareRequest] declareSticks is invalid.");
            return;
        }

        var req = new DeclareRequest
        {
            roomId = playerManager.currentRoom.roomId,
            playerId = playerManager.this_player.id,
            s1 = declareSticks[0],
            s2 = declareSticks[1]
        };

        string json = JsonConvert.SerializeObject(req);

        Debug.Log("[DeclareRequest]");
        Debug.Log("Request JSON: " + json);

        string result = await SendJsonToServer(serverUrl + "/hall/declare", json);

        if (string.IsNullOrEmpty(result))
        {
            Debug.LogWarning("[DeclareRequest] Server response is empty.");
            return;
        }

        Debug.Log("[DeclareRequest Response] " + result);

        ApiResponse<object> response = null;

        try
        {
            response = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[DeclareRequest] Failed to parse response.");
            Debug.LogException(e);
            return;
        }

        if (response != null && response.success)
        {
            Debug.Log("[Declare Success] " + response.message);
        }
        else
        {
            Debug.LogWarning("[Declare Failed] " + response?.message);
            Debug.LogWarning("[Declare ErrorCode] " + response?.errorCode);
        }
    }

    public async UniTask ChallengeVoteRequest(bool challenge)
    {
        ResolveManagers();

        if (!CanSendGameAction("ChallengeVoteRequest"))
            return;

        var req = new ChallengeVoteRequest
        {
            roomId = playerManager.currentRoom.roomId,
            playerId = playerManager.this_player.id,
            challenge = challenge
        };

        string json = JsonConvert.SerializeObject(req);

        Debug.Log("[ChallengeVoteRequest]");
        Debug.Log("Request JSON: " + json);

        string result = await SendJsonToServer(serverUrl + "/hall/challenge", json);

        if (string.IsNullOrEmpty(result))
        {
            Debug.LogWarning("[ChallengeVoteRequest] Server response is empty.");
            return;
        }

        Debug.Log("[ChallengeVoteRequest Response] " + result);

        ApiResponse<object> response = null;

        try
        {
            response = JsonConvert.DeserializeObject<ApiResponse<object>>(result);
        }
        catch (Exception e)
        {
            Debug.LogWarning("[ChallengeVoteRequest] Failed to parse response.");
            Debug.LogException(e);
            return;
        }

        if (response != null && response.success)
        {
            Debug.Log("[Challenge Vote Success] " + response.message);
        }
        else
        {
            Debug.LogWarning("[Challenge Vote Failed] " + response?.message);
            Debug.LogWarning("[Challenge Vote ErrorCode] " + response?.errorCode);
        }
    }

    // =========================================================
    // Deprecated Old APIs
    // =========================================================
    // 구버전 API 호출이 남아 있는 다른 스크립트가 있을 수 있어서,
    // 완전히 삭제하지 않고 경고 로그를 남기고 신버전 함수로 우회함.

    public async UniTaskVoid YutRequest()
    {
        Debug.LogWarning("[YutRequest] Deprecated. Use ThrowYutRequest() instead.");
        await ThrowYutRequest();
    }

    public async UniTask BoardStateRequest()
    {
        Debug.LogWarning("[BoardStateRequest] Deprecated. Board state comes from /game/state polling.");
        await UniTask.CompletedTask;
    }

    public async UniTaskVoid PrivateExitRequest()
    {
        Debug.LogWarning("[PrivateExitRequest] Deprecated. Private exit is controlled by TurnPhase on server.");
        await UniTask.CompletedTask;
    }

    // =========================================================
    // Validation Helper
    // =========================================================

    private bool CanSendGameAction(string caller)
    {
        ResolveManagers();

        if (playerManager == null)
        {
            Debug.LogWarning($"[{caller}] playerManager is null.");
            return false;
        }

        if (playerManager.this_player == null)
        {
            Debug.LogWarning($"[{caller}] this_player is null.");
            return false;
        }

        if (string.IsNullOrEmpty(playerManager.this_player.id))
        {
            Debug.LogWarning($"[{caller}] this_player.id is null or empty.");
            return false;
        }

        if (playerManager.currentRoom == null)
        {
            Debug.LogWarning($"[{caller}] currentRoom is null.");
            return false;
        }

        if (string.IsNullOrEmpty(playerManager.currentRoom.roomId))
        {
            Debug.LogWarning($"[{caller}] currentRoom.roomId is null or empty.");
            return false;
        }

        return true;
    }

    // =========================================================
    // Low Level HTTP Functions
    // =========================================================

    private async UniTask<string> FetchDataFromServer(string target_url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(target_url))
        {
            request.SetRequestHeader("Accept", "application/json");

            Debug.Log("[FetchDataFromServer] GET " + target_url);

            try
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[FetchDataFromServer Exception] " + e.Message);
                Debug.LogWarning("[FetchDataFromServer Exception Detail] " + e);
                Debug.LogWarning("[FetchDataFromServer URL] " + target_url);
                return null;
            }

            string body = request.downloadHandler != null
                ? request.downloadHandler.text
                : "";

            Debug.Log("[FetchDataFromServer Code] " + request.responseCode);
            Debug.Log("[FetchDataFromServer Body] " + body);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[FetchDataFromServer Failed]");
                Debug.LogWarning("URL: " + target_url);
                Debug.LogWarning("Code: " + request.responseCode);
                Debug.LogWarning("Error: " + request.error);
                Debug.LogWarning("Body: " + body);
                return null;
            }

            return body;
        }
    }

    public async UniTask<string> SendJsonToServer(string url, string json)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            Debug.Log("[SendJsonToServer] POST " + url);
            Debug.Log("[SendJsonToServer JSON] " + json);

            try
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[SendJsonToServer Exception] " + e.Message);
                Debug.LogWarning("[SendJsonToServer Exception Detail] " + e);
                Debug.LogWarning("[SendJsonToServer URL] " + url);
                Debug.LogWarning("[SendJsonToServer JSON] " + json);
                return null;
            }

            string body = request.downloadHandler != null
                ? request.downloadHandler.text
                : "";

            Debug.Log("[SendJsonToServer Code] " + request.responseCode);
            Debug.Log("[SendJsonToServer Body] " + body);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[SendJsonToServer Failed]");
                Debug.LogWarning("URL: " + url);
                Debug.LogWarning("Code: " + request.responseCode);
                Debug.LogWarning("Error: " + request.error);
                Debug.LogWarning("Body: " + body);
                Debug.LogWarning("Request JSON: " + json);

                // 서버가 ApiResponse 형태로 실패 사유를 보내는 경우가 있으므로 body는 반환
                return body;
            }

            return body;
        }
    }

    private async UniTask<Texture2D> FetchTextureFromServer(string target_url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(target_url))
        {
            request.SetRequestHeader("Accept", "image/*");

            Debug.Log("[FetchTextureFromServer] GET " + target_url);

            try
            {
                await request.SendWebRequest();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[FetchTextureFromServer Exception] " + e.Message);
                Debug.LogWarning("[FetchTextureFromServer URL] " + target_url);
                Debug.LogException(e);
                return null;
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[FetchTextureFromServer Success]");
                return DownloadHandlerTexture.GetContent(request);
            }

            Debug.LogWarning("[FetchTextureFromServer Failed]");
            Debug.LogWarning("URL: " + target_url);
            Debug.LogWarning("Code: " + request.responseCode);
            Debug.LogWarning("Error: " + request.error);

            return null;
        }
    }

    private async UniTask<string> GetPollingRequest(string uri, CancellationToken token)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(uri))
        {
            request.SetRequestHeader("Accept", "application/json");

            Debug.Log("[GetPollingRequest] GET " + uri);

            try
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    if (token.IsCancellationRequested)
                    {
                        request.Abort();
                        throw new OperationCanceledException();
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[GetPollingRequest] Canceled.");
                throw;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[GetPollingRequest Exception] " + e.Message);
                Debug.LogWarning("[GetPollingRequest Exception Detail] " + e);
                Debug.LogWarning("[GetPollingRequest URI] " + uri);
                return null;
            }

            string body = request.downloadHandler != null
                ? request.downloadHandler.text
                : "";

            Debug.Log("[GetPollingRequest Code] " + request.responseCode);
            Debug.Log("[GetPollingRequest Body] " + body);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[GetPollingRequest Failed]");
                Debug.LogWarning("URI: " + uri);
                Debug.LogWarning("Code: " + request.responseCode);
                Debug.LogWarning("Error: " + request.error);
                Debug.LogWarning("Body: " + body);

                return null;
            }

            return body;
        }
    }
}