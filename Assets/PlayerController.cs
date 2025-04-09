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

    private Vector2 inputVector = Vector2.zero;
    private bool isGrounded = false;
    private bool isRunning = false;
    private bool isCrouched = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        controls = new @MolanoRimbauArnau_M17UF4R1();
        controls.Player.SetCallbacks(this);
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
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer() //codi per moure el player
    {
        Vector3 movement = new Vector3(inputVector.x, 0, inputVector.y);

        // Ajustar la velocidad en función de si está agachado o no
        float currentSpeed = isCrouched ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);

        // Usamos la velocidad directamente para evitar problemas de aplicación de fuerza
        Vector3 moveVelocity = movement * currentSpeed;

        // Solo modificamos la velocidad en el plano horizontal (X y Z), manteniendo la Y intacta
        rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);

        // Debugging: Verifica las velocidades en cada frame
        Debug.Log($"Current Speed: {currentSpeed}, Velocity: {moveVelocity}, Input Vector: {inputVector}");
    }






    private void CheckGrounded()
    {
        float rayLength = 1.2f;
        float sphereCastHeight = 1.0f;
        Vector3 spherePosition = transform.position + Vector3.up * sphereCastHeight;

        isGrounded = Physics.SphereCast(spherePosition, 0.3f, Vector3.down, out _, rayLength, groundLayer);
        Debug.DrawRay(spherePosition, Vector3.down * rayLength, isGrounded ? Color.green : Color.red);

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
            if (animator != null)
                animator.SetTrigger("Jump");
        }
    }

    private void UpdateAnimation()
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



    // INPUTS
    public void OnMove(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
        Debug.Log($"Input Vector: {inputVector}");
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
        // Implementación de rotación de cámara (si aplica)
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        // Acción de ataque, si aplica
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
}
