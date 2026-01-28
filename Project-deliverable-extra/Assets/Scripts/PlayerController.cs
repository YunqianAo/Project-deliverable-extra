using System.Collections;
using System.Collections.Generic;
using _MessageType;
using TMPro;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // ===========================================================
    // Movement Settings
    // ===========================================================

    [Header("Movement Settings")]
    [SerializeField] private float playerSpeed = 2.0f;           // Player movement speed
    [SerializeField] private float gravityValue = -9.81f;        // Gravity applied to the player
    [SerializeField] private float rotationSpeed = 5f;           // Player rotation speed
    private Vector2 input = Vector2.zero;

    // ===========================================================
    // Dash Settings
    // ===========================================================

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5.0f;          // Dash distance
    [SerializeField] private float dashDuration = 0.2f;          // Dash duration
    [SerializeField] private float dashCooldown = 1.0f;          // Time between dashes
    private float dashTime = 0f;
    private bool isDashing = false;
    private bool canDash = true;
    private Vector3 dashDirection;

    // ===========================================================
    // Camera and Effects Settings
    // ===========================================================

    [Header("Camera and Effects Settings")]
    [SerializeField] private Cinemachine.CinemachineImpulseSource impulseSource; // Impulse for camera shake
    [SerializeField] private float shakeDuration = 0.2f;         // Shake duration
    [SerializeField] private float shakeMagnitude = 0.1f;        // Shake magnitude
    [SerializeField] private Transform cameras = null;           // Reference to the player's camera

    // ===========================================================
    // Gameplay Settings
    // ===========================================================

    [Header("Gameplay Settings")]
    [SerializeField] private float groundCheckDistance = 1f;      // Distance to check if the player is grounded
    [SerializeField] private LayerMask groundLayer;              // Ground layer for checking ground collisions

    // ===========================================================
    // Component References
    // ===========================================================

    [Header("Component References")]
    private CharacterController controller;
    private PlayerInput playerInput;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private Transform cameraTransform;
    private GameState gameState;

    // ===========================================================
    // Input Actions
    // ===========================================================

    [Header("Input Actions")]
    private InputAction moveAction;      // Player movement action
    private InputAction lookAction;      // Camera look action
    private InputAction shootAction;     // Shooting action
    private InputAction jumpAction;      // Jump action
    private InputAction dashAction;      // Dash action

    // ===========================================================
    // Player State
    // ===========================================================

    [Header("Player State")]
    [SerializeField] private GameObject screenPlayer = null;      // Reference to the player's screen (UI)
    private int playerId;                                         // Player unique ID
    public bool movementBlocked = false;

    [HideInInspector]public int health = 4; // Vida inicial del jugador
    [SerializeField] private GameObject Damage1;
    [SerializeField] private GameObject Damage2;
    [SerializeField] private GameObject Damage3;


    [SerializeField] private GameObject FireReticle; // Objeto del FireReticle
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject muzzleFlashPrefab; // Prefab del efecto visual
    [SerializeField] private GameObject hitVFXPrefab; // Prefab del efecto de impacto
    [SerializeField] private GameObject deadthTotemPrefab;

    [SerializeField] private Transform barrelTransform;
    [SerializeField] private Transform bulletParent;
    BulletManager bulletManager;
    private Color originalColor;
    private GameObject totemref = null;


    // animations
    public Animator animator;
    [Header("SFX")]
    [SerializeField] private AudioClip shootSound; // Clip de sonido del disparo
    private AudioSource audioSource;             // Componente de audio

    public bool isDead = false;


    private void Awake()
    {
        // animations
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        audioSource = GetComponent<AudioSource>(); // Inicializar el AudioSource

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        shootAction = playerInput.actions["Shoot"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];

        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Start()
    {
        bulletManager = FindObjectOfType<BulletManager>();
        playerInput = GetComponent<PlayerInput>();
        gameState = FindObjectOfType<GameState>();
        cameraTransform = cameras;

        // StartCoroutine(PingRoutine());

        StartCoroutine(AnimationSyncRoutine());

        if (FireReticle != null)
        {
            var image = FireReticle.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                originalColor = image.color; // Guardar el color original del componente Image
            }
            else
            {
                var renderer = FireReticle.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalColor = renderer.material.color; // Guardar el color original del material
                }
            }
        }

    }

    private void OnEnable()
    {
        shootAction.performed += _ => ShootGun();
    }

    private void OnDisable()
    {
        shootAction.performed -= _ => ShootGun();

    }


    public void Shoot( Vector3 hitPoint)
    {
        if (gameState.isGamePaused) return;

        // Instanciar el efecto visual del disparo con corrección de rotación
        if (muzzleFlashPrefab != null)
        {
            Quaternion correctedRotation = barrelTransform.rotation * Quaternion.Euler(0, 180, 0); // Corregir el giro 180° en Y
            GameObject muzzleFlash = GameObject.Instantiate(muzzleFlashPrefab, barrelTransform.position, correctedRotation, barrelTransform);
            Destroy(muzzleFlash, 0.5f); // Destruir el efecto después de 0.5 segundos
        }

        RaycastHit hit;
        GameObject bullet = GameObject.Instantiate(bulletPrefab, barrelTransform.position, Quaternion.identity, bulletParent);
        
        bulletController bulletControll = bullet.GetComponent<bulletController>();
        bulletControll.original = false;
        bulletControll.target = hitPoint;
        // Realiza el raycast desde la cámara hacia adelante

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity))
        {
            Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");
            bulletControll.hit = true;
            bulletControll.SetPlayerId(playerId);

            PlayerController otherPlayer = hit.collider.GetComponent<PlayerController>();
            if (otherPlayer != null && otherPlayer != this)
            {
                Debug.Log("Hit another player!");
                StartCoroutine(ChangeReticleColor(Color.red, 1f));
            }
            else
            {
                Debug.Log("Hit something else, but not a player.");
            }
        }
        else
        {
            bulletControll.hit = false;
            Debug.Log("No hit detected.");
        }
    }



    private void ShootGun()
    {
        if (movementBlocked || gameState.isGamePaused) return;


        // Reproducir el sonido del disparo
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        // Instanciar el efecto visual del disparo con corrección de rotación
        if (muzzleFlashPrefab != null)
        {
            Quaternion correctedRotation = barrelTransform.rotation * Quaternion.Euler(0, 180, 0); // Corregir el giro 180° en Y
            GameObject muzzleFlash = GameObject.Instantiate(muzzleFlashPrefab, barrelTransform.position, correctedRotation, barrelTransform);
            Destroy(muzzleFlash, 0.5f); // Destruir el efecto después de 0.5 segundos
        }

        RaycastHit hit;
        GameObject bullet = GameObject.Instantiate(bulletPrefab, barrelTransform.position, transform.rotation, bulletParent);
        
        bulletController bulletControll = bullet.GetComponent<bulletController>();
        bulletControll.original = true;
        // Realiza el raycast desde la cámara hacia adelante

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity))
        {
           
            bulletControll.target = hit.point;
            MessageManager.SendMessage(new Shoot(hit.point,GetPlayerId()));
            bulletControll.hit = true;

            PlayerController otherPlayer = hit.collider.GetComponent<PlayerController>();
            if (otherPlayer != null && otherPlayer != this)
            {
                Debug.Log("Hit another player!");
                bulletControll.SetPlayerId(playerId);
                StartCoroutine(ChangeReticleColor(Color.red, 1f));
            }
            else
            {
                Debug.Log("Hit something else, but not a player.");
            }
        }
        else
        {
            bulletControll.hit = false;
            Debug.Log("No hit detected.");
        }
    }


    private IEnumerator ChangeReticleColor(Color newColor, float duration)
    {
       
        if (FireReticle != null)
        {
            var image = FireReticle.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.color = newColor; // Cambiar el color
                yield return new WaitForSeconds(duration);
                image.color = originalColor; // Restaurar el color original
            }
            else
            {
                var renderer = FireReticle.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = newColor; // Cambiar el color
                    yield return new WaitForSeconds(duration);
                    renderer.material.color = originalColor; // Restaurar el color original
                }
            }
        }
    }

    void Update()
    {
        if (movementBlocked || gameState.isGamePaused) return;

        groundedPlayer = IsGrounded();
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 rawInput = moveAction.ReadValue<Vector2>();
        // Obtener la entrada del jugador
        float smoothSpeed = 10f; // Ajusta la velocidad del suavizado
        input.x = Mathf.Lerp(input.x, rawInput.x, Time.deltaTime * smoothSpeed);
        input.y = Mathf.Lerp(input.y, rawInput.y, Time.deltaTime * smoothSpeed);
        Vector3 move = new Vector3(input.x, 0, input.y);
        
        // Normalizar para mantener la coherencia en los valores de Blend Tree
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0;

        // Mover al jugador
        controller.Move(move * Time.deltaTime * playerSpeed);

        // Asignar valores al Animator para el Blend Tree
        animator.SetFloat("Horizontal", input.x);
        animator.SetFloat("Vertical", input.y);

        if (dashAction.triggered && canDash)
        {
            StartDash(move);
        }

        if (isDashing)
        {
            HandleDash();
        }

        // Rotación del jugador hacia la cámara
        AlignPlayerWithCamera();
    }

    void AlignPlayerWithCamera()
    {
        // Calcula la dirección hacia donde está mirando la cámara (sin componente Y)
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0; // Ignorar inclinaciones verticales de la cámara

        if (cameraForward.sqrMagnitude > 0.001f) // Asegúrate de que haya una dirección válida
        {
            // Calcula la rotación hacia la dirección de la cámara
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);

            // Verifica si la diferencia de rotación es significativa
            float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);
            if (angleDifference > 1f) // Ajusta el umbral si es necesario
            {
                // Rota suavemente hacia la rotación objetivo
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    public void BlockMovement()
    {
        movementBlocked = true;
        screenPlayer.SetActive(false);

    }

    void SetCameraPriority(GameObject player, int priority)
    {
        Cinemachine.CinemachineVirtualCamera virtualCamera = player.GetComponentInChildren<Cinemachine.CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            virtualCamera.Priority = priority;
        }
        else
        {
            Debug.LogWarning($"No se encontró una cámara virtual en el jugador {player.name}");
        }
    }

    public int GetPlayerId()
    {
        return playerId;
    }

    public void SetPlayerId(int id)
    {
        playerId = id;
    }

    public void Revive()
    {
        health = 4;
        if(totemref!= null)
        {
            Destroy(totemref);
        }
        Damage1.SetActive(false);
        Damage2.SetActive(false);
        Damage3.SetActive(false);
    }
    public void TakeDmg()
    {
        if (!movementBlocked)
        {
            impulseSource.GenerateImpulse();
            health -= 1;

            switch (health)
            {
                case 4:
                    Debug.Log("Player has full health. No Canvas active.");
                    break;
                case 3:
                    if (Damage1 != null) Damage1.SetActive(true);
                    Debug.Log("Player has been hit. Damage1 Canvas active.");
                    break;
                case 2:
                    if (Damage2 != null) Damage2.SetActive(true);
                    Debug.Log("Player has moderate health. Damage2 Canvas active.");
                    break;
                case 1:
                    if (Damage3 != null) Damage3.SetActive(true);
                    Debug.Log("Player is critically injured. Damage3 Canvas active.");
                    break;
                case 0:
                    // Notificar al GameState sobre la muerte del jugador
                    MessageManager.SendMessage(new KillMessage(playerId));
                   
                    Dead(); 

                    Debug.Log($"{gameObject.name} has been killed!");
                    break;
                default:
                    Debug.LogWarning("Invalid health value.");
                    break;
            }
        }
        Debug.Log($"{gameObject.name} has taken 1 damage!");
    }

    public void Dead()
    {
        GameObject totem = GameObject.Instantiate(deadthTotemPrefab,transform.position,transform.rotation);
        if (movementBlocked)
        {
           Revive script = totem.AddComponent<Revive>();
            script.player = gameObject;
        }
        isDead = true;
        totemref = totem;
        gameObject.SetActive(false);
        EnemyManager.instance.ResetIA();
    }


    private IEnumerator ShakeCamera()
    {
        Vector3 originalPosition = cameraTransform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;

            cameraTransform.localPosition = new Vector3(
                originalPosition.x + offsetX,
                originalPosition.y + offsetY,
                originalPosition.z
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset camera to its original position
        cameraTransform.localPosition = originalPosition;
    }

    private bool IsGrounded()
    {
        RaycastHit hit;
        return Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance, groundLayer);
    }

    private void StartDash(Vector3 direction)
    {
        // Comenzamos el dash
        isDashing = true;
        canDash = false; // No se puede hacer otro dash hasta que termine el cooldown
        dashTime = 0f; // Reiniciar el tiempo del dash

        // Si el jugador se está moviendo, el dash será en la dirección del movimiento
        

        // Si el jugador no se está moviendo, entonces el dash será hacia adelante (dirección de la cámara)
        if (direction.sqrMagnitude < 0.1f)
        {
            direction = cameraTransform.forward.normalized;
        }

        dashDirection = direction;

        // Inicia el cooldown del dash
        Invoke(nameof(ResetDashCooldown), dashCooldown);
    }

    private void HandleDash()
    {
        // Realizamos el dash
        if (dashTime < dashDuration)
        {
            controller.Move(dashDirection * dashDistance * Time.deltaTime);
            dashTime += Time.deltaTime;
        }
        else
        {
            // Después de que termine el dash, volvemos a permitir el movimiento normal
            isDashing = false;
        }
    }

    private void ResetDashCooldown()
    {
        canDash = true; // Ya podemos hacer otro dash después del cooldown
    }

    IEnumerator PingRoutine()
    {
        while (true)
        {
            // Crear un mensaje de tipo Ping con el timestamp actual


            MessageManager.SendMessage(new PingMessage(Time.time));

            yield return new WaitForSeconds(1.0f); // Intervalo entre pings
        }
    }

    private IEnumerator AnimationSyncRoutine()
    {
        while (true)
        {
            SendAnimationState();
            yield return new WaitForSeconds(0.05f); // Cada 50ms (20 Hz)
        }
    }

    private void SendAnimationState()
    {
        // Recoge parámetros necesarios del Animator
        float horizontal = animator.GetFloat("Horizontal");
        float vertical = animator.GetFloat("Vertical");
       
        // Crea un mensaje de animación
        AnimationStateMessage animationMessage = new AnimationStateMessage(playerId, horizontal, vertical);

        // Envía el mensaje usando tu sistema de mensajería
        MessageManager.SendMessage(animationMessage);
    }
}
