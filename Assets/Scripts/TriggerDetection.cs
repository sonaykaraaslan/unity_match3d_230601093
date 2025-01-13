using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetection : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            StartCoroutine(Merge(other));
        }
    }

    private IEnumerator Merge(Collider other)
    {
        other.attachedRigidbody.isKinematic = true;

        yield return null;
        float time = 0f;
        Vector3 startPos = other.attachedRigidbody.transform.position;
        Vector3 targetPos = transform.position + Vector3.up * 5f;
        float duration = 3f;
        var otherObject = other.attachedRigidbody.gameObject;

        while (time < duration)
        {
            time += Time.deltaTime;
            otherObject.transform.position = Vector3.Lerp(startPos, targetPos, time / duration);
            other.transform.rotation = Quaternion.Lerp(other.transform.rotation, Quaternion.identity, time / duration);
            other.transform.localScale = Vector3.Lerp(other.transform.localScale, Vector3.one * 4, time / duration);
            yield return null;
        }

        Destroy(other.attachedRigidbody.gameObject);
    }
}
