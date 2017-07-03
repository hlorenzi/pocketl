using System;
using System.Collections.Generic;


namespace pocketl.syn
{
    public class Node
    {
        public diagn.Span span;


        public void AddSpan(diagn.Span span)
        {
            if (this.span == null)
                this.span = span;
            else
                this.span += span;
        }


        public void AddChildrenSpansRecursively()
        {
            foreach (var child in this.Children())
            {
                child.AddChildrenSpansRecursively();
                this.AddSpan(child.span);
            }
        }


        public virtual IEnumerable<Node> Children()
        {
            yield break;
        }


        public void PrintDebug(util.Output output, Context ctx, sema.Map semanticMap = null)
        {
            switch (output.Indentation % 4)
            {
                case 0: output = output.WithColor(ConsoleColor.White); break;
                case 1: output = output.WithColor(ConsoleColor.Gray); break;
                case 2: output = output.WithColor(ConsoleColor.DarkGray); break;
                case 3: output = output.WithColor(ConsoleColor.Gray); break;
            }

            output.Write(this.GetType().Name);
            output.Write(" ");
            this.PrintDebugExtra(output, ctx);

            if (semanticMap != null)
            {
                output.Write(" ");
                semanticMap.PrintDebugExtra(output, ctx, this);
            }

            output.WriteLine();
            foreach (var child in this.Children())
                child.PrintDebug(output.Indented, ctx, semanticMap);
        }


        public virtual void PrintDebugExtra(util.Output output, Context ctx)
        {

        }


        public class Error : Node
        {

        }


        public class TopLevel : Node
        {
            public List<Node> defs = new List<Node>();


            public override IEnumerable<Node> Children()
            {
                foreach (var def in this.defs)
                    yield return def;
            }
        }


        public class FunctionDef : Node
        {
            public Node name;
            public List<Node> parameters = new List<Node>();
            public Node returnType;
            public Node body;


            public override IEnumerable<Node> Children()
            {
                yield return this.name;

                foreach (var param in this.parameters)
                    yield return param;

                if (this.returnType != null)
                    yield return this.returnType;

                yield return this.body;
            }
        }


        public class FunctionDefParameter : Node
        {
            public Node identifier;
            public Node type;


            public override IEnumerable<Node> Children()
            {
                yield return this.identifier;
                yield return this.type;
            }
        }


        public class StructureDef : Node
        {
            public Node name;
            public List<Node> fields = new List<Node>();


            public override IEnumerable<Node> Children()
            {
                yield return this.name;

                foreach (var field in this.fields)
                    yield return field;
            }
        }


        public class StructureDefField : Node
        {
            public Node identifier;
            public Node type;


            public override IEnumerable<Node> Children()
            {
                yield return this.identifier;
                yield return this.type;
            }
        }


        public class Identifier : Node
        {
            public Token token;


            public string Excerpt(Context ctx)
            {
                return this.token.excerpt;
            }


            public override void PrintDebugExtra(util.Output output, Context ctx)
            {
                output.Write("`" + this.token.excerpt + "`");
            }
        }


        public class Number : Node
        {
            public Token token;


            public string Excerpt(Context ctx)
            {
                return this.token.excerpt;
            }


            public override void PrintDebugExtra(util.Output output, Context ctx)
            {
                output.Write("`" + this.token.excerpt + "`");
            }
        }


        public class TypeStructure : Node
        {
            public Node name;


            public override IEnumerable<Node> Children()
            {
                yield return this.name;
            }
        }


        public class TypePointer : Node
        {
            public bool mutable;
            public Node innerType;


            public override IEnumerable<Node> Children()
            {
                yield return this.innerType;
            }
        }


        public class TypeRefCounted : Node
        {
            public bool mutable;
            public Node innerType;


            public override IEnumerable<Node> Children()
            {
                yield return this.innerType;
            }
        }


        public class TypeTuple : Node
        {
            public List<Node> elements = new List<Node>();


            public override IEnumerable<Node> Children()
            {
                foreach (var elem in this.elements)
                    yield return elem;
            }
        }


        public class TypeFunction : Node
        {
            public List<Node> parameters = new List<Node>();
            public Node returnType;


            public override IEnumerable<Node> Children()
            {
                foreach (var p in this.parameters)
                    yield return p;

                if (this.returnType != null)
                    yield return this.returnType;
            }
        }


        public class Block : Node
        {
            public List<Node> exprs = new List<Node>();


            public override IEnumerable<Node> Children()
            {
                foreach (var expr in this.exprs)
                    yield return expr;
            }
        }


        public class Parenthesized : Node
        {
            public Node inner;


            public override IEnumerable<Node> Children()
            {
                yield return this.inner;
            }
        }


        public class Let : Node
        {
            public Node identifier;
            public Node type;
            public Node expr;


            public override IEnumerable<Node> Children()
            {
                yield return this.identifier;
                if (this.type != null)
                    yield return this.type;
                if (this.expr != null)
                    yield return this.expr;
            }
        }


        public class If : Node
        {
            public Node condition;
            public Node trueBlock;
            public Node falseBlock;


            public override IEnumerable<Node> Children()
            {
                yield return this.condition;
                yield return this.trueBlock;
                if (this.falseBlock != null)
                    yield return this.falseBlock;
            }
        }


        public class While : Node
        {
            public Node condition;
            public Node block;


            public override IEnumerable<Node> Children()
            {
                yield return this.condition;
                yield return this.block;
            }
        }


        public class Loop : Node
        {
            public Node block;


            public override IEnumerable<Node> Children()
            {
                yield return this.block;
            }
        }


        public class Break : Node
        {
            public Node label;


            public override IEnumerable<Node> Children()
            {
                if (this.label != null)
                    yield return this.label;
            }
        }


        public class Continue : Node
        {
            public Node label;


            public override IEnumerable<Node> Children()
            {
                if (this.label != null)
                    yield return this.label;
            }
        }


        public class Return : Node
        {
            public Node expr;


            public override IEnumerable<Node> Children()
            {
                if (this.expr != null)
                    yield return this.expr;
            }
        }


        public class LiteralBool : Node
        {
            public bool value;
        }


        public class LiteralTuple : Node
        {
            public List<Node> elems = new List<Node>();


            public override IEnumerable<Node> Children()
            {
                foreach (var elem in this.elems)
                    yield return elem;
            }
        }


        public class LiteralStructure : Node
        {
            public Node type;
            public List<Node> fields = new List<Node>();


            public override IEnumerable<Node> Children()
            {
                yield return this.type;
                foreach (var field in this.fields)
                    yield return field;
            }
        }


        public class LiteralStructureField : Node
        {
            public Node name;
            public Node value;


            public override IEnumerable<Node> Children()
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
            public Node expr;


            public override IEnumerable<Node> Children()
            {
                yield return this.expr;
            }


            public override void PrintDebugExtra(util.Output output, Context ctx)
            {
                switch (this.op)
                {
                    case UnaryOperator.RefCount: output.Write("($)"); break;
                    case UnaryOperator.RefCountMut: output.Write("($mut)"); break;
                    case UnaryOperator.Negate: output.Write("(-)"); break;
                    case UnaryOperator.Not: output.Write("(!_)"); break;
                    case UnaryOperator.Unwrap: output.Write("(_!)"); break;
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
            public Node lhs;
            public Node rhs;


            public override IEnumerable<Node> Children()
            {
                yield return this.lhs;
                yield return this.rhs;
            }


            public override void PrintDebugExtra(util.Output output, Context ctx)
            {
                switch (this.op)
                {
                    case BinaryOperator.Assign: output.Write("(=)"); break;
                    case BinaryOperator.FieldAccess: output.Write("(.)"); break;
                    case BinaryOperator.Add: output.Write("(+)"); break;
                    case BinaryOperator.Subtract: output.Write("(-)"); break;
                    case BinaryOperator.Multiply: output.Write("(*)"); break;
                    case BinaryOperator.Divide: output.Write("(/)"); break;
                }
            }
        }
    }
}
