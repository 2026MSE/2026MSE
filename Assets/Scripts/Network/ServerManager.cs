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
    private CancellationTokenSource polling_cts;
    private CancellationTokenSource room_polling_cts;

    public GameObject test_plane;

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
            string playerInfo = await GetPollingRequest(serverUrl + "/state/playerInfo?=" + PlayerManager.instance.currentRoom.roomId, token);
            string turnInfo = await GetPollingRequest(serverUrl + "/state/turnInfo?=" + PlayerManager.instance.currentRoom.roomId, token);

            PlayerManager.instance.playerList = JsonConvert.DeserializeObject<ApiResponse<List<PlayerInfo>>>(playerInfo).data;
            MainGameManager.instance.turnInfo = JsonConvert.DeserializeObject<ApiResponse<TurnInfo>>(turnInfo).data;

            await UniTask.Delay((int)(pollInterval * 1000), cancellationToken: token);
        }
    }
    private async UniTaskVoid PollRoomStateServer(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            string roomInfo = await GetPollingRequest(serverUrl + "/room/state?roomId=" + PlayerManager.instance.currentRoom.roomId, token);
            string playersInfo = await GetPollingRequest(serverUrl + "/room/players?roomId=" + PlayerManager.instance.currentRoom.roomId, token);

            RoomInfo roomInfo1 = JsonConvert.DeserializeObject<RoomInfo>(roomInfo);

            PlayerManager.instance.currentRoom = roomInfo1;

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
            PlayerManager.instance.currentRoom = JsonUtility.FromJson<ApiResponse<RoomInfo>>(result).data;
            Debug.Log(PlayerManager.instance.currentRoom.roomId);
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
        string result = await FetchDataFromServer(
            serverUrl + "/private/result?roomId=" + PlayerManager.instance.currentRoom.roomId + "&playerId=" + PlayerManager.instance.this_player.id);

        JsonUtility.FromJsonOverwrite(result, PrivateRoom_GameManager.instance.yutResult);
    }

    public async UniTaskVoid PlayerRequest(string name, string style)
    {
        string result = await FetchDataFromServer(
            serverUrl + "/api/avatar/player?name=" + name + "&style=" + style);
        if (result != null)
        {
            Player player = JsonUtility.FromJson<Player>(result);
            PlayerManager.instance.this_player = player;
        }
    }
    public async UniTaskVoid PrivateExitRequest()
    {
        string result = await FetchDataFromServer(
            serverUrl + "/private/exit?roomId=" + PlayerManager.instance.currentRoom.roomId + "&playerId=" + PlayerManager.instance.this_player.id);

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
                    Debug.Log("전송 성공!");
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
