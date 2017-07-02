using System;
using System.Collections.Generic;


namespace pocketl.sema
{
    public class Code
    {
        public class Body
        {
            public int parameterCount;
            public List<Register> registers = new List<Register>();
            public List<Segment> segments = new List<Segment>();


            public int CreateSegment()
            {
                var segment = new Segment { terminator = new Terminator.Return() };
                this.segments.Add(segment);
                return this.segments.Count - 1;
            }


            public void AddInstruction(int segmentIndex, Instruction instr)
            {
                this.segments[segmentIndex].instrs.Add(instr);
            }


            public void PrintToConsole(Context ctx, int indent = 0)
            {
                for (var i = 0; i < this.registers.Count; i++)
                {
                    var reg = this.registers[i];
                    Console.Write(new string(' ', indent * 3));
                    Console.Write("r#" + i);

                    if (reg.name != null)
                        Console.Write(" `" + reg.name + "`");

                    Console.Write(": ");
                    Console.WriteLine(reg.type.PrintableName(ctx));
                }

                if (this.registers.Count > 0 && this.segments.Count > 0)
                    Console.WriteLine();

                for (var i = 0; i < this.segments.Count; i++)
                {
                    Console.Write(new string(' ', indent * 3));
                    Console.WriteLine("s#" + i + ":");
                    this.segments[i].PrintToConsole(ctx, indent + 1);
                    Console.WriteLine();
                }
            }
        }


        public class Register
        {
            public diagn.Span spanDef;
            public diagn.Span spanDefName;
            public string name;
            public Type type;
        }


        public class Segment
        {
            public List<Instruction> instrs = new List<Instruction>();
            public Terminator terminator;

            
            public void PrintToConsole(Context ctx, int indent = 0)
            {
                foreach (var instr in this.instrs)
                {
                    Console.Write(new string(' ', indent * 3));
                    instr.PrintToConsole(ctx);
                    Console.WriteLine();
                }

                Console.Write(new string(' ', indent * 3));
                terminator.PrintToConsole(ctx);
                Console.WriteLine();
            }
        }


        public abstract class Terminator
        {
            public virtual void PrintToConsole(Context ctx)
            {

            }


            public class Return : Terminator
            {
                public override void PrintToConsole(Context ctx)
                {
                    Console.Write("return");
                }
            }


            public class Goto : Terminator
            {
                public int segmentIndex;


                public override void PrintToConsole(Context ctx)
                {
                    Console.Write("goto s#" + segmentIndex);
                }
            }


            public class Branch : Terminator
            {
                public int conditionRegisterIndex;
                public int trueSegmentIndex;
                public int falseSegmentIndex;


                public override void PrintToConsole(Context ctx)
                {
                    Console.Write("branch r#" + conditionRegisterIndex);
                    Console.Write(" ? s#" + trueSegmentIndex);
                    Console.Write(" : s#" + falseSegmentIndex);
                }
            }
        }


        public class PrimitiveNumber
        {
            public enum Type
            {
                Int8, Int16, Int32, Int64,
                UInt8, UInt16, UInt32, UInt64,
                Float32, Float64
            }

            public ulong unsignedValue;
            public long signedValue;
            public double floatValue;
        }


        public abstract class Instruction
        {
            public diagn.Span span;
            public Lvalue destination;


            public virtual void PrintToConsole(Context ctx)
            {

            }


            public class CopyLiteralNumber : Instruction
            {
                public PrimitiveNumber number;
            }


            public class CopyLiteralBool : Instruction
            {
                public bool value;
            }


            public class CopyLiteralStructure : Instruction
            {
                public H<Def> hDef;
                public List<int> sourceFields = new List<int>();
            }


            public class CopyLiteralTuple : Instruction
            {
                public List<int> sources = new List<int>();


                public override void PrintToConsole(Context ctx)
                {
                    this.destination.PrintToConsole(ctx);
                    Console.Write(" = (");
                    for (var i = 0; i < this.sources.Count; i++)
                    {
                        if (i > 0)
                            Console.Write(", ");

                        Console.Write("r#" + this.sources[i]);
                    }
                    Console.Write(")");
                }
            }


            public class CopyFunction : Instruction
            {
                public H<Def> hDef;
            }


            public class CopyCallResult : Instruction
            {
                public int sourceTarget;
                public List<int> sourceArguments = new List<int>();
            }


            public class CopyLvalue : Instruction
            {
                public Lvalue source;


                public override void PrintToConsole(Context ctx)
                {
                    this.destination.PrintToConsole(ctx);
                    Console.Write(" = ");
                    this.source.PrintToConsole(ctx);
                }
            }
        }


        public abstract class Lvalue
        {
            public diagn.Span span;


            public virtual void PrintToConsole(Context ctx)
            {

            }


            public class Error : Lvalue
            {
                public override void PrintToConsole(Context ctx)
                {
                    Console.Write("<error>");
                }
            }


            public class Discard : Lvalue
            {
                public override void PrintToConsole(Context ctx)
                {
                    Console.Write("_");
                }
            }


            public class Register : Lvalue
            {
                public int index;


                public override void PrintToConsole(Context ctx)
                {
                    Console.Write("r#" + index);
                }
            }
        }
    }
}
