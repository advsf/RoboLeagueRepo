using UnityEngine;
using System.Collections;

public class HandleGameHelperTexts : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(AdjustUI());
    }

    private IEnumerator AdjustUI()
    {
        yield return new WaitUntil(() => ServerManager.instance != null);

        gameObject.SetActive(!ServerManager.instance.isPracticeServer && !ServerManager.instance.isTutorialServer);
    }

    private void Update()
    {
        if (ServerManager.instance.didStartGame.Value && transform.GetChild(0).gameObject.activeInHierarchy)
            EnableTexts(false);

        else if (!ServerManager.instance.didStartGame.Value && !transform.GetChild(0).gameObject.activeInHierarchy)
            EnableTexts(true);

    }

    private void EnableTexts(bool condition)
    {
        foreach (Transform child in transform)
            child.gameObject.SetActive(condition);
    }
}
