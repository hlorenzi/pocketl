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


        public class TypeStructure : Node
        {
            public H<Node> name;


            public override IEnumerable<H<Node>> Children()
            {
                yield return this.name;
            }
        }


        public class ExprBlock : Node
        {
            public List<H<Node>> exprs = new List<H<Node>>();


            public override IEnumerable<H<Node>> Children()
            {
                foreach (var expr in this.exprs)
                    yield return expr;
            }
        }
    }
}
