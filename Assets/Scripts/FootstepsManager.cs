// Manages vehicle creation based on where the camera is in the scene.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FootstepsManager : MonoBehaviour {

    public GameObject beastFootstep;
    public GameObject beastFootPrint;

    private GameObject[] beastFootsteps = new GameObject[10];

    private Dictionary<int, Vector2> footsteps;
    
    void Start () {
        footsteps = new Dictionary<int, Vector2>();
    }
    
    //void Update () {
    //}

    public void FootstepDown(Vector3 pos, Quaternion rot, int id) {
        beastFootsteps[id] = Instantiate(beastFootstep, pos, rot) as GameObject;
        footsteps.Add(id, new Vector2(pos.x, pos.z));
    }

    public void FootstepUp(int id) {
        // Swap foot step for foot print.
        Vector3 pos = beastFootsteps[id].transform.position;
        Vector3 rot = beastFootsteps[id].transform.eulerAngles;
        Destroy(beastFootsteps[id]);
        Instantiate(beastFootPrint, pos, Quaternion.Euler(rot));
        footsteps.Remove(id);
    }

    public bool IsCollidingWithAFootstep(float x, float y) {
        Vector2 pos = new Vector2(x, y);
        float length = 0.0f;
        foreach (KeyValuePair<int, Vector2> step in footsteps) {
            length = Vector2.Distance(step.Value, pos);
            if (length < 0.4f) {
                return true;
            }
        }
        return false;
    }
}
