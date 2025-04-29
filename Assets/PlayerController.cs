using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, @MolanoRimbauArnau_M17UF4R1.IPlayerActions
{
    private Rigidbody rb;
    private PlayerInput playerInput;
    private @MolanoRimbauArnau_M17UF4R1 controls;
    private Animator animator;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float walkSpeed = 1.0f;
    [SerializeField] private float runSpeed = 4.0f;
    [SerializeField] private float crouchSpeed = 4.0f;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float rotationSpeed = 100.0f; //velocitat de rotacio del jugador (per la camera)
    private float aimBlendSpeed = 5.0f; //velocitat d'animacio de la transicio entre el layer 0 i el layer 1 (aim)

    [SerializeField] private int upperBodyLayerIndex = 1; // Index del layer d'aim (0 es el base layer, 1 es el d'aim)

    private Vector2 inputVector = Vector2.zero;
    private Vector2 lookInput = Vector2.zero;
    private bool isGrounded = false;
    private bool isRunning = false;
    private bool isCrouched = false;
    private bool isAiming = true;
    private bool isShooting = false; // Variable para controlar el disparo


    [SerializeField] private CinemachineVirtualCamera thirdPersonCam;
    [SerializeField] private CinemachineVirtualCamera firstPersonCam;

    [SerializeField] private Transform activeCameraTransform;
    [SerializeField] private Transform firstPersonCamTransform;
    [SerializeField] private Transform target;
    [SerializeField] private float aimTargetDistance = 2f;
    [SerializeField] private float verticalAimRange = 1f; // Rango vertical del objetivo de apuntar

    void Awake()
    {

        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        controls = new @MolanoRimbauArnau_M17UF4R1();
        controls.Player.SetCallbacks(this);

        //bloquear el cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Update()
    {
        CheckGrounded();
        UpdateBaseLayerAnimation();
        UpdateJumpHeight();
        UpdateAimLayerAnimation();
        UpdateActiveCamera();
        RotatePlayerAndCamera(); // Rotar el jugador y la cámara
        UpdateAimTarget(); // Actualizar la posición del aimTarget

    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer() //codi per moure el player
    {
        Vector3 movement = new Vector3(inputVector.x, 0, inputVector.y);

        Vector3 camForward = activeCameraTransform.forward;
        Vector3 camRight = activeCameraTransform.right;
        camForward.y = 0; 
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * inputVector.y + camRight * movement.x;
        moveDirection.Normalize(); // Normalizar el vector de movimiento


        // Ajustar la velocidad en función de si está agachado o no
        float currentSpeed = isCrouched ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);

        // Usamos la velocidad directamente para evitar problemas de aplicación de fuerza
        Vector3 moveVelocity = moveDirection * currentSpeed;

        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

        // Debugging: Verifica las velocidades en cada frame
        //Debug.Log($"Current Speed: {currentSpeed}, Velocity: {moveVelocity}, Input Vector: {inputVector}");
    
        if(moveDirection != Vector3.zero && !isAiming)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }






    private void CheckGrounded()
    {
        float rayLength = 1.2f;
        float sphereCastHeight = 1.0f;
        Vector3 spherePosition = transform.position + Vector3.up * sphereCastHeight;

        isGrounded = Physics.SphereCast(spherePosition, 0.3f, Vector3.down, out _, rayLength, groundLayer);
        Debug.DrawRay(spherePosition, Vector3.down * rayLength, isGrounded ? Color.green : Color.red);

        if (isGrounded && animator != null)
        {
            animator.SetFloat("JumpHeight", 0f);
        }
        
        if (animator != null)
        {
            animator.SetBool("isGrounded", isGrounded);
        }
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void UpdateJumpHeight()
    {
        float jumpHeight = rb.velocity.y;

        if (jumpHeight > 0)
        {
            jumpHeight = Mathf.Clamp01(jumpHeight / jumpForce); //que no sigui mes gran que 1
        }

        if (!isGrounded && jumpHeight < 0f) //si esta caient
        {
            jumpHeight = Mathf.Clamp01(-jumpHeight / jumpForce); //normalitzem la altura del salt en el rang [0, 1] (que no sigui -1 perque faria la animacio de jumpdown)
        }
        else if (isGrounded)
        {
            //quan toca el terra, jumpheight es -1 per l'animacio de jumpdown
            jumpHeight = -1f;
        }
        if (!isGrounded && jumpHeight < 0.5f && jumpHeight > 0f)
        {
            jumpHeight = Mathf.Lerp(jumpHeight, 0f, Time.deltaTime * 5); // Suaviza la caída
        }

        if (animator != null)
        {
            animator.SetFloat("JumpHeight", jumpHeight, 0.1f, Time.deltaTime);
        }
    }

    private void UpdateBaseLayerAnimation() //actualitza l'animacio del base layer
    {
        float targetVelocity = 0f;

        if (inputVector != Vector2.zero)
        {
            if (isCrouched)
                targetVelocity = 0.5f; // caminar agachado
            else
                targetVelocity = isRunning ? 1f : 0.5f; // caminar o correr normal
        }

        if (animator != null)
        {
            animator.SetFloat("Velocity", targetVelocity, 0.1f, Time.deltaTime);
        }
    }

    private void UpdateAimLayerAnimation()
    {
        if(animator != null)
        {
            float aimWeight = isAiming ? 1f : 1f; //es el weight del layer d'aim
            float currentWeight = animator.GetLayerWeight(upperBodyLayerIndex); //layer 1 es el d'aim
            float newWeight = Mathf.Lerp(currentWeight, aimWeight, Time.deltaTime * aimBlendSpeed); //fa la transicio entre el layer 0 i el layer 1
            animator.SetLayerWeight(upperBodyLayerIndex, newWeight); //actualitza el weight del layer d'aim
        }
    }

    private void UpdateActiveCamera()
    {
        if (isAiming)
        {
            activeCameraTransform = firstPersonCam.transform; //activa la camara de 1a persona
        }
        else
        {
            activeCameraTransform = thirdPersonCam.transform; //activa la camara de 3a persona
        }
    }

    private float cameraPitch = 0f; // rotación vertical (pitch)

    private void RotatePlayerAndCamera()
    {
        float mouseX = lookInput.x * rotationSpeed * Time.deltaTime;
        float mouseY = lookInput.y * rotationSpeed * Time.deltaTime;

        // Rota el jugador horizontalmente
        transform.Rotate(Vector3.up * mouseX);

        // Rota la cámara verticalmente (con límite)
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        activeCameraTransform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
    }

    void UpdateAimTarget()
    {
        if (isAiming)
        {
            Vector3 forward = firstPersonCam.transform.forward;
            Vector3 up = firstPersonCam.transform.up;

            // Coloca el aimTarget delante de la cámara, con desplazamiento vertical según el pitch
            Vector3 targetPosition = firstPersonCam.transform.position + forward * aimTargetDistance;

            // Añadimos un offset vertical en función del pitch
            targetPosition += up * Mathf.Sin(cameraPitch * Mathf.Deg2Rad) * verticalAimRange;

            target.position = targetPosition;
        }
    }



    // INPUTS
    public void OnMove(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
        //Debug.Log($"Input Vector: {inputVector}");
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            Jump();
        }
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        
        if (context.performed)
        {
            isShooting = true; //activem el dispar
            if(animator != null)
            {
                animator.SetBool("Fire", isShooting);
            }
        }
        else if (context.canceled)
        {
            isShooting = false; //desactivem el dispar
            if (animator != null)
            {
                animator.SetBool("Fire", isShooting);
            }

        }
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isRunning = true;
        }
        else if (context.canceled)
        {
            isRunning = false;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isCrouched = true;
        }
        else if (context.canceled)
        {
            isCrouched = false;
        }

        if (animator != null)
        {
            Debug.Log("Crouched: " + isCrouched);
            animator.SetBool("Crouched", isCrouched);
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isAiming = true;
            firstPersonCam.Priority = 20; //activa la camara de 1a persona
        }
        else if (context.canceled)
        {
            isAiming = false;
            firstPersonCam.Priority = 5; //activa la camara de 1a persona
        }
    }
}
