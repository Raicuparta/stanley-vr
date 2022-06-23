using UnityEngine;

using UnityEditor.XR.OpenXR.Features;

namespace UnityEditor.XR.OpenXR
{
    [CustomEditor(typeof(OpenXRPackageSettings))]
    internal class PackageSettingsEditor : UnityEditor.Editor
    {
        OpenXRFeatureEditor m_FeatureEditor = null;
        Vector2 scrollPos = Vector2.zero;

        static class Content
        {
            public const float k_Space = 15.0f;
        }

        public void Awake()
        {
            m_FeatureEditor = OpenXRFeatureEditor.CreateFeatureEditor();
        }

        public override void OnInspectorGUI()
        {
            var buildTargetGroup = EditorGUILayout.BeginBuildTargetSelectionGrouping();
            OpenXRProjectValidationWindow.SetSelectedBuildTargetGroup(buildTargetGroup);

            OpenXRPackageSettings settings = serializedObject.targetObject as OpenXRPackageSettings;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            EditorGUILayout.BeginVertical();

            var openXrSettings = settings.GetSettingsForBuildTargetGroup(buildTargetGroup);
            var serializedOpenXrSettings = new SerializedObject(openXrSettings);

            EditorGUIUtility.labelWidth = 200;
            DrawPropertiesExcluding(serializedOpenXrSettings, "m_Script");
            EditorGUIUtility.labelWidth = 0;

            if (serializedOpenXrSettings.hasModifiedProperties)
                serializedOpenXrSettings.ApplyModifiedProperties();

            if (buildTargetGroup == BuildTargetGroup.Standalone)
            {
                EditorGUILayout.Space();
                OpenXRRuntimeSelector.DrawSelector();
            }

            EditorGUILayout.EndVertical();


            if (m_FeatureEditor != null)
            {
                EditorGUILayout.Space();
                m_FeatureEditor.OnGUI(buildTargetGroup);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndBuildTargetSelectionGrouping();

            EditorGUILayout.EndScrollView();

        }
    }
}