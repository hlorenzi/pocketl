namespace pocketl.syn
{
    public class Token
    {
        public diagn.Span span;
        public TokenKind kind;
        public string excerpt;
    }


    public enum TokenKind
    {
        Error,
        Whitespace,
        Comment,
        Identifier,
        Number,
        KeywordFn,
        KeywordStruct,
        BraceOpen,
        BraceClose,
        ParenOpen,
        ParenClose,
        Comma,
        Dot,
        Colon,
        Semicolon,
        Arrow,
        Equal,
        Plus,
        Minus,
        Asterisk,
        Slash
    }


    public static class TokenKindNamer
    {
        public static string Name(this TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind.Error: return "error";
                case TokenKind.Whitespace: return "whitespace";
                case TokenKind.Comment: return "comment";
                case TokenKind.Identifier: return "identifier";
                case TokenKind.Number: return "number";
                case TokenKind.KeywordFn: return "`fn`";
                case TokenKind.KeywordStruct: return "`struct`";
                case TokenKind.BraceOpen: return "`{`";
                case TokenKind.BraceClose: return "`}`";
                case TokenKind.ParenOpen: return "`(`";
                case TokenKind.ParenClose: return "`)`";
                case TokenKind.Comma: return "`,`";
                case TokenKind.Dot: return "`.`";
                case TokenKind.Colon: return "`:`";
                case TokenKind.Semicolon: return "`;`";
                case TokenKind.Arrow: return "`->`";
                case TokenKind.Equal: return "`=`";
                case TokenKind.Plus: return "`+`";
                case TokenKind.Minus: return "`-`";
                case TokenKind.Asterisk: return "`*`";
                case TokenKind.Slash: return "`/`";
                default: return "unknown";
            }
        }
    }
}
