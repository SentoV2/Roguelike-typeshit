using System;
using UnityEngine;

public class AdminCommands : MonoBehaviour
{
    // Ändra lösenordet här innan du använder konsolen.
    private const string AdminPassword = "koolkool12";
    private const float KillRange = 100f;

    [SerializeField]
    GameObject respawnMeny;

    public static bool WeaponInstantKill { get; private set; }
    public static bool ConsoleOpen { get; private set; }
    public static bool AdminFlying { get; private set; }

    private HealthManager health;
    private Rigidbody body;
    private Rigidbody2D body2D;
    private CharacterController characterController;
    private bool originalUseGravity;
    private float originalGravityScale;
    private bool consoleOpen;
    private bool unlocked;
    private bool flying;
    private float flySpeed = 8f;
    private string passwordInput = "";
    private string commandInput = "";
    private string message = "Enter password.";
    private string healthStatus = "";

    private void Awake()
    {
        // Hämta komponenter från spelarobjektet.
        health = GetComponent<HealthManager>();
        body = GetComponent<Rigidbody>();
        body2D = GetComponent<Rigidbody2D>();
        characterController = GetComponent<CharacterController>();
        if (respawnMeny == null) respawnMeny = FindFirstObjectByType<RespawnMeny>().gameObject;
        if (body != null) originalUseGravity = body.useGravity;
        if (body2D != null) originalGravityScale = body2D.gravityScale;
    }

    private void Update()
    {
        // Öppna konsolen med Shift + Enter.
        if (!consoleOpen) CheckOpenShortcut();

        if (consoleOpen && Input.GetKeyDown(KeyCode.Escape))
            SetConsoleOpen(false);

        if (flying && !consoleOpen)
            MoveWhileFlying();
    }

    private void CheckOpenShortcut()
    {
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool enter = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        if (shift && enter) SetConsoleOpen(true);
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
        if (health != null)
        {
            health.HealthChanged -= OnHealthChanged;
            health.Died -= OnPlayerDied;
        }
        ConsoleOpen = false;
        AdminFlying = false;
        WeaponInstantKill = false;
    }

    private void OnDisable()
    {
        if (body != null) body.useGravity = originalUseGravity;
        if (body2D != null) body2D.gravityScale = originalGravityScale;
        flying = false;
        AdminFlying = false;
        consoleOpen = false;
        ConsoleOpen = false;
        WeaponInstantKill = false;
    }

    private void OnHealthChanged(HealthManager changedHealth)
    {
        UpdateHealthStatus(changedHealth);
    }

    private void OnPlayerDied(HealthManager deadHealth)
    {
        if (deadHealth == health) {
            message = "Your character died.";
            respawnMeny.SetActive(true);
        }
    }

    private void UpdateHealthStatus(HealthManager changedHealth)
    {
        healthStatus = "Health: " + changedHealth.CurrentHealth + "/" + changedHealth.MaxHealth;
    }

    private void MoveWhileFlying()
    {
        if (body2D != null)
        {
            Vector2 move2D = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (Input.GetKey(KeyCode.Space)) move2D.y += 1f;
            if (Input.GetKey(KeyCode.LeftControl)) move2D.y -= 1f;
            if (move2D.sqrMagnitude > 1f) move2D.Normalize();
            body2D.MovePosition(body2D.position + move2D * flySpeed * Time.deltaTime);
            return;
        }

        // Flytta i riktningen kameran tittar.
        Vector3 forward = Camera.main != null ? Camera.main.transform.forward : transform.forward;
        Vector3 right = Camera.main != null ? Camera.main.transform.right : transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal");
        if (Input.GetKey(KeyCode.Space)) move.y += 1f;
        if (Input.GetKey(KeyCode.LeftControl)) move.y -= 1f;
        if (move.sqrMagnitude > 1f) move.Normalize();
        move *= flySpeed * Time.deltaTime;

        if (characterController != null) characterController.Move(move);
        else if (body != null) body.MovePosition(body.position + move);
        else transform.position += move;
    }

    private void OnGUI()
    {
        // Visa bara den lilla konsolen när den är öppen.
        if (!consoleOpen) return;

        GUILayout.BeginArea(new Rect(20f, 20f, 330f, 150f), GUI.skin.box);
        GUILayout.Label(unlocked ? "Admin console (Esc to close)" : "Admin login");

        if (!unlocked)
        {
            passwordInput = GUILayout.PasswordField(passwordInput, '*');
            if (GUILayout.Button("Unlock")) Unlock();
        }
        else
        {
            commandInput = GUILayout.TextField(commandInput);
            if (GUILayout.Button("Run") ||
                (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
            {
                RunCommand(commandInput);
                commandInput = "";
                GUI.FocusControl("AdminCommand");
            }
        }

        GUILayout.Label(message);
        if (health != null) GUILayout.Label(healthStatus);
        GUILayout.EndArea();
    }

    private void Unlock()
    {
        // Kontrollera lösenordet och rensa rutan efteråt.
        unlocked = passwordInput == AdminPassword;
        passwordInput = "";
        message = unlocked ? "Access granted. Type help." : "Wrong password.";
    }

    private void SetConsoleOpen(bool open)
    {
        // Stäng konsolen och kräv lösenord nästa gång.
        consoleOpen = open;
        ConsoleOpen = open;
        if (!open)
        {
            unlocked = false;
            passwordInput = "";
        }
    }

    private void RunCommand(string text)
    {
        // Dela upp kommandot så att till exempel speed kan få ett värde.
        string[] parts = text.Trim().ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        switch (parts[0])
        {
            case "help":
                message = "fly, god, kill, heal, weaponkill, speed, banana, rickroll, troll";
                break;
            case "fly":
                flying = Toggle(parts, flying);
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
                AdminFlying = flying;
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
            case "banana":
                message = "Banana protocol activated. Nothing has changed.";
                break;
            case "rickroll":
                message = "Never gonna give you up! (Audio not included.)";
                break;
            case "troll":
                message = "Admin detected: 0% serious, 100% suspicious.";
                break;
            default:
                message = "Unknown command. Type help.";
                break;
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
        // Leta efter ett mål framför spelarens kamera.
        Camera view = Camera.main;
        if (view == null) { message = "No camera found."; return; }

        RaycastHit hit;
        if (Physics.Raycast(view.transform.position, view.transform.forward, out hit, KillRange))
        {
            Kill(hit.collider.GetComponentInParent<HealthManager>());
            return;
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

