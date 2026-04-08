using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace vue_spotify_app.Classes.SortNameHelpers
{
    public static class RegexHelpers
    {
        /// <summary>
        /// An array of common English articles often moved to the end of a name for sorting purposes, such as "a", "an", and "the".
        /// </summary>
        /// TODO: may expand to xover non-English articles in the future
        public static readonly string[] Articles = { "the", "a", "an" };
        public static string RemovePunctuationFromText(string text)
        {
            // Use a regular expression to remove all punctuation characters
            return Regex.Replace(text, @"[^\w]", "");
        }

        /// <summary>
        /// Moves leading articles from the beginning of a string to the end, if present.
        /// </summary>
        /// <param name="text">The string to be inputted.</param>
        /// <returns>The imputted string, if no article is present, or a modifed string with articles moved to the end (e.g., "The 1975" becomes becomes "1975The").</returns>
        public static string MoveLeadingArticles(string text, bool removeArticle = false)
        {
            // Loops through the list of articles
            foreach (var article in Articles)
            {
                // Checks if the text starts with the article followed by a space (case-insensitive)
                if (text.StartsWith(article + " ", StringComparison.OrdinalIgnoreCase))
                {
                    // Moves the article to the end of the text
                    if(removeArticle) return text.Substring(article.Length+1);
                    return text.Substring(article.Length + 1) + article;
                }
            }
            return text;
        }

        /// <summary>
        /// Generates a modified version of an inputted name for sorting purposes, removing punctuation, converting to lowercase, and moving leading articles to the end.
        /// </summary>
        /// <param name="name">The string to be converted.</param>
        /// <returns>A sortable name in lowercase with removed punctuation and spaces, and articles moved to the end.</returns>
        public static string GenerateSortName(string name, bool removeArticle = false)
        {
            string sortName = name.Trim(); // Normalize the name by trimming whitespace
            // Step 1: Convert to lowercase for consistent sorting
            sortName = sortName.ToLowerInvariant();
            // Step 2: Remove leading articles
            sortName = MoveLeadingArticles(sortName, removeArticle);
            // Step 3: Remove punctuation
            sortName = RemovePunctuationFromText(sortName);
            return sortName;
        }
    }
}
