using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance = 6f;
    public float height = 3f;
    public float turnSmooth = 6f;
    public float smooth = 8f;
    public float lookHeight = 0.5f;

    private float yaw;

    void Start()
    {
        yaw = target != null ? target.eulerAngles.y : transform.eulerAngles.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, turnSmooth * Time.deltaTime);

        Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
        Vector3 wanted = target.position - rot * Vector3.forward * distance + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, wanted, smooth * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * lookHeight);
    }
}
