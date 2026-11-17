using UnityEngine;

public class SurvivalTimer : MonoBehaviour
{
    public Animator gateAnimator;

    float t;
    bool won;

    void Update()
    {
        if (won) return;
        t += Time.deltaTime;
        if (t < GameConfig.Instance.survivalGoal) return;

        won = true;
        Debug.Log("YOU WIN!");
        if (gateAnimator != null) gateAnimator.SetBool("Open", true);
    }
}
