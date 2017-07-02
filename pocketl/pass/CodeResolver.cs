using System.Collections.Generic;


namespace pocketl.pass
{
    public class CodeResolver
    {
        public static void ResolveFunctionBody(Context ctx, diagn.Reporter reporter, sema.Code.Body body, syn.Node node)
        {
            var resolver = new CodeResolver(ctx, reporter, body);

            var segment = body.CreateSegment();
            var destination = new sema.Code.Lvalue.Register { index = 0 };

            resolver.ResolveExpr(ref segment, destination, node);
        }


        private Context ctx;
        private diagn.Reporter reporter;
        private sema.Code.Body code;
        private List<bool> registersInScope = new List<bool>();


        private CodeResolver(Context ctx, diagn.Reporter reporter, sema.Code.Body code)
        {
            this.ctx = ctx;
            this.reporter = reporter;
            this.code = code;

            for (var i = 0; i < code.registers.Count; i++)
                this.registersInScope.Add(true);
        }


        private void ResolveExpr(ref int segment, sema.Code.Lvalue destination, syn.Node node)
        {
            var nodeBlock = node as syn.Node.Block;
            var nodeLet = node as syn.Node.Let;
            var nodeIdentifier = node as syn.Node.Identifier;
            var nodeTuple = node as syn.Node.LiteralTuple;
            var nodeBinaryOp = node as syn.Node.BinaryOperation;
            
            if (nodeBlock != null)
                this.ResolveExprBlock(ref segment, destination, nodeBlock);

            else if (nodeLet != null)
                this.ResolveExprLet(ref segment, destination, nodeLet);

            else if (nodeIdentifier != null)
                this.ResolveExprName(ref segment, destination, nodeIdentifier);

            else if (nodeTuple != null)
                this.ResolveExprTuple(ref segment, destination, nodeTuple);

            else if (nodeBinaryOp != null)
                this.ResolveExprBinaryOp(ref segment, destination, nodeBinaryOp);

            else
                this.reporter.InternalError("unimplemented", new diagn.Caret(node.span));
        }


        private sema.Code.Lvalue ResolveLvalue(ref int segment, syn.Node node)
        {
            var nodeIdentifier = node as syn.Node.Identifier;
            if (nodeIdentifier != null)
                return this.ResolveLvalueName(ref segment, nodeIdentifier);

            else
            {
                this.reporter.InternalError("unimplemented", new diagn.Caret(node.span));
                return new sema.Code.Lvalue.Discard { span = node.span };
            }
        }


        private void ResolveExprBlock(ref int segment, sema.Code.Lvalue destination, syn.Node.Block node)
        {
            if (node.exprs.Count == 0)
            {
                this.AddVoidInstruction(segment, destination, node.span);
                return;
            }

            var scopeIndexBefore = this.registersInScope.Count;

            for (var i = 0; i < node.exprs.Count; i++)
            {
                var exprDestination = destination;
                if (i < node.exprs.Count - 1)
                    exprDestination = new sema.Code.Lvalue.Discard();

                this.ResolveExpr(ref segment, exprDestination, node.exprs[i]);
            }

            for (var i = scopeIndexBefore; i < this.registersInScope.Count; i++)
                this.registersInScope[i] = false;
        }


        private void ResolveExprLet(ref int segment, sema.Code.Lvalue destination, syn.Node.Let node)
        {
            var registerIndex = this.CreateRegister();
            var register = this.code.registers[registerIndex];

            register.spanDef = node.span;
            register.spanDefName = node.identifier.span;
            register.name = (node.identifier as syn.Node.Identifier).token.excerpt;

            if (node.type != null)
                register.type = TypeResolver.Resolve(this.ctx, this.reporter, node.type);
            else
                register.type = new sema.Type.Placeholder();

            if (node.expr != null)
            {
                var assignDestination = new sema.Code.Lvalue.Register
                {
                    span = node.identifier.span,
                    index = registerIndex
                };

                this.ResolveExpr(ref segment, assignDestination, node.expr);
            }

            this.AddVoidInstruction(segment, destination, node.span);
        }


        private void ResolveExprName(ref int segment, sema.Code.Lvalue destination, syn.Node.Identifier node)
        {
            this.code.AddInstruction(segment, new sema.Code.Instruction.CopyLvalue
            {
                span = node.span,
                destination = destination,
                source = this.ResolveLvalue(ref segment, node)
            });
        }


        private void ResolveExprTuple(ref int segment, sema.Code.Lvalue destination, syn.Node.LiteralTuple node)
        {
            var elems = new List<int>();
            foreach (var elem in node.elems)
                elems.Add(this.ResolveIntoNewRegister(ref segment, elem));

            this.code.AddInstruction(segment, new sema.Code.Instruction.CopyLiteralTuple
            {
                span = node.span,
                destination = destination,
                sources = elems
            });
        }


        private void ResolveExprBinaryOp(ref int segment, sema.Code.Lvalue destination, syn.Node.BinaryOperation node)
        {
            if (node.op == syn.Node.BinaryOperator.Assign)
            {
                var assignDestination = this.ResolveLvalue(ref segment, node.lhs);
                this.ResolveExpr(ref segment, assignDestination, node.rhs);
                this.AddVoidInstruction(segment, destination, node.span);
            }
            else
                this.reporter.InternalError("unimplemented", new diagn.Caret(node.span));
        }


        private sema.Code.Lvalue ResolveLvalueName(ref int segment, syn.Node.Identifier node)
        {
            var registerIndex = FindRegisterByName(node.token.excerpt);
            if (registerIndex >= 0)
                return new sema.Code.Lvalue.Register { span = node.span, index = registerIndex };

            else
            {
                this.reporter.Error("unknown `" + node.token.excerpt + "`", new diagn.Caret(node.span));
                return new sema.Code.Lvalue.Error { span = node.span };
            }
        }


        private void AddVoidInstruction(int segment, sema.Code.Lvalue destination, diagn.Span span)
        {
            if (destination is sema.Code.Lvalue.Discard)
                return;

            this.code.AddInstruction(segment, new sema.Code.Instruction.CopyLiteralTuple
            {
                destination = destination,
                span = span
            });
        }


        private int CreateRegister()
        {
            var newRegisterIndex = this.code.registers.Count;
            this.registersInScope.Add(true);
            this.code.registers.Add(new sema.Code.Register());
            return newRegisterIndex;
        }


        private int ResolveIntoNewRegister(ref int segment, syn.Node node)
        {
            var newRegisterIndex = this.code.registers.Count;
            this.registersInScope.Add(true);
            this.code.registers.Add(new sema.Code.Register
            {
                spanDef = node.span,
                type = new sema.Type.Placeholder()
            });

            var destination = new sema.Code.Lvalue.Register { span = node.span, index = newRegisterIndex };
            this.ResolveExpr(ref segment, destination, node);
            return newRegisterIndex;
        }


        private int FindRegisterByName(string name)
        {
            for (var i = this.code.registers.Count - 1; i >= 0; i--)
            {
                if (!this.registersInScope[i])
                    continue;

                if (this.code.registers[i].name != name)
                    continue;

                return i;
            }

            return -1;
        }
    }
}
