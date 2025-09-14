using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

[InitializeOnLoad]
public class FixURPMaterialsOnLoad
{
    static FixURPMaterialsOnLoad()
    {
        EditorApplication.update += CheckAndFixMaterials;
    }

    static void CheckAndFixMaterials()
    {
        EditorApplication.update -= CheckAndFixMaterials;

        // Автоматически исправляем материалы при запуске
        if (EditorSceneManager.GetActiveScene().isLoaded)
        {
            FixAllPurpleMaterialsInScene();
        }
    }

    [MenuItem("Tools/Fix Purple Materials Now")]
    static void FixAllPurpleMaterialsInScene()
    {
        Renderer[] renderers = Object.FindObjectsOfType<Renderer>();
        int fixedCount = 0;

        Debug.Log($"🔧 Starting to fix purple materials. Found {renderers.Length} renderers...");

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            bool changed = false;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].shader.name == "Standard")
                {
                    string oldPath = AssetDatabase.GetAssetPath(materials[i]);
                    Debug.Log($"Found Standard material: {materials[i].name} on {renderer.name} at {oldPath}");

                    if (!string.IsNullOrEmpty(oldPath))
                    {
                        string newPath = oldPath.Replace(".mat", "_URP.mat");
                        Material newMat = AssetDatabase.LoadAssetAtPath<Material>(newPath);

                        if (newMat != null)
                        {
                            materials[i] = newMat;
                            changed = true;
                            Debug.Log($"✅ Replaced: {renderer.name} -> {newMat.name}");
                        }
                        else
                        {
                            // Создаем URP материал, если не найден
                            Material urpMat = CreateURPMaterial(materials[i]);
                            if (urpMat != null)
                            {
                                materials[i] = urpMat;
                                changed = true;
                                Debug.Log($"✅ Created and replaced: {renderer.name} -> {urpMat.name}");
                            }
                        }
                    }
                }
            }

            if (changed)
            {
                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log($"🔧 Fixed {fixedCount} objects with purple materials.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        else
        {
            Debug.Log("🔧 No purple materials found to fix.");
        }
    }

    static Material CreateURPMaterial(Material standardMaterial)
    {
        try
        {
            Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLitShader == null)
            {
                Debug.LogError("URP Lit Shader not found!");
                return null;
            }

            string oldPath = AssetDatabase.GetAssetPath(standardMaterial);
            string newPath = oldPath.Replace(".mat", "_URP.mat");

            // Проверяем существование
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(newPath);
            if (existing != null)
            {
                return existing;
            }

            Material urpMaterial = new Material(urpLitShader);

            // Копируем свойства
            if (standardMaterial.HasProperty("_MainTex"))
            {
                urpMaterial.SetTexture("_BaseMap", standardMaterial.GetTexture("_MainTex"));
                urpMaterial.SetTextureScale("_BaseMap", standardMaterial.GetTextureScale("_MainTex"));
                urpMaterial.SetTextureOffset("_BaseMap", standardMaterial.GetTextureOffset("_MainTex"));
            }

            if (standardMaterial.HasProperty("_Color"))
            {
                urpMaterial.SetColor("_BaseColor", standardMaterial.GetColor("_Color"));
            }

            if (standardMaterial.HasProperty("_BumpMap"))
            {
                urpMaterial.SetTexture("_BumpMap", standardMaterial.GetTexture("_BumpMap"));
            }

            if (standardMaterial.HasProperty("_Metallic"))
            {
                urpMaterial.SetFloat("_Metallic", standardMaterial.GetFloat("_Metallic"));
            }

            if (standardMaterial.HasProperty("_Glossiness"))
            {
                urpMaterial.SetFloat("_Smoothness", standardMaterial.GetFloat("_Glossiness"));
            }

            AssetDatabase.CreateAsset(urpMaterial, newPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Created URP material: {Path.GetFileName(newPath)}");
            return urpMaterial;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating URP material: {e.Message}");
            return null;
        }
    }
}