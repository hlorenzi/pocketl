using System;
using System.Collections.Generic;


namespace pocketl.sema
{
    public class Map
    {
        public Dictionary<syn.Node, H<sema.Def>> def = new Dictionary<syn.Node, H<Def>>();
        public Dictionary<syn.Node, sema.Namespace.Node> references = new Dictionary<syn.Node, Namespace.Node>();


        public void PrintExtraInfoToConsole(Context ctx, syn.Node node)
        {
            if (this.def.TryGetValue(node, out var def))
                Console.Write("<def#" + def.id + "> ");

            if (this.references.TryGetValue(node, out var references))
                Console.Write("<refs `" + ctx.names.PrintableFullKeyOf(references) + "`> ");
        }
    }
}
