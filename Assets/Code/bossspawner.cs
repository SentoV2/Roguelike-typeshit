using System;
using System.Reflection;
using UnityEngine;

public class bossspawner : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float spacing = 1f;

    private static readonly Vector2[] Directions =
    {
        new Vector2(-1f, -1f), new Vector2(0f, -1f), new Vector2(1f, -1f),
        new Vector2(-1f,  0f),                         new Vector2(1f,  0f),
        new Vector2(-1f,  1f), new Vector2(0f,  1f), new Vector2(1f,  1f)
    };

    private void Start()
    {
        
        enabled = false;

        Type enemyMovementType = FindType("EnemyMovment");
        if (enemyMovementType == null || !typeof(Component).IsAssignableFrom(enemyMovementType))
        {
            Debug.LogError("bossspawner: Could not find a MonoBehaviour named EnemyMovement. Ensure that script compiles and its class is named EnemyMovement.", this);
            Destroy(gameObject);
            return;
        }

        Vector3 origin = transform.position;
        Vector3 scale = transform.lossyScale;
        float stepX = Mathf.Max(spacing, Mathf.Abs(scale.x));
        float stepY = Mathf.Max(spacing, Mathf.Abs(scale.y));

        foreach (Vector2 direction in Directions)
        {
            Vector3 position = origin + new Vector3(direction.x * stepX, direction.y * stepY, 0f);
            GameObject enemy = Instantiate(gameObject, position, transform.rotation);
            enemy.name = gameObject.name + " Enemy";

            bossspawner copiedSpawner = enemy.GetComponent<bossspawner>();
            if (copiedSpawner != null)
            {
                copiedSpawner.enabled = false;
                Destroy(copiedSpawner);
            }

            if (enemy.GetComponent(enemyMovementType) == null)
            {
                enemy.AddComponent(enemyMovementType);
            }
        }

        Destroy(gameObject);
    }

    private static Type FindType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types;
            }

            foreach (Type type in types)
            {
                if (type != null && type.Name == typeName)
                {
                    return type;
                }
            }
        }

        return null;
    }
}