using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    public float viewRadius;
	public float viewAngle;

    public Vector3 DirFromAngle(float angleInDegrees)
	{
		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
	}

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
  //      Vector3 leftBound = DirFromAngle(angle + 90 + (viewAngle) / 2, false) * range * wayFacing.x;
  //      Vector3 rightBound = DirFromAngle(angle + 90 - (viewAngle) / 2, false) * range * wayFacing.x;

		//Debug.DrawRay(transform.position, DirFromAngle(angle + 90 + (viewAngle) / 2, false) * range * wayFacing.x);
		//Debug.DrawRay(transform.position, DirFromAngle(angle + 90 - (viewAngle) / 2, false) * range * wayFacing.x);
	}


	//void DrawMesh()
	//{
	//	//int stepcount = Mathf.RoundToInt (viewAngle * meshresolution);
	//	visibleTargets.Clear();
	//	float stepAngleSize = viewAngle / (numVertex + 1);
	//	List<Vector3> viewPoints = new List<Vector3>();

	//	for (int i = 0; i < numVertex; i++)
	//	{
	//		RaycastHit2D hit;
	//		Vector3 dir = DirFromAngle(angle + 90 + (viewAngle - (i + 1) * stepAngleSize * 2) / -2, false) * range * wayFacing.x;

	//		if (!(hit = Physics2D.Raycast(transform.position, dir, range, obstacleMask)))
	//		{
	//			//	Debug.DrawRay (transform.position, dir, Color.red);
	//			viewPoints.Add(dir);
	//		}
	//		else
	//		{
	//			//	Debug.DrawRay (transform.position, DirFromAngle (angle + 90+(viewAngle - (i + 1) * stepAngleSize * 2) / -2, false) * wayFacing.x * hit.distance, Color.red);
	//			viewPoints.Add(DirFromAngle(angle + 90 + (viewAngle - (i + 1) * stepAngleSize * 2) / -2, false) * wayFacing.x * hit.distance);
	//			if (hit.transform.name == "Player" && !GAME.Player.GetComponent<PlayerMovement>().crouched)
	//			{
	//				if (!visibleTargets.Contains(hit.transform))
	//				{
	//					visibleTargets.Add(hit.transform);
	//				}
	//			}
	//		}
	//	}

	//	int vertexCount = viewPoints.Count + 1;
	//	Vector3[] vertices = new Vector3[vertexCount];
	//	int[] triangles = new int[(vertexCount - 2) * 3];

	//	vertices[0] = Vector3.zero;
	//	for (int i = 0; i < vertexCount - 1; i++)
	//	{


	//		vertices[i + 1] = new Vector3(viewPoints[i].x, viewPoints[i].y, 0);

	//		if (i < vertexCount - 2)
	//		{
	//			triangles[i * 3] = 0;
	//			triangles[i * 3 + 1] = i + 1;
	//			triangles[i * 3 + 2] = i + 2;
	//		}
	//	}

	//	viewMesh.Clear();
	//	viewMesh.vertices = vertices;
	//	viewMesh.triangles = triangles;
	//	viewMesh.RecalculateBounds();
	//	viewMesh.RecalculateNormals();
	//}
}
