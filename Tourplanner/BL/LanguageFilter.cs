using System;
using System.Collections.Generic;
using System.Linq;

public class LanguageFilter
{
    private HashSet<string> offensiveWords;

    public LanguageFilter()
    {
        offensiveWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Add your list of offensive words here
            "badword1",
            "badword2",
            "badword3"
        };
    }

    public bool ContainsOffensiveWords(string comment)
    {
        var words = comment.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            if (offensiveWords.Contains(word))
            {
                return true;
            }
        }
        return false;
    }

    public string FilterOffensiveWords(string comment)
    {
        var words = comment.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            if (offensiveWords.Contains(words[i]))
            {
                words[i] = new string('*', words[i].Length);
            }
        }
        return string.Join(' ', words);
    }
}
