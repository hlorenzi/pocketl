using System;
using System.Collections.Generic;


namespace pocketl.syn
{
    public class Node
    {
        public virtual IEnumerable<H<Node>> Children()
        {
            yield break;
        }


        public virtual void PrintExtraInfoToConsole(Context ctx)
        {
            
        }


        public void PrintToConsole(Context ctx, int indent = 0)
        {
            switch (indent % 4)
            {
                case 0: Console.ForegroundColor = ConsoleColor.White; break;
                case 1: Console.ForegroundColor = ConsoleColor.Gray; break;
                case 2: Console.ForegroundColor = ConsoleColor.DarkGray; break;
                case 3: Console.ForegroundColor = ConsoleColor.Gray; break;
            }

            Console.Write(new string(' ', indent * 3));
            Console.Write(this.GetType().Name);
            Console.Write(" ");
            this.PrintExtraInfoToConsole(ctx);
            Console.WriteLine();
            foreach (var child in this.Children())
                ctx[child].PrintToConsole(ctx, indent + 1);
        }


        public class Error : Node
        {

        }


        public class TopLevel : Node
        {
            public List<H<Node>> defs = new List<H<Node>>();


            public override IEnumerable<H<Node>> Children()
            {
                foreach (var def in this.defs)
                    yield return def;
            }
        }


        public class FunctionDef : Node
        {
            public H<Node> name;
            public List<H<Node>> parameters = new List<H<Node>>();
            public H<Node>? returnType;
            public H<Node> body;


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.name;

                foreach (var param in this.parameters)
                    yield return param;

                if (this.returnType.HasValue)
                    yield return this.returnType.Value;

                yield return this.body;
            }
        }


        public class FunctionDefParameter : Node
        {
            public H<Node> name;
            public H<Node> type;


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.name;
                yield return this.type;
            }
        }


        public class StructureDef : Node
        {
            public H<Node> name;
            public List<H<Node>> fields = new List<H<Node>>();


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.name;

                foreach (var field in this.fields)
                    yield return field;
            }
        }


        public class StructureDefField : Node
        {
            public H<Node> name;
            public H<Node> type;


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.name;
                yield return this.type;
            }
        }


        public class Identifier : Node
        {
            public H<syn.Token> token;


            public string Excerpt(Context ctx)
            {
                return ctx[token].excerpt;
            }


            public override void PrintExtraInfoToConsole(Context ctx)
            {
                Console.Write("`{0}`", ctx[token].excerpt);
            }
        }


        public class Number : Node
        {
            public H<syn.Token> token;


            public string Excerpt(Context ctx)
            {
                return ctx[token].excerpt;
            }


            public override void PrintExtraInfoToConsole(Context ctx)
            {
                Console.Write("`{0}`", ctx[token].excerpt);
            }
        }


        public class TypeStructure : Node
        {
            public H<Node> name;


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.name;
            }
        }


        public class Block : Node
        {
            public List<H<Node>> exprs = new List<H<Node>>();


            public override IEnumerable<H<Node>> Children()
            {
                foreach (var expr in this.exprs)
                    yield return expr;
            }
        }


        public class Parenthesized : Node
        {
            public H<Node> inner;


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.inner;
            }
        }


        public class If : Node
        {
            public H<Node> condition;
            public H<Node> trueBlock;
            public H<Node>? falseBlock;


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.condition;
                yield return this.trueBlock;
                if (this.falseBlock.HasValue)
                    yield return this.falseBlock.Value;
            }
        }


        public class While : Node
        {
            public H<Node> condition;
            public H<Node> block;


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.condition;
                yield return this.block;
            }
        }


        public class Loop : Node
        {
            public H<Node> block;


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.block;
            }
        }


        public class Break : Node
        {
            public H<Node>? label;


            public override IEnumerable<H<Node>> Children()
            {
                if (this.label.HasValue)
                    yield return this.label.Value;
            }
        }


        public class Continue : Node
        {
            public H<Node>? label;


            public override IEnumerable<H<Node>> Children()
            {
                if (this.label.HasValue)
                    yield return this.label.Value;
            }
        }


        public class Return : Node
        {
            public H<Node>? expr;


            public override IEnumerable<H<Node>> Children()
            {
                if (this.expr.HasValue)
                    yield return this.expr.Value;
            }
        }


        public class LiteralTuple : Node
        {
            public List<H<Node>> elems = new List<H<Node>>();


            public override IEnumerable<H<Node>> Children()
            {
                foreach (var elem in this.elems)
                    yield return elem;
            }
        }


        public class LiteralStructure : Node
        {
            public H<Node> type;
            public List<H<Node>> fields = new List<H<Node>>();


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.type;
                foreach (var field in this.fields)
                    yield return field;
            }
        }


        public class LiteralStructureField : Node
        {
            public H<Node> name;
            public H<Node> value;


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.name;
                yield return this.value;
            }
        }


        public enum UnaryOperator
        {
            RefCount,
            RefCountMut,
            Negate,
            Not,
            Unwrap
        }


        public class UnaryOperation : Node
        {
            public UnaryOperator op;
            public H<Node> expr;


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.expr;
            }


            public override void PrintExtraInfoToConsole(Context ctx)
            {
                switch (this.op)
                {
                    case UnaryOperator.RefCount: Console.Write("($)"); break;
                    case UnaryOperator.RefCountMut: Console.Write("($mut)"); break;
                    case UnaryOperator.Negate: Console.Write("(-)"); break;
                    case UnaryOperator.Not: Console.Write("(!_)"); break;
                    case UnaryOperator.Unwrap: Console.Write("(_!)"); break;
                }
            }
        }


        public enum BinaryOperator
        {
            Assign,
            FieldAccess,
            Add,
            Subtract,
            Multiply,
            Divide
        }


        public class BinaryOperation : Node
        {
            public BinaryOperator op;
            public H<Node> lhs;
            public H<Node> rhs;


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.lhs;
                yield return this.rhs;
            }


            public override void PrintExtraInfoToConsole(Context ctx)
            {
                switch (this.op)
                {
                    case BinaryOperator.Assign: Console.Write("(=)"); break;
                    case BinaryOperator.FieldAccess: Console.Write("(.)"); break;
                    case BinaryOperator.Add: Console.Write("(+)"); break;
                    case BinaryOperator.Subtract: Console.Write("(-)"); break;
                    case BinaryOperator.Multiply: Console.Write("(*)"); break;
                    case BinaryOperator.Divide: Console.Write("(/)"); break;
                }
            }
        }
    }
}
