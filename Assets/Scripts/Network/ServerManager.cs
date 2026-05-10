using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class ServerManager : MonoBehaviour
{
    public static ServerManager instance { get; private set; }

    public bool isUsingServer = false;
    public string serverUrl = "http://localhost:8080";
    public float pollInterval = 1.0f;
    public bool is_debugging = false;
    private CancellationTokenSource polling_cts;
    private CancellationTokenSource room_polling_cts;

    public GameObject test_plane;
    private PlayerManager playerManager;
    private MainGameManager mainGameManager;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        playerManager = PlayerManager.instance;
        mainGameManager = MainGameManager.instance;
    }

    public void GameStart()
    {
        polling_cts = new CancellationTokenSource();
        PollServer(polling_cts.Token).Forget();
    }

    public void GameStop()
    {
        if (polling_cts != null)
        {
            polling_cts.Cancel();
            polling_cts.Dispose();
            polling_cts = null;
        }
    }
    void RoomStart()
    {
        if(room_polling_cts != null)
        {
            return;
        }
        room_polling_cts = new CancellationTokenSource();
        PollRoomStateServer(room_polling_cts.Token).Forget();
    }

    void RoomStop()
    {
        Debug.Log("RoomStop called");
        if (room_polling_cts != null)
        {
            room_polling_cts.Cancel();
            room_polling_cts.Dispose();
            room_polling_cts = null;
        }
    }

    private async UniTaskVoid PollServer(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            //Debug.Log("RoomID : " + playerManager.currentRoom.roomId);

            string playerInfo = await GetPollingRequest(serverUrl + "/state/playerInfo?roomId=" + playerManager.currentRoom.roomId, token);
            string turnInfo = await GetPollingRequest(serverUrl + "/state/turnInfo?roomId=" + playerManager.currentRoom.roomId, token);

            ApiResponse<List<PlayerInfo>> playerInfoResponse = JsonConvert.DeserializeObject<ApiResponse<List<PlayerInfo>>>(playerInfo);
            ApiResponse<TurnInfo> turnInfoResponse = JsonConvert.DeserializeObject<ApiResponse<TurnInfo>>(turnInfo);

            //Debug.Log($"[Poll] player : {playerInfoResponse.message}");
            //Debug.Log($"[Poll] turn : {turnInfoResponse.message} + {turnInfoResponse.data.currentTurnPlayerRoom} + {turnInfoResponse.data.currentTurnPlayerId}");

            playerManager.playerList = playerInfoResponse.data;
            mainGameManager.turnInfo = turnInfoResponse.data;

            if (is_debugging)
            {
                if (mainGameManager.turnInfo.currentTurnPlayerId != playerManager.this_player.id) 
                { 
                    playerManager.this_player.id = mainGameManager.turnInfo.currentTurnPlayerId;
                }
                
            }

            await UniTask.Delay((int)(pollInterval * 1000), cancellationToken: token);
        }
    }
    private async UniTaskVoid PollRoomStateServer(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            string roomInfo = await GetPollingRequest(serverUrl + "/room/state?roomId=" + playerManager.currentRoom.roomId, token);
            string playersInfo = await GetPollingRequest(serverUrl + "/room/players?roomId=" + playerManager.currentRoom.roomId, token);

            ApiResponse<RoomInfo> roomResponse = JsonConvert.DeserializeObject<ApiResponse<RoomInfo>>(roomInfo);
            ApiResponse<List<PlayerInfo>> playerResponse = JsonConvert.DeserializeObject<ApiResponse<List<PlayerInfo>>>(playersInfo);

            //Debug.Log($"[PollRoom] room : {roomResponse.message}");
            //Debug.Log($"[PollRoom] player : {playerResponse.message}");

            RoomInfo roomInfo1 = roomResponse.data;
            List<PlayerInfo> playerInfos = playerResponse.data;

            playerManager.currentRoom = roomInfo1;
            playerManager.playerList = playerInfos;

            await UniTask.Delay((int)(pollInterval * 1000), cancellationToken: token);
        }
    }

    public enum RoomActionType
    {
        Create,
        Join,
        Start,
    }

    public async UniTaskVoid RoomRequest(GameActionRequest request, RoomActionType actionType)
    {
        string json = JsonUtility.ToJson(request);
        string endpoint = actionType switch
        {
            RoomActionType.Create => "/room/create",
            RoomActionType.Join => "/room/join",
            RoomActionType.Start => "/room/start",
            _ => throw new System.ArgumentException("Invalid RoomActionType")
        };

        string result = await SendJsonToServer(serverUrl + endpoint, json);
        if (result != null)
        {
            ApiResponse<RoomInfo> response = JsonConvert.DeserializeObject<ApiResponse<RoomInfo>>(result);
            Debug.Log($"[RoomRequest] {response.message} RoomRequest RoomID : {response.data.roomId}");

            playerManager.currentRoom = response.data;
            if (request.playerId != playerManager.this_player.id)
            {
                return;
            }
            switch (actionType)
            {
                case RoomActionType.Create:
                    RoomStart();
                    break;
                case RoomActionType.Join:
                    RoomStart();
                    break;
                case RoomActionType.Start:
                    RoomStop();
                    GameStart();
                    break;
            }
        }
    }

    public async UniTaskVoid TextureRequest()
    {
        Texture2D result = await FetchTextureFromServer("https://api.dicebear.com/9.x/bottts/png");
        if (result != null)
        {
            //test_plane.GetComponent<Renderer>().material.mainTexture = result;
        }
    }
    // 미완
    public async UniTask YutRequest()
    {
        await SendJsonToServer(serverUrl + "/board/throw", JsonUtility.ToJson(new GameActionRequest { playerId = playerManager.this_player.id, roomId = playerManager.currentRoom.roomId }));
        string result = await FetchDataFromServer(serverUrl + "/private/info?roomId=" + playerManager.currentRoom.roomId + "&playerId=" + playerManager.this_player.id);

        ApiResponse<ThrowResponse> response = JsonConvert.DeserializeObject<ApiResponse<ThrowResponse>>(result);
        Debug.Log($"YutRequest {response.message} : {response.data.sticks}");

        mainGameManager.throwResponse = response.data;

    }

    public async UniTaskVoid PlayerRequest(string name, string style)
    {
        string result = await FetchDataFromServer(
            serverUrl + "/api/avatar/player?name=" + name + "&style=" + style);
        if (result != null)
        {
            ApiResponse<Player> response = JsonConvert.DeserializeObject<ApiResponse<Player>>(result);
            Player player = response.data;
            Debug.Log($"[PlayerRequest] {response.message}: {player.name} (ID: {player.id}) (is_local : {playerManager.this_player == null})");
            if (playerManager.this_player == null) {
                playerManager.this_player = player;
            }
            else
            {
                playerManager.debug_players.Add(player);
            }
        }
    }
    public async UniTaskVoid PrivateExitRequest()
    {
        await SendJsonToServer(
            serverUrl + "/private/exit", JsonUtility.ToJson(new GameActionRequest { playerId = playerManager.this_player.id, roomId = playerManager.currentRoom.roomId }));

        return;
    }

    /*
     * 이 아래로는 서버와 직접 통신하는 함수들
     * 더이상 건드리지 않아도 됨.
     */


    // 직접 서버에서 데이터를 받아오는 역할
    private async UniTask<string> FetchDataFromServer(string target_url)
    {
        using (var request = UnityWebRequest.Get(target_url))
        {
            request.SetRequestHeader("Accept", "application/json");
            await request.SendWebRequest().ToUniTask();

            if (request.result == UnityWebRequest.Result.Success)
                return request.downloadHandler.text;

            return null;
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

            try
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.text;
                }
                else
                {
                    Debug.LogError("전송 실패: " + request.error);
                    return null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("예외 발생: " + e.Message);
                return null;
            }
        }
    }
    private async UniTask<Texture2D> FetchTextureFromServer(string target_url)
    {
        using (var request = UnityWebRequestTexture.GetTexture(target_url))
        {
            try
            {
                request.SetRequestHeader("Accept", "image/*");
                await request.SendWebRequest().ToUniTask();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return DownloadHandlerTexture.GetContent(request);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Error] 이미지 요청 실패: {e.Message}");
                return null;
            }
            return null;
        }
    }

    // 폴링 요청 실제 처리 함수.
    private async UniTask<string> GetPollingRequest(string uri, CancellationToken token)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.SetRequestHeader("Accept", "application/json");
            try
            {
                await webRequest.SendWebRequest().WithCancellation(token);

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string json = webRequest.downloadHandler.text;
                    return json;
                }
                else
                {
                    Debug.LogWarning($"[Fail] 서버 에러: {webRequest.error}");
                    return null;
                }
            }
            catch (System.OperationCanceledException)
            {
                Debug.Log("폴링 취소");
                return null;
            }
        }
    }
}
