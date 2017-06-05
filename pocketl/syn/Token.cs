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
        KeywordType,
        KeywordStruct,
        KeywordMut,
        KeywordIf,
        KeywordElse,
        KeywordWhile,
        KeywordLoop,
        KeywordBreak,
        KeywordContinue,
        KeywordReturn,
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
        Slash,
        Dollar,
        ExclamationMark,
        QuestionMark
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
                case TokenKind.KeywordType: return "`type`";
                case TokenKind.KeywordStruct: return "`struct`";
                case TokenKind.KeywordMut: return "`mut`";
                case TokenKind.KeywordIf: return "`if`";
                case TokenKind.KeywordElse: return "`else`";
                case TokenKind.KeywordWhile: return "`while`";
                case TokenKind.KeywordLoop: return "`loop`";
                case TokenKind.KeywordBreak: return "`break`";
                case TokenKind.KeywordContinue: return "`continue`";
                case TokenKind.KeywordReturn: return "`return`";
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
                case TokenKind.Dollar: return "`$`";
                case TokenKind.ExclamationMark: return "`!`";
                case TokenKind.QuestionMark: return "`?`";
                default: return "unknown";
            }
        }
    }
}
