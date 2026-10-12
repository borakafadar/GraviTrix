using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlyingBlocksManager : MonoBehaviour
{
    [SerializeField] private Image blockPrefab;

    private float spawnTimer;

    private class BlockData
    {
        public RectTransform rect;
        public Vector2 velocity;
        public float rotationSpeed;
    }

    private List<BlockData> activeBlocks = new List<BlockData>();
    private Color[] blockColors = {
        Color.cyan,
        Color.magenta,
        Color.yellow,
        new Color(1f, 0.5f, 0f),
        Color.green,
        new Color(1f, 0.2f, 0.3f)
    };

    private void Start()
    {
        for (int i = 0; i < 8; i++)
        {
            SpawnBlock(true);
        }
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnBlock(false);
            spawnTimer = Random.Range(1.0f, 2.5f);
        }

        for (int i = activeBlocks.Count - 1; i >= 0; i--)
        {
            BlockData b = activeBlocks[i];
            if (b.rect == null)
            {
                activeBlocks.RemoveAt(i);
                continue;
            }

            b.rect.anchoredPosition += b.velocity * Time.deltaTime;
            b.rect.Rotate(0, 0, b.rotationSpeed * Time.deltaTime);

            if (Mathf.Abs(b.rect.anchoredPosition.x) > 1200f || Mathf.Abs(b.rect.anchoredPosition.y) > 2200f)
            {
                Destroy(b.rect.gameObject);
                activeBlocks.RemoveAt(i);
            }
        }
    }

    private void SpawnBlock(bool randomInsideScreen)
    {
        Image img = Instantiate(blockPrefab, transform);

        Color c = blockColors[Random.Range(0, blockColors.Length)];
        c.a = Random.Range(0.1f, 0.3f);
        img.color = c;

        RectTransform rect = img.rectTransform;

        // Boyutlar korundu
        float size = Random.Range(90f, 280f);
        rect.sizeDelta = new Vector2(size, size);

        Vector2 startPos = Vector2.zero;
        Vector2 vel = Vector2.zero;

        float halfW = 1080f / 2f + 100f;
        float halfH = 1920f / 2f + 100f;
        float speed = Random.Range(50f, 250f);

        if (randomInsideScreen)
        {
            startPos = new Vector2(Random.Range(-halfW, halfW), Random.Range(-halfH, halfH));
            vel = new Vector2(Random.Range(-speed, speed), Random.Range(-speed, speed));
        }
        else
        {
            int side = Random.Range(0, 4);
            switch (side)
            {
                case 0:
                    startPos = new Vector2(-halfW, Random.Range(-halfH, halfH));
                    vel = new Vector2(speed, Random.Range(-speed * 0.5f, speed * 0.5f));
                    break;
                case 1:
                    startPos = new Vector2(halfW, Random.Range(-halfH, halfH));
                    vel = new Vector2(-speed, Random.Range(-speed * 0.5f, speed * 0.5f));
                    break;
                case 2:
                    startPos = new Vector2(Random.Range(-halfW, halfW), -halfH);
                    vel = new Vector2(Random.Range(-speed * 0.5f, speed * 0.5f), speed);
                    break;
                case 3:
                    startPos = new Vector2(Random.Range(-halfW, halfW), halfH);
                    vel = new Vector2(Random.Range(-speed * 0.5f, speed * 0.5f), -speed);
                    break;
            }
        }

        rect.anchoredPosition = startPos;

        BlockData bd = new BlockData();
        bd.rect = rect;
        bd.velocity = vel;
        bd.rotationSpeed = Random.Range(-150f, 150f);

        activeBlocks.Add(bd);
    }
}
