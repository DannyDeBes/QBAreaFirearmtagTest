using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace OpenScripts2
{
    public class ChargeTrigger : OpenScripts2_BasePlugin
    {
        public FVRFireArm FireArm;
        [Tooltip("Charge time in seconds")]
        public float ChargeTime = 1f;
        [Tooltip("If checked, every shot will be charged, even in automatic fire. Else only the first shot will be delayed.")]
        public bool ChargesUpEveryShot = false;

        public AudioEvent ChargingSounds;
        public AudioEvent ChargingAbortSounds;

        private bool _isHooked = false;
        private float _timeCharged = 0f;
        private bool _isCharging = false;
        private bool _isAutomaticFire = false;
#if !DEBUG
        public void Awake()
        {
            if (!_isHooked)
                switch (FireArm)
                {
                    case ClosedBoltWeapon w:
                        HookClosedBolt();
                        _isHooked = true;
                        break;
                    case OpenBoltReceiver w:
                        HookOpenBolt();
                        _isHooked = true;
                        break;
                    case Handgun w:
                        HookHandgun();
                        _isHooked = true;
                        break;
                    case BoltActionRifle w:
                        HookBoltActionRifle();
                        _isHooked = true;
                        break;
                    case TubeFedShotgun w:
                        HookTubeFedShotgun();
                        _isHooked = true;
                        break;
                    case LeverActionFirearm w:
                        HookLeverActionFirearm();
                        _isHooked = true;
                        break;
                    case BreakActionWeapon w:
                        HookBreakActionWeapon();
                        _isHooked = true;
                        break;
                    default:
                        LogWarning($"Firearm type \"{FireArm.GetType()}\" not supported!");
                        break;
                }
        }

        public void OnDestroy()
        {
            if (_isHooked)
                switch (FireArm)
                {
                    case ClosedBoltWeapon w:
                        UnhookClosedBolt();
                        _isHooked = false;
                        break;
                    case OpenBoltReceiver w:
                        UnhookOpenBolt();
                        _isHooked = false;
                        break;
                    case Handgun w:
                        UnhookHandgun();
                        _isHooked = false;
                        break;
                    case BoltActionRifle w:
                        UnhookBoltActionRifle();
                        _isHooked = false;
                        break;
                    case TubeFedShotgun w:
                        UnhookTubeFedShotgun();
                        _isHooked = false;
                        break;
                    case LeverActionFirearm w:
                        UnhookLeverActionRifle();
                        _isHooked = false;
                        break;
                    case BreakActionWeapon w:
                        UnhookBreakActionWeapon();
                        _isHooked = false;
                        break;
                    default:
                        break;
                }
        }

        // ClosedBoltWeapon Hooks and Coroutine
        private void UnhookClosedBolt()
        {
            On.FistVR.ClosedBoltWeapon.DropHammer -= ClosedBoltWeapon_DropHammer;
            On.FistVR.ClosedBoltWeapon.FVRUpdate -= ClosedBoltWeapon_FVRUpdate;
        }
        private void HookClosedBolt()
        {
            On.FistVR.ClosedBoltWeapon.DropHammer += ClosedBoltWeapon_DropHammer;
            On.FistVR.ClosedBoltWeapon.FVRUpdate += ClosedBoltWeapon_FVRUpdate;
        }

        private void ClosedBoltWeapon_FVRUpdate(On.FistVR.ClosedBoltWeapon.orig_FVRUpdate orig, ClosedBoltWeapon self)
        {
            orig(self);
            if (FireArm == self && (!self.IsHeld || self.m_hand.Input.TriggerFloat < self.TriggerResetThreshold)) _isAutomaticFire = false;
        }

        private void ClosedBoltWeapon_DropHammer(On.FistVR.ClosedBoltWeapon.orig_DropHammer orig, ClosedBoltWeapon self)
        {
            if (!_isCharging && !_isAutomaticFire && self == FireArm) StartCoroutine(HammerDropClosedBolt(orig, self));
            else if (_isAutomaticFire || self != FireArm) orig(self);
        }
        private IEnumerator HammerDropClosedBolt(On.FistVR.ClosedBoltWeapon.orig_DropHammer orig, ClosedBoltWeapon self)
        {
            _isCharging = true;
            _timeCharged = 0f;
            SM.PlayGenericSound(ChargingSounds, self.transform.position);
            while (_timeCharged < ChargeTime)
            {
                if (!self.IsHeld || self.m_hand.Input.TriggerFloat < self.TriggerResetThreshold) break;
                _timeCharged += Time.deltaTime;
                yield return null;
            }
            _isCharging = false;
            ClosedBoltWeapon.FireSelectorModeType modeType = self.FireSelector_Modes[self.m_fireSelectorMode].ModeType;
            if (!ChargesUpEveryShot && modeType != ClosedBoltWeapon.FireSelectorModeType.Single) _isAutomaticFire = true;
            if (_timeCharged >= ChargeTime) orig(self);
            else SM.PlayGenericSound(ChargingAbortSounds, self.transform.position);
        }

        // OpenBoltReceiver Hooks and Coroutine
        private void UnhookOpenBolt()
        {
            On.FistVR.OpenBoltReceiver.ReleaseSeer -= OpenBoltReceiver_ReleaseSeer;
            On.FistVR.OpenBoltReceiver.FVRUpdate -= OpenBoltReceiver_FVRUpdate;
        }
        private void HookOpenBolt()
        {
            On.FistVR.OpenBoltReceiver.ReleaseSeer += OpenBoltReceiver_ReleaseSeer;
            On.FistVR.OpenBoltReceiver.FVRUpdate += OpenBoltReceiver_FVRUpdate;
        }

        private void OpenBoltReceiver_FVRUpdate(On.FistVR.OpenBoltReceiver.orig_FVRUpdate orig, OpenBoltReceiver self)
        {
            orig(self);
            if (FireArm == self && (!self.IsHeld || self.m_hand.Input.TriggerFloat < self.TriggerResetThreshold)) _isAutomaticFire = false;
        }

        private void OpenBoltReceiver_ReleaseSeer(On.FistVR.OpenBoltReceiver.orig_ReleaseSeer orig, OpenBoltReceiver self)
        {
            if (!_isCharging && !_isAutomaticFire && self == FireArm) StartCoroutine(SeerReleaseOpenBolt(orig, self));
            else if (self != FireArm) orig(self);
        }
        private IEnumerator SeerReleaseOpenBolt(On.FistVR.OpenBoltReceiver.orig_ReleaseSeer orig, OpenBoltReceiver self)
        {
            _isCharging = true;
            _timeCharged = 0f;
            SM.PlayGenericSound(ChargingSounds, self.transform.position);
            while (_timeCharged < ChargeTime)
            {
                if (!self.IsHeld || self.m_hand.Input.TriggerFloat < self.TriggerResetThreshold) break;
                _timeCharged += Time.deltaTime;
                yield return null;
            }
            _isCharging = false;
            OpenBoltReceiver.FireSelectorModeType modeType = self.FireSelector_Modes[self.m_fireSelectorMode].ModeType;
            if (!ChargesUpEveryShot && modeType != OpenBoltReceiver.FireSelectorModeType.Single) _isAutomaticFire = true;
            if (_timeCharged >= ChargeTime) orig(self);
            else SM.PlayGenericSound(ChargingAbortSounds, self.transform.position);
        }

        // Handgun Hooks and Coroutine
        private void UnhookHandgun()
        {
            On.FistVR.Handgun.ReleaseSeer -= Handgun_ReleaseSeer;
            On.FistVR.Handgun.FVRUpdate -= Handgun_FVRUpdate;
        }
        private void HookHandgun()
        {
            On.FistVR.Handgun.ReleaseSeer += Handgun_ReleaseSeer;
            On.FistVR.Handgun.FVRUpdate += Handgun_FVRUpdate;
        }

        private void Handgun_FVRUpdate(On.FistVR.Handgun.orig_FVRUpdate orig, Handgun self)
        {
            orig(self);
            if (FireArm == self && (!self.IsHeld || self.m_hand.Input.TriggerFloat < self.TriggerResetThreshold)) _isAutomaticFire = false;
        }

        private void Handgun_ReleaseSeer(On.FistVR.Handgun.orig_ReleaseSeer orig, Handgun self)
        {
            if (!_isCharging && !_isAutomaticFire && self == FireArm) StartCoroutine(SeerReleaseHandgun(orig, self));
            else if (self != FireArm) orig(self);
        }
        private IEnumerator SeerReleaseHandgun(On.FistVR.Handgun.orig_ReleaseSeer orig, Handgun self)
        {
            _isCharging = true;
            _timeCharged = 0f;
            SM.PlayGenericSound(ChargingSounds, self.transform.position);
            while (_timeCharged < ChargeTime)
            {
                if (!self.IsHeld || self.m_hand.Input.TriggerFloat < self.TriggerResetThreshold) break;
                _timeCharged += Time.deltaTime;
                yield return null;
            }
            _isCharging = false;
            Handgun.FireSelectorModeType modeType = self.FireSelectorModes[self.m_fireSelectorMode].ModeType;
            if (!ChargesUpEveryShot && modeType != Handgun.FireSelectorModeType.Single) _isAutomaticFire = true;
            if (_timeCharged >= ChargeTime) orig(self);
            else SM.PlayGenericSound(ChargingAbortSounds, self.transform.position);
        }

        // BoltActionRifle Hooks and Coroutine
        private void UnhookBoltActionRifle()
        {
            On.FistVR.BoltActionRifle.DropHammer -= BoltActionRifle_DropHammer;
        }

        private void HookBoltActionRifle()
        {
            On.FistVR.BoltActionRifle.DropHammer += BoltActionRifle_DropHammer;
        }

        private void BoltActionRifle_DropHammer(On.FistVR.BoltActionRifle.orig_DropHammer orig, BoltActionRifle self)
        {
            if (!_isCharging && self == FireArm) StartCoroutine(DropHammerBoltAction(orig, self));
            else if (self != FireArm) orig(self);
        }

        private IEnumerator DropHammerBoltAction(On.FistVR.BoltActionRifle.orig_DropHammer orig, BoltActionRifle self)
        {
            _isCharging = true;
            _timeCharged = 0f;
            SM.PlayGenericSound(ChargingSounds, self.transform.position);
            while (_timeCharged < ChargeTime)
            {
                if (!self.IsHeld || self.m_hand.Input.TriggerFloat < self.TriggerResetThreshold) break;
                _timeCharged += Time.deltaTime;
                yield return null;
            }
            _isCharging = false;
            if (_timeCharged >= ChargeTime) orig(self);
            else SM.PlayGenericSound(ChargingAbortSounds, self.transform.position);
        }

        // TubeFedShotgun Hooks and Coroutine
        private void UnhookTubeFedShotgun()
        {
            On.FistVR.TubeFedShotgun.ReleaseHammer -= TubeFedShotgun_ReleaseHammer;
        }

        private void HookTubeFedShotgun()
        {
            On.FistVR.TubeFedShotgun.ReleaseHammer += TubeFedShotgun_ReleaseHammer;
        }

        private void TubeFedShotgun_ReleaseHammer(On.FistVR.TubeFedShotgun.orig_ReleaseHammer orig, TubeFedShotgun self)
        {
            if (!_isCharging && self == FireArm) StartCoroutine(ReleaseHammerTubeFed(orig, self));
            else if (self != FireArm) orig(self);
        }

        private IEnumerator ReleaseHammerTubeFed(On.FistVR.TubeFedShotgun.orig_ReleaseHammer orig, TubeFedShotgun self)
        {
            _isCharging = true;
            _timeCharged = 0f;
            SM.PlayGenericSound(ChargingSounds, self.transform.position);
            while (_timeCharged < ChargeTime)
            {
                if (!self.IsHeld || self.m_hand.Input.TriggerFloat < self.TriggerResetThreshold) break;
                _timeCharged += Time.deltaTime;
                yield return null;
            }
            _isCharging = false;
            if (_timeCharged >= ChargeTime) orig(self);
            else SM.PlayGenericSound(ChargingAbortSounds, self.transform.position);
        }

        // LeverActionFirearm Hooks and Coroutine
        private void UnhookLeverActionRifle()
        {
            On.FistVR.LeverActionFirearm.Fire -= LeverActionFirearm_Fire;
        }
        private void HookLeverActionFirearm()
        {
            On.FistVR.LeverActionFirearm.Fire += LeverActionFirearm_Fire;
        }

        private void LeverActionFirearm_Fire(On.FistVR.LeverActionFirearm.orig_Fire orig, LeverActionFirearm self)
        {
            if (!_isCharging && self == FireArm) StartCoroutine(FireLeverAction(orig, self));
            else if (self != FireArm) orig(self);
        }

        private IEnumerator FireLeverAction(On.FistVR.LeverActionFirearm.orig_Fire orig, LeverActionFirearm self)
        {
            _isCharging = true;
            _timeCharged = 0f;
            SM.PlayGenericSound(ChargingSounds, self.transform.position);
            while (_timeCharged < ChargeTime)
            {
                if (!self.IsHeld || self.m_hand.Input.TriggerUp) break;
                _timeCharged += Time.deltaTime;
                yield return null;
            }
            _isCharging = false;
            if (_timeCharged >= ChargeTime) orig(self);
            else SM.PlayGenericSound(ChargingAbortSounds, self.transform.position);
        }

        // BreakOpenShotgun Hooks and Coroutine
        private void UnhookBreakActionWeapon()
        {
            On.FistVR.BreakActionWeapon.DropHammer -= BreakActionWeapon_DropHammer;
        }
        private void HookBreakActionWeapon()
        {
            On.FistVR.BreakActionWeapon.DropHammer += BreakActionWeapon_DropHammer;
        }

        private void BreakActionWeapon_DropHammer(On.FistVR.BreakActionWeapon.orig_DropHammer orig, BreakActionWeapon self)
        {
            if (!_isCharging && self == FireArm) StartCoroutine(DropHammerBreakAction(orig, self));
            else if (self != FireArm) orig(self);
        }
        private IEnumerator DropHammerBreakAction(On.FistVR.BreakActionWeapon.orig_DropHammer orig, BreakActionWeapon self)
        {
            _isCharging = true;
            _timeCharged = 0f;
            SM.PlayGenericSound(ChargingSounds, self.transform.position);
            while (_timeCharged < ChargeTime)
            {
                if (!self.m_isLatched || !self.IsHeld || self.m_hand.Input.TriggerFloat <= 0.45f) break;
                _timeCharged += Time.deltaTime;
                yield return null;
            }
            _isCharging = false;
            if (_timeCharged >= ChargeTime) orig(self);
            else SM.PlayGenericSound(ChargingAbortSounds, self.transform.position);
        }
#endif
    }
}