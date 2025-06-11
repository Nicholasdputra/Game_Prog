using UnityEngine;

[CreateAssetMenu(menuName = "Wave List")]
public class WaveListScriptableObject : ScriptableObject
{
    public WaveData[] waves;
}