using System;
using System.Collections.Generic;


namespace pocketl.sema
{
    public abstract class Def
    {
        public diagn.Span spanDef;
        public diagn.Span spanDefName;
        public Namespace.Node namespaceNode;


        public virtual void PrintToConsole(Context ctx, int indent = 0)
        {

        }


        public class Structure : Def
        {
            public class Field
            {
                public diagn.Span spanDef;
                public diagn.Span spanDefName;
                public string name;
                public Type type;
            }


            public List<Field> fields = new List<Field>();


            public override void PrintToConsole(Context ctx, int indent = 0)
            {
                Console.WriteLine("type");

                foreach (var field in this.fields)
                {
                    Console.Write(new string(' ', indent * 3));
                    Console.Write(field.name);
                    Console.Write(": ");
                    Console.WriteLine(field.type.PrintableName(ctx));
                }
            }
        }


        public class Function : Def
        {
            public Code.Body body;


            public override void PrintToConsole(Context ctx, int indent = 0)
            {
                Console.WriteLine("fn");
                this.body.PrintToConsole(ctx, indent);
            }
        }
    }
}
