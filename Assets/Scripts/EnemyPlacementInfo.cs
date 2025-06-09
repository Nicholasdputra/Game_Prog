using UnityEngine;

[System.Serializable]
public class EnemyPlacementInfo
{
    public GameObject enemy;
    public Vector3 position;
    public Quaternion rotation = Quaternion.identity; // Optional: allow rotation
}