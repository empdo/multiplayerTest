using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandllePlayes : MonoBehaviour
{
    // Start is called before the first frame update

    List< KeyValuePair<int, GameObject>> players;
    public GameObject playerPrefab;

    public void initPlayers(int id, Vector3 position) {
        GameObject player = Instantiate(playerPrefab, position, Quaternion.identity) as GameObject;

        players.Add(new KeyValuePair<int, GameObject>(id, player));
    }

    public void removePlayer(int id) {
        players.ForEach(delegate(KeyValuePair<int, GameObject> keyPair) {
            if(keyPair.Key == id) {
               Destroy(keyPair.Value);
            }
        });
    }

    public void updatePosition(int id, Vector3 position) {
        players.ForEach(delegate(KeyValuePair<int, GameObject> keyPair) {
            if(keyPair.Key == id) {
               keyPair.Value.GetComponent<Rigidbody>().position = position; 
            }
        });
    }
}
