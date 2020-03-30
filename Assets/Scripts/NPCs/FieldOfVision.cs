using System;
using System.Collections;
using System.Collections.Generic;
using NPCs;
using NPCs.Components;
using UnityEngine;

public class FieldOfVision : MonoBehaviour
{
	[Header("General Settings")]
	public float viewRadius;
	[Range(0, 360)]
	public float viewAngle;
	public LayerMask targetMask;
	public LayerMask obstacleMask;

	[HideInInspector]
	public List<Transform> visibleTargets;

	[Header("Mesh Interaction Settings")]
	public float meshResolution;
	public int edgeResolveInterations;
	public float edgeDstThreshold;

	[Header("View Mesh Options")]
	public GameObject viewMeshObject;
	public Material defaultMaterial;
	public Material spottedMaterial;
	public float flashingSpeed;
	public float distanceFromViewPlaneMin = 2.5f;
	private Mesh viewMesh;
	private MeshFilter viewMeshFilter;
	private MeshRenderer viewMeshRenderer;

	private Chaser chaser;
	private bool targetsVisible = false;

	public delegate void TargetSpottedAction();
	public event TargetSpottedAction OnTargetLost;
	public event TargetSpottedAction OnTargetSpotted;

	public delegate void TargetsUpdatedAction(int numberTargets);
	public event TargetsUpdatedAction OnTargetsUpdated;

	private void Awake()
	{
		visibleTargets = new List<Transform>();
	}

	void Start()
	{
		chaser = GetComponent<Chaser>();

		viewMeshFilter = Utils.GetRequiredComponent<MeshFilter>(viewMeshObject);
		viewMesh = new Mesh { name = "View Mesh" };
		viewMeshFilter.mesh = viewMesh;

		viewMeshRenderer = Utils.GetRequiredComponent<MeshRenderer>(viewMeshObject);
		viewMeshRenderer.material = defaultMaterial;
	}

	private void LateUpdate()
	{
		FindVisibleTargets();
		UpdateViewColor();
		DrawFieldOfView();
	}

	void FindVisibleTargets()
	{
		visibleTargets.Clear();

		Plane viewPlane = new Plane(viewMeshObject.transform.up, viewMeshObject.transform.position);

		Collider[] targetsInViewRadius = Physics.OverlapSphere(viewMeshObject.transform.position, viewRadius, targetMask);

		// Check for all colliders within view radius
		for (int i = 0; i < targetsInViewRadius.Length; i++)
		{
			Transform target = targetsInViewRadius[i].transform;
			Vector3 dirToTarget = (target.position - viewMeshObject.transform.position).normalized;

			// Checks if collider is within the view angle
			if (Vector3.Angle(viewMeshObject.transform.forward, dirToTarget) < viewAngle / 2)
			{
				float dstToTarget = Vector3.Distance(viewMeshObject.transform.position, target.position);

				// Checks that a raycast can hit a target
				if (!Physics.Raycast(viewMeshObject.transform.position, dirToTarget, dstToTarget, obstacleMask) 
					&& Mathf.Abs(viewPlane.GetDistanceToPoint(target.position)) < distanceFromViewPlaneMin)
				{
					visibleTargets.Add(target);
				}
			}
		}

		OnTargetsUpdated?.Invoke(visibleTargets.Count);
	}

	void UpdateViewColor()
	{
		if (visibleTargets.Count == 0)
		{
			viewMeshRenderer.material = defaultMaterial;
			if (targetsVisible)
			{
				OnTargetLost?.Invoke();
			}
			targetsVisible = false;
		}
        else
		{
            viewMeshRenderer.material = spottedMaterial;

			viewMeshRenderer.material.SetColor("_BaseColor", Color.Lerp(
				Color.black,
				spottedMaterial.color,
				Mathf.PingPong(Time.time * flashingSpeed, 1f)
			));
			if (!targetsVisible)
			{
				OnTargetSpotted?.Invoke();
			}
			targetsVisible = true;
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
			float angle = viewMeshObject.transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
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
			vertices[i + 1] = viewMeshObject.transform.InverseTransformPoint(viewPoints[i]);

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

        if (Physics.Raycast(viewMeshObject.transform.position, direction, out hit, viewRadius, obstacleMask))
		{
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
		}
        else
		{
			return new ViewCastInfo(false, viewMeshObject.transform.position + direction * viewRadius, viewRadius, globalAngle);
		}
	}

	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
	{
		if (!angleIsGlobal)
		{
			angleInDegrees += viewMeshObject.transform.eulerAngles.y;
		}
		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), -viewMeshObject.transform.eulerAngles.x * Mathf.Deg2Rad, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
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
