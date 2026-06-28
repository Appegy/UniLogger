using UnityEngine;

namespace Appegy.UniLogger.Example
{
    public class InMemoryLogOverlay : MonoBehaviour
    {
        private const int WindowId = 0x554C4F47;
        private const KeyCode ToggleKey = KeyCode.BackQuote;
        private const float RefreshInterval = 0.2f;
        private const float ReferenceHeight = 720f;

        private InMemoryTarget _target;
        private string _content = string.Empty;
        private int _lineCount;
        private float _nextRefresh;

        private bool _visible = true;
        private bool _expanded = true;
        private bool _tail = true;
        private bool _resizing;
        private Vector2 _scroll;
        private Rect _window;
        private float _expandedHeight;
        private bool _placed;

        private bool _ready;
        private Texture2D _panelTex;
        private Texture2D _titleTex;
        private Texture2D _buttonTex;
        private Texture2D _buttonHoverTex;
        private GUIStyle _windowStyle;
        private GUIStyle _titleBarStyle;
        private GUIStyle _titleTextStyle;
        private GUIStyle _logStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _toggleOnStyle;
        private GUIStyle _gripStyle;
        private GUIStyle _showStyle;

        public static InMemoryLogOverlay Spawn()
        {
            var go = new GameObject("[UniLogger Overlay]") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            return go.AddComponent<InMemoryLogOverlay>();
        }

        private void Update()
        {
            if (IsTogglePressed())
            {
                _visible = !_visible;
            }
            if (Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + RefreshInterval;
            Refresh();
        }

        private void Refresh()
        {
            if (_target == null) _target = ULogger.GetTarget<InMemoryTarget>();
            var previousLength = _content.Length;
            _content = _target != null ? _target.GetContent() : string.Empty;
            _lineCount = CountLines(_content);
            if (_tail && _content.Length != previousLength) _scroll.y = float.MaxValue;
        }

        private void OnGUI()
        {
            EnsureResources();

            var scale = Mathf.Clamp(Screen.height / ReferenceHeight, 0.85f, 3f);
            ApplyScale(scale);

            var area = SafeArea();

            if (!_visible)
            {
                DrawReopenButton(area, scale);
                return;
            }

            if (!_placed) PlaceDefault(area);

            var titleHeight = Mathf.Round(26f * scale);
            _window.width = Mathf.Clamp(_window.width, 220f * scale, area.width);
            _expandedHeight = Mathf.Clamp(_expandedHeight, titleHeight * 3f, area.height);
            _window.height = _expanded ? _expandedHeight : titleHeight + _windowStyle.padding.vertical;

            _window = GUI.Window(WindowId, _window, DrawWindow, GUIContent.none, _windowStyle);

            ClampToArea(ref _window, area);
        }

        private void DrawWindow(int id)
        {
            var scale = Mathf.Clamp(Screen.height / ReferenceHeight, 0.85f, 3f);
            var titleHeight = Mathf.Round(26f * scale);
            var glyph = Mathf.Round(30f * scale);

            GUILayout.BeginHorizontal(_titleBarStyle, GUILayout.Height(titleHeight));
            if (GUILayout.Button(_expanded ? "▼" : "▶", _buttonStyle, GUILayout.Width(glyph)))
            {
                _expanded = !_expanded;
            }
            GUILayout.Label(_lineCount > 0 ? $"UniLogger  ·  {_lineCount} lines" : "UniLogger", _titleTextStyle);
            GUILayout.FlexibleSpace();
            _tail = GUILayout.Toggle(_tail, "Tail", _tail ? _toggleOnStyle : _buttonStyle, GUILayout.Width(glyph * 1.9f));
            if (GUILayout.Button("Copy", _buttonStyle, GUILayout.Width(glyph * 1.9f)))
            {
                GUIUtility.systemCopyBuffer = _content;
            }
            if (GUILayout.Button("Clear", _buttonStyle, GUILayout.Width(glyph * 1.9f)))
            {
                _target?.Clear();
                Refresh();
            }
            if (GUILayout.Button("✕", _buttonStyle, GUILayout.Width(glyph)))
            {
                _visible = false;
            }
            GUILayout.EndHorizontal();

            if (_expanded)
            {
                _scroll = GUILayout.BeginScrollView(_scroll, false, true);
                if (_content.Length == 0)
                {
                    GUILayout.Label("No log entries yet.", _logStyle);
                }
                else
                {
                    GUILayout.Label(_content, _logStyle);
                }
                GUILayout.EndScrollView();

                HandleResize(scale);
            }

            GUI.DragWindow(new Rect(0f, 0f, _window.width, titleHeight));
        }

        private void HandleResize(float scale)
        {
            var size = Mathf.Round(18f * scale);
            var grip = new Rect(_window.width - size, _window.height - size, size, size);
            GUI.Label(grip, "◢", _gripStyle);

            var e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDown when grip.Contains(e.mousePosition):
                    _resizing = true;
                    e.Use();
                    break;
                case EventType.MouseDrag when _resizing:
                    _window.width = e.mousePosition.x + size * 0.5f;
                    _expandedHeight = e.mousePosition.y + size * 0.5f;
                    e.Use();
                    break;
                case EventType.MouseUp when _resizing:
                    _resizing = false;
                    e.Use();
                    break;
            }
        }

        private void DrawReopenButton(Rect area, float scale)
        {
            var width = Mathf.Round(140f * scale);
            var height = Mathf.Round(30f * scale);
            var rect = new Rect(area.x + 8f * scale, area.y + 8f * scale, width, height);
            if (GUI.Button(rect, $"▸ Logs ({_lineCount})", _showStyle))
            {
                _visible = true;
            }
        }

        private void PlaceDefault(Rect area)
        {
            var portrait = Screen.height >= Screen.width;
            var width = portrait ? area.width * 0.94f : Mathf.Clamp(area.width * 0.44f, 360f, area.width);
            _expandedHeight = area.height * 0.5f;
            _window = new Rect(area.x + 8f, area.y + 8f, width, _expandedHeight);
            _placed = true;
        }

        private Rect SafeArea()
        {
            var safe = Screen.safeArea;
            var top = Screen.height - (safe.y + safe.height);
            return new Rect(safe.x, top, safe.width, safe.height);
        }

        private static void ClampToArea(ref Rect window, Rect area)
        {
            window.width = Mathf.Min(window.width, area.width);
            window.height = Mathf.Min(window.height, area.height);
            window.x = Mathf.Clamp(window.x, area.x, area.xMax - window.width);
            window.y = Mathf.Clamp(window.y, area.y, area.yMax - window.height);
        }

        private static int CountLines(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            var lines = 0;
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') lines++;
            }
            return text[text.Length - 1] == '\n' ? lines : lines + 1;
        }

        private static bool IsTogglePressed()
        {
            try
            {
                return Input.GetKeyDown(ToggleKey);
            }
            catch
            {
                return false;
            }
        }

        private void ApplyScale(float scale)
        {
            var body = Mathf.RoundToInt(13f * scale);
            var ui = Mathf.RoundToInt(13f * scale);
            _logStyle.fontSize = body;
            _titleTextStyle.fontSize = ui;
            _buttonStyle.fontSize = ui;
            _toggleOnStyle.fontSize = ui;
            _showStyle.fontSize = ui;
            _gripStyle.fontSize = Mathf.RoundToInt(14f * scale);
            var pad = Mathf.RoundToInt(8f * scale);
            _logStyle.padding = new RectOffset(pad, pad, pad, pad);
        }

        private void EnsureResources()
        {
            if (_ready) return;
            _ready = true;

            _panelTex = SolidTexture(new Color(0.06f, 0.06f, 0.08f, 0.93f));
            _titleTex = SolidTexture(new Color(0.12f, 0.13f, 0.17f, 0.98f));
            _buttonTex = SolidTexture(new Color(1f, 1f, 1f, 0.04f));
            _buttonHoverTex = SolidTexture(new Color(1f, 1f, 1f, 0.14f));

            _windowStyle = new GUIStyle
            {
                normal = { background = _panelTex },
                border = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(2, 2, 2, 2),
            };

            _titleBarStyle = new GUIStyle
            {
                normal = { background = _titleTex },
                padding = new RectOffset(6, 6, 4, 4),
                margin = new RectOffset(0, 0, 0, 2),
            };

            _titleTextStyle = new GUIStyle
            {
                normal = { textColor = new Color(0.6f, 0.8f, 1f) },
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(6, 6, 0, 0),
            };

            _logStyle = new GUIStyle
            {
                normal = { textColor = new Color(0.86f, 0.87f, 0.9f) },
                richText = true,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
            };

            _buttonStyle = new GUIStyle
            {
                normal = { background = _buttonTex, textColor = new Color(0.85f, 0.86f, 0.9f) },
                hover = { background = _buttonHoverTex, textColor = Color.white },
                active = { background = _buttonHoverTex, textColor = Color.white },
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(4, 4, 4, 4),
            };

            _toggleOnStyle = new GUIStyle(_buttonStyle)
            {
                normal = { background = _buttonHoverTex, textColor = new Color(0.55f, 0.95f, 0.7f) },
            };

            _gripStyle = new GUIStyle
            {
                normal = { textColor = new Color(1f, 1f, 1f, 0.35f) },
                alignment = TextAnchor.LowerRight,
            };

            _showStyle = new GUIStyle
            {
                normal = { background = _titleTex, textColor = new Color(0.8f, 0.85f, 0.95f) },
                hover = { background = _buttonHoverTex, textColor = Color.white },
                active = { background = _buttonHoverTex, textColor = Color.white },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };
        }

        private static Texture2D SolidTexture(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void OnDestroy()
        {
            DestroyTexture(_panelTex);
            DestroyTexture(_titleTex);
            DestroyTexture(_buttonTex);
            DestroyTexture(_buttonHoverTex);
        }

        private static void DestroyTexture(Texture2D texture)
        {
            if (texture != null) Destroy(texture);
        }
    }
}
