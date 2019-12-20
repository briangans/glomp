using UnityEngine;
using UnityEditor;
using System.Collections;

public enum TileEdgeTypes {
    GENERIC = 0,
    ROADSTRAIGHT = 1,
    INTERSECTION = 2,
    SIDEWALK = 3,
    ROADCURVE = 4,
    SIDEWALKCURVE = 5
}

public class Tile : MonoBehaviour {
    public TileEdgeTypes north;
    public TileEdgeTypes east;
    public TileEdgeTypes south;
    public TileEdgeTypes west;

    public GameObject normal;
    public GameObject crushed;

    [HideInInspector]
    public TileNavNode[] navNodes;

    private int gridX;
    private int gridY;
    private BuildLevel buildLevel;
    private CameraManager cameraManager;
    private enum tileState {
        NORMAL = 0,
        CRUSHED = 1
    }
    private tileState currentState;

    void Start() {
        if (normal) {
            normal.SetActive(true);
        }
        if (crushed) {
            crushed.SetActive(false);
        }
        currentState = tileState.NORMAL;
        cameraManager = GameObject.Find("/Main Camera").GetComponent<CameraManager>();
    }

    // Glomped on by finger touch
    public void Crush() {
        if (currentState == tileState.NORMAL) {
            if (normal) {
                normal.SetActive(false);
                cameraManager.shakeAmount += 0.3f;
            }
            if (crushed) {
                crushed.SetActive(true);
            }
            currentState = tileState.CRUSHED;
        }
    }

    // Called by BuildLevel.cs
    public void SetData(int x, int y, int rotation, BuildLevel bl) {
        gridX = x;
        gridY = y;
        buildLevel = bl;

        navNodes = GetComponentsInChildren<TileNavNode>();
        for (int i = 0; i < navNodes.Length; i++) {
            navNodes[i].SetData(rotation);
        }
    }

    public void LinkEdgeNodes() {
        for (int i = 0; i < navNodes.Length; i++) {
            if (navNodes[i].nextTileLinker != AdjacentTileLinkDirection.NONE) {
                navNodes[i].nextNode[0] = RetrieveAdjacentTilesEdgeNode(navNodes[i].nextTileLinker, navNodes[i].thisNodesType, true);
            }
            if (navNodes[i].prevTileLinker != AdjacentTileLinkDirection.NONE) {
                navNodes[i].prevNode[0] = RetrieveAdjacentTilesEdgeNode(navNodes[i].prevTileLinker, navNodes[i].thisNodesType, false);
            }
        }
    }

    TileNavNode RetrieveAdjacentTilesEdgeNode(AdjacentTileLinkDirection direction, NodeType type, bool next) {
        switch (direction) {
            case AdjacentTileLinkDirection.NORTH:
                if (gridY < buildLevel.gridSize * 2) {
                    return buildLevel.createdTiles[gridX, gridY + 1].SupplyEdgeNode(AdjacentTileLinkDirection.SOUTH, type, next);
                } else {
                    return null;
                }
            case AdjacentTileLinkDirection.EAST:
                if (gridX < buildLevel.gridSize * 2) {
                    return buildLevel.createdTiles[gridX + 1, gridY].SupplyEdgeNode(AdjacentTileLinkDirection.WEST, type, next);
                } else {
                    return null;
                }
            case AdjacentTileLinkDirection.SOUTH:
                if (gridY > 0) {
                    return buildLevel.createdTiles[gridX, gridY - 1].SupplyEdgeNode(AdjacentTileLinkDirection.NORTH, type, next);
                } else {
                    return null;
                }
            case AdjacentTileLinkDirection.WEST:
                if (gridX > 0) {
                    return buildLevel.createdTiles[gridX - 1, gridY].SupplyEdgeNode(AdjacentTileLinkDirection.EAST, type, next);
                } else {
                    return null;
                }
            default:
                return null;
        }
    }

    public TileNavNode SupplyEdgeNode(AdjacentTileLinkDirection direction, NodeType type, bool next) {
        if (next == true) {
            for (int i = 0; i < navNodes.Length; i++) {
                if (navNodes[i].prevTileLinker == direction && navNodes[i].thisNodesType == type) {
                    return navNodes[i];
                }
            }
        } else {
            for (int i = 0; i < navNodes.Length; i++) {
                if (navNodes[i].nextTileLinker == direction && navNodes[i].thisNodesType == type) {
                    return navNodes[i];
                }
            }
        }
        return null;
    }
}