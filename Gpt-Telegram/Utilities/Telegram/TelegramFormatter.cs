using Markdig;
using Markdig.Syntax;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Gpt_Telegram.Utilities.Telegram
{
    public class TelegramFormatter : ITelegramFormatter
    {
        private const int TelegramLimit = 4096;

        private readonly MarkdownPipeline _pipeline;

        public TelegramFormatter()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();
        }

        public IEnumerable<string> FormatMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return Enumerable.Empty<string>();
            }

            var html = Markdown.ToHtml(message, _pipeline);

            html = FormatForTelegram(html);

            return SplitMessage(html.Trim());
        }

        private IEnumerable<string> SplitMessage(string text)
        {
            if (text.Length <= TelegramLimit)
            {
                yield return text;
                yield break;
            }

            var paragraphs = Regex.Split(text, @"(\n\n)");

            var buffer = "";
            foreach (var part in paragraphs)
            {
                if (part.Length > TelegramLimit)
                {
                    if (!string.IsNullOrEmpty(buffer))
                    {
                        yield return buffer;
                        buffer = "";
                    }

                    for (int i = 0; i < part.Length; i += TelegramLimit)
                    {
                        yield return part.Substring(i, Math.Min(TelegramLimit, part.Length - i));
                    }
                    continue;
                }

                if (buffer.Length + part.Length > TelegramLimit)
                {
                    yield return buffer;
                    buffer = "";
                }

                buffer += part;
            }

            if (!string.IsNullOrEmpty(buffer))
                yield return buffer;
        }

        private string FormatForTelegram(string html)
        {
            var codeBlocks = ExtractCodeBlocks(ref html);

            html = ConvertHeadings(html);
            html = ConvertLists(html);
            html = ConvertParagraphs(html);
            html = ConvertTextStyles(html);
            html = ConvertInlineCode(html);
            html = ConvertLinks(html);
            html = RemoveUnsupportedTags(html);
            html = FixListLineBreaks(html);
            html = NormalizeNewLines(html);
            html = RestoreCodeBlocks(html, codeBlocks);

            return html.Trim();
        }


        private List<string> ExtractCodeBlocks(ref string html) 
        {
            var codeBlocks = new List<string>();

            html = Regex.Replace(
                html,
                @"<pre><code([^>]*)>(.*?)<\/code><\/pre>",
                m =>
                {
                    var codeOpen = m.Groups[1].Value;      // атрибуты <code ...>
                    var body = m.Groups[2].Value;          // содержимое кода

                    // язык из class="language-xxx"
                    var lang = "";
                    var lm = Regex.Match(codeOpen, @"class=""language-([a-z0-9\+\-#]+)""", RegexOptions.IgnoreCase);
                    if (lm.Success) lang = lm.Groups[1].Value;

                    // Внутри <pre> телеграм не парсит HTML — оставляем как есть
                    var content = body;

                    var idx = codeBlocks.Count;
                    codeBlocks.Add($"<pre>{content}</pre>");
                    return $"[[CODE_BLOCK_{idx}]]";
                },
                RegexOptions.Singleline | RegexOptions.IgnoreCase
            );
            return codeBlocks;
        }
        private string ConvertHeadings(string html) 
        {
            html = Regex.Replace(
                html,
                @"<h[1-6][^>]*>(.*?)<\/h[1-6]>",
                "<b>$1</b>\n",
                RegexOptions.Singleline | RegexOptions.IgnoreCase
            );
            return html;
        }
        private string ConvertLists(string html) 
        {
            int listIndex = 0;

            html = Regex.Replace(html,
                @"<ol[^>]*>(.*?)<\/ol>",
                m =>
                {
                    listIndex = 0;
                    return Regex.Replace(m.Groups[1].Value, @"<li[^>]*>(.*?)<\/li>", mm =>
                    {
                        listIndex++;
                        var content = mm.Groups[1].Value.Trim();
                        return $"{listIndex}. {content}\n";
                    }, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                },
                RegexOptions.Singleline | RegexOptions.IgnoreCase
            );

            html = Regex.Replace(html,
                @"<ul[^>]*>(.*?)<\/ul>",
                m =>
                {
                    return Regex.Replace(m.Groups[1].Value, @"<li[^>]*>(.*?)<\/li>", mm =>
                    {
                        var content = mm.Groups[1].Value.Trim();
                        return $"• {content}\n";
                    }, RegexOptions.Singleline | RegexOptions.IgnoreCase);
                },
                RegexOptions.Singleline | RegexOptions.IgnoreCase
            );
            return html;
        }
        private string ConvertParagraphs(string html) 
        {
            html = Regex.Replace(html, @"<\/?p[^>]*>", "\n", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<\/?blockquote[^>]*>", "\n", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<br\s*\/?>", "\n", RegexOptions.IgnoreCase);
            return html;
        }
        private string ConvertTextStyles(string html) 
        {
            html = Regex.Replace(html, @"<\/?strong>", m => m.Value[1] == '/' ? "</b>" : "<b>", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<\/?em>", m => m.Value[1] == '/' ? "</i>" : "<i>", RegexOptions.IgnoreCase);
            return html;
        }
        private string ConvertInlineCode(string html) 
        {
            html = Regex.Replace(html, @"<code[^>]*>(.*?)<\/code>", "<code>$1</code>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return html;
        }
        private string ConvertLinks(string html) 
        {
            html = Regex.Replace(html, @"<a[^>]*href=""([^""]+)""[^>]*>(.*?)<\/a>", "<a href=\"$1\">$2</a>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            return html;
        }
        private string RemoveUnsupportedTags(string html) 
        {
            html = Regex.Replace(html, @"<\/?(?!b|i|u|s|a|code|pre|blockquote)\w+[^>]*>", "",
                RegexOptions.IgnoreCase);
            return html;
        }
        private string FixListLineBreaks(string html) 
        {
            html = Regex.Replace(html, @"(\d+)\.\s*\n\s*", "$1. ");
            return html;
        }
        private string NormalizeNewLines(string html) 
        {
            html = Regex.Replace(html, @"(\n\s*){3,}", "\n\n");
            return html;
        }
        private string RestoreCodeBlocks(string html, List<string> blocks) 
        {
            // 10) Вернуть код-блоки на место
            html = Regex.Replace(html, @"\[\[CODE_BLOCK_(\d+)\]\]", m =>
            {
                var idx = int.Parse(m.Groups[1].Value);
                return blocks[idx];
            });


            return html;
        }
    }
}
