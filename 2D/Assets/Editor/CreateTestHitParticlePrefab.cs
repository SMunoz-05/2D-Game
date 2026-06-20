using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class CreateTestHitParticlePrefab
{
    [MenuItem("Tools/Create Test Hit Particle Prefab and Assign to Enemies")]
    public static void CreateAndAssign()
    {
        // Ensure folder
        string folder = "Assets/TestData/Prefabs";
        if (!AssetDatabase.IsValidFolder("Assets/TestData"))
        {
            AssetDatabase.CreateFolder("Assets", "TestData");
        }
        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets/TestData", "Prefabs");
        }

        // Create temporary GameObject
        GameObject go = new GameObject("TestHitParticles");
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.35f;
        main.startSpeed = 2.5f;
        main.startSize = 0.12f;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 100;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 8) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.02f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.6f,0.8f,1f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        // Add a renderer and simple material
        var psr = ps.GetComponent<ParticleSystemRenderer>();
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = Color.white;
        psr.sharedMaterial = mat;

        // Save prefab
        string prefabPath = folder + "/TestHitParticles.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        if (prefab != null)
        {
            Debug.Log($"Created test particle prefab at: {prefabPath}");
        }
        else
        {
            Debug.LogError("Failed to create prefab.");
            return;
        }

        // Assign to all FlashEffect2D components in scene; also add FlashEffect2D to EnemyBase objects
        int assignedCount = 0;

        // Add/assign to enemies in scene
        var enemies = Object.FindObjectsOfType<MonoBehaviour>();
        foreach (var mb in enemies)
        {
            if (mb == null) continue;
            var type = mb.GetType();
            // A rough check: check if object inherits from EnemyBase by name comparison to avoid assembly reference
            if (type.BaseType != null && type.BaseType.Name == "EnemyBase")
            {
                var enemyObj = ((MonoBehaviour)mb).gameObject;
                var flash = enemyObj.GetComponent<FlashEffect2D>();
                if (flash == null)
                {
                    flash = enemyObj.AddComponent<FlashEffect2D>();
                    Debug.Log($"Added FlashEffect2D to enemy: {enemyObj.name}");
                }

                // Assign prefab via SerializedObject
                SerializedObject so = new SerializedObject(flash);
                var prop = so.FindProperty("hitParticlePrefab");
                if (prop != null)
                {
                    prop.objectReferenceValue = prefab;
                    so.ApplyModifiedProperties();
                    assignedCount++;
                }
            }
        }

        // Also assign to any existing FlashEffect2D in the scene
        var flashes = Object.FindObjectsByType<FlashEffect2D>(FindObjectsInactive.Include);
        foreach (var f in flashes)
        {
            SerializedObject so = new SerializedObject(f);
            var prop = so.FindProperty("hitParticlePrefab");
            if (prop != null && prop.objectReferenceValue == null)
            {
                prop.objectReferenceValue = prefab;
                so.ApplyModifiedProperties();
                assignedCount++;
            }
        }

        Debug.Log($"Assigned test particle prefab to {assignedCount} FlashEffect2D components in scene.");

        // Mark scene dirty so changes persist
        EditorSceneManager.MarkAllScenesDirty();
    }
}
