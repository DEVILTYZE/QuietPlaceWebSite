using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Microsoft.AspNetCore.Html;

namespace QuietPlaceWebProject.Helpers
{
    public static class TextHelper
    {
        public static char[] Symbols = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g' };
        
        public static string RemoveTags(string text)
        {
            var tags = new[] {"[b]", "[/b]", "[i]", "[/i]", "[ul]", "[/ul]", "[ol]", "[/ol]", "[lt]", "[/lt]",
                "[spoiler]", "[/spoiler]"};

            text = tags.Aggregate(text, (current, t) 
                => current.Replace(t, string.Empty));

            return text;
        }

        public static HtmlString BuildMediaFiles(string file)
        {
            var tagString = GetTypeFile(file) switch
            {
                "image" => "img",
                "audio" => "audio",
                "video" => "video",
                _ => "NULL"
            };

            string tag;
            var tagBuilder = new TagBuilder(tagString);
            tagBuilder.Attributes.Add(new KeyValuePair<string, string>("src", "/files/" + file));
            tagBuilder.Attributes.Add(new KeyValuePair<string, string>("style", "max-height: 400px; max-width: 250px;"));
            
            if (string.Compare("img", tagString) == 0)
            {
                tagBuilder.AddCssClass("img-fluid");
                tag = tagBuilder.ToString(TagRenderMode.SelfClosing);
            }
            else
                tag = tagBuilder.ToString();
            
            return new HtmlString(tag);
        }
        
        public static HtmlString BuildText(string text)
        {
            // Ответы (пример: ">>1").
            var answerIdsStrings = GetStringsWithRegex(text, 
                @">>\d+", RegexOptions.None);
            var answerIdsStringsOriginalPoster = GetStringsWithRegex(text, 
                @">>\d+ \(OP\)", RegexOptions.None).ToList();

            foreach (var str in answerIdsStrings)
            {
                if (!answerIdsStringsOriginalPoster.Contains(str + " (OP)"))
                    answerIdsStringsOriginalPoster.Add(str);
            }
            
            text = GetTextWithTag(text, answerIdsStringsOriginalPoster, "a",
                new KeyValuePair<string, string>("style", "color: #ff6600"), false, true);
            
            // Гринтекст (пример: ">текст").
            var greenStrings = GetStringsWithRegex(text, @"^>.+", 
                RegexOptions.Multiline);
            text = GetTextWithTag(text, greenStrings, "span",
                new KeyValuePair<string, string>("style", "color: #008000"), false);
            
            // Жирный (пример: "[b]жирный[/b]").
            var boldStrings = GetTagStrings(text, "[b]", "[/b]");
            text = GetTextWithTag(text, boldStrings, "b", new KeyValuePair<string, string>("", ""));

            // Курсив (пример: "[i]курсив[/i]").
            var italicStrings = GetTagStrings(text, "[i]", "[/i]");
            text = GetTextWithTag(text, italicStrings, "i", new KeyValuePair<string, string>("", ""));
            
            // Подчёркнутый (пример: "[ul]подчёркнутый[/ul]").
            var underlineStrings = GetTagStrings(text, "[ul]", "[/ul]");
            text = GetTextWithTag(text, underlineStrings, "span",
                new KeyValuePair<string, string>("style", "text-decoration: underline"));
            
            // Надчёркнутый (пример: "[ol]надчёркнутый[/ol]").
            var overlineStrings = GetTagStrings(text, "[ol]", "[/ol]");
            text = GetTextWithTag(text, overlineStrings, "span",
                new KeyValuePair<string, string>("style", "text-decoration: overline"));
            
            // Зачёркнутый (пример: "[lt]зачёркнутый[/lt]").
            var lineThroughStrings = GetTagStrings(text, "[lt]", "[/lt]");
            text = GetTextWithTag(text, lineThroughStrings, "span",
                new KeyValuePair<string, string>("style", "text-decoration: line-through"));
            
            // Спойлер (пример: "[spoiler]спойлер[/spoiler]").
            var spoilerStrings = GetTagStrings(text, "[spoiler]", "[/spoiler]");
            text = GetTextWithTag(text, spoilerStrings, "span",
                new KeyValuePair<string, string>("onmouseenter", "changeClass(this, 'spoiler')"), 
                true, false, true);
            
            return new HtmlString(text);
        }

        private static string GetTextWithTag(string text, IEnumerable<string> list, string tag,
            KeyValuePair<string, string> attribute, bool isDoubleTag = true, bool isHref = false, bool isSpoiler = false)
        {
            foreach (var item in list)
            {
                var tagBuilder = new TagBuilder(tag);
                
                if (!string.IsNullOrEmpty(attribute.Key))
                    tagBuilder.Attributes.Add(attribute);
                
                tagBuilder.SetInnerText(isDoubleTag ? GetValueOfTagString(item) : item);
                
                if (isHref)
                    tagBuilder.Attributes.Add(new KeyValuePair<string, string>("href", 
                        $"#post {item.Substring(2, item.Length - 2)}"));
                
                if (isSpoiler)
                {
                    tagBuilder.AddCssClass("spoiler");
                    tagBuilder.Attributes.Add(new KeyValuePair<string, string>("onmouseleave", 
                        "changeClass(this, 'spoiler')"));
                }

                text = text.Replace(item, tagBuilder.ToString(TagRenderMode.Normal));
                text = text.Replace("&gt;", ">").Replace("&lt;", "<").Replace(
                    "&quot;", "\"");
            }

            return text;
        }

        private static string GetValueOfTagString(string tagString)
        {
            var startIndex = tagString.IndexOf(']') + 1;
            var length = tagString.LastIndexOf('[') - startIndex;

            return tagString.Substring(startIndex, length);
        }

        private static IEnumerable<string> GetStringsWithRegex(string text, string regexPattern, 
            RegexOptions option = RegexOptions.Singleline)
        {
            var result = new List<string>();
            var regex = new Regex(regexPattern, option);
            var matches = regex.Matches(text);

            foreach (Match match in matches)
                result.Add(match.Value);
            
            return result;
        }

        private static IEnumerable<string> GetTagStrings(string text, string tagStart, string tagEnd)
        {
            var result = new List<string>();
            var indexOfEnd = 0;

            while (true)
            {
                var indexOfStart = text.IndexOf(tagStart, indexOfEnd, StringComparison.Ordinal);

                if (indexOfStart == -1)
                    break;

                indexOfEnd = text.IndexOf(tagEnd, indexOfStart, StringComparison.Ordinal);

                if (indexOfEnd == -1)
                    break;
                
                result.Add(text.Substring(indexOfStart, indexOfEnd - indexOfStart + tagEnd.Length));
            }

            return result;
        }

        private static string GetTypeFile(string url)
        {
            var extension = Path.GetExtension(url);

            if (string.Compare(extension, ".jpg") == 0 || string.Compare(extension, ".jpeg") == 0 ||
                string.Compare(extension, ".png") == 0 || string.Compare(extension, ".bmp") == 0)
                return "image";

            if (string.Compare(extension, ".mp3") == 0)
                return "audio";

            if (string.Compare(extension, ".mp4") == 0 || string.Compare(extension, ".webm") == 0)
                return "video";

            return "NULL";
        }
    }
}