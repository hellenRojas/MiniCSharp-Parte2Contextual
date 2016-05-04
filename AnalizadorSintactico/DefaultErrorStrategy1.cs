using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalisisSintactico
{
    class DefaultErrorStrategy1 : DefaultErrorStrategy
    {
        protected override void ReportUnwantedToken(Parser recognizer)
        {
            if (InErrorRecoveryMode(recognizer))
            {
                return;
            }
            BeginErrorCondition(recognizer);
            IToken to = recognizer.CurrentToken;
            string nombretoken = GetTokenErrorDisplay(to);
            IntervalSet expecting = GetExpectedTokens(recognizer);
            string msg = "Entrada no reconocida con " + nombretoken + ", se esperaba " + expecting.ToString(recognizer.TokenNames);
            recognizer.NotifyErrorListeners(to, msg, null);
        }

        protected override void ReportMissingToken(Parser recognizer)
        {
            if (InErrorRecoveryMode(recognizer))
            {
                return;
            }
            BeginErrorCondition(recognizer);
            IToken to = recognizer.CurrentToken;
            IntervalSet expecting = GetExpectedTokens(recognizer);
            string msg = "Se esperaba " + expecting.ToString(recognizer.TokenNames) + " pero viene " + GetTokenErrorDisplay(to);
            recognizer.NotifyErrorListeners(to, msg, null);
        }


    }
}
