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
    private bool isGrounded = false;
    private bool isRunning = false;
    private bool isCrouched = false;
    private bool isAiming = false;
    private bool isShooting = false; // Variable para controlar el disparo


    [SerializeField] private CinemachineVirtualCamera thirdPersonCam;
    [SerializeField] private CinemachineVirtualCamera firstPersonCam;


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
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer() //codi per moure el player
    {
        Vector3 movement = new Vector3(inputVector.x, 0, inputVector.y);

        movement = transform.TransformDirection(movement); // Transformar la dirección de movimiento a espacio local

        // Ajustar la velocidad en función de si está agachado o no
        float currentSpeed = isCrouched ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);

        // Usamos la velocidad directamente para evitar problemas de aplicación de fuerza
        Vector3 moveVelocity = movement * currentSpeed;

        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

        // Debugging: Verifica las velocidades en cada frame
        //Debug.Log($"Current Speed: {currentSpeed}, Velocity: {moveVelocity}, Input Vector: {inputVector}");
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
            float aimWeight = isAiming ? 1f : 0f; //es el weight del layer d'aim
            float currentWeight = animator.GetLayerWeight(upperBodyLayerIndex); //layer 1 es el d'aim
            float newWeight = Mathf.Lerp(currentWeight, aimWeight, Time.deltaTime * aimBlendSpeed); //fa la transicio entre el layer 0 i el layer 1
            animator.SetLayerWeight(upperBodyLayerIndex, newWeight); //actualitza el weight del layer d'aim
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
        Vector2 lookInput = context.ReadValue<Vector2>();
        if (lookInput != Vector2.zero)
        {
            //rotar el jugador en 2 ejes
            float rotationX = lookInput.x * rotationSpeed * Time.deltaTime;
            float rotationY = lookInput.y * rotationSpeed * Time.deltaTime;

            transform.Rotate(rotationX,rotationY , 0); //rotar el jugador en el eix Y
            
        }
        
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
