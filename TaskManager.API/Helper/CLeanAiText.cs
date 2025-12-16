
namespace TaskManager.Helper
{
    public class CleanAi
    {
        public static string CleanAiText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            return text
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Replace("**", "")
                .Replace("###", "")
                .Replace("###", "")
                .Replace("##", "")
                .Replace("#", "")
                .Replace("-", "")
                .Replace("*", "")
                .Trim();
        }

    }
}
