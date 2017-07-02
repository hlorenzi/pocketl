using System;
using System.Collections.Generic;


namespace pocketl.diagn
{
    public interface Reporter
    {
        void InternalError(string descr, params Caret[] carets);
        void Error(string descr, params Caret[] carets);
    }


    public class Caret
    {
        public Span span;
        public bool primary;
        public string descr;


        public Caret(Span span, bool primary = true, string descr = null)
        {
            this.span = span;
            this.primary = primary;
            this.descr = descr;
        }
    }


    public class ReporterDefault : Reporter
    {
        List<Message> messages = new List<Message>();


        class Message
        {
            public MessageKind kind;
            public string descr;
            public List<Caret> carets;
        }


        enum MessageKind
        {
            InternalError,
            Error,
            Warning,
            Info,
            Lint
        }


        void Reporter.InternalError(string descr, params Caret[] carets)
        {
            this.messages.Add(new Message
            {
                kind = MessageKind.InternalError,
                descr = descr,
                carets = new List<Caret>(carets)
            });
        }


        void Reporter.Error(string descr, params Caret[] carets)
        {
            this.messages.Add(new Message
            {
                kind = MessageKind.Error,
                descr = descr,
                carets = new List<Caret>(carets)
            });
        }


        public void Print(util.Output output, Context ctx)
        {
            foreach (var msg in this.messages)
            {
                this.PrintMessage(output.Cloned, ctx, msg);
                output.WriteLine();
            }
        }


        void PrintMessage(util.Output output, Context ctx, Message msg)
        {
            output.SetColor(ConsoleColor.Gray);

            // Print location.
            var primaryCaret = msg.carets.Find(c => c.primary && c.span.unit != null);
            if (primaryCaret != null)
            {
                var unit = ctx.units[primaryCaret.span.unit];
                var src = unit.ReadSource(ctx);

                var unitName = unit.name;
                var start = util.CharCounter.LineColumnOfIndex(src, primaryCaret.span.start);
                var end = util.CharCounter.LineColumnOfIndex(src, primaryCaret.span.end);
                output.WriteLine("{0}:{1}:{2} {3}:{4}:",
                    unitName,
                    start.line + 1, start.column + 1,
                    end.line + 1, end.column + 1);
            }
            else
                output.WriteLine("<unknown location>:");

            // Print description.
            ConsoleColor textColor = ConsoleColor.White;
            switch (msg.kind)
            {
                case MessageKind.InternalError:
                    textColor = ConsoleColor.Red;
                    output.SetColor(textColor);
                    output.Write("internal compiler error: ");
                    break;
                case MessageKind.Error:
                    textColor = ConsoleColor.Red;
                    output.SetColor(textColor);
                    output.Write("error: ");
                    break;
                case MessageKind.Warning:
                    textColor = ConsoleColor.Yellow;
                    output.SetColor(textColor);
                    output.Write("warning: ");
                    break;
                case MessageKind.Info:
                    textColor = ConsoleColor.Cyan;
                    output.SetColor(textColor);
                    output.Write("note: ");
                    break;
                case MessageKind.Lint:
                    textColor = ConsoleColor.Magenta;
                    output.SetColor(textColor);
                    output.Write("lint: ");
                    break;
            }

            output.WriteLine(msg.descr);

            // Print annotated source code.
            var curCaret = 0;
            while (curCaret < msg.carets.Count)
            {
                // Collect consecutive carets with the same source unit
                // and ascending line numbers.
                var carets = new List<Caret>();
                carets.Add(msg.carets[curCaret]);
                curCaret++;

                while (curCaret < msg.carets.Count &&
                    msg.carets[curCaret].span.unit?.id == carets[0].span.unit?.id &&
                    msg.carets[curCaret].span.start >= carets[0].span.start)
                {
                    carets.Add(msg.carets[curCaret]);
                    curCaret++;
                }

                if (carets[0].span.unit == null)
                    continue;

                // Collect line numbers that will be printed.
                // Also collect surrounding lines for contextual aid.
                var unit = ctx.units[carets[0].span.unit];
                var src = unit.ReadSource(ctx);
                var srcLineCount = util.CharCounter.LineCount(src);
                var surrounding = 2;
                var lineSet = new HashSet<int>();

                foreach (var caret in carets)
                {
                    var start = util.CharCounter.LineColumnOfIndex(src, caret.span.start);
                    var end = util.CharCounter.LineColumnOfIndex(src, caret.span.end);

                    for (var line = start.line - surrounding; line <= end.line + surrounding; line++)
                        lineSet.Add(line);
                }

                var lines = new List<int>(lineSet);
                lines.Sort();

                // Print unit name.
                output.SetColor(ConsoleColor.DarkGray);
                output.WriteLine("   --> " + unit.name);
                output.WriteLine("     |");

                // Print source code lines.
                var lastLinePrinted = (int?)null;
                foreach (var line in lines)
                {
                    if (line < 0 || line >= srcLineCount)
                        continue;

                    // Print line number.
                    output.SetColor(ConsoleColor.DarkGray);

                    if (lastLinePrinted.HasValue && line != lastLinePrinted.Value + 1)
                        output.WriteLine("    ...");

                    lastLinePrinted = line;

                    output.Write("{0,4} | ", line + 1);

                    // Print line text.
                    var range = util.CharCounter.IndexRangeOfLine(src, line);

                    output.SetColor(ConsoleColor.White);
                    for (var i = range.start; i < range.end; i++)
                    {
                        // Add a space for spans of zero characters.
                        if (carets.Find(c => i == c.span.start && i == c.span.end) != null)
                            output.Write(" ");

                        // Print char with special handling for whitespace.
                        switch (src[i])
                        {
                            case '\t': output.Write("  "); break;
                            case '\r':
                            case '\n': break;
                            default: output.Write(src[i].ToString()); break;
                        }
                    }

                    output.WriteLine();

                    // Print line caret markings.
                    var hasAnyMarkings =
                        (carets.Find(c => c.span.end >= range.start && c.span.start < range.end) != null);

                    if (hasAnyMarkings)
                    {
                        output.SetColor(ConsoleColor.DarkGray);
                        output.Write("     | ");
                        output.SetColor(textColor);

                        for (var i = range.start; i < range.end; i++)
                        {
                            // Print marking for spans of zero characters.
                            var zeroCharCaret = carets.Find(c => i == c.span.start && i == c.span.end);
                            if (zeroCharCaret != null)
                                output.Write(zeroCharCaret.primary ? "^" : "-");

                            // Print marking with special handling for whitespace.
                            var marking = " ";
                            var charCaret = carets.Find(c => i >= c.span.start && i < c.span.end);
                            if (charCaret != null)
                                marking = (charCaret.primary ? "^" : "-");

                            switch (src[i])
                            {
                                case '\t': output.Write(marking + marking); break;
                                case '\r': output.Write(""); break;
                                case '\n': output.Write(""); break;
                                default: output.Write(marking); break;
                            }
                        }

                        output.WriteLine();
                    }
                }
            }
        }
    }
}
