using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemyData
{
    public string enemyType; // "Ranged" or "Melee"
    public bool isAlive;
    public float health;
    public Vector2 position;
}

[CreateAssetMenu(fileName = "PlayerDataSO", menuName = "ScriptableObjects/PlayerDataSO")]
public class PlayerDataSO : ScriptableObject
{
    public int[] timeLeftAtCompletion; // Time taken for each level in seconds
    public int[] finalScoreAtCompletion; // Score achieved in each level
    public int[] timeElapsed; // Time elapsed in each level in seconds
    public int[] currentScores; // Current scores in each level

    public int[] starsAchieved; // Stars earned in each level
    public bool[] levelCompleted; // Indicates if a level is completed (1 for completed, 0 for not completed)

    public Vector2[] positionInLevel; // Player's position in each level
    public List<EnemyData>[] enemiesInLevel; // List of enemies in each level, with their data
    
}