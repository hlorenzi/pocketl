using System.Collections.Generic;
using pocketl.syn;


namespace pocketl.pass
{
    public class Parser
    {
        public static void Parse(Context ctx, diagn.Reporter reporter, H<mod.Unit> hUnit)
        {
            var unit = ctx[hUnit];

            var parser = new Parser
            {
                ctx = ctx,
                reporter = reporter,
                tokens = unit.tokens,
                index = 0
            };

            parser.SkipIgnorable();

            try
            {
                unit.ast = parser.ParseTopLevel();
            }
            catch (ParseException)
            {
                unit.ast = ctx.nodes.Add(new Node.TopLevel());
            }
        }


        private class ParseException : System.Exception
        {

        }


        private Context ctx;
        private diagn.Reporter reporter;
        private List<H<Token>> tokens;
        private int index;


        private Parser()
        {

        }


        private bool IsOver
        {
            get { return this.index >= this.tokens.Count; }
        }


        private void Advance()
        {
            this.index++;
            this.SkipIgnorable();
        }


        private void SkipIgnorable()
        {
            while (!this.IsOver &&
                (this.Current.kind == TokenKind.Error ||
                this.Current.kind == TokenKind.Whitespace ||
                this.Current.kind == TokenKind.Comment))
                this.index++;
        }


        private Token Current
        {
            get { return this.ctx[this.tokens[this.index]]; }
        }


        private diagn.Span SpanBeforeCurrent
        {
            get
            {
                if (this.index >= this.tokens.Count)
                    return this.ctx[this.tokens[this.tokens.Count - 1]].span.JustAfter;
                else
                    return this.ctx[this.tokens[this.index]].span.JustBefore;
            }
        }


        private void ErrorBeforeCurrent(string descr)
        {
            this.reporter.Error(descr, new diagn.Caret(SpanBeforeCurrent));
        }


        private H<Token> Match(TokenKind kind)
        {
            if (!this.IsOver && this.Current.kind == kind)
            {
                var hToken = this.tokens[this.index];
                this.Advance();
                return hToken;
            }
            else
            {
                this.ErrorBeforeCurrent("expected " + kind.Name());
                throw new ParseException();
            }
        }


        private H<Token>? MatchMaybe(TokenKind kind)
        {
            if (!this.IsOver && this.Current.kind == kind)
            {
                var hToken = this.tokens[this.index];
                this.Advance();
                return hToken;
            }
            else
            {
                return null;
            }
        }


        private List<H<Node>> MatchList(TokenKind separator, TokenKind terminator, System.Func<H<Node>> parseItemFunc)
        {
            var list = new List<H<Node>>();

            while (!this.IsOver && this.Current.kind != terminator)
            {
                list.Add(parseItemFunc());
                if (this.Current.kind != terminator)
                    this.Match(separator);
            }

            this.Match(terminator);
            return list;
        }


        private H<Node> ParseTopLevel()
        {
            var node = new Node.TopLevel();

            while (!this.IsOver)
                node.defs.Add(this.ParseExpr());

            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseIdentifier()
        {
            var node = new Node.Identifier();
            node.token = this.Match(TokenKind.Identifier);
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseType()
        {
            switch (this.Current.kind)
            {
                case TokenKind.Identifier:
                    return this.ParseTypeStructure();
                default:
                    this.ErrorBeforeCurrent("expected type");
                    throw new ParseException();
            }
        }


        private H<Node> ParseTypeStructure()
        {
            var node = new Node.TypeStructure();
            node.name = this.ParseIdentifier();
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseExpr()
        {
            switch (this.Current.kind)
            {
                case TokenKind.KeywordFn:
                    return this.ParseFunctionDef();
                case TokenKind.BraceOpen:
                    return this.ParseExprBlock();
                default:
                    this.ErrorBeforeCurrent("expected expression");
                    throw new ParseException();
            }
        }


        private H<Node> ParseFunctionDef()
        {
            var node = new Node.FunctionDef();
            this.Match(TokenKind.KeywordFn);
            node.name = this.ParseIdentifier();
            this.Match(TokenKind.ParenOpen);
            node.parameters = this.MatchList(TokenKind.Comma, TokenKind.ParenClose, this.ParseFunctionDefParam);

            if (this.MatchMaybe(TokenKind.Arrow).HasValue)
                node.returnType = this.ParseType();

            node.body = this.ParseExprBlock();
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseFunctionDefParam()
        {
            var node = new Node.FunctionDefParameter();
            node.name = this.ParseIdentifier();
            this.Match(TokenKind.Colon);
            node.type = this.ParseType();
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseExprBlock()
        {
            var node = new Node.ExprBlock();
            this.Match(TokenKind.BraceOpen);
            node.exprs = this.MatchList(TokenKind.Semicolon, TokenKind.BraceClose, this.ParseExpr);
            return this.ctx.nodes.Add(node);
        }
    }
}
