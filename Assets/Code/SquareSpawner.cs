using UnityEngine;

public class SquareSpawner : MonoBehaviour {
    [Header("Spawn")]
    [SerializeField] private Sprite squareSprite;
    [SerializeField] private Vector2 spawnArea = new Vector2(4f, 2f);
    [SerializeField] private Vector2 squareSize = Vector2.one;
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int maxSquares = 20;
    [SerializeField] private Color squareColor = Color.white;
    [SerializeField] private bool spawnOnStart = true;

    private float nextSpawnTime;
    private int spawnedCount;
    private static Sprite defaultSquare;

    private void Start() {
        if (spawnOnStart) SpawnSquare();
        nextSpawnTime = Time.time + Mathf.Max(0.01f, spawnInterval);
    }

    private void Update() {
        if (spawnedCount >= maxSquares || Time.time < nextSpawnTime) return;
        SpawnSquare();
        nextSpawnTime = Time.time + Mathf.Max(0.01f, spawnInterval);
    }

    public void SpawnSquare() {
        if (spawnedCount >= maxSquares) return;

        Vector2 offset = new Vector2(
            Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
            Random.Range(-spawnArea.y * 0.5f, spawnArea.y * 0.5f));
        GameObject square = new GameObject("Spawned Square");
        square.transform.position = transform.position + (Vector3)offset;
        square.transform.localScale = new Vector3(squareSize.x, squareSize.y, 1f);

        SpriteRenderer renderer = square.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite != null ? squareSprite : GetDefaultSquare();
        renderer.color = squareColor;
        spawnedCount++;
    }

    private static Sprite GetDefaultSquare() {
        if (defaultSquare == null) {
            Texture2D texture = Texture2D.whiteTexture;
            defaultSquare = Sprite.Create(texture,
                new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f);
        }
        return defaultSquare;
    }

    private void OnValidate() {
        spawnArea = new Vector2(Mathf.Max(0f, spawnArea.x), Mathf.Max(0f, spawnArea.y));
        squareSize = new Vector2(Mathf.Max(0.01f, squareSize.x), Mathf.Max(0.01f, squareSize.y));
        spawnInterval = Mathf.Max(0.01f, spawnInterval);
        maxSquares = Mathf.Max(0, maxSquares);
    }
}