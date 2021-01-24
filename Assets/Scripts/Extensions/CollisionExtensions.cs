using UnityEngine;

public static class CollisionExtensions {

    /// <summary>
    /// Returns force in Newtons
    /// </summary>
    public static Vector3 GetForce (this Collision collision) {
        return collision.impulse / Time.fixedDeltaTime;
    }

    public static Vector3 GetAveragePoint (this Collision collision) {
        Vector3 averagePoint = Vector3.zero;
        int cc = collision.contactCount;
        for(int i=0; i<cc; i++){
            averagePoint += collision.GetContact(i).point;
        }
        return averagePoint / collision.contacts.Length;
    }

    public static Vector3 GetAverageNormal (this Collision collision) {
        Vector3 averageNormal = Vector3.zero;
        int cc = collision.contactCount;
        for(int i=0; i<cc; i++){
            averageNormal += collision.GetContact(i).normal;
        }
        return averageNormal.normalized;
    }

    public static void GetAveragePointAndNormal (this Collision collision, out Vector3 averagePoint, out Vector3 averageNormal) {
        averagePoint = Vector3.zero;
        averageNormal = Vector3.zero;
        int cc = collision.contactCount;
        for(int i=0; i<cc; i++){
            averagePoint += collision.GetContact(i).point;
            averageNormal += collision.GetContact(i).normal;
        }
        averagePoint /= collision.contacts.Length;
        averageNormal = averageNormal.normalized;
    }

}
