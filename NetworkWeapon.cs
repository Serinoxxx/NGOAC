using UnityEngine;

[ExecuteInEditMode]
public class NetworkWeapon : MonoBehaviour
{
    [SerializeField] bool randomizeID;

    [ExecuteInEditMode]
    void OnEnable()
    {
        if (networkID == -1 && randomizeID) networkID = Random.Range(0, 1000000);
    }
    public int networkID;
}
