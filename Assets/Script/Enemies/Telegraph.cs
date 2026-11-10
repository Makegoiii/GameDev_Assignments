using UnityEngine;

public class Telegraph : MonoBehaviour
{
    [SerializeField] Renderer body;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color warnColor = Color.red;
    [SerializeField] Color stunColor = new Color(0.4f, 0.6f, 1f);
    [SerializeField] float blinkSpeed = 14f;
    [SerializeField] AudioClip warnClip;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    MaterialPropertyBlock block;
    AudioSource src;
    bool blinking;

    void Awake()
    {
        block = new MaterialPropertyBlock();
        src = GetComponent<AudioSource>();
        SetColor(normalColor);
    }

    public void StartWarning()
    {
        blinking = true;
        if (src != null && warnClip != null) src.PlayOneShot(warnClip);
    }

    public void ShowStunned()
    {
        blinking = false;
        SetColor(stunColor);
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
        if (body == null) return;
        body.GetPropertyBlock(block);
        block.SetColor(BaseColorId, c);
        body.SetPropertyBlock(block);
    }
}
