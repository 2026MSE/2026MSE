using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;

public class ServerManager : MonoBehaviour
{
    public static ServerManager instance { get; private set; }

    public bool isUsingServer = false;
    public string serverUrl = "http://localhost:8080";
    public GameRoom gameRoom { get; private set; }
    public float pollInterval = 1.0f;
    private CancellationTokenSource polling_cts;

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
    private void OnDestroy()
    {
        if (polling_cts != null)
        {
            polling_cts.Cancel();
            polling_cts.Dispose();
        }
    }
    void GameStart()
    {
        polling_cts = new CancellationTokenSource();
        PollServer(polling_cts.Token).Forget();
    }

    void GameStop()
    {
        if (polling_cts != null)
        {
            polling_cts.Cancel();
            polling_cts.Dispose();
            polling_cts = null;
        }
    }

    private async UniTaskVoid PollServer(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            string playerInfo = await GetPollingRequest(serverUrl + "/state/playerInfo", token);
            string turnInfo = await GetPollingRequest(serverUrl + "/state/turnInfo", token);

            PlayerManager.instance.playerList = JsonConvert.DeserializeObject<List<PlayerInfo>>(playerInfo);
            MainGameManager.instance.turnInfo = JsonConvert.DeserializeObject<TurnInfo>(turnInfo);

            await UniTask.Delay((int)(pollInterval * 1000), cancellationToken: token);
        }
    }

    public async UniTaskVoid CreateRoomRequest(string roomName, int maxPlayers)
    {
        string result = await FetchDataFromServer(serverUrl + "/api/room/create?roomName=" + roomName + "&maxPlayers=" + maxPlayers);
        if (result != null)
        {
            gameRoom = JsonUtility.FromJson<GameRoom>(result);
            GameStart();
        }
    }

    // 미완
    public async UniTask YutRequest()
    {
        string result = await FetchDataFromServer(serverUrl + "/private/result?roomId=" + gameRoom.roomId + "&playerId=" + PlayerManager.instance.this_player.id);

        JsonUtility.FromJsonOverwrite(result, PrivateRoom_GameManager.instance.yutResult);
    }

    public async UniTaskVoid PlayerRequest(string name, string style)
    {
        string result = await FetchDataFromServer(serverUrl + "/api/avatar/player?name=" + name + "&style=" + style);
        if (result != null)
        {
            Player player = JsonUtility.FromJson<Player>(result);
            PlayerManager.instance.this_player = player;
        }
    }
    public async UniTaskVoid PrivateExitRequest()
    {
        string result = await FetchDataFromServer(serverUrl + "/private/exit?roomId=" + gameRoom.roomId + "&playerId=" + PlayerManager.instance.this_player.id);

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

    // 폴링 요청 실제 처리 함수.
    private async UniTask<string> GetPollingRequest(string uri, CancellationToken token)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
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
