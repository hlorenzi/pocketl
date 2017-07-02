using System.Collections.Generic;


namespace pocketl.sema
{
    public abstract class Def
    {
        public diagn.Span spanDef;
        public diagn.Span spanDefName;
        public Namespace.Node namespaceNode;


        public virtual void PrintDebug(util.Output output, Context ctx)
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


            public override void PrintDebug(util.Output output, Context ctx)
            {
                output.WriteLine("type");
                output = output.Indented;

                foreach (var field in this.fields)
                {
                    output.Write(field.name);
                    output.Write(": ");
                    output.WriteLine(field.type.PrintableName(ctx));
                }
            }
        }


        public class Function : Def
        {
            public Code.Body body;


            public override void PrintDebug(util.Output output, Context ctx)
            {
                output.WriteLine("fn");
                this.body.PrintDebug(output.Indented, ctx);
            }
        }
    }
}
