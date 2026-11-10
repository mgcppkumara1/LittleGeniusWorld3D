using UnityEngine;

// Thin wrapper over android.speech.tts.TextToSpeech. Does nothing (just logs) in the Editor.
public class AndroidTts
{
#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject tts;
    AndroidJavaObject activity;
    volatile bool ready;

    class InitListener : AndroidJavaProxy
    {
        readonly System.Action<int> cb;
        public InitListener(System.Action<int> cb) : base("android.speech.tts.TextToSpeech$OnInitListener") { this.cb = cb; }
        public void onInit(int status) { cb(status); }
    }

    public AndroidTts()
    {
        try
        {
            using (var up = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                activity = up.GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new InitListener(s => ready = (s == 0)));
            }));
        }
        catch (System.Exception e) { Debug.LogWarning("TTS init failed: " + e.Message); }
    }

    public void Speak(string text, string lang, bool flush)
    {
        if (activity == null) return;
        activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            try
            {
                if (!ready || tts == null) return;
                using (var loc = lang == "si"
                    ? new AndroidJavaObject("java.util.Locale", "si", "LK")
                    : new AndroidJavaObject("java.util.Locale", "en", "US"))
                {
                    int r = tts.Call<int>("setLanguage", loc);
                    if (r < 0) // -1 missing data, -2 not supported
                    {
                        if (lang == "si") return; // no Sinhala voice installed: stay silent instead of mispronouncing
                    }
                }
                using (var map = new AndroidJavaObject("java.util.HashMap"))
                    tts.Call<int>("speak", text, flush ? 0 : 1, map);
            }
            catch (System.Exception e) { Debug.LogWarning("TTS speak failed: " + e.Message); }
        }));
    }

    public void Shutdown()
    {
        try { if (tts != null) { tts.Call("stop"); tts.Call("shutdown"); } } catch { }
    }
#else
    public AndroidTts() { }
    public void Speak(string text, string lang, bool flush) { Debug.Log("[TTS " + lang + "] " + text); }
    public void Shutdown() { }
#endif
}
