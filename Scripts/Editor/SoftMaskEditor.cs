using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;


namespace Coffee.UISoftMask
{
    /// <summary>
    /// SoftMask editor.
    /// </summary>
    [CustomEditor(typeof(SoftMask))]
    [CanEditMultipleObjects]
    public class SoftMaskEditor : Editor
    {
        const string k_PrefsPreview = "SoftMaskEditor_Preview";
        static readonly List<Graphic> s_Graphics = new List<Graphic>();
        static bool s_Preview;

        private void OnEnable()
        {
            s_Preview = EditorPrefs.GetBool(k_PrefsPreview, false);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var current = target as SoftMask;
            current.GetComponentsInChildren<Graphic>(true, s_Graphics);
            var fixTargets = s_Graphics.Where(x =>
                x.gameObject != current.gameObject && !x.GetComponent<SoftMaskable>() &&
                (!x.GetComponent<Mask>() || x.GetComponent<Mask>().showMaskGraphic)).ToList();
            if (0 < fixTargets.Count)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(
                    "There are child Graphics that does not have a SoftMaskable component.\nAdd SoftMaskable component to them.",
                    MessageType.Warning);
                GUILayout.BeginVertical();
                if (GUILayout.Button("Fix"))
                {
                    foreach (var p in fixTargets)
                    {
                        p.gameObject.AddComponent<SoftMaskable>();
                    }

                    EditorUtils.MarkPrefabDirty();
                }

                if (GUILayout.Button("Ping"))
                {
                    EditorGUIUtility.PingObject(fixTargets[0]);
                }

                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            // Preview buffer.
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (s_Preview != (s_Preview = EditorGUILayout.ToggleLeft("Preview Buffer", s_Preview,
                GUILayout.MaxWidth(EditorGUIUtility.labelWidth))))
            {
                EditorPrefs.SetBool(k_PrefsPreview, s_Preview);
            }

            if (s_Preview)
            {
                var tex = current.softMaskBuffer;
                var width = tex.width * 64 / tex.height;
                EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(width, 64), tex, null, ScaleMode.ScaleToFit);
                Repaint();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }


        //%%%% Context menu for editor %%%%
        [MenuItem("CONTEXT/Mask/Convert To SoftMask", true)]
        static bool _ConvertToSoftMask(MenuCommand command)
        {
            return EditorUtils.CanConvertTo<SoftMask>(command.context);
        }

        [MenuItem("CONTEXT/Mask/Convert To SoftMask", false)]
        static void ConvertToSoftMask(MenuCommand command)
        {
            EditorUtils.ConvertTo<SoftMask>(command.context);
        }

        [MenuItem("CONTEXT/Mask/Convert To Mask", true)]
        static bool _ConvertToMask(MenuCommand command)
        {
            return EditorUtils.CanConvertTo<Mask>(command.context);
        }

        [MenuItem("CONTEXT/Mask/Convert To Mask", false)]
        static void ConvertToMask(MenuCommand command)
        {
            EditorUtils.ConvertTo<Mask>(command.context);
        }
    }
}
