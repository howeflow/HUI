namespace HUI
{
    using System;
    using System.Collections;
    using System.Linq;
    using UnityEngine;
    using System.Collections.Generic;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Animations;

    [CustomEditor(typeof(AnimatorUIAnimation))]
    public class AnimatorUIAnimationEditor : Editor
    {
        private AnimatorUIAnimation implement;

        private void OnEnable()
        {
            implement = (AnimatorUIAnimation)target;
        }
        public override void OnInspectorGUI()
        {
            implement.Init();

            if (!implement.IsValid)
            {
                GUI.color = Color.red;
                GUILayout.Label("Animator is invalid.");
                GUI.color = Color.white;
                return;
            }

            implement.mode = (AnimatorUIAnimation.Mode)EditorGUILayout.EnumPopup("Mode", implement.mode);


            List<string> nameList = new List<string>() { "none" };
            var controller = implement.animator.runtimeAnimatorController as AnimatorController;

            if (implement.mode == AnimatorUIAnimation.Mode.Play)
            {
                var names = controller.layers[0].stateMachine.states.Select(p => p.state.name);
                nameList.AddRange(names);
            }
            else
            {
                var names = controller.parameters.Select(p => p.name);
                nameList.AddRange(names);
            }

            var clipNames = nameList.ToArray();

            var openAnimIndex = Mathf.Max(0, Array.IndexOf(clipNames, implement.openId));
            var closeAnimIndex = Mathf.Max(0, Array.IndexOf(clipNames, implement.closeId));

            openAnimIndex = EditorGUILayout.Popup("Open", openAnimIndex, clipNames);
            implement.openId = clipNames[openAnimIndex];
            closeAnimIndex = EditorGUILayout.Popup("Close", closeAnimIndex, clipNames);
            implement.closeId = clipNames[closeAnimIndex];

            if (GUI.changed)
            {
                EditorUtility.SetDirty(implement);
            }
        }
    }


#endif

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public class AnimatorUIAnimation : MonoBehaviour, IUIAnimation
    {
        public enum Mode
        {
            Play,
            Trigger,
        }
        public Mode mode;
        public string openId = "open";
        public string closeId = "close";

        private int openHash;
        private int closeHash;

        [HideInInspector] public Animator animator;

        public bool IsValid => animator != null && animator.runtimeAnimatorController != null;


        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            animator = GetComponent<Animator>();
            openHash = Animator.StringToHash(openId);
            closeHash = Animator.StringToHash(closeId);
        }

        public IEnumerator Show()
        {
            if (!IsValid)
                yield break;
            if (openId.Equals("none"))
                yield break;

            if (mode == Mode.Play)
            {
                animator.Play(openHash);
            }
            else
            {
                animator.SetTrigger(openHash);
            }

            yield return new WaitWhile(() => {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                return state.shortNameHash == openHash && state.normalizedTime < 1;
            });
        }

        public IEnumerator Hide()
        {
            if (!IsValid)
                yield break;
            if (closeId.Equals("none"))
                yield break;

            if (mode == Mode.Play)
            {
                animator.Play(closeHash);
            }
            else
            {
                animator.SetTrigger(closeHash);
            }

            yield return new WaitUntil(() => {
                var state = animator.GetCurrentAnimatorStateInfo(0);
                return state.shortNameHash == closeHash && state.normalizedTime >= 1;
            });
        }
    }

}