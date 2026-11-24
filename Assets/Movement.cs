using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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

    public float messageTime = 2f;
    const float MOVING_SPEED_PART = 0.3f;
    private Coroutine messageRoutine;
    private Rigidbody rb;
    private int coinsCollected = 0;
    private int totalCoins;
    private bool grounded = false;
    private float timeLeft;
    private bool gameOver = false;

    private Vector2 move;
    private Shooter shooter;
    private PlayerLook look;
    private PlayerStats stats;

    public int CoinsCollected => coinsCollected;
    public float TimeLeft => timeLeft;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        shooter = GetComponent<Shooter>();
        stats = GameConfig.Instance.player;
        look = GetComponent<PlayerLook>();
        if (look == null) look = gameObject.AddComponent<PlayerLook>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
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
    }

    public void OnMove(InputValue value) { }

    static Vector2 ReadMove()
    {
        Vector2 m = Vector2.zero;
        Keyboard k = Keyboard.current;
        if (k != null)
        {
            if (k.wKey.isPressed || k.upArrowKey.isPressed) m.y += 1f;
            if (k.sKey.isPressed || k.downArrowKey.isPressed) m.y -= 1f;
            if (k.dKey.isPressed || k.rightArrowKey.isPressed) m.x += 1f;
            if (k.aKey.isPressed || k.leftArrowKey.isPressed) m.x -= 1f;
        }
        foreach (Gamepad gp in Gamepad.all)
        {
            Vector2 stick = gp.leftStick.ReadValue();
            if (stick.sqrMagnitude > m.sqrMagnitude) m = stick;
        }
        return Vector2.ClampMagnitude(m, 1f);
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
            Sfx.Play(SoundEvent.Jump);
        }
    }

    public void OnFire() { }

    void Update()
    {
        if (gameOver || PauseMenu.IsPaused) return;

        move = ReadMove();
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
            Sfx.Play(SoundEvent.Lose);
            Invoke(nameof(Restart), stats.gameOverDelay);
        }
        timerText.text = "Время: " + Mathf.CeilToInt(timeLeft);

        bool fireHeld = false;
        foreach (Gamepad gp in Gamepad.all)
            if (gp.buttonWest.isPressed || gp.rightTrigger.isPressed) { fireHeld = true; break; }

        if (Keyboard.current != null && Keyboard.current.xKey.isPressed)
            fireHeld = true;

        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
            fireHeld = true;

        if (shooter != null)
        {
            shooter.SetMotion(flat.magnitude > stats.moveSpeed * MOVING_SPEED_PART, grounded);
            if (fireHeld) shooter.TryShoot();
        }

        if (transform.position.y < stats.fallKillY)
        {
            Sfx.Play(SoundEvent.Lose);
            Restart();
        }
    }

    void FixedUpdate()
    {
        if (gameOver) return;

        Quaternion body = Quaternion.Euler(0f, look.Yaw, 0f);
        rb.MoveRotation(body);

        Vector2 input = Vector2.ClampMagnitude(move, 1f);
        float forwardSpeed = input.y >= 0f ? stats.moveSpeed : stats.moveSpeed * stats.backwardSpeedFactor;
        Vector3 target = body * new Vector3(input.x * stats.moveSpeed * stats.strafeSpeedFactor, 0f, input.y * forwardSpeed);
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
            Sfx.Play(SoundEvent.Pickup);

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
        Sfx.Play(SoundEvent.PlayerDeath);
        Sfx.Play(SoundEvent.Lose);
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
