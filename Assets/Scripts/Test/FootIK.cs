using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootIK : MonoBehaviour
{
    Animator animator;

    public float positionWeight = 1;
    public float rotationWeight = 1;

    public Transform leftFootPosition;
    public Transform rightFootPosition;

    public LayerMask mask;

    public float maxDetectDistance = .5f;

    public Transform parent;

    public float multiplier = 1;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public float offset = 0;
    private void OnAnimatorIK(int layerIndex)
    {
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, positionWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, rotationWeight);

        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, positionWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, rotationWeight);

        SetLeftLegIK();
        SetRightLegIK();
    }

    public float pelvisHeight;
    public float heightToChangePelvisHeight;

    void SetLeftLegIK()
    {
        Vector3 targetPosition = animator.GetIKPosition(AvatarIKGoal.LeftFoot);

        RaycastHit hit;

        Vector3 test = new Vector3(0, maxDetectDistance / 2, 0);
        Debug.DrawRay(animator.GetIKPosition(AvatarIKGoal.LeftFoot) + test, Vector3.down, Color.red);
        if (Physics.Raycast(animator.GetIKPosition(AvatarIKGoal.LeftFoot) + test, Vector3.down, out hit, maxDetectDistance, mask))
        {
            float newPoint = hit.point.y - parent.position.y - offset;
            targetPosition.y += newPoint * multiplier;

            if (targetPosition.y > heightToChangePelvisHeight)
            {
                animator.bodyPosition += new Vector3(0, pelvisHeight, 0);
            }
        } 

        animator.SetIKPosition(AvatarIKGoal.LeftFoot, targetPosition);
    }

    void SetRightLegIK()
    {
        Vector3 targetPosition = animator.GetIKPosition(AvatarIKGoal.RightFoot);

        RaycastHit hit;

        Vector3 test = new Vector3(0, maxDetectDistance / 2, 0);
        Debug.DrawRay(animator.GetIKPosition(AvatarIKGoal.RightFoot) + test, Vector3.down, Color.red);
        if (Physics.Raycast(animator.GetIKPosition(AvatarIKGoal.RightFoot) + test, Vector3.down, out hit, maxDetectDistance, mask))
        {
            float newPoint = hit.point.y - parent.position.y - offset;
            targetPosition.y += newPoint * multiplier;

            if(targetPosition.y > heightToChangePelvisHeight)
            {
                animator.bodyPosition += new Vector3(0, pelvisHeight, 0);
            }
        } 


        animator.SetIKPosition(AvatarIKGoal.RightFoot, targetPosition);
    }
}