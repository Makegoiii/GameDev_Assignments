using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Movement : MonoBehaviour
{
    public Transform feet;
    public float groundRadius = 0.2f;
    public LayerMask groundMask;

    public Animator anim;
    public Animator gateAnimator;

    public TextMeshProUGUI coinText;
    public TextMeshProUGUI winPanel;
    public TextMeshProUGUI timerText;
    public AudioSource audioSource;
    public AudioClip coinSound;
    public AudioSource bgMusic;

    public float messageTime = 2f;
    private Coroutine messageRoutine;
    private Rigidbody rb;
    private int coinsCollected = 0;
    private int totalCoins;
    private bool grounded = false;
    private float timeLeft;
    private bool gameOver = false;

    private Vector2 move;
    private bool fireRequested;
    private Shooter shooter;
    private PlayerStats stats;

    public int CoinsCollected => coinsCollected;
    public float TimeLeft => timeLeft;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        shooter = GetComponent<Shooter>();
        stats = GameConfig.Instance.player;
    }

    void Start()
    {
        totalCoins = GameObject.FindGameObjectsWithTag("Coin").Length;
        Debug.Log("Coins in this level: " + totalCoins);

        LevelRunner runner = LevelRunner.Active;
        timeLeft = runner != null && runner.Level != null
            ? runner.Level.timeLimit
            : GameConfig.Instance.fallbackTimeLimit;
        winPanel.text = "";
        ShowCoins();

        SaveSystem.ApplyMusic(bgMusic);
    }

    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnJump()
    {
        if (gameOver || PauseMenu.IsPaused) return;

        if (grounded && rb.linearVelocity.y <= 0.1f)
        {
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;
            rb.AddForce(Vector3.up * stats.jumpImpulse, ForceMode.Impulse);
        }
    }

    public void OnFire()
    {
        if (PauseMenu.IsPaused) return;
        fireRequested = true;
    }

    Vector3 Facing()
    {
        Vector3 f = rb.rotation * Vector3.forward;
        f.y = 0f;
        return f.normalized;
    }

    void Update()
    {
        if (gameOver || PauseMenu.IsPaused) return;

        grounded = Physics.CheckSphere(feet.position, groundRadius, groundMask);

        Vector3 flat = rb.linearVelocity;
        flat.y = 0f;
        anim.SetFloat("Speed", flat.magnitude);
        anim.SetBool("Grounded", grounded);

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            EndGame("Время вышло!");
            Invoke(nameof(Restart), stats.gameOverDelay);
        }
        timerText.text = "Время: " + Mathf.CeilToInt(timeLeft);

        bool fireHeld = false;
        foreach (Gamepad gp in Gamepad.all)
            if (gp.buttonWest.isPressed) { fireHeld = true; break; }

        if (Keyboard.current != null && Keyboard.current.xKey.isPressed)
            fireHeld = true;

        bool pointerOnUI = EventSystem.current != null
                           && EventSystem.current.IsPointerOverGameObject();
        bool clickShot = fireRequested && !pointerOnUI;
        fireRequested = false;

        if (shooter != null && (fireHeld || clickShot))
            shooter.Shoot(Facing());

        if (transform.position.y < stats.fallKillY)
            Restart();
    }

    void FixedUpdate()
    {
        if (gameOver) return;

        float turn = move.x * stats.turnSpeed * Time.fixedDeltaTime;
        if (turn != 0f)
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));

        float forwardInput = Mathf.Clamp(move.y, -1f, 1f);
        float speed = forwardInput >= 0f ? stats.moveSpeed : stats.moveSpeed * stats.backwardSpeedFactor;
        Vector3 target = Facing() * forwardInput * speed;
        Vector3 current = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 horizontal = Vector3.Lerp(current, target, grounded ? 1f : stats.airControl);

        rb.linearVelocity = new Vector3(horizontal.x, rb.linearVelocity.y, horizontal.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameOver) return;

        if (other.CompareTag("Coin"))
        {
            Destroy(other.gameObject);
            coinsCollected++;
            ShowCoins();

            if (audioSource != null && coinSound != null)
                audioSource.PlayOneShot(coinSound);

            if (coinsCollected >= totalCoins && gateAnimator != null)
                gateAnimator.SetBool("Open", true);
        }

        if (other.CompareTag("Exit"))
        {
            if (coinsCollected >= totalCoins)
            {
                if (LevelRunner.Active != null) LevelRunner.Active.OnExitReached();
                else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                ShowMessage("Сначала соберите все монеты!");
            }
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
            GameOver();
    }

    void GameOver()
    {
        if (gameOver) return;

        gameOver = true;
        winPanel.text = "Game Over!";
        StopPlayer();
        Invoke(nameof(Restart), stats.gameOverDelay);
    }

    void EndGame(string message)
    {
        gameOver = true;
        winPanel.text = message;
        StopPlayer();
    }

    void StopPlayer()
    {
        rb.linearVelocity = Vector3.zero;
        anim.SetFloat("Speed", 0f);
        anim.SetBool("Grounded", true);
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void ShowCoins()
    {
        coinText.text = "Монеты: " + coinsCollected + " / " + totalCoins;
    }

    public void ShowMessage(string message)
    {
        if (gameOver) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(message));
    }

    public void ShowResult(string message)
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        EndGame(message);
    }

    System.Collections.IEnumerator MessageRoutine(string message)
    {
        winPanel.text = message;
        yield return new WaitForSeconds(messageTime);

        if (!gameOver)
            winPanel.text = "";
    }

    public void GoToMenu()
    {
        LevelFlow.ToMenu();
    }

    void OnDrawGizmosSelected()
    {
        if (feet == null) return;
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(feet.position, groundRadius);
    }
}
