using UnityEngine;
using System.Collections;

public class BuildLevel : MonoBehaviour {
    public int gridSize = 5;

    [Header("Last element is versatile cul de sac.")]
    public Tile[] tilePrefabs; // array of all tile types, road, curve, building, etc

    [HideInInspector]
    public Tile[,] createdTiles; // 2d array of references to each tile gameobject in the grid

    [HideInInspector]
    public TileEdgeTypes[,,] tileEdges;

    private MasterStateMachine stateMachine;

    void Start() {
        stateMachine = GetComponent<MasterStateMachine>();
        stateMachine.AddStateEnterListener(MasterStateMachine.State.PLACING_TILES, PlaceTiles);
    }
    
    public void PlaceTiles() {
        tileEdges = new TileEdgeTypes[gridSize * 2 + 1, gridSize * 2 + 1, 4];
        createdTiles = new Tile[gridSize * 2 + 1, gridSize * 2 + 1];
        bool compatibleTile;
        int loops;
        int tileNum = 0;
        int rotation = 0;
        int flexibility = 0;
        int excludeCulDeSac = 1;
        for (int i = -gridSize; i <= gridSize; i++) {
            for (int j = -gridSize; j <= gridSize; j++) {
                compatibleTile = false;
                loops = 0;
                flexibility = 0;
                excludeCulDeSac = 1;

                // Need to re-do this with probablilties for roads vs non roads, less brute force.
                while (compatibleTile == false) {
                    tileNum = Mathf.FloorToInt(Random.Range(0, tilePrefabs.Length - excludeCulDeSac));
                    rotation = Mathf.FloorToInt(Random.Range(0, 4));
                    if (j > -gridSize) {
                        compatibleTile = TileEdgesCompatible(tileEdges[i + gridSize, j + gridSize - 1, 0], TileEdgeRotate(tilePrefabs[tileNum], 2 - rotation), flexibility);
                    } else {
                        compatibleTile = true;
                    }
                    if (compatibleTile == true && i > -gridSize) {
                        compatibleTile = TileEdgesCompatible(tileEdges[i + gridSize - 1, j + gridSize, 1], TileEdgeRotate(tilePrefabs[tileNum], 3 - rotation), flexibility);
                    }
                    loops++;
                    if (loops == 200) {
                        flexibility = 1;
                    } else if (loops == 300) {
                        flexibility = 2;
                        excludeCulDeSac = 0;
                    } else if (loops > 400) {
                        //Debug.Log("Giving up on " + i + ", " + j);
                        compatibleTile = true;
                    }
                }
                // Create tile and store reference
                createdTiles[i + gridSize, j + gridSize] =
                    Instantiate(tilePrefabs[tileNum], new Vector3(i, 0.0f, j), Quaternion.Euler(0.0f, rotation * 90, 0.0f)) as Tile;
                createdTiles[i + gridSize, j + gridSize].SetData(i + gridSize, j + gridSize, rotation, this);
                tileEdges[i + gridSize, j + gridSize, 0] = TileEdgeRotate(tilePrefabs[tileNum], 0 - rotation);
                tileEdges[i + gridSize, j + gridSize, 1] = TileEdgeRotate(tilePrefabs[tileNum], 1 - rotation);
                tileEdges[i + gridSize, j + gridSize, 2] = TileEdgeRotate(tilePrefabs[tileNum], 2 - rotation);
                tileEdges[i + gridSize, j + gridSize, 3] = TileEdgeRotate(tilePrefabs[tileNum], 3 - rotation);
            }
        }

        // Use next and prev node linkers to find adjacent tile's corresponding nodes and point them at each other
        for (int i = -gridSize; i <= gridSize; i++) {
            for (int j = -gridSize; j <= gridSize; j++) {
                createdTiles[i + gridSize, j + gridSize].LinkEdgeNodes();
            }
        }
        
        stateMachine.NextState();
    }

    // Returns the tileEdgeType for the provided tile and rotation
    TileEdgeTypes TileEdgeRotate(Tile tile, int rotation) {
        if (rotation > 3) {
            rotation -= 4;
        }
        if (rotation < 0) {
            rotation += 4;
        }
        switch (rotation) {
            case 0:
                return tile.north;
            case 1:
                return tile.east;
            case 2:
                return tile.south;
            case 3:
                return tile.west;
            default:
                return TileEdgeTypes.GENERIC;
        }
    }

    // Returns true if the two edge types will tile together.
    bool TileEdgesCompatible(TileEdgeTypes one, TileEdgeTypes two, int flexibility) {
        if (flexibility == 0) {
            switch (one) {
                case TileEdgeTypes.GENERIC:
                    return two == TileEdgeTypes.GENERIC || two == TileEdgeTypes.SIDEWALK || two == TileEdgeTypes.SIDEWALKCURVE;
                case TileEdgeTypes.ROADSTRAIGHT:
                    return two == TileEdgeTypes.ROADSTRAIGHT || two == TileEdgeTypes.ROADCURVE || two == TileEdgeTypes.INTERSECTION;
                case TileEdgeTypes.INTERSECTION:
                    return two == TileEdgeTypes.ROADSTRAIGHT;
                case TileEdgeTypes.SIDEWALK:
                    return two == TileEdgeTypes.GENERIC;
                case TileEdgeTypes.ROADCURVE:
                    return two == TileEdgeTypes.ROADSTRAIGHT;
                case TileEdgeTypes.SIDEWALKCURVE:
                    return two == TileEdgeTypes.GENERIC;
                default:
                    return false;
            }
        } else if (flexibility == 1) {
            switch (one) {
                case TileEdgeTypes.GENERIC:
                    return two == TileEdgeTypes.GENERIC || two == TileEdgeTypes.SIDEWALK || two == TileEdgeTypes.SIDEWALKCURVE;
                case TileEdgeTypes.ROADSTRAIGHT:
                    return two == TileEdgeTypes.ROADSTRAIGHT || two == TileEdgeTypes.ROADCURVE || two == TileEdgeTypes.INTERSECTION;
                case TileEdgeTypes.INTERSECTION:
                    return two == TileEdgeTypes.ROADSTRAIGHT || two == TileEdgeTypes.ROADCURVE;
                case TileEdgeTypes.SIDEWALK:
                    return two == TileEdgeTypes.GENERIC;
                case TileEdgeTypes.ROADCURVE:
                    return two == TileEdgeTypes.ROADSTRAIGHT || two == TileEdgeTypes.ROADCURVE;
                case TileEdgeTypes.SIDEWALKCURVE:
                    return two == TileEdgeTypes.GENERIC || two == TileEdgeTypes.SIDEWALKCURVE;
                default:
                    return false;
            }
        } else if (flexibility == 2) {
            switch (one) {
                case TileEdgeTypes.GENERIC:
                    return two == TileEdgeTypes.GENERIC || two == TileEdgeTypes.SIDEWALK || two == TileEdgeTypes.SIDEWALKCURVE;
                case TileEdgeTypes.ROADSTRAIGHT:
                    return two == TileEdgeTypes.ROADSTRAIGHT || two == TileEdgeTypes.ROADCURVE || two == TileEdgeTypes.INTERSECTION;
                case TileEdgeTypes.INTERSECTION:
                    return two == TileEdgeTypes.ROADSTRAIGHT || two == TileEdgeTypes.ROADCURVE;
                case TileEdgeTypes.SIDEWALK:
                    return two == TileEdgeTypes.GENERIC || two == TileEdgeTypes.SIDEWALKCURVE;
                case TileEdgeTypes.ROADCURVE:
                    return two == TileEdgeTypes.ROADSTRAIGHT || two == TileEdgeTypes.ROADCURVE || two == TileEdgeTypes.INTERSECTION;
                case TileEdgeTypes.SIDEWALKCURVE:
                    return two == TileEdgeTypes.GENERIC || two == TileEdgeTypes.SIDEWALK || two == TileEdgeTypes.SIDEWALKCURVE;
                default:
                    return false;
            }
        }
        return true;
    }
}