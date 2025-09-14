using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class URPMaterialConverter : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> logMessages = new List<string>();
    private bool showLog = true;

    [MenuItem("Tools/URP Material Converter")]
    public static void ShowWindow()
    {
        GetWindow<URPMaterialConverter>("URP Converter");
    }

    void OnGUI()
    {
        GUILayout.Label("URP Material Converter", EditorStyles.boldLabel);
        GUILayout.Label("Converts Standard materials to URP/Lit", EditorStyles.miniLabel);
        GUILayout.Space(10);

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 12;
        buttonStyle.fontStyle = FontStyle.Bold;

        if (GUILayout.Button("1. Fix Purple Materials NOW (Recommended)", buttonStyle, GUILayout.Height(40)))
        {
            FixPurpleMaterialsNow();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("2. Convert All Standard Materials", buttonStyle, GUILayout.Height(30)))
        {
            ConvertAllStandardMaterials();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("3. Force Replace All Materials in Scene", buttonStyle, GUILayout.Height(30)))
        {
            ForceReplaceAllMaterialsInScene();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("4. Refresh Scene and Assets", buttonStyle, GUILayout.Height(30)))
        {
            RefreshEverything();
        }

        GUILayout.Space(15);

        // Лог
        showLog = EditorGUILayout.Foldout(showLog, "Log", true);
        if (showLog)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            GUIStyle logStyle = new GUIStyle(EditorStyles.label);
            logStyle.wordWrap = true;
            logStyle.richText = true;

            foreach (string log in logMessages)
            {
                GUILayout.Label(log, logStyle);
            }
            GUILayout.EndScrollView();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Clear Log", GUILayout.Width(100)))
        {
            logMessages.Clear();
        }
    }

    private void AddLog(string message)
    {
        string logMessage = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        logMessages.Add(logMessage);
        Debug.Log(message);
        Repaint();
    }

    private void FixPurpleMaterialsNow()
    {
        AddLog("🚀 STARTING IMMEDIATE FIX FOR PURPLE MATERIALS...");

        int fixedCount = 0;

        // Получаем все рендереры в сцене
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        AddLog($"Found {renderers.Length} renderers in scene");

        // Принудительно заменяем все Standard материалы
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].shader.name == "Standard")
                {
                    string oldMaterialPath = AssetDatabase.GetAssetPath(materials[i]);
                    AddLog($"Found Standard material: {materials[i].name} on {renderer.name}");

                    if (!string.IsNullOrEmpty(oldMaterialPath))
                    {
                        // Создаем путь к URP версии
                        string urpMaterialPath = oldMaterialPath.Replace(".mat", "_URP.mat");

                        // Загружаем URP материал
                        Material urpMaterial = AssetDatabase.LoadAssetAtPath<Material>(urpMaterialPath);

                        if (urpMaterial != null)
                        {
                            materials[i] = urpMaterial;
                            changed = true;
                            AddLog($"✅ FIXED: {renderer.name} -> {urpMaterial.name}");
                            fixedCount++;
                        }
                        else
                        {
                            // Если URP материал не найден, создаем его
                            AddLog($"Creating URP version for: {materials[i].name}");
                            Material newURPMaterial = CreateURPMaterial(materials[i]);
                            if (newURPMaterial != null)
                            {
                                materials[i] = newURPMaterial;
                                changed = true;
                                AddLog($"✅ CREATED & FIXED: {renderer.name} -> {newURPMaterial.name}");
                                fixedCount++;
                            }
                        }
                    }
                }
            }

            if (changed)
            {
                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }

        // Принудительное обновление
        Scene activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);

        AddLog($"🎉 FIXED {fixedCount} PURPLE MATERIALS!");
        AddLog("🔄 REFRESHING ASSETS...");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"Fixed {fixedCount} purple materials! Check your scene.", "OK");
    }

    private Material CreateURPMaterial(Material standardMaterial)
    {
        try
        {
            Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLitShader == null)
            {
                AddLog("❌ URP Lit Shader not found!");
                return null;
            }

            string oldPath = AssetDatabase.GetAssetPath(standardMaterial);
            string newPath = oldPath.Replace(".mat", "_URP.mat");

            // Проверяем, существует ли уже
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(newPath);
            if (existing != null)
            {
                return existing;
            }

            Material urpMaterial = new Material(urpLitShader);
            CopyMaterialProperties(standardMaterial, urpMaterial);

            AssetDatabase.CreateAsset(urpMaterial, newPath);
            AssetDatabase.SaveAssets();

            AddLog($"Created URP material: {Path.GetFileName(newPath)}");
            return urpMaterial;
        }
        catch (System.Exception e)
        {
            AddLog($"Error creating URP material: {e.Message}");
            return null;
        }
    }

    private void CopyMaterialProperties(Material source, Material target)
    {
        try
        {
            // Base Map
            if (source.HasProperty("_MainTex"))
            {
                Texture mainTex = source.GetTexture("_MainTex");
                if (mainTex != null)
                {
                    target.SetTexture("_BaseMap", mainTex);
                    target.SetTextureScale("_BaseMap", source.GetTextureScale("_MainTex"));
                    target.SetTextureOffset("_BaseMap", source.GetTextureOffset("_MainTex"));
                }
            }

            // Base Color
            if (source.HasProperty("_Color"))
            {
                target.SetColor("_BaseColor", source.GetColor("_Color"));
            }

            // Normal Map
            if (source.HasProperty("_BumpMap"))
            {
                Texture bumpMap = source.GetTexture("_BumpMap");
                if (bumpMap != null)
                {
                    target.SetTexture("_BumpMap", bumpMap);
                    if (source.HasProperty("_BumpScale"))
                    {
                        target.SetFloat("_BumpScale", source.GetFloat("_BumpScale"));
                    }
                }
            }

            // Metallic/Smoothness
            if (source.HasProperty("_MetallicGlossMap") && source.GetTexture("_MetallicGlossMap") != null)
            {
                target.SetTexture("_MetallicGlossMap", source.GetTexture("_MetallicGlossMap"));
            }
            else
            {
                if (source.HasProperty("_Metallic"))
                {
                    target.SetFloat("_Metallic", source.GetFloat("_Metallic"));
                }
                if (source.HasProperty("_Glossiness"))
                {
                    target.SetFloat("_Smoothness", source.GetFloat("_Glossiness"));
                }
            }

            // Render Mode
            if (source.HasProperty("_Mode"))
            {
                float mode = source.GetFloat("_Mode");
                target.SetFloat("_Surface", mode == 3 ? 1 : 0);
            }
        }
        catch (System.Exception e)
        {
            AddLog($"Warning copying properties: {e.Message}");
        }
    }

    private void ConvertAllStandardMaterials()
    {
        AddLog("Converting all Standard materials to URP...");

        string[] guids = AssetDatabase.FindAssets("t:Material");
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");

        if (urpLitShader == null)
        {
            AddLog("❌ URP Lit Shader not found!");
            return;
        }

        int converted = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".mat")) continue;

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.shader.name == "Standard")
            {
                string urpPath = path.Replace(".mat", "_URP.mat");

                // Проверяем существование
                if (AssetDatabase.LoadAssetAtPath<Material>(urpPath) == null)
                {
                    Material urpMat = new Material(urpLitShader);
                    CopyMaterialProperties(mat, urpMat);
                    AssetDatabase.CreateAsset(urpMat, urpPath);
                    converted++;
                    AddLog($"Converted: {mat.name}");
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AddLog($"✅ Converted {converted} materials to URP");
    }

    private void ForceReplaceAllMaterialsInScene()
    {
        AddLog("Force replacing all materials in scene...");

        Renderer[] renderers = FindObjectsOfType<Renderer>();
        int replaced = 0;

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    string oldPath = AssetDatabase.GetAssetPath(materials[i]);
                    if (!string.IsNullOrEmpty(oldPath))
                    {
                        string newPath = oldPath.Replace(".mat", "_URP.mat");
                        Material newMat = AssetDatabase.LoadAssetAtPath<Material>(newPath);

                        if (newMat != null && materials[i] != newMat)
                        {
                            materials[i] = newMat;
                            changed = true;
                            AddLog($"Replaced: {renderer.name} -> {newMat.name}");
                            replaced++;
                        }
                    }
                }
            }

            if (changed)
            {
                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }

        Scene activeScene = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(activeScene);

        AddLog($"✅ Force replaced {replaced} materials");
    }

    private void RefreshEverything()
    {
        AddLog("Refreshing everything...");

        // Сохраняем сцену
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.isDirty)
        {
            EditorSceneManager.SaveScene(activeScene);
        }

        // Обновляем ассеты
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Перезагружаем сцену
        EditorSceneManager.OpenScene(activeScene.path);

        AddLog("✅ Everything refreshed!");
        EditorUtility.DisplayDialog("Refreshed", "Scene and assets refreshed! Purple materials should be fixed now.", "OK");
    }
}