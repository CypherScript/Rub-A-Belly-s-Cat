using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class CatPupil
{
    // Enum to represent the type of eye
    public enum EyeType
    {
        LeftEye,
        RightEye,
        ThirdEye
    }

    [SerializeField]
    private EyeType eyeType;  // Define the type of the eye

    [FormerlySerializedAs("catPupilSprite")] [SerializeField]
    private SpriteRenderer catPupilSpriteRenderer = null;
    
    [SerializeField]
    private Sprite catPupilLevel1Sprite = null;
    
    [SerializeField]
    private Sprite catPupilLevel2Sprite = null;
    
    [SerializeField]
    private Transform catPupilPosition = null;
    
    [SerializeField]
    private Transform vfxSpawnPoint = null;

    [SerializeField]
    private ParticleSystem eyeVFXPrefab; // VFX prefab specific to this eye type

    // The static center of the eyeball
    [SerializeField] 
    public Vector2 eyeballCenter { get; set; }

    public EyeType TypeOfEye => eyeType;  // Property to get the eye type
    public SpriteRenderer CatPupilSpriteRenderer => catPupilSpriteRenderer;
    
    public Sprite CatPupilLevel1Sprite => catPupilLevel1Sprite;
    public Sprite CatPupilLevel2Sprite => catPupilLevel2Sprite;
    public Transform CatPupilPosition => catPupilPosition;
    public Transform VFXSpawnPoint => vfxSpawnPoint;
    public ParticleSystem EyeVFXPrefab => eyeVFXPrefab; // Property to get the VFX prefab
}