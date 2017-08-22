using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraRig : MonoBehaviour {
    public Transform target;
    public bool autoTargetPlayer;
    public LayerMask wallLayers;

    public enum Shoulder
    {
        right,left
    }
    public Shoulder shoulder;

    [System.Serializable]
    public class CameraSettings
    {
        [Header("-Postioning-")]
        public Vector3 camPositionOffsetLeft;
        public Vector3 camPositionOffsetRight;

        [Header("-Camera Options-")]
        public float mouseXSensitivity = 5.0f;
        public float mouseYSensitivity = 5.0f;
        public float minAngle = -30.0f;
        public float maxAngle = 70.0f;
        public float rotationSpeed = 5.0f;
        public float maxCheckDist = .1f;

        [Header("-Zoom settings-")]
        public float fieldOfView = 70.0f;
        public float zoomFieldOfView = 30.0f;
        public float zoomSpeed = 3.0f;

        [Header("-Visual Options-")]
        public float hidMeshWhenDistance = 0.5f;

    }
    [SerializeField]
    public CameraSettings cameraSettings;

    [System.Serializable]
    public class InputSettings
    {
        public string verticalAxis = "Mouse X";
        public string horizontalAxis = "Mouse Y";
        public string aimButton = "Fire2";
        public string switchShoulderButton = "Fire4";
    }
    [SerializeField]
    public InputSettings input;

    [System.Serializable]
    public class MovementSettings
    {
        public float movementLerpSpeed = 5.0f;
    }
    [SerializeField]
    public MovementSettings movement;

    Transform pivot;
    Camera mainCamera;
    float newX = 0.0f;
    float newY = 0.0f;


    // Use this for initialization
    void Start () {
        mainCamera = Camera.main;
        pivot = transform.GetChild(0);
	}
	
	// Update is called once per frame
	void Update () {
        if (target)
        {
            if (Application.isPlaying)
            {
                RotateCamera();
                CheckWall();
                CheckMeshRender();
                Zoom(Input.GetButton(input.aimButton));

                if (Input.GetButtonDown(input.switchShoulderButton))
                {
                    SwitchShoulders();
                }
            }
        }
	}
    void LateUpdate()
    {
        if (!target)
        {
            TargetPlayer();
        }
        else
        {
            Vector3 targetPostion = target.position;
            Quaternion targetRotation = target.rotation;

            FollowTarget(targetPostion, targetRotation);
        }
    }

    //Finds the player gameobject and sets it as target
    void TargetPlayer()
    {
        if (autoTargetPlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player)
            {
                Transform playerT = player.transform;
                target = playerT;
            }
        }
    }
    //Following the target with Time.deltaTime smoothly
    void FollowTarget(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (!Application.isPlaying)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
        else
        {
            Vector3 newPos = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movement.movementLerpSpeed);
            transform.position = newPos;
        }
    }
    //Hides the targets mesh render when too close
    void CheckMeshRender()
    {
        if (!mainCamera || !target)
            return;
        SkinnedMeshRenderer[] meshes = target.GetComponentsInChildren<SkinnedMeshRenderer>();
        Transform mainCamT = mainCamera.transform;
        Vector3 mainCamPos = mainCamT.position;
        Vector3 targetPos = target.position;
        float dist = Vector3.Distance(mainCamPos, (targetPos + target.up));

        if (meshes.Length > 0)
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                if(dist <= cameraSettings.hidMeshWhenDistance)
                {
                    meshes[i].enabled = false;
                }
                else
                {
                    meshes[i].enabled = true;
                }
            }
        }
    
    }

    void CheckWall()
    {
        if (!pivot || !mainCamera)
            return;
        RaycastHit hit;
        Transform mainCamT = mainCamera.transform;
        Vector3 mainCamPos = mainCamT.position;
        Vector3 pivotPos = pivot.position;

        Vector3 start = pivotPos;
        Vector3 dir = mainCamPos - pivotPos;

        float dist = Mathf.Abs(shoulder == Shoulder.left ? cameraSettings.camPositionOffsetLeft.z : cameraSettings.camPositionOffsetRight.z);
       
        if(Physics.SphereCast(start, cameraSettings.maxCheckDist, dir, out hit, dist, wallLayers))
        {
            MoveCamUp(hit, pivotPos, dir, mainCamT);
        }
        else
        {
            switch (shoulder)
            {
                case Shoulder.left:
                    PostionCamera(cameraSettings.camPositionOffsetLeft);
                    break;
                case Shoulder.right:
                    PostionCamera(cameraSettings.camPositionOffsetRight);
                    break;
            }
        }
    }
    //This Moves the camera Forward when we hit a wall
    void MoveCamUp(RaycastHit hit, Vector3 pivotPos, Vector3 dir, Transform cameraT)
    {
        float hitDis = hit.distance;
        Vector3 sphereCastCenter = pivotPos + (dir.normalized * hitDis);
        cameraT.position = sphereCastCenter;
    }
    // Positions the cameras localPosition to a given location
    void PostionCamera(Vector3 cameraPos)
    {
        if (!mainCamera)
            return;
        Transform mainCamt = mainCamera.transform;
        Vector3 mainCamPos = mainCamt.localPosition;
        Vector3 newPos = Vector3.Lerp(mainCamPos, cameraPos, Time.deltaTime * movement.movementLerpSpeed);
        mainCamt.localPosition = newPos;
    }

    void RotateCamera()
    {
        if (!pivot)
        {
            return;
        }
        newX += cameraSettings.mouseXSensitivity * Input.GetAxis(input.verticalAxis);
        newY += cameraSettings.mouseYSensitivity * Input.GetAxis(input.horizontalAxis);

        Vector3 eulerAngleAxis = new Vector3();
        eulerAngleAxis.x = newY;
        eulerAngleAxis.y = newX;

        newX = Mathf.Repeat(newX, 360);
        newY = Mathf.Clamp(newY, cameraSettings.minAngle, cameraSettings.maxAngle);

        Quaternion newRotation = Quaternion.Slerp(pivot.localRotation, Quaternion.Euler(eulerAngleAxis), Time.deltaTime * cameraSettings.rotationSpeed);

        pivot.localRotation = newRotation;   
    }
    //Zooms camera when aimbutton is down
    void Zoom(bool isZooming)
    {
        if (!mainCamera) return;

        if (isZooming)
        {
            float newFieldOfView = Mathf.Lerp(mainCamera.fieldOfView, cameraSettings.zoomFieldOfView, Time.deltaTime * cameraSettings.zoomSpeed);
            mainCamera.fieldOfView = newFieldOfView;
        }
        else
        {
            float originalFieldOfView = Mathf.Lerp(mainCamera.fieldOfView, cameraSettings.fieldOfView, Time.deltaTime * cameraSettings.zoomSpeed);
            mainCamera.fieldOfView = originalFieldOfView;
        }
    }
    //Switches the shoulders of the player
    void SwitchShoulders()
    {
        switch (shoulder)
        {
            case Shoulder.left:
                shoulder = Shoulder.right;
                break;
            case Shoulder.right:
                shoulder = Shoulder.left;
                break;
        }
    }
}
