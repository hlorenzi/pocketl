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
                index = 0,
                insideCondition = new Stack<bool>()
            };

            parser.insideCondition.Push(false);
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
        private Stack<bool> insideCondition;


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
            while (this.NextNthIs(0, TokenKind.Error) ||
                this.NextNthIs(0, TokenKind.Whitespace) ||
                this.NextNthIs(0, TokenKind.Comment))
            {
                this.index++;
            }
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


        private bool NextNthIs(int n, TokenKind kind)
        {
            if (this.index + n >= this.tokens.Count)
                return false;

            return this.ctx[this.tokens[this.index + n]].kind == kind;
        }


        private H<Token> Expect(TokenKind kind)
        {
            if (this.NextNthIs(0, kind))
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


        private H<Token>? ExpectMaybe(TokenKind kind)
        {
            if (this.NextNthIs(0, kind))
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


        private List<H<Node>> ParseList(TokenKind separator, TokenKind terminator, System.Func<H<Node>> parseItemFunc)
        {
            var list = new List<H<Node>>();

            while (!this.IsOver && this.Current.kind != terminator)
            {
                list.Add(parseItemFunc());
                if (this.Current.kind != terminator)
                    this.Expect(separator);
            }

            this.Expect(terminator);
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
            node.token = this.Expect(TokenKind.Identifier);
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseNumber()
        {
            var node = new Node.Number();
            node.token = this.Expect(TokenKind.Number);
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
            return this.ParseExprOperator(0);
        }


        private H<Node> ParseFunctionDef()
        {
            var node = new Node.FunctionDef();
            this.Expect(TokenKind.KeywordFn);
            node.name = this.ParseIdentifier();
            this.Expect(TokenKind.ParenOpen);
            node.parameters = this.ParseList(TokenKind.Comma, TokenKind.ParenClose, this.ParseFunctionDefParam);

            if (this.ExpectMaybe(TokenKind.Arrow).HasValue)
                node.returnType = this.ParseType();

            node.body = this.ParseBlock();
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseFunctionDefParam()
        {
            var node = new Node.FunctionDefParameter();
            node.name = this.ParseIdentifier();
            this.Expect(TokenKind.Colon);
            node.type = this.ParseType();
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseStructureDef()
        {
            var node = new Node.StructureDef();
            this.Expect(TokenKind.KeywordType);
            node.name = this.ParseIdentifier();
            this.Expect(TokenKind.BraceOpen);
            node.fields = this.ParseList(TokenKind.Comma, TokenKind.BraceClose, this.ParseStructureDefField);
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseStructureDefField()
        {
            var node = new Node.StructureDefField();
            node.name = this.ParseIdentifier();
            this.Expect(TokenKind.Colon);
            node.type = this.ParseType();
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseBlock()
        {
            var node = new Node.Block();
            this.Expect(TokenKind.BraceOpen);
            this.insideCondition.Push(false);
            node.exprs = this.ParseList(TokenKind.Semicolon, TokenKind.BraceClose, this.ParseExpr);
            this.insideCondition.Pop();
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseParenthesizedOrLiteralTuple()
        {
            this.Expect(TokenKind.ParenOpen);

            if (this.ExpectMaybe(TokenKind.ParenClose).HasValue)
                return this.ctx.nodes.Add(new Node.LiteralTuple());

            this.insideCondition.Push(false);

            var exprs = new List<H<Node>>();
            exprs.Add(this.ParseExpr());

            if (this.ExpectMaybe(TokenKind.Comma).HasValue)
            {
                exprs.AddRange(this.ParseList(TokenKind.Comma, TokenKind.ParenClose, this.ParseExpr));

                var node = new Node.LiteralTuple();
                node.elems = exprs;

                this.insideCondition.Pop();
                return this.ctx.nodes.Add(node);
            }
            else
            {
                var node = new Node.Parenthesized();
                node.inner = exprs[0];
                this.Expect(TokenKind.ParenClose);

                this.insideCondition.Pop();
                return this.ctx.nodes.Add(node);
            }
        }


        private H<Node> ParseIdentifierOrLiteralStructure()
        {
            var ident = this.ParseIdentifier();

            if (!this.insideCondition.Peek() && this.ExpectMaybe(TokenKind.BraceOpen).HasValue)
            {
                var fields = this.ParseList(TokenKind.Comma, TokenKind.BraceClose, this.ParseLiteralStructureField);
                var node = new Node.LiteralStructure();
                node.type = ident;
                node.fields = fields;
                return this.ctx.nodes.Add(node);
            }
            else
                return ident;
        }


        private H<Node> ParseLiteralStructureField()
        {
            var node = new Node.LiteralStructureField();
            node.name = this.ParseIdentifier();
            this.Expect(TokenKind.Equal);
            node.value = this.ParseExpr();
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseIf()
        {
            var node = new Node.If();
            this.Expect(TokenKind.KeywordIf);
            this.insideCondition.Push(true);
            node.condition = this.ParseExpr();
            this.insideCondition.Pop();
            node.trueBlock = this.ParseBlock();

            if (this.ExpectMaybe(TokenKind.KeywordElse).HasValue)
                node.falseBlock = this.ParseBlock();

            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseWhile()
        {
            var node = new Node.While();
            this.Expect(TokenKind.KeywordWhile);
            this.insideCondition.Push(true);
            node.condition = this.ParseExpr();
            this.insideCondition.Pop();
            node.block = this.ParseBlock();
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseLoop()
        {
            var node = new Node.Loop();
            this.Expect(TokenKind.KeywordLoop);
            node.block = this.ParseBlock();
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseBreak()
        {
            var node = new Node.Break();
            this.Expect(TokenKind.KeywordBreak);
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseContinue()
        {
            var node = new Node.Continue();
            this.Expect(TokenKind.KeywordContinue);
            return this.ctx.nodes.Add(node);
        }


        private H<Node> ParseReturn()
        {
            var node = new Node.Return();
            this.Expect(TokenKind.KeywordReturn);

            if (this.Current.kind != TokenKind.BraceClose &&
                this.Current.kind != TokenKind.ParenClose &&
                this.Current.kind != TokenKind.Comma &&
                this.Current.kind != TokenKind.Semicolon)
                node.expr = this.ParseExpr();

            return this.ctx.nodes.Add(node);
        }


        private enum Associativity
        {
            BinaryLeft,
            BinaryRight,
            UnaryPrefix,
            UnarySuffix
        }


        private class Operator
        {
            public TokenKind token1;
            public TokenKind? token2;
            public Associativity associativity;
            public Node.UnaryOperator unaryOp;
            public Node.BinaryOperator binaryOp;


            public Operator(TokenKind token1, TokenKind? token2, Associativity assoc, Node.UnaryOperator op)
            {
                this.token1 = token1;
                this.token2 = token2;
                this.associativity = assoc;
                this.unaryOp = op;
            }


            public Operator(TokenKind token1, TokenKind? token2, Associativity assoc, Node.BinaryOperator op)
            {
                this.token1 = token1;
                this.token2 = token2;
                this.associativity = assoc;
                this.binaryOp = op;
            }
        }


        private static Operator[][] operators = new Operator[][]
        {
            new Operator[]
            {
                new Operator(TokenKind.Equal, null, Associativity.BinaryRight, Node.BinaryOperator.Assign)
            },
            new Operator[]
            {
                new Operator(TokenKind.Plus, null, Associativity.BinaryLeft, Node.BinaryOperator.Add),
                new Operator(TokenKind.Minus, null, Associativity.BinaryLeft, Node.BinaryOperator.Subtract)
            },
            new Operator[]
            {
                new Operator(TokenKind.Asterisk, null, Associativity.BinaryLeft, Node.BinaryOperator.Multiply),
                new Operator(TokenKind.Slash, null, Associativity.BinaryLeft, Node.BinaryOperator.Divide)
            },
            new Operator[]
            {
                new Operator(TokenKind.Dollar, null, Associativity.UnaryPrefix, Node.UnaryOperator.RefCount),
                new Operator(TokenKind.Dollar, TokenKind.KeywordMut, Associativity.UnaryPrefix, Node.UnaryOperator.RefCountMut),
                new Operator(TokenKind.Minus, null, Associativity.UnaryPrefix, Node.UnaryOperator.Negate),
                new Operator(TokenKind.ExclamationMark, null, Associativity.UnaryPrefix, Node.UnaryOperator.Not),
                new Operator(TokenKind.ExclamationMark, null, Associativity.UnarySuffix, Node.UnaryOperator.Unwrap)
            },
            new Operator[]
            {
                new Operator(TokenKind.Dot, null, Associativity.BinaryLeft, Node.BinaryOperator.FieldAccess)
            }
        };


        private Operator MatchOperator(Operator[] ops, Associativity assoc)
        {
            foreach (var op in ops)
            {
                if (op.associativity != assoc)
                    continue;

                if (!this.NextNthIs(0, op.token1))
                    continue;

                if (!op.token2.HasValue)
                {
                    this.Advance();
                    return op;
                }

                if (this.NextNthIs(1, op.token2.Value))
                {
                    this.Advance();
                    this.Advance();
                    return op;
                }
            }

            return null;
        }


        private H<Node> ParseExprOperator(int level)
        {
            if (level >= operators.Length)
                return this.ParseExprLeaf();

            // Match unary prefix operators.
            var unaryPrefixMatch = MatchOperator(operators[level], Associativity.UnaryPrefix);
            if (unaryPrefixMatch != null)
            {
                var node = new Node.UnaryOperation();
                node.op = unaryPrefixMatch.unaryOp;
                node.expr = this.ParseExprOperator(level);
                return this.ctx.nodes.Add(node);
            }

            // Match binary operators.
            var lhs = this.ParseExprOperator(level + 1);

            while (true)
            {
                var binaryMatch =
                    MatchOperator(operators[level], Associativity.BinaryLeft) ??
                    MatchOperator(operators[level], Associativity.BinaryRight);

                if (binaryMatch == null)
                    break;

                var isRightAssoc = (binaryMatch.associativity == Associativity.BinaryRight);

                var rhs = this.ParseExprOperator(isRightAssoc ? 0 : level + 1);

                var node = new Node.BinaryOperation();
                node.op = binaryMatch.binaryOp;
                node.lhs = lhs;
                node.rhs = rhs;

                lhs = this.ctx.nodes.Add(node);

                if (isRightAssoc)
                    return lhs;
            }

            // Match unary suffix operators.
            var expr = lhs;

            while (true)
            {
                var unarySuffixMatch = MatchOperator(operators[level], Associativity.UnarySuffix);
                if (unarySuffixMatch == null)
                    break;

                var node = new Node.UnaryOperation();
                node.op = unarySuffixMatch.unaryOp;
                node.expr = expr;
                expr = this.ctx.nodes.Add(node);
            }

            return expr;
        }


        private H<Node> ParseExprLeaf()
        {
            switch (this.Current.kind)
            {
                case TokenKind.KeywordFn:
                    return this.ParseFunctionDef();

                case TokenKind.KeywordType:
                    return this.ParseStructureDef();

                case TokenKind.BraceOpen:
                    return this.ParseBlock();

                case TokenKind.ParenOpen:
                    return this.ParseParenthesizedOrLiteralTuple();

                case TokenKind.KeywordIf:
                    return this.ParseIf();

                case TokenKind.KeywordWhile:
                    return this.ParseWhile();

                case TokenKind.KeywordLoop:
                    return this.ParseLoop();

                case TokenKind.KeywordBreak:
                    return this.ParseBreak();

                case TokenKind.KeywordContinue:
                    return this.ParseContinue();

                case TokenKind.KeywordReturn:
                    return this.ParseReturn();

                case TokenKind.Identifier:
                    return this.ParseIdentifierOrLiteralStructure();

                case TokenKind.Number:
                    return this.ParseNumber();

                default:
                    this.ErrorBeforeCurrent("expected expression");
                    throw new ParseException();
            }
        }
    }
}
