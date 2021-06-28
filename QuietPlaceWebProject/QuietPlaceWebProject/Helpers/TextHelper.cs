using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Microsoft.AspNetCore.Html;

namespace QuietPlaceWebProject.Helpers
{
    public static class TextHelper
    {
        public static string RemoveTags(string text)
        {
            var tags = new[] {"[b]", "[/b]", "[i]", "[/i]", "[ul]", "[/ul]", "[ol]", "[/ol]", "[lt]", "[/lt]",
                "[spoiler]", "[/spoiler]"};

            text = tags.Aggregate(text, (current, t) 
                => current.Replace(t, string.Empty));

            return text;
        }
        
        public static HtmlString BuildText(string text)
        {
            // Ответы (пример: ">>1").
            var answerIdsStrings = GetStringsWithRegex(text, @">>\d+", RegexOptions.None);
            text = GetTextWithTag(text, answerIdsStrings, "a",
                new KeyValuePair<string, string>("style", "color: #ff6600"), false, true);
            
            // Гринтекст (пример: ">текст").
            var greenStrings = GetStringsWithRegex(text, @"^>.+", 
                RegexOptions.Multiline);
            text = GetTextWithTag(text, greenStrings, "span",
                new KeyValuePair<string, string>("style", "color: #008000"), false);
            
            // Жирный (пример: "[b]жирный[/b]").
            var boldStrings = GetStringsWithRegex(text, @"\[b\].*\[/b\]");
            text = GetTextWithTag(text, boldStrings, "b", new KeyValuePair<string, string>("", ""));

            // Курсив (пример: "[i]курсив[/i]").
            var italicStrings = GetStringsWithRegex(text, @"\[i\].*\[/i\]");
            text = GetTextWithTag(text, italicStrings, "i", new KeyValuePair<string, string>("", ""));
            
            // Подчёркнутый (пример: "[ul]подчёркнутый[/ul]").
            var underlineStrings = GetStringsWithRegex(text, @"\[ul\].*\[/ul\]");
            text = GetTextWithTag(text, underlineStrings, "div",
                new KeyValuePair<string, string>("style", "text-decoration: underline"));
            
            // Надчёркнутый (пример: "[ol]надчёркнутый[/ol]").
            var overlineStrings = GetStringsWithRegex(text, @"\[ol\].*\[/ol\]");
            text = GetTextWithTag(text, overlineStrings, "div",
                new KeyValuePair<string, string>("style", "text-decoration: overline"));
            
            // Зачёркнутый (пример: "[lt]зачёркнутый[/lt]").
            var lineThroughStrings = GetStringsWithRegex(text, @"\[lt\].*\[/lt\]");
            text = GetTextWithTag(text, lineThroughStrings, "div",
                new KeyValuePair<string, string>("style", "text-decoration: line-through"));
            
            // Спойлер (пример: "[spoiler]спойлер[/spoiler]").
            var spoilerStrings = GetStringsWithRegex(text, @"\[spoiler\].*\[/spoiler\]");
            text = GetTextWithTag(text, spoilerStrings, "div",
                new KeyValuePair<string, string>("onmouseenter", "unspoiler(this)"), 
                true, false, true);
            
            return new HtmlString(text);
        }

        private static string GetTextWithTag(string text, IEnumerable<Match> matches, string tag,
            KeyValuePair<string, string> attribute, bool isDoubleTag = true, bool isHref = false, bool isSpoiler = false)
        {
            foreach (var match in matches)
            {
                var tagBuilder = new TagBuilder(tag);
                
                if (!string.IsNullOrEmpty(attribute.Key))
                    tagBuilder.Attributes.Add(attribute);
                
                tagBuilder.SetInnerText(isDoubleTag ? GetValue(match.Value) : match.Value);
                
                if (isHref)
                    tagBuilder.Attributes.Add(new KeyValuePair<string, string>("href", 
                        $"#post {match.Value.Substring(2, match.Value.Length - 2)}"));
                
                if (isSpoiler)
                {
                    tagBuilder.AddCssClass("spoiler");
                    tagBuilder.Attributes.Add(new KeyValuePair<string, string>("onmouseout", "spoiler(this)"));
                }

                text = text.Replace(match.Value, tagBuilder.ToString(TagRenderMode.Normal));
                text = text.Replace("&gt;", ">").Replace("&lt;", "<").Replace(
                    "&quot;", "\"");
            }

            return text;
        }

        private static string GetValue(string matchValue)
        {
            var startIndex = matchValue.IndexOf(']') + 1;
            var length = matchValue.LastIndexOf('[') - startIndex;

            return matchValue.Substring(startIndex, length);
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