﻿using System;
using UnityEditor;
using UnityEngine;

namespace HT.Framework
{
    internal sealed class StepParameterWindow : HTFEditorWindow, ILocalizeWindow
    {
        public static void ShowWindow(StepEditorWindow stepEditorWindow, StepContentAsset contentAsset, StepContent content, Language language)
        {
            StepParameterWindow window = GetWindow<StepParameterWindow>();
            window.CurrentLanguage = language;
            window.titleContent.image = EditorGUIUtility.IconContent("d_editicon.sml").image;
            window.titleContent.text = "Parameters";
            window._stepEditorWindow = stepEditorWindow;
            window._contentAsset = contentAsset;
            window._content = content;
            window.minSize = new Vector2(300, 300);
            window.maxSize = new Vector2(300, 900);
            window.Show();
        }

        private StepEditorWindow _stepEditorWindow;
        private StepContentAsset _contentAsset;
        private StepContent _content;
        private Vector2 _scroll;
        
        protected override void OnTitleGUI()
        {
            base.OnTitleGUI();

            if (GUILayout.Button(_content.Name, EditorStyles.toolbarPopup))
            {
                GenericMenu gm = new GenericMenu();
                for (int i = 0; i < _contentAsset.Content.Count; i++)
                {
                    StepContent stepContent = _contentAsset.Content[i];
                    gm.AddItem(new GUIContent(i + "." + stepContent.Name), stepContent == _content, () =>
                    {
                        _content = stepContent;
                    });
                }
                gm.ShowAsContext();
            }
            string button = _content.Helper == "<None>" ? GetWord(_content.Helper) : _content.Helper;
            if (GUILayout.Button(button, EditorStyles.toolbarButton))
            {
                if (_content.Helper != "<None>")
                {
                    _stepEditorWindow.OpenHelperScript(_content.Helper);
                }
            }
            GUILayout.FlexibleSpace();
        }
        protected override void OnBodyGUI()
        {
            base.OnBodyGUI();

            GUILayout.BeginVertical(EditorGlobalTools.Styles.Box);
            _scroll = GUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < _content.Parameters.Count; i++)
            {
                StepParameter stepParameter = _content.Parameters[i];

                GUILayout.BeginVertical(EditorStyles.helpBox);

                GUILayout.BeginHorizontal();
                GUILayout.Label(GetWord("Type") + ":", GUILayout.Width(40));
                stepParameter.Type = (StepParameter.ParameterType)EditorGUILayout.EnumPopup(stepParameter.Type, GUILayout.Width(100));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("▲", EditorStyles.miniButtonLeft, GUILayout.Width(20)))
                {
                    if (i > 0)
                    {
                        _content.Parameters.Remove(stepParameter);
                        _content.Parameters.Insert(i - 1, stepParameter);
                        continue;
                    }
                }
                if (GUILayout.Button("▼", EditorStyles.miniButtonMid, GUILayout.Width(20)))
                {
                    if (i < _content.Parameters.Count - 1)
                    {
                        _content.Parameters.Remove(stepParameter);
                        _content.Parameters.Insert(i + 1, stepParameter);
                        continue;
                    }
                }
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button(GetWord("Delete"), EditorStyles.miniButtonRight))
                {
                    _content.Parameters.RemoveAt(i);
                    continue;
                }
                GUI.backgroundColor = Color.white;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(GetWord("Name") + ":", GUILayout.Width(40));
                stepParameter.Name = EditorGUILayout.TextField(stepParameter.Name);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label(GetWord("Value") + ":", GUILayout.Width(40));
                switch (_content.Parameters[i].Type)
                {
                    case StepParameter.ParameterType.String:
                        stepParameter.StringValue = EditorGUILayout.TextField(stepParameter.StringValue);
                        break;
                    case StepParameter.ParameterType.Integer:
                        stepParameter.IntegerValue = EditorGUILayout.IntField(stepParameter.IntegerValue);
                        break;
                    case StepParameter.ParameterType.Float:
                        stepParameter.FloatValue = EditorGUILayout.FloatField(stepParameter.FloatValue);
                        break;
                    case StepParameter.ParameterType.Boolean:
                        stepParameter.BooleanValue = EditorGUILayout.Toggle(stepParameter.BooleanValue);
                        break;
                    case StepParameter.ParameterType.Vector2:
                        stepParameter.Vector2Value = EditorGUILayout.Vector2Field("", stepParameter.Vector2Value);
                        break;
                    case StepParameter.ParameterType.Vector3:
                        stepParameter.Vector3Value = EditorGUILayout.Vector3Field("", stepParameter.Vector3Value);
                        break;
                    case StepParameter.ParameterType.Color:
                        stepParameter.ColorValue = EditorGUILayout.ColorField(stepParameter.ColorValue);
                        break;
                    case StepParameter.ParameterType.GameObject:
                        #region 目标物体丢失，根据目标GUID重新搜寻
                        if (!stepParameter.GameObjectValue)
                        {
                            if (stepParameter.GameObjectGUID != "<None>")
                            {
                                stepParameter.GameObjectValue = GameObject.Find(stepParameter.GameObjectPath);
                                if (!stepParameter.GameObjectValue)
                                {
                                    StepTarget[] targets = FindObjectsOfType<StepTarget>();
                                    foreach (StepTarget target in targets)
                                    {
                                        if (target.GUID == stepParameter.GameObjectGUID && !target.GetComponent<StepPreview>())
                                        {
                                            stepParameter.GameObjectValue = target.gameObject;
                                            stepParameter.GameObjectPath = target.transform.FullName();
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    StepTarget target = stepParameter.GameObjectValue.GetComponent<StepTarget>();
                                    if (!target)
                                    {
                                        target = stepParameter.GameObjectValue.AddComponent<StepTarget>();
                                        target.GUID = stepParameter.GameObjectGUID;
                                        HasChanged(stepParameter.GameObjectValue);
                                    }
                                }
                            }
                        }
                        #endregion

                        GUI.color = stepParameter.GameObjectValue ? Color.white : Color.gray;
                        GameObject objValue = EditorGUILayout.ObjectField(stepParameter.GameObjectValue, typeof(GameObject), true) as GameObject;
                        GUI.color = Color.white;

                        #region 目标改变
                        if (objValue != stepParameter.GameObjectValue)
                        {
                            if (objValue)
                            {
                                StepTarget target = objValue.GetComponent<StepTarget>();
                                if (!target)
                                {
                                    target = objValue.AddComponent<StepTarget>();
                                    HasChanged(objValue);
                                }
                                if (target.GUID == "<None>")
                                {
                                    target.GUID = Guid.NewGuid().ToString();
                                }
                                stepParameter.GameObjectValue = objValue;
                                stepParameter.GameObjectGUID = target.GUID;
                                stepParameter.GameObjectPath = objValue.transform.FullName();
                            }
                        }
                        #endregion
                        break;
                    case StepParameter.ParameterType.Texture:
                        GUI.color = stepParameter.TextureValue ? Color.white : Color.gray;
                        stepParameter.TextureValue = EditorGUILayout.ObjectField(stepParameter.TextureValue, typeof(Texture), false) as Texture;
                        GUI.color = Color.white;
                        break;
                    case StepParameter.ParameterType.AudioClip:
                        GUI.color = stepParameter.AudioClipValue ? Color.white : Color.gray;
                        stepParameter.AudioClipValue = EditorGUILayout.ObjectField(stepParameter.AudioClipValue, typeof(AudioClip), false) as AudioClip;
                        GUI.color = Color.white;
                        break;
                    case StepParameter.ParameterType.Material:
                        GUI.color = stepParameter.MaterialValue ? Color.white : Color.gray;
                        stepParameter.MaterialValue = EditorGUILayout.ObjectField(stepParameter.MaterialValue, typeof(Material), false) as Material;
                        GUI.color = Color.white;
                        break;
                }
                GUILayout.EndHorizontal();

                if (_content.Parameters[i].Type == StepParameter.ParameterType.GameObject)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(GetWord("GUID") + ":", GUILayout.Width(45));
                    string guid = _content.Parameters[i].GameObjectGUID;
                    EditorGUILayout.TextField(guid == "<None>" ? GetWord(guid) : guid);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(GetWord("Clear"), EditorStyles.miniButton, GUILayout.Width(45)))
                    {
                        _content.Parameters[i].GameObjectValue = null;
                        _content.Parameters[i].GameObjectGUID = "<None>";
                        _content.Parameters[i].GameObjectPath = "<None>";
                        GUI.FocusControl(null);
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(GetWord("Add"), EditorGlobalTools.Styles.ButtonLeft))
            {
                _content.Parameters.Add(new StepParameter());
            }
            if (GUILayout.Button(GetWord("Clear"), EditorGlobalTools.Styles.ButtonRight))
            {
                string prompt = CurrentLanguage == Language.English ? "Are you sure delete all parameter？" : "你确定要删除所有的参数吗？";
                if (EditorUtility.DisplayDialog(GetWord("Prompt"), prompt, GetWord("Yes"), GetWord("No")))
                {
                    _content.Parameters.Clear();
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(GetWord("Apply")))
            {
                GUI.FocusControl(null);
                HasChanged(_contentAsset);
            }
            GUILayout.EndHorizontal();
        }
        protected override void GenerateWords()
        {
            base.GenerateWords();

            AddWord("类型", "Type");
            AddWord("删除", "Delete");
            AddWord("名称", "Name");
            AddWord("值", "Value");
            AddWord("身份号", "GUID");
            AddWord("清空", "Clear");
            AddWord("添加", "Add");
            AddWord("应用", "Apply");
            AddWord("提示", "Prompt");
            AddWord("是的", "Yes");
            AddWord("不", "No");
            AddWord("<无>", "<None>");
        }
        private void Update()
        {
            if (_stepEditorWindow == null || _contentAsset == null || _content == null)
            {
                Close();
            }
        }
    }
}