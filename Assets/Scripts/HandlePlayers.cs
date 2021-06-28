using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlePlayers : MonoBehaviour
{
    // Start is called before the first frame update

    public Player localPlayer;

    Dictionary<int, Player> players; //id, gameobject
    public GameObject playerPrefab;

    public void InitPlayers(int id, Vector3 position) {
        Player newPlayer = Instantiate(playerPrefab, position, Quaternion.identity).GetComponent<Player>();

        players.Add(id, newPlayer);
    }

    public void RemovePlayer(int id) {
        Destroy(players[id]);
        players.Remove(id);
    }

    public void UpdatePlayerPosition(int id, Vector3 deltaPosition) {
        players[id].position += deltaPosition;
    }
}
