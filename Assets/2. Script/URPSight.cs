using UnityEngine;

public class URPSight : MonoBehaviour
{
    
    #region Variables

    [Header("Transform components for raycasting")]
    private Transform cachedTransform = null;
    private Transform sightObjectTransform = null;

    [Header("Components for instantiating the sight mesh")]
    private Mesh sightMesh = null;
    private MeshFilter sightMeshFilter = null;

    /// <summary>
    /// Field of view value.
    /// </summary>
    private float fov = 90f;

    /// <summary>
    /// The variable determining how much to check for raycasting.
    /// In other words, it determines at what angle intervals raycasting will be checked.
    /// If the value is 360, which is default value, it will check raycasting every 1 degree.
    /// </summary>
    private int rayCount = 360;

    /// <summary>
    /// The max distance of raycasting within the FOV range.
    /// </summary>
    private float viewDistance = 8f;
    
    /// <summary>
    /// The max distance of raycasting outside the FOV range.
    /// </summary>
    private float surroundingDistance = 2f;

    /// <summary>
    /// The layer mask for walls.
    /// The sight does not reach behind walls.
    /// </summary>
    private LayerMask layerMask;

    #endregion Variables

    #region Unity Events

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

    #endregion Unity Events

    /// <summary>
    /// Generate a sight mesh based on the given direction.
    /// The mesh is formed by connecting triangles to approximate a circle.
    /// The interval of the angle at which raycasting is checked changes according to the rayCount value.
    /// As the rayCount increases, the mesh becomes closer to a circle, but the computational cost also increases.
    /// </summary>
    /// <param name="direction">The direction the character is facing</param>
    private void DrawSightWithURP(Vector2 direction)
    {
        Vector3 position = cachedTransform.position;
        Vector3 origin = position + sightObjectTransform.localPosition;
        
        float startAngle, endAngle, angle;
        startAngle = angle = Utilities.GetAngleFromVector(direction) - (fov * 0.5f);
        endAngle = startAngle + fov;
        
        // Calculate the interval of the angle
        float angleIncrease = 360f / rayCount;

        // Initialize the arrays to construct triangles
        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        // Set the origin of the FOV as the first vertex
        vertices[0] = origin - position;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int index = 0; index <= rayCount; index++)
        {
            Vector3 vertex = Vector3.zero;
            Vector3 rayDirection = Utilities.GetVectorFromAngle(angle);
            float viewDistance = angle < startAngle || angle > endAngle ? surroundingDistance : this.viewDistance;
            RaycastHit2D hitObject = Physics2D.Raycast(origin, rayDirection, viewDistance, layerMask);

            // if there is a wall within the range
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

            // Set the vertex to form the triangle
            if (index > 0)
            {
                triangles[triangleIndex++] = 0;
                triangles[triangleIndex++] = vertexIndex - 1;
                triangles[triangleIndex++] = vertexIndex;
            }

            vertexIndex++;
            angle += angleIncrease;
        }

        sightMesh.vertices = vertices;
        sightMesh.uv = uv;
        sightMesh.triangles = triangles;
    }
}
