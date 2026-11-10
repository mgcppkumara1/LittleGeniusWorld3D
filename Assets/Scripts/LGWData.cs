public struct Vocab
{
    public string en, si, kind;
    public Vocab(string e, string s, string k) { en = e; si = s; kind = k; }
}

public static class LGWData
{
    public static readonly Vocab[] Words =
    {
        new Vocab("CAT", "පූසා", "cat"),
        new Vocab("DOG", "බල්ලා", "dog"),
        new Vocab("ELEPHANT", "අලියා", "elephant"),
        new Vocab("RABBIT", "හාවා", "rabbit"),
        new Vocab("APPLE", "ඇපල්", "apple"),
        new Vocab("BANANA", "කෙසෙල්", "banana"),
        new Vocab("MANGO", "අඹ", "mango"),
        new Vocab("BIRD", "කුරුල්ලා", "bird"),
        new Vocab("LION", "සිංහයා", "lion"),
        new Vocab("GRAPES", "මිදි", "grapes"),
    };

    public static readonly string[] LetterWords =
    {
        "Apple","Ball","Cat","Dog","Elephant","Fish","Grapes","Hat","Ice cream","Juice","Kite","Lion","Mango",
        "Nest","Orange","Pig","Queen","Rabbit","Sun","Tree","Umbrella","Van","Water","Xylophone","Yo-yo","Zebra"
    };

    public static readonly string[] NumEn = { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };
    public static readonly string[] NumSi = { "එක", "දෙක", "තුන", "හතර", "පහ", "හය", "හත", "අට", "නවය", "දහය" };

    public static readonly string[] Badges =
    {
        "Seashell", "Baby Crab", "Starfish", "Parrot", "Coconut", "Rainbow", "Golden Key", "Treasure Map", "Pearl", "Crown"
    };
}
