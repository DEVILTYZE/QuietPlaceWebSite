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
            var answerIdsStrings = GetAnswerIdsStrings(text);
            var greenStrings = GetGreenTextStrings(text);

            foreach (var match in answerIdsStrings)
            {
                var tagABuilder = new TagBuilder("a");
                tagABuilder.Attributes.Add(new KeyValuePair<string, string>("style", "color: #ff6600"));
                tagABuilder.SetInnerText(match.Value);
                tagABuilder.Attributes.Add(new KeyValuePair<string, string>("href", 
                    $"#post {match.Value.Substring(2, match.Value.Length - 2)}"));

                text = text.Replace(match.Value, tagABuilder.ToString(TagRenderMode.Normal));
            }

            foreach (var match in greenStrings)
            {
                var tagSpanBuilder = new TagBuilder("span");
                tagSpanBuilder.Attributes.Add(new KeyValuePair<string, string>("style", "color: #008000"));
                tagSpanBuilder.SetInnerText(match.Value);
                
                text = text.Replace(match.Value, tagSpanBuilder.ToString(TagRenderMode.Normal));
            }

            return new HtmlString(text);
        }
        
        private static IEnumerable<Match> GetAnswerIdsStrings(string text)
        {
            var regex = new Regex(@">>\d+");
            var matches = regex.Matches(text);

            return matches.ToList();
        }

        private static IEnumerable<Match> GetGreenTextStrings(string text)
        {
            var regex = new Regex(@"^>.+", RegexOptions.Multiline);
            var matches = regex.Matches(text);

            return matches.ToList();
        }
    }
}