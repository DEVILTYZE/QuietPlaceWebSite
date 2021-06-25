using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Microsoft.AspNetCore.Html;

namespace QuietPlaceWebProject.Helpers
{
    public static class TextHelper
    {
        public static HtmlString BuildText(string text)
        {
            var answerIdsStrings = GetStringsWithRegex(text, @">>\d+");
            var greenStrings = GetStringsWithRegex(text, @"^>.+", 
                RegexOptions.Multiline);
            var underlineStrings = GetStringsWithRegex(text, "<u>.</u>");
            var overlineStrings = GetStringsWithRegex(text, "<o>.</o>");
            var lineThroughStrings = GetStringsWithRegex(text, "<lt>.</lt>");
            var spoilerStrings = GetStringsWithRegex(text, "<spoiler>.</spoiler>");

            text = GetTextWithTag(text, underlineStrings, "div",
                new KeyValuePair<string, string>("style", $"text-decoration: underline"));
            
            text = GetTextWithTag(text, overlineStrings, "div",
                new KeyValuePair<string, string>("style", $"text-decoration: overline"));
            
            text = GetTextWithTag(text, lineThroughStrings, "div",
                new KeyValuePair<string, string>("style", $"text-decoration: line-though"));
            
            text = GetTextWithTag(text, spoilerStrings, "div",
                new KeyValuePair<string, string>("onmouseenter", "unspoiler(this)"));
            
            text = GetTextWithTag(text, answerIdsStrings, "a",
                new KeyValuePair<string, string>("style", "color: #ff6600"), true);

            text = GetTextWithTag(text, greenStrings, "span",
                new KeyValuePair<string, string>("style", "color: #008000"));

            return new HtmlString(text);
        }

        private static string GetTextWithTag(string text, IEnumerable<Match> matches, string tag,
            KeyValuePair<string, string> attribute, bool isHref = false, bool isSpoiler = false)
        {
            foreach (var match in matches)
            {
                var tagBuilder = new TagBuilder(tag);
                tagBuilder.Attributes.Add(attribute);
                tagBuilder.SetInnerText(match.Value);
                
                if (isHref)
                    tagBuilder.Attributes.Add(new KeyValuePair<string, string>("href", 
                        $"#post {match.Value.Substring(2, match.Value.Length - 2)}"));
                
                if (isSpoiler)
                {
                    tagBuilder.AddCssClass("spoiler");
                    tagBuilder.Attributes.Add(new KeyValuePair<string, string>("onmouseout", "spoiler(this)"));
                }

                text = text.Replace(match.Value, tagBuilder.ToString(TagRenderMode.Normal));
            }

            return text;
        }

        private static IEnumerable<Match> GetStringsWithRegex(string text, string regexPattern, 
            RegexOptions option = RegexOptions.Singleline)
        {
            var regex = new Regex(regexPattern, option);
            var matches = regex.Matches(text);

            return matches.ToList();
        }
    }
}