using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.Demo
{
    public class RandomMovement : MonoBehaviour
    {
        public float radius = 40.0f;
        public float speed = 3.0f;
        private Vector3 origin;
        private Vector3 currentDestination;

        void Start()
        {
            origin = transform.position;
            PickNewRandomDestination();
        }

        void Update()
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, currentDestination, step);

            if (Vector3.Distance(transform.position, currentDestination) < 0.001f)
            {
                PickNewRandomDestination();
            }
        }

        void PickNewRandomDestination()
        {
            currentDestination = (Random.insideUnitSphere * radius) + origin;
            currentDestination.y = transform.position.y;
        }
    }

}
