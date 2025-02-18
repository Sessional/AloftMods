using System.Collections;
using System.Collections.Generic;
using Scriptable_Objects;
using UnityEngine;

namespace AloftModLoader
{
    /// <summary>
    /// This class exists to provide the ability to attach a recipe
    /// to an existing station utilizing a postfix hook into the get
    /// recipe group call.
    ///
    /// Constructing a recipe, which in the framework contains a reference
    /// to the station it belongs to, the station needs to be retained to
    /// allow the postfix call to find the recipes belonging to the station.
    ///
    /// Without this here, the predefined recipe groups (workbench/hands),
    /// can not be appended to because they are instances of ScriptableObjects
    /// that are defined in the build and there would be no way to understand
    /// what recipe goes to what station.
    /// </summary>
    public class AloftModLoaderCraftingRecipe : ScriptableCraftRecipe
    {
        public SRecipeManager.CraftingStation craftingStation;
    }
}
