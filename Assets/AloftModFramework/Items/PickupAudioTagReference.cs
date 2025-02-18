using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;

namespace AloftModFramework.Items
{
    [Serializable]
    public class PickupAudioTagReference
    {
        public PickupAudioTag audioTag;
        public Audio_Pickup.PickUpAudioTagID vanillaTag;
        public int id;
        
        public int GetWeightAsInt()
        {
            if (audioTag != null) return audioTag.id;
            if (vanillaTag != Audio_Pickup.PickUpAudioTagID.None) return (int)vanillaTag;
            return id;
        }

        public Audio_Pickup.PickUpAudioTagID  GetAudioTag()
        {
            return (Audio_Pickup.PickUpAudioTagID) GetWeightAsInt();
        }
    }
}
