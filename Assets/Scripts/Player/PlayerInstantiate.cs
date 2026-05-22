using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInstantiate : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public List<Transform> playerSpawnPoints = new List<Transform>();

    [Header("Options")]
    public float waitInterval = 0.2f;
    public float maxWaitTime = 10f;

    [Header("Debug")]
    public bool isDebugging = true;

    private PlayerManager playerManager;
    private MainGameManager mainGameManager;

    public List<GameObject> playerObjects { get; private set; } = new List<GameObject>();

    private bool initialized = false;

    private IEnumerator Start()
    {
        DebugLog("[PlayerInstantiate] Start - waiting for game state.");

        float elapsed = 0f;

        while (elapsed < maxWaitTime)
        {
            ResolveManagers();

            if (CanInstantiatePlayers())
            {
                InstantiatePlayers();
                yield break;
            }

            elapsed += waitInterval;
            yield return new WaitForSeconds(waitInterval);
        }

        Debug.LogError("[PlayerInstantiate] Failed to initialize players. playerList or turnInfo is not ready.");
        PrintCurrentState();
    }

    private void ResolveManagers()
    {
        if (playerManager == null)
            playerManager = PlayerManager.instance;

        if (mainGameManager == null)
            mainGameManager = MainGameManager.instance;
    }

    private bool CanInstantiatePlayers()
    {
        if (initialized)
            return false;

        if (playerPrefab == null)
        {
            DebugLog("[PlayerInstantiate] playerPrefab is null.");
            return false;
        }

        if (playerSpawnPoints == null || playerSpawnPoints.Count == 0)
        {
            DebugLog("[PlayerInstantiate] playerSpawnPoints is empty.");
            return false;
        }

        if (playerManager == null)
        {
            DebugLog("[PlayerInstantiate] playerManager is null.");
            return false;
        }

        if (playerManager.this_player == null || string.IsNullOrEmpty(playerManager.this_player.id))
        {
            DebugLog("[PlayerInstantiate] this_player is not ready.");
            return false;
        }

        if (playerManager.playerList == null || playerManager.playerList.Count == 0)
        {
            DebugLog("[PlayerInstantiate] playerList is not ready.");
            return false;
        }

        if (mainGameManager == null)
        {
            DebugLog("[PlayerInstantiate] mainGameManager is null.");
            return false;
        }

        if (mainGameManager.turnInfo == null)
        {
            DebugLog("[PlayerInstantiate] turnInfo is null.");
            return false;
        }

        if (string.IsNullOrEmpty(mainGameManager.turnInfo.currentTurnPlayerId))
        {
            DebugLog("[PlayerInstantiate] currentTurnPlayerId is null or empty.");
            return false;
        }

        return true;
    }

    private void InstantiatePlayers()
    {
        initialized = true;

        ClearExistingPlayers();

        string myPlayerId = playerManager.this_player.id;
        string turnPlayerId = mainGameManager.turnInfo.currentTurnPlayerId;

        Debug.Log("[PlayerInstantiate] InstantiatePlayers");
        Debug.Log("[PlayerInstantiate] myPlayerId: " + myPlayerId);
        Debug.Log("[PlayerInstantiate] turnPlayerId: " + turnPlayerId);
        Debug.Log("[PlayerInstantiate] player count: " + playerManager.playerList.Count);

        int otherIndex = 1;

        foreach (PlayerInfo player in playerManager.playerList)
        {
            if (player == null || string.IsNullOrEmpty(player.playerId))
            {
                Debug.LogWarning("[PlayerInstantiate] Invalid player info.");
                continue;
            }

            int spawnIndex;

            if (player.playerId == turnPlayerId)
            {
                spawnIndex = 0;
            }
            else
            {
                spawnIndex = otherIndex;
                otherIndex++;
            }

            if (spawnIndex >= playerSpawnPoints.Count)
            {
                Debug.LogWarning($"[PlayerInstantiate] Not enough spawn points. playerId={player.playerId}, spawnIndex={spawnIndex}");
                spawnIndex = playerSpawnPoints.Count - 1;
            }

            Transform spawnPoint = playerSpawnPoints[spawnIndex];

            if (spawnPoint == null)
            {
                Debug.LogWarning($"[PlayerInstantiate] Spawn point is null. index={spawnIndex}");
                continue;
            }

            GameObject obj = Instantiate(
                playerPrefab,
                spawnPoint.position,
                spawnPoint.rotation,
                spawnPoint
            );

            obj.name = $"Player_{player.playerId}_{player.name}";

            playerObjects.Add(obj);

            PlayerController controller = obj.GetComponent<PlayerController>();

            if (controller != null)
            {
                controller.is_local_player = player.playerId == myPlayerId;
            }
            else
            {
                Debug.LogWarning("[PlayerInstantiate] PlayerController not found on playerPrefab.");
            }

            Debug.Log($"[PlayerInstantiate] Spawned player: {player.name} / {player.playerId} / local={player.playerId == myPlayerId} / spawnIndex={spawnIndex}");
        }
    }

    private void ClearExistingPlayers()
    {
        foreach (GameObject obj in playerObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        playerObjects.Clear();
    }

    private void PrintCurrentState()
    {
        ResolveManagers();

        Debug.Log("========== [PlayerInstantiate Current State] ==========");

        Debug.Log("playerPrefab: " + (playerPrefab != null));
        Debug.Log("spawnPointCount: " + (playerSpawnPoints != null ? playerSpawnPoints.Count : -1));

        Debug.Log("playerManager: " + (playerManager != null));
        Debug.Log("this_player: " + (playerManager != null && playerManager.this_player != null));
        Debug.Log("this_player.id: " +
            (playerManager != null && playerManager.this_player != null ? playerManager.this_player.id : "null"));

        Debug.Log("playerList count: " +
            (playerManager != null && playerManager.playerList != null ? playerManager.playerList.Count : -1));

        Debug.Log("mainGameManager: " + (mainGameManager != null));
        Debug.Log("turnInfo: " + (mainGameManager != null && mainGameManager.turnInfo != null));
        Debug.Log("currentTurnPlayerId: " +
            (mainGameManager != null && mainGameManager.turnInfo != null
                ? mainGameManager.turnInfo.currentTurnPlayerId
                : "null"));

        Debug.Log("=======================================================");
    }

    private void DebugLog(string message)
    {
        if (isDebugging)
            Debug.Log(message);
    }
}