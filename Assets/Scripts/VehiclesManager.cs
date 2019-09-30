// Manages vehicle creation based on where the camera is in the scene.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VehiclesManager : MonoBehaviour {

    public CameraManager cameraManager;
    private MasterStateMachine stateMachine;
    private BuildLevel buildLevel;

    private Vector2 camPos;
    public Vehicle[] vehicles;
    public const int MAX_NUM_OF_VEHICLES = 5;

    private Vehicle[] instantiatedVehicles;

    private int numVehicles = 0;
    private const float ACTOR_RANGE = 5.5f;

    private enum VehiclesManagerState {
        IDLE = 0,
        ACTIVE = 1
    }

    private VehiclesManagerState currentState;
    
    void Start () {
        currentState = VehiclesManagerState.IDLE;

        instantiatedVehicles = new Vehicle[MAX_NUM_OF_VEHICLES];

        buildLevel = GetComponent<BuildLevel>();
        stateMachine = GetComponent<MasterStateMachine>();
        stateMachine.AddStateEnterListener(MasterStateMachine.State.RUN_GAME, Activate);
    }
    
    void Update () {
        if (currentState == VehiclesManagerState.ACTIVE) {
            camPos = new Vector2(cameraManager.transform.position.x, cameraManager.transform.position.z);

            if (numVehicles < MAX_NUM_OF_VEHICLES) {
                TryCreateVehicle();
            }

            // Destroy if too far away
            for (int i = 0; i < numVehicles; i++) {
                Vector2 carPos = new Vector2(instantiatedVehicles[i].transform.position.x, instantiatedVehicles[i].transform.position.z);
                if (Vector2.Distance(camPos, carPos) > ACTOR_RANGE) {
                    DestroyVehicle(i);
                };
            }
        }
    }

    public void Activate() {
        currentState = VehiclesManagerState.ACTIVE;
    }
    
    void TryCreateVehicle() {
        int x = Mathf.RoundToInt(camPos.x) + buildLevel.gridSize;
        int y = Mathf.RoundToInt(camPos.y) + buildLevel.gridSize;
        float randomAngle = Mathf.Deg2Rad * Random.Range(0.0f, 360.0f);
        x += Mathf.RoundToInt(Mathf.Cos(randomAngle) * (ACTOR_RANGE - 1));
        y += Mathf.RoundToInt(Mathf.Sin(randomAngle) * (ACTOR_RANGE - 1));
        if (x >= 0 && x <= buildLevel.gridSize * 2 && y >= 0 && y <= buildLevel.gridSize * 2) {
            for (int i = 0; i < buildLevel.createdTiles[x, y].navNodes.Length; i++) {
                if (buildLevel.createdTiles[x, y].navNodes[i].thisNodesType == NodeType.ROAD) {
                    instantiatedVehicles[numVehicles] = Instantiate(vehicles[Random.Range(0, vehicles.Length)]) as Vehicle;
                    instantiatedVehicles[numVehicles].SetData(buildLevel.createdTiles[x, y].navNodes[i]);
                    numVehicles++;
                    break;
                }
            }
        }
    }

    void DestroyVehicle(int i) {
        Destroy(instantiatedVehicles[i].gameObject);
        for (int j = i; j < numVehicles - 1; j++) {
            instantiatedVehicles[j] = instantiatedVehicles[j + 1];
        }
        numVehicles--;
    }
}
