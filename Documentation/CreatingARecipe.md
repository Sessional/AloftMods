## Creating your first recipe

Our first recipe is going to be to turn Wood into Charcoal.


Right click in the asset browser and choose to create a new recipe.

![Screenshot showing where to find the button to create a crafting recipe.](Images/Create_New_Recipe.png)

Tweak the settings for the recipe to turn wood into coal:

![Screenshot showing the set up of the recipe](Images/Set_Up_Recipe.png)

To create this recipe, wire up `input items`, which is referencing `ItemIds`. `3` = Wood. For output choose one of the fields: `Output Vanilla Item`, `Output Item Id`, `Output Mod Item`. `Output Vanilla Item` is useful for quickly picking the item without having to research IDs. `Output Item Id` is great for hooking a new recipe into another content mods Items. `Output Mod Item` is great for referencing items created inside the same content bundle.

Launch the game and explore the new recipe!
![Screenshot showing the new recipe in the game](Images/Use_Recipe.png)
