using UnityEngine;

namespace NBodySimulator
{
    public class CameraMovement : MonoBehaviour
    {
        public CameraLooker camLook = null;

        float hAngleOffest = 0f;
        float vAngleOffest = 0f;

        float distance = 50f;
        float distanceOffset = 0f;

        public float maxZoomOut = 500f;

        public KeyCode rotateRight = KeyCode.D;
        public KeyCode rotateLeft = KeyCode.A;
        public KeyCode rotateUp = KeyCode.W;
        public KeyCode rotateDown = KeyCode.S;

        void Start()
        {
            camLook = new CameraLooker();
        }

        void Update()
        {
            UpdateCamera();
        }

        void UpdateCamera()
        {
            camLook.LookAtTransform(transform, Vector3.zero, distance + distanceOffset, hAngleOffest, vAngleOffest);
            float rad = 10f;

            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if ((distance + distanceOffset) > rad)
                {
                    distanceOffset = distanceOffset - 0.05f * (distance + distanceOffset);
                }
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if ((distance + distanceOffset) < maxZoomOut)
                {
                    distanceOffset = distanceOffset + 0.05f * (distance + distanceOffset);
                }
            }

            if (Input.GetKey(rotateRight))
            {
                hAngleOffest = hAngleOffest + 1f;
                if (hAngleOffest > 360f)
                {
                    hAngleOffest = hAngleOffest - 360f;
                }
            }

            if (Input.GetKey(rotateLeft))
            {
                hAngleOffest = hAngleOffest - 1f;
                if (hAngleOffest < -360f)
                {
                    hAngleOffest = hAngleOffest + 360f;
                }
            }

            if (Input.GetKey(rotateDown))
            {
                if ((vAngleOffest - 90f) < 0f)
                {
                    vAngleOffest = vAngleOffest + 1f;
                }
            }

            if (Input.GetKey(rotateUp))
            {
                if ((vAngleOffest - 90f) > -180f)
                {
                    vAngleOffest = vAngleOffest - 1f;
                }
            }
        }
    }

    public class CameraLooker
    {
        public void LookAtTransform(Transform transf, Vector3 source, float dist, float hRot, float vRot)
        {
            PosRot look = LookAt(source, dist, hRot, vRot);
            transf.position = look.position;
            transf.rotation = look.rotation;
        }

        PosRot LookAt(Vector3 source, float dist, float hRot, float vRot)
        {
            Vector3 norm = new Vector3(0f, 0f, 1f);
            norm = RotAround(vRot, norm, new Vector3(1f, 0f, 0f));
            norm = RotAround(hRot, norm, new Vector3(0f, 1f, 0f));

            norm = norm.normalized;

            Vector3 finalPos = source - dist * norm;
            Quaternion finalRot = Quaternion.Euler(-vRot, -hRot, 0f);

            PosRot pr = new PosRot
            {
                position = finalPos,
                rotation = finalRot
            };

            return pr;
        }

        Vector3 RotAround(float rotAngle, Vector3 original, Vector3 direction)
        {
            Vector3 cross1 = Vector3.Cross(original, direction);

            Vector3 pr = Vector3.Project(original, direction);
            Vector3 pr2 = original - pr;

            Vector3 cross2 = Vector3.Cross(pr2, cross1);
            Vector3 rotatedVector = (Quaternion.AngleAxis(rotAngle, cross2) * pr2) + pr;

            return rotatedVector;
        }
    }

    public struct PosRot
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}
