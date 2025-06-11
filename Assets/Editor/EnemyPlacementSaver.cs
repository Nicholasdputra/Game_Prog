using UnityEngine;
using UnityEditor;
using System.Linq;

public class EnemyPlacementSaver : EditorWindow
{
    public GameObject meleeEnemyPrefab;
    public GameObject rangedEnemyPrefab;
    private WaveListScriptableObject waveList;
    private int waveIndex = 0; // Add this field

    [MenuItem("Tools/Save Enemy Placements")]
    public static void ShowWindow()
    {
        GetWindow<EnemyPlacementSaver>("Save Enemy Placements");
    }

    void OnGUI()
    {
        waveList = (WaveListScriptableObject)EditorGUILayout.ObjectField("Wave List", waveList, typeof(WaveListScriptableObject), false);
        meleeEnemyPrefab = (GameObject)EditorGUILayout.ObjectField("Melee Enemy Prefab", meleeEnemyPrefab, typeof(GameObject), false);
        rangedEnemyPrefab = (GameObject)EditorGUILayout.ObjectField("Ranged Enemy Prefab", rangedEnemyPrefab, typeof(GameObject), false);

        if (waveList != null && waveList.waves.Length > 0)
        {
            waveIndex = EditorGUILayout.IntSlider("Wave Index", waveIndex, 0, waveList.waves.Length - 1);
        }

        if (GUILayout.Button($"Save Current Scene Enemies To Wave {waveIndex}"))
        {
            SavePlacements();
        }
    }

    void SavePlacements()
    {
        if (waveList == null)
        {
            Debug.LogError("Assign a WaveListScriptableObject!");
            return;
        }

        var enemies = FindObjectsOfType<EnemyScript>();
        var placements = enemies.Select(enemy =>
        {
            GameObject prefab = null;
            if (enemy is MeeleeEnemyScript)
                prefab = meleeEnemyPrefab;
            else if (enemy is RangedEnemyScript)
                prefab = rangedEnemyPrefab;

            return new EnemyPlacementInfo
            {
                enemy = prefab,
                position = enemy.transform.position,
                rotation = enemy.transform.rotation
            };
        }).ToArray();

        waveList.waves[waveIndex].enemies = placements;
        EditorUtility.SetDirty(waveList);
        Debug.Log($"Saved {placements.Length} enemies to wave {waveIndex}.");
        
    }
}