using System;
using System.Collections;
using System.Collections.Generic;
using GraviTrix.Core;
using UnityEngine;

namespace GraviTrix.Runtime
{
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private Transform boardRoot;
        [SerializeField] private BlockCellView cellPrefab;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Color activePieceTint = new Color(1f, 1f, 1f, 0.9f);
        [SerializeField] private bool autoCenterBoard = true;
        [SerializeField] private Vector2 manualBoardOffset = Vector2.zero;
        [SerializeField] private Sprite flashSprite;

        private readonly List<BlockCellView> spawnedViews = new List<BlockCellView>();
        private Vector2 cachedBoardOffset;
        
        private GameObject gridRoot;
        private List<LineRenderer> gridLines = new List<LineRenderer>();
        private SpriteRenderer gridBackground;

        private void Awake()
        {
            if (cellPrefab != null && cellPrefab.gameObject.scene.IsValid())
            {
                cellPrefab.gameObject.SetActive(false);
            }

            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(70f / 255f, 44f / 255f, 125f / 255f); // #462C7D
            }
        }

        public void Render(BoardGrid board, PieceInstance activePiece, PieceInstance nextPiece, PieceInstance heldPiece, HashSet<Vector2Int> hiddenCells = null)
        {
            ClearViews();
            UpdateDynamicCellSize(board);
            cachedBoardOffset = ComputeBoardOffset(board);

            if (board != null)
            {
                RenderGridBackground(board);
                
                foreach (BlockCellInfo cell in board.GetOccupiedCells())
                {
                    if (hiddenCells != null && hiddenCells.Contains(cell.Position))
                    {
                        continue;
                    }
                    SpawnCell(cell, null);
                }
            }

            if (activePiece != null)
            {
                foreach (BlockCellInfo cell in activePiece.GetWorldCells())
                {
                    SpawnCell(cell, activePieceTint);
                }
            }

            // Always render HOLD box
            RenderPreviewBox("HOLD", 1.5f, 4.0f, heldPiece, board);

            // Always render NEXT box
            float nextX = (board != null ? board.Width - 2.5f : 8.5f);
            RenderPreviewBox("NEXT", nextX, 4.0f, nextPiece, board);
        }

        private void RenderPreviewBox(string label, float xBoardPos, float yBoardPos, PieceInstance piece, BoardGrid board)
        {
            float centerX = xBoardPos * cellSize + cachedBoardOffset.x;
            float centerY = yBoardPos * cellSize + cachedBoardOffset.y;
            
            float bgSize = cellSize * 3f; 
            Vector3 bgPos = new Vector3(centerX, centerY, 5f);
            
            // Outer Box (Border) - sortingOrder -6 to prevent Z-fighting
            SpawnBackgroundBox(bgPos, new Vector2(bgSize + 0.15f * cellSize, bgSize + 0.15f * cellSize), new Color(0.4f, 0.25f, 0.6f, 1f), transform, -6);
            
            // Inner Box - sortingOrder -5
            SpawnBackgroundBox(bgPos, new Vector2(bgSize, bgSize), new Color(0.12f, 0.12f, 0.15f, 1f), transform, -5);
            
            // Text Label
            SpawnLabel(label, new Vector3(centerX, centerY + bgSize * 0.5f - 0.35f * cellSize, 0f), transform);

            // Piece Cells
            if (piece != null)
            {
                List<BlockCellInfo> cells = new List<BlockCellInfo>(piece.GetWorldCellsAtOrigin(Vector2Int.zero));
                if (cells.Count > 0)
                {
                    float minX = float.MaxValue, maxX = float.MinValue;
                    float minY = float.MaxValue, maxY = float.MinValue;
                    foreach (var cell in cells)
                    {
                        minX = Mathf.Min(minX, cell.Position.x);
                        maxX = Mathf.Max(maxX, cell.Position.x);
                        minY = Mathf.Min(minY, -cell.Position.y); 
                        maxY = Mathf.Max(maxY, -cell.Position.y);
                    }
                    
                    float cx = (minX + maxX) * 0.5f;
                    float cy = (minY + maxY) * 0.5f;
                    
                    float blockScale = 0.5f;
                    
                    foreach (var cell in cells)
                    {
                        float localX = (cell.Position.x - cx) * cellSize * blockScale;
                        float localY = (-cell.Position.y - cy) * cellSize * blockScale;
                        SpawnCellVisual(cell, new Vector3(centerX + localX, centerY + localY - 0.15f * cellSize, 0f), blockScale, transform);
                    }
                }
            }
        }

        private void SpawnCellVisual(BlockCellInfo cell, Vector3 localPos, float scale, Transform root)
        {
            if (cellPrefab == null) return;
            BlockCellView view = Instantiate(cellPrefab, root);
            view.gameObject.SetActive(true);
            view.transform.localPosition = localPos;
            view.transform.localScale = new Vector3(scale, scale, 1f);
            view.SetCell(cell, null);
            
            SpriteRenderer sr = view.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 5;
            
            spawnedViews.Add(view);
        }

        private void SpawnLabel(string text, Vector3 pos, Transform root)
        {
            GameObject labelObj = new GameObject("Label");
            if (root != null) labelObj.transform.SetParent(root, false);
            
            TMPro.TextMeshPro tm = labelObj.AddComponent<TMPro.TextMeshPro>();
            tm.text = text;
            tm.alignment = TMPro.TextAlignmentOptions.Center;
            tm.fontSize = 1.5f; 
            tm.color = Color.white;
            tm.fontStyle = TMPro.FontStyles.Bold | TMPro.FontStyles.SmallCaps;
            tm.enableVertexGradient = true;
            tm.colorGradient = new TMPro.VertexGradient(new Color(1f, 0.9f, 0.4f), new Color(1f, 0.8f, 0.2f), new Color(1f, 0.6f, 0f), new Color(1f, 0.5f, 0f));
            tm.characterSpacing = 3f;
            
            RectTransform rt = tm.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(20f, 5f);
            
            labelObj.transform.localPosition = pos;
            tm.sortingOrder = 10;
            
            spawnedViews.Add(labelObj.AddComponent<BlockCellView>());
        }

        private void SpawnBackgroundBox(Vector3 position, Vector2 size, Color color, Transform root, int order = -5)
        {
            GameObject bgObj = new GameObject("PreviewBackground");
            if (root != null) bgObj.transform.SetParent(root, false);
            bgObj.transform.localPosition = position;
            
            SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
            if (gridBackground != null)
            {
                sr.sprite = gridBackground.sprite;
            }
            sr.color = color;
            sr.sortingOrder = order;
            
            bgObj.transform.localScale = new Vector3(size.x, size.y, 1f);
            
            spawnedViews.Add(bgObj.AddComponent<BlockCellView>());
        }

        private void SpawnCell(BlockCellInfo cell, Color? tintOverride, Transform overrideRoot = null)
        {
            if (cellPrefab == null)
            {
                return;
            }

            Transform root = overrideRoot != null ? overrideRoot : (boardRoot != null ? boardRoot : transform);
            BlockCellView view = Instantiate(cellPrefab, root);
            view.gameObject.SetActive(true);
            float x = cell.Position.x * cellSize + cachedBoardOffset.x;
            float y = -cell.Position.y * cellSize + cachedBoardOffset.y;
            view.transform.localPosition = new Vector3(x, y, 0f);
            view.SetCell(cell, tintOverride);

            if (cell.VisualType == BlockVisualType.Slippery)
            {
                var sr = view.GetComponent<SpriteRenderer>();
                if (sr != null) 
                {
                    if (flashSprite != null) 
                    {
                        sr.sprite = flashSprite;
                    }
                    else
                    {
#if UNITY_EDITOR
                        Sprite editorSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Blocks/block_removed.png");
                        if (editorSprite != null) sr.sprite = editorSprite;
#endif
                    }
                    if (tintOverride == null) sr.color = Color.white;
                }
            }

            spawnedViews.Add(view);
        }

        private void UpdateDynamicCellSize(BoardGrid board)
        {
            Camera cam = Camera.main;
            if (cam == null || board == null) return;

            float screenHeight = 2f * cam.orthographicSize;
            float screenWidth = screenHeight * cam.aspect;

            float maxCellWidth = (screenWidth * 0.95f) / board.Width;
            float availableHeightRatio = cam.aspect < 1f ? 0.65f : 0.95f; 
            float maxCellHeight = (screenHeight * availableHeightRatio) / (board.Height + 6f);

            cellSize = Mathf.Min(maxCellWidth, maxCellHeight);
        }

        private Vector2 ComputeBoardOffset(BoardGrid board)
        {
            if (!autoCenterBoard || board == null)
            {
                return manualBoardOffset;
            }

            float centeredX = -((board.Width - 1) * cellSize * 0.5f);
            float centeredY = ((board.Height - 1) * cellSize * 0.5f);

            Camera cam = Camera.main;
            if (cam != null && cam.aspect < 1f)
            {
                float topScreenY = cam.transform.position.y + cam.orthographicSize;
                float margin = cellSize; 
                
                centeredY = topScreenY - margin - (5.5f * cellSize);
            }

            return new Vector2(centeredX, centeredY) + manualBoardOffset;
        }

        private void RenderGridBackground(BoardGrid board)
        {
            if (gridRoot == null)
            {
                gridRoot = new GameObject("BackgroundGrid");
                gridRoot.transform.SetParent(boardRoot != null ? boardRoot : transform);
                gridRoot.transform.localPosition = new Vector3(0, 0, 10f);

                GameObject bgObj = new GameObject("GridBackgroundQuad");
                bgObj.transform.SetParent(gridRoot.transform);
                gridBackground = bgObj.AddComponent<SpriteRenderer>();
                
                Texture2D tex = new Texture2D(1, 1);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                gridBackground.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                gridBackground.color = new Color(0.15f, 0.15f, 0.15f, 1f); // Koyu gri (Dark Gray) arka plan
                gridBackground.sortingOrder = -20;
            }

            int expectedLines = board.Width + 1 + board.Height + 1;
            while (gridLines.Count < expectedLines)
            {
                GameObject lineObj = new GameObject($"GridLine_{gridLines.Count}");
                lineObj.transform.SetParent(gridRoot.transform);
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Sprites/Default"));
                Color gridLineColor = new Color(0.4f, 0.4f, 0.4f, 0.5f); // Gray
                lr.startColor = gridLineColor;
                lr.endColor = gridLineColor;
                lr.sortingOrder = -10;
                lr.useWorldSpace = false;
                gridLines.Add(lr);
            }

            float width = board.Width * cellSize;
            float height = board.Height * cellSize;
            
            float centerX = cachedBoardOffset.x + (width * 0.5f) - (cellSize * 0.5f);
            float centerY = cachedBoardOffset.y - (height * 0.5f) + (cellSize * 0.5f);

            gridBackground.transform.localPosition = new Vector3(centerX, centerY, 0f);
            gridBackground.transform.localScale = new Vector3(width, height, 1f);

            float lineWidth = cellSize * 0.03f; 
            int lineIndex = 0;

            for (int x = 0; x <= board.Width; x++)
            {
                float xPos = x * cellSize + cachedBoardOffset.x - (cellSize * 0.5f);
                float yTop = cachedBoardOffset.y + (cellSize * 0.5f);
                float yBottom = -board.Height * cellSize + cachedBoardOffset.y + (cellSize * 0.5f);

                LineRenderer lr = gridLines[lineIndex++];
                lr.startWidth = lineWidth;
                lr.endWidth = lineWidth;
                lr.SetPosition(0, new Vector3(xPos, yTop, 0f));
                lr.SetPosition(1, new Vector3(xPos, yBottom, 0f));
            }

            for (int y = 0; y <= board.Height; y++)
            {
                float yPos = -y * cellSize + cachedBoardOffset.y + (cellSize * 0.5f);
                float xLeft = cachedBoardOffset.x - (cellSize * 0.5f);
                float xRight = board.Width * cellSize + cachedBoardOffset.x - (cellSize * 0.5f);

                LineRenderer lr = gridLines[lineIndex++];
                lr.startWidth = lineWidth;
                lr.endWidth = lineWidth;
                lr.SetPosition(0, new Vector3(xLeft, yPos, 0f));
                lr.SetPosition(1, new Vector3(xRight, yPos, 0f));
            }
        }

        private void ClearViews()
        {
            for (int index = 0; index < spawnedViews.Count; index++)
            {
                if (spawnedViews[index] != null)
                {
                    Destroy(spawnedViews[index].gameObject);
                }
            }

            spawnedViews.Clear();
        }

        public void PlayClearAnimation(List<BlockCellInfo> cellsToAnimate, float duration)
        {
            StartCoroutine(ClearAnimationCoroutine(cellsToAnimate, duration));
        }

        private IEnumerator ClearAnimationCoroutine(List<BlockCellInfo> cells, float duration)
        {
            List<BlockCellView> animatingViews = new List<BlockCellView>();
            List<Vector3> basePositions = new List<Vector3>();
            List<Vector3> scatterOffsets = new List<Vector3>();
            List<float> spinSpeeds = new List<float>();

            foreach (BlockCellInfo cell in cells)
            {
                if (cellPrefab == null) continue;
                Transform root = boardRoot != null ? boardRoot : transform;
                BlockCellView view = Instantiate(cellPrefab, root);
                view.gameObject.SetActive(true);
                float x = cell.Position.x * cellSize + cachedBoardOffset.x;
                float y = -cell.Position.y * cellSize + cachedBoardOffset.y;
                Vector3 pos = new Vector3(x, y, 0f);
                view.transform.localPosition = pos;
                view.SetCell(cell, null);
                
                SpriteRenderer sr = view.GetComponent<SpriteRenderer>();
                if (sr != null) 
                {
                    sr.sortingOrder = 10;
                    if (flashSprite != null)
                    {
                        sr.sprite = flashSprite;
                    }
                    else
                    {
#if UNITY_EDITOR
                        Sprite editorSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Blocks/block_removed.png");
                        if (editorSprite != null) sr.sprite = editorSprite;
#endif
                    }
                    sr.color = Color.white;
                }
                
                animatingViews.Add(view);
                basePositions.Add(pos);
                
                Vector3 randomDir = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-0.2f, 1.5f), 0f).normalized;
                scatterOffsets.Add(randomDir * UnityEngine.Random.Range(1.5f, 4f) * cellSize);
                spinSpeeds.Add(UnityEngine.Random.Range(-700f, 700f));
            }

            float elapsed = 0f;
            Vector3 originalScale = Vector3.one;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                for (int i = 0; i < animatingViews.Count; i++)
                {
                    BlockCellView view = animatingViews[i];
                    if (view == null) continue;

                    Vector3 basePos = basePositions[i];
                    Vector3 scatterOffset = scatterOffsets[i];
                    float spin = spinSpeeds[i];
                    SpriteRenderer sr = view.GetComponent<SpriteRenderer>();

                    if (t < 0.25f)
                    {
                        float localT = t / 0.25f;
                        float scale = Mathf.Lerp(1f, 1.4f, EaseOutBack(localT));
                        view.transform.localScale = originalScale * scale;
                        view.transform.localPosition = basePos;
                        
                        if (sr != null)
                        {
                            sr.color = Color.Lerp(Color.white, new Color(1f, 0.9f, 0.4f, 1f), localT);
                        }
                    }
                    else
                    {
                        float localT = (t - 0.25f) / 0.75f;
                        float moveEaseT = 1f - Mathf.Pow(1f - localT, 3f);
                        
                        float scale = Mathf.Lerp(1.4f, 0f, localT * localT);
                        view.transform.localScale = originalScale * scale;
                        view.transform.localPosition = basePos + scatterOffset * moveEaseT;
                        view.transform.localRotation = Quaternion.Euler(0, 0, spin * moveEaseT);
                        
                        if (sr != null)
                        {
                            Color c = sr.color;
                            c.a = Mathf.Lerp(1f, 0f, localT);
                            sr.color = c;
                        }
                    }
                }
                yield return null;
            }

            foreach (BlockCellView view in animatingViews)
            {
                if (view != null)
                {
                    Destroy(view.gameObject);
                }
            }
        }

        public void PlayBombAnimation(List<BlockCellInfo> cellsToAnimate, float duration)
        {
            StartCoroutine(BombAnimationCoroutine(new List<BlockCellInfo>(cellsToAnimate), duration));
        }

        private IEnumerator BombAnimationCoroutine(List<BlockCellInfo> cells, float duration)
        {
            List<BlockCellView> animatingViews = new List<BlockCellView>();
            List<Vector3> basePositions = new List<Vector3>();
            List<Vector3> scatterOffsets = new List<Vector3>();
            List<float> spinSpeeds = new List<float>();

            // Cloud explosion overlay instead of flashbang
            GameObject flashObj = new GameObject("BombCloud");
            flashObj.transform.SetParent(boardRoot != null ? boardRoot : transform, false);
            SpriteRenderer flashSr = flashObj.AddComponent<SpriteRenderer>();
            
            if (flashSprite != null) flashSr.sprite = flashSprite;
            else
            {
#if UNITY_EDITOR
                flashSr.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Blocks/block_removed.png");
#endif
            }
            flashSr.sortingOrder = 20;
            flashSr.color = new Color(0.2f, 0.2f, 0.2f, 0f); // Dark smoke color
            flashObj.transform.localScale = new Vector3(5f, 5f, 1f); // Start small, grow big

            foreach (BlockCellInfo cell in cells)
            {
                if (cellPrefab == null) continue;
                Transform root = boardRoot != null ? boardRoot : transform;
                BlockCellView view = Instantiate(cellPrefab, root);
                view.gameObject.SetActive(true);
                float x = cell.Position.x * cellSize + cachedBoardOffset.x;
                float y = -cell.Position.y * cellSize + cachedBoardOffset.y;
                Vector3 pos = new Vector3(x, y, 0f);
                view.transform.localPosition = pos;
                view.SetCell(cell, null);
                
                SpriteRenderer sr = view.GetComponent<SpriteRenderer>();
                if (sr != null) 
                {
                    sr.sortingOrder = 15;
                    // Random mix of orange/fire and dark smoke
                    sr.color = UnityEngine.Random.value > 0.5f ? new Color(1f, 0.4f, 0f, 1f) : new Color(0.3f, 0.3f, 0.3f, 1f);
                }
                
                animatingViews.Add(view);
                basePositions.Add(pos);
                
                Vector3 randomDir = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f).normalized;
                scatterOffsets.Add(randomDir * UnityEngine.Random.Range(8f, 20f) * cellSize); 
                spinSpeeds.Add(UnityEngine.Random.Range(-800f, 800f));
            }

            float elapsed = 0f;
            Vector3 originalScale = Vector3.one;
            
            Transform targetToShake = boardRoot != null ? boardRoot : transform;
            Vector3 originalTargetPos = targetToShake.localPosition;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Board Shake (instead of full camera shake)
                float shakeIntensity = (1f - t) * 0.8f; // Shake reduces over time
                Vector3 shakeOffset = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f) * shakeIntensity;
                targetToShake.localPosition = originalTargetPos + shakeOffset;

                if (flashSr != null)
                {
                    // Cloud expands and fades
                    float cloudT = 1f - Mathf.Pow(1f - t, 3f);
                    flashObj.transform.localScale = Vector3.Lerp(new Vector3(5f, 5f, 1f), new Vector3(40f, 40f, 1f), cloudT);
                    
                    float alpha = t < 0.2f ? t / 0.2f : 1f - ((t - 0.2f) / 0.8f);
                    flashSr.color = new Color(0.15f, 0.15f, 0.15f, alpha * 0.8f); // Dark smoke
                }

                for (int i = 0; i < animatingViews.Count; i++)
                {
                    BlockCellView view = animatingViews[i];
                    if (view == null) continue;

                    Vector3 basePos = basePositions[i];
                    Vector3 scatterOffset = scatterOffsets[i];
                    float spin = spinSpeeds[i];
                    SpriteRenderer sr = view.GetComponent<SpriteRenderer>();

                    float moveEaseT = 1f - Mathf.Pow(1f - t, 3f); 
                    
                    float scale = Mathf.Lerp(1.2f, 0f, t * t);
                    view.transform.localScale = originalScale * scale;
                    view.transform.localPosition = basePos + scatterOffset * moveEaseT;
                    view.transform.localRotation = Quaternion.Euler(0, 0, spin * moveEaseT);
                    
                    if (sr != null)
                    {
                        Color c = sr.color;
                        c.a = Mathf.Lerp(1f, 0f, t);
                        sr.color = c;
                    }
                }
                yield return null;
            }

            targetToShake.localPosition = originalTargetPos;

            if (flashObj != null) Destroy(flashObj);

            foreach (BlockCellView view in animatingViews)
            {
                if (view != null) Destroy(view.gameObject);
            }
        }

        public void PlayExtinguishAnimation(List<BlockCellInfo> cellsToAnimate, float duration)
        {
            StartCoroutine(ExtinguishAnimationCoroutine(new List<BlockCellInfo>(cellsToAnimate), duration));
        }

        private IEnumerator ExtinguishAnimationCoroutine(List<BlockCellInfo> cells, float duration)
        {
            List<BlockCellView> animatingViews = new List<BlockCellView>();
            List<ParticleSystem> particles = new List<ParticleSystem>();

            foreach (BlockCellInfo cell in cells)
            {
                BlockCellView view = Instantiate(cellPrefab, boardRoot);
                view.gameObject.SetActive(true);
                // Draw it as Lava initially
                view.SetCell(cell, null);

                Vector3 targetPos = new Vector3(
                    cell.Position.x * cellSize + cachedBoardOffset.x,
                    -cell.Position.y * cellSize + cachedBoardOffset.y,
                    -1f // slightly in front to cover the real metal block seamlessly
                );

                view.transform.localPosition = targetPos;
                animatingViews.Add(view);

                // Create a simple smoke particle system
                GameObject smokeObj = new GameObject("SmokeEffect");
                smokeObj.transform.parent = boardRoot;
                smokeObj.transform.localPosition = targetPos;
                ParticleSystem ps = smokeObj.AddComponent<ParticleSystem>();
                
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                
                var main = ps.main;
                main.duration = duration;
                main.startLifetime = 1f;
                main.startSpeed = 0.5f;
                main.startSize = cellSize * 0.4f;
                main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.6f, 0.6f, 0.6f, 0.7f), new Color(0.8f, 0.8f, 0.8f, 0.4f));
                main.loop = false;
                main.playOnAwake = false;

                var emission = ps.emission;
                emission.rateOverTime = 20;

                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.scale = new Vector3(cellSize, cellSize, 1f);

                // Add size over lifetime to disperse
                var sizeOverLifetime = ps.sizeOverLifetime;
                sizeOverLifetime.enabled = true;
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1.0f, 2.0f);

                // Add color over lifetime to fade out
                var colorOverLifetime = ps.colorOverLifetime;
                colorOverLifetime.enabled = true;
                Gradient grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                colorOverLifetime.color = grad;

                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                renderer.sortingOrder = 10;
                renderer.material = new Material(Shader.Find("Sprites/Default"));

                ps.Play();
                particles.Add(ps);
                Destroy(smokeObj, duration + 1.5f);
            }

            float elapsed = 0f;
            Color lavaColor = new Color(1f, 0.35f, 0.1f, 1f); // Lava starting color

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);

                // We'll fade the overlay block's alpha from 1 to 0, smoothly exposing the real metal block behind it
                Color fadeColor = new Color(lavaColor.r, lavaColor.g, lavaColor.b, 1f - normalizedTime);

                for (int i = 0; i < animatingViews.Count; i++)
                {
                    if (animatingViews[i] != null)
                    {
                        animatingViews[i].SetCell(cells[i], fadeColor);
                    }
                }
                yield return null;
            }

            for (int i = 0; i < animatingViews.Count; i++)
            {
                if (animatingViews[i] != null)
                {
                    Destroy(animatingViews[i].gameObject);
                }
            }
        }

        public void PlayMeltAnimation(List<BlockCellInfo> cellsToAnimate, float duration)
        {
            StartCoroutine(MeltAnimationCoroutine(cellsToAnimate, duration));
        }

        private IEnumerator MeltAnimationCoroutine(List<BlockCellInfo> cells, float duration)
        {
            List<BlockCellView> animatingViews = new List<BlockCellView>();
            List<Vector3> basePositions = new List<Vector3>();
            List<Color> originalColors = new List<Color>();
            
            List<BlockCellView> particleViews = new List<BlockCellView>();
            List<Vector3> particleStarts = new List<Vector3>();
            List<Vector3> particleVelocities = new List<Vector3>();
            List<Color> particleColors = new List<Color>();
            List<float> particleBaseScales = new List<float>();
            List<bool> isSmoke = new List<bool>();

            foreach (BlockCellInfo cell in cells)
            {
                if (cellPrefab == null) continue;
                Transform root = boardRoot != null ? boardRoot : transform;
                
                BlockCellView view = Instantiate(cellPrefab, root);
                view.gameObject.SetActive(true);
                float x = cell.Position.x * cellSize + cachedBoardOffset.x;
                float y = -cell.Position.y * cellSize + cachedBoardOffset.y;
                Vector3 pos = new Vector3(x, y, 0f);
                view.transform.localPosition = pos;
                view.SetCell(cell, null);
                view.enabled = false;
                
                SpriteRenderer sr = view.GetComponent<SpriteRenderer>();
                Color startColor = Color.white;
                if (sr != null) 
                {
                    sr.sortingOrder = 15;
                    startColor = sr.color;
                }
                
                animatingViews.Add(view);
                basePositions.Add(pos);
                originalColors.Add(startColor);

                for (int p = 0; p < 5; p++)
                {
                    BlockCellView drop = Instantiate(cellPrefab, root);
                    drop.gameObject.SetActive(true);
                    drop.SetCell(cell, null);
                    drop.enabled = false;
                    
                    float pScale = UnityEngine.Random.Range(0.15f, 0.4f);
                    drop.transform.localScale = Vector3.one * pScale;
                    
                    SpriteRenderer dsr = drop.GetComponent<SpriteRenderer>();
                    if (dsr != null)
                    {
                        dsr.sortingOrder = 16;
                        dsr.sprite = sr != null ? sr.sprite : null;
                        dsr.color = Color.clear;
                    }
                    
                    particleViews.Add(drop);
                    particleStarts.Add(pos);
                    Vector3 vel = new Vector3(UnityEngine.Random.Range(-2.5f, 2.5f), UnityEngine.Random.Range(-1f, 3f), 0f) * cellSize;
                    particleVelocities.Add(vel);
                    particleColors.Add(new Color(1f, 0.6f, 0f, 1f));
                    particleBaseScales.Add(pScale);
                    isSmoke.Add(false);
                }

                for (int s = 0; s < 3; s++)
                {
                    BlockCellView smoke = Instantiate(cellPrefab, root);
                    smoke.gameObject.SetActive(true);
                    smoke.SetCell(cell, null);
                    smoke.enabled = false;
                    
                    float sScale = UnityEngine.Random.Range(0.2f, 0.6f);
                    smoke.transform.localScale = Vector3.one * sScale;
                    
                    SpriteRenderer ssr = smoke.GetComponent<SpriteRenderer>();
                    if (ssr != null)
                    {
                        ssr.sortingOrder = 17;
                        ssr.sprite = sr != null ? sr.sprite : null;
                        ssr.color = Color.clear;
                    }
                    
                    particleViews.Add(smoke);
                    particleStarts.Add(pos);
                    Vector3 vel = new Vector3(UnityEngine.Random.Range(-0.8f, 0.8f), UnityEngine.Random.Range(1.5f, 3.5f), 0f) * cellSize;
                    particleVelocities.Add(vel);
                    particleColors.Add(new Color(0.8f, 0.8f, 0.8f, 0.7f));
                    particleBaseScales.Add(sScale);
                    isSmoke.Add(true);
                }
            }

            float elapsed = 0f;
            Vector3 originalScale = Vector3.one;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                for (int i = 0; i < animatingViews.Count; i++)
                {
                    BlockCellView view = animatingViews[i];
                    if (view == null) continue;

                    Vector3 basePos = basePositions[i];
                    Color origColor = originalColors[i];
                    SpriteRenderer sr = view.GetComponent<SpriteRenderer>();

                    if (t < 0.2f)
                    {
                        float jiggleStr = (0.2f - t) * 0.4f * cellSize; 
                        Vector3 jiggle = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f) * jiggleStr;
                        view.transform.localPosition = basePos + jiggle;

                        if (sr != null)
                        {
                            float flash = Mathf.PingPong(t * 20f, 1f);
                            sr.color = Color.Lerp(origColor, new Color(1f, 0.9f, 0.2f, 1f), flash);
                        }
                    }
                    else
                    {
                        float meltT = (t - 0.2f) / 0.8f;
                        float easeInT = meltT * meltT; 

                        if (sr != null)
                        {
                            Color meltColor = new Color(1f, 0.2f, 0f, 1f);
                            Color c = Color.Lerp(origColor, meltColor, meltT * 3f);
                            c.a = Mathf.Lerp(1f, 0f, easeInT);
                            sr.color = c;
                        }

                        float scaleX = Mathf.Lerp(1f, 1.8f, easeInT);
                        float scaleY = Mathf.Lerp(1f, 0.1f, easeInT);
                        view.transform.localScale = new Vector3(originalScale.x * scaleX, originalScale.y * scaleY, originalScale.z);

                        float meltDrop = Mathf.Lerp(0f, -0.45f * cellSize, easeInT);
                        view.transform.localPosition = basePos + new Vector3(0, meltDrop, 0);
                    }
                }

                for (int p = 0; p < particleViews.Count; p++)
                {
                    BlockCellView pView = particleViews[p];
                    if (pView == null) continue;

                    if (t < 0.2f) continue;

                    float pt = (t - 0.2f) / 0.8f;
                    Vector3 startPos = particleStarts[p];
                    Vector3 vel = particleVelocities[p];
                    SpriteRenderer psr = pView.GetComponent<SpriteRenderer>();

                    if (isSmoke[p])
                    {
                        float baseScale = particleBaseScales[p];
                        pView.transform.localScale = Vector3.one * Mathf.Lerp(baseScale, baseScale * 3f, pt);
                        
                        Vector3 wiggle = new Vector3(Mathf.Sin(pt * 15f + p) * 0.3f * cellSize, 0f, 0f);
                        pView.transform.localPosition = startPos + vel * pt + wiggle;
                        pView.transform.localRotation = Quaternion.Euler(0, 0, pt * 90f);
                        
                        if (psr != null)
                        {
                            Color c = particleColors[p];
                            c.a = Mathf.Lerp(c.a, 0f, pt * pt);
                            psr.color = c;
                        }
                    }
                    else
                    {
                        Vector3 gravity = new Vector3(0f, -8f * cellSize * pt * pt, 0f);
                        pView.transform.localPosition = startPos + vel * pt + gravity;
                        pView.transform.localRotation = Quaternion.Euler(0, 0, pt * 1000f * (p % 2 == 0 ? 1 : -1));

                        if (psr != null)
                        {
                            Color c = particleColors[p];
                            if (pt < 0.5f) {
                                psr.color = Color.Lerp(c, new Color(0.6f, 0f, 0f, 1f), pt * 2f);
                            } else {
                                Color dark = new Color(0.2f, 0f, 0f, 1f);
                                dark.a = Mathf.Lerp(1f, 0f, (pt - 0.5f) * 2f);
                                psr.color = dark;
                            }
                        }
                    }
                }
                
                yield return null;
            }

            foreach (BlockCellView view in animatingViews)
            {
                if (view != null) Destroy(view.gameObject);
            }
            foreach (BlockCellView pView in particleViews)
            {
                if (pView != null) Destroy(pView.gameObject);
            }
        }

        public void PlayBoardRotationAnimation(bool isLeft, Action onComplete)
        {
            StartCoroutine(BoardRotationCoroutine(isLeft, onComplete));
        }

        private IEnumerator BoardRotationCoroutine(bool isLeft, Action onComplete)
        {
            Transform targetTransform = boardRoot != null ? boardRoot : transform;
            Vector3 originalScale = targetTransform.localScale;
            Vector3 scaleDown = originalScale * 0.90f;
            Quaternion originalRotation = targetTransform.localRotation;
            Vector3 originalLocalPos = targetTransform.localPosition;

            float angle = isLeft ? 90f : -90f;
            Quaternion targetRotation = originalRotation * Quaternion.Euler(0, 0, angle);

            Vector3 pivotLocal = new Vector3(0f, cachedBoardOffset.y - 5.5f * cellSize, 0f);
            Vector3 pivotParentSpace = originalLocalPos + originalRotation * Vector3.Scale(originalScale, pivotLocal);

            // Flash the grid background during rotation
            Color originalGridColor = Color.clear;
            if (gridBackground != null) originalGridColor = gridBackground.color;

            float duration = 0.85f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // Scale: down 0-25%, hold 25-75%, bounce back 75-100%
                float scaleDownT = Mathf.Clamp01(t / 0.25f);
                float scaleUpT = Mathf.Clamp01((t - 0.75f) / 0.25f);

                Vector3 currentScale = originalScale;
                if (scaleDownT < 1f)
                {
                    currentScale = Vector3.Lerp(originalScale, scaleDown, Mathf.SmoothStep(0f, 1f, scaleDownT));
                }
                else if (scaleUpT > 0f)
                {
                    currentScale = Vector3.LerpUnclamped(scaleDown, originalScale, EaseOutElastic(scaleUpT));
                }
                else
                {
                    currentScale = scaleDown;
                }
                targetTransform.localScale = currentScale;

                // Rotation with juicy overshoot
                float rotT = Mathf.Clamp01((t - 0.1f) / 0.75f);
                Quaternion currentRot = Quaternion.SlerpUnclamped(originalRotation, targetRotation, EaseInOutBack(rotT));
                targetTransform.localRotation = currentRot;

                targetTransform.localPosition = pivotParentSpace - currentRot * Vector3.Scale(currentScale, pivotLocal);

                // Grid flash pulse during rotation peak
                if (gridBackground != null)
                {
                    float flashT = Mathf.Clamp01((t - 0.3f) / 0.4f);
                    float flash = Mathf.Sin(flashT * Mathf.PI);
                    Color flashColor = Color.Lerp(originalGridColor, new Color(0.3f, 0.2f, 0.5f, 1f), flash * 0.6f);
                    gridBackground.color = flashColor;
                }

                yield return null;
            }

            targetTransform.localScale = originalScale;
            targetTransform.localRotation = originalRotation;
            targetTransform.localPosition = originalLocalPos;
            if (gridBackground != null) gridBackground.color = originalGridColor;
            
            onComplete?.Invoke();
        }

        public void PlayBoardDropAnimation(int dropAmount, Action onComplete)
        {
            StartCoroutine(BoardDropCoroutine(dropAmount, onComplete));
        }

        private IEnumerator BoardDropCoroutine(int dropAmount, Action onComplete)
        {
            if (dropAmount <= 0)
            {
                onComplete?.Invoke();
                yield break;
            }

            float dropDistance = dropAmount * cellSize;
            
            // Snapshot current positions and offset them upward
            List<BlockCellView> activeCells = new List<BlockCellView>();
            List<Vector3> targetPositions = new List<Vector3>();

            foreach (BlockCellView view in spawnedViews)
            {
                if (view != null && view.transform.parent == (boardRoot != null ? boardRoot : transform))
                {
                    Vector3 targetPos = view.transform.localPosition;
                    targetPositions.Add(targetPos);
                    view.transform.localPosition = targetPos + new Vector3(0f, dropDistance, 0f);
                    activeCells.Add(view);
                }
            }

            float duration = 0.35f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = EaseOutBounce(t);

                for (int i = 0; i < activeCells.Count; i++)
                {
                    BlockCellView view = activeCells[i];
                    if (view != null)
                    {
                        Vector3 target = targetPositions[i];
                        Vector3 start = target + new Vector3(0f, dropDistance, 0f);
                        view.transform.localPosition = Vector3.LerpUnclamped(start, target, easedT);
                    }
                }

                yield return null;
            }

            for (int i = 0; i < activeCells.Count; i++)
            {
                if (activeCells[i] != null)
                    activeCells[i].transform.localPosition = targetPositions[i];
            }

            onComplete?.Invoke();
        }

        public void PlaySlipperySlideAnimation(List<BoardGrid.SlideInfo> slideMovements, Action onComplete)
        {
            StartCoroutine(SlipperySlideCoroutine(slideMovements, onComplete));
        }

        private IEnumerator SlipperySlideCoroutine(List<BoardGrid.SlideInfo> slideMovements, Action onComplete)
        {
            float duration = 0.35f;
            float elapsed = 0f;

            List<BlockCellView> slidingViews = new List<BlockCellView>();
            List<Vector3> startLocalPos = new List<Vector3>();
            List<Vector3> endLocalPos = new List<Vector3>();

            foreach (var movement in slideMovements)
            {
                float toX = movement.To.x * cellSize + cachedBoardOffset.x;
                float toY = -movement.To.y * cellSize + cachedBoardOffset.y;
                Vector3 targetPos = new Vector3(toX, toY, 0f);

                float fromX = movement.From.x * cellSize + cachedBoardOffset.x;
                float fromY = -movement.From.y * cellSize + cachedBoardOffset.y;
                Vector3 startPos = new Vector3(fromX, fromY, 0f);

                BlockCellView matchedView = null;
                foreach (var view in spawnedViews)
                {
                    if (view != null && Vector3.Distance(view.transform.localPosition, targetPos) < 0.01f)
                    {
                        matchedView = view;
                        break;
                    }
                }

                if (matchedView != null)
                {
                    slidingViews.Add(matchedView);
                    startLocalPos.Add(startPos);
                    endLocalPos.Add(targetPos);
                    
                    matchedView.transform.localPosition = startPos;
                }
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = EaseOutBounce(t);
                
                for (int i = 0; i < slidingViews.Count; i++)
                {
                    if (slidingViews[i] != null)
                    {
                        slidingViews[i].transform.localPosition = Vector3.LerpUnclamped(startLocalPos[i], endLocalPos[i], easedT);
                    }
                }

                yield return null;
            }

            for (int i = 0; i < slidingViews.Count; i++)
            {
                if (slidingViews[i] != null)
                {
                    slidingViews[i].transform.localPosition = endLocalPos[i];
                }
            }

            onComplete?.Invoke();
        }

        public void PlayGameOverAnimation(Action onComplete = null)
        {
            StartCoroutine(GameOverAnimationCoroutine(onComplete));
        }

        private IEnumerator GameOverAnimationCoroutine(Action onComplete)
        {
            List<BlockCellView> blocksToAnimate = new List<BlockCellView>();
            Transform targetRoot = boardRoot != null ? boardRoot : transform;
            
            // Collect all blocks on the board
            foreach (var view in spawnedViews)
            {
                if (view != null && view.transform.parent == targetRoot)
                {
                    if (view.gameObject.name == "PreviewBackground") continue;
                    if (view.GetComponent<TextMesh>() != null) continue;
                    
                    var sr = view.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        blocksToAnimate.Add(view);
                    }
                }
            }

            if (blocksToAnimate.Count == 0) yield break;

            // Flash red phase
            float flashDuration = 1.2f;
            float elapsed = 0f;
            List<SpriteRenderer> srs = new List<SpriteRenderer>();
            List<Color> origColors = new List<Color>();
            
            foreach (var view in blocksToAnimate)
            {
                var sr = view.GetComponent<SpriteRenderer>();
                srs.Add(sr);
                origColors.Add(sr.color);
            }

            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                
                // Pulsing red effect
                float flash = (Mathf.Sin(t * Mathf.PI * 6f) + 1f) * 0.5f; 
                
                for (int i = 0; i < srs.Count; i++)
                {
                    if (srs[i] != null)
                    {
                        srs[i].color = Color.Lerp(origColors[i], new Color(1f, 0.2f, 0.2f, 1f), flash);
                    }
                }
                yield return null;
            }
            
            // Restore colors briefly before falling
            for (int i = 0; i < srs.Count; i++)
            {
                if (srs[i] != null) srs[i].color = origColors[i];
            }
            
            yield return new WaitForSeconds(0.1f);

            // Shuffle blocks for random falling order
            for (int i = 0; i < blocksToAnimate.Count; i++)
            {
                BlockCellView temp = blocksToAnimate[i];
                int randomIndex = UnityEngine.Random.Range(i, blocksToAnimate.Count);
                blocksToAnimate[i] = blocksToAnimate[randomIndex];
                blocksToAnimate[randomIndex] = temp;
            }

            float gravity = -20f * cellSize;
            int groupSize = Mathf.Max(3, blocksToAnimate.Count / 15); // Fall in groups
            int currentIdx = 0;
            
            List<BlockCellView> fallingBlocks = new List<BlockCellView>();
            List<Vector3> velocities = new List<Vector3>();
            List<float> rotationalVels = new List<float>();

            float spawnTimer = 0f;
            float spawnInterval = 0.05f;

            while (currentIdx < blocksToAnimate.Count || fallingBlocks.Count > 0)
            {
                spawnTimer -= Time.deltaTime;
                
                // Add new group of blocks to fall
                if (spawnTimer <= 0f && currentIdx < blocksToAnimate.Count)
                {
                    spawnTimer = spawnInterval;
                    for (int i = 0; i < groupSize && currentIdx < blocksToAnimate.Count; i++)
                    {
                        BlockCellView view = blocksToAnimate[currentIdx++];
                        if (view != null)
                        {
                            fallingBlocks.Add(view);
                            velocities.Add(new Vector3(
                                UnityEngine.Random.Range(-3f, 3f) * cellSize, 
                                UnityEngine.Random.Range(2f, 6f) * cellSize, 
                                0f));
                            rotationalVels.Add(UnityEngine.Random.Range(-400f, 400f));
                            
                            var sr = view.GetComponent<SpriteRenderer>();
                            if (sr != null) sr.sortingOrder += 20;
                        }
                    }
                }

                // Update falling blocks
                for (int i = fallingBlocks.Count - 1; i >= 0; i--)
                {
                    BlockCellView b = fallingBlocks[i];
                    if (b == null)
                    {
                        fallingBlocks.RemoveAt(i);
                        velocities.RemoveAt(i);
                        rotationalVels.RemoveAt(i);
                        continue;
                    }

                    Vector3 vel = velocities[i];
                    vel.y += gravity * Time.deltaTime;
                    velocities[i] = vel;

                    b.transform.localPosition += vel * Time.deltaTime;
                    b.transform.localRotation *= Quaternion.Euler(0, 0, rotationalVels[i] * Time.deltaTime);

                    if (b.transform.localPosition.y < cachedBoardOffset.y - (30f * cellSize))
                    {
                        Destroy(b.gameObject);
                        fallingBlocks.RemoveAt(i);
                        velocities.RemoveAt(i);
                        rotationalVels.RemoveAt(i);
                    }
                }

                yield return null;
            }

            onComplete?.Invoke();
        }

        private float EaseOutBack(float x)
        {
            float c1 = 2.2f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        private float EaseInOutBack(float x)
        {
            float c1 = 2.2f;
            float c2 = c1 * 1.525f;
            return x < 0.5f
              ? (Mathf.Pow(2f * x, 2f) * ((c2 + 1f) * 2f * x - c2)) / 2f
              : (Mathf.Pow(2f * x - 2f, 2f) * ((c2 + 1f) * (x * 2f - 2f) + c2) + 2f) / 2f;
        }

        private float EaseOutElastic(float x)
        {
            if (x <= 0f) return 0f;
            if (x >= 1f) return 1f;
            float c4 = (2f * Mathf.PI) / 3f;
            return Mathf.Pow(2f, -10f * x) * Mathf.Sin((x * 10f - 0.75f) * c4) + 1f;
        }

        private float EaseOutBounce(float x)
        {
            float n1 = 7.5625f;
            float d1 = 2.75f;
            if (x < 1f / d1) return n1 * x * x;
            if (x < 2f / d1) return n1 * (x -= 1.5f / d1) * x + 0.75f;
            if (x < 2.5f / d1) return n1 * (x -= 2.25f / d1) * x + 0.9375f;
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }
    }
}