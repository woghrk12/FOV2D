using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMaskSight : MonoBehaviour
{
    #region Variables

    private Camera mainCamera = null;
    private Vector3 direction = Vector3.right;

    [Header("Transform component for raycasting")]
    private Transform sightObjectTransform = null;

    [Header("Components for sprite mask")]
    private SpriteMask spriteMask = null;
    private Texture2D maskTexture = null;
    private Vector2 maskCenter = Vector2.zero;
    private float maskRadius = 0f;

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
    private Vector2[] vertices = null;

    /// <summary>
    /// An array containing the indices of each vertex of the triangles to override the geometry of the sprite.
    /// </summary>
    private ushort[] triangles = null;

    #endregion Variables

    #region Unity Events

    private void Awake()
    {
        sightObjectTransform = transform.GetChild(0);

        spriteMask = sightObjectTransform.GetComponent<SpriteMask>();
        maskTexture = Resources.Load("Sprite/Circle") as Texture2D;
        maskRadius = maskTexture.width * 0.5f;
        maskCenter = new Vector2(maskRadius, maskRadius);

        layerMask = LayerMask.GetMask(TagAndLayer.Layer.WALL);

        angleIncrease = 360f / rayCount;

        vertices = new Vector2[rayCount + 1 + 1];
        triangles = new ushort[rayCount * 3];
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            direction = (mainCamera.ScreenToWorldPoint(Input.mousePosition) - sightObjectTransform.position).normalized;
        }

        DrawSightWithSpriteMask(direction);
    }

    #endregion Unity Events

    /// <summary>
    /// Override the geometry of the texture to be used as the sprite mask.
    /// The texture is constructed by connecting triangles to approximate a circle.
    /// The interval of the angle at which raycasting is checked changes according to the rayCount value.
    /// As the rayCount increases, the texture becomes closer to a circle, but the computational cost also increases.
    /// </summary>
    /// <param name="direction"></param>
    private void DrawSightWithSpriteMask(Vector2 direction)
    {
        origin = sightObjectTransform.position;

        // Set the FOV range according to the given direction
        float angle = Utilities.GetAngleFromVector(direction) - (fov * 0.5f);
        startAngle = angle;
        endAngle = angle + fov;

        // Set the origin of the FOV as the first vertex
        vertices[0] = maskCenter;

        ushort vertexIndex = 1;
        ushort triangleIndex = 0;
        for (int index = 0; index <= rayCount; index++)
        {
            Vector3 vertex = Vector3.zero;
            Vector3 rayDirection = Utilities.GetVectorFromAngle(angle);
            float viewDistance = angle < startAngle || angle > endAngle ? surroundingDistance : this.viewDistance;

            RaycastHit2D hitObject = Physics2D.Raycast(origin, rayDirection, viewDistance, layerMask);

            // if there is a wall within the raycasting range
            if (hitObject)
            {
                vertex = hitObject.point;
            }
            else
            {
                vertex = origin + rayDirection * viewDistance;
            }

            // Convert the vertex position to match the texture size
            vertex -= origin;
            vertices[vertexIndex] = (Vector2)(vertex / this.viewDistance * maskRadius) + maskCenter;

            if (index > 0)
            {
                triangles[triangleIndex++] = 0;
                triangles[triangleIndex++] = (ushort)(vertexIndex - 1);
                triangles[triangleIndex++] = vertexIndex;
            }

            vertexIndex++;
            angle += angleIncrease;
        }

        spriteMask.sprite.OverrideGeometry(vertices, triangles);
    }
}
