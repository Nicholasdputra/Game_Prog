using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSaveDataSO", menuName = "SaveData/PlayerSaveDataSO")]
public class PlayerSaveDataSO : ScriptableObject
{
    public int score;
    public int wavesCompleted;
    public int enemiesDefeated;
    public int timeSurvived;
}