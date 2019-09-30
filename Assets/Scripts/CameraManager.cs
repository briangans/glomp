using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {
    public GameObject gameMaster;
    private BuildLevel buildLevel;
    private FootstepsManager footstepsManager;

    private Camera mainCamera;
    private Touch[] touches;

    [HideInInspector]
    public float shakeAmount = 0.0f;

    private Vector3 shakeVector = Vector3.zero;
    private Plane groundPlane = new Plane(Vector3.up, new Vector3(0, 0.01f, 0));
    private Ray groundRay;
    float rayDistance;
    private Vector3[] touchesPosWS = new Vector3[10];
    private int multiTouchNum;
    private Vector3 worldTouchPos1;
    private Vector3 footstep1pos;
    private Vector3 multiTouchCamDiff1;
    private Vector3 worldTouchPos2;
    private Vector3 footstep2pos;
    private Vector3 multiTouchCamDiff2;
    private float cameraRotationAngle;
    private float fov;

    private Vector3 pos; // Generic use vector

    void Start() {
        mainCamera = GetComponent<Camera>();
        buildLevel = gameMaster.GetComponent<BuildLevel>();
        footstepsManager = gameMaster.GetComponent<FootstepsManager>();
    }

    void Update() {
        touches = Input.touches;
        multiTouchNum = 0;
        transform.position -= shakeVector;
        for (int i = 0; i < touches.Length; i++) {
            // Started a touch
            if (touches[i].phase == TouchPhase.Began) {
                shakeAmount += 0.15f;
                groundRay = mainCamera.ScreenPointToRay(touches[i].position);
                if (groundPlane.Raycast(groundRay, out rayDistance)) {
                    pos = groundRay.GetPoint(rayDistance);
                    touchesPosWS[touches[i].fingerId] = pos;
                    footstepsManager.FootstepDown(pos, Quaternion.Euler(90, transform.eulerAngles.y, 0), touches[i].fingerId);

                    // TODO: Environment Crushing: do this more precisely, maybe with collision objects?
                    pos.x = Mathf.Clamp(pos.x, -buildLevel.gridSize, buildLevel.gridSize);
                    pos.z = Mathf.Clamp(pos.z, -buildLevel.gridSize, buildLevel.gridSize);
                    buildLevel.createdTiles[(int)Mathf.Round(pos.x + buildLevel.gridSize), (int)Mathf.Round(pos.z + buildLevel.gridSize)].Crush();
                }
            
            // Currently one touch
            } else if ((touches[i].phase == TouchPhase.Moved || touches[i].phase == TouchPhase.Stationary) && touches.Length == 1) {
                Vector2 deltaPositionRotated180 = Quaternion.Euler(0, 0, 180) * touches[i].deltaPosition;
                float cameraRotationAngle = Mathf.Atan2(deltaPositionRotated180.x, deltaPositionRotated180.y) * deltaPositionRotated180.magnitude / 50;
                Mathf.Clamp(cameraRotationAngle, -3.0f, 3.0f);
                transform.RotateAround(transform.position, Vector3.up, cameraRotationAngle);
                groundRay = mainCamera.ScreenPointToRay(touches[i].position);
                if (groundPlane.Raycast(groundRay, out rayDistance)) {
                    CameraMove(groundRay.GetPoint(rayDistance) - touchesPosWS[touches[i].fingerId]);
                }

            // Currently two touches
            } else if ((touches[i].phase == TouchPhase.Moved || touches[i].phase == TouchPhase.Stationary) && touches.Length == 2){
                multiTouchNum++;
                if (multiTouchNum == 1) {
                    groundRay = mainCamera.ScreenPointToRay(touches[i].position);
                    if (groundPlane.Raycast(groundRay, out rayDistance)) {
                        worldTouchPos1 = groundRay.GetPoint(rayDistance);
                        footstep1pos = touchesPosWS[touches[i].fingerId];
                    }
                } else if (multiTouchNum == 2) {
                    groundRay = mainCamera.ScreenPointToRay(touches[i].position);
                    if (groundPlane.Raycast(groundRay, out rayDistance)) {
                        multiTouchCamDiff1 = worldTouchPos1 - footstep1pos;
                        worldTouchPos2 = groundRay.GetPoint(rayDistance);
                        footstep2pos = touchesPosWS[touches[i].fingerId];
                        multiTouchCamDiff2 = worldTouchPos2 - footstep2pos;
                        cameraRotationAngle = Vector3.Angle(worldTouchPos1 - worldTouchPos2, footstep1pos - footstep2pos);
                        cameraRotationAngle *= Mathf.Sign(Vector3.Cross(worldTouchPos1 - footstep1pos, worldTouchPos1 - worldTouchPos2).y);
                        cameraRotationAngle = Mathf.Clamp(cameraRotationAngle, -3f, 3f);
                        transform.RotateAround(transform.position, Vector3.up, cameraRotationAngle);
                        CameraMove((multiTouchCamDiff1 + multiTouchCamDiff2) / 2);
                        fov = mainCamera.fieldOfView - (Vector3.Magnitude(worldTouchPos1 - worldTouchPos2) - Vector3.Magnitude(footstep1pos - footstep2pos));
                        mainCamera.fieldOfView = Mathf.Clamp(fov, 55, 65);
                    }
                }

            // Released a touch
            } else if (touches[i].phase == TouchPhase.Ended || touches[i].phase == TouchPhase.Canceled) {
                footstepsManager.FootstepUp(touches[i].fingerId);
            }
        }
        if (touches.Length != 2) {
            // Return camera fov to normal 60 degrees
            float fovChange = mainCamera.fieldOfView - 60;
            mainCamera.fieldOfView -= Mathf.Clamp(fovChange, -0.1f, 0.1f);
        }
        CameraShake();
    }

    void CameraMove(Vector3 dir) {
        dir = Vector3.Magnitude(dir) < 0.06f ? dir : Vector3.Normalize(dir) * 0.06f;
        transform.position -= dir;
    }

    void CameraShake() {
        if (shakeAmount > 0.0f) {
            if (shakeAmount > 0.7f) {
                shakeAmount = 0.7f;
            }
            Vector3 camPos = transform.position;
            shakeVector.x = Mathf.PerlinNoise(Time.time * 13, 0) - 0.5f;
            shakeVector.z = Mathf.PerlinNoise(Time.time * 13, 1) - 0.5f;
            shakeVector = shakeVector * shakeAmount;
            transform.position = camPos + shakeVector;
            shakeAmount -= Time.deltaTime;
            if (shakeAmount < 0.005f) {
                shakeAmount = 0.0f;
                shakeVector = Vector3.zero;
            }
        }
    }
}
