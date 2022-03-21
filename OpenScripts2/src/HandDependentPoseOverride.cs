using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OpenScripts2
{
    public class HandDependentPoseOverride : OpenScripts2_BasePlugin
    {
        public FVRPhysicalObject physicalObject;
        public Transform leftPoseOverride;
        public Transform leftPoseOverride_Touch;
        public Transform rightPoseOverride;
        public Transform rightPoseOverride_Touch;

        private bool hasPoseOverride_Touch = false;

#if !(UNITY_EDITOR || UNITY_5 || MEATKIT || DEBUG)

        public void Start()
        {
            if (leftPoseOverride_Touch != null && rightPoseOverride_Touch != null) hasPoseOverride_Touch = true;

            Hook();
        }

        public void OnDestroy()
        {
            Unhook();
        }

        void Unhook()
        {
            On.FistVR.FVRPhysicalObject.BeginInteraction -= FVRPhysicalObject_BeginInteraction;
        }
        void Hook()
        {
            On.FistVR.FVRPhysicalObject.BeginInteraction += FVRPhysicalObject_BeginInteraction;
        }

        private void FVRPhysicalObject_BeginInteraction(On.FistVR.FVRPhysicalObject.orig_BeginInteraction orig, FVRPhysicalObject self, FVRViveHand hand)
        {
            if (self == physicalObject)
            {
                if (!hand.IsThisTheRightHand)
                {
                    if ((hand.CMode == ControlMode.Oculus || hand.CMode == ControlMode.Index) && hasPoseOverride_Touch) self.PoseOverride = leftPoseOverride_Touch;
                    else physicalObject.PoseOverride = leftPoseOverride;
                }
                else
                {
                    if ((hand.CMode == ControlMode.Oculus || hand.CMode == ControlMode.Index) && hasPoseOverride_Touch) self.PoseOverride = rightPoseOverride_Touch;
                    else physicalObject.PoseOverride = rightPoseOverride;
                }
            }
        
            orig(self, hand);
        }
#endif

#if MEATKIT && !DEBUG
        void Update()
        {
            FVRViveHand hand = physicalObject.m_hand;
            if (hand != null)
            {
                if (!hand.IsThisTheRightHand)
                {
                    if ((hand.CMode == ControlMode.Oculus || hand.CMode == ControlMode.Index) && hasPoseOverride_Touch) physicalObject.PoseOverride = leftPoseOverride_Touch;
                    else physicalObject.PoseOverride = leftPoseOverride;
                }
                else
                {
                    if ((hand.CMode == ControlMode.Oculus || hand.CMode == ControlMode.Index) && hasPoseOverride_Touch) physicalObject.PoseOverride = rightPoseOverride_Touch;
                    else physicalObject.PoseOverride = rightPoseOverride;
                }
            }
        }
#endif
    }
}
