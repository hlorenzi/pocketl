using System.Collections.Generic;
using System.Linq;


namespace pocketl.sema
{
    public abstract class Type
    {
        public virtual string PrintableName(Context ctx)
        {
            return "<?>";
        }


        public class Error : Type
        {
            public override string PrintableName(Context ctx)
            {
                return "<error>";
            }
        }


        public class Structure : Type
        {
            public H<Def> def;


            public override string PrintableName(Context ctx)
            {
                return ctx.names.PrintableFullKeyOf(ctx[def].namespaceNode);
            }
        }


        public class Pointer : Type
        {
            public bool mutable;
            public Type innerType;


            public override string PrintableName(Context ctx)
            {
                return (this.mutable ? "*mut " : "*") + this.innerType.PrintableName(ctx);
            }
        }


        public class RefCounted : Type
        {
            public bool mutable;
            public Type innerType;


            public override string PrintableName(Context ctx)
            {
                return (this.mutable ? "$mut " : "$") + this.innerType.PrintableName(ctx);
            }
        }


        public class Tuple : Type
        {
            public List<Type> types = new List<Type>();

            
            public override string PrintableName(Context ctx)
            {
                var str = "(";
                for (var i = 0; i < this.types.Count; i++)
                {
                    if (i > 0)
                        str += ", ";

                    str += this.types[i].PrintableName(ctx);
                }

                return str + ")";
            }
        }


        public class Function : Type
        {
            public Type returnType;
            public List<Type> parameters = new List<Type>();


            public override string PrintableName(Context ctx)
            {
                var str = "fn(";
                for (var i = 0; i < this.parameters.Count; i++)
                {
                    if (i > 0)
                        str += ", ";

                    str += this.parameters[i].PrintableName(ctx);
                }

                return str + ") -> " + this.returnType.PrintableName(ctx);
            }
        }
    }
}
