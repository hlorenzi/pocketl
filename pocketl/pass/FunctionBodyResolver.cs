namespace pocketl.pass
{
    public class FunctionBodyResolver
    {
        public static void Resolve(Context ctx, diagn.Reporter reporter, H<mod.Unit> hUnit)
        {
            var unit = ctx[hUnit];
            ResolveNode(ctx, reporter, unit, unit.ast);
        }


        private static void ResolveNode(Context ctx, diagn.Reporter reporter, mod.Unit unit, syn.Node node)
        {
            TryCollectFunctionDef(ctx, reporter, unit, node as syn.Node.FunctionDef);

            foreach (var child in node.Children())
                ResolveNode(ctx, reporter, unit, child);
        }


        private static void TryCollectFunctionDef(Context ctx, diagn.Reporter reporter, mod.Unit unit, syn.Node.FunctionDef node)
        {
            if (node == null)
                return;

            var hDef = unit.semanticMap.def[node];
            if (hDef == null)
                reporter.InternalError("def not found for node", new diagn.Caret(node.span));

            else
            {
                var functionDef = ctx[hDef] as sema.Def.Function;
                CodeResolver.ResolveFunctionBody(ctx, reporter, functionDef.body, node.body);
            }
        }
    }
}
