using System;
using System.Collections.Generic;


namespace pocketl.diagn
{
    public interface Reporter
    {
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
            public Caret[] carets;
        }


        enum MessageKind
        {
            Error,
            Warning,
            Info,
            Lint
        }


        void Reporter.Error(string descr, params Caret[] carets)
        {
            this.messages.Add(new Message
            {
                kind = MessageKind.Error,
                descr = descr,
                carets = carets
            });
        }


        public void PrintToConsole(Context ctx)
        {
            foreach (var msg in this.messages)
                this.PrintMessageToConsole(ctx, msg);
        }


        void PrintMessageToConsole(Context ctx, Message msg)
        {
            Console.WriteLine(msg.descr);
        }
    }
}
