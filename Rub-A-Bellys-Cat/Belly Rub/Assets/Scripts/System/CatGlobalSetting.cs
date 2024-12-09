using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zoodle;

namespace BellyRub
{
    /// <summary>
    /// ScriptableObject that defines values such as how many points are awarded for belly rubs
    /// </summary>
    [CreateAssetMenu(fileName = "Cat Global Setting", menuName = "Belly Rub/Cat Global Setting")]
    [Singleton("Initialize", "Instance", 90)]
    public class CatGlobalSetting : ScriptableObject
    {
        public static CatGlobalSetting Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Register() => SingletonUtility.RegisterSO<CatGlobalSetting>();
        public static CatGlobalSetting Initialize() => Resources.Load<CatGlobalSetting>("Cat Global Setting");

        /// <summary>
        /// Length of the interval between rewarding successful rub inputs.
        /// </summary>
        [field: SerializeField]
        public float TickLength { get; private set; }

        /// <summary>
        /// Ddistance to register a minimum intensity rub, as a factor of the length of the shorter screen axis.
        /// </summary>
        [field: SerializeField]
        public float MinDistancePerTick { get; private set; }

        /// <summary>
        /// Distance to register a maximum intensity rub, as a factor of the length of the shorter screen axis.
        /// </summary>
        [field: SerializeField]
        public float MaxDistancePerTick { get; private set; }

        /// <summary>
        /// Number of points to award for a minimum intensity rub, per tick. Floored to an int after any multipliers.
        /// </summary>
        [field: SerializeField]
        public float MinPointRewardPerTick { get; private set; }

        /// <summary>
        /// Number of points to award for a maximum intensity rub, per tick. Floored to an int after any multipliers.
        /// </summary>
        [field: SerializeField]
        public float MaxPointRewardPerTick { get; private set; }

        /// <summary>
        /// Amount of patience to award for a minimum intensity rub, per tick.
        /// </summary>
        [field: SerializeField]
        public float MinPatienceRewardPerTick { get; private set; }

        /// <summary>
        /// Amount of patience to award for a maximum intensity rub, per tick.
        /// </summary>
        [field: SerializeField]
        public float MaxPatienceRewardPerTick { get; private set; }

        /// <summary>
        /// Length of the interval between no belly rub detected and patience starting to drain.
        /// </summary>
        [field: SerializeField]
        public float PatienceDrainDelay { get; private set; }

        /// <summary>
        /// Amount of patience to drain per second, when not rubbing for long enough.
        /// </summary>
        [field: SerializeField]
        public float PatienceDrainPerSecond { get; private set; }

        /// <summary>
        /// Length of the interval between getting attacked and being allowed to touch the cat again.
        /// </summary>
        [field: SerializeField]
        public float DamagedLockOutTime { get; private set; }

        /// <summary>
        /// When attacked, patience is decreased by a set amount.
        /// </summary>
        [field: SerializeField]
        public float PatienceDamagePerHit { get; private set; } = 20.0f;

        /// <summary>
        /// Increase to time scale per 100 pts when at max difficulty
        /// </summary>
        [field: SerializeField]
        public float CatIntensityScalar { get; private set; } = 0.1f;

        [field: SerializeField] public List<CatDifficultySetting> DifficultySettings { get; private set; } = new();
    }
}
