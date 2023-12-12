using UnityEngine;

public class URPSight : MonoBehaviour
{
    
    #region Variables

    [Header("Transform components for raycasting")]
    private Transform sightObjectTransform = null;

    [Header("Components for instantiating the sight mesh")]
    private Mesh sightMesh = null;
    private MeshFilter sightMeshFilter = null;

    /// <summary>
    /// The origin point of the FOV sight.
    /// </summary>
    private Vector3 origin = Vector3.zero;

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
    /// Limit angle value of lower visual field.
    /// </summary>
    private float startAngle = 0f;

    /// <summary>
    /// Limit angle value of upper visual field.
    /// </summary>
    private float endAngle = 0f;

    /// <summary>
    /// The interval value determining at what angle intervals raycasting will be checked.
    /// This value is set as the result of dividing 360 by rayCount.
    /// </summary>
    private float angleIncrease = 0f;

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

    /// <summary>
    /// An array containing vertices to construct triangles.
    /// </summary>
    private Vector3[] vertices = null;
    
    /// <summary>
    /// An array containing uv coordinates for generating the mesh.
    /// </summary>
    private Vector2[] uv = null;
    
    /// <summary>
    /// An array containing the indices of each vertex of the triangles to construct the mesh.
    /// </summary>
    private int[] triangles = null;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        sightObjectTransform = transform.GetChild(0);

        sightMeshFilter = sightObjectTransform.GetComponent<MeshFilter>();
        
        sightMesh = new();
        sightMeshFilter.mesh = sightMesh;

        layerMask = LayerMask.GetMask(TagAndLayer.Layer.WALL);

        angleIncrease = 360f / rayCount;

        vertices = new Vector3[rayCount + 1 + 1];
        uv = new Vector2[vertices.Length];
        triangles = new int[rayCount * 3];
    }

    private void Update()
    {
        DrawSightWithURP(Vector2.right);
    }

    #endregion Unity Events

    /// <summary>
    /// Generate a sight mesh based on the given direction.
    /// The mesh is constructed by connecting triangles to approximate a circle.
    /// The interval of the angle at which raycasting is checked changes according to the rayCount value.
    /// As the rayCount increases, the mesh becomes closer to a circle, but the computational cost also increases.
    /// </summary>
    /// <param name="direction">The direction the character is facing</param>
    private void DrawSightWithURP(Vector2 direction)
    {
        Vector3 origin = sightObjectTransform.position;

        // Set the FOV range according to the given direction
        float angle = Utilities.GetAngleFromVector(direction) - (fov * 0.5f);
        startAngle = angle;
        endAngle = angle + fov;

        // Set the origin of the FOV as the first vertex
        vertices[0] = sightObjectTransform.localPosition;

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

            vertex -= origin;
            vertices[vertexIndex] = vertex;

            // Set the vertex to construct the triangle
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
