using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Xio.EditorTools
{
    /// <summary>
    /// 导出原版 Prefabs/Model/* 3D 模型类 prefab 资产（Block1/Blockx2-4/ElvesSkeleton*/SkillEft*/Projectile* 等）。
    /// 运行时这些对象由 Scene3D/GameFX 按代码动态构建（占位 Cube 结构保真目录），
    /// 此处生成同名 prefab 资产用于 Project 窗口结构对齐原版 Addressables 目录。
    /// </summary>
    public static class ModelPrefabExporter
    {
        // 原版 Prefabs/Model/* 键（来自 catalog_1.json m_InternalIds 考古）
        private static readonly string[] Models =
        {
            "Block1", "Blockx2", "Blockx3", "Blockx4",
            "ElvesSkeleton1", "ElvesSkeleton1NoController", "ElvesSkeleton2", "ElvesSkeleton2NoController",
            "ElvesSkeleton3", "ElvesSkeleton3NoController", "ElvesSkeleton4", "ElvesSkeleton4NoController",
            "ElvesSkeleton5", "ElvesSkeleton5NoController", "ElvesSkeleton6", "ElvesSkeleton6NoController",
            "Hit1", "Projectile1", "Projectile2", "PropBox", "PropBoxUI",
            "Shine_pink", "SkillEft1", "SkillEft2", "SkillEft3", "Smoke",
            "Transition_lizi", "WaterOrbitSphere", "ct", "vine", "PhysicMaterial", "PropPhysicMat",
        };

        public static void Run()
        {
            string dir = "Assets/Prefabs/Model";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            int ok = 0;
            foreach (var name in Models)
            {
                var root = new GameObject(name);
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = "Renderer0";
                cube.transform.SetParent(root.transform, false);
                cube.transform.localScale = new Vector3(2.9f, 3.8f, 1.34f); // 原版牌块碰撞体尺寸
                cube.GetComponent<Renderer>().sharedMaterial = null;

                string path = dir + "/" + name + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(root, path);
                ok++;
                Debug.Log("[ModelPrefabExporter] " + name);
            }
            Debug.Log($"[ModelPrefabExporter] done model={ok} -> {dir}");
            AssetDatabase.Refresh();
        }
    }
}