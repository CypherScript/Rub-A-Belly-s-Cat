using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;




namespace BellyRub
{

    [System.Serializable]
    public abstract class CatAttackData
    {
        // Type of the cat attack
        [HorizontalGroup, HideLabel]
        public CatAttackType AttackType;
    }

    // Class that stores the type of attack and quantity of controllers to use
    public class CatAttackQuantity : CatAttackData
    {
        // Number of attack controllers
        [HorizontalGroup]
        public int count = 0;
    }

    // Class that stores the type of attack and specific indices to use
    public class CatAttackIndices : CatAttackData
    {
        // Number of attack controllers
        [HorizontalGroup]
        public string indices;

        public IEnumerable<int> Indices => (indices ?? "").Split(',').Select(int.Parse);
    }

    // Class that stores the attack patterns for the cat
    [CreateAssetMenu(fileName = "atk_NewAttack", menuName = "Belly Rub/Cat Attack Pattern")]
    public class CatAttackPattern : SerializedScriptableObject
    {
        // Frequency of the attack pattern
        [field: SerializeField]
        public float Frequency { get; private set; } = 0.0f;

        // Steps to execute in order
        [field: SerializeField, ListDrawerSettings(ShowFoldout = true, ElementColor = "GetStepColor")]
        public List<CatAttackPatternStep> Steps { get; private set; } = new();

        private Color GetStepColor(int index, Color defaultColor)
        {
            CatAttackPatternStep step = Steps[index];

            switch (step)
            {
                case PrepareTargetStep:
                case StartNextPatternStep:
                    return new Color(0.4f, 0.2f, 0.4f);


                case DelayStep:
                case WaitForAttacksCompletedStep:
                    return new Color(0.4f, 0.4f, 0.2f);


                case RaiseDangerStep:
                    return new Color(0.4f, 0.2f, 0.2f);


                case LowerDangerStep:
                    return new Color(0.2f, 0.4f, 0.2f);

                default:
                    return defaultColor;
            }
        }
    }

    [System.Serializable]
    public abstract class CatAttackPatternStep
    {

    }

    /// <summary>
    /// When performing this step, CatController tries to retrieve valid targets and reserve them for future steps.
    /// For example, if the PrepareTargetStep specifies 3 PawClaws, it will pick 3 CatAttackControllers that are valid to perform that attack, which will become index 0, 1, and 2 respectively for the remainder of the pattern.
    /// If this step fails, the CatAttackPattern should proceed to select the next pattern immediately.
    /// </summary>
    public class PrepareTargetStep : CatAttackPatternStep
    {
        [field: SerializeField, ListDrawerSettings(ShowFoldout = true)]
        public List<CatAttackQuantity> Targets { get; private set; } = new();
    }

    /// <summary>
    /// When performing this step, CatController will delay executing the next step for the specified time.
    /// </summary>
    public class DelayStep : CatAttackPatternStep
    {
        [field: SerializeField]
        public float DelayTime { get; private set; } = 1f;
    }

    /// <summary>
    /// When performing this step, CatController will wait for all attacks to return to danger level 1, or 5 seconds, whichever is shorter.
    /// </summary>
    public class WaitForAttacksCompletedStep : CatAttackPatternStep
    {
        [field: SerializeField]
        public bool IncludeNonReservedAttacks { get; private set; } = false;
    }

    /// <summary>
    /// When performing this step, CatController raises the danger level for the specified targets.
    /// If the targets are specified as CatAttackQuantity, random valid CatAttackControllers are selected.
    /// If the targets are specified as CatAttackIndices, specific CatAttackControllers reserved in a previous PrepareTargetStep are used.
    /// </summary>
    public class RaiseDangerStep : CatAttackPatternStep
    {
        [field: SerializeField, ListDrawerSettings(ShowFoldout = true)]
        public List<CatAttackData> Targets { get; private set; } = new();
    }

    /// <summary>
    /// When performing this step, CatController lowers the danger level for the specified targets.
    /// If the targets are specified as CatAttackQuantity, random valid CatAttackControllers are selected.
    /// If the targets are specified as CatAttackIndices, specific CatAttackControllers reserved in a previous PrepareTargetStep are used.
    /// </summary>
    public class LowerDangerStep : CatAttackPatternStep
    {
        [field: SerializeField, ListDrawerSettings(ShowFoldout = true)]
        public List<CatAttackData> Targets { get; private set; } = new();
    }

    /// <summary>
    /// When performing this step, CatController will check the difficulty setting and prepare to start the next attack pattern.
    /// The current CatAttackPattern will continue executing in parallel.
    /// </summary>
    public class StartNextPatternStep : CatAttackPatternStep
    {

    }
}