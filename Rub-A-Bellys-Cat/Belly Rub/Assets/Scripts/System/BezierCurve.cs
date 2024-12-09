using UnityEngine;

namespace BellyRub
{
    [System.Serializable]
    public struct BezierCurve
    {
        public Transform startPointTransform;
        public Transform controlPointTransform;
        public Transform endPointTransform;

        public Vector3 startPoint;
        public Vector3 controlPoint;
        public Vector3 endPoint;

        public bool autoComputeControlPoint;
        public float autoComputeHeight;

        public Vector3 StartPoint => startPointTransform ? startPointTransform.position : startPoint;

        public Vector3 ControlPoint
        {
            get
            {
                if (controlPointTransform)
                    return controlPointTransform.position;

                if (autoComputeControlPoint)
                {
                    Vector3 midPoint = (StartPoint + EndPoint) / 2f;
                    return midPoint + Vector3.up * autoComputeHeight;
                }

                return controlPoint;
            }
        }

        public Vector3 EndPoint => endPointTransform ? endPointTransform.position : endPoint;
    }
}


