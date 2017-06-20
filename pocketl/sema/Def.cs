using System.Collections.Generic;


namespace pocketl.sema
{
    public abstract class Def
    {
        public diagn.Span defSpan;
        public diagn.Span defNameSpan;
        public sema.Namespace.Node hNamespaceNode;


        public class Structure : Def
        {
            public class Field
            {
                public string name;
            }


            public List<Field> fields = new List<Field>();
        }


        public class Function : Def
        {

        }
    }
}
