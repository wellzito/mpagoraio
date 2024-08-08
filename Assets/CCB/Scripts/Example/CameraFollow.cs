using UnityEngine;

public class CameraFollow : MonoBehaviour

{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 locationOffset;
    public Vector3 rotationOffset;

    public bool useRotation = true;
    public Camera _camera;
    public Camera _cameraPrimary;

    private void Start()
    {
        _camera.enabled = false;
        _cameraPrimary.enabled = true;
    }
    private void Update()
    {
        if (target == null)
        {
            var list = FindObjectsOfType<PlayerMovement>();
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].HasInputAuthority)
                {
                    target = list[i].transform;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            _camera.enabled = !_camera.enabled;
            _cameraPrimary.enabled = !_camera.enabled;
        }
    }
    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + target.rotation * locationOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        if (!useRotation) return;
        Quaternion desiredrotation = target.rotation * Quaternion.Euler(rotationOffset);
        Quaternion smoothedrotation = Quaternion.Lerp(transform.rotation, desiredrotation, smoothSpeed);
        transform.rotation = smoothedrotation;
    }
}