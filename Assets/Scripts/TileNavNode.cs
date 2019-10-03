using UnityEngine;
using System.Collections;

public enum NodeType {
    ROAD = 0,
    SIDEWALK = 1
}

public enum AdjacentTileLinkDirection {
    NONE = 0,
    NORTH = 1,
    EAST = 2,
    SOUTH = 3,
    WEST = 4
}

public class TileNavNode : MonoBehaviour {

    public NodeType thisNodesType;
    public bool comeToStop = false;
    public AdjacentTileLinkDirection nextTileLinker;
    public TileNavNode[] nextNode;
    public AdjacentTileLinkDirection prevTileLinker;
    public TileNavNode[] prevNode;

    private void OnDrawGizmos() {
        if (Application.isPlaying == false) {
            for (int i = 0; i < nextNode.Length; i++) {
                Gizmos.DrawLine(gameObject.transform.position, nextNode[i].transform.position);
            }
            for (int i = 0; i < prevNode.Length; i++) {
                Gizmos.DrawLine(gameObject.transform.position, prevNode[i].transform.position);
            }
        }
    }

    //Create a space for the next / prev node linkers to store a node
    public void SetData(int rot) {
        if (nextNode.Length == 0) {
            nextNode = new TileNavNode[1];
        }
        if (prevNode.Length == 0) {
            prevNode = new TileNavNode[1];
        }

        nextTileLinker = LinkerRotator(nextTileLinker, rot);
        prevTileLinker = LinkerRotator(prevTileLinker, rot);
    }

    AdjacentTileLinkDirection LinkerRotator(AdjacentTileLinkDirection dir, int rot) {
        if (dir != AdjacentTileLinkDirection.NONE) {
            dir = dir + rot;
            if ((int)dir > 4) {
                dir = dir - 4;
            }
        }
        return dir;
    }
}
