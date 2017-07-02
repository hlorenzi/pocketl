namespace pocketl.pass
{
    public class FunctionHeaderResolver
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
                functionDef.body = new sema.Code.Body();

                if (node.returnType == null)
                    functionDef.body.registers.Add(new sema.Code.Register
                    {
                        type = new sema.Type.Tuple()
                    });
                else
                    functionDef.body.registers.Add(new sema.Code.Register
                    {
                        spanDef = node.returnType.span,
                        type = TypeResolver.Resolve(ctx, reporter, node.returnType)
                    });

                foreach (var child in node.parameters)
                {
                    var nodeParameter = child as syn.Node.FunctionDefParameter;
                    var nodeIdentifier = nodeParameter.identifier as syn.Node.Identifier;

                    var parameterName = nodeIdentifier.token.excerpt;
                    var parameterType = TypeResolver.Resolve(ctx, reporter, nodeParameter.type);

                    functionDef.body.parameterCount++;
                    functionDef.body.registers.Add(new sema.Code.Register
                    {
                        spanDef = nodeParameter.span,
                        spanDefName = nodeIdentifier.span,
                        name = parameterName,
                        type = parameterType
                    });
                }
            }
        }
    }
}
