using DesertImage.ECS;
using Game.Physics;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Vehicle
{
    public class WheelView : EntityView
    {
        [SerializeField] private Rigidbody rigidbody;

        [SerializeField] [Space] [Header("Suspension")]
        private float height = .55f;

        [SerializeField] private float strength = 40000f;
        [SerializeField] private float damping = 4000f;

        [SerializeField] [Space] [Header("Wheel")]
        private float radius = .5f;

        [SerializeField] private Transform view;
        [SerializeField] private bool isDriveWheel;
        [SerializeField] private bool isSteeringWheel;
        [SerializeField] private bool isBrakingWheel;
        [SerializeField] private bool isHandBrakingWheel;

        [FormerlySerializedAs("Friction")]
        [FormerlySerializedAs("Side")] [Space] [Header("Friction")] 
        [SerializeField] private AnimationCurve friction;
        [SerializeField] private float slipAnglePeak;

        public override void Initialize(in Entity entity)
        {
            base.Initialize(in entity);

            entity.Replace(new PhysicalObject { Rigidbody = rigidbody });

            entity.Replace
            (
                new WheelFriction
                {
                    FrictionCurve = new Curve(entity.CreateBufferList<float, WheelFriction>(128), friction, 128),
                    CorneringStiffness = 1f,
                    ForwardStiffness = 1f,
                    GroundGrip = 1f,
                    SlipAnglePeak = slipAnglePeak
                }
            );

            const float mass = 20f;

            entity.Replace(
                new Wheel
                {
                    Mass = mass,
                    Radius = radius,
                    Inertia = mass * radius * radius * .5f,
                    View = view,
                    AngularVelocity = 1f
                }
            );

            if (isDriveWheel)
            {
                entity.Replace<DriveWheel>();
            }

            if (isSteeringWheel)
            {
                // entity.Replace(new SteeringWheel { MaxAngle = 35f });
            }

            if (isBrakingWheel)
            {
                entity.Replace<Brakes>();
            }

            if (isHandBrakingWheel)
            {
                // entity.Replace<HandBrakeAxis>();
            }

            entity.Replace
            (
                new Suspension
                {
                    Height = height,
                    Strength = strength,
                    Damping = damping
                }
            );
        }

        private void OnDrawGizmos()
        {
            if (!Entity.IsAlive()) return;

            var transf = transform;
            var contactPosition = transf.position + -transf.up * Entity.Read<Wheel>().Radius;

            var friction = Entity.Read<WheelFriction>();
            var lateralFriction = friction.Slips.x;
            var longFriction = friction.Slips.y;

            Gizmos.color = Color.red;
            // Gizmos.DrawCube
            // (
            //     position + (transform.right * lateralFriction * .5f),
            //     new Vector3(lateralFriction, .1f, .1f)
            // );
            Gizmos.DrawLine
            (
                contactPosition,
                contactPosition + transform.right * lateralFriction
            );

            Gizmos.color = Color.blue;
            Gizmos.DrawLine
            (
                contactPosition,
                contactPosition + transform.forward * longFriction
            );
            // Gizmos.DrawCube
            // (
            // position + (transform.forward * longFriction * .5f),
            // new Vector3(longFriction, .1f, .1f)
            // );
        }

        private void DrawBoldLine(float length, Vector3 direction)
        {
            var halfWidth = length * .5f;
            Gizmos.DrawCube(transform.position + (direction * halfWidth), direction * length);
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (!view)
            {
                view = transform.GetChild(0);
            }
        }
    }
}