using System.Collections;
using GraviTrix.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GraviTrix.UI
{
    public sealed class PauseMenuView : MonoBehaviour
    {
        private const string HomeSceneName = "HomeScene";
        private const string SfxVolumeKey = "SfxVolume";
        private const string MusicVolumeKey = "MusicVolume";

        [SerializeField] private GameController gameController;
        [SerializeField] private Sprite pauseButtonSprite;
        [SerializeField] private Sprite cancelButtonSprite;

        private Canvas pauseCanvas;
        private CanvasGroup canvasGroup;
        private GameObject popupRoot;
        private bool isPaused;

        private Text musicVolumeText;
        private Text sfxVolumeText;
        private Slider musicVolumeSlider;
        private Slider sfxVolumeSlider;
        private float musicVolume;
        private float sfxVolume;

        private void Start() { BuildPauseButton(); }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        private void BuildPauseButton()
        {
            GameObject canvasObj = new GameObject("PauseButton Canvas");
            canvasObj.transform.SetParent(transform, false);
            Canvas c = canvasObj.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 90;
            CanvasScaler sc = canvasObj.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1080, 1920);
            sc.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject btnObj = new GameObject("pause_button");
            btnObj.transform.SetParent(canvasObj.transform, false);
            Image img = btnObj.AddComponent<Image>();
            if (pauseButtonSprite != null) { img.sprite = pauseButtonSprite; img.preserveAspect = true; }
            else img.color = new Color(0.3f, 0.2f, 0.5f, 0.85f);
            img.raycastTarget = true;
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(TogglePause);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f); rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(30f, -30f);
            rt.sizeDelta = new Vector2(100f, 100f);

            if (pauseButtonSprite == null)
            {
                GameObject t = MakeRect("Icon", btnObj.transform);
                Text tx = t.AddComponent<Text>();
                tx.text = "\u275A\u275A"; tx.font = GetFont(); tx.fontSize = 38;
                tx.alignment = TextAnchor.MiddleCenter; tx.color = Color.white; tx.raycastTarget = false;
                Stretch(t);
            }
        }

        public void TogglePause() { if (isPaused) ResumeGame(); else PauseGame(); }

        private void PauseGame()
        {
            isPaused = true; Time.timeScale = 0f;
            if (popupRoot == null) BuildPopup();
            popupRoot.SetActive(true);
            StartCoroutine(FadeIn(0.25f));
        }

        private void ResumeGame()
        {
            isPaused = false; Time.timeScale = 1f;
            if (popupRoot != null) popupRoot.SetActive(false);
        }

        private void BuildPopup()
        {
            GameObject canvasObj = new GameObject("PauseMenu Canvas");
            canvasObj.transform.SetParent(transform, false);
            pauseCanvas = canvasObj.AddComponent<Canvas>();
            pauseCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            pauseCanvas.sortingOrder = 95;
            CanvasScaler sc = canvasObj.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1080, 1920);
            sc.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();
            canvasGroup = canvasObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            popupRoot = canvasObj;

            // Dark overlay
            GameObject ov = MakeRect("DarkOverlay", canvasObj.transform);
            Image ovImg = ov.AddComponent<Image>();
            ovImg.color = new Color(0f, 0f, 0f, 0.6f); ovImg.raycastTarget = true;
            Stretch(ov);

            // Navy Blue Rounded Border Frame
            GameObject border = MakeRect("Blue Border", canvasObj.transform);
            Image brdImg = border.AddComponent<Image>();
            brdImg.sprite = CreateRoundedPanelSprite(740, 870, 46);
            brdImg.type = Image.Type.Simple;
            brdImg.color = new Color32(20, 40, 120, 255);
            brdImg.raycastTarget = false;
            RectTransform brt = border.GetComponent<RectTransform>();
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = Vector2.zero;
            brt.sizeDelta = new Vector2(740f, 870f);

            // Panel
            GameObject panel = MakeRect("Panel", canvasObj.transform);
            Image pImg = panel.AddComponent<Image>();
            pImg.sprite = CreateRoundedPanelSprite(720, 850, 42);
            pImg.type = Image.Type.Simple;
            Shadow ps = panel.AddComponent<Shadow>();
            ps.effectColor = new Color(0f, 0f, 0f, 0.45f);
            ps.effectDistance = new Vector2(0f, -10f);
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.pivot = new Vector2(0.5f, 0.5f);
            prt.anchoredPosition = Vector2.zero;
            prt.sizeDelta = new Vector2(720f, 850f);

            // Title
            MakeLabel(panel.transform, "PAUSED", 62, new Vector2(0f, 330f), new Color32(255, 225, 90, 255));

            // Volume sliders (same style as Options popup)
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
            sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 0.8f);
            CreateMusicVolumeSlider(panel.transform, new Vector2(0f, 180f));
            CreateSfxVolumeSlider(panel.transform, new Vector2(0f, 15f));

            // Buttons
            CreatePopupButton(panel.transform, "RESTART", new Vector2(0f, -150f), OnRestartClicked);
            CreatePopupButton(panel.transform, "QUIT GAME", new Vector2(0f, -280f), OnQuitClicked);

            // Close (X) button
            BuildCloseButton(panel.transform);
        }

        // ─── Volume Sliders (matching Options popup style) ───

        private void CreateMusicVolumeSlider(Transform parent, Vector2 pos)
        {
            GameObject root = MakeRect("Music Volume Slider Root", parent);
            RectTransform rrt = root.GetComponent<RectTransform>();
            rrt.anchorMin = rrt.anchorMax = new Vector2(0.5f, 0.5f);
            rrt.pivot = new Vector2(0.5f, 0.5f);
            rrt.anchoredPosition = pos; rrt.sizeDelta = new Vector2(560f, 130f);

            GameObject lbl = MakeRect("Music Volume Label", root.transform);
            musicVolumeText = lbl.AddComponent<Text>();
            musicVolumeText.font = GetFont(); musicVolumeText.fontSize = 30;
            musicVolumeText.fontStyle = FontStyle.Bold;
            musicVolumeText.alignment = TextAnchor.MiddleCenter;
            musicVolumeText.color = new Color32(255, 225, 90, 255);
            musicVolumeText.raycastTarget = false;
            RectTransform lrt = lbl.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.5f, 1f); lrt.anchorMax = new Vector2(0.5f, 1f);
            lrt.pivot = new Vector2(0.5f, 0.5f);
            lrt.anchoredPosition = new Vector2(0f, -20f); lrt.sizeDelta = new Vector2(560f, 50f);

            musicVolumeSlider = BuildSlider(root.transform);
            musicVolumeSlider.value = musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            UpdateMusicVolumeText();
        }

        private void CreateSfxVolumeSlider(Transform parent, Vector2 pos)
        {
            GameObject root = MakeRect("SFX Volume Slider Root", parent);
            RectTransform rrt = root.GetComponent<RectTransform>();
            rrt.anchorMin = rrt.anchorMax = new Vector2(0.5f, 0.5f);
            rrt.pivot = new Vector2(0.5f, 0.5f);
            rrt.anchoredPosition = pos; rrt.sizeDelta = new Vector2(560f, 130f);

            GameObject lbl = MakeRect("SFX Volume Label", root.transform);
            sfxVolumeText = lbl.AddComponent<Text>();
            sfxVolumeText.font = GetFont(); sfxVolumeText.fontSize = 30;
            sfxVolumeText.fontStyle = FontStyle.Bold;
            sfxVolumeText.alignment = TextAnchor.MiddleCenter;
            sfxVolumeText.color = new Color32(255, 225, 90, 255);
            sfxVolumeText.raycastTarget = false;
            RectTransform lrt = lbl.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0.5f, 1f); lrt.anchorMax = new Vector2(0.5f, 1f);
            lrt.pivot = new Vector2(0.5f, 0.5f);
            lrt.anchoredPosition = new Vector2(0f, -20f); lrt.sizeDelta = new Vector2(560f, 50f);

            sfxVolumeSlider = BuildSlider(root.transform);
            sfxVolumeSlider.value = sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            UpdateSfxVolumeText();
        }

        private Slider BuildSlider(Transform parent)
        {
            GameObject sObj = MakeRect("Slider", parent);
            RectTransform srt = sObj.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0f); srt.anchorMax = new Vector2(0.5f, 0f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0f, 35f); srt.sizeDelta = new Vector2(520f, 55f);

            // Background
            GameObject bg = MakeRect("Background", sObj.transform);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.sprite = CreateSliderTrackSprite(520, 28, 14, new Color32(45, 28, 80, 255), new Color32(160, 130, 210, 255));
            bgImg.type = Image.Type.Simple;
            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.5f); bgRt.anchorMax = new Vector2(1f, 0.5f);
            bgRt.pivot = new Vector2(0.5f, 0.5f);
            bgRt.anchoredPosition = Vector2.zero; bgRt.sizeDelta = new Vector2(0f, 28f);

            // Fill area
            GameObject fa = MakeRect("Fill Area", sObj.transform);
            RectTransform fart = fa.GetComponent<RectTransform>();
            fart.anchorMin = new Vector2(0f, 0.5f); fart.anchorMax = new Vector2(1f, 0.5f);
            fart.pivot = new Vector2(0.5f, 0.5f);
            fart.anchoredPosition = Vector2.zero; fart.sizeDelta = new Vector2(-32f, 28f);

            GameObject fill = MakeRect("Fill", fa.transform);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.sprite = CreateSliderTrackSprite(520, 28, 14, new Color32(255, 220, 45, 255), new Color32(235, 135, 22, 255));
            fillImg.type = Image.Type.Simple;
            RectTransform frt = fill.GetComponent<RectTransform>();
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;

            // Handle area
            GameObject ha = MakeRect("Handle Slide Area", sObj.transform);
            RectTransform hart = ha.GetComponent<RectTransform>();
            hart.anchorMin = Vector2.zero; hart.anchorMax = Vector2.one;
            hart.offsetMin = new Vector2(18f, 0f); hart.offsetMax = new Vector2(-18f, 0f);

            GameObject handle = MakeRect("Handle", ha.transform);
            Image hImg = handle.AddComponent<Image>();
            hImg.sprite = CreateCircleSprite(48, new Color32(255, 235, 95, 255), new Color32(145, 75, 20, 255));
            hImg.type = Image.Type.Simple;
            RectTransform hrt = handle.GetComponent<RectTransform>();
            hrt.sizeDelta = new Vector2(48f, 48f);

            Slider slider = sObj.AddComponent<Slider>();
            slider.fillRect = frt; slider.handleRect = hrt;
            slider.targetGraphic = hImg;
            slider.minValue = 0f; slider.maxValue = 1f; slider.wholeNumbers = false;
            return slider;
        }

        // ─── Popup Button (matching Options popup style) ───

        private void CreatePopupButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            GameObject obj = MakeRect("Btn_" + label, parent);
            Image img = obj.AddComponent<Image>();
            img.sprite = CreateRoundedButtonSprite(500, 95, 26);
            img.type = Image.Type.Simple; img.raycastTarget = true;
            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img; btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(action);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(500f, 95f);

            // Dark overlay for hover
            GameObject dov = MakeRect("Hover Dark Overlay", obj.transform);
            Image dovImg = dov.AddComponent<Image>();
            dovImg.sprite = img.sprite; dovImg.type = Image.Type.Simple;
            dovImg.color = new Color(0f, 0f, 0f, 0f); dovImg.raycastTarget = false;
            Stretch(dov);

            GameObject txt = MakeRect("Text", obj.transform);
            Text t = txt.AddComponent<Text>();
            t.text = label; t.font = GetFont(); t.fontSize = 36;
            t.fontStyle = FontStyle.Bold; t.alignment = TextAnchor.MiddleCenter;
            t.color = new Color32(75, 42, 22, 255); t.raycastTarget = false;
            Stretch(txt);

            MainMenuButtonEffect fx = obj.AddComponent<MainMenuButtonEffect>();
            fx.SetOverlay(dovImg);
            dov.transform.SetAsLastSibling();
            txt.transform.SetAsLastSibling();
        }

        private void BuildCloseButton(Transform panelTransform)
        {
            GameObject obj = new GameObject("cancel_button");
            obj.transform.SetParent(panelTransform, false);
            Image img = obj.AddComponent<Image>();
            if (cancelButtonSprite != null) { img.sprite = cancelButtonSprite; img.preserveAspect = true; }
            else img.color = new Color(0.6f, 0.2f, 0.2f, 0.9f);
            img.raycastTarget = true;
            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img; btn.onClick.AddListener(ResumeGame);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-15f, -15f); rt.sizeDelta = new Vector2(80f, 80f);

            if (cancelButtonSprite == null)
            {
                GameObject t = MakeRect("XText", obj.transform);
                Text tx = t.AddComponent<Text>();
                tx.text = "\u2715"; tx.font = GetFont(); tx.fontSize = 42;
                tx.alignment = TextAnchor.MiddleCenter; tx.color = Color.white; tx.raycastTarget = false;
                Stretch(t);
            }
        }

        // ─── Callbacks ───

        private void OnMusicVolumeChanged(float v)
        {
            musicVolume = v;
            PlayerPrefs.SetFloat(MusicVolumeKey, v); PlayerPrefs.Save();
            UpdateMusicVolumeText();
            if (MusicManager.Instance != null) MusicManager.Instance.SetVolume(v);
        }

        private void OnSfxVolumeChanged(float v)
        {
            sfxVolume = v;
            PlayerPrefs.SetFloat(SfxVolumeKey, v); PlayerPrefs.Save();
            UpdateSfxVolumeText();
            if (SfxManager.Instance != null) SfxManager.Instance.SetVolume(v);
        }

        private void UpdateMusicVolumeText()
        {
            if (musicVolumeText != null)
                musicVolumeText.text = "MUSIC VOLUME: " + Mathf.RoundToInt(musicVolume * 100f) + "%";
        }

        private void UpdateSfxVolumeText()
        {
            if (sfxVolumeText != null)
                sfxVolumeText.text = "SFX VOLUME: " + Mathf.RoundToInt(sfxVolume * 100f) + "%";
        }

        private void OnRestartClicked()
        {
            ResumeGame();
            if (gameController != null) gameController.RestartGame();
            else
            {
                GameController gc = FindObjectOfType<GameController>();
                if (gc != null) gc.RestartGame();
                else SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        private void OnQuitClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(HomeSceneName);
        }

        private IEnumerator FadeIn(float duration)
        {
            if (canvasGroup == null) yield break;
            canvasGroup.alpha = 0f; float el = 0f;
            while (el < duration)
            {
                el += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(el / duration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // ─── Sprite Generation (same as MainMenuBuilder) ───

        private Sprite CreateRoundedPanelSprite(int w, int h, int r)
        {
            Texture2D tex = new Texture2D(w, h); tex.filterMode = FilterMode.Bilinear;
            Color cen = new Color32(30, 50, 110, 250), edg = new Color32(18, 30, 75, 250), brd = new Color32(50, 90, 180, 255);
            Vector2 center = new Vector2(w / 2f, h / 2f);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float d = GetRoundedRectDistance(x, y, w, h, r);
                if (d > 0f) { tex.SetPixel(x, y, Color.clear); continue; }
                if (d > -3f) { tex.SetPixel(x, y, brd); continue; }
                float dist = Vector2.Distance(new Vector2(x, y), center) / (w * 0.5f);
                tex.SetPixel(x, y, Color.Lerp(cen, edg, dist * 0.8f));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        private Sprite CreateRoundedButtonSprite(int w, int h, int r)
        {
            Texture2D tex = new Texture2D(w, h); tex.filterMode = FilterMode.Bilinear;
            Color left = new Color32(255, 220, 45, 255), right = new Color32(235, 135, 22, 255);
            Color brd = new Color32(155, 82, 18, 255), hi = new Color32(255, 238, 105, 255);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float d = GetRoundedRectDistance(x, y, w, h, r);
                if (d > 0f) { tex.SetPixel(x, y, Color.clear); continue; }
                if (d > -3f) { tex.SetPixel(x, y, brd); continue; }
                Color c = Color.Lerp(left, right, (float)x / w);
                float ht = Mathf.Clamp01((float)(h - y) / (h * 0.25f));
                tex.SetPixel(x, y, Color.Lerp(c, hi, ht * 0.35f));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        private Sprite CreateSliderTrackSprite(int w, int h, int r, Color left, Color right)
        {
            Texture2D tex = new Texture2D(w, h); tex.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float d = GetRoundedRectDistance(x, y, w, h, r);
                if (d > 0f) { tex.SetPixel(x, y, Color.clear); continue; }
                tex.SetPixel(x, y, Color.Lerp(left, right, (float)x / (w - 1)));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        private Sprite CreateCircleSprite(int size, Color fill, Color border)
        {
            Texture2D tex = new Texture2D(size, size); tex.filterMode = FilterMode.Bilinear;
            Vector2 c = new Vector2(size / 2f, size / 2f);
            float rad = size / 2f - 2f, bRad = rad - 5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), c);
                if (dist > rad) tex.SetPixel(x, y, Color.clear);
                else if (dist > bRad) tex.SetPixel(x, y, border);
                else tex.SetPixel(x, y, fill);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private float GetRoundedRectDistance(int x, int y, int w, int h, int r)
        {
            float px = x - w / 2f, py = y - h / 2f;
            float hw = w / 2f - r, hh = h / 2f - r;
            float dx = Mathf.Abs(px) - hw, dy = Mathf.Abs(py) - hh;
            return new Vector2(Mathf.Max(dx, 0), Mathf.Max(dy, 0)).magnitude + Mathf.Min(Mathf.Max(dx, dy), 0) - r;
        }

        // ─── Helpers ───

        private void MakeLabel(Transform p, string text, int size, Vector2 pos, Color col)
        {
            GameObject o = MakeRect("Lbl_" + text, p);
            Text t = o.AddComponent<Text>();
            t.text = text; t.font = GetFont(); t.fontSize = size;
            t.fontStyle = FontStyle.Bold; t.alignment = TextAnchor.MiddleCenter;
            t.color = col; t.raycastTarget = false;
            Shadow s = o.AddComponent<Shadow>();
            s.effectColor = new Color(0, 0, 0, 0.5f); s.effectDistance = new Vector2(4f, -4f);
            RectTransform rt = o.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(600f, 100f);
        }

        private GameObject MakeRect(string n, Transform p)
        {
            GameObject o = new GameObject(n); o.AddComponent<RectTransform>();
            o.transform.SetParent(p, false); return o;
        }

        private void Stretch(GameObject o)
        {
            RectTransform rt = o.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        private Font GetFont() { return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
    }
}
