using UnityEngine;

[DefaultExecutionOrder(50)]
public class OverworldAmbience : MonoBehaviour
{
    private void Start()
    {
        RPGFantasyAmbience.ApplyToOverworld();
    }
}
