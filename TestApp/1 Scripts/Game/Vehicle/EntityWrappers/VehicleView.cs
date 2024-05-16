using System;
using System.Linq;
using DesertImage.ECS;
using UnityEngine;

namespace Game.Vehicle
{
    public class VehicleView : EntityView
    {
        [SerializeField] private WheelView[] wheels;
        [SerializeField] private WheelAxisWrapper[] wheelAxis;
        [SerializeField] private AnimationCurve engineTorque;
        [SerializeField] private Transform body;

        [Serializable]
        private struct WheelAxisWrapper
        {
            public EntityView Left;
            public EntityView Right;

            public float DriveRate;
            public float BrakeRate;

            public bool IsSteering;
            public bool IsHandBraking;
        }

        public override unsafe void Initialize(in Entity entity)
        {
            base.Initialize(in entity);

            entity.Replace<Steering>();
            entity.Replace<SteeringInput>();

            entity.Replace<Gas>();
            entity.Replace<GasInput>();

            var gearRatios = entity.CreateBufferArray<float>(7);
            gearRatios[0] = -3.615f;
            gearRatios[1] = 0f;
            gearRatios[2] = 3.583f;
            gearRatios[3] = 2.038f;
            gearRatios[4] = 1.414f;
            gearRatios[5] = 1.108f;
            gearRatios[6] = 0.878f;

            entity.Replace
            (
                new Gearbox
                {
                    GearRatio = gearRatios,
                    MainGear = 3.83f,
                    Gear = 2,
                    Max = 7,
                    Efficiency = .8f,
                    SwitchTime = .5f
                }
            );
            entity.Replace<GearboxInput>();
            entity.Replace(new GearboxAutomatic { DecreaseRPM = 2000f });

            entity.Replace
            (
                new Brakes
                {
                    Torque = 1000f,
                    HandBrakeTorque = 4000f,
                }
            );
            entity.Replace<BrakesInput>();

            entity.Replace
            (
                new Engine
                {
                    TorqueCurve = new Curve
                    (
                        entity.CreateBufferList<float, Engine>(9000),
                        engineTorque,
                        9000
                    ),
                    IdleRpm = 700f,
                    MaxRpm = 7000f,
                    Intertia = .3f,
                    BackTorque = -100f
                }
            );

            var wheelsBuffer = entity.CreateBufferList<Entity, Vehicle>(wheels.Length);

            var driveRatio = 1f / wheels.Sum(x => x.Entity.Has<DriveWheel>() ? 1 : 0);

            foreach (var wheel in wheels)
            {
                var wheelEntity = wheel.Entity;

                if (wheelEntity.Has<DriveWheel>())
                {
                    wheelEntity.Get<DriveWheel>().DriveRatio = driveRatio;
                }

                wheelsBuffer.Add(wheelEntity);
            }

            var axisBuffer = entity.CreateBufferList<Entity, Vehicle>(wheelAxis.Length);

            foreach (var axis in wheelAxis)
            {
                var axisEntity = entity.World->GetNewEntity();

                axisEntity.Replace
                (
                    new WheelAxis
                    {
                        Left = axis.Left.Entity,
                        Right = axis.Right.Entity,
                        DriveRatio = axis.DriveRate,
                        BrakeRatio = axis.BrakeRate
                    }
                );

                if (axis.IsSteering)
                {
                    axisEntity.Replace<SteeringWheelAxis>();
                    
                    var steeringWheel = new SteeringWheel { MaxAngle = 35f };
                    
                    axis.Left.Entity.Replace(steeringWheel);
                    axis.Right.Entity.Replace(steeringWheel);
                }

                if (axis.IsHandBraking)
                {
                    axisEntity.Replace<HandBrakeWheelAxis>();
                }

                axisBuffer.Add(axisEntity);
            }

            entity.Replace
            (
                new Vehicle
                {
                    Wheels = wheelsBuffer,
                    WheelAxis = axisBuffer,
                    Body = body
                }
            );
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (wheels is { Length: 0 })
            {
                wheels = GetComponentsInChildren<WheelView>();
            }
        }
    }
}