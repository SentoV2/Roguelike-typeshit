using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class AdminCommands : MonoBehaviour
{
    // Ändra lösenordet här i koden.
    private const string AdminPassword = "koolkool12";
    private const float KillRange = 100f;

    // DamageManager använder detta när vapendöd är på.
    public static bool WeaponInstantKill { get; private set; }

    private HealthManager health;
    private Rigidbody body;
    private Rigidbody2D body2D;
    private CharacterController characterController;
    private MonoBehaviour enemyMovement;
    private bool originalUseGravity;
    private float originalGravityScale;
    private bool originalEnemyMovementState;
    private bool consoleOpen;
    private bool unlocked;
    private bool flying;
    private float flySpeed = 8f;
    private string passwordInput = "";
    private string commandInput = "";
    private string message = "Enter password to continue.";
    private string healthStatus = "";

    private GUIStyle titleStyle;
    private GUIStyle labelStyle;
    private GUIStyle statusStyle;
    private GUIStyle buttonStyle;

    private void Awake()
    {
        health = GetComponent<HealthManager>();
        body = GetComponent<Rigidbody>();
        body2D = GetComponent<Rigidbody2D>();
        characterController = GetComponent<CharacterController>();
        if (body != null) originalUseGravity = body.useGravity;
        if (body2D != null) originalGravityScale = body2D.gravityScale;

        // Hitta rörelseskriptet utan att låsa andra projekt till dess klassfil.
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
        {
            if (script != this && script.GetType().Name == "EnemyMovment")
            {
                enemyMovement = script;
                originalEnemyMovementState = script.enabled;
                break;
            }
        }
    }

    private void Start()
    {
        // Koppla konsolen till spelarens hälsahändelser.
        if (health == null) return;
        health.HealthChanged += OnHealthChanged;
        health.Died += OnPlayerDied;
        UpdateHealthStatus(health);
    }

    private void OnDestroy()
    {
        if (health == null) return;
        health.HealthChanged -= OnHealthChanged;
        health.Died -= OnPlayerDied;
    }

    private void Update()
    {
        // Öppna med Shift + Enter, med stöd för båda Input System-versionerna.
        bool shift = false;
        bool enter = false;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            shift = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            enter = Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        shift |= Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        enter |= Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
        if (!consoleOpen && shift && enter) SetConsoleOpen(true);

        if (flying && !consoleOpen && body2D == null)
            MoveWhileFlying();
    }

    private void FixedUpdate()
    {
        if (!flying || consoleOpen || body2D == null) return;

        Vector2 move = new Vector2(ReadHorizontal(), ReadVertical());
        if (move.sqrMagnitude > 1f) move.Normalize();
        body2D.linearVelocity = move * flySpeed;
    }

    private float ReadHorizontal()
    {
        float value = 0f;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
            value = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f)
                  - (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (value == 0f) value = Input.GetAxisRaw("Horizontal");
#endif
        return value;
    }

    private float ReadVertical()
    {
        float value = 0f;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
            value = (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed || Keyboard.current.spaceKey.isPressed ? 1f : 0f)
                  - (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed || Keyboard.current.leftCtrlKey.isPressed ? 1f : 0f);
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (value == 0f) value = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(KeyCode.Space)) value += 1f;
        if (Input.GetKey(KeyCode.LeftControl)) value -= 1f;
#endif
        return value;
    }

    private void MoveWhileFlying()
    {
        float horizontal = ReadHorizontal();
        float vertical = ReadVertical();
        Vector3 move;

        if (characterController != null || body != null)
        {
            Vector3 forward = Camera.main != null ? Camera.main.transform.forward : transform.forward;
            Vector3 right = Camera.main != null ? Camera.main.transform.right : transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            float up = ReadFlyUpDown();
            move = right * horizontal + forward * vertical + Vector3.up * up;
        }
        else
        {
            move = new Vector3(horizontal, vertical, 0f);
        }

        if (move.sqrMagnitude > 1f) move.Normalize();
        move *= flySpeed * Time.deltaTime;

        if (characterController != null) characterController.Move(move);
        else if (body != null) body.MovePosition(body.position + move);
        else transform.position += move;
    }

    private float ReadFlyUpDown()
    {
        float value = 0f;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
            value = (Keyboard.current.spaceKey.isPressed ? 1f : 0f)
                  - (Keyboard.current.leftCtrlKey.isPressed ? 1f : 0f);
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.Space)) value = 1f;
        if (Input.GetKey(KeyCode.LeftControl)) value = -1f;
#endif
        return value;
    }

    private void OnHealthChanged(HealthManager changedHealth)
    {
        UpdateHealthStatus(changedHealth);
    }

    private void OnPlayerDied(HealthManager deadHealth)
    {
        if (deadHealth == health) message = "Your character died.";
    }

    private void UpdateHealthStatus(HealthManager changedHealth)
    {
        healthStatus = "Health  " + changedHealth.CurrentHealth + " / " + changedHealth.MaxHealth;
    }

    private void OnGUI()
    {
        Event keyEvent = Event.current;
        if (!consoleOpen && keyEvent.type == EventType.KeyDown && keyEvent.shift &&
            (keyEvent.keyCode == KeyCode.Return || keyEvent.keyCode == KeyCode.KeypadEnter))
        {
            SetConsoleOpen(true);
            keyEvent.Use();
        }

        if (consoleOpen && keyEvent.type == EventType.KeyDown && keyEvent.keyCode == KeyCode.Escape)
        {
            SetConsoleOpen(false);
            keyEvent.Use();
        }

        if (!consoleOpen) return;
        CreateStyles();

        float panelHeight = unlocked ? 246f : 216f;
        Rect panel = new Rect(24f, 24f, 370f, panelHeight);
        Color previousColor = GUI.color;
        GUI.color = new Color(0.10f, 0.12f, 0.16f, 0.98f);
        GUI.Box(panel, GUIContent.none);
        GUI.color = previousColor;

        GUILayout.BeginArea(new Rect(panel.x + 16f, panel.y + 12f, panel.width - 32f, panel.height - 20f));
        GUILayout.Label("ADMIN CONSOLE", titleStyle);
        GUILayout.Label(unlocked ? "ACCESS GRANTED" : "PASSWORD REQUIRED", statusStyle);

        if (!unlocked)
        {
            GUILayout.Space(8f);
            passwordInput = GUILayout.PasswordField(passwordInput, '*');
            GUILayout.Space(6f);
            if (GUILayout.Button("Unlock", buttonStyle) || IsEnterPressed(keyEvent)) Unlock();
        }
        else
        {
            GUILayout.Space(6f);
            GUILayout.BeginHorizontal();
            GUI.SetNextControlName("AdminCommandInput");
            commandInput = GUILayout.TextField(commandInput);
            bool run = GUILayout.Button("Run", buttonStyle, GUILayout.Width(68f));
            GUILayout.EndHorizontal();

            if (run || IsEnterPressed(keyEvent))
            {
                RunCommand(commandInput);
                commandInput = "";
                GUI.FocusControl("AdminCommandInput");
            }

            GUILayout.Label("help  fly  god  kill  heal  weaponkill  speed", labelStyle);
            GUILayout.Label("banana  rickroll  troll", labelStyle);
        }

        GUILayout.Space(4f);
        GUILayout.Label(message, labelStyle);
        if (health != null) GUILayout.Label(healthStatus, statusStyle);
        GUILayout.EndArea();
    }

    private bool IsEnterPressed(Event keyEvent)
    {
        return keyEvent.type == EventType.KeyDown &&
               (keyEvent.keyCode == KeyCode.Return || keyEvent.keyCode == KeyCode.KeypadEnter);
    }

    private void CreateStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(0.78f, 0.82f, 0.88f) } };
        statusStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.38f, 0.86f, 0.68f) } };
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 12, fontStyle = FontStyle.Bold, fixedHeight = 28f };
    }

    private void Unlock()
    {
        // Kontrollera lösenordet och rensa rutan efteråt.
        unlocked = passwordInput == AdminPassword;
        passwordInput = "";
        message = unlocked ? "Access granted. Type a command." : "Wrong password.";
    }

    private void SetConsoleOpen(bool open)
    {
        // Stäng konsolen och kräv lösenord nästa gång.
        consoleOpen = open;
        if (!open)
        {
            unlocked = false;
            passwordInput = "";
        }
    }

    private void SetFlying(bool value)
    {
        flying = value;
        if (body != null)
        {
            body.useGravity = flying ? false : originalUseGravity;
            if (flying) body.linearVelocity = Vector3.zero;
        }
        if (body2D != null)
        {
            body2D.gravityScale = flying ? 0f : originalGravityScale;
            if (flying) body2D.linearVelocity = Vector2.zero;
        }
        if (enemyMovement != null)
        {
            if (flying)
            {
                originalEnemyMovementState = enemyMovement.enabled;
                enemyMovement.enabled = false;
            }
            else enemyMovement.enabled = originalEnemyMovementState;
        }
    }

    private void RunCommand(string text)
    {
        // Dela upp kommandot så att till exempel speed kan få ett värde.
        string[] parts = text.Trim().ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        switch (parts[0])
        {
            case "help": message = "fly, god, kill, heal, weaponkill, speed, banana, rickroll, troll"; break;
            case "fly":
                SetFlying(Toggle(parts, flying));
                message = "Fly " + (flying ? "on" : "off");
                break;
            case "god":
                if (health == null) { message = "Add HealthManager to this object."; break; }
                health.SetInvulnerable(Toggle(parts, health.IsInvulnerable));
                message = "God mode " + (health.IsInvulnerable ? "on" : "off");
                break;
            case "heal":
                if (health == null) { message = "No HealthManager found."; break; }
                health.ResetHealth();
                message = "Health restored.";
                break;
            case "kill":
                if (parts.Length > 1 && parts[1] == "self") Kill(health);
                else KillTargetInFront();
                break;
            case "weaponkill":
                WeaponInstantKill = Toggle(parts, WeaponInstantKill);
                message = "Weapon instant kill " + (WeaponInstantKill ? "on" : "off");
                break;
            case "speed":
                float newSpeed;
                if (parts.Length > 1 && float.TryParse(parts[1], out newSpeed) && newSpeed > 0f)
                {
                    flySpeed = newSpeed;
                    message = "Fly speed set to " + flySpeed;
                }
                else message = "Use: speed <positive number>";
                break;
            case "banana": message = "Banana protocol activated. Nothing has changed."; break;
            case "rickroll": message = "Never gonna give you up! (Audio not included.)"; break;
            case "troll": message = "Admin detected: 0% serious, 100% suspicious."; break;
            default: message = "Unknown command. Type help."; break;
        }
    }

    private bool Toggle(string[] parts, bool current)
    {
        // Godta on, off eller toggle på kommandon som växlar läge.
        if (parts.Length < 2 || parts[1] == "toggle") return !current;
        return parts[1] == "on" || parts[1] == "true";
    }

    private void KillTargetInFront()
    {
        // Leta efter ett mål mitt i kamerabilden.
        Camera view = Camera.main;
        if (view == null) { message = "No camera found."; return; }

        Ray ray = view.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));
        if (body2D != null)
        {
            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, KillRange);
            if (hit2D.collider != null)
            {
                Kill(hit2D.collider.GetComponentInParent<HealthManager>());
                return;
            }
        }
        else
        {
            RaycastHit hit3D;
            if (Physics.Raycast(ray, out hit3D, KillRange))
            {
                Kill(hit3D.collider.GetComponentInParent<HealthManager>());
                return;
            }
        }
        message = "No target in view.";
    }

    private void Kill(HealthManager target)
    {
        // Döda målet direkt via dess HealthManager.
        if (target == null) { message = "No HealthManager found."; return; }
        target.Kill();
        message = "Target killed.";
    }
}