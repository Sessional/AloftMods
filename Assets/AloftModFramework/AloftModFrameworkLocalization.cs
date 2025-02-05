using UnityEngine;

namespace AloftModFramework
{
    [CreateAssetMenu(fileName = "Localization", menuName = "AloftModFramework/Localization Resource")]
    public class AloftModFrameworkLocalization : ScriptableObject
    {
        public TextAsset LocalizationFile;
        public string Language;
    }
}
