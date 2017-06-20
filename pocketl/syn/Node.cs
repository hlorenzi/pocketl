using System;
using System.Collections.Generic;


namespace pocketl.syn
{
    public class Node
    {
        public virtual IEnumerable<Node> Children()
        {
            yield break;
        }


        public virtual void PrintExtraInfoToConsole(Context ctx)
        {
            
        }


        public void PrintToConsole(Context ctx, sema.Map semanticMap = null, int indent = 0)
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

            if (semanticMap != null)
            {
                Console.Write(" ");
                semanticMap.PrintExtraInfoToConsole(ctx, this);
            }

            Console.WriteLine();
            foreach (var child in this.Children())
                child.PrintToConsole(ctx, semanticMap, indent + 1);
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
            public Node name;
            public Node type;


            public override IEnumerable<Node> Children()
            {
                yield return this.name;
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
            public Node name;
            public Node type;


            public override IEnumerable<Node> Children()
            {
                yield return this.name;
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


            public override void PrintExtraInfoToConsole(Context ctx)
            {
                Console.Write("`{0}`", this.token.excerpt);
            }
        }


        public class Number : Node
        {
            public Token token;


            public string Excerpt(Context ctx)
            {
                return this.token.excerpt;
            }


            public override void PrintExtraInfoToConsole(Context ctx)
            {
                Console.Write("`{0}`", this.token.excerpt);
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
            public Node lhs;
            public Node rhs;


            public override IEnumerable<Node> Children()
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
