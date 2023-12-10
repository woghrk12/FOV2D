using UnityEngine;

public class URPSight : MonoBehaviour
{
    
    #region Variables

    private Transform cachedTransform = null;
    private Transform sightObjectTransform = null;

    private Mesh sightMesh = null;
    private MeshFilter sightMeshFilter = null;

    private float fov = 90f;

    private int rayCount = 360;

    private float viewDistance = 8f;
    private float surroundingDistance = 2f;

    private LayerMask layerMask;

    #endregion Variables

    #region URP Sight

    #endregion URP Sight

    private void Awake()
    {
        cachedTransform = transform;
        sightObjectTransform = cachedTransform.GetChild(0);

        sightMeshFilter = sightObjectTransform.GetComponent<MeshFilter>();
        
        sightMesh = new();
        sightMeshFilter.mesh = sightMesh;

        layerMask = LayerMask.GetMask(TagAndLayer.Layer.WALL);
    }

    private void Update()
    {
        DrawSightWithURP(Vector2.right);
    }

    private void DrawSightWithURP(Vector2 direction)
    {
        Vector3 position = cachedTransform.position;
        Vector3 origin = position + sightObjectTransform.localPosition;
        
        float startAngle, endAngle, angle;
        startAngle = angle = Utilities.GetAngleFromVector(direction) - (fov * 0.5f);
        endAngle = startAngle + fov;
        
        float angleIncrease = 360f / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin - position;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int index = 0; index <= rayCount; index++)
        {
            Vector3 vertex = Vector3.zero;
            Vector3 rayDirection = Utilities.GetVectorFromAngle(angle);
            float viewDistance = angle < startAngle || angle > endAngle ? surroundingDistance : this.viewDistance;
            RaycastHit2D hitObject = Physics2D.Raycast(origin, rayDirection, viewDistance, layerMask);

            if (hitObject)
            {
                vertex = hitObject.point;
            }
            else
            {
                vertex = origin + rayDirection * viewDistance;
            }

            vertex -= position;
            vertices[vertexIndex] = vertex;

            if (index > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex] = vertexIndex;
                triangleIndex += 3;
            }

            vertexIndex++;
            angle += angleIncrease;
        }

        sightMesh.vertices = vertices;
        sightMesh.uv = uv;
        sightMesh.triangles = triangles;
    }
}
