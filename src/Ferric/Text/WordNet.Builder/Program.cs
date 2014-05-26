﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using EntityFramework.BulkInsert.Extensions;
using Ferric.Text.WordNet.Data;

namespace Ferric.Text.WordNet.Builder
{
    class Program
    {
        const string BasePath = @"..\..\..\..\..\..\data\english\wordnet";

        static void Main(string[] args)
        {
            var program = new Program();
            program.BuildWordNet();
        }

        void BuildWordNet()
        {
            try
            {
                // read data
                Console.WriteLine("reading...");
                var info = new BuilderInfo();

                LoadData(info);

                // add to db
                var csBuilder = new SqlConnectionStringBuilder();
                csBuilder.DataSource = @"(LocalDB)\v11.0";
                csBuilder.AttachDBFilename = Path.GetFullPath(Path.Combine(BasePath, @"WordNet.mdf"));
                csBuilder.IntegratedSecurity = true;

                Console.WriteLine("connecting to " + csBuilder.ConnectionString);
                using (var context = new Data.WordNet(csBuilder.ConnectionString))
                {
                    // clear the db
                    Console.WriteLine("clearing...");
                    context.Database.Delete();
                    context.Database.Create();

                    Console.WriteLine("saving...");
                    using (var transactionScope = new TransactionScope())
                    {
                        var options = new BulkInsertOptions
                        {
                            EnableStreaming = true,
                            NotifyAfter = 10000,
                            Callback = (s, e) => { Console.WriteLine(e.RowsCopied.ToString()); }
                        };

                        context.BulkInsert(info.Synsets.Values, options);
                        context.BulkInsert(info.Synsets.Values.SelectMany(s => s.Senses));
                        context.SaveChanges();

                        Console.WriteLine("relations...");
                        context.Database.Connection.Open();

                        SaveRelations(info, context);

                        transactionScope.Complete();
                    }
                }
  
                // write lemmas
                WriteLemmas(info);
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();
                sb.Append("EXCEPTION: ");
                PrintException(sb, e);
                Console.Write(sb.ToString());
                Console.WriteLine();
            }
        }

        private void LoadData(BuilderInfo info)
        {
            Load<LoaderS>("wn_s.pl", info);
            Load<LoaderSk>("wn_sk.pl", info);
            Load<LoaderG>("wn_g.pl", info);
            Load<LoaderSyntax>("wn_syntax.pl", info);
            Load<LoaderHyp>("wn_hyp.pl", info);
            Load<LoaderIns>("wn_ins.pl", info);
            Load<LoaderEnt>("wn_ent.pl", info);
            Load<LoaderSim>("wn_sim.pl", info);
            Load<LoaderMm>("wn_mm.pl", info);
            Load<LoaderMs>("wn_ms.pl", info);
            Load<LoaderMp>("wn_mp.pl", info);
            Load<LoaderDer>("wn_der.pl", info);
        }

        private void SaveRelations(BuilderInfo info, Data.WordNet context)
        {
            var synsets = info.Synsets.Values;

            SaveRelation<Synset>(context, synsets, "Hypernyms");
            SaveRelation<Synset>(context, synsets, "Hyponyms");
            SaveRelation<Synset>(context, synsets, "Prototypes");
            SaveRelation<Synset>(context, synsets, "Instances");
            SaveRelation<Synset>(context, synsets, "Entailments");
            SaveRelation<Synset>(context, synsets, "Satellites");
            SaveRelation<Synset>(context, synsets, "MemberMeronyms");
            SaveRelation<Synset>(context, synsets, "MemberHolonyms");
            SaveRelation<Synset>(context, synsets, "SubstanceMeronyms");
            SaveRelation<Synset>(context, synsets, "SubstanceHolonyms");
            SaveRelation<Synset>(context, synsets, "PartMeronyms");
            SaveRelation<Synset>(context, synsets, "PartHolonyms");
            SaveRelation<Synset>(context, synsets, "Derivations");
        }

        static Type[] CtorTypes = new[] { typeof(TextReader), typeof(BuilderInfo) };

        void Load<T>(string fname, BuilderInfo info)
            where T : Loader
        {
            var type = typeof(T);
            var ctor = type.GetConstructor(CtorTypes);
            if (ctor == null)
                throw new Exception("Unable to get constructor for " + type.FullName);

            using (var tr = new StreamReader(Path.GetFullPath(Path.Combine(BasePath, fname))))
            {
                var loader = ctor.Invoke(new object[] { tr, info }) as Loader;
                loader.Load();
            }
        }

        void SaveRelation<T>(WordNet.Data.WordNet context, IEnumerable<T> source, string destPropertyName)
        {
            Console.Write(destPropertyName);
            Console.Write(": ");

            using (var bc = new SqlBulkCopy(context.Database.Connection as SqlConnection))
            {
                var dr = new RelationReader<T>(source, destPropertyName);

                bc.DestinationTableName = destPropertyName;
                bc.EnableStreaming = true;
                bc.WriteToServer(dr);

                Console.WriteLine(dr.RecordsAffected);
            }
        }

        void WriteLemmas(BuilderInfo builderInfo)
        {
            Console.WriteLine("writing lemmas...");
            using (var tr = new StreamWriter(Path.GetFullPath(Path.Combine(BasePath, "lemmas.txt"))))
            {
                foreach (var lemma in builderInfo.Lemmas)
                    tr.WriteLine(lemma);
            }
        }

        void PrintException(StringBuilder sb, Exception e, string indent = null)
        {
            if (indent == null)
                indent = "";

            sb.AppendFormat("{0}{1}", indent, e.Message);
            sb.AppendLine();

            var aggregate = e as AggregateException;
            if (aggregate != null)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    PrintException(sb, inner, indent + "  ");
            }

            if (e.InnerException != null)
                PrintException(sb, e.InnerException, indent + "  ");
        }
    }
}
