using System.Data;
using System.Data.SqlClient;

namespace HangmanH1
{
    internal class HighScore
    {
        const string connectionString = @"Data Source = 62.171.142.200; Initial Catalog = HangmanHighScore; User ID=sa; Password=MyOtherPassword";

        public string? Name { get; set; }
        public string? Word { get; set; }
        public long Time { get; set; }
        public int Life { get; set; }
        public int Score { get; set; }

        public async Task PostHighScore()
        {
            using (SqlConnection conn = new(connectionString))
            {
                SqlCommand cmd = new("INSERT INTO [HighScore] " +
                "([name], word, score, [time], life) VALUES " +
                "(@name, @word, @score, @time, @life)", conn);
                cmd.Parameters.Add("@name", SqlDbType.NVarChar, 50).Value = Name;
                cmd.Parameters.Add("@word", SqlDbType.NVarChar, 50).Value = Word;
                cmd.Parameters.Add("@score", SqlDbType.Int).Value = Score;
                cmd.Parameters.Add("@time", SqlDbType.Int).Value = Time;
                cmd.Parameters.Add("@life", SqlDbType.Int).Value = Life;
                conn.Open();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task<List<HighScore>> GetHighScoresAsync()
        {
            List<HighScore> highScores = new List<HighScore>();

            using (SqlConnection conn = new(connectionString))
            {
                SqlCommand cmd = new("SELECT TOP(20) * FROM HighScore ORDER BY Score DESC", conn);
                conn.Open();
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        highScores.Add(new HighScore()
                        {
                            Name = reader["Name"].ToString(),
                            Word = reader["Word"].ToString(),
                            Score = (int)reader["Score"],
                            Time = (int)reader["Time"],
                            Life = (int)reader["Life"]
                        });
                    }
                }
            }
            return highScores;
        }
    }
}
