using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SPCR
{
    public class SPCRLoadOptionsWindow : EditorWindow
    {
        public struct SPCRLoadOptions
        {
            public bool enableAll;
            public bool maintainPointHierarchy;
            public bool maintainColliderHierarchy;
            public bool maintainGrabberHierarchy;

            public void LoadDefaultConfig()
            {
                enableAll = true;
                maintainPointHierarchy = true;
                maintainColliderHierarchy = true;
                maintainGrabberHierarchy = true;
            }
        }

        public class HierarchyObject
        {
            public string name;
            //0 : Unspecified
            //1 : SPCRJointdynamicsController
            //2 : SPCRJointDynamicsPoint
            //3 : SPCRJointDynamicsCollider
            //4 : SPCRJointDynamicsGrabber
            public ItemType type = ItemType.UNDEFINED;
            public HierarchyObject parent;
            public List<HierarchyObject> directChild;
            public bool isPartOfPrefab;
            public bool isSceneObject;
            public bool isEnabled;
            public GameObject sceneObject;//The instance id of existing game object
            public string refUniqueID;//The guiid from the saved file

            public object savedData = null;

            public SPCRJointSettingLocalSave.TransformData transformData;

            public HierarchyObject(string name)
            {
                this.name = name;
                parent = null;
                directChild = new List<HierarchyObject>();
                isPartOfPrefab = false;
                isEnabled = true;
                directChild = new List<HierarchyObject>();
                savedData = null;
            }

            public HierarchyObject(SPCRJointDynamicsController controller)
            {
                UpdateVars(controller.transform, ItemType.Controller);
                sceneObject = controller.gameObject;
                parent = null;
                refUniqueID = "";
                isSceneObject = true;
                savedData = null;
            }

            public HierarchyObject(SPCRJointSettingLocalSave.TransformData transformData, HierarchyObject parent)
            {
                this.transformData = transformData;

                name = transformData.GOName;
                this.parent = parent;
                directChild = new List<HierarchyObject>();
                isPartOfPrefab = false;
                isEnabled = true;
                directChild = new List<HierarchyObject>();
                type = ItemType.UNDEFINED;
                savedData = null;
            }

            void UpdateVars(Transform trans, ItemType itemType)
            {
                name = trans.name;
                type = itemType;
                isPartOfPrefab = PrefabUtility.IsPartOfPrefabInstance(trans);
                isEnabled = true;
                directChild = new List<HierarchyObject>();
                savedData = null;
            }

            public void UpdateVars(SPCRJointDynamicsPoint point, SPCRJointSettingLocalSave.SPCRJointDynamicsPointSave pointSave)
            {
                bool isPointNull = point == null;

                type = ItemType.Point;
                isPartOfPrefab = isPointNull ? false : PrefabUtility.IsPartOfPrefabInstance(point.transform);
                isEnabled = true;
                sceneObject = isPointNull ? null : point.gameObject;
                refUniqueID = pointSave.RefUniqueID;
                savedData = pointSave;
            }

            public void UpdateVars(SPCRJointDynamicsCollider collider, SPCRJointSettingLocalSave.SPCRJointDynamicsColliderSave colliderSave)
            {
                bool isColliderNull = collider == null;

                type = ItemType.Collider;
                isPartOfPrefab = isColliderNull ? false : PrefabUtility.IsPartOfPrefabInstance(collider.transform);
                isEnabled = true;
                sceneObject = isColliderNull ? null : collider.gameObject;
                refUniqueID = colliderSave.RefUniqueId;
                savedData = colliderSave;
            }

            public void UpdateVars(SPCRJointDynamicsPointGrabber grabber, SPCRJointSettingLocalSave.SPCRJointDynamicsPointGrabberSave graberSave)
            {
                bool isGrabberNull = grabber == null;

                type = ItemType.Grabber;
                isPartOfPrefab = isGrabberNull ? false : PrefabUtility.IsPartOfPrefabInstance(grabber.transform);
                isEnabled = true;
                sceneObject = isGrabberNull ? null : grabber.gameObject;
                refUniqueID = graberSave.RefUniqueGUIID;
                savedData = graberSave;
            }

            public HierarchyObject GetChild(string name)
            {
                return directChild.Find(obj => obj.name == name);
            }

            public void SetParent(HierarchyObject newParent)
            {
                if (parent == newParent)
                    return;

                if(parent != null)
                    parent.directChild.Remove(this);

                parent = newParent;
                parent.directChild.Add(this);
            }
        }

        public enum ItemType
        {
            UNDEFINED = 0,
            Controller,
            Point,
            Collider,
            Grabber
        }

        public enum eInspectorLang
        {
            日本語,
            English,
        }

        SPCRLoadOptions loadOptions = default(SPCRLoadOptions);

        static SPCRLoadOptionsWindow thisWinodw = null;
        System.Action<bool, bool, HierarchyObject> FuncCallback = null;

        // 0 : Load with old format
        //1 : Load with this new Hierarchy Format
        bool loadWithOldVersion = false;
        bool loadButtonClicked = false;

        eInspectorLang InspectorLang = eInspectorLang.日本語;

        string[] allEnableText = { "全イネーブル", "Enable All" };
        string[] maintainPointText = { "ポイント階層を維持する", "Maintain Point hierarchy" };
        string[] maintainColliderText = { "コライダー階層を維持する", "Maintain Collider hierarchy" };
        string[] maintainGrabberText = { "グラバー階層を維持する", "Maintain Grabber hierarchy" };
        string[] useOldVersionText = { "古いメソッドでロードする", "Load with old method" };
        string[] fileSavedWithOldVer = { "このセーブファイルは古いバージョンでセーブしていますから、古いメソッドでロードします",
            "This file was saved with old version, so we use old method to load it from" };
        string[] cancelText = { "キャンセル", "Cancel" };
        string[] loadText = { "ロード", "Load" };
        string[] hierarchyText = { "階層", "Hierarchy" };
        string[] saveFileHierarchyText = { "セーブファイルの階層", "Save file hierarchy" };
        string[] regenerateTreeText = { "ツリーを再生する", "Regenerate Tree" };

        SPCRJointDynamicsController controller;
        SPCRJointSettingLocalSave.SPCRJointDynamicsControllerSave savedControllerData;

        List<HierarchyObject> hierarchy = new List<HierarchyObject>();
        HierarchyObject rootHierarchy = null;

        List<HierarchyObject> savedHierarchy = new List<HierarchyObject>();
        HierarchyObject savedRootHierarcy = null;
        System.Collections.Generic.List<Object> globalUniqueIdList;

        Vector2 hierarchyScrollPos;
        Vector2 savedHierarScrollPos;

        bool showSavedHierarhcy = false;


        public static void ShowWindow(SPCRJointDynamicsController controller, SPCRJointSettingLocalSave.SPCRJointDynamicsControllerSave savedData, System.Action<bool, bool, HierarchyObject> cbCallbackLoadSettings)
        {
            thisWinodw = GetWindow<SPCRLoadOptionsWindow>();
            thisWinodw.titleContent = new GUIContent("Load Options");
            thisWinodw.FuncCallback = cbCallbackLoadSettings;

            thisWinodw.loadWithOldVersion = savedData.SaveVersion < SPCRJointSettingLocalSave.SAVE_VERSION;

            thisWinodw.loadOptions.LoadDefaultConfig();
            thisWinodw.controller = controller;
            thisWinodw.savedControllerData = savedData;

            thisWinodw.globalUniqueIdList = SPCRJointSettingLocalSave.globalUniqueIdList;

            if (!thisWinodw.loadWithOldVersion)
            {

                thisWinodw.RegenerateHierarchyWithSettings();
                thisWinodw.GenerateHierarchyFromSavedFile();
            }

            thisWinodw.ShowPopup();
        }

        public static void CloseWindow()
        {
            thisWinodw.Close();
        }

        private void OnDestroy()
        {
            //if (FuncCallback != null)
            //{
            //    FuncCallback(loadButtonClicked, loadType, rootHierarchy);
            //}
        }

        public static bool Foldout(bool display, string title, Color color, bool showToggleRect = false)
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;

            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.label).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            if (showToggleRect)
            {
                var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
                if (e.type == EventType.Repaint)
                {
                    EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
                }

                if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                {
                    display = !display;
                    e.Use();
                }
            }

            GUI.backgroundColor = backgroundColor;

            return display;
        }

        private void OnGUI()
        {
            Foldout(true, "Basics", new Color(1.0f, 0.7f, 1.0f));

            InspectorLang = (eInspectorLang)EditorGUILayout.EnumPopup("言語(Language)", InspectorLang);
            int languageIndex = (int)InspectorLang;

            GUILayout.Space(10);

            EditorGUILayout.LabelField("Save/Load Ver : " + SPCRJointSettingLocalSave.SAVE_VERSION);

            GUILayout.Space(5);

            if (savedControllerData.SaveVersion >= SPCRJointSettingLocalSave.SAVE_VERSION)
            {
                EditorGUIUtility.labelWidth = 200;
                loadWithOldVersion = EditorGUILayout.Toggle(useOldVersionText[languageIndex], loadWithOldVersion);
            }

            GUILayout.Space(5);

            if (savedControllerData.SaveVersion >= SPCRJointSettingLocalSave.SAVE_VERSION)
            {
                if (!loadWithOldVersion)
                {
                    DrawLoadOptions(languageIndex);

                    Foldout(true, hierarchyText[languageIndex], new Color(1.0f, 1.0f, 0.7f));
                    DrawHierarchy(languageIndex);

                    showSavedHierarhcy = Foldout(showSavedHierarhcy, saveFileHierarchyText[languageIndex], new Color(0.7f, 1.0f, 1.0f), true);
                    if (showSavedHierarhcy)
                    {
                        DrawSavedHierarchy();
                    }

                    if (GUILayout.Button(regenerateTreeText[languageIndex]))
                    {
                        RegenerateHierarchyWithSettings();
                        GenerateHierarchyFromSavedFile();
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField(fileSavedWithOldVer[languageIndex]);
            }

            DrawButtons(languageIndex);
        }

        void DrawLoadOptions(int languageIndex)
        {
            GUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            loadOptions.enableAll = EditorGUILayout.Toggle(allEnableText[languageIndex], loadOptions.enableAll);
            if (EditorGUI.EndChangeCheck() && loadOptions.enableAll)
            {
                loadOptions.maintainPointHierarchy = true;
                loadOptions.maintainColliderHierarchy = true;
                loadOptions.maintainGrabberHierarchy = true;
                RegenerateHierarchyWithSettings();
            }

            EditorGUI.indentLevel++;

            EditorGUI.BeginDisabledGroup(loadOptions.enableAll);

            EditorGUIUtility.labelWidth = 300;
            EditorGUI.BeginChangeCheck();
            loadOptions.maintainPointHierarchy = EditorGUILayout.Toggle(maintainPointText[languageIndex], loadOptions.maintainPointHierarchy);
            loadOptions.maintainColliderHierarchy = EditorGUILayout.Toggle(maintainColliderText[languageIndex], loadOptions.maintainColliderHierarchy);
            loadOptions.maintainGrabberHierarchy = EditorGUILayout.Toggle(maintainGrabberText[languageIndex], loadOptions.maintainGrabberHierarchy);
            if(EditorGUI.EndChangeCheck())
            {
                RegenerateHierarchyWithSettings();
            }

            EditorGUI.EndDisabledGroup();
        }

        void DrawHierarchy(int languageIndex)
        {
            EditorGUI.indentLevel = 0;
            GUILayout.Space(10);
            EditorGUILayout.LabelField(hierarchyText[languageIndex]);

            GUILayout.Space(5);

            if(rootHierarchy == null)
            {
                EditorGUILayout.LabelField("Root is not specified");
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            GUILayout.BeginHorizontal();

            EditorGUILayout.Toggle(rootHierarchy.name, rootHierarchy.isEnabled);
            DrawHierarchyObjectType(rootHierarchy.type);

            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            hierarchyScrollPos = EditorGUILayout.BeginScrollView(hierarchyScrollPos);

            DrawHierarchy(rootHierarchy, EditorGUI.indentLevel + 1);

            EditorGUILayout.EndScrollView();
        }

        void DrawHierarchy(HierarchyObject current, int indetation = 0)
        {
            for(int i = 0; i < current.directChild.Count; i++)
            {
                EditorGUI.indentLevel = indetation;

                HierarchyObject child = current.directChild[i];

                GUILayout.BeginHorizontal("Badge");

                EditorGUI.BeginDisabledGroup(child.sceneObject == null);
                if (GUILayout.Button("●", GUILayout.Width(20)))
                {
                    PingSceneObject(child);
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(child.isPartOfPrefab || child.sceneObject != null);
                EditorGUI.BeginChangeCheck();

                EditorGUIUtility.labelWidth = 100 + 10 * indetation + 5 * child.name.Length;

                child.isEnabled = EditorGUILayout.Toggle(child.name, child.isEnabled);

                DrawHierarchyObjectType(child.type);

                EditorGUI.indentLevel = 0;

                GUI.color = Color.yellow;
                EditorGUILayout.LabelField(child.sceneObject == null ? "(New)" : "", GUILayout.Width(50));
                GUI.color = Color.white;

                if(EditorGUI.EndChangeCheck())
                {
                    EnableAllChild(child, child.isEnabled);
                }

                if (GUILayout.Button("↑", GUILayout.Width(50)))
                {
                    MoveOneParentUp(child);
                }

                if (GUILayout.Button("↓", GUILayout.Width(50)))
                {
                    MoveOneParentDown(child);
                }
                EditorGUI.indentLevel = indetation;

                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();

                DrawHierarchy(child, indetation + 1);
            }
        }

        void PingSceneObject(HierarchyObject curr)
        {
            if (curr.sceneObject == null)
                return;
            EditorGUIUtility.PingObject(curr.sceneObject);
        }

        void EnableAllChild(HierarchyObject current, bool enable)
        {
            foreach(HierarchyObject child in current.directChild)
            {
                child.isEnabled = enable;
                EnableAllChild(child, enable);
            }
        }

        void MoveOneParentUp(HierarchyObject current)
        {
            HierarchyObject currentParent = current.parent;

            HierarchyObject targetParent = currentParent;

            int index = targetParent.directChild.IndexOf(current);
            int childCount = targetParent.directChild.Count;
            if (childCount <= 1 || index == 0)
            {
                HierarchyObject parentP = targetParent.parent;
                if (parentP == null)
                {
                    //If we have reached beyond the root hierarchy
                    targetParent = rootHierarchy;
                }
                else
                {
                    int parentPChildCount = parentP.directChild.Count;
                    int currentPIndex = parentP.directChild.IndexOf(targetParent);
                    if (parentPChildCount <= 1 || currentPIndex == 0)
                    {
                        targetParent = parentP;
                    }
                    else
                    {
                        currentPIndex--;
                        targetParent = parentP.directChild[currentPIndex];
                    }
                }
            }
            else
            {
                index--;
                targetParent = targetParent.directChild[index];
            }

            current.SetParent(targetParent);
        }

        void MoveOneParentDown(HierarchyObject current)
        {
            HierarchyObject currentParent = current.parent;
            //if (currentParent == rootHierarchy)
            //    return;

            HierarchyObject targetParent = currentParent;


            if (targetParent.directChild.Contains(current))
            {
                int index = targetParent.directChild.IndexOf(current);
                int childCount = targetParent.directChild.Count;
                if (index >= childCount - 1)
                {
                    HierarchyObject parentP = targetParent.parent;
                    if (parentP == null)
                    {
                        //If we have reached beyond the root hierarchy
                        targetParent = rootHierarchy;
                    }
                    else
                    {
                        int parentPChildCount = parentP.directChild.Count;
                        int currentPIndex = parentP.directChild.IndexOf(targetParent);
                        if (currentPIndex >= parentPChildCount - 1)
                        {
                            targetParent = parentP;
                        }
                        else
                        {
                            currentPIndex++;
                            targetParent = parentP.directChild[currentPIndex];
                        }
                    }
                }
                else
                {
                    index++;
                    targetParent = targetParent.directChild[index];
                }
            }

            //while (targetParent.isPartOfPrefab && targetParent != rootHierarchy)
            //{
            //    targetParent = targetParent.parent;
            //}

            current.SetParent(targetParent);
        }

        void DrawSavedHierarchy()
        {
            savedHierarScrollPos = EditorGUILayout.BeginScrollView(savedHierarScrollPos);

            GUILayout.BeginHorizontal();

            EditorGUI.indentLevel = 0;

            EditorGUILayout.LabelField(rootHierarchy.name);
            DrawHierarchyObjectType(rootHierarchy.type);

            GUILayout.EndHorizontal();

            DrawSavedHierarchy(savedRootHierarcy, EditorGUI.indentLevel + 1);
            EditorGUILayout.EndScrollView();
        }

        void DrawSavedHierarchy(HierarchyObject current, int indetation = 0)
        {
            for (int i = 0; i < current.directChild.Count; i++)
            {
                EditorGUI.indentLevel = indetation;

                HierarchyObject child = current.directChild[i];

                GUILayout.BeginHorizontal();

                EditorGUIUtility.labelWidth = child.name.Length;
                EditorGUILayout.LabelField(child.name);

                DrawHierarchyObjectType(child.type);

                GUILayout.EndHorizontal();

                DrawSavedHierarchy(child, indetation + 1);

            }
        }

        void DrawHierarchyObjectType(ItemType type)
        {
            Color defColor = GUI.color;

            switch (type)
            {
                case ItemType.Controller:
                    GUI.color = Color.white;
                    //This is a SPCRJointDynamicsController
                    EditorGUILayout.LabelField("[Controller]");
                    break;
                case ItemType.Point:
                    GUI.color = Color.blue;
                    //This is a SPCRJointDynamicsPoint
                    EditorGUILayout.LabelField("[Point]");
                    break;
                case ItemType.Collider:
                    GUI.color = Color.green;
                    //This is a SPCRJointDynamicsCollider
                    EditorGUILayout.LabelField("[Collider]");
                    break;
                case ItemType.Grabber:
                    GUI.color = Color.red;
                    //This is a SPCRJointDynamicsGrabber
                    EditorGUILayout.LabelField("[Graber]");
                    break;
            }

            GUI.color = defColor;
        }

        void DrawButtons(int languageIndex)
        {
            GUILayout.Space(50);

            EditorGUI.indentLevel = 0;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(cancelText[languageIndex]))
            {
                loadButtonClicked = false;
                CloseWindow();
            }

            if (GUILayout.Button(loadText[languageIndex])) 
            {
                loadButtonClicked = true;
                //CloseWindow();
                if (FuncCallback != null)
                {
                    FuncCallback(loadButtonClicked, loadWithOldVersion, rootHierarchy);
                }
                CloseWindow();
            }

            GUILayout.EndHorizontal();
        }

        void RegenerateHierarchyWithSettings()
        {
            if (controller == null || savedControllerData == null)
                return;
            hierarchy.Clear();
            rootHierarchy = new HierarchyObject(controller);
            hierarchy.Add(rootHierarchy);

            if (savedControllerData.spcrChildJointDynamicsPointList != null)
            {
                List<SPCRJointSettingLocalSave.SPCRJointDynamicsPointSave> pointList = new List<SPCRJointSettingLocalSave.SPCRJointDynamicsPointSave>(savedControllerData.spcrChildJointDynamicsPointList);

                HierarchyObject parent = rootHierarchy;

                for (int i = 0; i < pointList.Count; i++)
                {
                    SPCRJointSettingLocalSave.SPCRJointDynamicsPointSave pointSave = pointList[i];
                    SPCRJointDynamicsPoint point = (SPCRJointDynamicsPoint)globalUniqueIdList.Find(
                            obj =>
                            obj.GetType() == typeof(SPCRJointDynamicsPoint)
                                            && ((SPCRJointDynamicsPoint)obj).UniqueGUIID.Equals(pointSave.RefUniqueID)
                            );

                    //What about the parent list?
                    if (point != null || loadOptions.maintainPointHierarchy)
                    {
                        HierarchyObject curr = CreateArbitoryParentHierarchyAndReturnCurrent(pointSave, rootHierarchy);
                        curr.UpdateVars(point, pointSave);
                    }else
                    {
                        HierarchyObject curr = new HierarchyObject(pointSave.GOName);
                        curr.SetParent(rootHierarchy);
                        curr.UpdateVars(point, pointSave);
                    }
                }
            }

            if (savedControllerData.spcrChildJointDynamicsColliderList != null)
            {
                for (int i = 0; i < savedControllerData.spcrChildJointDynamicsColliderList.Length; i++)
                {
                    SPCRJointSettingLocalSave.SPCRJointDynamicsColliderSave colliderSave = savedControllerData.spcrChildJointDynamicsColliderList[i];

                    SPCRJointDynamicsCollider collider = (SPCRJointDynamicsCollider)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsCollider) && ((SPCRJointDynamicsCollider)obj).UniqueGUIID.Equals(savedControllerData.spcrChildJointDynamicsColliderList[i].RefUniqueId));

                    if (collider != null || loadOptions.maintainColliderHierarchy)
                    {
                        HierarchyObject curr = CreateArbitoryParentHierarchyAndReturnCurrent(colliderSave, rootHierarchy);
                        curr.UpdateVars(collider, colliderSave);
                    }
                    else
                    {
                        HierarchyObject curr = new HierarchyObject(colliderSave.GOName);
                        curr.SetParent(rootHierarchy);
                        curr.UpdateVars(collider, colliderSave);
                    }
                }
            }

            if (savedControllerData.spcrChildJointDynamicsPointGtabberList != null)
            {
                List<SPCRJointDynamicsPointGrabber> grabberList = new List<SPCRJointDynamicsPointGrabber>();
                for (int i = 0; i < savedControllerData.spcrChildJointDynamicsPointGtabberList.Length; i++)
                {
                    SPCRJointSettingLocalSave.SPCRJointDynamicsPointGrabberSave grabberSave = savedControllerData.spcrChildJointDynamicsPointGtabberList[i];
                    SPCRJointDynamicsPointGrabber grabber = (SPCRJointDynamicsPointGrabber)globalUniqueIdList.Find(obj => obj.GetType() == typeof(SPCRJointDynamicsPointGrabber) && ((SPCRJointDynamicsPointGrabber)obj).UniqueGUIID.Equals(grabberSave.RefUniqueGUIID));

                    if (grabber != null || loadOptions.maintainGrabberHierarchy)
                    {
                        HierarchyObject curr = CreateArbitoryParentHierarchyAndReturnCurrent(grabberSave, rootHierarchy);
                        curr.UpdateVars(grabber, grabberSave);
                    }
                    else
                    {
                        HierarchyObject curr = new HierarchyObject(grabberSave.GOName);
                        curr.SetParent(rootHierarchy);
                        curr.UpdateVars(grabber, grabberSave);
                    }
                }
            }

            CheckForSceneGameObjectToLinkWithHierarchy(rootHierarchy, controller.transform);
        }

        void CheckForSceneGameObjectToLinkWithHierarchy(HierarchyObject parentH, Transform parentTrans)
        {
            foreach(var currH in parentH.directChild)
            {
                if (currH.sceneObject != null)
                    continue;

                Transform currT = parentTrans.Find(currH.name);
                if(currT != null)
                {
                    currH.sceneObject = currT.gameObject;
                    CheckForSceneGameObjectToLinkWithHierarchy(currH, currT);
                }

            }
        }

        HierarchyObject CreateArbitoryParentHierarchyAndReturnCurrent(SPCRJointSettingLocalSave.TransformData currentTransform, HierarchyObject hRoot)
        {
            List<SPCRJointSettingLocalSave.TransformData> newPlist = new List<SPCRJointSettingLocalSave.TransformData>();
            if (currentTransform.ParentList != null || currentTransform.ParentList.Count > 0)
                newPlist.AddRange(currentTransform.ParentList);
            else
                newPlist.Add(currentTransform);
            newPlist.Reverse();

            //After reverse the first element is always the controller
            //So skip that element
            //Because anyhow we need to make the root as a controller so even if it's not the same we can make our current hRoot as the main root

            HierarchyObject lastHO = hRoot;
            for (int i = 1; i < newPlist.Count; i++)
            {
                HierarchyObject curr = lastHO.GetChild(newPlist[i].GOName);
                if (curr == null)
                {
                    curr = new HierarchyObject(newPlist[i], lastHO);
                    hierarchy.Add(curr);

                    lastHO.directChild.Add(curr);
                }

                lastHO = curr;
            }

            return lastHO;
        }

        void GenerateHierarchyFromSavedFile()
        {
            if (savedControllerData == null)
                return;

            savedHierarchy.Clear();
            savedRootHierarcy = new HierarchyObject(savedControllerData.rootTransformName);
            savedHierarchy.Add(savedRootHierarcy);


            if (savedControllerData.spcrChildJointDynamicsPointList != null)
            {
                List<SPCRJointSettingLocalSave.SPCRJointDynamicsPointSave> pointList = new List<SPCRJointSettingLocalSave.SPCRJointDynamicsPointSave>(savedControllerData.spcrChildJointDynamicsPointList);

                HierarchyObject parent = rootHierarchy;

                for (int i = 0; i < pointList.Count; i++)
                {
                    SPCRJointSettingLocalSave.SPCRJointDynamicsPointSave pointSave = pointList[i];

                    //What about the parent list?

                    HierarchyObject curr = CreateArbitoryParentHierarchyAndReturnCurrent(pointSave, savedRootHierarcy);
                    curr.UpdateVars(null, pointSave);
                }
            }

            if (savedControllerData.spcrChildJointDynamicsColliderList != null)
            {
                for (int i = 0; i < savedControllerData.spcrChildJointDynamicsColliderList.Length; i++)
                {
                    SPCRJointSettingLocalSave.SPCRJointDynamicsColliderSave colliderSave = savedControllerData.spcrChildJointDynamicsColliderList[i];

                    HierarchyObject curr = CreateArbitoryParentHierarchyAndReturnCurrent(colliderSave, savedRootHierarcy);
                    curr.UpdateVars(null, colliderSave);
                }
            }

            if (savedControllerData.spcrChildJointDynamicsPointGtabberList != null)
            {
                List<SPCRJointDynamicsPointGrabber> grabberList = new List<SPCRJointDynamicsPointGrabber>();
                for (int i = 0; i < savedControllerData.spcrChildJointDynamicsPointGtabberList.Length; i++)
                {
                    SPCRJointSettingLocalSave.SPCRJointDynamicsPointGrabberSave grabberSave = savedControllerData.spcrChildJointDynamicsPointGtabberList[i];

                    HierarchyObject curr = CreateArbitoryParentHierarchyAndReturnCurrent(grabberSave, savedRootHierarcy);
                    curr.UpdateVars(null, grabberSave);
                }
            }
        }
    }
}
