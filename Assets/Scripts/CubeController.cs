using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace ObjectExamples
{

    public class CubeController : MonoBehaviour
    {
        public float speed = 1f;
        public float yThreshold = 0;
        public MeshRenderer meshRenderer;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void Start()
        {

        }


        private void Update()
        {
            MoveWithGravity();
        }



        private void MoveWithGravity()
        {
            var currentPos = transform.position;
            if (currentPos.y <= yThreshold)
            {
                currentPos.y = yThreshold;
                transform.position = currentPos;
                if (speed <= 0)
                    return;
            }

            var gravity = Physics.gravity;

            var velocityChange = gravity.y * Time.deltaTime;
            speed += velocityChange;

            currentPos.y += speed * Time.deltaTime;
            transform.position = currentPos;
        }
    }

}
