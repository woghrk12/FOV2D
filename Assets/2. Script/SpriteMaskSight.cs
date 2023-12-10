using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMaskSight : MonoBehaviour
{
    #region Variables

    private Transform cachedTransform = null;
    private Transform sightObjectTransform = null;

    private SpriteMask spriteMask = null;

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

        spriteMask = sightObjectTransform.GetComponent<SpriteMask>();

        layerMask = LayerMask.GetMask(TagAndLayer.Layer.WALL);
    }

    private void Update()
    {
        DrawSightWithSpriteMask(Vector2.right);
    }

    private void DrawSightWithSpriteMask(Vector2 direction)
    {
        Vector2 origin = cachedTransform.position + sightObjectTransform.localPosition;

        float startAngle, endAngle, angle;
        startAngle = angle = Utilities.GetAngleFromVector(direction) - (fov * 0.5f);
        endAngle = startAngle + fov;

        float angleIncrease = 360f / rayCount;

        Vector2[] vertices = new Vector2[rayCount + 1 + 1];
        ushort[] triangles = new ushort[rayCount * 3];

        vertices[0] = new Vector2(128f, 128f);

        ushort vertexIndex = 1;
        ushort triangleIndex = 0;
        for (int index = 0; index <= rayCount; index++)
        {
            Vector2 vertex = Vector3.zero;
            Vector2 rayDirection = Utilities.GetVectorFromAngle(angle);
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

            vertex -= origin;
            vertices[vertexIndex] = vertex / this.viewDistance * 128f + new Vector2(128f, 128f);

            if (index > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = (ushort)(vertexIndex - 1);
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }

            vertexIndex++;
            angle += angleIncrease;
        }

        spriteMask.sprite.OverrideGeometry(vertices, triangles);
    }
}
