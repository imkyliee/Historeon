using System.Collections;
using UnityEngine;

public class RandomEyeBlink : MonoBehaviour
{
    public Animator[] eyes;

    public float minWait = 5f;
    public float maxWait = 15f;

    public float openTime = 3f;
    public float closeTime = 1f;

    private string[] currentAnimations;


    void Start()
    {
        currentAnimations = new string[eyes.Length];

        for (int i = 0; i < eyes.Length; i++)
        {
            StartCoroutine(EyeRoutine(i));
        }
    }


    public void ChangeAnimation(int eyeIndex, string animation, float crossfade = 0.1f)
    {
        if (currentAnimations[eyeIndex] == animation)
            return;

        currentAnimations[eyeIndex] = animation;
        eyes[eyeIndex].CrossFade(animation, crossfade);
    }

    // Eye's life cycle.
    IEnumerator EyeRoutine(int index)
    {
        while (true)
        {
            // Random delay for each eye
            yield return new WaitForSeconds(Random.Range(minWait, maxWait));

            // Open
            ChangeAnimation(index, "Open" + (index + 1));
            yield return new WaitForSeconds(openTime);

            // Close
            ChangeAnimation(index, "Close" + (index + 1));
            yield return new WaitForSeconds(closeTime);
        }
    }
}