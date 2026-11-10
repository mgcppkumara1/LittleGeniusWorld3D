using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StationTag : MonoBehaviour { public string id; }
public class Spinner : MonoBehaviour
{
    public float speed = 60f;
    void Update() { transform.Rotate(0, speed * Time.deltaTime, 0, Space.World); }
}

// Builds the whole 3D world + UI in code. Attach to one empty GameObject in an empty scene.
public class LGWGame : MonoBehaviour
{
    enum Mode { Home, Learn, Puzzle, Alphabet, Numbers, Treasure, Gate, Parent }

    static readonly Color Ink = new Color(.14f, .29f, .27f);
    static readonly Color Mint = new Color(.18f, .54f, .47f);
    static readonly Color Sun = new Color(1f, .72f, .36f);
    static readonly Vector3 StagePos = new Vector3(0, 1000, 0);

    class Station { public string id, title, shortName; public Color color; public Transform t; public RectTransform label; }

    Mode mode;
    Camera cam;
    Transform worldRoot, stageRoot, stageHolder, numRoot, cloudRing, mascot;
    readonly List<Station> stations = new List<Station>();
    RectTransform canvasRect, labelsRoot, actPanel, bottomBar;
    Text starsText;
    AndroidTts tts;
    int stars, wordIndex, puzzleTarget, gateAnswer;
    float orbit = .6f, downTime;
    bool dragging, moved, camSnap = true, locked, confirmReset;
    Vector2 downPos;
    float lastX;

    static Vector2 V2(float x, float y) { return new Vector2(x, y); }
    static Vector3 V3(float x, float y, float z) { return new Vector3(x, y, z); }

    void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        stars = PlayerPrefs.GetInt("lgw_stars", 12);
        tts = new AndroidTts();
        BuildCameraAndLight();
        BuildWorld();
        BuildStage();
        BuildUI();
        SetMode(Mode.Home);
    }

    void OnDestroy() { if (tts != null) tts.Shutdown(); }

    // ---------------------------------------------------------------- 3D
    void BuildCameraAndLight()
    {
        var cg = new GameObject("Main Camera");
        cg.tag = "MainCamera";
        cam = cg.AddComponent<Camera>();
        cg.AddComponent<AudioListener>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(.55f, .82f, .96f);
        cam.fieldOfView = 60; cam.nearClipPlane = .3f; cam.farClipPlane = 600;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.62f, .66f, .7f);

        var lg = new GameObject("Sun");
        var l = lg.AddComponent<Light>();
        l.type = LightType.Directional; l.intensity = 1.1f; l.color = new Color(1f, .96f, .88f);
        l.shadows = LightShadows.Soft;
        lg.transform.rotation = Quaternion.Euler(50, -30, 0);
    }

    void BuildWorld()
    {
        worldRoot = new GameObject("World").transform;
        Toys.P(PrimitiveType.Plane, worldRoot, V3(0, -.6f, 0), V3(40, 1, 40), new Color(.2f, .65f, .8f));
        Toys.P(PrimitiveType.Cylinder, worldRoot, V3(0, -.55f, 0), V3(12, .02f, 12), new Color(.5f, .88f, .9f));
        Toys.P(PrimitiveType.Cylinder, worldRoot, V3(0, -.3f, 0), V3(9, .3f, 9), new Color(.96f, .87f, .62f));
        Toys.P(PrimitiveType.Cylinder, worldRoot, V3(0, .02f, 0), V3(7.6f, .06f, 7.6f), new Color(.46f, .78f, .4f));

        for (int i = 0; i < 5; i++)
        {
            float a = (i * 72f + 36f) * Mathf.Deg2Rad;
            Palm(V3(Mathf.Sin(a) * 3.9f, 0, Mathf.Cos(a) * 3.9f), i * 55f);
        }
        for (int i = 0; i < 4; i++)
        {
            float a = (i * 90f + 20f) * Mathf.Deg2Rad;
            Toys.P(PrimitiveType.Sphere, worldRoot, V3(Mathf.Sin(a) * 5.3f, -.35f, Mathf.Cos(a) * 5.3f), V3(.9f, .6f, .8f), new Color(.55f, .57f, .6f));
        }
        Random.InitState(7);
        Color[] fl = { new Color(1f, .45f, .55f), new Color(1f, .85f, .3f), Color.white, new Color(.7f, .5f, 1f) };
        for (int i = 0; i < 26; i++)
        {
            float a = Random.value * Mathf.PI * 2f, d = Random.Range(1.3f, 3.7f);
            Toys.P(PrimitiveType.Sphere, worldRoot, V3(Mathf.Sin(a) * d, .1f, Mathf.Cos(a) * d), V3(.15f, .11f, .15f), fl[i % fl.Length]);
        }

        cloudRing = new GameObject("Clouds").transform;
        cloudRing.SetParent(worldRoot, false);
        for (int i = 0; i < 7; i++)
        {
            float a = i * 51f * Mathf.Deg2Rad, d = Random.Range(9f, 16f);
            var c = new GameObject("Cloud").transform;
            c.SetParent(cloudRing, false);
            c.localPosition = V3(Mathf.Sin(a) * d, Random.Range(5f, 8f), Mathf.Cos(a) * d);
            Toys.P(PrimitiveType.Sphere, c, V3(-.8f, 0, 0), V3(1.4f, 1f, 1f), Color.white);
            Toys.P(PrimitiveType.Sphere, c, V3(0, .3f, 0), V3(1.9f, 1.4f, 1.2f), Color.white);
            Toys.P(PrimitiveType.Sphere, c, V3(.9f, 0, 0), V3(1.4f, 1f, 1f), Color.white);
        }

        BuildMascot();
        BuildHuts();
    }

    void Palm(Vector3 pos, float yaw)
    {
        var brown = new Color(.5f, .33f, .15f);
        var h = new GameObject("Palm").transform;
        h.SetParent(worldRoot, false);
        h.localPosition = pos; h.localEulerAngles = V3(0, yaw, 8);
        Toys.P(PrimitiveType.Capsule, h, V3(0, .95f, 0), V3(.22f, .95f, .22f), brown);
        for (int i = 0; i < 6; i++)
        {
            var lf = new GameObject("Leaf").transform;
            lf.SetParent(h, false);
            lf.localPosition = V3(0, 1.9f, 0); lf.localEulerAngles = V3(0, i * 60f, 0);
            Toys.P(PrimitiveType.Sphere, lf, V3(.6f, -.05f, 0), V3(1.2f, .07f, .38f), new Color(.22f, .65f, .27f), V3(0, 0, -15));
        }
        Toys.P(PrimitiveType.Sphere, h, V3(.12f, 1.78f, 0), S(.22f), new Color(.35f, .22f, .1f));
        Toys.P(PrimitiveType.Sphere, h, V3(-.1f, 1.78f, .1f), S(.22f), new Color(.35f, .22f, .1f));
    }
    static Vector3 S(float s) { return new Vector3(s, s, s); }

    void BuildMascot()
    {
        var m = new GameObject("Mascot").transform;
        m.SetParent(worldRoot, false);
        mascot = m;
        var body = Toys.P(PrimitiveType.Sphere, m, V3(0, .8f, 0), S(1.1f), new Color(1f, .85f, .3f), default(Vector3), true);
        body.AddComponent<StationTag>().id = "mascot";
        var dark = new Color(.08f, .08f, .12f);
        foreach (float sx in new[] { -1f, 1f })
        {
            Toys.P(PrimitiveType.Sphere, m, V3(sx * .2f, .95f, .46f), S(.3f), Color.white);
            Toys.P(PrimitiveType.Sphere, m, V3(sx * .2f, .95f, .6f), S(.14f), dark);
            Toys.P(PrimitiveType.Sphere, m, V3(sx * .38f, .72f, .44f), V3(.15f, .1f, .06f), new Color(1f, .6f, .65f));
            Toys.P(PrimitiveType.Sphere, m, V3(sx * .2f, .2f, .15f), V3(.28f, .14f, .4f), new Color(1f, .55f, .15f));
        }
        Toys.P(PrimitiveType.Cylinder, m, V3(0, 1.3f, 0), V3(.55f, .08f, .55f), dark);
        Toys.P(PrimitiveType.Cube, m, V3(0, 1.42f, 0), V3(.95f, .06f, .95f), new Color(.1f, .12f, .3f), V3(0, 45, 0));
        Toys.P(PrimitiveType.Sphere, m, V3(.45f, 1.3f, .45f), S(.12f), new Color(1f, .8f, .2f));
    }

    void BuildHuts()
    {
        string[] ids = { "learn", "puzzle", "alphabet", "numbers", "treasure" };
        string[] titles = { "Animal Safari", "Puzzle Garden", "Alphabet Cloud", "Number Bay", "Treasure Box" };
        string[] shorts = { "Animals", "Puzzles", "ABC", "Numbers", "Treasure" };
        Color[] cols =
        {
            new Color(.98f, .55f, .3f), new Color(.45f, .6f, .95f), new Color(.65f, .5f, .95f),
            new Color(.3f, .75f, .55f), new Color(.95f, .75f, .2f)
        };
        for (int i = 0; i < 5; i++)
        {
            float a = i * 72f * Mathf.Deg2Rad;
            var p = V3(Mathf.Sin(a) * 3.2f, 0, Mathf.Cos(a) * 3.2f);
            var hut = new GameObject("Hut_" + ids[i]).transform;
            hut.SetParent(worldRoot, false);
            hut.localPosition = p;
            hut.rotation = Quaternion.LookRotation(-p.normalized, Vector3.up);
            var b = Toys.P(PrimitiveType.Cube, hut, V3(0, .5f, 0), V3(1.3f, 1f, 1.3f), new Color(1f, .95f, .85f), default(Vector3), true);
            b.AddComponent<StationTag>().id = ids[i];
            Toys.P(PrimitiveType.Cube, hut, V3(0, 1.05f, 0), V3(1f, 1f, 1.5f), cols[i], V3(0, 0, 45));
            Toys.P(PrimitiveType.Cube, hut, V3(0, .35f, .66f), V3(.4f, .7f, .05f), new Color(.35f, .22f, .12f));
            var icon = Toys.Icon(ids[i]).transform;
            icon.SetParent(hut, false);
            icon.localPosition = V3(0, 2.4f, 0);
            icon.gameObject.AddComponent<Spinner>().speed = 70f;
            stations.Add(new Station { id = ids[i], title = titles[i], shortName = shorts[i], color = cols[i], t = hut });
        }
    }

    void BuildStage()
    {
        stageRoot = new GameObject("Stage").transform;
        stageRoot.position = StagePos;
        Toys.P(PrimitiveType.Cylinder, stageRoot, V3(0, -.15f, 0), V3(3.2f, .15f, 3.2f), new Color(1f, .9f, .7f));
        Toys.P(PrimitiveType.Cylinder, stageRoot, V3(0, -.5f, 0), V3(3.6f, .2f, 3.6f), Mint);
        stageHolder = new GameObject("ToyHolder").transform;
        stageHolder.SetParent(stageRoot, false);
        stageHolder.gameObject.AddComponent<Spinner>().speed = 35f;
        var ring = new GameObject("Sparkles").transform;
        ring.SetParent(stageRoot, false);
        ring.gameObject.AddComponent<Spinner>().speed = -50f;
        for (int i = 0; i < 8; i++)
        {
            float a = i * 45f * Mathf.Deg2Rad;
            Toys.P(PrimitiveType.Sphere, ring, V3(Mathf.Sin(a) * 2.2f, .6f + (i % 2) * .8f, Mathf.Cos(a) * 2.2f), S(.18f), Color.HSVToRGB(i / 8f, .5f, 1f));
        }
        numRoot = new GameObject("Numbers").transform;
        numRoot.SetParent(stageRoot, false);
    }

    void SetToy(string kind)
    {
        UIKit.Clear(stageHolder);
        var toy = Toys.Build(kind);
        toy.transform.SetParent(stageHolder, false);
        stageHolder.localEulerAngles = V3(0, 160, 0);
    }

    void ClearStage() { UIKit.Clear(stageHolder); UIKit.Clear(numRoot); }

    // ---------------------------------------------------------------- UI
    void BuildUI()
    {
        var cg = new GameObject("Canvas");
        var canvas = cg.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = cg.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = V2(1080, 1920);
        sc.matchWidthOrHeight = .5f;
        cg.AddComponent<GraphicRaycaster>();
        canvasRect = cg.GetComponent<RectTransform>();
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        labelsRoot = UIKit.Rect("Labels", canvasRect, Vector2.zero, Vector2.one);
        foreach (var s in stations)
        {
            var st = s;
            var b = UIKit.Btn(labelsRoot, st.title, 30, new Color(1, 1, 1, .9f), Ink, V2(.5f, .5f), V2(.5f, .5f), () => Open(st.id));
            st.label = b.GetComponent<RectTransform>();
            st.label.sizeDelta = V2(300, 70);
        }

        actPanel = UIKit.Rect("Activity", canvasRect, Vector2.zero, Vector2.one);

        bottomBar = UIKit.Rect("BottomBar", canvasRect, V2(.03f, .02f), V2(.97f, .15f));
        for (int i = 0; i < stations.Count; i++)
        {
            var st = stations[i];
            Vector2 mn, mx;
            UIKit.Cell(i, 5, 1, .06f, out mn, out mx);
            UIKit.Btn(bottomBar, st.shortName, 30, st.color, Color.white, mn, mx, () => Open(st.id));
        }

        var hud = UIKit.Rect("HUD", canvasRect, Vector2.zero, Vector2.one);
        UIKit.Label(hud, "LITTLE GENIUS WORLD", 36, Ink, V2(.04f, .935f), V2(.58f, .985f), TextAnchor.MiddleLeft);
        var pill = UIKit.Panel(hud, V2(.58f, .935f), V2(.80f, .985f), new Color(1, 1, 1, .88f));
        starsText = UIKit.Label(pill.transform, "", 34, Ink, Vector2.zero, Vector2.one);
        UIKit.Btn(hud, "Parent", 30, new Color(1, 1, 1, .88f), Ink, V2(.82f, .935f), V2(.96f, .985f), () => OpenGate());
        RefreshStars();
    }

    void RefreshStars() { if (starsText != null) starsText.text = "Stars: " + stars; }

    void AddStars(int n)
    {
        stars += n;
        PlayerPrefs.SetInt("lgw_stars", stars);
        RefreshStars();
    }

    void BackButton()
    {
        UIKit.Btn(actPanel, "< Home", 36, Mint, Color.white, V2(.04f, .86f), V2(.28f, .92f), () => SetMode(Mode.Home));
    }

    // ---------------------------------------------------------------- Flow
    void Open(string id)
    {
        switch (id)
        {
            case "mascot": tts.Speak("Hello! I am Little Genius. Let's learn together!", "en", true); break;
            case "learn": SetMode(Mode.Learn); break;
            case "puzzle": puzzleTarget = Random.Range(0, LGWData.Words.Length); SetMode(Mode.Puzzle); break;
            case "alphabet": SetMode(Mode.Alphabet); break;
            case "numbers": SetMode(Mode.Numbers); break;
            case "treasure": SetMode(Mode.Treasure); break;
        }
    }

    void OpenGate()
    {
        gateAnswer = 0;
        SetMode(Mode.Gate);
    }

    static bool UsesStage(Mode m) { return m == Mode.Learn || m == Mode.Puzzle || m == Mode.Numbers; }

    void SetMode(Mode m)
    {
        mode = m; camSnap = true; locked = false; confirmReset = false;
        UIKit.Clear(actPanel);
        ClearStage();
        bool home = m == Mode.Home;
        bottomBar.gameObject.SetActive(home);
        labelsRoot.gameObject.SetActive(home);
        switch (m)
        {
            case Mode.Learn: ShowLearn(); break;
            case Mode.Puzzle: ShowPuzzle(); break;
            case Mode.Alphabet: ShowAlphabet(); break;
            case Mode.Numbers: ShowNumbers(); break;
            case Mode.Treasure: ShowTreasure(); break;
            case Mode.Gate: ShowGate(); break;
            case Mode.Parent: ShowParent(); break;
        }
    }

    void SpeakPair(string en, string si)
    {
        tts.Speak(en.ToLower(), "en", true);
        if (!string.IsNullOrEmpty(si)) tts.Speak(si, "si", false);
    }

    void ShowLearn()
    {
        BackButton();
        var w = LGWData.Words[wordIndex % LGWData.Words.Length];
        SetToy(w.kind);
        var card = UIKit.Panel(actPanel, V2(.05f, .04f), V2(.95f, .40f), new Color(1, 1, 1, .93f));
        UIKit.Label(card.transform, w.en, 110, Ink, V2(0, .55f), V2(1, 1));
        UIKit.Label(card.transform, w.si, 84, Mint, V2(0, .32f), V2(1, .58f));
        UIKit.Btn(card.transform, "Listen", 50, Sun, Color.white, V2(.06f, .05f), V2(.48f, .28f), () => SpeakPair(w.en, w.si));
        UIKit.Btn(card.transform, "Next >", 50, Mint, Color.white, V2(.52f, .05f), V2(.94f, .28f), () =>
        {
            AddStars(1);
            wordIndex = (wordIndex + 1) % LGWData.Words.Length;
            SetMode(Mode.Learn);
        });
        SpeakPair(w.en, w.si);
    }

    void ShowPuzzle()
    {
        BackButton();
        var words = LGWData.Words;
        SetToy(words[puzzleTarget].kind);
        var card = UIKit.Panel(actPanel, V2(.05f, .04f), V2(.95f, .42f), new Color(1, 1, 1, .93f));
        var q = UIKit.Label(card.transform, "Which one is this?", 52, Ink, V2(0, .78f), V2(1, 1));
        var opts = new List<int> { puzzleTarget };
        while (opts.Count < 3) { int k = Random.Range(0, words.Length); if (!opts.Contains(k)) opts.Add(k); }
        for (int i = opts.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); int tmp = opts[i]; opts[i] = opts[j]; opts[j] = tmp; }
        Color[] cols = { new Color(.98f, .55f, .3f), new Color(.45f, .6f, .95f), new Color(.3f, .75f, .55f) };
        for (int r = 0; r < 3; r++)
        {
            int idx = opts[r];
            float y0 = .52f - r * .25f;
            UIKit.Btn(card.transform, words[idx].en, 56, cols[r], Color.white, V2(.06f, y0), V2(.94f, y0 + .24f), () => Pick(idx, q));
        }
    }

    void Pick(int idx, Text q)
    {
        if (locked) return;
        if (idx == puzzleTarget)
        {
            locked = true;
            AddStars(2);
            q.text = "Great job!";
            tts.Speak("Great job! " + LGWData.Words[idx].en.ToLower(), "en", true);
            StartCoroutine(NextPuzzle());
        }
        else
        {
            q.text = "Try again!";
            tts.Speak("Try again", "en", true);
        }
    }

    IEnumerator NextPuzzle()
    {
        yield return new WaitForSeconds(1.6f);
        if (mode != Mode.Puzzle) yield break;
        int n = LGWData.Words.Length, nt;
        do { nt = Random.Range(0, n); } while (nt == puzzleTarget);
        puzzleTarget = nt;
        SetMode(Mode.Puzzle);
    }

    void ShowAlphabet()
    {
        BackButton();
        var disp = UIKit.Label(actPanel, "Tap a letter!", 52, Ink, V2(.30f, .84f), V2(.96f, .93f), TextAnchor.MiddleLeft);
        var grid = UIKit.Panel(actPanel, V2(.04f, .04f), V2(.96f, .80f), new Color(1, 1, 1, .9f));
        for (int i = 0; i < 26; i++)
        {
            int k = i;
            string L = ((char)('A' + i)).ToString();
            string word = LGWData.LetterWords[i];
            Vector2 mn, mx;
            UIKit.Cell(i, 5, 6, .08f, out mn, out mx);
            UIKit.Btn(grid.transform, L, 64, Color.HSVToRGB(i / 26f, .55f, .92f), Color.white, mn, mx, () =>
            {
                disp.text = L + " is for " + word;
                tts.Speak(L + ". " + L + " is for " + word, "en", true);
                if (k % 5 == 0) AddStars(1);
            });
        }
    }

    void ShowNumbers()
    {
        BackButton();
        var info = UIKit.Label(actPanel, "Tap a number!", 60, Ink, V2(.05f, .35f), V2(.95f, .45f));
        var card = UIKit.Panel(actPanel, V2(.05f, .04f), V2(.95f, .34f), new Color(1, 1, 1, .93f));
        for (int i = 0; i < 10; i++)
        {
            int n = i + 1;
            Vector2 mn, mx;
            UIKit.Cell(i, 5, 2, .1f, out mn, out mx);
            UIKit.Btn(card.transform, n.ToString(), 70, Color.HSVToRGB(i / 10f, .55f, .92f), Color.white, mn, mx, () =>
            {
                info.text = n + "  " + LGWData.NumEn[n - 1] + "  " + LGWData.NumSi[n - 1];
                ShowCount(n);
                SpeakPair(LGWData.NumEn[n - 1], LGWData.NumSi[n - 1]);
                AddStars(1);
            });
        }
    }

    void ShowCount(int n)
    {
        UIKit.Clear(numRoot);
        for (int k = 0; k < n; k++)
        {
            int row = k / 5, col = k % 5;
            int cnt = Mathf.Min(5, n - row * 5);
            float x = (col - (cnt - 1) / 2f) * .62f;
            Toys.P(PrimitiveType.Sphere, numRoot, V3(x, .3f + row * .6f, 0), S(.5f), Color.HSVToRGB((k * .11f) % 1f, .6f, 1f));
        }
    }

    void ShowTreasure()
    {
        BackButton();
        var panel = UIKit.Panel(actPanel, V2(.05f, .06f), V2(.95f, .80f), new Color(1, 1, 1, .92f));
        UIKit.Label(panel.transform, "Treasure Box", 64, Ink, V2(0, .90f), V2(1, 1));
        UIKit.Label(panel.transform, "Collect stars to unlock treasures! You have " + stars, 32, Mint, V2(0, .83f), V2(1, .91f));
        var grid = UIKit.Rect("Grid", panel.transform, V2(.03f, .02f), V2(.97f, .82f));
        for (int i = 0; i < LGWData.Badges.Length; i++)
        {
            int need = (i + 1) * 5;
            bool got = stars >= need;
            Vector2 mn, mx;
            UIKit.Cell(i, 2, 5, .06f, out mn, out mx);
            var p = UIKit.Panel(grid, mn, mx, got ? new Color(1f, .85f, .35f) : new Color(.82f, .85f, .85f));
            UIKit.Label(p.transform, LGWData.Badges[i], 36, Ink, V2(0, .45f), V2(1, 1));
            UIKit.Label(p.transform, got ? "Unlocked!" : "Needs " + need + " stars", 28, Ink, V2(0, 0), V2(1, .5f));
        }
    }

    void ShowGate()
    {
        BackButton();
        int a = Random.Range(3, 10), b = Random.Range(3, 10);
        gateAnswer = a + b;
        var panel = UIKit.Panel(actPanel, V2(.06f, .25f), V2(.94f, .72f), new Color(1, 1, 1, .95f));
        var q = UIKit.Label(panel.transform, "Grown-ups only!\n" + a + " + " + b + " = ?", 56, Ink, V2(0, .60f), V2(1, 1));
        var opts = new List<int> { gateAnswer, gateAnswer + Random.Range(1, 4), gateAnswer - Random.Range(1, 3) };
        for (int i = opts.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); int t = opts[i]; opts[i] = opts[j]; opts[j] = t; }
        for (int i = 0; i < 3; i++)
        {
            int v = opts[i];
            Vector2 mn, mx;
            UIKit.Cell(i, 3, 1, .1f, out mn, out mx);
            mn.y = .1f; mx.y = .5f;
            UIKit.Btn(panel.transform, v.ToString(), 64, Mint, Color.white, mn, mx, () =>
            {
                if (v == gateAnswer) SetMode(Mode.Parent); else q.text = "Try again!";
            });
        }
    }

    void ShowParent()
    {
        BackButton();
        var panel = UIKit.Panel(actPanel, V2(.06f, .12f), V2(.94f, .80f), new Color(1, 1, 1, .95f));
        UIKit.Label(panel.transform, "Parent Corner", 60, Ink, V2(0, .86f), V2(1, 1));
        UIKit.Label(panel.transform,
            "Stars collected: " + stars +
            "\n\nVoice: English first, Sinhala second.\nSinhala voice depends on the phone's installed Text-to-Speech voice " +
            "(Settings > Text-to-speech output).\n\nProgress is saved on this device.",
            34, Ink, V2(.06f, .30f), V2(.94f, .86f), TextAnchor.UpperLeft);
        Button btn = null;
        btn = UIKit.Btn(panel.transform, "Reset progress", 44, new Color(.9f, .4f, .4f), Color.white, V2(.15f, .06f), V2(.85f, .22f), () =>
        {
            if (!confirmReset)
            {
                confirmReset = true;
                btn.GetComponentInChildren<Text>().text = "Tap again to confirm";
            }
            else
            {
                stars = 0; wordIndex = 0;
                PlayerPrefs.SetInt("lgw_stars", 0);
                RefreshStars();
                SetMode(Mode.Parent);
            }
        });
    }

    // ---------------------------------------------------------------- Update
    void Update()
    {
        float dt = Time.deltaTime;
        if (cloudRing != null) cloudRing.Rotate(0, 2f * dt, 0);
        if (mascot != null)
        {
            mascot.localPosition = V3(0, .08f * Mathf.Sin(Time.time * 2.2f), 0);
            mascot.Rotate(0, 18f * dt, 0);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (mode != Mode.Home) SetMode(Mode.Home); else Application.Quit();
        }
        HandleInput(dt);
        UpdateCamera(dt);
    }

    bool OverUI()
    {
        if (EventSystem.current == null) return false;
        if (Input.touchCount > 0) return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return EventSystem.current.IsPointerOverGameObject();
    }

    void HandleInput(float dt)
    {
        if (mode != Mode.Home) return;
        if (Input.GetMouseButtonDown(0))
        {
            downPos = Input.mousePosition; lastX = downPos.x; downTime = Time.time;
            dragging = !OverUI(); moved = false;
        }
        if (dragging && Input.GetMouseButton(0))
        {
            float x = Input.mousePosition.x;
            orbit -= (x - lastX) * .006f;
            lastX = x;
            if (((Vector2)Input.mousePosition - downPos).magnitude > 25f) moved = true;
        }
        if (dragging && Input.GetMouseButtonUp(0))
        {
            dragging = false;
            if (!moved) Tap(Input.mousePosition);
        }
        if (!dragging) orbit += .08f * dt;
    }

    void Tap(Vector2 pos)
    {
        var ray = cam.ScreenPointToRay(pos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            var tag = hit.collider.GetComponentInParent<StationTag>();
            if (tag != null) Open(tag.id);
        }
    }

    void UpdateCamera(float dt)
    {
        Vector3 pos, look;
        if (UsesStage(mode))
        {
            pos = StagePos + V3(0, 1.7f, -5.4f);
            look = StagePos + V3(0, -.2f, 0);
        }
        else
        {
            pos = V3(Mathf.Sin(orbit) * 14f, 7.5f, Mathf.Cos(orbit) * 14f);
            look = V3(0, .6f, 0);
        }
        var rot = Quaternion.LookRotation(look - pos, Vector3.up);
        if (camSnap)
        {
            cam.transform.SetPositionAndRotation(pos, rot);
            camSnap = false;
        }
        else
        {
            float k = 1f - Mathf.Exp(-6f * dt);
            cam.transform.position = Vector3.Lerp(cam.transform.position, pos, k);
            cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, rot, k);
        }
    }

    void LateUpdate()
    {
        if (mode != Mode.Home) return;
        foreach (var s in stations)
        {
            Vector3 sp = cam.WorldToScreenPoint(s.t.position + Vector3.up * 3.3f);
            if (sp.z < 0) { s.label.gameObject.SetActive(false); continue; }
            s.label.gameObject.SetActive(true);
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, sp, null, out lp);
            s.label.anchoredPosition = lp;
        }
    }
}
