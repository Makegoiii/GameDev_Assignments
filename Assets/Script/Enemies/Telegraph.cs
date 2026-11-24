using UnityEngine;

public class Telegraph : MonoBehaviour
{
    [SerializeField] Renderer[] bodies;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color warnColor = Color.red;
    [SerializeField] float blinkSpeed = 14f;
    [SerializeField] float warnSoundInterval = 1.5f;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    MaterialPropertyBlock block;
    bool blinking;
    float lastWarnSound = float.NegativeInfinity;

    void Awake()
    {
        block = new MaterialPropertyBlock();
        SetColor(normalColor);
    }

    public void StartWarning()
    {
        if (blinking) return;
        blinking = true;
        if (Time.time - lastWarnSound < warnSoundInterval) return;
        lastWarnSound = Time.time;
        Sfx.PlayAt(SoundEvent.EnemyWindUp, transform.position);
    }

    public void Clear()
    {
        blinking = false;
        SetColor(normalColor);
    }

    void Update()
    {
        if (!blinking) return;
        float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;
        SetColor(Color.Lerp(normalColor, warnColor, 0.4f + 0.6f * t));
    }

    void SetColor(Color c)
    {
        if (bodies == null) return;
        foreach (Renderer r in bodies)
        {
            if (r == null) continue;
            r.GetPropertyBlock(block);
            block.SetColor(BaseColorId, c);
            r.SetPropertyBlock(block);
        }
    }
}
