using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

[DefaultExecutionOrder(100)]
public class PlayerLook : MonoBehaviour
{
    const int NO_TOUCH = -1;
    const float FALLBACK_EYE_HEIGHT = 1.6f;

    public float Yaw { get; private set; }
    public float Pitch { get; private set; }
    public Camera Cam { get; private set; }

    PlayerStats stats;
    float eyeOffset;
    int lookTouchId = NO_TOUCH;
    bool mouseDragging;
    readonly List<RaycastResult> uiHits = new List<RaycastResult>();

    void Awake()
    {
        stats = GameConfig.Instance.player;
        Yaw = transform.eulerAngles.y;
    }

    void Start()
    {
        Yaw = transform.eulerAngles.y;
        Cam = PickCamera();
        if (Cam == null) { Debug.LogError("PlayerLook: no camera"); enabled = false; return; }

        Collider body = GetComponent<Collider>();
        eyeOffset = body != null
            ? body.bounds.max.y - transform.position.y - stats.eyeBelowTop
            : FALLBACK_EYE_HEIGHT;
    }

    Camera PickCamera()
    {
        Camera[] all = FindObjectsByType<Camera>();
        Camera chosen = null;
        foreach (Camera c in all)
            if (c.enabled && c.GetComponent<CameraFollow>() != null) { chosen = c; break; }
        if (chosen == null) chosen = Camera.main;
        foreach (Camera c in all)
        {
            CameraFollow follow = c.GetComponent<CameraFollow>();
            if (follow != null) follow.enabled = false;
            if (c == chosen) continue;
            c.enabled = false;
            AudioListener extraEar = c.GetComponent<AudioListener>();
            if (extraEar != null) extraEar.enabled = false;
        }
        return chosen;
    }

    void Update()
    {
        if (PauseMenu.IsPaused || Time.timeScale == 0f)
        {
            lookTouchId = NO_TOUCH;
            mouseDragging = false;
            return;
        }

        Vector2 d = ReadLookDegrees();
        Yaw += d.x;
        Pitch = Mathf.Clamp(Pitch - d.y, -stats.maxPitch, stats.maxPitch);
    }

    void LateUpdate()
    {
        if (Cam == null) return;
        Quaternion rot = Quaternion.Euler(Pitch, Yaw, 0f);
        Vector3 pivot = transform.position + Vector3.up * eyeOffset;
        Vector3 wanted = rot * new Vector3(stats.shoulderOffset, stats.shoulderHeight, -stats.cameraDistance);
        float dist = wanted.magnitude;

        if (Physics.SphereCast(pivot, stats.cameraCollisionRadius, wanted / dist, out RaycastHit hit, dist,
                               ~0, QueryTriggerInteraction.Ignore) && !hit.transform.IsChildOf(transform))
            dist = Mathf.Max(hit.distance - stats.cameraCollisionRadius, stats.cameraMinDistance);

        Cam.transform.SetPositionAndRotation(pivot + wanted.normalized * dist, rot);
    }

    Vector2 ReadLookDegrees()
    {
        Vector2 total = Vector2.zero;
        total += ReadMouseDrag() * stats.touchSensitivity;

        foreach (Gamepad gp in Gamepad.all)
            total += gp.rightStick.ReadValue() * stats.gamepadLookSpeed * Time.deltaTime;

        total += ReadTouchLook() * stats.touchSensitivity;
        return total;
    }

    Vector2 ReadMouseDrag()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return Vector2.zero;
        if (mouse.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = mouse.position.ReadValue();
            mouseDragging = pos.x > Screen.width * 0.5f && !PointerOverUi(pos);
        }
        if (!mouse.leftButton.isPressed) mouseDragging = false;
        return mouseDragging ? mouse.delta.ReadValue() : Vector2.zero;
    }

    Vector2 ReadTouchLook()
    {
        Touchscreen ts = Touchscreen.current;
        if (ts == null) return Vector2.zero;

        foreach (var touch in ts.touches)
        {
            int id = touch.touchId.ReadValue();
            if (lookTouchId == NO_TOUCH && touch.press.wasPressedThisFrame)
            {
                Vector2 pos = touch.position.ReadValue();
                if (pos.x > Screen.width * 0.5f && !PointerOverUi(pos)) lookTouchId = id;
            }
            if (id != lookTouchId) continue;
            if (!touch.press.isPressed) { lookTouchId = NO_TOUCH; return Vector2.zero; }
            return touch.delta.ReadValue();
        }
        return Vector2.zero;
    }

    bool PointerOverUi(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;
        uiHits.Clear();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = screenPos }, uiHits);
        foreach (RaycastResult hit in uiHits)
            if (hit.gameObject.GetComponentInParent<Selectable>() != null
                || hit.gameObject.GetComponentInParent<OnScreenControl>() != null)
                return true;
        return false;
    }

    public void AddRecoil(float up, float side)
    {
        Pitch = Mathf.Clamp(Pitch - up, -stats.maxPitch, stats.maxPitch);
        Yaw += side;
    }
}
