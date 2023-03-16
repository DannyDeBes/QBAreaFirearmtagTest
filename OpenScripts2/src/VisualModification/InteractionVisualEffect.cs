using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OpenScripts2
{
    [RequireComponent(typeof(VisualModifier))]
    public class InteractionVisualEffect : OpenScripts2_BasePlugin
    {
        [Header("Interaction Visual Effect Config")]
        public FVRInteractiveObject ObjectToMonitor;
        public enum E_InteractionType
        {
            None = -1,
            Holding,
            Touchpad,
            TouchpadUp,
            TouchpadDown,
            TouchpadLeft,
            TouchpadRight,
            AXButton,
            BYButton,
            Trigger
        }
        [Tooltip("Types of interactions that can trigger the effect. It's a list so you can make it streamline compatible! *shakes fist in touchpad controls!*")]
        public List<E_InteractionType> InteractionTypes = new();

        [Tooltip("Should the effect be applied instantanious or gradually? (Binary or linear transition)")]
        public bool SmoothTransition = false;
        [Tooltip("Transition time in seconds")]
        public float TransitionTime = 0.5f;

        private float _currentLerpValue = 0f;
        private bool _effectActive = false;

        private List<E_InteractionType> _currentInteractions;

        private VisualModifier _visualModifier;

        public void Awake()
        {
            _visualModifier = GetComponent<VisualModifier>();
        }
        public void Update()
        {
            _currentInteractions.Clear();
            FVRViveHand hand = ObjectToMonitor.m_hand;
            if (hand != null) 
            {
                _currentInteractions.Add(E_InteractionType.Holding);
                HandInput input = hand.Input;

                if (input.AXButtonPressed) _currentInteractions.Add(E_InteractionType.AXButton);
                if (input.BYButtonPressed) _currentInteractions.Add(E_InteractionType.BYButton);
                if (input.TouchpadPressed) _currentInteractions.Add(E_InteractionType.Touchpad);

                if (input.TouchpadPressed && Vector2.Angle(input.TouchpadAxes, Vector2.up) < 45f) _currentInteractions.Add(E_InteractionType.TouchpadUp);
                if (input.TouchpadPressed && Vector2.Angle(input.TouchpadAxes, Vector2.down) < 45f) _currentInteractions.Add(E_InteractionType.TouchpadDown);
                if (input.TouchpadPressed && Vector2.Angle(input.TouchpadAxes, Vector2.left) < 45f) _currentInteractions.Add(E_InteractionType.TouchpadLeft);
                if (input.TouchpadPressed && Vector2.Angle(input.TouchpadAxes, Vector2.right) < 45f) _currentInteractions.Add(E_InteractionType.TouchpadRight);

                if (input.TriggerPressed) _currentInteractions.Add(E_InteractionType.Trigger);
            }
            else
            {
                _currentInteractions.Add(E_InteractionType.None);
            }

            _effectActive = _currentInteractions.Intersect(InteractionTypes).Any();

            if (!SmoothTransition)
            {
                if (_effectActive) _visualModifier.UpdateVisualEffects(1f);
                else _visualModifier.UpdateVisualEffects(0f);
            }
            else
            {
                if (_effectActive) _currentLerpValue += Time.deltaTime / TransitionTime;
                else _currentLerpValue -= Time.deltaTime / TransitionTime;

                _currentLerpValue = Mathf.Clamp01(_currentLerpValue);

                _visualModifier.UpdateVisualEffects(_currentLerpValue);
            }
        }
    }
}
