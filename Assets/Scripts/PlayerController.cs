using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Animations.Rigging;
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
    [SerializeField] private float health = 100f; //salut del jugador
    private float aimBlendSpeed = 5.0f; //velocitat d'animacio de la transicio entre el layer 0 i el layer 1 (aim)

    [SerializeField] private int upperBodyLayerIndex = 1; // Index del layer d'aim (0 es el base layer, 1 es el d'aim)
    [SerializeField] private Rig rig;
    [SerializeField] private GameObject hat;
    [SerializeField] private ParticleSystem shootParticles;




    private Vector2 inputVector = Vector2.zero;
    private Vector2 lookInput = Vector2.zero;
    private bool isGrounded = false;
    private bool isRunning = false;
    private bool isCrouched = false;
    private bool isAiming = true;
    private bool isShooting = false; // Variable para controlar el disparo
    public bool isDancing = false; // Variable para controlar si el jugador está bailando
    public Transform shootSpawn;
    public GameObject bullet;


    [SerializeField] private CinemachineVirtualCamera thirdPersonCam;
    [SerializeField] private CinemachineVirtualCamera firstPersonCam;
    [SerializeField] private CinemachineVirtualCamera dancingCam;

    [SerializeField] private Transform activeCameraTransform;
    [SerializeField] private Transform firstPersonCamTransform;
    [SerializeField] private Transform target;
    [SerializeField] private float aimTargetDistance = 2f;
    [SerializeField] private float verticalAimRange = 1f; // Rango vertical del objetivo de apuntar

    [SerializeField] private float stepHeight = 0.4f; // altura máxima del escalón que puede subir
    [SerializeField] private float stepSmooth = 0.1f;  // suavidad de subida
    [SerializeField] private Transform stepRayOrigin;  // un punto cerca del pie para lanzar el raycast


    void Awake()
    {
        LoadPlayerData();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        controls = new @MolanoRimbauArnau_M17UF4R1();
        controls.Player.SetCallbacks(this);
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
        isAiming = true; 
        isAiming = false; //ho inicio i ho torno a parar ja que al afegir cinemachinecollider i posarli un lookAt a la thirdPersonCam, la rotacio en Y es bloqueja i aixi al fer la transicio entre una camara i una altra, recalcula i ja funciona
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
        StepClimb();
    }

    private void MovePlayer() //codi per moure el player
    {
        if (isDancing) return; // Si está bailando, no se mueve
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

    private void StepClimb()
    {
        Vector3 origin = stepRayOrigin.position;
        Vector3 forward = transform.forward;

        // RAY INFERIOR (para detectar si hay un obstáculo delante en la parte baja)
        Ray lowerRay = new Ray(origin, forward);
        RaycastHit lowerHit;

        if (Physics.Raycast(lowerRay, out lowerHit, 0.5f, groundLayer))
        {
            // RAY SUPERIOR (verifica si hay espacio libre encima del obstáculo)
            Ray upperRay = new Ray(origin + Vector3.up * stepHeight, forward);
            RaycastHit upperHit;

            if (!Physics.Raycast(upperRay, out upperHit, 0.5f, groundLayer))
            {
                // Si hay un escalón y hay espacio arriba, lo subimos
                rb.position += new Vector3(0f, stepSmooth, 0f);
            }
        }

        // Opcional: Debug rays
        Debug.DrawRay(origin, forward * 0.5f, Color.red); // ray inferior
        Debug.DrawRay(origin + Vector3.up * stepHeight, forward * 0.5f, Color.green); // ray superior
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

    // Disparar
    private void Shoot()
    {
        if (isShooting && isAiming)
        {
            shootParticles.Play();
            int layerMask = ~LayerMask.GetMask("Player");

            //calcular el punto medio de la camara de first person con un raycast
            Ray ray = new Ray(firstPersonCam.transform.position, firstPersonCam.transform.forward); //ray que apunta a la direcció de la camara
            RaycastHit hit;

            Vector3 targetPoint = Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) ? hit.point : ray.GetPoint(1000);

            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 2f);

            Vector3 direction = (targetPoint - shootSpawn.position).normalized;
            GameObject bulletInstance = Instantiate(bullet, shootSpawn.position, Quaternion.identity);

            bulletInstance.GetComponent<Rigidbody>().velocity = direction * 100f;
            Destroy(bulletInstance, 2f);
        }
    }

    public void Hurt(float damage)
    {
        if (damage < 0) damage = 0;
        health -= damage; 
    }

    public void ChangeRigValue(int value)
    {
        if (rig != null)
        {
            rig.weight = value;
        }
    }
    public void ChangeDancingValue()
    {
        isDancing = false;
    }

    public void SavePlayerData()
    {
        PlayerPrefs.SetFloat("PlayerPosX", transform.position.x);
        PlayerPrefs.SetFloat("PlayerPosY", transform.position.y);
        PlayerPrefs.SetFloat("PlayerPosZ", transform.position.z);

        PlayerPrefs.SetFloat("PlayerRotY", transform.eulerAngles.y);

        PlayerPrefs.SetInt("HasHat", hat.activeSelf ? 1 : 0);

        PlayerPrefs.Save();
    }

    public void LoadPlayerData()
    {
        if (PlayerPrefs.HasKey("PlayerPosX"))
        {
            float x = PlayerPrefs.GetFloat("PlayerPosX");
            float y = PlayerPrefs.GetFloat("PlayerPosY");
            float z = PlayerPrefs.GetFloat("PlayerPosZ");

            float rotY = PlayerPrefs.GetFloat("PlayerRotY");

            transform.position = new Vector3(x, y, z);
            transform.eulerAngles = new Vector3(0, rotY, 0);

            bool hasHat = PlayerPrefs.GetInt("HasHat") == 1;
            hat.SetActive(hasHat);

            Debug.Log("Datos del jugador cargados.");
        }
    }

    // INPUTS
    public void OnMove(InputAction.CallbackContext context)
    {
        if (isDancing) return;
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
        if (isDancing) return;
        if (context.performed)
        {
            isShooting = true; //activem el dispar
            if(animator != null)
            {
                animator.SetBool("Fire", isShooting);
            }
            Shoot(); // Disparar
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
        if (isDancing) return;
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
        if (isDancing) return;
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
        if (isDancing) return;
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

    public void OnBailaloRocky(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(rig!= null)
            {
                ChangeRigValue(0);
            }
            if (animator != null && !isDancing)
            {
                isAiming = false;
                dancingCam.Priority = 20; //activa la camara de 1a persona
                animator.SetTrigger("BailaloRocky");
                StartCoroutine(WaitForDanceToEnd());
            }
        }
    }

    private IEnumerator WaitForDanceToEnd()
    {
        isDancing = true;

        // Asegúrate de que entramos en el estado correcto
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("BailaloRocky"));
        Debug.Log("Animación BailaloRocky empezó");

        // Espera a que termine la animación y asegúrate de que estamos en el estado correcto
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("BailaloRocky"))
        {
            yield return null;
        }

        Debug.Log("Animación BailaloRocky terminada");

        // Aquí volvemos a asegurar que al final de la animación, desactivamos el estado de baile
        isDancing = false;
        dancingCam.Priority = 0;
        ChangeRigValue(1);
    }

    public void OnPickObject(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            int layerMask = ~LayerMask.GetMask("Player");
            Ray ray = new Ray(firstPersonCam.transform.position, firstPersonCam.transform.forward); //raycast que apunta a la direcció de la camara
            RaycastHit hit;

            Vector3 targetPoint = Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) ? hit.point : ray.GetPoint(2); //si hi ha un hit, agafa el punt on el hit, sino agafa el punt 2 unitats davant de la camara

            Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 2f);
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

            if (hit.collider.CompareTag("Hat"))
            {
                if (hat != null)
                {
                    hat.SetActive(true);
                    Destroy(hit.collider.gameObject);
                }
                else
                {
                    Debug.LogError("Hat no está asignado en el Inspector.");
                }
            }
            if (hit.collider.CompareTag("Save"))
            {
                SavePlayerData();
            }

            if (hit.collider.CompareTag("Door"))
            {
                Door door = hit.collider.GetComponent<Door>();
                if (door != null)
                {
                    door.Toggle();
                }
            }
        }


    }
}
