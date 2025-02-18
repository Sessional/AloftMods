using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AloftModFramework.Localization
{

    [Serializable]
    public class Localization
    {
        //Uncomfortable to have to expose it with an arbitrary layer like this, but it lets us
        // hook into Aloft's language list and populate it rather than requiring perfect string
        // matches and gives us a sweet condensed inspector
        public string language;
        public TextAsset localizations;
    }

    [CreateAssetMenu(fileName = "Localization", menuName = "Aloft Mod Framework/Localization")]
    public class LocalizationResource : ScriptableObject
    {
        public Localization[] localizations;
    }
}
