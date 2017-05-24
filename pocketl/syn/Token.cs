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
}
