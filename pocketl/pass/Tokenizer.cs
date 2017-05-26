using System;
using System.Collections.Generic;


namespace pocketl.pass
{
    public class Tokenizer
    {
        public static void Tokenize(Context ctx, diagn.Reporter reporter, H<mod.Unit> hUnit)
        {
            var unit = ctx[hUnit];
            var src = unit.ReadSource(ctx);
            var index = 0;

            unit.tokens = new List<H<syn.Token>>();

            while (index < src.Length)
            {
                var match =
                    TryMatchFixed(src, index) ??
                    TryMatchVarying(syn.TokenKind.Whitespace, src, index, IsWhitespace,       IsWhitespace) ??
                    TryMatchVarying(syn.TokenKind.Identifier, src, index, IsIdentifierPrefix, IsIdentifier) ??
                    TryMatchVarying(syn.TokenKind.Number,     src, index, IsNumberPrefix,     IsNumber) ??
                    new Match(src[index].ToString(), syn.TokenKind.Error);

                var span = new diagn.Span(hUnit, index, index + match.excerpt.Length);

                // Signal errors.
                if (match.kind == syn.TokenKind.Error)
                    reporter.Error("unexpected character", new diagn.Caret(span));

                var hToken = ctx.tokens.Add(new syn.Token
                {
                    span = span,
                    kind = match.kind,
                    excerpt = match.excerpt
                });

                unit.tokens.Add(hToken);

                index += match.excerpt.Length;
            }
        }


        class Match
        {
            public string excerpt;
            public syn.TokenKind kind;


            public Match(string excerpt, syn.TokenKind kind)
            {
                this.excerpt = excerpt;
                this.kind = kind;
            }
        }


        static Match TryMatchVarying(
            syn.TokenKind kind,
            string src, int index,
            System.Func<char, bool> filterPrefix,
            System.Func<char, bool> filterRest)
        {
            if (!filterPrefix(src[index]))
                return null;

            var length = 1;
            while (index + length < src.Length && filterRest(src[index + length]))
                length++;

            return new Match(src.Substring(index, length), kind);
        }


        static Match TryMatchFixed(string src, int index)
        {
            var models = new Match[]
            {
                new Match("{", syn.TokenKind.BraceOpen),
                new Match("}", syn.TokenKind.BraceClose),
                new Match("(", syn.TokenKind.ParenOpen),
                new Match(")", syn.TokenKind.ParenClose),
                new Match(",", syn.TokenKind.Comma),
                new Match(".", syn.TokenKind.Dot),
                new Match(":", syn.TokenKind.Colon),
                new Match(";", syn.TokenKind.Semicolon),
                new Match("->", syn.TokenKind.Arrow),
                new Match("=", syn.TokenKind.Equal),
                new Match("+", syn.TokenKind.Plus),
                new Match("-", syn.TokenKind.Minus),
                new Match("*", syn.TokenKind.Asterisk),
                new Match("/", syn.TokenKind.Slash),
                new Match("fn", syn.TokenKind.KeywordFn),
                new Match("struct", syn.TokenKind.KeywordStruct)
            };

            // Check whether one of the models match.
            return Array.Find(models, (model) =>
            {
                for (int i = 0; i < model.excerpt.Length; i++)
                {
                    if (index + i >= src.Length ||
                        src[index + i] != model.excerpt[i])
                        return false;
                }

                return true;
            });
        }


        static bool IsWhitespace(char c)
        {
            return c == ' ' || c == '\t' || c == '\r' || c == '\n';
        }


        static bool IsIdentifierPrefix(char c)
        {
            return (c >= 'A' && c <= 'Z') ||
                (c >= 'a' && c <= 'z') ||
                c == '_';
        }


        static bool IsIdentifier(char c)
        {
            return IsIdentifierPrefix(c) ||
                (c >= '0' && c <= '9');
        }


        static bool IsNumberPrefix(char c)
        {
            return (c >= '0' && c <= '9');
        }


        static bool IsNumber(char c)
        {
            return IsNumberPrefix(c) ||
                (c >= 'A' && c <= 'Z') ||
                (c >= 'a' && c <= 'z') ||
                c == '_' || c == '.';
        }
    }
}
