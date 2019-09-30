using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Vehicle : MonoBehaviour {

    public GameObject normal;
    public GameObject crushed;

    private TileNavNode currentNavNode;
    private TileNavNode previousNavNode;

    private float timeStamp = -1.0f;
    private float elapsedTime = 0.0f;
    private Vector3 originPos;
    private Quaternion originRot;
    private Vector3 originFwd;
    private float targetSpeed = 0.8f;
    private Vector3 destinationPos;
    private Quaternion destinationRot;
    private Vector3 destinationFwd;
    private float process = 1.0f;
    private float distance = 0.1f;
    private Vector3 originPosFar;
    private Vector3 destinationPosFar;

    private CameraManager cameraManager;
    private FootstepsManager footstepsManager;
    private BoxCollider boxCollider;

    private enum VehicleState {
        PATHING = 0,
        WAITING = 1,
        CRUSHED = 2
    }
    private VehicleState currentState;

    void Start () {
        if (normal) {
            normal.SetActive(true);
        }
        if (crushed) {
            crushed.SetActive(false);
        }

        cameraManager = GameObject.Find("/Main Camera").GetComponent<CameraManager>();
        footstepsManager = GameObject.Find("/GameMaster").GetComponent<FootstepsManager>();
        boxCollider = gameObject.GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;

        currentState = VehicleState.PATHING;
    }

    //******************WIP
    //private void OnTriggerEnter(Collider other) {
    //    Crush();
    //}
    //*******************

    /**
     * @param t		Current time (in frames or seconds).
     * @param b		Starting value.
     * @param c		Change needed in value.
     * @param d		Expected easing duration (in frames or seconds).
     * @return		The correct value.
     */
    public static float EaseInQuad(float t, float b, float c, float d) {
        return c * (t /= d) * t + b;
    }
    public static float EaseOutQuad(float t, float b, float c, float d) {
        return -c * (t /= d) * (t - 2) + b;
    }

    void Update () {
        if (currentState == VehicleState.WAITING) {
            // If intersection is empty
            currentState = VehicleState.PATHING;
        }

        if (currentState == VehicleState.PATHING) {

            elapsedTime = Time.time - timeStamp;
            process = elapsedTime / distance * targetSpeed;

            // Stop at intersections
            if (process < 1) {
                if (currentNavNode.comeToStop == true) {
                    process = EaseOutQuad(elapsedTime, 0, 1, distance / (targetSpeed / 2));
                } else if (previousNavNode.comeToStop == true) {
                    process = EaseInQuad(elapsedTime, 0, 1, distance / (targetSpeed / 2));
                }
            }

            if (process >= 1) {
                process = 0;
                if (currentNavNode.comeToStop == true) {
                    currentState = VehicleState.WAITING;
                }
                previousNavNode = currentNavNode;
                currentNavNode = FindNextWaypoint(currentNavNode);
                originPos = transform.position;
                originRot = transform.rotation;
                originFwd = -transform.forward;
                destinationPos = currentNavNode.transform.position;
                destinationRot = currentNavNode.transform.rotation;
                destinationFwd = -currentNavNode.transform.forward;
                timeStamp = Time.time;
                distance = Vector3.Distance(originPos, destinationPos);
            }

            // Interpolate to next nav node
            transform.position = Clerp();
            transform.rotation = Quaternion.Slerp(originRot, destinationRot, process);

            if (footstepsManager.IsCollidingWithAFootstep(this.transform.position.x, this.transform.position.z)) {
                Crush();
            }
        }
    }

    public void SetData(TileNavNode nnp) {
        transform.position = nnp.transform.position;
        transform.rotation = nnp.transform.rotation;
        currentNavNode = nnp;
    }

    // Glomped on by finger touch
    public void Crush() {
        if (normal) {
            normal.SetActive(false);
            cameraManager.shakeAmount += 0.1f;
        }
        if (crushed) {
            crushed.SetActive(true);
        }

        currentState = VehicleState.CRUSHED;
    }

    TileNavNode FindNextWaypoint(TileNavNode node) {
        node = node.nextNode[Random.Range(0, node.nextNode.Length)];
        if (node == null) {
            Tile tile = node.transform.parent.GetComponent<Tile>();
            node = tile.navNodes[Random.Range(0, tile.navNodes.Length)];
        }
        return node;
    }

    Vector3 Clerp() {
        originPosFar = originPos + originFwd * distance / 2;
        destinationPosFar = destinationPos - destinationFwd * distance / 2;
        originPosFar = Vector3.Lerp(originPos, originPosFar, process);
        destinationPosFar = Vector3.Lerp(destinationPosFar, destinationPos, process);
        return Vector3.Lerp(originPosFar, destinationPosFar, process);
    }
}