using System.Collections;
using UnityEngine;

public class HandlePracticeHelperTexts : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(AdjustUI());
    }

    private IEnumerator AdjustUI()
    {
        yield return new WaitUntil(() => ServerManager.instance != null);

        foreach (Transform child in transform)
            child.gameObject.SetActive(ServerManager.instance.isPracticeServer);
    }
}
