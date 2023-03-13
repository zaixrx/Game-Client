using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
    private static GameManager _instance;
    public static GameManager Instance { 
        get => _instance;
        set {
            if (_instance == null) {
                _instance = value;
            } else if (_instance != value) {
                Destroy(value);
            }
        }
    }

    public Dictionary<int, PlayerManager> players;

    public LocalPlayerController localPlayer;
    public RemotePlayerController[] remotePlayers;

    [SerializeField]
    private int maxPlayers = 30;

    [SerializeField]
    private PlayerManager playerPrefab;

    [SerializeField]
    private PlayerManager localPlayerPrefab;

    void Awake() {
        Instance = this;
        players = new Dictionary<int, PlayerManager>();
        remotePlayers = new RemotePlayerController[maxPlayers];
    }

    public void RemovePlayer(int id)
    {
        if (!players.ContainsKey(id)) return;

        Destroy(players[id].gameObject);
        remotePlayers[id] = null;
        players.Remove(id);
    }

    public PlayerManager InstantiatePlayer(ushort id, bool isLocalPlayer, Vector3 position) {
        var playerToIntstantitate = isLocalPlayer ? localPlayerPrefab : playerPrefab;
        var player = Instantiate(playerToIntstantitate,
                                 position,
                                 Quaternion.identity);

        player.id = id;
        player.isLocalPlayer = isLocalPlayer;

        players.Add(id, player);

        if (isLocalPlayer)
        {
            localPlayer = player.GetComponent<LocalPlayerController>();
        }
        else
        {
            remotePlayers[id] = player.GetComponent<RemotePlayerController>();
        }

        return player;
    }
}