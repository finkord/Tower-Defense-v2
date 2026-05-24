using UnityEngine;
using System.Collections;

public class DestroyAfterTime : MonoBehaviour
{
    public float time = 1f;

    void OnEnable()
    {
        StartCoroutine(DisableRoutine());
    }

    private IEnumerator DisableRoutine()
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
}
