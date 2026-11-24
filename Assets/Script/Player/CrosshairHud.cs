using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairHud : MonoBehaviour
{
    const float REF_WIDTH = 1920f;
    const float REF_HEIGHT = 1080f;
    const int SORT_ORDER = 50;
    const float LINE_LENGTH = 18f;
    const float LINE_THICKNESS = 4f;
    const float MIN_GAP = 6f;
    const float GAP_PER_DEGREE = 9f;
    const float AMMO_FONT = 44f;
    const float AMMO_BOTTOM = 40f;
    static readonly Vector2 AMMO_SIZE = new Vector2(400f, 70f);
    const float FLASH_ALPHA = 0.35f;
    const float FLASH_FADE_PER_SEC = 1f;
    static readonly Color CROSS_COLOR = new Color(0.3f, 1f, 0.3f, 0.95f);
    static readonly Color FLASH_COLOR = new Color(0.8f, 0f, 0f, 0f);

    Shooter shooter;
    Canvas canvas;
    RectTransform[] lines;
    TextMeshProUGUI ammoText;
    Image flash;
    int shownAmmo = int.MinValue;

    public static CrosshairHud Create(Shooter owner)
    {
        CrosshairHud hud = new GameObject("CrosshairHud").AddComponent<CrosshairHud>();
        hud.Build(owner);
        return hud;
    }

    void Build(Shooter owner)
    {
        shooter = owner;
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SORT_ORDER;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(REF_WIDTH, REF_HEIGHT);
        scaler.matchWidthOrHeight = 1f;
        gameObject.AddComponent<GraphicRaycaster>();

        flash = MakeImage("DamageFlash", FLASH_COLOR);
        RectTransform fr = flash.rectTransform;
        fr.anchorMin = Vector2.zero;
        fr.anchorMax = Vector2.one;
        fr.sizeDelta = Vector2.zero;

        lines = new RectTransform[4];
        for (int i = 0; i < lines.Length; i++)
        {
            bool vertical = i < 2;
            lines[i] = MakeImage("Cross" + i, CROSS_COLOR).rectTransform;
            lines[i].sizeDelta = vertical
                ? new Vector2(LINE_THICKNESS, LINE_LENGTH)
                : new Vector2(LINE_LENGTH, LINE_THICKNESS);
        }

        ammoText = new GameObject("Ammo", typeof(RectTransform)).AddComponent<TextMeshProUGUI>();
        ammoText.transform.SetParent(transform, false);
        ammoText.fontSize = AMMO_FONT;
        ammoText.alignment = TextAlignmentOptions.Bottom;
        RectTransform ar = ammoText.rectTransform;
        ar.anchorMin = ar.anchorMax = new Vector2(0.5f, 0f);
        ar.pivot = new Vector2(0.5f, 0f);
        ar.anchoredPosition = new Vector2(0f, AMMO_BOTTOM);
        ar.sizeDelta = AMMO_SIZE;
        ammoText.gameObject.AddComponent<Button>().onClick.AddListener(shooter.Reload);

        Health.Damaged += OnDamaged;
    }

    Image MakeImage(string name, Color color)
    {
        Image img = new GameObject(name, typeof(RectTransform)).AddComponent<Image>();
        img.transform.SetParent(transform, false);
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    void OnDestroy() => Health.Damaged -= OnDamaged;

    void OnDamaged() => flash.color = new Color(FLASH_COLOR.r, FLASH_COLOR.g, FLASH_COLOR.b, FLASH_ALPHA);

    void Update()
    {
        if (shooter == null) { Destroy(gameObject); return; }
        canvas.enabled = !PauseMenu.IsPaused;

        float gap = MIN_GAP + shooter.CurrentSpread * GAP_PER_DEGREE + LINE_LENGTH * 0.5f;
        lines[0].anchoredPosition = new Vector2(0f, gap);
        lines[1].anchoredPosition = new Vector2(0f, -gap);
        lines[2].anchoredPosition = new Vector2(gap, 0f);
        lines[3].anchoredPosition = new Vector2(-gap, 0f);

        int ammoKey = shooter.IsReloading ? -1 : shooter.Ammo;
        if (ammoKey != shownAmmo)
        {
            shownAmmo = ammoKey;
            ammoText.text = shooter.IsReloading ? "RELOADING..." : shooter.Ammo + " / " + shooter.MagazineSize;
        }

        Color c = flash.color;
        c.a = Mathf.MoveTowards(c.a, 0f, FLASH_FADE_PER_SEC * Time.deltaTime);
        flash.color = c;
    }
}
