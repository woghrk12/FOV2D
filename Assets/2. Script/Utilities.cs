using UnityEngine;

public class Utilities
{
    public static Vector3 GetVectorFromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);

        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static float GetAngleFromVector(Vector2 direction)
    {
        direction = direction.normalized;
        float degree = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        return degree < 0f ? degree + 360 : degree;
    }
}
