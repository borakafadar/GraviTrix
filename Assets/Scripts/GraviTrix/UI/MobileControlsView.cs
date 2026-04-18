using GraviTrix.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace GraviTrix.UI
{
    public class MobileControlsView : MonoBehaviour
    {
        [Header("Button Sprites")]
        [SerializeField] private Sprite rotateLeftSprite;
        [SerializeField] private Sprite rotateRightSprite;
        [SerializeField] private Sprite dropSprite;
        [SerializeField] private Sprite leftSprite;
        [SerializeField] private Sprite rightSprite;
        [SerializeField] private Sprite holdSprite;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInitialize()
        {
            // Only create mobile controls in the game scene (where GameInputRelay exists)
            if (FindObjectOfType<GameInputRelay>() == null) return;

            if (FindObjectOfType<MobileControlsView>() == null)
            {
                GameObject obj = new GameObject("MobileControlsManager");
                obj.AddComponent<MobileControlsView>();
            }
        }

        [ContextMenu("Generate Mobile Buttons")]
        public void GenerateButtons()
        {
            GameInputRelay relay = FindObjectOfType<GameInputRelay>();
            if (relay == null)
            {
                Debug.LogWarning("Could not find GameInputRelay in the scene. Buttons will not be functional until connected.");
            }

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in the scene to place the buttons!");
                return;
            }

            // Check if already generated
            Transform existing = canvas.transform.Find("MobileControls");
            if (existing != null)
            {
                if (Application.isPlaying) return; // Don't regenerate on play if exists
                DestroyImmediate(existing.gameObject); // Re-generate in editor
            }

            GameObject container = new GameObject("MobileControls", typeof(RectTransform));
            RectTransform containerRt = container.GetComponent<RectTransform>();
            containerRt.SetParent(canvas.transform, false);
            
            // Anchor to full screen to freely position buttons
            containerRt.anchorMin = new Vector2(0f, 0f);
            containerRt.anchorMax = new Vector2(1f, 1f);
            containerRt.pivot = new Vector2(0.5f, 0.5f);
            containerRt.sizeDelta = new Vector2(0f, 0f);
            containerRt.anchoredPosition = Vector2.zero;

            Vector2 size = new Vector2(180f, 180f);
            Vector2 anchor = new Vector2(0.5f, 0f);
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            // Row 1 (Bottom): Rotate L, Drop, Rotate R
            CreateButton(containerRt, "RotateLeftBtn", "Rotate L", new Vector2(-210f, 120f), size, anchor, pivot, rotateLeftSprite, relay != null ? relay.OnRotateLeft : null);
            CreateButton(containerRt, "DropBtn", "Drop", new Vector2(0f, 120f), size, anchor, pivot, dropSprite, relay != null ? relay.OnSoftDrop : null);
            CreateButton(containerRt, "RotateRightBtn", "Rotate R", new Vector2(210f, 120f), size, anchor, pivot, rotateRightSprite, relay != null ? relay.OnRotateRight : null);
            
            // Row 2 (Top): Left, Hold, Right
            CreateButton(containerRt, "LeftBtn", "Left", new Vector2(-210f, 320f), size, anchor, pivot, leftSprite, relay != null ? relay.OnMoveLeft : null);
            CreateButton(containerRt, "HoldBtn", "Hold", new Vector2(0f, 320f), size, anchor, pivot, holdSprite, relay != null ? relay.OnHoldPiece : null);
            CreateButton(containerRt, "RightBtn", "Right", new Vector2(210f, 320f), size, anchor, pivot, rightSprite, relay != null ? relay.OnMoveRight : null);
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null && canvas.transform.Find("MobileControls") == null)
                {
                    GenerateButtons();
                }
            }
        }

        private void CreateButton(RectTransform parent, string name, string text, Vector2 pos, Vector2 size, Vector2 anchor, Vector2 pivot, Sprite sprite, UnityEngine.Events.UnityAction action)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            Image img = btnObj.GetComponent<Image>();
            img.sprite = sprite;
            if (sprite != null)
            {
                img.color = Color.white; 
            }
            else
            {
                img.color = new Color(0.15f, 0.15f, 0.15f, 0.75f); 
            }

            Button btn = btnObj.GetComponent<Button>();
            if (action != null)
            {
                btn.onClick.AddListener(action);
            }

            // Only create text if we don't have a sprite
            if (sprite == null)
            {
                GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Text));
                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.SetParent(rt, false);
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.sizeDelta = Vector2.zero;
                textRt.anchoredPosition = Vector2.zero;

                UnityEngine.UI.Text txt = textObj.GetComponent<UnityEngine.UI.Text>();
                txt.text = text;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = Color.white;
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 45;
                txt.fontStyle = FontStyle.Bold;
            }
        }
    }
}
