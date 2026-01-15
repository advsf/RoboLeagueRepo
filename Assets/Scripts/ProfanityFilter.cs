using UnityEngine;
using System;
using System.Text.RegularExpressions;
using System.Linq;

public class ProfanityFilter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextAsset textAssetBlockList;
    private Regex combinedRegex;

    private void Start()
    {
        string[] words = textAssetBlockList.text
                .Split(new[] { ",", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim())
                .ToArray();

        string pattern = @"(?i)\b(" + string.Join("|", words.Select(Regex.Escape)) + @")\b";
        combinedRegex = new Regex(pattern, RegexOptions.Compiled);
    }

    public string CheckForProfanity(string text)
    {
        if (string.IsNullOrEmpty(text) || combinedRegex == null)
            return text;

        return combinedRegex.Replace(text, match =>
        {
            return new string('*', match.Value.Length);
        });
    }
}
