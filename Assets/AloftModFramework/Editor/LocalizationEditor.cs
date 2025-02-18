#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using AloftModFramework.Localization;
using UnityEditor;

using UnityEngine;

namespace AloftModFramework
{
    [CustomPropertyDrawer(typeof(Localization.Localization))]
    public class LocalizationEditor : PropertyDrawer
    {
        private readonly string[] options;

        public LocalizationEditor()
        {
            var numLanguages = Utilities.Localization.GetLanguageCount();
            var languages = new List<string>();
            for (int i = 0; i < numLanguages; i++)
            {
                languages.Add(Utilities.Localization.GetLanguageName(i));
            }

            options = languages.ToArray();
        }
            
            
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int index = 0;
            var targetProp = property.FindPropertyRelative(nameof(Localization.Localization.language));
            if (targetProp.stringValue != "")
            {
                index = Array.FindIndex(options, x => x == targetProp.stringValue);
            }
            var newSelection = EditorGUI.Popup(new Rect(position.x, position.y, position.width / 2f, position.height), index, options);
            targetProp.stringValue = options[newSelection];

            var textAssetProp = property.FindPropertyRelative(nameof(Localization.Localization.localizations));
            EditorGUI.PropertyField(new Rect(position.x + position.width / 2f, position.y, position.width / 2f, position.height), textAssetProp, GUIContent.none);
        }
    }
}
#endif