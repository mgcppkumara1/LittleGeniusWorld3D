using UnityEngine;

// Procedural 3D "toys" built from Unity primitives (placeholder art - swap for real models later).
public static class Toys
{
    static Material baseMat;

    static Vector3 V(float x, float y, float z) { return new Vector3(x, y, z); }
    static Vector3 S(float s) { return new Vector3(s, s, s); }

    public static Material Mat(Color c)
    {
        if (baseMat == null)
        {
            // Borrow the engine's default material so the shader is always included in builds (works in Built-in and URP).
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseMat = tmp.GetComponent<Renderer>().sharedMaterial;
            Object.Destroy(tmp);
        }
        var m = new Material(baseMat);
        m.color = c;
        return m;
    }

    public static GameObject P(PrimitiveType t, Transform parent, Vector3 pos, Vector3 scale, Color c, Vector3 rot = default(Vector3), bool collider = false)
    {
        var g = GameObject.CreatePrimitive(t);
        if (!collider) { var col = g.GetComponent<Collider>(); if (col != null) Object.Destroy(col); }
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        g.transform.localEulerAngles = rot;
        g.GetComponent<Renderer>().sharedMaterial = Mat(c);
        return g;
    }

    static void Eyes(Transform r, float y, float z, float sp, float s)
    {
        foreach (float sx in new[] { -1f, 1f })
        {
            P(PrimitiveType.Sphere, r, V(sx * sp, y, z), S(s), new Color(.08f, .08f, .1f));
            P(PrimitiveType.Sphere, r, V(sx * sp + .02f, y + .03f, z + .04f), S(s * .35f), Color.white);
        }
    }

    static void Legs(Transform r, Color c, float x, float zf, float zb, float h, float w)
    {
        foreach (float sx in new[] { -1f, 1f })
            foreach (float z in new[] { zf, zb })
                P(PrimitiveType.Cylinder, r, V(sx * x, h / 2f, z), V(w, h / 2f, w), c);
    }

    public static GameObject Build(string kind)
    {
        var root = new GameObject("Toy_" + kind);
        var r = root.transform;
        var black = new Color(.1f, .08f, .08f);
        var pink = new Color(1f, .62f, .72f);
        var leaf = new Color(.3f, .7f, .25f);
        var brown = new Color(.45f, .28f, .12f);
        float scale = 1f;

        switch (kind)
        {
            case "cat":
            {
                var c = new Color(.98f, .67f, .3f);
                P(PrimitiveType.Sphere, r, V(0, .6f, 0), V(1f, .85f, 1.3f), c);
                P(PrimitiveType.Sphere, r, V(0, 1.1f, .7f), V(.85f, .75f, .75f), c);
                P(PrimitiveType.Cube, r, V(-.3f, 1.5f, .65f), V(.25f, .25f, .1f), c, V(0, 0, 45));
                P(PrimitiveType.Cube, r, V(.3f, 1.5f, .65f), V(.25f, .25f, .1f), c, V(0, 0, 45));
                Eyes(r, 1.15f, 1.05f, .18f, .12f);
                P(PrimitiveType.Sphere, r, V(0, 1.02f, 1.07f), S(.1f), pink);
                P(PrimitiveType.Capsule, r, V(0, .9f, -1.05f), V(.13f, .5f, .13f), c, V(-55, 0, 0));
                Legs(r, c, .3f, .5f, -.5f, .3f, .2f);
                break;
            }
            case "dog":
            {
                var c = new Color(.72f, .47f, .27f); var d = new Color(.45f, .28f, .15f); var tan = new Color(.9f, .75f, .55f);
                P(PrimitiveType.Sphere, r, V(0, .65f, 0), V(1f, .9f, 1.4f), c);
                P(PrimitiveType.Sphere, r, V(0, 1.15f, .75f), V(.8f, .75f, .8f), c);
                P(PrimitiveType.Sphere, r, V(0, 1.02f, 1.2f), V(.4f, .3f, .35f), tan);
                P(PrimitiveType.Sphere, r, V(0, 1.1f, 1.4f), S(.13f), black);
                P(PrimitiveType.Sphere, r, V(-.42f, 1.2f, .7f), V(.15f, .45f, .28f), d, V(0, 0, 15));
                P(PrimitiveType.Sphere, r, V(.42f, 1.2f, .7f), V(.15f, .45f, .28f), d, V(0, 0, -15));
                Eyes(r, 1.28f, 1.08f, .2f, .12f);
                P(PrimitiveType.Capsule, r, V(0, 1.0f, -1.0f), V(.12f, .45f, .12f), c, V(-45, 0, 0));
                Legs(r, c, .32f, .55f, -.55f, .35f, .22f);
                break;
            }
            case "elephant":
            {
                var g = new Color(.62f, .67f, .75f); var gd = new Color(.5f, .55f, .64f);
                P(PrimitiveType.Sphere, r, V(0, .85f, 0), V(1.3f, 1.1f, 1.6f), g);
                P(PrimitiveType.Sphere, r, V(0, 1.25f, .9f), V(.95f, .95f, .9f), g);
                P(PrimitiveType.Cylinder, r, V(-.62f, 1.35f, .7f), V(.75f, .04f, .75f), gd, V(0, 0, 90));
                P(PrimitiveType.Cylinder, r, V(.62f, 1.35f, .7f), V(.75f, .04f, .75f), gd, V(0, 0, 90));
                P(PrimitiveType.Capsule, r, V(0, .75f, 1.4f), V(.24f, .5f, .24f), g, V(-12, 0, 0));
                P(PrimitiveType.Capsule, r, V(-.3f, .9f, 1.45f), V(.06f, .2f, .06f), Color.white, V(70, 0, 0));
                P(PrimitiveType.Capsule, r, V(.3f, .9f, 1.45f), V(.06f, .2f, .06f), Color.white, V(70, 0, 0));
                Eyes(r, 1.4f, 1.25f, .28f, .11f);
                P(PrimitiveType.Capsule, r, V(0, .7f, -1.0f), V(.07f, .3f, .07f), gd, V(-20, 0, 0));
                Legs(r, gd, .45f, .6f, -.6f, .45f, .38f);
                scale = .9f;
                break;
            }
            case "rabbit":
            {
                var w = new Color(.97f, .97f, .99f);
                P(PrimitiveType.Sphere, r, V(0, .55f, 0), V(.95f, .9f, 1.15f), w);
                P(PrimitiveType.Sphere, r, V(0, 1.1f, .5f), S(.7f), w);
                P(PrimitiveType.Capsule, r, V(-.17f, 1.75f, .42f), V(.15f, .5f, .1f), w, V(0, 0, 6));
                P(PrimitiveType.Capsule, r, V(.17f, 1.75f, .42f), V(.15f, .5f, .1f), w, V(0, 0, -6));
                P(PrimitiveType.Capsule, r, V(-.17f, 1.75f, .47f), V(.08f, .38f, .05f), pink, V(0, 0, 6));
                P(PrimitiveType.Capsule, r, V(.17f, 1.75f, .47f), V(.08f, .38f, .05f), pink, V(0, 0, -6));
                Eyes(r, 1.17f, .82f, .17f, .1f);
                P(PrimitiveType.Sphere, r, V(0, 1.07f, .84f), S(.08f), pink);
                P(PrimitiveType.Sphere, r, V(0, .55f, -.62f), S(.28f), w);
                P(PrimitiveType.Sphere, r, V(-.25f, .12f, .45f), V(.2f, .12f, .35f), w);
                P(PrimitiveType.Sphere, r, V(.25f, .12f, .45f), V(.2f, .12f, .35f), w);
                P(PrimitiveType.Sphere, r, V(-.45f, .35f, -.2f), V(.3f, .4f, .5f), w);
                P(PrimitiveType.Sphere, r, V(.45f, .35f, -.2f), V(.3f, .4f, .5f), w);
                scale = .8f;
                break;
            }
            case "lion":
            {
                var g = new Color(.96f, .76f, .32f); var m = new Color(.72f, .42f, .14f);
                P(PrimitiveType.Sphere, r, V(0, .7f, 0), V(1f, .9f, 1.5f), g);
                P(PrimitiveType.Sphere, r, V(0, 1.15f, .65f), V(1.35f, 1.35f, .9f), m);
                P(PrimitiveType.Sphere, r, V(0, 1.1f, .95f), V(.85f, .8f, .6f), g);
                P(PrimitiveType.Sphere, r, V(-.5f, 1.65f, .7f), S(.22f), m);
                P(PrimitiveType.Sphere, r, V(.5f, 1.65f, .7f), S(.22f), m);
                P(PrimitiveType.Sphere, r, V(0, 1.0f, 1.12f), V(.35f, .25f, .25f), new Color(1f, .93f, .78f));
                P(PrimitiveType.Sphere, r, V(0, 1.08f, 1.24f), V(.14f, .1f, .1f), black);
                Eyes(r, 1.2f, 1.2f, .2f, .1f);
                P(PrimitiveType.Capsule, r, V(0, .85f, -1.1f), V(.08f, .5f, .08f), g, V(-60, 0, 0));
                P(PrimitiveType.Sphere, r, V(0, 1.1f, -1.53f), S(.22f), m);
                Legs(r, g, .35f, .6f, -.6f, .4f, .25f);
                scale = .9f;
                break;
            }
            case "bird":
            {
                var b = new Color(.3f, .6f, .95f); var o = new Color(1f, .6f, .1f);
                P(PrimitiveType.Sphere, r, V(0, .8f, 0), V(.9f, .9f, 1.1f), b);
                P(PrimitiveType.Sphere, r, V(0, 1.4f, .35f), S(.55f), b);
                P(PrimitiveType.Cube, r, V(0, 1.35f, .7f), V(.15f, .1f, .25f), new Color(1f, .85f, .2f));
                Eyes(r, 1.5f, .6f, .16f, .09f);
                P(PrimitiveType.Sphere, r, V(-.5f, .85f, 0), V(.12f, .5f, .6f), new Color(.2f, .45f, .85f), V(0, 0, 15));
                P(PrimitiveType.Sphere, r, V(.5f, .85f, 0), V(.12f, .5f, .6f), new Color(.2f, .45f, .85f), V(0, 0, -15));
                P(PrimitiveType.Cube, r, V(0, .8f, -.75f), V(.3f, .05f, .5f), new Color(.2f, .45f, .85f), V(-20, 0, 0));
                P(PrimitiveType.Cylinder, r, V(-.2f, .2f, .1f), V(.06f, .2f, .06f), o);
                P(PrimitiveType.Cylinder, r, V(.2f, .2f, .1f), V(.06f, .2f, .06f), o);
                break;
            }
            case "apple":
            {
                P(PrimitiveType.Sphere, r, V(0, .6f, 0), V(1.1f, 1f, 1.1f), new Color(.9f, .15f, .15f));
                P(PrimitiveType.Cylinder, r, V(0, 1.15f, 0), V(.06f, .15f, .06f), brown);
                P(PrimitiveType.Sphere, r, V(.2f, 1.2f, 0), V(.3f, .05f, .15f), leaf, V(0, 0, 20));
                scale = 1.2f;
                break;
            }
            case "banana":
            {
                var y = new Color(1f, .87f, .2f);
                for (int i = 0; i < 4; i++)
                {
                    float k = i - 1.5f;
                    float x = k * .38f;
                    float yy = .35f + .28f * (1f - (k / 1.5f) * (k / 1.5f));
                    P(PrimitiveType.Capsule, r, V(x, yy, 0), V(.22f, .28f, .22f), y, V(0, 0, 90f - k * 16f));
                }
                P(PrimitiveType.Sphere, r, V(-.85f, .38f, 0), S(.12f), brown);
                P(PrimitiveType.Sphere, r, V(.85f, .38f, 0), S(.12f), brown);
                scale = 1.2f;
                break;
            }
            case "mango":
            {
                P(PrimitiveType.Sphere, r, V(0, .65f, 0), V(1f, 1.2f, .85f), new Color(1f, .65f, .2f), V(0, 0, 20));
                P(PrimitiveType.Sphere, r, V(.1f, 1.3f, 0), V(.35f, .06f, .18f), leaf, V(0, 0, 25));
                scale = 1.1f;
                break;
            }
            case "grapes":
            {
                var p = new Color(.55f, .2f, .7f);
                float[] ys = { 1.05f, .78f, .53f };
                int[] ns = { 4, 3, 2 };
                float[] rs = { .3f, .22f, .12f };
                for (int l = 0; l < 3; l++)
                    for (int i = 0; i < ns[l]; i++)
                    {
                        float a = i * Mathf.PI * 2f / ns[l] + l * .5f;
                        P(PrimitiveType.Sphere, r, V(Mathf.Sin(a) * rs[l], ys[l], Mathf.Cos(a) * rs[l]), S(.4f), p);
                    }
                P(PrimitiveType.Sphere, r, V(0, .3f, 0), S(.38f), p);
                P(PrimitiveType.Cylinder, r, V(0, 1.35f, 0), V(.07f, .15f, .07f), brown);
                P(PrimitiveType.Sphere, r, V(.2f, 1.4f, 0), V(.3f, .05f, .16f), leaf, V(0, 0, 20));
                scale = 1.1f;
                break;
            }
        }
        root.transform.localScale = S(scale);
        return root;
    }

    public static GameObject Icon(string id)
    {
        if (id == "learn")
        {
            var g = Build("lion");
            g.transform.localScale = S(.4f);
            return g;
        }
        var root = new GameObject("Icon_" + id);
        var r = root.transform;
        switch (id)
        {
            case "puzzle":
                P(PrimitiveType.Cube, r, V(-.3f, 0, 0), S(.5f), new Color(.95f, .3f, .3f), V(0, 0, 10));
                P(PrimitiveType.Cube, r, V(.3f, .05f, 0), S(.5f), new Color(.3f, .5f, .95f), V(0, 0, -10));
                P(PrimitiveType.Cube, r, V(0, .5f, 0), S(.45f), new Color(1f, .85f, .2f));
                break;
            case "alphabet":
                P(PrimitiveType.Sphere, r, V(-.3f, 0, 0), S(.5f), Color.white);
                P(PrimitiveType.Sphere, r, V(.05f, .12f, 0), S(.65f), Color.white);
                P(PrimitiveType.Sphere, r, V(.4f, 0, 0), S(.5f), Color.white);
                break;
            case "numbers":
                P(PrimitiveType.Sphere, r, V(-.45f, -.1f, 0), S(.3f), new Color(.95f, .4f, .4f));
                P(PrimitiveType.Sphere, r, V(0, 0, 0), S(.42f), new Color(.4f, .8f, .4f));
                P(PrimitiveType.Sphere, r, V(.5f, .1f, 0), S(.55f), new Color(.4f, .55f, .95f));
                break;
            default:
                P(PrimitiveType.Cube, r, V(0, 0, 0), V(.8f, .45f, .55f), new Color(.5f, .3f, .12f));
                P(PrimitiveType.Cube, r, V(0, .02f, 0), V(.2f, .5f, .6f), new Color(1f, .8f, .2f));
                P(PrimitiveType.Sphere, r, V(0, .35f, 0), S(.3f), new Color(1f, .8f, .2f));
                break;
        }
        return root;
    }
}
