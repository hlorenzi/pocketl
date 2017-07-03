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
                indexPrevious = 0,
                insideCondition = new Stack<bool>()
            };

            parser.insideCondition.Push(false);
            parser.SkipIgnorable();

            try
            {
                unit.ast = parser.ParseTopLevel();
                unit.ast.AddChildrenSpansRecursively();
            }
            catch (ParseException)
            {
                unit.ast = new Node.TopLevel();
            }
        }


        private class ParseException : System.Exception
        {

        }


        private Context ctx;
        private diagn.Reporter reporter;
        private List<Token> tokens;
        private int index, indexPrevious;
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
            this.indexPrevious = this.index;
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
            get { return this.tokens[this.index]; }
        }


        private Token Previous
        {
            get { return this.tokens[this.indexPrevious]; }
        }


        private diagn.Span SpanBeforeCurrent
        {
            get
            {
                if (this.index >= this.tokens.Count)
                    return this.tokens[this.tokens.Count - 1].span.JustAfter;
                else
                    return this.tokens[this.index].span.JustBefore;
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

            return this.tokens[this.index + n].kind == kind;
        }


        private Token Expect(TokenKind kind)
        {
            if (this.NextNthIs(0, kind))
            {
                var token = this.tokens[this.index];
                this.Advance();
                return token;
            }
            else
            {
                this.ErrorBeforeCurrent("expected " + kind.Name());
                throw new ParseException();
            }
        }


        private Token ExpectMaybe(TokenKind kind)
        {
            if (this.NextNthIs(0, kind))
            {
                var token = this.tokens[this.index];
                this.Advance();
                return token;
            }
            else
            {
                return null;
            }
        }


        private List<Node> ParseList(TokenKind separator, TokenKind terminator, System.Func<Node> parseItemFunc)
        {
            var list = new List<Node>();

            while (!this.IsOver && this.Current.kind != terminator)
            {
                list.Add(parseItemFunc());
                if (this.Current.kind != terminator)
                    this.Expect(separator);
            }

            this.Expect(terminator);
            return list;
        }


        private Node ParseTopLevel()
        {
            var node = new Node.TopLevel();

            while (!this.IsOver)
                node.defs.Add(this.ParseExpr());

            return node;
        }


        private Node ParseIdentifier()
        {
            var node = new Node.Identifier();
            node.token = this.Expect(TokenKind.Identifier);
            node.AddSpan(node.token.span);
            return node;
        }


        private Node ParseNumber()
        {
            var node = new Node.Number();
            node.token = this.Expect(TokenKind.Number);
            node.AddSpan(node.token.span);
            return node;
        }


        private Node ParseType()
        {
            switch (this.Current.kind)
            {
                case TokenKind.Identifier:
                    return this.ParseTypeStructure();

                case TokenKind.Asterisk:
                    return this.ParseTypePointer();

                case TokenKind.Dollar:
                    return this.ParseTypeRefCounted();

                case TokenKind.ParenOpen:
                    return this.ParseTypeTuple();

                case TokenKind.KeywordFn:
                    return this.ParseTypeFunction();

                default:
                    this.ErrorBeforeCurrent("expected type");
                    throw new ParseException();
            }
        }


        private Node ParseTypeStructure()
        {
            var node = new Node.TypeStructure();
            node.name = this.ParseIdentifier();
            return node;
        }


        private Node ParseTypePointer()
        {
            var node = new Node.TypePointer();
            node.AddSpan(this.Expect(TokenKind.Asterisk).span);

            if (this.ExpectMaybe(TokenKind.KeywordMut) != null)
                node.mutable = true;

            node.innerType = this.ParseType();
            return node;
        }


        private Node ParseTypeRefCounted()
        {
            var node = new Node.TypeRefCounted();
            node.AddSpan(this.Expect(TokenKind.Dollar).span);

            if (this.ExpectMaybe(TokenKind.KeywordMut) != null)
                node.mutable = true;

            node.innerType = this.ParseType();
            return node;
        }


        private Node ParseTypeTuple()
        {
            var node = new Node.TypeTuple();
            node.AddSpan(this.Expect(TokenKind.ParenOpen).span);
            node.elements = this.ParseList(TokenKind.Comma, TokenKind.ParenClose, this.ParseType);
            node.AddSpan(this.Previous.span);
            return node;
        }


        private Node ParseTypeFunction()
        {
            var node = new Node.TypeFunction();
            node.AddSpan(this.Expect(TokenKind.KeywordFn).span);
            this.Expect(TokenKind.ParenOpen);
            node.parameters = this.ParseList(TokenKind.Comma, TokenKind.ParenClose, this.ParseType);
            node.AddSpan(this.Previous.span);

            if (this.ExpectMaybe(TokenKind.Arrow) != null)
                node.returnType = this.ParseType();

            return node;
        }


        private Node ParseExpr()
        {
            return this.ParseExprOperator(0);
        }


        private Node ParseFunctionDef()
        {
            var node = new Node.FunctionDef();
            node.AddSpan(this.Expect(TokenKind.KeywordFn).span);
            node.name = this.ParseIdentifier();
            this.Expect(TokenKind.ParenOpen);
            node.parameters = this.ParseList(TokenKind.Comma, TokenKind.ParenClose, this.ParseFunctionDefParam);

            if (this.ExpectMaybe(TokenKind.Arrow) != null)
                node.returnType = this.ParseType();

            node.body = this.ParseBlock();
            return node;
        }


        private Node ParseFunctionDefParam()
        {
            var node = new Node.FunctionDefParameter();
            node.identifier = this.ParseIdentifier();
            this.Expect(TokenKind.Colon);
            node.type = this.ParseType();
            return node;
        }


        private Node ParseStructureDef()
        {
            var node = new Node.StructureDef();
            node.AddSpan(this.Expect(TokenKind.KeywordType).span);
            node.name = this.ParseIdentifier();
            this.Expect(TokenKind.BraceOpen);
            node.fields = this.ParseList(TokenKind.Comma, TokenKind.BraceClose, this.ParseStructureDefField);
            return node;
        }


        private Node ParseStructureDefField()
        {
            var node = new Node.StructureDefField();
            node.identifier = this.ParseIdentifier();
            this.Expect(TokenKind.Colon);
            node.type = this.ParseType();
            return node;
        }


        private Node ParseBlock()
        {
            var node = new Node.Block();
            node.AddSpan(this.Expect(TokenKind.BraceOpen).span);
            this.insideCondition.Push(false);
            node.exprs = this.ParseList(TokenKind.Semicolon, TokenKind.BraceClose, this.ParseExpr);
            this.insideCondition.Pop();
            node.AddSpan(this.Previous.span);
            return node;
        }


        private Node ParseParenthesizedOrLiteralTuple()
        {
            var openParenToken = this.Expect(TokenKind.ParenOpen);

            if (this.ExpectMaybe(TokenKind.ParenClose) != null)
            {
                var node = new Node.LiteralTuple();
                node.AddSpan(openParenToken.span);
                node.AddSpan(this.Previous.span);
                return node;
            }

            this.insideCondition.Push(false);

            var exprs = new List<Node>();
            exprs.Add(this.ParseExpr());

            if (this.ExpectMaybe(TokenKind.Comma) != null)
            {
                exprs.AddRange(this.ParseList(TokenKind.Comma, TokenKind.ParenClose, this.ParseExpr));
                this.insideCondition.Pop();

                var node = new Node.LiteralTuple();
                node.AddSpan(openParenToken.span);
                node.AddSpan(this.Previous.span);
                node.elems = exprs;
                return node;
            }
            else
            {
                this.Expect(TokenKind.ParenClose);
                this.insideCondition.Pop();

                var node = new Node.Parenthesized();
                node.AddSpan(openParenToken.span);
                node.AddSpan(this.Previous.span);
                node.inner = exprs[0];
                return node;
            }
        }


        private Node ParseIdentifierOrLiteralStructure()
        {
            var ident = this.ParseIdentifier();

            if (!this.insideCondition.Peek() && this.ExpectMaybe(TokenKind.BraceOpen) != null)
            {
                var fields = this.ParseList(TokenKind.Comma, TokenKind.BraceClose, this.ParseLiteralStructureField);
                var node = new Node.LiteralStructure();
                node.type = ident;
                node.fields = fields;
                node.AddSpan(this.Previous.span);
                return node;
            }
            else
                return ident;
        }


        private Node ParseLiteralBool()
        {
            if (this.Current.kind == TokenKind.KeywordTrue)
            {
                var node = new Node.LiteralBool { value = true };
                node.AddSpan(this.Expect(TokenKind.KeywordTrue).span);
                return node;
            }
            else
            {
                var node = new Node.LiteralBool { value = false };
                node.AddSpan(this.Expect(TokenKind.KeywordFalse).span);
                return node;
            }
        }


        private Node ParseLiteralStructureField()
        {
            var node = new Node.LiteralStructureField();
            node.name = this.ParseIdentifier();
            this.Expect(TokenKind.Equal);
            node.value = this.ParseExpr();
            return node;
        }


        private Node ParseLet()
        {
            var node = new Node.Let();
            node.AddSpan(this.Expect(TokenKind.KeywordLet).span);
            node.identifier = this.ParseIdentifier();

            if (this.ExpectMaybe(TokenKind.Colon) != null)
                node.type = this.ParseType();

            if (this.ExpectMaybe(TokenKind.Equal) != null)
                node.expr = this.ParseExpr();

            return node;
        }


        private Node ParseIf()
        {
            var node = new Node.If();
            node.AddSpan(this.Expect(TokenKind.KeywordIf).span);
            this.insideCondition.Push(true);
            node.condition = this.ParseExpr();
            this.insideCondition.Pop();
            node.trueBlock = this.ParseBlock();

            if (this.ExpectMaybe(TokenKind.KeywordElse) != null)
                node.falseBlock = this.ParseBlock();

            return node;
        }


        private Node ParseWhile()
        {
            var node = new Node.While();
            node.AddSpan(this.Expect(TokenKind.KeywordWhile).span);
            this.insideCondition.Push(true);
            node.condition = this.ParseExpr();
            this.insideCondition.Pop();
            node.block = this.ParseBlock();
            return node;
        }


        private Node ParseLoop()
        {
            var node = new Node.Loop();
            node.AddSpan(this.Expect(TokenKind.KeywordLoop).span);
            node.block = this.ParseBlock();
            return node;
        }


        private Node ParseBreak()
        {
            var node = new Node.Break();
            node.AddSpan(this.Expect(TokenKind.KeywordBreak).span);
            return node;
        }


        private Node ParseContinue()
        {
            var node = new Node.Continue();
            node.AddSpan(this.Expect(TokenKind.KeywordContinue).span);
            return node;
        }


        private Node ParseReturn()
        {
            var node = new Node.Return();
            node.AddSpan(this.Expect(TokenKind.KeywordReturn).span);

            if (this.Current.kind != TokenKind.BraceClose &&
                this.Current.kind != TokenKind.ParenClose &&
                this.Current.kind != TokenKind.Comma &&
                this.Current.kind != TokenKind.Semicolon)
                node.expr = this.ParseExpr();

            return node;
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


        private Node ParseExprOperator(int level)
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
                return node;
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

                lhs = node;

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
                expr = node;
            }

            return expr;
        }


        private Node ParseExprLeaf()
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

                case TokenKind.KeywordLet:
                    return this.ParseLet();

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

                case TokenKind.KeywordTrue:
                case TokenKind.KeywordFalse:
                    return this.ParseLiteralBool();

                default:
                    this.ErrorBeforeCurrent("expected expression");
                    throw new ParseException();
            }
        }
    }
}
