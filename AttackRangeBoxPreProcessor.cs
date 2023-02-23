using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackRangeBoxPreProcessor : MonoBehaviour
{
    [SerializeField] Animator _animator;
    [SerializeField] List<string> _animNames;
    [SerializeField] List<AttackSO> _attackSOs;

    RuntimeAnimatorController ac;

    private void Awake()
    {
        RuntimeAnimatorController ac = _animator.runtimeAnimatorController;
        
        for(int i=0; i<_animNames.Count; ++i)
        {
            for (int j = 0; j < ac.animationClips.Length; ++j)
            {
                if (ac.animationClips[j].name == _animNames[i])
                {
                    EditorCurveBinding[] curveBindings = UnityEditor.AnimationUtility.GetCurveBindings(ac.animationClips[j]);
                    foreach (var binding in curveBindings)
                    {
                        AnimationCurve curveData = AnimationUtility.GetEditorCurve(ac.animationClips[j], binding);

                        if (binding.propertyName == "m_LocalScale.x")
                            _attackSOs[i].AttackRangeBoxSize.x = curveData[0].value;
                        if (binding.propertyName == "m_LocalScale.y")
                            _attackSOs[i].AttackRangeBoxSize.y = curveData[0].value;
                        if (binding.propertyName == "m_LocalScale.z")
                            _attackSOs[i].AttackRangeBoxSize.z = curveData[0].value;
                        if (binding.propertyName == "m_LocalPosition.x")
                            _attackSOs[i].AttackRangeBoxPosition.x = curveData[0].value;
                        if (binding.propertyName == "m_LocalPosition.y")
                            _attackSOs[i].AttackRangeBoxPosition.y = curveData[0].value;
                        if (binding.propertyName == "m_LocalPosition.z")
                            _attackSOs[i].AttackRangeBoxPosition.z = curveData[0].value;
                    }
                }
            }
        }

    }
}
