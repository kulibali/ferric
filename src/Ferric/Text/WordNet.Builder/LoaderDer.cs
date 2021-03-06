﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ferric.Text.WordNet.Data;

namespace Ferric.Text.WordNet.Builder
{
    class LoaderDer : Loader
    {
        static readonly Regex reg = new Regex(@"der\( 
                                                    (?<synset_id1>\d\d\d\d\d\d\d\d\d),
                                                    (?<w_num1>\d+),
                                                    (?<synset_id2>\d\d\d\d\d\d\d\d\d),
                                                    (?<w_num2>\d+)
                                                \)\.", RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);

        public LoaderDer(TextReader tr, BuilderInfo info)
            : base("derivations...", reg, tr, info)
        {
        }

        protected override void ProcessLine(System.Text.RegularExpressions.Match match)
        {
            var synset_id1 = GetValue<int>(match, "synset_id1");
            var w_num1 = GetValue<int>(match, "w_num1");
            var synset_id2 = GetValue<int>(match, "synset_id2");
            var w_num2 = GetValue<int>(match, "w_num2");

            var ws1 = GetWordSense(synset_id1, w_num1);
            var ws2 = GetWordSense(synset_id2, w_num2);

            if (ws1.Derivations == null) ws1.Derivations = new List<WordSense>();
            if (ws2.Derivations == null) ws2.Derivations = new List<WordSense>();

            if (ws1.Derivations.All(ws => ws.WordSenseId != ws2.WordSenseId))
                ws1.Derivations.Add(ws2);

            if (ws2.Derivations.All(ws => ws.WordSenseId != ws1.WordSenseId))
                ws2.Derivations.Add(ws1);
        }
    }
}
