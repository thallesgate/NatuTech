using UnityEngine;
using UnityEngine.Rendering;

namespace SOG.CVDFilter
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Volume))]
    public class CVDFilter : MonoBehaviour
    {
        Volume postProcessVolume;

        [SerializeField]
        private CVDProfilesSO profiles; // Manually assign the ScriptableObject in the Inspector.

        [SerializeField]
        public VisionTypeNames currentType;

        public VisionTypeInfo SelectedVisionType { get; private set; }

        void Reset()
        {
            Setup();
            ChangeProfile();
        }

        void Start()
        {
            Setup();
            ChangeProfile();
        }

        void Setup()
        {
            ConfigureVolume();

            if (profiles == null)
            {
                Debug.LogErrorFormat("[{0}] ({1}): Error - Please assign the CVDProfilesSO in the Inspector.", GetType().Name, nameof(Setup));
                return;
            }

            SelectedVisionType = profiles.VisionTypes[0];
        }

        void ConfigureVolume()
        {
            postProcessVolume = GetComponent<Volume>();
            postProcessVolume.isGlobal = true;
        }

        public void ChangeProfile()
        {
            if (profiles == null)
            {
                Debug.LogErrorFormat("[{0}] ({1}): Error - Please assign the CVDProfilesSO in the Inspector.", GetType().Name, nameof(ChangeProfile));
                return;
            }

            SelectedVisionType = profiles.VisionTypes[(int)currentType];
            postProcessVolume.profile = SelectedVisionType.profile;
        }
    }

    public enum VisionTypeNames
    {
        Normal,
        Protanopia,
        Protanomaly,
        Deuteranopia,
        Deuteranomaly,
        Tritanopia,
        Tritanomaly,
        Achromatopsia,
        Achromatomaly
    }

    [System.Serializable]
    public struct VisionTypeInfo
    {
        public VisionTypeNames typeName;
        public string description;
        public VolumeProfile profile;
        public Texture2D previewImage;
    }
}
