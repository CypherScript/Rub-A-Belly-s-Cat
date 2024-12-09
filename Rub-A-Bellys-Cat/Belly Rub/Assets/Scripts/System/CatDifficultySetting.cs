using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BellyRub
{
    [CreateAssetMenu(fileName = "Cat Difficulty Setting", menuName = "Belly Rub/Cat Difficulty Setting")]
    public class CatDifficultySetting : SerializedScriptableObject
    {
        [field: SerializeField]
        public int ScoreThreshold { get; private set; }

        [field: SerializeField]
        public AudioClip Theme { get; private set; }

        [field: SerializeField]
        public List<CatAttackSource> NewlyActivatedAttacks { get; private set; } = new();

        [field: SerializeField]
        public CatAttackPattern FirstAttackPattern { get; private set; } = null;

        [field: SerializeField]
        public List<CatAttackPattern> AttackPatterns { get; private set; } = new();
    }
}