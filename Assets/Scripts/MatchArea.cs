using System.Collections;
using DG.Tweening;
using UnityEngine;
using System;

namespace Match
{
    public class MatchArea : MonoBehaviour
    {
        public GameObject currentObject;

        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _leftObjectPlacement;
        [SerializeField] private Transform _rightObjectPlacement;
        [SerializeField] private GameObject matchEffectPrefab; // CFXR3 Hit Light B (Air) prefab

        [Header("Animation Settings")]
        [SerializeField] private float rotationAmount = 180f; // Dönüş miktarı
        [SerializeField] private float rotationDuration = 0.5f; // Dönüş süresi
        [SerializeField] private Ease rotationEase = Ease.InOutSine; // Dönüş easing tipi

        private readonly string objectTag = "Moveable";
        private Coroutine matchCoroutine;

        private readonly int _openLidHash = Animator.StringToHash("OpenLid");
        private readonly int _closeLidHash = Animator.StringToHash("CloseLid");

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == null || other.attachedRigidbody.CompareTag(objectTag) == false)
                return;

            if (other.gameObject == currentObject)
                return;

            if (currentObject == null)
            {
                SetCurrentObject(other);
            }
            else
            {
                if (ChechMatch(other))
                    return;

                other.attachedRigidbody.AddForce(Vector3.up * 15 + Vector3.forward * 15f, ForceMode.Impulse);
            }
        }

        private bool ChechMatch(Collider other)
        {
            if (matchCoroutine != null)
                return false;

            var currentItem = currentObject.GetComponent<Item>();
            var otherItem = other.attachedRigidbody.gameObject.GetComponent<Item>();
            if (!currentItem.IsMatching(otherItem))
                return false;

            other.attachedRigidbody.isKinematic = true;
            matchCoroutine = StartCoroutine(MatchCoroutine(otherItem));
            return true;
        }

        private void PlayMatchEffect(Vector3 position)
        {
            if (matchEffectPrefab != null)
            {
                GameObject effect = Instantiate(matchEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }

        private IEnumerator MatchCoroutine(Item otherItem)
        {
            float openDuration = 0.5f;
            float closeDuration = 0.5f;
            float objectMovementDuration = 1f;

            yield return null;
            var currentItem = currentObject.GetComponent<Item>();

            currentItem.SetCollidersActive(false);
            otherItem.SetCollidersActive(false);

            // DOTween'leri temizle
            DOTween.Kill(currentItem.transform, true);
            DOTween.Kill(otherItem.transform, true);

            // Sağdaki nesneyi yerleştir ve döndür
            Sequence rightSequence = DOTween.Sequence();
            rightSequence.Join(otherItem.transform.DOMove(_rightObjectPlacement.position, openDuration))
                        .Join(otherItem.transform.DORotate(_rightObjectPlacement.rotation.eulerAngles, openDuration))
                        .Join(otherItem.transform.DORotate(
                            otherItem.transform.rotation.eulerAngles + new Vector3(0, rotationAmount, 0),
                            rotationDuration)
                        .SetEase(rotationEase));

            // Soldaki nesneyi döndür
            currentItem.transform.DORotate(
                currentItem.transform.rotation.eulerAngles + new Vector3(0, -rotationAmount, 0),
                rotationDuration)
                .SetEase(rotationEase);

            // Kapak açılma animasyonunu başlat
            _animator.SetTrigger(_openLidHash);
            yield return new WaitForSeconds(openDuration);

            // Orta noktayı hesapla
            Vector3 targetPos = (currentItem.transform.position + otherItem.transform.position) / 2f;
            Vector3 effectPosition = targetPos; // Efekt için pozisyonu sakla

            // Merkeze hareket
            Sequence centerSequence = DOTween.Sequence();
            centerSequence.Join(currentItem.transform.DOMove(targetPos, objectMovementDuration))
                         .Join(otherItem.transform.DOMove(targetPos, objectMovementDuration));

            yield return new WaitForSeconds(objectMovementDuration * 0.5f);
            PlayMatchEffect(effectPosition); // Nesneler birleşirken efekti oynat

            yield return new WaitForSeconds(objectMovementDuration * 0.5f);

            // Aşağıya hareket
            targetPos = targetPos + Vector3.down * 2f;
            Sequence downSequence = DOTween.Sequence();
            downSequence.Join(currentItem.transform.DOMove(targetPos, objectMovementDuration))
                       .Join(otherItem.transform.DOMove(targetPos, objectMovementDuration));

            yield return new WaitForSeconds(objectMovementDuration);

            // Kapak kapanma animasyonu
            _animator.SetTrigger(_closeLidHash);
            yield return new WaitForSeconds(closeDuration);

            // Nesneleri devre dışı bırak
            if (currentItem != null && otherItem != null)
            {
                currentItem.gameObject.SetActive(false);
                otherItem.gameObject.SetActive(false);
                GameEvents.InvokeItemMatched(currentItem.itemData);
            }

            currentObject = null;
            matchCoroutine = null;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody == null || other.attachedRigidbody.CompareTag(objectTag) == false)
                return;
            if (other.attachedRigidbody.gameObject == currentObject)
            {
                DOTween.Kill(currentObject.transform);
                currentObject = null;
            }
        }

        private void SetCurrentObject(Collider other)
        {
            other.attachedRigidbody.isKinematic = true;
            currentObject = other.attachedRigidbody.gameObject;

            var tweenDuration = 1f;

            Sequence placementSequence = DOTween.Sequence();
            placementSequence.Join(currentObject.transform.DORotate(_leftObjectPlacement.rotation.eulerAngles, tweenDuration))
                           .Join(currentObject.transform.DOMove(_leftObjectPlacement.position, tweenDuration));
        }
    }
}