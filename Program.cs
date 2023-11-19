using System.Diagnostics;

namespace HangmanH1
{
    internal class Program
    {
        static Word word;

        static void Main(string[] args)
        {
            Write("Enter your name (max 10 char): ", 10, 5);
            string? name = Console.ReadLine();
            if (name != null && name.Length > 10) name = name[..10];
            if (name == null || name == "") name = "Unknown";

            do Setup(name);
            while (Console.ReadKey(true).Key != ConsoleKey.N);
        }

        static void Setup(string name)
        {
            Console.CursorVisible = false;
            Console.Clear();
            _ = ShowHighScoresAsync();
            word = new Word();
            DrawHangman();

            Stopwatch sw = Stopwatch.StartNew();
            while (Game()) ;
            sw.Stop();

            int score = (word.Secret.Length + word.Life) * 1000000 / (int)sw.ElapsedMilliseconds;

            if (word.Life > 0)
            {
                HighScore hs = new() { Name = name, Life = word.Life, Score = score, Word = word.Secret, Time = sw.ElapsedMilliseconds };
                Task.Run(() => hs.PostHighScore());
                Write($"You guessed the word in {sw.Elapsed}\n with {word.Life} lives left." +
                    $" Your score was {score}.\n Try again? (Y/N)", 1, ++word.HintsLine, ConsoleColor.Green);
            }
            else Write($"You lost. The word was {word.Secret}.\n Try again? (Y/N)", 1, ++word.HintsLine, ConsoleColor.Red);
        }

        static bool Game()
        {
            if (ShowWord() || word.Life <= 0) return false;
            char letter = GuessLetter();
            if (!CheckLetter(letter))
            {
                word.Life--;
                DrawHangman();
            }
            return true;
        }

        static char GuessLetter()
        {
            while (true)
            {
                Write($"Guess a letter (Hints: {word.Hints.Count - word.HintsUsed})", 10, 4, ConsoleColor.Yellow);
                var character = char.ToUpper(Console.ReadKey(true).KeyChar);
                if (character == ' ' && word.Hints.Count > word.HintsUsed && word.Life > 1) ShowDefinition();
                else if (char.IsLetter(character) && !word.LettersUsed.Contains(character))
                {
                    word.LettersUsed += character;
                    ShowGuessedLetters();
                    return character;
                }
            }
        }

        static bool CheckLetter(char letter)
        {
            if (!word.Secret.Contains(letter))
            {
                Write($"{letter} is not in the word".PadRight(20), 10, 6, ConsoleColor.Red);
                return false;
            }
            Write($"The word contains {letter}".PadRight(20), 10, 6, ConsoleColor.Green);
            return true;
        }

        static void DrawHangman()
        {
            string gallows = @"
╔═══╤═
║
║ 
║
║ 
║ 
║ ";
            string[] man = {"    │", "\n    O", "\n\n   ─", "\n\n    ╫", "\n\n     ─",
            "\n\n\n   ╔", "\n\n\n    ╩", "\n\n\n     ╗", "\n\n\n\n   ╜", "\n\n\n\n     ╙"};

            for (int i = man.Length - 1; i >= word.Life; i--)
                Write(man[i], 0, 3, ConsoleColor.Red);
            Write(gallows, 0, 1, ConsoleColor.DarkBlue);
        }

        static bool ShowWord()
        {
            bool isWon = true;
            for (int i = 0; i < word.Secret.Length; i++)
            {
                if (word.LettersUsed.Contains(word.Secret[i]) || word.Secret[i] == '-')
                    Write(word.Secret[i], 10 + i, 2);
                else
                {
                    Write('*', 10 + i, 2);
                    isWon = false;
                }
            }
            return isWon;
        }

        static void ShowDefinition()
        {
            word.Life--;
            DrawHangman();
            int counter = 0;
            Write(" ", 0, word.HintsLine++, ConsoleColor.Magenta);
            foreach (string t in word.Hints[word.HintsUsed++].Split(' '))
            {
                int similarity = Math.Abs((word.Secret.Length - LevenshteinDistance(word.Secret, t.ToUpper())) * 100 / word.Secret.Length);
                if (t.ToUpper().Contains(word.Secret.Replace("TION", "").Replace("ING", "").Replace("ANCE", "")) || similarity >= 70)
                    Console.Write(new string('_', t.Length) + " ");
                else Console.Write(t + " ");
                counter += t.Length;
                if (counter > 25)
                {
                    Console.WriteLine(" ");
                    word.HintsLine++;
                    counter = 0;
                }
            }
        }

        static void ShowGuessedLetters()
        {
            char[] characters = word.LettersUsed.ToArray();
            Array.Sort(characters);
            word.LettersUsed = new string(characters);
            Write($"Used: {word.LettersUsed}", 10, 8, ConsoleColor.Cyan);
        }

        static async Task ShowHighScoresAsync()
        {
            List<HighScore> hs = await HighScore.GetHighScoresAsync();
            Write("Score Name      Word", 44, 1);
            for (int i = 0; i < hs.Count; i++)
            {
                Write(hs[i].Score, 44, i + 2, SelectColor(i));
                Write(hs[i].Name, 50, i + 2, SelectColor(i));
                Write(hs[i].Word, 60, i + 2, SelectColor(i));
            }
        }

        static void Write(object? t, int x, int y, ConsoleColor c = ConsoleColor.White)
        {
            Console.ForegroundColor = c;
            Console.SetCursorPosition(x, y);
            if (t != null) Console.Write(t.ToString());
        }

        static ConsoleColor SelectColor(int pos)
        {
            switch (pos)
            {
                case < 3: return Console.ForegroundColor = ConsoleColor.Yellow;
                case < 10: return Console.ForegroundColor = ConsoleColor.Gray;
                default: return Console.ForegroundColor = ConsoleColor.DarkRed;
            }
        }

        static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
    }
}