using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    static readonly int SpeedId = Animator.StringToHash("Speed");
    static readonly int GroundedId = Animator.StringToHash("Grounded");

    [SerializeField] Animator anim;

    Vector3 lastPos;

    void Start()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        lastPos = transform.position;
        if (anim != null) anim.SetBool(GroundedId, true);
    }

    void Update()
    {
        if (anim == null || Time.deltaTime <= 0f) return;
        Vector3 delta = transform.position - lastPos;
        delta.y = 0f;
        lastPos = transform.position;
        anim.SetFloat(SpeedId, delta.magnitude / Time.deltaTime);
    }
}
