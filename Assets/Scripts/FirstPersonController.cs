using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using Cinemachine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FirstPersonController : MonoBehaviour
{
    private Rigidbody rb;

    public CapsuleCollider FrictionlessCollider = null;

    public Camera playerCamera;
    public CinemachineVirtualCamera walkCamera;

    public bool invertCamera = false;
    public float maxLookAngle = 50f;
    public float initialAngle = 0;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    // Internal Variables
    public Image crosshairObject;
    private Vector3 noMovement = new Vector3(0, 0, 0);

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    // Internal Variables
    private bool isWalking = false;
    private float currentSpeed = 0f;

    public bool enableSprint = true;
    public float sprintSpeed = 7f;
    private float sprintFOVVelocity = 1;
    
    // Internal Variables
    private bool sprintButtonHeld = false;
    private bool isSprinting = false;
    private bool ignoreSprinting = false;
    private float timeStampOfStartSprint = 9999;
    private const float _timeSprintingForBeforePossiblePanting = 10;
    private const int _oneInThisNumberOddsPerFrameToStartPanting = 2000; //TODO: Will need to find a decent number for this. Can also make it not set by framerate eventually, but doesn't really matter.

    // Internal Variables
    private bool isGrounded = false;
    private bool isOnSlope = false;

    public float crouchHeight = .75f;
    public float speedReduction = .5f;
    public float overencumberedSpeedReduction = .8f;
    public float crouchTime = .5f;

    // Internal Variables
    private bool isCrouched = false;
    private Vector3 originalScale;
    private Vector3 startingScale;
    private bool crouchingInProgress = false;
    private float crouchTimer = 0;
    private float distanceToCheckForUncrouchable = 1f;

    public float distanceToInteractable = 1f;
    public LayerMask interactableMask; //TODO: For some reason this isn't working and have to do it manually / hardcoded later

    private float preventInteractionTimer = 0;
    private float timeToPreventInteraction = -1;

    private IInteractable currentInteraction = null;
    private Animator animator;
    private Transform meshTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        SetAngle(initialAngle);

        originalScale = transform.localScale;
        startingScale = originalScale;

        GameManager.instance.controls.PlayerControl.Crouch.started += OnCrouchToggle;
        GameManager.instance.controls.PlayerControl.Sprint.started += OnStartSprint;
        GameManager.instance.controls.PlayerControl.Sprint.canceled += OnEndSprint;
        GameManager.instance.controls.PlayerControl.Interact.performed += context => currentInteraction?.Interacted();

        animator = GetComponent<Animator>();

        //To keep player from being able to interact with something the player starts facing (door at train station in Level1V2) before player is teleported to checkpoint
        PreventInteractableFor(1);
    }

    private void OnEnable()
    {
        GameManager.instance.SwitchInput(GameManager.instance.controls.PlayerControl.Get());
    }

    private void OnStartSprint(InputAction.CallbackContext ctx)
    {
        if (isCrouched) if (!CanUnCrouch()) return;
        sprintButtonHeld = true;
        ignoreSprinting = false;
        startingScale = transform.localScale;
    }

    private void OnEndSprint(InputAction.CallbackContext ctx)
    {
        sprintButtonHeld = false;
    }

    private Coroutine transitionCameraRoutine = null;
    private bool alreadyDecreasing = false;
    private bool alreadyIncreasing = false;
    private void TransitionToSprintingCamera()
    {
        if (alreadyIncreasing) return;
        timeStampOfStartSprint = Time.time;
        alreadyDecreasing = false;
        alreadyIncreasing = true;
        if (transitionCameraRoutine != null) StopCoroutine(transitionCameraRoutine);
        transitionCameraRoutine = StartCoroutine(TransitionFOV(70));
    }

    public void TransitionToWalkingCamera()
    {
        if (alreadyDecreasing) return;
        alreadyDecreasing = true;
        alreadyIncreasing = false;
        if (transitionCameraRoutine != null) StopCoroutine(transitionCameraRoutine);
        transitionCameraRoutine = StartCoroutine(TransitionFOV(60));
    }

    private IEnumerator TransitionFOV(float target)
    {
        float startingFOV = walkCamera.m_Lens.FieldOfView;
        float timer = 0;
        while (timer < .4f)
        {
            walkCamera.m_Lens.FieldOfView = Mathf.Lerp(startingFOV, target, timer / .4f);
            timer += Time.deltaTime;
            yield return null;
        }
        walkCamera.m_Lens.FieldOfView = target;
        transitionCameraRoutine = null;
    }

    private void OnCrouchToggle(InputAction.CallbackContext ctx)
    {
        animator.SetBool("isCrouched", !isCrouched);

        if (isCrouched) OnEndCrouch(ctx);
        else OnStartCrouch(ctx);
    }

    private void OnStartCrouch(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale != 0 || !isGrounded)
        {
            if (sprintButtonHeld) ignoreSprinting = true;
            SetCrouch(true);
        }
    }
    
    private void OnEndCrouch(InputAction.CallbackContext ctx)
    {
        if (CanUnCrouch())    
        {
            SetCrouch(false);
        }
    }

    private void SetCrouch(bool state)
    {

        if (state)
        {
            if (!isGrounded) return;
            isSprinting = false;
            isCrouched = true;
            crouchingInProgress = true;
            crouchTimer = 0;
            startingScale = transform.localScale;
            if (isWalking)
            {
                StartPlayerMovementAudio();
            }
        }
        else
        {
            ignoreSprinting = false;
            isCrouched = false;
            crouchingInProgress = true;
            crouchTimer = 0;
            startingScale = transform.localScale;
            if (isWalking) StartPlayerMovementAudio();
        }
    }

    private bool CanUnCrouch()
    {
        return !Physics.Raycast(GameManager.instance.Player.PlayerHead.position, GameManager.instance.Player.PlayerHead.up, distanceToCheckForUncrouchable, LayerMask.GetMask("Obstacles"));
    }

    private void OnDisable()
    {
        GameManager.instance.SwitchInput(GameManager.instance.controls.None.Get());
    }

    void Start()
    {
        currentSpeed = walkSpeed;

        if(lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if(crosshair)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else
        {
            if (crosshairObject != null)
            crosshairObject.gameObject.SetActive(false);
        }
        
    }

    public void PreventInteractableFor(float time)
    {
        preventInteractionTimer = 0;
        timeToPreventInteraction = time;
    }

    private void Update()
    {
        #region Crouch

        if (GameManager.instance.Player.isSightJacking)
        {
            if (isCrouched)
            {
                ignoreSprinting = false;
                crouchingInProgress = true;
                crouchTimer = crouchTime;
            }
            //            isCrouched = false;
        }

        if (crouchingInProgress)
        {
            if (isCrouched)
            {
                crouchTimer += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startingScale, new Vector3(originalScale.x, crouchHeight, originalScale.z), crouchTimer / crouchTime);
                currentSpeed = walkSpeed * speedReduction;
                FrictionlessCollider.enabled = false;

                if (Math.Abs(transform.localScale.y - crouchHeight) <= .01)
                {
                    transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
                    crouchingInProgress = false;
                }

            }
            else
            {
                crouchTimer += Time.deltaTime;
                transform.localScale = Vector3.Lerp(startingScale, originalScale, crouchTimer / crouchTime);
                currentSpeed = walkSpeed;

                if (Math.Abs(transform.localScale.y - originalScale.y) <= .01)
                {
                    FrictionlessCollider.enabled = true;
                    transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
                    crouchingInProgress = false;
                }
            }
        }

        #endregion

        #region Interactable

        currentInteraction = null;

        if (!GameManager.instance.Player.isSightJacking && !GameManager.instance.GamePaused)
        {
            //Additional check added to be used to prevent interactions when player first gains control in a scene (or after ladder) to prevent interacting in places the player shouldn't be
            bool preventInteraction = false;
            if (timeToPreventInteraction > 0)
            {
                preventInteractionTimer += Time.deltaTime;
                if (preventInteractionTimer < timeToPreventInteraction) preventInteraction = true;
                else
                {
                    preventInteractionTimer = 0;
                    timeToPreventInteraction = -1;
                }
            }
            if (!preventInteraction)
            {
                RaycastHit interactableHit;
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out interactableHit, distanceToInteractable, LayerMask.GetMask("InteractableObject", "InteractablePassthrough"))) //TODO: Find out why assigning interactableMask in editor isn't working
                {
                    if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, Vector3.Distance(playerCamera.transform.position, interactableHit.point), LayerMask.GetMask("Obstacles")))
                    {
                        currentInteraction = interactableHit.transform.gameObject.GetComponent<IInteractable>();
                        if (currentInteraction == null) currentInteraction = interactableHit.transform.gameObject.GetComponentInParent<IInteractable>();
                        if (currentInteraction is PortraitInteractable && (PortraitManager.instance.GetPhase() == 1 || PortraitManager.instance.GetPhase() == 2)) currentInteraction = null;
                        if (currentInteraction != null)
                        {
                            GameManager.instance.SetInteractableReticle();
                        }
                    }
                }

                if (PortraitManager.instance.GetPhase() == 1)
                {
                    RaycastHit portraitHit;
                    bool staringAtPortrait = false;
                    if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out portraitHit, 15, LayerMask.GetMask("InteractableObject"))) //TODO: Find out why assigning interactableMask in editor isn't working
                    {
                        if (!Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, Vector3.Distance(playerCamera.transform.position, portraitHit.point), LayerMask.GetMask("Obstacles")))
                        {
                            if (portraitHit.transform.gameObject.GetComponent<PortraitInteractable>() != null)
                            {
                                staringAtPortrait = true;
                            }
                        }
                    }
                    PortraitManager.instance.StaringAtPortrait(staringAtPortrait);

                }
                if (currentInteraction == null)
                {
                    GameManager.instance.SetDefaultReticle();
                }
            }

            #endregion

            CheckGround();
        }
    }

    void FixedUpdate()
    {
        #region Movement

        if (!GameManager.instance.IsShitting && playerCanMove && !GameManager.instance.Player.isSightJacking)
        {
            Vector2 move = GameManager.instance.controls.PlayerControl.Move.ReadValue<Vector2>();

            //Check to prevent sliding on ramp
            if (move.magnitude == 0 && isGrounded && isOnSlope && rb.velocity.y <= 0) rb.useGravity = false;
            else rb.useGravity = true;

            // Calculate how fast we should be moving
            Vector3 targetVelocity = Vector3.ClampMagnitude(new Vector3(move.x, 0, move.y), 1);

            if (targetVelocity.x != 0 || targetVelocity.z != 0 && isGrounded)
            {
                if (!isWalking) StartPlayerMovementAudio();
                isWalking = true;
            }
            else
            {
                if (isWalking) StopPlayerMovementAudio();
                isWalking = false;
            }
            

            Vector3 velocity = rb.velocity;

            Vector2 horizontalVelocity = new Vector2(velocity.x, velocity.z);
            
            SetPlayerMovementVolume(targetVelocity.magnitude);

            // All movement calculations while sprint is active
            if(targetVelocity.z > 0 && enableSprint && !ignoreSprinting && sprintButtonHeld)
            {
                targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

                // Apply a force that attempts to reach our target velocity
                
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                // Player is only moving when valocity change != 0
                // Makes sure fov change only happens during movement
                if (velocityChange.x != 0 || velocityChange.z != 0)
                {
                    if (!isSprinting)
                    {
                        isSprinting = true;
                        if (isWalking) StartPlayerMovementAudio();
                    }

                    //If crouched when attempting to sprint, uncrouch
                    if(isCrouched)
                    {
                        crouchingInProgress = true;
                        isCrouched = false;
                        crouchTimer = 0;

                    }
                }

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
            // All movement calculations while walking
            else
            {
                if (isSprinting)
                {
                    isSprinting = false;
                    if (isWalking) StartPlayerMovementAudio();
                }

                if (enableSprint) targetVelocity = transform.TransformDirection(targetVelocity) * currentSpeed;
                else targetVelocity = transform.TransformDirection(targetVelocity) * currentSpeed * overencumberedSpeedReduction;

                // Apply a force that attempts to reach our target velocity


                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }

            if (isSprinting && horizontalVelocity.magnitude > sprintFOVVelocity)
            {
                TransitionToSprintingCamera();

                if (Time.time - timeStampOfStartSprint >= _timeSprintingForBeforePossiblePanting)
                {
                    if (UnityEngine.Random.Range(1, _oneInThisNumberOddsPerFrameToStartPanting) == 1)
                    {
                        KaedeVoiceManager.instance.TriggerPanting();
                        timeStampOfStartSprint = Time.time;
                    }
                }
            }
            else
            {
                TransitionToWalkingCamera();
            }

            //If player barely moving / running into object, don't play sound
            if (horizontalVelocity.magnitude <= .05f) StopPlayerMovementAudio();
            else if (!AudioManager.instance.IsPlayingMovementAudio()) StartPlayerMovementAudio();

            //Lock player onto slopes / prevent them from 'bouncing'
                if ((isOnSlope || rb.velocity.y > 0) && isGrounded && !isCrouched)
            {
                bool isSlightlyAboveGround = false;

                Vector3 origin = new Vector3(transform.position.x, transform.position.y + .1f, transform.position.z);
                Vector3 direction = transform.TransformDirection(Vector3.down);
                float distance = .3f;

                if (!Physics.Raycast(origin, direction, out RaycastHit hit, distance, ~LayerMask.GetMask("Player")))
                {
                    isSlightlyAboveGround = true;
                }

                if (isSlightlyAboveGround)
                {
                    rb.AddForce(new Vector3(0, -100));
                }

                float maxSpeed = 5.25f;

                if (rb.velocity.magnitude > maxSpeed)
                {
                    rb.velocity = rb.velocity.normalized * maxSpeed;
                }
            }

        }
        //Prevent sliding around when entering sightjack
        else if (GameManager.instance.Player.isSightJacking && isGrounded)
        {
            rb.velocity = noMovement;
        }

        #endregion
    }

    //For overencumbering player
    public void SetEnabledSprinting(bool val)
    {
        enableSprint = val;
        if (!val)
        {
            if (isSprinting)
            {
                isSprinting = false;
                StartPlayerMovementAudio();
            }
        }
    }

    public void SetAngle(float angle)
    {
        walkCamera.GetCinemachineComponent<CinemachinePOV>().ForceCameraPosition(Vector3.zero, Quaternion.Euler(0, angle, 0));
    }

    private void StartPlayerMovementAudio()
    {
        if (GameManager.instance.Player.isSightJacking) return;

        if (isSprinting) AudioManager.instance.PlayRunningAudio();
        else if (isCrouched) AudioManager.instance.PlayCrouchAudio();
        else AudioManager.instance.PlayWalkingAudio();
    }

    private void SetPlayerMovementVolume(float vol)
    {
        if (isSprinting) AudioManager.instance.SetRunningVolume(vol);
        else if (isCrouched) AudioManager.instance.SetCrouchVolume(vol);
        else AudioManager.instance.SetWalkingVolume(vol);
    }

    private void StopPlayerMovementAudio()
    {
        AudioManager.instance.StopPlayerMovementAudio();
    }

    // Sets isGrounded based on a raycast sent straigth down from the player object
    private void CheckGround()
    {
        isOnSlope = false;
        Vector3 origin = new Vector3(transform.position.x, transform.position.y + .1f, transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;
        Debug.DrawRay(origin, direction * distance, Color.red);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, ~LayerMask.GetMask("Player")))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = true;

            if (hit.normal != Vector3.up)
            {
                isOnSlope = true;
            }
        }
        else
        {
            isGrounded = false;
            if (isCrouched)
            {
                SetCrouch(false);
            }
        }
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }  

    public bool IsCrouched()
    {
        return isCrouched;
    }

    public float GetCrouchHeight()
    {
        return crouchHeight;
    }

    public float GetCrouchRatio()
    {
        return crouchTimer / crouchTimer;
    }
}



// Custom Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(FirstPersonController)), InitializeOnLoadAttribute]
    public class FirstPersonControllerEditor : Editor
    {
    FirstPersonController fpc;
    SerializedObject SerFPC;

    private void OnEnable()
    {
        fpc = (FirstPersonController)target;
        SerFPC = new SerializedObject(fpc);
        
    }

    public override void OnInspectorGUI()
    {
        SerFPC.Update();

        EditorGUILayout.Space();
        GUILayout.Label("Modular First Person Controller", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 16 });
        GUILayout.Label("By Jess Case", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
        GUILayout.Label("version 1.0", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
        EditorGUILayout.Space();

        #region Camera Setup

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Camera Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.playerCamera = (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera", "Camera attached to the controller."), fpc.playerCamera, typeof(Camera), true);
        fpc.walkCamera = (CinemachineVirtualCamera)EditorGUILayout.ObjectField(new GUIContent("Walk Camera", "Cinemachine camera configured for walking."), fpc.walkCamera, typeof(CinemachineVirtualCamera), true);
        
        fpc.invertCamera = EditorGUILayout.ToggleLeft(new GUIContent("Invert Camera Rotation", "Inverts the up and down movement of the camera."), fpc.invertCamera);
        fpc.maxLookAngle = EditorGUILayout.Slider(new GUIContent("Max Look Angle", "Determines the max and min angle the player camera is able to look."), fpc.maxLookAngle, 40, 90);
        fpc.initialAngle = EditorGUILayout.Slider(new GUIContent("Initial Angle", "The angle that player should face initially"), fpc.initialAngle, 0, 360);
        GUI.enabled = true;
        
        fpc.lockCursor = EditorGUILayout.ToggleLeft(new GUIContent("Lock and Hide Cursor", "Turns off the cursor visibility and locks it to the middle of the screen."), fpc.lockCursor);

        fpc.crosshair = EditorGUILayout.ToggleLeft(new GUIContent("Auto Crosshair", "Determines if the basic crosshair will be turned on, and sets is to the center of the screen."), fpc.crosshair);

        // Only displays crosshair options if crosshair is enabled
        if(fpc.crosshair) 
        { 
            EditorGUI.indentLevel++; 
            EditorGUILayout.BeginHorizontal(); 
            // EditorGUILayout.PrefixLabel(new GUIContent("Crosshair Image", "Sprite to use as the crosshair.")); 
            // fpc.crosshairImage = (Sprite)EditorGUILayout.ObjectField(fpc.crosshairImage, typeof(Sprite), false);
            EditorGUILayout.PrefixLabel(new GUIContent("Crosshair Object", "Image object to use as the crosshair.")); 
            fpc.crosshairObject = (Image)EditorGUILayout.ObjectField(fpc.crosshairObject, typeof(Image), true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            fpc.crosshairColor = EditorGUILayout.ColorField(new GUIContent("Crosshair Color", "Determines the color of the crosshair."), fpc.crosshairColor);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--; 
        }

        EditorGUILayout.Space();
        
        #endregion

        #region Movement Setup

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Movement Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.playerCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Player Movement", "Determines if the player is allowed to move."), fpc.playerCanMove);

        GUI.enabled = fpc.playerCanMove;
        fpc.walkSpeed = EditorGUILayout.Slider(new GUIContent("Walk Speed", "Determines how fast the player will move while walking."), fpc.walkSpeed, .1f, fpc.sprintSpeed);
        GUI.enabled = true;

        fpc.FrictionlessCollider = (CapsuleCollider)EditorGUILayout.ObjectField(fpc.FrictionlessCollider, typeof(CapsuleCollider), true);

        EditorGUILayout.Space();

        #region Sprint

        GUILayout.Label("Sprint", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpc.enableSprint = EditorGUILayout.ToggleLeft(new GUIContent("Enable Sprint", "Determines if the player is allowed to sprint."), fpc.enableSprint);

        GUI.enabled = fpc.enableSprint;
        fpc.sprintSpeed = EditorGUILayout.Slider(new GUIContent("Sprint Speed", "Determines how fast the player will move while sprinting."), fpc.sprintSpeed, fpc.walkSpeed, 20f);

        //GUI.enabled = !fpc.unlimitedSprint;
        //GUI.enabled = true;
        
        GUI.enabled = true;

        EditorGUILayout.Space();

        #endregion
        
        #region Crouch

        GUILayout.Label("Crouch", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpc.crouchHeight = EditorGUILayout.Slider(new GUIContent("Crouch Height", "Determines the y scale of the player object when crouched."), fpc.crouchHeight, .1f, 1);
        fpc.speedReduction = EditorGUILayout.Slider(new GUIContent("Speed Reduction", "Determines the percent 'Walk Speed' is reduced by. 1 being no reduction, and .5 being half."), fpc.speedReduction, .1f, 1);
        fpc.crouchTime = EditorGUILayout.Slider(new GUIContent("Crouch Time", "Amount of time to complete a crouch animation"), fpc.crouchTime, 0, 10);

        #endregion

        #endregion
        
        //Sets any changes from the prefab
        if (GUI.changed)
        {
            EditorUtility.SetDirty(fpc);
            Undo.RecordObject(fpc, "FPC Change");
            SerFPC.ApplyModifiedProperties();
        }
    }

}

#endif