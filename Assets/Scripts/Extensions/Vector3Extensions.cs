using UnityEngine;

public static class Vector3Extensions {

    public static Vector3 ProjectOnVector (this Vector3 vector, Vector3 normal) {
        return Vector3.Project(vector, normal);
    }

    public static Vector3 ProjectOnPlane (this Vector3 vector, Vector3 planeNormal) {
        return Vector3.ProjectOnPlane(vector, planeNormal);
    }

    public static Vector3 ProjectOnPlaneAlongVector (this Vector3 vector, Vector3 planeNormal, Vector3 projectVector) {
        float x = Vector3.Dot(planeNormal, vector) / Vector3.Dot(planeNormal, projectVector);
        return (vector - (x * projectVector));
    }

    public static float Average (this Vector3 vector) {
        return (vector.x + vector.y + vector.z) / 3f;
    }

    public static Vector2 XY (this Vector3 vector) {
        return new Vector2(vector.x, vector.y);
    }

    public static Vector2 XZ (this Vector3 vector) {
        return new Vector2(vector.x, vector.z);
    }

    public static Vector2 YX (this Vector3 vector) {
        return new Vector2(vector.y, vector.x);
    }

    public static Vector2 YZ (this Vector3 vector) {
        return new Vector2(vector.y, vector.z);
    }

    public static Vector2 ZX (this Vector3 vector) {
        return new Vector2(vector.z, vector.x);
    }

    public static Vector2 ZY (this Vector3 vector) {
        return new Vector2(vector.z, vector.y);
    }
	
}
