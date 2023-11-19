namespace HangmanH1
{
    public class Word
    {
        public string Secret { get; set; }
        public List<string> Hints { get; set; } = new();
        public int Life { get; set; } = 10;
        public int HintsUsed { get; set; } = 0;
        public int HintsLine { get; set; } = 10;
        public string LettersUsed { get; set; } = "";

        public Word()
        {
            List<WordApi> wordDetail = GetRandomWord();
            Secret = wordDetail[0].word.ToUpper();
            foreach (var def in wordDetail[0].meanings[0].definitions)
                Hints.Add(def.definition);
        }

        static List<WordApi> GetRandomWord()
        {
            string[]? words = Get<string[]?>("https://random-word-form.repl.co/random/noun?count=10");
            if (words == null)
            {
                Console.WriteLine("No words found online. Using Hardcoded wordlist.");
                words = new string[] { "umbrella", "tube", "spaghetti", "tablespoon", "xylophone", "video", };
            }

            Random rnd = new();
            List<WordApi>? wordDetails = null;
            while (wordDetails == null)
            {
                string word = words[rnd.Next(words.Length)];
                wordDetails = Get<List<WordApi>>("https://api.dictionaryapi.dev/api/v2/entries/en/" + word);
            }
            return wordDetails;
        }

        static T? Get<T>(string url)
        {
            using HttpClient httpClient = new();
            try
            {
                string result = httpClient.GetStringAsync(url).Result;
                return System.Text.Json.JsonSerializer.Deserialize<T>(result);
            }
            catch (Exception) { return default; }
        }
    }

    public class WordApi
    {
        public string word { get; set; } = "";
        public List<Meaning> meanings { get; set; } = new();
    }

    public class Meaning
    {
        public List<Definition> definitions { get; set; } = new();

    }

    public class Definition
    {
        public string definition { get; set; } = "";
    }
}