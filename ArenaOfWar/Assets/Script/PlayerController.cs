using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{

    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float lookSensitivity = 3f;


    [SerializeField]
    private float thrusterForce = 1000f;

    [SerializeField]
    private float thrusterFuelBurnSpeed = 1f;
    [SerializeField]
    private float thrusterFuelRegenSpeed = 0.3f;
    private float thrusterFuelAmount = 1f;

    public float GetThrusterFuelAmount()
    {
        return thrusterFuelAmount;
    }

    [SerializeField]
    private LayerMask environmentMask;

    [Header("Spring settings:")]
    [SerializeField]
    private float jointMaxForce = 40f;
    [SerializeField]
    private float jointSpring = 20f;

    //Components caching
    private PlayerMotor motor;
    private ConfigurableJoint joint;
    private Animator animator;

    void Start()
    {
        motor = GetComponent<PlayerMotor>();
        joint = GetComponent<ConfigurableJoint>();
        animator = GetComponent<Animator>();

        SetJointSettings(jointSpring);
    }

    void Update()
    {
        float _xMov = CrossPlatformInputManager.GetAxis("leftX");
        float _zMov = CrossPlatformInputManager.GetAxis("leftY");

        float _hRot = CrossPlatformInputManager.GetAxis("rightX");
        float _vRot = CrossPlatformInputManager.GetAxis("rightY");

        float _cameraRotationX = _vRot * lookSensitivity;





        Vector3 _movHorizontal = transform.right * _xMov;
        Vector3 _movVertical = transform.forward * _zMov;
        Vector3 _velocity = (_movHorizontal + _movVertical) * speed;
        Vector3 _rotation = new Vector3(0f, _hRot, 0f) * lookSensitivity;


        motor.Move(_velocity);
        motor.Rotate(_rotation);
        motor.RotateCamera(_cameraRotationX);








        if (PauseMenu.IsOn)
            return;
            
        // Setting target position for spring, this makes the physics act right when it comes to applying gravitiy when flying over objects.
        RaycastHit _hit;
        if (Physics.Raycast(transform.position, Vector3.down, out _hit, 100f, environmentMask))
        {
            joint.targetPosition = new Vector3(0f, -_hit.point.y, 0f);
        }
        else
        {
            joint.targetPosition = new Vector3(0f, 0f, 0f);
        }

        // Calculate movement velocity as a 3D vector.
        









        

        
        //Animate Movement
        animator.SetFloat("ForwardVelocity", _zMov);



        // Calculate the thrusterforce based on player input
        Vector3 _thrusterForce = Vector3.zero;
        if (CrossPlatformInputManager.GetButton("leftButton") && thrusterFuelAmount > 0f)
        {
            thrusterFuelAmount -= thrusterFuelBurnSpeed * Time.deltaTime;

            if(thrusterFuelAmount >= 0.01f)
            { 
            _thrusterForce = Vector3.up * thrusterForce;
            SetJointSettings(0f);
            }
        }
        else
        {
            thrusterFuelAmount += thrusterFuelRegenSpeed * Time.deltaTime;

            SetJointSettings(jointSpring);
        }

        thrusterFuelAmount = Mathf.Clamp(thrusterFuelAmount, 0f, 1f);


        // Apply the thruster force
        motor.ApplyThruster(_thrusterForce);

    }

    private void SetJointSettings(float _jointSpring)
    {
        joint.yDrive = new JointDrive {
            positionSpring = _jointSpring,
            maximumForce = jointMaxForce
        };
    }


}