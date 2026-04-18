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
        private readonly List<BlockCellView> spawnedBoardCells = new List<BlockCellView>();
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
                    BlockCellView view = SpawnCell(cell, null);
                    if (view != null)
                    {
                        spawnedBoardCells.Add(view);
                    }
                }
            }

            if (activePiece != null)
            {
                foreach (BlockCellInfo cell in activePiece.GetWorldCells())
                {
                    SpawnCell(cell, activePieceTint);
                }
            }

            if (heldPiece != null)
            {
                Vector2Int heldOrigin = new Vector2Int(0, -4);
                foreach (BlockCellInfo cell in heldPiece.GetWorldCellsAtOrigin(heldOrigin))
                {
                    SpawnCell(cell, null, transform);
                }
                
                float hx = 0 * cellSize + cachedBoardOffset.x + cellSize;
                float hy = -(-5) * cellSize + cachedBoardOffset.y;
                SpawnLabel("HOLD", new Vector3(hx, hy, 0f), transform);
            }

            if (nextPiece != null)
            {
                Vector2Int nextOrigin = new Vector2Int(board != null ? board.Width - 3 : 9, -4);
                foreach (BlockCellInfo cell in nextPiece.GetWorldCellsAtOrigin(nextOrigin))
                {
                    SpawnCell(cell, null, transform);
                }
                
                float nx = (board != null ? board.Width - 3 : 9) * cellSize + cachedBoardOffset.x + cellSize;
                float ny = -(-5) * cellSize + cachedBoardOffset.y;
                SpawnLabel("NEXT", new Vector3(nx, ny, 0f), transform);
            }
        }

        private void SpawnLabel(string text, Vector3 pos, Transform root)
        {
            GameObject labelObj = new GameObject("Label");
            if (root != null) labelObj.transform.SetParent(root, false);
            TextMesh tm = labelObj.AddComponent<TextMesh>();
            tm.text = text;
            tm.anchor = TextAnchor.LowerCenter;
            tm.fontSize = 40;
            tm.characterSize = (1.5f * cellSize) / 40f;
            tm.color = Color.white;
            labelObj.transform.localPosition = pos;
            
            // Add dummy BlockCellView so it gets cleaned up by ClearViews()
            spawnedViews.Add(labelObj.AddComponent<BlockCellView>());
        }

        private BlockCellView SpawnCell(BlockCellInfo cell, Color? tintOverride, Transform overrideRoot = null)
        {
            if (cellPrefab == null)
            {
                return null;
            }

            Transform root = overrideRoot != null ? overrideRoot : (boardRoot != null ? boardRoot : transform);
            BlockCellView view = Instantiate(cellPrefab, root);
            view.gameObject.SetActive(true);
            float x = cell.Position.x * cellSize + cachedBoardOffset.x;
            float y = -cell.Position.y * cellSize + cachedBoardOffset.y;
            view.transform.localPosition = new Vector3(x, y, 0f);
            view.SetCell(cell, tintOverride);
            spawnedViews.Add(view);
            return view;
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
                gridBackground.color = new Color(0.18f, 0.18f, 0.18f, 1f);
                gridBackground.sortingOrder = -20;
            }

            int expectedLines = board.Width + 1 + board.Height + 1;
            while (gridLines.Count < expectedLines)
            {
                GameObject lineObj = new GameObject($"GridLine_{gridLines.Count}");
                lineObj.transform.SetParent(gridRoot.transform);
                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = new Color(0.4f, 0.4f, 0.4f, 1f);
                lr.endColor = new Color(0.4f, 0.4f, 0.4f, 1f);
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
            spawnedBoardCells.Clear();
        }

        public void PlayClearAnimation(List<BlockCellInfo> cellsToAnimate, float duration)
        {
            StartCoroutine(ClearAnimationCoroutine(cellsToAnimate, duration));
        }

        private IEnumerator ClearAnimationCoroutine(List<BlockCellInfo> cells, float duration)
        {
            List<BlockCellView> animatingViews = new List<BlockCellView>();
            List<Vector3> basePositions = new List<Vector3>();

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
                    sr.color = Color.white; // Bright white flash
                }
                
                animatingViews.Add(view);
                basePositions.Add(pos);
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
                    SpriteRenderer sr = view.GetComponent<SpriteRenderer>();

                    if (t < 0.2f)
                    {
                        // 0 to 0.2: Quick flash & scale up
                        float localT = t / 0.2f;
                        float scale = Mathf.Lerp(1f, 1.3f, localT);
                        view.transform.localScale = originalScale * scale;
                        view.transform.localPosition = basePos;
                        
                        if (sr != null)
                        {
                            sr.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0.8f), localT);
                        }
                    }
                    else
                    {
                        // 0.2 to 1.0: Shrink down and fade out
                        float localT = (t - 0.2f) / 0.8f;
                        // Smooth easing for shrink
                        float scale = Mathf.Lerp(1.3f, 0f, localT * localT);
                        view.transform.localScale = originalScale * scale;
                        view.transform.localPosition = basePos;
                        
                        if (sr != null)
                        {
                            Color c = sr.color;
                            c.a = Mathf.Lerp(0.8f, 0f, localT);
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

        public void PlayBoardRotationAnimation(bool isLeft, Action onComplete)
        {
            StartCoroutine(BoardRotationCoroutine(isLeft, onComplete));
        }

        private IEnumerator BoardRotationCoroutine(bool isLeft, Action onComplete)
        {
            Transform targetTransform = boardRoot != null ? boardRoot : transform;
            Vector3 originalScale = targetTransform.localScale;
            Vector3 scaleDown = originalScale * 0.94f;
            Quaternion originalRotation = targetTransform.localRotation;
            Vector3 originalLocalPos = targetTransform.localPosition;

            float angle = isLeft ? 90f : -90f;
            Quaternion targetRotation = originalRotation * Quaternion.Euler(0, 0, angle);

            // Pivot is exactly in the center of the 12x12 grid
            Vector3 pivotLocal = new Vector3(0f, cachedBoardOffset.y - 5.5f * cellSize, 0f);
            
            // To rotate around the pivot, we keep its parent-space position constant.
            Vector3 pivotParentSpace = originalLocalPos + originalRotation * Vector3.Scale(originalScale, pivotLocal);

            float duration = 0.7f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                float scaleDownT = Mathf.Clamp01(t / 0.3f);
                float scaleUpT = Mathf.Clamp01((t - 0.7f) / 0.3f);

                Vector3 currentScale = originalScale;
                if (scaleDownT < 1f)
                {
                    currentScale = Vector3.Lerp(originalScale, scaleDown, Mathf.SmoothStep(0f, 1f, scaleDownT));
                }
                else if (scaleUpT > 0f)
                {
                    currentScale = Vector3.LerpUnclamped(scaleDown, originalScale, EaseOutBack(scaleUpT));
                }
                else
                {
                    currentScale = scaleDown;
                }
                targetTransform.localScale = currentScale;

                float rotT = Mathf.Clamp01((t - 0.1f) / 0.8f);
                Quaternion currentRot = Quaternion.SlerpUnclamped(originalRotation, targetRotation, EaseInOutBack(rotT));
                targetTransform.localRotation = currentRot;

                targetTransform.localPosition = pivotParentSpace - currentRot * Vector3.Scale(currentScale, pivotLocal);

                yield return null;
            }

            targetTransform.localScale = originalScale;
            targetTransform.localRotation = originalRotation;
            targetTransform.localPosition = originalLocalPos;
            
            onComplete?.Invoke();
        }

        private float EaseOutBack(float x)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        private float EaseInOutBack(float x)
        {
            float c1 = 1.70158f;
            float c2 = c1 * 1.525f;
            return x < 0.5f
              ? (Mathf.Pow(2f * x, 2f) * ((c2 + 1f) * 2f * x - c2)) / 2f
              : (Mathf.Pow(2f * x - 2f, 2f) * ((c2 + 1f) * (x * 2f - 2f) + c2) + 2f) / 2f;
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
            
            List<Vector3> targetPositions = new List<Vector3>(spawnedBoardCells.Count);
            foreach (BlockCellView view in spawnedBoardCells)
            {
                if (view != null)
                {
                    Vector3 targetPos = view.transform.localPosition;
                    targetPositions.Add(targetPos);
                    
                    Vector3 startPos = targetPos + new Vector3(0f, dropDistance, 0f);
                    view.transform.localPosition = startPos;
                }
                else
                {
                    targetPositions.Add(Vector3.zero);
                }
            }

            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                float easedT = EaseOutBounce(t);

                for (int i = 0; i < spawnedBoardCells.Count; i++)
                {
                    BlockCellView view = spawnedBoardCells[i];
                    if (view != null)
                    {
                        Vector3 targetPos = targetPositions[i];
                        Vector3 startPos = targetPos + new Vector3(0f, dropDistance, 0f);
                        view.transform.localPosition = Vector3.LerpUnclamped(startPos, targetPos, easedT);
                    }
                }

                yield return null;
            }

            for (int i = 0; i < spawnedBoardCells.Count; i++)
            {
                BlockCellView view = spawnedBoardCells[i];
                if (view != null)
                {
                    view.transform.localPosition = targetPositions[i];
                }
            }

            onComplete?.Invoke();
        }

        private float EaseOutBounce(float x)
        {
            float n1 = 7.5625f;
            float d1 = 2.75f;

            if (x < 1f / d1) {
                return n1 * x * x;
            } else if (x < 2f / d1) {
                return n1 * (x -= 1.5f / d1) * x + 0.75f;
            } else if (x < 2.5f / d1) {
                return n1 * (x -= 2.25f / d1) * x + 0.9375f;
            } else {
                return n1 * (x -= 2.625f / d1) * x + 0.984375f;
            }
        }
    }
}