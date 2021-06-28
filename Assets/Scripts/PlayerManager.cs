using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Start is called before the first frame update

    public Player localPlayer;
    
    public Vector3 oldPosition = Vector3.zero;

    Dictionary<int, Player> players; //id, gameobject
    public GameObject playerPrefab;

    public ServerConnection serverConnection;

    void FixedUpdate() {
        if(localPlayer.position != oldPosition){
            serverConnection.SendPositionDelta(localPlayer.position - oldPosition);
        }

        oldPosition = localPlayer.position;
    }

    public void InitPlayer(int id, Vector3 position) {
        Player newPlayer = Instantiate(playerPrefab, position, Quaternion.identity).GetComponent<Player>();

        players.Add(id, newPlayer);
    }

    public void RemovePlayer(int id) {
        Destroy(players[id]);
        players.Remove(id);
    }

    public void UpdatePlayerPosition(int id, Vector3 deltaPosition) {
        if (players.ContainsKey(id)) {
            players[id].position += deltaPosition;
        } else {
            InitPlayer(id, deltaPosition);
        }
    }
}
