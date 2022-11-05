using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    #region Configurable/Editor Values
    //Rotation Routine Behavior configurable values
    [Tooltip("Whether or not this enemy should wait for a specific trigger before beginning its behavior")]
    [SerializeField] private bool waitForTrigger = false;
    [Tooltip("Whether or not this enemy should rotate when in Search state")]
    [SerializeField] private bool rotatingEnemy = true;
    [Tooltip("If true, the enemy rotates back after rotating")]
    [SerializeField] private bool alternateRotateBack = false;
    [Tooltip("If this enemy should raise and lower (without following patrol)")]
    [SerializeField] private bool raiseLowerEnemy = false;
    [Tooltip("If this enemy should bob up and down when not chasing (setup values are constants in script)")]
    [SerializeField] private bool bounceEnemy = false;
    [Tooltip("Amount of time before enemy starts bouncing (to prevent all bouncing at same time)")]
    [SerializeField] private float initBounceDelay = 0;
    [Tooltip("If this enemy should go other direction when bouncing")]
    [SerializeField] private bool reverseBounce = false;
    [Tooltip("How long the enemy should pause after a full rotation")]
    public bool SleepingEnemy = false;
    [Tooltip("If this is an angler fish enemy (affects its collision detection)")]
    public bool AnglerEnemy = false;
    [Tooltip("How long enemy waits before rotating")]
    [SerializeField] private float timeBetweenRotations = 5f;
    [Tooltip("How quickly the enemy should rotate")]
    [SerializeField] private float rotationRate = 50;
    [Tooltip("How far the enemy should rotate before pausing")]
    [SerializeField] private float totalRotation = 180;
    [Tooltip("How long the enemy should move backward for when starting its 'SearchLastKnownPosition' state")]
    [SerializeField] private float searchingAreaBackUpMaxTime = .5f;
    [Tooltip("How long the enemy takes to make its x and z rotation return to 0 when transitioning to 'SearchLastKnownPosition' state")]
    [SerializeField] private float reBalanceRotationTime = .5f;

    //Patrol Routine Behavior configurable values
    [Tooltip("The patrol route that this enemy should follow. If no patrol, this should be empty.")]
    [SerializeField] private GameObject patrolRoute = null;
    [Tooltip("How quickly the enemy moves while on patrol")]
    [SerializeField] private float patrolSpeed = 2.5f;
    [Tooltip("Whether or not enemy should attempt to avoid obstacles on patrol")]
    [SerializeField] private bool patrolObstacleAvoidance = false;
    [Tooltip("If patrolCanGetStuck then this should probably be on in most cases. It is used to allow a stuck returning enemy to find its patrol again. Set as variable so tower enemies can not do it.")]
    [SerializeField] private bool handleStuckOnReturnEnemy = false;
    [Tooltip("Whether or not enemy can get stuck on patrol AND when returning (if there's dynamic obstacles in the way)")]
    [SerializeField] private bool patrolCanGetStuck = false;
    [Tooltip("Whether or not enemy should backup when it is stuck too close / inside an object (should only be off for things where you want to force them into an object like gong enemies)")]
    [SerializeField] private bool backupWhenStuck = true;

    //Angler Patrol Configurable Values
    [Tooltip("The amount of time that must pass before the enemy can turn around to switch patrol route if it senses player near")]
    [SerializeField] private float cooldownForTurnAround = 30;
    [Tooltip("What the cooldowntimer should start at (if immediately off CD put as same time as cooldown or -1. Used if you want enemy to not be able to turn around at first)")]
    [SerializeField] private float initialCooldownTimerValue = -1;
    [Tooltip("The distance the player must be within for the enemy to sense them and turn around their patrol")]
    [SerializeField] private float distanceThresholdForTurnAround = 30;

    //Dynamic Speed Configurable Variables
    [Tooltip("Used for enemies that you want to go faster when far away and go slower when nearby.. for chasing enemies that you don't want to too easily/unfairly catch player")]
    [SerializeField] private bool dynamicSpeedEnabled = false;
    [Tooltip("Distance at which the monster should slow down")]
    [SerializeField] private float distanceForSlowDown = 5;
    [Tooltip("Distance at which the monster should speed up")]
    [SerializeField] private float distanceForSpeedUp = 20;
    [Tooltip("How much the enemy should speed up / slow down by when near")]
    [SerializeField] private float dynamicSpeedChange = 1.5f;

    //Raise/Lower behavior configurable values
    [Tooltip("How quickly the enemy raises/lowers")]
    [SerializeField] private float raiseLowerSpeed = 2.5f;
    [Tooltip("How high the enemy should go before lowering")]
    [SerializeField] private float maxHeight = 0;
    [Tooltip("How low the enemy should go before rising")]
    [SerializeField] private float minHeight = 0;

    //Chase and returning behavior configurable values
    [Tooltip("How quickly the enemy should pursue when in chase mode (currently returns to position at same speed)")]
    [SerializeField] private float chaseSpeed = 4.5f;
    [Tooltip("How much the chase speed increases to over time (set to 0 if enemy shouldn't speed up)")]
    [SerializeField] private float maxBonusSpeed = 3f;
    [Tooltip("How long the enemy should go at normal speed before starting to speed up")]
    [SerializeField] private float delayBeforeBonusSpeed = 3f;
    [Tooltip("The amount of time it takes after the delay to achieve the full bonus speed")]
    [SerializeField] private float timeTakenForFullBonusSpeed = 15f;
    [Tooltip("How close the enemy needs to be to its target (last known position or starting position) before it changes states")]
    [SerializeField] private float distanceThreshold = .05f;
    [Tooltip("How close the enemy's rotation needs to be to its original before it moves to next state")]
    [SerializeField] private float rotationThreshold = .01f;
    [Tooltip("How far up the enemy should go when calculating if it can just fly up to create a return path")]
    [SerializeField] private float returningIntoAirIntervalDistance = 5f;
    [Tooltip("How many times it should go up returningIntoAirIntervalDistance to determine if there's a clear path")]
    [SerializeField] private int returningMaxAirAttempts = 10;
    [Tooltip("How often the enemy should remember a previous position for planning its return route")]
    [SerializeField] private float pathTakenPollingInterval = .1f;
    [Tooltip("If the enemy is chasing the player and is within this distance, it will instantly turn to them instead of over time (to prevent player being able to run behind enemy to hide)")]
    [SerializeField] private float chaseQuickTurnDistanceThreshold = 3f;
    [Tooltip("How quickly the enemy turns towards its target, should be rather high")]
    [SerializeField] private float chaseRotateTowardsPlayerRate = 300;
    [Tooltip("How quickly the enemy turns while returning to its starting position")]
    [SerializeField] private float returningRotateRate = 150;
    [Tooltip("(Debug) Whether or not enemy should leave cubes to denote its intended return path")]
    [SerializeField] private bool debugPathingCubeEnabled = false;
    [Tooltip("(Debug) Prefab to create instances of debug cubes from for pathing")]
    [SerializeField] private GameObject debugCubeForPathing = null;
    [Tooltip("Set true if you want the enemy to not be able to return via the air (useful for indoors)")]
    [SerializeField] private bool cannotTakeAirRouteBack = false;
    [Tooltip("Set true if you want the enemy to never give up on chasing its target even if stuck")]
    [SerializeField] private bool neverGiveUpOnAggro = false;
    [Tooltip("How many rotations enemy should do before returning after failing to find player")]
    [SerializeField] private float numberOfSearchingLastKnownAreaRotations = 1;
    [Tooltip("If you want the enemy to use its patrol speed instead of chase speed when returning")]
    [SerializeField] private bool usePatrolSpeedInsteadOfChaseOnReturn = false;

    //Obstacle avoidance configurable values
    [Tooltip("How far ahead the enemy should check for obstacles to determine if it needs to adjust path")]
    [SerializeField] private float distanceToCheckForObstacles = 1f;
    [Tooltip("How far ahead the enemy should check for obstacles to determine if it is stuck (should be less than obstacle check to give it time to move)")]
    [SerializeField] private float distanceToCheckForStuck = .5f;
    [Tooltip("Magnitude of how much enemy should move to avoid obstacles")]
    [SerializeField] private float obstacleAvoidanceAdjustmentRate = 1.25f;
    [Tooltip("REQUIRED: Transform to represent enemy's left side (for obstacle avoidance)")]
    [SerializeField] private Transform leftSide;
    [Tooltip("REQUIRED: Transform to represent enemy's right side (for obstacle avoidance)")]
    [SerializeField] private Transform rightSide;
    [Tooltip("REQUIRED: Transform to represent enemy's top side (for obstacle avoidance)")]
    [SerializeField] private Transform topSide;
    [Tooltip("(Not used currently) Transform to represent enemy's center (for obstacle avoidance)")]
    [SerializeField] private Transform center;

    //Enemy Sounds
    [Tooltip("Sound played when enemy is not chasing player")]
    [SerializeField] private AudioSource enemyBreathing = null;
    [Tooltip("Sound played when enemy sees player")]
    [SerializeField] private AudioSource enemyAggroNoise = null;
    [Tooltip("Sound played when enemy is chasing player")]
    [SerializeField] private AudioSource enemyChasingNoise = null;
    [Tooltip("Sound played when enemy is sleeping")]
    [SerializeField] private AudioSource enemySleepingNoise = null;
    [Tooltip("Sound played when enemy is stunned")]
    [SerializeField] private AudioSource enemyStunnedNoise = null;
    [Tooltip("Does Kaede have to yell SHIT! when she is seen?")]
    [SerializeField] private bool forcedYellShit = false;

    //Stuck configurable values
    [Tooltip("How long enemy is stuck without seeing player before it switches to search last area state")]
    [SerializeField] private float maxStuckTime = 5f;
    [Tooltip("Multiplier used to determine how aggressively enemy should try to move out of way of obstacle when it is stuck")]
    [SerializeField] private float stuckAvoidanceMultiplier = 3;

    //Stun configurable values
    [Tooltip("Whether or not this enemy should stop being stunned after a period of time")]
    [SerializeField] private bool canBreakFreeFromStun = false;
    [Tooltip("How long this enemy gets stunned for")]
    [SerializeField] private float stunLength = 30;

    [Tooltip("If enabled, this enemy will still kill you even in Story mode (not dev though)")]
    public bool OverrideKillStoryMode = false;

    #endregion

    [HideInInspector] public bool ForceChasing = false;

    #region Private/Local Values
    //Searching rotation values
    private bool rotating = false;
    private float timeSinceRotate = 0;
    private float currentRotation = 0;
    private int rotationsTotal = 1;
    private float searchingLastKnownRotation = 0;
    private float searchingAreaBackupTimer = 0;
    private float searchingLastKnownAreaStartingXRotation;
    private float searchingLastKnownAreaStartingZRotation;
    private bool altRotateCurrent = false;
    [HideInInspector] public bool IsCurrentlySleeping = false;

    private LayerMask layerMask;
    private LayerMask safeSpotOnlyMask;

    //Patrol following values
    private PatrolPoint[] patrolRoutePoints = null;
    private int patrolRouteIndex = 0;
    private bool goingForwardInPatrol = true;
    private bool waitForFullRotationFlag = true;
    private bool waitAtPatrolPointFlag = false;
    private float waitAtPatrolTimer = 0;
    private bool enemySwitchedToStayStill = false;

    private float turnAroundTimer = 0;

    //Raise/lower values
    private bool isRising = true;

    //Bounce values
    private const float _bounceDistance = .1f;
    private const float _bounceDuration = 3;
    private const float _thresholdToNotBounceDuringPatrol = 1.5f;
    private float bounceTimer = _bounceDuration/2;
    private float bounceCurrentHeight = _bounceDistance/2;
    private bool bouncingUp = true;


    //Values for determining return path
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private ReturnPath enemyReturnPathState = ReturnPath.NotDetermined;
    private Vector3 airTarget;
    private List<Vector3> pathTaken = null;
    private float pathTakenTimeSinceLastCheck = 0;
    private List<Vector3> returnPath = null;
    private float timeSinceReturnStateStart = 0;
    private const float _waitBeforeHandlingStuckWhenChangingToReturnTimer = .25f;

    //Chase values
    [HideInInspector] public Vector3 lastKnownTargetPos;
    [HideInInspector] public Vector3 lastKnownTargetPosToBeUpdated;
    private float timeSinceChaseStarted = 0;
    private float currentBonusSpeed = 0;
    private float dynamicSpeedAdjustment = 0;

    //Distraction values
    private float timeToWaitAtDistraction = 0;
    private float distractionTimer = 0;
    private bool investigatingDistraction = false;
    private Vector3 distractionLookAt;

    //Stuck values
    private bool stuck = false;
    private float stuckTime = 0;
    private int stuckCount = 0;
    private const int _maxStuckCount = 1000;
    private const float _stuckPlayerIgnoreTime = 3;
    private const float _distanceToCheckForPreventingClippingIntoDoor = .925f;
    private bool hasBackedUpWhenStuck = false;
    private bool hasCompletedBackUpWhenStuck = false;

    private float stunTimer = 0;

    //Enemy State Machine
    private EnemyState currentState = EnemyState.Searching;
    private bool wasSearchingBeforeStun = true;

    //Used for raycasts that should come out of enemy's eye (child with FieldOfView script)
    private Transform fieldOfViewTransform;
    private FieldOfView fov;

    //Audio values
    private Coroutine swapAudio = null;
    private float chasingAudioStartVolume;
    private float searchingAudioStartVolume;
    private float audioSwapTime = 1;

    //Animation
    private Animator animator;

    //Story Mode
    private bool isDisabledStoryMode = false;

    //State machine for determining and following return route
    private enum ReturnPath
    {
        NotDetermined, //Initial state for before the route is calculated
        StraightShot, //State for if the route is just to head straight to start pos
        AirFirst, //State for if the route is to immediately fly into air and return home
        ComplexRoute //State for if enemy has to follow its prior path before returning home
    }

    //State machine used to control enemy behavior
    public enum EnemyState
    {
        Searching, //Default state
        Chasing, //State for when enemy is actively pursuing the player's or a distraction's last known position
        Returning, //State for when enemy has given up and is returning to post
        Readjusting, //State for when it has returned to starting position but needs to turn back to its correct rotation
        SearchingLastKnownArea, //State for when enemy reaches last known position to search that area for player before returning
        Distracted, //State for when enemy has arrived at distraction and is investigating
        Stunned, // State for when stunned by a gong
        Unstunned // In case we want to bring them back out
    }
    #endregion

    private void Start()
    {
        if (initialCooldownTimerValue == -1) turnAroundTimer = cooldownForTurnAround;
        else turnAroundTimer = initialCooldownTimerValue;

        IsCurrentlySleeping = SleepingEnemy;
        animator = GetComponent<Animator>();
        chasingAudioStartVolume = enemyChasingNoise.volume;
        searchingAudioStartVolume = enemyBreathing.volume;
        pathTaken = new List<Vector3>();
        fov = GetComponentInChildren<FieldOfView>();
        fieldOfViewTransform = fov.transform;
        startingPosition = transform.position;
        startingRotation = transform.rotation;

        if (patrolRoute != null && patrolRoutePoints == null)
        {
            patrolRoutePoints = patrolRoute.GetComponentsInChildren<PatrolPoint>();
        }

        if (SleepingEnemy)
        {
            animator.SetBool("isSleeping", true);
        }

        layerMask = LayerMask.GetMask("Obstacles", "SafeSpot");
        safeSpotOnlyMask = LayerMask.GetMask("SafeSpot");

        if (bounceEnemy && reverseBounce)
        {
            bouncingUp = false;
            bounceCurrentHeight *= -1;
        }

        //Hardcore Anglers turn around more frequently and have wider range for it.
        if (AnglerEnemy && GameManager.instance.CurrentGameMode == GameManager.GameMode.Hardcore)
        {
            cooldownForTurnAround -= 15;
            distanceThresholdForTurnAround += 5;
        }
    }

    //Trigger for when contact with player is made
    void OnTriggerEnter(Collider other)
    {
        if (AnglerEnemy) return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameManager.instance.CaughtPlayer(center);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if (neverGiveUpOnAggro) return;
            EnemyAI otherEnemy = other.GetComponent<EnemyAI>();

            //Only should adjust for other enemy if they're both chasing or at least aware of / near player (Chasing, Searching Last Known Area)
            if (currentState == EnemyState.Chasing)
            {
                if (otherEnemy.currentState == EnemyState.Chasing && Vector3.Distance(transform.position, lastKnownTargetPos) < Vector3.Distance(other.transform.position, otherEnemy.lastKnownTargetPos))
                {
                    Debug.Log(gameObject + " gave up because it ran into " + other.gameObject + "which was closer to target.");
                    SwapFromChasingToReturningDirectly();
                }
                else if (otherEnemy.currentState == EnemyState.SearchingLastKnownArea)
                {
                    Debug.Log(gameObject + " gave up because it ran into " + other.gameObject + ", which was already at target in SearchingLastKnownArea state");
                    SwapFromChasingToReturningDirectly();
                }
            }
            else if (currentState == EnemyState.SearchingLastKnownArea && otherEnemy.currentState == EnemyState.SearchingLastKnownArea)
            {
                Debug.Log(gameObject + " gave up because it ran into " + other.gameObject + " and they were both already in SearchingLastKnownArea state");
                SwapFromChasingToReturningDirectly();
            }
        }
    }

    #region Update
    void Update()
    {
        if (GameManager.instance.GamePaused || isDisabledStoryMode) return;

        if (bounceEnemy && currentState != EnemyState.Chasing && currentState != EnemyState.Stunned && DistanceToNextPatrolIsGreaterThanThreshold() && !(currentState == EnemyState.Returning && enemyReturnPathState == ReturnPath.StraightShot))
        {
            if (Time.timeSinceLevelLoad > initBounceDelay)
            {
                if (bouncingUp)
                {
                    bounceTimer += Time.deltaTime;
                    float targetHeight = Mathf.Lerp(-_bounceDistance, _bounceDistance, bounceTimer / _bounceDuration);
                    if (bounceTimer >= _bounceDuration)
                    {
                        targetHeight = _bounceDistance;
                        bouncingUp = false;
                        bounceTimer = 0;
                    }
                    float deltaY = targetHeight - bounceCurrentHeight;
                    bounceCurrentHeight = targetHeight;
                    transform.position += new Vector3(0, deltaY, 0);
                }
                else
                {
                    bounceTimer += Time.deltaTime;
                    float targetHeight = Mathf.Lerp(_bounceDistance, -_bounceDistance, bounceTimer / _bounceDuration);
                    if (bounceTimer >= _bounceDuration)
                    {
                        targetHeight = -_bounceDistance;
                        bouncingUp = true;
                        bounceTimer = 0;
                    }
                    float deltaY = targetHeight - bounceCurrentHeight;
                    bounceCurrentHeight = targetHeight;
                    transform.position += new Vector3(0, deltaY, 0);
                }
            }
        }

        //WaitForTrigger check after bounce so it will still bounce even when not activated (for angler sightjack)
        if (waitForTrigger) return;
        
        //Remember path taken in case needed for return route
        if (currentState == EnemyState.Chasing || currentState == EnemyState.Returning)
        {
            pathTakenTimeSinceLastCheck += Time.deltaTime;

            if (pathTakenTimeSinceLastCheck >= pathTakenPollingInterval)
            {
                pathTaken.Add(transform.position);
                pathTakenTimeSinceLastCheck = 0;
            }
        }

        switch (currentState)
        {
            case EnemyState.Searching:
                if (!SleepingEnemy)
                {
                    if (!enemyBreathing.isPlaying) enemyBreathing.Play();
                    if (rotatingEnemy && patrolRoute == null)
                    {
                        if (!rotating) timeSinceRotate += Time.deltaTime;

                        if (timeSinceRotate >= timeBetweenRotations)
                        {
                            rotating = true;
                            timeSinceRotate = 0;
                        }

                        if (rotating)
                        {
                            if (!altRotateCurrent)
                            {
                                transform.Rotate(new Vector3(0, rotationRate * Time.deltaTime, 0));
                                currentRotation += rotationRate * Time.deltaTime;

                                if (currentRotation >= totalRotation * rotationsTotal)
                                {
                                    rotating = false;
                                    if (alternateRotateBack)
                                    {
                                        altRotateCurrent = true;
                                        rotationsTotal--;
                                    }
                                    else rotationsTotal++;
                                    
                                }
                            }
                            else
                            {
                                transform.Rotate(new Vector3(0, -rotationRate * Time.deltaTime, 0));
                                currentRotation += -rotationRate * Time.deltaTime;

                                if (currentRotation <= totalRotation * rotationsTotal)
                                {
                                    rotating = false;
                                    altRotateCurrent = false;
                                    rotationsTotal++;
                                }
                            }
                        }
                    }
                    else if (patrolRoute != null)
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(patrolRoutePoints[patrolRouteIndex].transform.position - transform.position), rotationRate * Time.deltaTime);
                        if (waitForFullRotationFlag)
                        {
                            //TODO: There seems to be some bug where occasionally it will ignore this flag, I assume the following line is occasionally returning true in unexpected cases. Only seen it happen twice and only when it is returning to the 0th patrol point
                            if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(patrolRoutePoints[patrolRouteIndex].transform.position - transform.position)) <= rotationThreshold)
                            {
                                waitForFullRotationFlag = false;
                            }
                        }
                        if (!waitForFullRotationFlag)
                        {
                            Vector3 dirToTarg = (patrolRoutePoints[patrolRouteIndex].transform.position - transform.position).normalized;
                            if (patrolObstacleAvoidance) ObstacleAvoidance();
                            if (patrolObstacleAvoidance && patrolCanGetStuck && stuck)
                            {
                                waitForFullRotationFlag = true;
                                if (goingForwardInPatrol)
                                {
                                    goingForwardInPatrol = false;
                                    patrolRouteIndex--;
                                    return;
                                }
                                else
                                {
                                    goingForwardInPatrol = true;
                                    patrolRouteIndex++;
                                    return;
                                }
                            }
                            else if (!waitAtPatrolPointFlag) transform.position += dirToTarg * patrolSpeed * Time.deltaTime;

                            if (Vector3.Distance(transform.position, patrolRoutePoints[patrolRouteIndex].transform.position) <= distanceThreshold)
                            {
                                if (patrolRoutePoints[patrolRouteIndex].WaitTime > 0)
                                {
                                    if (waitAtPatrolPointFlag)
                                    {
                                        waitAtPatrolTimer += Time.deltaTime;
                                        if (waitAtPatrolTimer >= patrolRoutePoints[patrolRouteIndex].WaitTime)
                                        {
                                            waitAtPatrolPointFlag = false;
                                            waitAtPatrolTimer = 0;
                                        }
                                        else return;
                                    }
                                    else
                                    {
                                        waitAtPatrolPointFlag = true;
                                        waitAtPatrolTimer = 0;
                                        return;
                                    }
                                }
                                if (patrolRoutePoints[patrolRouteIndex].EndPatrol)
                                {
                                    patrolRoute = null;
                                    return;
                                }
                                if (goingForwardInPatrol)
                                {
                                    waitForFullRotationFlag = patrolRoutePoints[patrolRouteIndex].WaitForRotate;
                                    patrolRouteIndex++;
                                    if (patrolRouteIndex >= patrolRoutePoints.Length)
                                    {
                                        if (patrolRoutePoints[patrolRouteIndex - 1].CircularRoute) patrolRouteIndex = 0;
                                        else
                                        {
                                            patrolRouteIndex = patrolRoutePoints.Length - 2;
                                            goingForwardInPatrol = false;
                                            if (patrolRouteIndex > -1 && patrolRoutePoints[patrolRouteIndex].SkipOnBackwards) patrolRouteIndex--;
                                        }
                                    }
                                }
                                else
                                {
                                    waitForFullRotationFlag = patrolRoutePoints[patrolRouteIndex].WaitForRotate;
                                    patrolRouteIndex--;
                                    //Note: Currently can only have 1 skip on backwards node in a row since I'm only throwing it in for one specific scenario right now anyway
                                    if (patrolRouteIndex > -1 && patrolRoutePoints[patrolRouteIndex].SkipOnBackwards) patrolRouteIndex--;
                                    if (patrolRouteIndex <= -1)
                                    {
                                        if (patrolRoutePoints[patrolRouteIndex + 1].CircularRoute) patrolRouteIndex = patrolRoutePoints.Length - 1;
                                        else
                                        {
                                            patrolRouteIndex = 1;
                                            goingForwardInPatrol = true;
                                        }
                                    }
                                }
                            }
                            else if (AnglerEnemy)
                            {
                                turnAroundTimer += Time.deltaTime;
                                //If not on cooldown
                                if (turnAroundTimer >= cooldownForTurnAround)
                                {
                                    //If player within threshold
                                    if (Vector3.Distance(transform.position, GameManager.instance.Player.PlayerHead.position) < distanceThresholdForTurnAround)
                                    {
                                        //Determine nearest patrol point to player that is also not blocked by obstacles and does not have CanTurnAround=false
                                        int closestPatrolIndex = GetNearestEligiblePatrolPoint();
                                        //If it is different than current, waitForRotateFlag and switch to it, and reset cooldown.
                                        if (closestPatrolIndex != patrolRouteIndex && closestPatrolIndex != -1 && patrolRoutePoints[closestPatrolIndex].CanTurnAround)
                                        {
                                            if (!DetermineIfIndexIsNextAnyway(closestPatrolIndex))
                                            {
                                                waitForFullRotationFlag = true;
                                                patrolRouteIndex = closestPatrolIndex;
                                                turnAroundTimer = 0;
                                                //Also determine if it should go forward or backwards
                                                goingForwardInPatrol = DetermineIfForwardsBetter();
                                                Debug.Log(ToString() + ": Turned around to patrol point: " + closestPatrolIndex + " and going forward = " + goingForwardInPatrol);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (enemySwitchedToStayStill) HandleStuckOnReturn();
                    if (raiseLowerEnemy)
                    {
                        if (isRising)
                        {
                            transform.position += new Vector3(0, raiseLowerSpeed * Time.deltaTime, 0);
                            if (transform.position.y >= maxHeight) isRising = false;
                        }
                        else
                        {
                            transform.position += new Vector3(0, -raiseLowerSpeed * Time.deltaTime, 0);
                            if (transform.position.y <= minHeight)
                            {
                                isRising = true;
                            }
                        }
                    }
                }
                //TODO: Sleeping Enemy
                else
                {
                    if (!IsCurrentlySleeping)
                    {
                        IsCurrentlySleeping = SleepingEnemy;
                        animator.SetBool("isSleeping", true);
                    }
                    if (enemyBreathing.isPlaying) enemyBreathing.Stop();
                    if (enemySleepingNoise != null && !enemySleepingNoise.isPlaying) enemySleepingNoise.Play();
                    //Perform check for player sound (if they're sprinting within set area could work), if so go to wake up state and temporarily activate visuals, end sleeping audio until that state ends.
                }
                break;
            case EnemyState.Chasing:
                if (SleepingEnemy) IsCurrentlySleeping = false;
                //If enemy is investigating distraction, it should not switch to chasing noise
                if (!investigatingDistraction)
                {
                    enemyBreathing.Stop();
                    if (enemySleepingNoise != null) enemySleepingNoise.Stop();
                    if (!enemyAggroNoise.isPlaying && !enemyChasingNoise.isPlaying)
                    {
                        enemyChasingNoise.Play();
                        enemyChasingNoise.volume = chasingAudioStartVolume;
                    }
                }
                //Make sure it can reach the last known position before setting it as target
                if (lastKnownTargetPosToBeUpdated != lastKnownTargetPos)
                {
                    if (!AreObstaclesInWay(lastKnownTargetPosToBeUpdated))
                    {
                        lastKnownTargetPos = lastKnownTargetPosToBeUpdated;
                    }
                }

                //Rotate slowly towards target unless we're right next to it, in which case do it instantly
                if (Vector3.Distance(transform.position, lastKnownTargetPos) <= chaseQuickTurnDistanceThreshold) transform.LookAt(lastKnownTargetPos);
                else transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lastKnownTargetPos - transform.position), chaseRotateTowardsPlayerRate * Time.deltaTime);

                if (!investigatingDistraction) ObstacleAvoidance();

                //If enemy is stuck on a safespot then just have it return rather than sit there
                if (stuck && StuckOnSafeSpot() && !neverGiveUpOnAggro) {
                    SwapFromChasingToReturningDirectly();
                    return;
                }

                //Used to prevent enemy from clipping through when stuck (specifically for sliding doors)
                if (backupWhenStuck && stuck && !hasCompletedBackUpWhenStuck && Physics.Raycast(center.position, center.forward, _distanceToCheckForPreventingClippingIntoDoor, layerMask))
                {
                    hasBackedUpWhenStuck = true;
                    Vector3 dirToTarg = (lastKnownTargetPos - transform.position).normalized;
                    transform.position -= dirToTarg * Time.deltaTime;
                }
                else if (backupWhenStuck && hasBackedUpWhenStuck)
                {
                    hasCompletedBackUpWhenStuck = true;
                }

                timeSinceChaseStarted += Time.deltaTime;
                if (timeSinceChaseStarted > delayBeforeBonusSpeed)
                {
                    currentBonusSpeed = Mathf.Lerp(0, maxBonusSpeed, (timeSinceChaseStarted - delayBeforeBonusSpeed)/timeTakenForFullBonusSpeed);
                }

                if (dynamicSpeedEnabled)
                {
                    float distanceToPlayer = Vector3.Distance(transform.position, GameManager.instance.Player.PlayerHead.position);
                    if (distanceToPlayer >= distanceForSpeedUp) dynamicSpeedAdjustment = dynamicSpeedChange;
                    else if (distanceToPlayer < distanceForSlowDown) dynamicSpeedAdjustment = -dynamicSpeedChange;
                    else dynamicSpeedAdjustment = 0;
                }

                //If ObstacleAvoidance() determined enemy is stuck, don't move, and if has been stuck for 5 sec without seeing player move on to next state. Otherwise, move.
                if (!stuck || investigatingDistraction)
                {
                    stuckTime = 0;
                    Vector3 dirToTarg = (lastKnownTargetPos - transform.position).normalized;
                    transform.position += dirToTarg * (chaseSpeed + currentBonusSpeed + dynamicSpeedAdjustment) * Time.deltaTime;
                }
                else if (!neverGiveUpOnAggro)
                {
                    stuckTime += Time.deltaTime;
                    if (stuckTime >= maxStuckTime)
                    {
                        GameManager.instance.Player.EnemyAggroCount--;
                        fov.TemporarilyIgnorePlayer(_stuckPlayerIgnoreTime);
                        SetState(EnemyState.SearchingLastKnownArea);
                    }
                }

                if (!neverGiveUpOnAggro && stuckCount >= _maxStuckCount)
                {
                    GameManager.instance.Player.EnemyAggroCount--;
                    fov.TemporarilyIgnorePlayer(_stuckPlayerIgnoreTime);
                    SetState(EnemyState.SearchingLastKnownArea);
                }

                //If enemy has arrived at target, time to switch to next state.
                if (Vector3.Distance(transform.position, lastKnownTargetPos) <= distanceThreshold)
                {
                    timeSinceChaseStarted = 0;
                    currentBonusSpeed = 0;

                    if (investigatingDistraction)
                    {
                        GameManager.instance.Player.EnemyAggroCount--;
                        SetState(EnemyState.Distracted);
                    }
                    else
                    {
                        GameManager.instance.Player.EnemyAggroCount--;
                        searchingLastKnownRotation = 0;
                        SetState(EnemyState.SearchingLastKnownArea);
                    }
                }
                break;
            //If enemy arrives where it last saw player and its not there, rotate around to search area.
            case EnemyState.SearchingLastKnownArea:
                if (!enemyBreathing.isPlaying)
                {
                    StopAllCoroutines();
                    swapAudio = StartCoroutine(SwapChaseForSearchingNoise());
                }

                searchingAreaBackupTimer += Time.deltaTime;

                if (searchingAreaBackupTimer <= searchingAreaBackUpMaxTime)
                {
                    Vector3 dirToTarg = (lastKnownTargetPos - transform.position).normalized;
                    transform.position -= dirToTarg * chaseSpeed * .3f * Time.deltaTime;
                }

                if (searchingAreaBackupTimer <= reBalanceRotationTime)
                {
                    transform.localEulerAngles = new Vector3(Mathf.Lerp(searchingLastKnownAreaStartingXRotation, 0, searchingAreaBackupTimer / reBalanceRotationTime), transform.localEulerAngles.y, Mathf.Lerp(searchingLastKnownAreaStartingZRotation, 0, searchingAreaBackupTimer / reBalanceRotationTime));
                }
                else
                {
                    transform.Rotate(new Vector3(0, rotationRate * Time.deltaTime, 0));
                    searchingLastKnownRotation += rotationRate * Time.deltaTime;

                    //Once the proper number of rotations are complete, move on to next state
                    if (searchingLastKnownRotation >= 360 * numberOfSearchingLastKnownAreaRotations)
                    {
                        searchingLastKnownRotation = 0;
                        SetState(EnemyState.Returning);
                    }
                }
                break;
            case EnemyState.Returning:
                if (!enemyBreathing.isPlaying) enemyBreathing.Play();

                /*If enemy hasn't determined return route yet, calculate it. Currently works as follows: 
                 * 1. If it can just head directly to starting point without obstacles, do that.
                 * 2. If not, if it can fly up and then get a direct shot to starting point, do that.
                 * 3. If not, determine the first prior point it was at where it could do 1 or 2. If it can immediately
                 * return to that point, head straight there. If not, follow the path it took to get here back until it gets
                 * to that point.
                 */
                if (enemyReturnPathState == ReturnPath.NotDetermined)
                {
                    timeSinceReturnStateStart = 0;

                    if (!AreObstaclesInWay(startingPosition)) {
                        enemyReturnPathState = ReturnPath.StraightShot;
                    }
                    else
                    {
                        //Determines if air is clear, if so state gets set to AirFirst
                        CheckAirFirst();

                        if (enemyReturnPathState != ReturnPath.AirFirst)
                        {
                            for(int j = pathTaken.Count - 1; j >= 0 && enemyReturnPathState == ReturnPath.NotDetermined; j--)
                            {
                                if (!AreObstaclesInWay(pathTaken[j], startingPosition)) GenerateStraightPath(j);
                                else {
                                    Vector3 checkAirforLocation = CheckAirForLocation(pathTaken[j]);
                                    if (checkAirforLocation != new Vector3())
                                    {
                                        GenerateAirPath(j, checkAirforLocation);
                                    }
                                }
                            }
                        }
                    }

                    //Debug.Log(enemyReturnPathState);
                }

                timeSinceReturnStateStart += Time.deltaTime;

                float returnSpeed = usePatrolSpeedInsteadOfChaseOnReturn ? patrolSpeed : chaseSpeed;

                //TODO: Maybe obstacle avoidance needs to be based off the angle it will turn to, or not be checked until it is done rotating
                if (enemyReturnPathState == ReturnPath.StraightShot)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(startingPosition - transform.position), returningRotateRate * Time.deltaTime);
                    ObstacleAvoidance();
                    if (patrolCanGetStuck && stuck)
                    {
                        if (timeSinceReturnStateStart > _waitBeforeHandlingStuckWhenChangingToReturnTimer)
                        {
                            HandleStuckOnReturn();
                            return;
                        }
                    }
                    Vector3 dirToTarg = (startingPosition - transform.position).normalized;
                    transform.position += dirToTarg * returnSpeed * Time.deltaTime;
                }
                else if (enemyReturnPathState == ReturnPath.AirFirst)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(startingPosition - transform.position), returningRotateRate * Time.deltaTime);
                    transform.position += new Vector3(0, returnSpeed * Time.deltaTime);

                    if (Vector3.Distance(transform.position, airTarget) <= distanceThreshold || transform.position.y >= airTarget.y)
                    {
                        enemyReturnPathState = ReturnPath.StraightShot;
                    }
                }
                else if (enemyReturnPathState == ReturnPath.ComplexRoute)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(returnPath[0] - transform.position), returningRotateRate * Time.deltaTime);
                    ObstacleAvoidance();
                    if (patrolCanGetStuck && stuck)
                    {
                        if (timeSinceReturnStateStart > _waitBeforeHandlingStuckWhenChangingToReturnTimer)
                        {
                            HandleStuckOnReturn();
                            return;
                        }
                    }
                    Vector3 dirToTarg = (returnPath[0] - transform.position).normalized;
                    transform.position += dirToTarg * returnSpeed * Time.deltaTime;

                    if (Vector3.Distance(transform.position, returnPath[0]) <= distanceThreshold)
                    {
                        returnPath.RemoveAt(0);
                        if (returnPath.Count == 0) enemyReturnPathState = ReturnPath.StraightShot;
                        //TODO: Probably best to revamp/simplify the entire state machine sometime later. This check is to make it so it will look at starting pos instead of straight up when doing air.
                        //TODO Update: ^This actually doesn't work properly right now. Definitely needs to be resolved eventually, though maybe we will make it so air isn't prioritized as much anyway?
                        /*else if (returnPath.Count == 1 && returnPath[0].y > startingPosition.y)
                        {
                            airTarget = returnPath[0];
                            enemyReturnPathState = ReturnPath.AirFirst;
                        }*/
                    }
                }

                //If we're back at the start, move on to next state
                if (Vector3.Distance(transform.position, startingPosition) <= distanceThreshold)
                {
                    SetState(EnemyState.Readjusting);
                }
                break;
            case EnemyState.Readjusting:
                if (!enemyBreathing.isPlaying) enemyBreathing.Play();
                transform.position = startingPosition;
                transform.rotation = Quaternion.Slerp(transform.rotation, startingRotation, Time.deltaTime);
                //TODO: I imagine there is a better solution to this, or at least I should be able to figure out why sometimes they're negative and sometimes positive...
                //Switched this to use angles since w/ quaternion it was giving massive leeway for rotationThreshold. Should probably still have this all fixed to not be absolute nonsense sometime but... ya know
                if ((Mathf.Abs(transform.rotation.eulerAngles.y + startingRotation.eulerAngles.y) < rotationThreshold || Mathf.Abs(transform.rotation.eulerAngles.y - startingRotation.eulerAngles.y) < rotationThreshold) &&
                    (Mathf.Abs(transform.rotation.eulerAngles.x + startingRotation.eulerAngles.x) < rotationThreshold || Mathf.Abs(transform.rotation.eulerAngles.x - startingRotation.eulerAngles.x) < rotationThreshold) &&
                    (Mathf.Abs(transform.rotation.eulerAngles.z + startingRotation.eulerAngles.z) < rotationThreshold || Mathf.Abs(transform.rotation.eulerAngles.z - startingRotation.eulerAngles.z) < rotationThreshold))
                {
                    transform.rotation = startingRotation;
                    SetState(EnemyState.Searching);
                }
                break;
            case EnemyState.Distracted:
                if (!enemyBreathing.isPlaying) enemyBreathing.Play();
                distractionTimer += Time.deltaTime;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(distractionLookAt - transform.position), rotationRate * Time.deltaTime);
                
                if (distractionTimer >= timeToWaitAtDistraction)
                {
                    SetState(EnemyState.Returning);
                }
                break;
            case EnemyState.Stunned:
                if (canBreakFreeFromStun)
                {
                    stunTimer += Time.deltaTime;
                    if (stunTimer >= stunLength)
                    {
                        animator.SetBool("isStunned", false);
                        if (wasSearchingBeforeStun) SetState(EnemyState.Searching);
                        else SetState(EnemyState.Returning);
                        GameManager.instance.AlertPCSThatNoLongerStunned(this);
                    }
                }
                break;
        }
    }
    #endregion

    private void SwapFromChasingToReturningDirectly()
    {
        timeSinceChaseStarted = 0;
        currentBonusSpeed = 0;
        GameManager.instance.Player.EnemyAggroCount--;
        searchingLastKnownRotation = 0;
        SetState(EnemyState.Returning);
        swapAudio = StartCoroutine(SwapChaseForSearchingNoise());
    }

    private void HandleStuckOnReturn()
    {
        if (!handleStuckOnReturnEnemy) return;

        if (patrolRoutePoints != null && patrolRoutePoints.Length > 0)
        {
            for (int i = 0; i < patrolRoutePoints.Length; i++)
            {
                if (!AreObstaclesInWay(patrolRoutePoints[i].transform.position))
                {
                    enemySwitchedToStayStill = false;
                    patrolRouteIndex = i;
                    patrolRoute = gameObject; //Kind of weird but the check is on if patrolRoute is null or not so if this happens twice it needs to be set to something after
                    SetState(EnemyState.Searching);
                    return;
                }
            }
        }
        enemySwitchedToStayStill = true;
        patrolRoute = null;
        SetState(EnemyState.Searching);
    }

    private bool DistanceToNextPatrolIsGreaterThanThreshold()
    {
        if (patrolRoute == null) return true;
        return (Vector3.Distance(transform.position, patrolRoutePoints[patrolRouteIndex].transform.position) > _thresholdToNotBounceDuringPatrol) || waitForFullRotationFlag;
    }

    public void ModifySpeeds(float patrolChange, float chaseChange)
    {
        patrolSpeed += patrolChange;
        chaseSpeed += chaseChange; 
    }

    public bool IsDisabledStory()
    {
        return isDisabledStoryMode;
    }

    public void SetDisabledStory(bool state = true)
    {
        isDisabledStoryMode = state;
    }

    public void ResetRotationValues()
    {
        rotating = false;
        timeSinceRotate = 0;
        currentRotation = 0;
        rotationsTotal = 1;
    }

    public int GetNearestEligiblePatrolPoint()
    {
        int closestIndex = -1;
        float minimumDistance = 99999;
        //Determine nearest patrol point to player that is also not blocked by obstacles
        //If it is different than current, waitForRotateFlag and switch to it, and reset cooldown.
        for (int i = 0; i < patrolRoutePoints.Length; i++)
        {
            if(!AreObstaclesInWay(patrolRoutePoints[i].transform.position) && !AreObstaclesInWay(patrolRoutePoints[i].transform.position, GameManager.instance.Player.PlayerHead.position))
            {
                float distanceToPlayer = Vector3.Distance(patrolRoutePoints[i].transform.position, GameManager.instance.Player.PlayerHead.position);   
                if (distanceToPlayer < minimumDistance)
                { 
                    minimumDistance = distanceToPlayer;
                    closestIndex = i;  
                }
            }
        }
        return closestIndex;
    }

    public bool DetermineIfIndexIsNextAnyway(int targetIndex)
    {
        if (goingForwardInPatrol)
        {
            if (patrolRoutePoints.Length == (patrolRouteIndex + 1))
            {
                return targetIndex == 0;
            }
            else
            {
                return targetIndex == patrolRouteIndex + 1;
            }
        }
        else
        {
            if (patrolRouteIndex == 0)
            {
                return targetIndex == patrolRoutePoints.Length - 1;
            }
            else
            {
                return targetIndex == patrolRouteIndex - 1;
            }
        }
    }

    //Assumes circular route
    public bool DetermineIfForwardsBetter()
    {
        Vector3 testPos = transform.position;
        float distanceFromForwardPatrol;
        float distanceFromBackwardsPatrol;
        Vector3 dirToTarg;
        if (patrolRoutePoints.Length == (patrolRouteIndex + 1))
        {
            dirToTarg = (patrolRoutePoints[0].transform.position - patrolRoutePoints[patrolRouteIndex].transform.position).normalized;
        }
        else
        {
            dirToTarg = (patrolRoutePoints[patrolRouteIndex + 1].transform.position - patrolRoutePoints[patrolRouteIndex].transform.position).normalized;
        }
        testPos += dirToTarg * 1;
        distanceFromForwardPatrol = Vector3.Distance(testPos, GameManager.instance.Player.PlayerHead.position);

        testPos = transform.position;
        if (patrolRouteIndex == 0)
        {
            dirToTarg = (patrolRoutePoints[patrolRoutePoints.Length - 1].transform.position - patrolRoutePoints[patrolRouteIndex].transform.position).normalized;
        }
        else
        {
            dirToTarg = (patrolRoutePoints[patrolRouteIndex - 1].transform.position - patrolRoutePoints[patrolRouteIndex].transform.position).normalized;
        }
        testPos += dirToTarg * 1;
        distanceFromBackwardsPatrol = Vector3.Distance(testPos, GameManager.instance.Player.PlayerHead.position);

        return distanceFromForwardPatrol < distanceFromBackwardsPatrol;
    }

    //Added this additional check because in editor after swapping save systems for some reason it will swap it twice, and since this method is only called on start of scene it will only occur once.
    private bool hasSwapped = false;

    public void SwapDirection()
    {
        if (!hasSwapped)
        {
            goingForwardInPatrol = !goingForwardInPatrol;
            hasSwapped = true;
        }
    }
     

    public void TriggerStart()
    {
        waitForTrigger = false;
    }

    public void TriggerInstant()
    {
        waitForTrigger = false;
        if (patrolRoute != null && patrolRoutePoints == null) patrolRoutePoints = patrolRoute.GetComponentsInChildren<PatrolPoint>();
        waitForFullRotationFlag = false;
        transform.SetPositionAndRotation(patrolRoutePoints[patrolRouteIndex].transform.position, transform.rotation);
    }

    //If the player's chest (main target) isn't in LOS then use other parts of body instead. This is to avoid enemy seeing like left arm and then giving up because it sees it can't reach head or something.
    public void SetNewTargetPos()
    {
        if (!AreObstaclesInWay(GameManager.instance.Player.PlayerChest.position)) lastKnownTargetPosToBeUpdated = GameManager.instance.Player.PlayerChest.position;
        else if (!AreObstaclesInWay(GameManager.instance.Player.PlayerLeftSide.position)) lastKnownTargetPosToBeUpdated = GameManager.instance.Player.PlayerLeftSide.position;
        else if (!AreObstaclesInWay(GameManager.instance.Player.PlayerRightSide.position)) lastKnownTargetPosToBeUpdated = GameManager.instance.Player.PlayerRightSide.position;
        else if (!AreObstaclesInWay(GameManager.instance.Player.PlayerHead.position)) lastKnownTargetPosToBeUpdated = GameManager.instance.Player.PlayerHead.position;
        else
        {
            //Debug.Log("Player not in LOS");
            //set to chest and it will fail to change state later
            lastKnownTargetPosToBeUpdated = GameManager.instance.Player.PlayerChest.position;
        }
    }

    //Sets state and changes any values needed for that state
    public void SetState(EnemyState state)
    {
        if (state == EnemyState.Stunned)
        {
            if (enemyBreathing.isPlaying)
            {
                enemyBreathing.Stop();
            }

            stunTimer = 0;

            if (currentState != EnemyState.Stunned) wasSearchingBeforeStun = currentState == EnemyState.Searching;

            if (enemyStunnedNoise != null && !enemyStunnedNoise.isPlaying && currentState != EnemyState.Stunned)
            {
                StartCoroutine(DelayAndPlaySound(1.0f, enemyStunnedNoise));
            }
        }

        if (currentState == EnemyState.Stunned && (!canBreakFreeFromStun || stunTimer < stunLength))
        {
            return;
        }

        //If we're back to searching, we need to reset pathTaken for next time
        if (state == EnemyState.Searching) pathTaken = new List<Vector3>();

        //Let's make sure we can actually reach the target we're chasing after before we try to chase. And if we can, play aggro noise too.
        if (state != currentState && state == EnemyState.Chasing)
        {
            if (!ForceChasing && AreObstaclesInWay(lastKnownTargetPosToBeUpdated)) return;
            if (!enemyAggroNoise.isPlaying) enemyAggroNoise.Play();
            ForceChasing = false;
            if (GameManager.instance.Player != null)
            {
                GameManager.instance.Player.EnemyAggroCount++;
                if (GameManager.instance.Player.EnemyAggroCount == 1 && !investigatingDistraction) KaedeVoiceManager.instance.TriggerEnemyAggro(forcedYellShit);
            }
            stuckCount = 0;
            currentBonusSpeed = 0;
            timeSinceChaseStarted = 0;
            //Possibly this should be set just if EnemyState set to Chasing even if already was
            hasBackedUpWhenStuck = false;
            hasCompletedBackUpWhenStuck = false;
        }

        if (state == EnemyState.Stunned)
        {
            if (enemyChasingNoise.isPlaying) enemyChasingNoise.Stop();
            if (currentState == EnemyState.Chasing)
            {
                if (enemyAggroNoise.isPlaying)
                {
                    enemyAggroNoise.Stop();
                }
                GameManager.instance.Player.EnemyAggroCount--;
            }
        }


        //Removing this for now. I thought before that it could be kind of cool if the enemy just sits there if it can see you but not reach you, but in practice it just
        //makes it feel a little silly when it gets stuck on geometry and just sits there forever.
        //If we currently see enemy, it's ok to be stuck (like if enemy sees you through a crack it can't go through)
        //if (state == EnemyState.Chasing) stuckTime = 0;

        if (state != currentState)
        {
            Debug.Log(gameObject.name + " state changed to: " + state);
            if (state == EnemyState.Chasing)
            {
                animator.SetBool("isChasing", true);

                if (SleepingEnemy)
                {
                    animator.SetBool("isSleeping", false);
                    animator.SetBool("isWaking", true);
                }
            }
            else
            {
                animator.SetBool("isChasing", false);
                if (state == EnemyState.Stunned)
                {
                    animator.SetBool("isStunned", true);
                }
            }
            if (state == EnemyState.Searching)
            {
                if (SleepingEnemy)
                {
                    animator.SetBool("isWaking", false);
                    animator.SetBool("isSleeping", true);
                }
            }
            if (state == EnemyState.SearchingLastKnownArea)
            {
                searchingAreaBackupTimer = 0;
                searchingLastKnownAreaStartingXRotation = transform.rotation.x;
                searchingLastKnownAreaStartingXRotation = transform.rotation.z;
            }
        } 
        investigatingDistraction = false;
        
        //If we're switching from searching to chasing a target then set the positions to return to where enemy was initially
        if (currentState == EnemyState.Searching && state == EnemyState.Chasing)
        {
            startingPosition = transform.position;
            startingRotation = transform.rotation;
        }

        //If we're switching to be returning, we need to determine return path next frame
        if (state == EnemyState.Returning)
        {
            enemyReturnPathState = ReturnPath.NotDetermined;
        }

        currentState = state;
    }

    public EnemyState GetState()
    {
        return currentState;
    }

    private IEnumerator DelayAndPlaySound(float delay, AudioSource sound)
    {
        yield return new WaitForSeconds(delay);

        sound.Play();
        yield return null;
    }

    //Coroutine used for switching from chasing noise to breathing noise so it doesn't immediately/abruptly shift. 
    private IEnumerator SwapChaseForSearchingNoise()
    {
        enemyBreathing.Play();
        float swapTime = 0;
        do
        {
            swapTime += Time.deltaTime;
            enemyChasingNoise.volume = Mathf.Lerp(chasingAudioStartVolume, 0, swapTime/audioSwapTime);
            enemyBreathing.volume = Mathf.Lerp(0, searchingAudioStartVolume, swapTime/audioSwapTime);
            yield return null;
        } while (enemyChasingNoise.volume > 0);
        enemyChasingNoise.Stop();
    }

    //Set init state and values for investigating a distraction
    public void InvestigateDistraction(Vector3 distractionLocation, Vector3 distractionToLookAt, float lengthOfDistraction)
    {
        if (currentState == EnemyState.Chasing || currentState == EnemyState.Distracted || isDisabledStoryMode) return;
        lastKnownTargetPosToBeUpdated = distractionLocation;
        if (Vector3.Distance(transform.position, lastKnownTargetPos) <= distanceThreshold * 3) SetState(EnemyState.Distracted);
        else
        {
            investigatingDistraction = true;
            SetState(EnemyState.Chasing);
        }
        investigatingDistraction = true;
        distractionLookAt = distractionToLookAt;
        timeToWaitAtDistraction = lengthOfDistraction;
        distractionTimer = 0;
    }

    #region Return Path Functions
    //Used to determine if Enemy can fly up and return to start, and if so sets path and state
    private void CheckAirFirst()
    {
        if (cannotTakeAirRouteBack) return;
        for (int i = 1; i < returningMaxAirAttempts + 1 && enemyReturnPathState != ReturnPath.AirFirst; i++)
        {
            if (IsAirClear(i * returningIntoAirIntervalDistance))
            {
                airTarget = transform.position;
                airTarget.y += i * returningIntoAirIntervalDistance;
                if (!AreObstaclesInWay(airTarget, startingPosition))
                {
                    //Debug.Log("Attempts taken for return in air: " + i);
                    enemyReturnPathState = ReturnPath.AirFirst;
                }
            }
        }
    }

    //Determines if 'location' has a clear sky that has straight-shot to starting pos, and if so returns the location. Returns new Vector3() otherwise as 'false' case
    private Vector3 CheckAirForLocation(Vector3 location)
    {
        if (cannotTakeAirRouteBack) return new Vector3();
        for (int i = 1; i < returningMaxAirAttempts + 1; i++)
        {
            if (IsAirClear(location, i * returningIntoAirIntervalDistance))
            {
                Vector3 toReturn = location;
                toReturn.y += i * returningIntoAirIntervalDistance;
                if (!AreObstaclesInWay(toReturn, startingPosition))
                {
                    return toReturn;
                }
            }
            else
            {
                return new Vector3();
            }
        }
        return new Vector3();
    }

    //Populates return path and sets state to complex route
    private void GenerateStraightPath(int pathTakenIndexTarget)
    {
        returnPath = new List<Vector3>();
        if (!AreObstaclesInWay(pathTaken[pathTakenIndexTarget]))
        {
            returnPath.Add(pathTaken[pathTakenIndexTarget]);
            enemyReturnPathState = ReturnPath.ComplexRoute;
            GenerateDebugPath();
            return;
        }

        for (int j = pathTaken.Count - 1; j >= pathTakenIndexTarget && enemyReturnPathState == ReturnPath.NotDetermined; j--)
        {
            returnPath.Add(pathTaken[j]);
            if (j < pathTaken.Count - 1 && !AreObstaclesInWay(pathTaken[j], pathTaken[pathTakenIndexTarget]))
            {
                returnPath.Add(pathTaken[pathTakenIndexTarget]);
                enemyReturnPathState = ReturnPath.ComplexRoute;
                GenerateDebugPath();
                return;
            }
        }
    }

    //Populates return path and sets state to complex route and adds an air target for the end of the path
    private void GenerateAirPath(int pathTakenIndexTarget, Vector3 airTarget)
    {
        GenerateStraightPath(pathTakenIndexTarget);
        returnPath.Add(airTarget);
    }

    private bool IsAirClear(float distance)
    {
        Vector3 target = transform.position;
        target.y += distance;
        return !AreObstaclesInWay(target, distance);
    }

    private bool IsAirClear(Vector3 startingPos, float distance)
    {
        startingPos.y += distance;
        return !AreObstaclesInWay(startingPos, distance);
    }
    #endregion

    #region Obstacle Avoidance Functions
    //Checks if any sides of enemy are approaching an obstacle, and if so adjusts. Further checks if enemy is stuck, and if so attempts to see if it can get unstuck. If it can't, sets stuck = true
    private void ObstacleAvoidance()
    {
        if (!IsLeftClear())
        {
            if (!IsRightClear())
            {
                if (IsStuckLeft() && IsStuckRight())
                {
                    stuck = true;
                    if (!IsStuckTop())
                    {
                        //Check if whole body moved top would work and adjust if so
                        if (WouldUpShiftWork())
                        {
                            //Debug.Log("Stuck up shift attempted");
                            transform.position += transform.up * obstacleAvoidanceAdjustmentRate * transform.localScale.x * Time.deltaTime * stuckAvoidanceMultiplier;
                            stuckCount++;
                            return;
                        }
                    }
                    if (!IsStuckBottom())
                    {
                        //Check if whole body moved down would work and adjust if so
                        if (WouldDownShiftWork())
                        {
                            //Debug.Log("Stuck down shift attempted");
                            transform.position -= transform.up * obstacleAvoidanceAdjustmentRate * transform.localScale.x * Time.deltaTime * stuckAvoidanceMultiplier;
                            stuckCount++;
                            return;
                        }
                    }
                    return;
                }
            }
            else
            {
                transform.position += transform.right * obstacleAvoidanceAdjustmentRate * transform.localScale.x * Time.deltaTime;
            }
        }
        else if (!IsRightClear())
        {
            transform.position -= transform.right * obstacleAvoidanceAdjustmentRate * transform.localScale.x * Time.deltaTime;
        }
        else if (!IsCenterClear())
        {
            //for now just testing if it moves left if center bad, moves at larger rate than usual obstacle avoidance since the obstacle is in middle not on side
            transform.position -= transform.right * obstacleAvoidanceAdjustmentRate * 3 * transform.localScale.x * Time.deltaTime;
        }

        if (!IsTopClear())
        {
            if (!IsBottomClear())
            {
                if (IsStuckTop() && IsStuckBottom())
                {
                    stuck = true;
                    if (!IsStuckLeft())
                    {
                        //Check if whole body moved left would work and adjust if so
                        if (WouldLeftShiftWork())
                        {
                            //Debug.Log("Stuck left shift attempted");
                            transform.position -= transform.right * obstacleAvoidanceAdjustmentRate * transform.localScale.x * Time.deltaTime * stuckAvoidanceMultiplier;
                            stuckCount++;
                            return;
                        }
                    }
                    if (!IsStuckRight())
                    {
                        //Check if whole body moved right would work and adjust if so
                        if (WouldRightShiftWork())
                        {
                            //Debug.Log("Stuck right shift attempted");
                            transform.position += transform.right * obstacleAvoidanceAdjustmentRate * transform.localScale.x * Time.deltaTime * stuckAvoidanceMultiplier;
                            stuckCount++;
                            return;
                        }
                    }
                    return;
                }
            }
            else
            {
                transform.position -= transform.up * obstacleAvoidanceAdjustmentRate * transform.localScale.x * Time.deltaTime;
            }
        }
        else if (!IsBottomClear())
        {
            transform.position += transform.up * obstacleAvoidanceAdjustmentRate * transform.localScale.x * Time.deltaTime;
        }
        stuck = false;
    }

    private bool StuckOnSafeSpot()
    {
        //Swapped this to fail if ANY is stuck on safe
        //return (IsLeftStuckOnSafe() && IsRightStuckOnSafe()) || (IsTopStuckOnSafe() && IsBottomStuckOnSafe());
        return IsLeftStuckOnSafe() || IsRightStuckOnSafe() || IsTopStuckOnSafe() || IsBottomStuckOnSafe() || IsCenterStuckOnSafe();
    }

    private bool IsLeftStuckOnSafe()
    {
        return Physics.Raycast(leftSide.position, leftSide.forward, distanceToCheckForStuck, safeSpotOnlyMask);
    }

    private bool IsRightStuckOnSafe()
    {
        return Physics.Raycast(rightSide.position, rightSide.forward, distanceToCheckForStuck, safeSpotOnlyMask);
    }

    private bool IsTopStuckOnSafe()
    {
        return Physics.Raycast(topSide.position, topSide.forward, distanceToCheckForStuck, safeSpotOnlyMask);
    }

    private bool IsBottomStuckOnSafe()
    {
        return Physics.Raycast(transform.position, transform.forward, distanceToCheckForStuck, safeSpotOnlyMask);
    }

    private bool IsCenterStuckOnSafe()
    {
        return Physics.Raycast(center.position, transform.forward, distanceToCheckForStuck, safeSpotOnlyMask);
    }

    //For now deprecating these to just return true instead of checking it to see if it works better. With these, it would rarely actually become unstuck.
    //By forcing it to always be true it will just attempt to shift and see if it breaks free, but can get into some bad states still.
    private bool WouldLeftShiftWork()
    {
        return true;
        /*Vector3 up = topSide.position - transform.right * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        Vector3 left = leftSide.position - transform.right * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        Vector3 right = rightSide.position - transform.right * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        Vector3 down = transform.position - transform.right * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        return CheckAllShiftedSides(up, left, right, down);*/
    }

    private bool WouldRightShiftWork()
    {
        return true;
        /*Vector3 up = topSide.position + transform.right * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        Vector3 left = leftSide.position + transform.right * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        Vector3 right = rightSide.position + transform.right * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        Vector3 down = transform.position + transform.right * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        return CheckAllShiftedSides(up, left, right, down);*/
    }

    private bool WouldUpShiftWork()
    {
        return true;
        /*Vector3 up = topSide.position + transform.up * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        Vector3 left = leftSide.position + transform.up * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        Vector3 right = rightSide.position + transform.up * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        Vector3 down = transform.position + transform.up * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        return CheckAllShiftedSides(up, left, right, down);*/
    }

    private bool WouldDownShiftWork()
    {
        return true;
        /*Vector3 up = topSide.position - transform.up * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        Vector3 left = leftSide.position - transform.up * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        Vector3 right = rightSide.position - transform.up * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        Vector3 down = transform.position - transform.up * obstacleAvoidanceAdjustmentRate * Time.deltaTime * stuckAvoidanceMultiplier;
        return CheckAllShiftedSides(up, left, right, down);*/
    }

    private bool CheckAllShiftedSides(Vector3 up, Vector3 left, Vector3 right, Vector3 bottom)
    {
        return !Physics.Raycast(left, leftSide.forward, distanceToCheckForStuck, layerMask) &&
            !Physics.Raycast(right, rightSide.forward, distanceToCheckForStuck, layerMask) &&
            !Physics.Raycast(up, topSide.forward, distanceToCheckForStuck, layerMask) &&
            !Physics.Raycast(bottom, transform.forward, distanceToCheckForStuck, layerMask);
    }

    private bool IsLeftClear()
    {
        return !Physics.Raycast(leftSide.position, leftSide.forward, distanceToCheckForObstacles, layerMask);
    }

    private bool IsRightClear()
    {
        return !Physics.Raycast(rightSide.position, rightSide.forward, distanceToCheckForObstacles, layerMask);
    }

    private bool IsCenterClear()
    {
        return !Physics.Raycast(center.position, center.forward, distanceToCheckForObstacles, layerMask);
    }

    private bool IsBottomClear()
    {
        return !Physics.Raycast(transform.position, transform.forward, distanceToCheckForObstacles, layerMask);
    }

    private bool IsTopClear()
    {
        return !Physics.Raycast(topSide.position, topSide.forward, distanceToCheckForObstacles, layerMask);
    }

    //Used to check sides but with lower max distance than for obstacle avoidance
    private bool StuckCheck()
    {
        if ((IsStuckLeft() && IsStuckRight()) ||
            (IsStuckTop() && IsStuckBottom())) {
            return true;
        }
        return false;
    }

    private bool IsStuckLeft()
    {
        return Physics.Raycast(leftSide.position, leftSide.forward, distanceToCheckForStuck, layerMask);
    }

    private bool IsStuckRight()
    {
        return Physics.Raycast(rightSide.position, rightSide.forward, distanceToCheckForStuck, layerMask);
    }

    private bool IsStuckTop()
    {
        return Physics.Raycast(topSide.position, topSide.forward, distanceToCheckForStuck, layerMask);
    }

    private bool IsStuckBottom()
    {
        return Physics.Raycast(transform.position, transform.forward, distanceToCheckForStuck, layerMask);
    }

    //TODO: Maybe this should be thrown onto something instanced
    private bool AreObstaclesInWay(Vector3 target)
    {
        Vector3 dirToTarget = (target - fieldOfViewTransform.position).normalized;
        float dstToTarget = Vector3.Distance(fieldOfViewTransform.position, target);
        return Physics.Raycast(fieldOfViewTransform.position, dirToTarget, dstToTarget, layerMask);
    }

    private bool AreObstaclesInWay(Vector3 target, float distance)
    {
        Vector3 dirToTarget = (target - fieldOfViewTransform.position).normalized;
        return Physics.Raycast(fieldOfViewTransform.position, dirToTarget, distance, layerMask);
    }

    private bool AreObstaclesInWay(Vector3 startPoint, Vector3 target)
    {
        Vector3 dirToTarget = (target - startPoint).normalized;
        float dstToTarget = Vector3.Distance(startPoint, target);
        return Physics.Raycast(startPoint, dirToTarget, dstToTarget, layerMask);
    }
    #endregion
    
    //Generates debug objects (cubes) to show the enemy's return path
    private void GenerateDebugPath()
    {
        if (debugPathingCubeEnabled && debugCubeForPathing != null)
        {
            foreach (Vector3 v in returnPath)
            {
                GameObject go = Instantiate(debugCubeForPathing);
                go.transform.position = v;
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw the patrol route, if set
        if (patrolRoute != null)
        {
            var routePoints = patrolRoute.GetComponentsInChildren<PatrolPoint>();
            // Set the color of the patrol route
            Gizmos.color = Color.yellow;
            
            Vector3 previousPoint = routePoints[0].transform.position;
            // Draw the patrol route
            for (int i = 1; i < routePoints.Length; i++)
            {
                Gizmos.DrawLine(previousPoint, routePoints[i].transform.position);
                previousPoint = routePoints[i].transform.position;
            }
        }
    }
}