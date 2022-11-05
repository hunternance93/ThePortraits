using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

//Original version of script from: https://github.com/SebLague/Field-of-View.
//Script determines if there is a Player in view, and if so attempts to switch enemy state to Chasing it

public class FieldOfView : MonoBehaviour {

	[Tooltip("Reference to the AI script on this enemy")]
	[SerializeField] private EnemyAI thisAI = null;

	[SerializeField] private bool doNotAdjustHeight = false;

	//Values from original version of script
	public float viewRadius;
	[Range(0, 360)]
	public float viewAngle;
	[Range(0, 360)]
	public float viewAngleHeight = 30;
	[Range(0, 360)]
	public float viewAngleHeightTarget = 90;
	public bool debug = false;
	[SerializeField] private LayerMask targetMask;
	[SerializeField] private LayerMask obstacleMask;
	[HideInInspector]
	public List<Transform> visibleTargets = new List<Transform>();
	[SerializeField] private float meshResolution;
	[SerializeField] private int edgeResolveIterations;
	[SerializeField] private float edgeDstThreshold;

	[Tooltip("Whether or not this enemy should detect player through all obstacles if they're within minimum distance")]
	[SerializeField] private bool proximityDetection = true;
	[Tooltip("If target is within this distance then enemy will detect it regardless of raycasts or anything else")]
	public float minDistance = 0;

	[Tooltip("Only detect player if they are standing in light (for angler fish)")]
	public bool onlyDetectIfPlayerLit = false;

	//Optional, used to view angle of field of view. Will not show height
	public MeshFilter viewMeshFilter;
	Mesh viewMesh;

	private const float DELAY_TIME = .2f;

	private float timer = 0; //Used to not check sight every frame, instead waits DELAY_TIME

	private float timerForNonDetect = 0;
	private float timeForNonDetectDelay = 0;

	void Start() {
		if (viewMeshFilter != null)
		{
			viewMesh = new Mesh();
			viewMesh.name = "View Mesh";
			viewMeshFilter.mesh = viewMesh;
		}

		StartCoroutine ("FindTargetsWithDelay", DELAY_TIME);

		if (thisAI == null)
        {
			EnemyAI ai = GetComponentInParent<EnemyAI>();
			if (ai != null) thisAI = ai;
        }
	}

	public void TemporarilyIgnorePlayer(float time)
    {
		timerForNonDetect = 0;
		timeForNonDetectDelay = time;
	}

	IEnumerator FindTargetsWithDelay(float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(delay);
			FindVisibleTargets();
		}
	}

	private void Update()
    {
		if (timeForNonDetectDelay > 0)
		{
			timerForNonDetect += Time.deltaTime;
		}

		if (visibleTargets.Count > 0)
        {
			timer += Time.deltaTime;
			if (timer >= DELAY_TIME)
			{
				timer = 0;

			    if (thisAI != null && timerForNonDetect >= timeForNonDetectDelay)
				{
					//TODO: Maybe make it so it can see you normally after it sees you initially unless you break LOS or something?
					if (onlyDetectIfPlayerLit)
                    {
						if (!GameManager.instance.Player.isInLight) return;
                    }
					timeForNonDetectDelay = 0;
					timerForNonDetect = 0;
					thisAI.SetNewTargetPos();
					thisAI.SetState(EnemyAI.EnemyState.Chasing);
				}
			}
        }
    }

	void LateUpdate() {
		DrawFieldOfView ();
	}

	void FindVisibleTargets() {
		visibleTargets.Clear ();
		Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, targetMask);

		//If player is within the radius of the enemy's vision sphere...
		for (int i = 0; i < targetsInViewRadius.Length; i++) {
			Transform target = targetsInViewRadius [i].transform;
			//Adjust target to have same height(y) to make angle not affected by height
			Vector3 adjustedTarget;
			if (doNotAdjustHeight) adjustedTarget = target.transform.position;
			else adjustedTarget = new Vector3(target.position.x, transform.position.y, target.position.z);
			Vector3 dirToTargetAdjusted = (adjustedTarget - transform.position).normalized;
			//If player (ignoring height(y)) is within min distance, then they should be seen regardless of where enemy is looking. If proximity detection is off or not currently searching do not do this.
			if (proximityDetection && (thisAI.GetState() == EnemyAI.EnemyState.Searching || thisAI.GetState() == EnemyAI.EnemyState.SearchingLastKnownArea) && Vector3.Distance(transform.position, adjustedTarget) < minDistance)
			{
				Debug.Log("Less than minimum distance!");
				visibleTargets.Add(target);
			}
			else
			{
				if (thisAI.IsCurrentlySleeping) return;
				//If player is within the viewing angle of enemy (player transform adjusted for height, treat them as on same plane for this)
				if (Vector3.Angle(transform.forward, dirToTargetAdjusted) < viewAngle / 2)
				{
					//TODO: I am certain this is not a good solution but it improves vertical oriented enemies that are setup to use it for now (like the rotating Meio tower raise/lower enemy)
					if (!doNotAdjustHeight || (thisAI.GetState() != EnemyAI.EnemyState.Searching || Mathf.Abs(viewAngleHeightTarget - Vector3.Angle(transform.up, dirToTargetAdjusted)) < viewAngleHeight))
					{
						//If player left side, right side, feet or head has no obstacles in the way
						if (!AreObstaclesInWay(GameManager.instance.Player.transform.position) || !AreObstaclesInWay(GameManager.instance.Player.PlayerLeftSide.position)
							|| !AreObstaclesInWay(GameManager.instance.Player.PlayerRightSide.position) || !AreObstaclesInWay(GameManager.instance.Player.PlayerHead.position))
						{

							visibleTargets.Add(target);
						}
					}
				}
			}
		}
	}

	//TODO: Maybe make this less redundant by keeping it in a common place since EnemyAI uses a similar function
	private bool AreObstaclesInWay(Vector3 target)
    {
		Vector3 dirToTarget = (target - transform.position).normalized;
		float dstToTarget = Vector3.Distance(transform.position, target);
		return Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask);
	}

	//TODO: Determine if any of this region is actually needed
    #region MeshDrawingCode

    void DrawFieldOfView() {
		int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
		float stepAngleSize = viewAngle / stepCount;
		List<Vector3> viewPoints = new List<Vector3> ();
		ViewCastInfo oldViewCast = new ViewCastInfo ();
		for (int i = 0; i <= stepCount; i++) {
			float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
			ViewCastInfo newViewCast = ViewCast (angle);

			if (i > 0) {
				bool edgeDstThresholdExceeded = Mathf.Abs (oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
				if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded)) {
					EdgeInfo edge = FindEdge (oldViewCast, newViewCast);
					if (edge.pointA != Vector3.zero) {
						viewPoints.Add (edge.pointA);
					}
					if (edge.pointB != Vector3.zero) {
						viewPoints.Add (edge.pointB);
					}
				}

			}


			viewPoints.Add (newViewCast.point);
			oldViewCast = newViewCast;
		}

		int vertexCount = viewPoints.Count + 1;
		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[(vertexCount-2) * 3];

		vertices [0] = Vector3.zero;
		for (int i = 0; i < vertexCount - 1; i++) {
			vertices [i + 1] = transform.InverseTransformPoint(viewPoints [i]);

			if (i < vertexCount - 2) {
				triangles [i * 3] = 0;
				triangles [i * 3 + 1] = i + 1;
				triangles [i * 3 + 2] = i + 2;
			}
		}

		if (viewMesh != null)
		{
			viewMesh.Clear();

			viewMesh.vertices = vertices;
			viewMesh.triangles = triangles;
			viewMesh.RecalculateNormals();
		}
	}


	EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
		float minAngle = minViewCast.angle;
		float maxAngle = maxViewCast.angle;
		Vector3 minPoint = Vector3.zero;
		Vector3 maxPoint = Vector3.zero;

		for (int i = 0; i < edgeResolveIterations; i++) {
			float angle = (minAngle + maxAngle) / 2;
			ViewCastInfo newViewCast = ViewCast (angle);

			bool edgeDstThresholdExceeded = Mathf.Abs (minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
			if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded || newViewCast.hit != minViewCast.hit && edgeDstThresholdExceeded)
			{
				minAngle = angle;
				minPoint = newViewCast.point;
			} else {
				maxAngle = angle;
				maxPoint = newViewCast.point;
			}
		}

		return new EdgeInfo (minPoint, maxPoint);
	}


	ViewCastInfo ViewCast(float globalAngle) {
		Vector3 dir = DirFromAngle (globalAngle, true);
		RaycastHit hit;

		if (Physics.Raycast (transform.position, dir, out hit, viewRadius, obstacleMask)) {
			return new ViewCastInfo (true, hit.point, hit.distance, globalAngle);
		} else {
			return new ViewCastInfo (false, transform.position + dir * viewRadius, viewRadius, globalAngle);
		}
	}

	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
		if (!angleIsGlobal) {
			angleInDegrees += transform.eulerAngles.y;
		}
		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),0,Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
	}

	public struct ViewCastInfo {
		public bool hit;
		public Vector3 point;
		public float dst;
		public float angle;

		public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle) {
			hit = _hit;
			point = _point;
			dst = _dst;
			angle = _angle;
		}
	}

	public struct EdgeInfo {
		public Vector3 pointA;
		public Vector3 pointB;

		public EdgeInfo(Vector3 _pointA, Vector3 _pointB) {
			pointA = _pointA;
			pointB = _pointB;
		}
	}
    #endregion

}