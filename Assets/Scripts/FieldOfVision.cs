using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfVision : MonoBehaviour
{
	public float viewRadius;
	[Range(0, 360)]
	public float viewAngle;

	public LayerMask targetMask;
	public LayerMask obstacleMask;

	[HideInInspector]
	public List<Transform> visibleTargets = new List<Transform>();

	public float meshResolution;
	public int edgeResolveInterations;
	public float edgeDstThreshold;

	public MeshFilter viewMeshFilter;

	public NursePatrol nursePatrol;

	Mesh viewMesh;

	void Start()
	{
		viewMesh = new Mesh();
		viewMesh.name = "View Mesh";
		viewMeshFilter.mesh = viewMesh;
		StartCoroutine("FindTargetsWithDelay", .2f);

		nursePatrol = GetComponent<NursePatrol>();
	}

    void LateUpdate ()
	{
		DrawFieldOfView();
	}


	IEnumerator FindTargetsWithDelay(float delay)
	{
		while (true)
		{
			yield return new WaitForSeconds(delay);
			FindVisibleTargets();
		}
	}


	void FindVisibleTargets()
	{
		visibleTargets.Clear();
		Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        // Check for all colliders within view radius
		for (int i = 0; i < targetsInViewRadius.Length; i++)
		{
			Transform target = targetsInViewRadius[i].transform;
			Vector3 dirToTarget = (target.position - transform.position).normalized;

            // Checks if collider is within the view angle
			if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
			{
				float dstToTarget = Vector3.Distance(transform.position, target.position);

                // Checks that a raycast can hit a target
				if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
				{
					visibleTargets.Add(target);
					Debug.Log("Found Target!");
                    // Code for finding target here
					if (nursePatrol.chase)
					{
						Debug.Log("Chase Target!");
						nursePatrol.ChaseTarget();
					}
				}
			}
		}
	}

    //Draws Mesh
    void DrawFieldOfView()
	{
		int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
		float stepAngleSize = viewAngle / stepCount;
		List<Vector3> viewPoints = new List<Vector3>();
		ViewCastInfo oldViewCast = new ViewCastInfo(); 

        for (int i = 0; i <= stepCount; i++)
		{
			float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
			ViewCastInfo newViewCast = ViewCast(angle);

			if (i > 0)
			{
				bool edgeDstThresholdExceeded = Mathf.Abs(oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
                if(oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded))
				{
					EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if(edge.pointA != Vector3.zero)
					{
						viewPoints.Add(edge.pointA);
					}
                    if(edge.pointB != Vector3.zero)
					{
						viewPoints.Add(edge.pointB);
					}
				}
			}

			viewPoints.Add(newViewCast.point);
			oldViewCast = newViewCast;
		}


        // Vertices to draw all triangles for each mesh
		int vertexCount = viewPoints.Count + 1;
		Vector3[] vertices = new Vector3[vertexCount];
		int[] triangles = new int[(vertexCount - 2) * 3];

		vertices[0] = Vector3.zero;
        for (int i=0; i< vertexCount-1; i++)
		{
			vertices[i + 1] = transform.InverseTransformPoint(viewPoints[i]);

            if(i < vertexCount - 2)
			{
				triangles[i * 3] = 0;
				triangles[i * 3 + 1] = i + 1;
				triangles[i * 3 + 2] = i + 2;
			}
		}

		viewMesh.Clear();
		viewMesh.vertices = vertices;
		viewMesh.triangles = triangles;
		viewMesh.RecalculateNormals();

	}

    //Used to smooth field view by locating edge of raycast
    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo MaxViewCast)
	{
		float minAngle = minViewCast.angle;
		float maxAngle = MaxViewCast.angle;
		Vector3 minPoint = Vector3.zero;
		Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i<edgeResolveInterations; i++)
		{
			float angle = (minAngle + maxAngle) / 2;
			ViewCastInfo newViewCast = ViewCast(angle);

			bool edgeDstThresholdExceeded = Mathf.Abs(minViewCast.dst - newViewCast.dst) > edgeDstThreshold;

			if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded)
			{
				minAngle = angle;
				minPoint = newViewCast.point;
			}
            else
			{
				maxAngle = angle;
				maxPoint = newViewCast.point;
			}
		}

		return new EdgeInfo(minPoint, maxPoint);
	}

    ViewCastInfo ViewCast(float globalAngle)
	{
        Vector3 direction = DirFromAngle(globalAngle, true);
		RaycastHit hit;

        if (Physics.Raycast(transform.position, direction, out hit, viewRadius, obstacleMask))
		{
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
		}
        else
		{
			return new ViewCastInfo(false, transform.position + direction * viewRadius, viewRadius, globalAngle);
		}
	}

	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
	{
		if (!angleIsGlobal)
		{
			angleInDegrees += transform.eulerAngles.y;
		}
		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
	}

    public struct ViewCastInfo
	{
		public bool hit;
		public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
		{
			hit = _hit;
			point = _point;
			dst = _dst;
			angle = _angle;
		}
	}

    public struct EdgeInfo
	{
		public Vector3 pointA;
		public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
		{
			pointA = _pointA;
			pointB = _pointB;
		}
	}
}
