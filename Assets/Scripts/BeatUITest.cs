using UnityEngine;
using System.Collections;

public class BeatUITest : MonoBehaviour
{
    public GameObject lowBeat;
    public GameObject midBeat;
    public GameObject highBeat;
    public float waitTime = 0.3f;

    public void ShowLow() => StartCoroutine(FlashBeat(lowBeat));
    public void ShowMid() => StartCoroutine(FlashBeat(midBeat));
    public void ShowHigh() => StartCoroutine(FlashBeat(highBeat));

    private IEnumerator FlashBeat(GameObject beatObject)
    {
        beatObject.SetActive(true);
        yield return new WaitForSeconds(waitTime);
        beatObject.SetActive(false);
    }
}
