﻿// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using TinyCsvParser.Mapping;
using TinyCsvParser.Model;

namespace TinyCsvParser
{
    public class CsvParser<TEntity> : ICsvParser<TEntity>
        where TEntity : class, new()
    {
        private readonly CsvParserOptions options;
        private readonly CsvMapping<TEntity> mapping;

        public CsvParser(CsvParserOptions options, CsvMapping<TEntity> mapping)
        {
            this.options = options;
            this.mapping = mapping;
        }

        public ParallelQuery<CsvMappingResult<TEntity>> Parse(IEnumerable<Row> csvData)
        {
            if (csvData == null)
            {
                throw new ArgumentNullException("csvData");
            }

            // Get the Tokenizer:
            var tokenize = options.Tokenizer;
            
            var query = csvData
                // Skip the Header:
                .Skip(options.SkipHeader ? 1 : 0)
                // Switch into Parallel Execution:
                .AsParallel();

            // If you want to get the same order as in the CSV file, this option needs to be set:
            if (options.KeepOrder)
            {
                query = query.AsOrdered();
            }

            query = query
                // Define the Number of Threads to be used for Mapping:
                .WithDegreeOfParallelism(options.DegreeOfParallelism)
                // Ignore Empty Lines:
                .Where(row => !string.IsNullOrWhiteSpace(row.Data));

            // Ignore Lines, that start with a comment character:
            if(!string.IsNullOrWhiteSpace(options.CommentCharacter)) 
            {
                query = query.Where(row => !row.Data.StartsWith(options.CommentCharacter));
            }
            
            return query
                // Map the Data to the Entity:
                .Select(row => Map(row));
        }


        private CsvMappingResult<TEntity> Map(Row row)
        {
            string[] tokens;
            try
            {
                tokens = options.Tokenizer.Tokenize(row.Data);
            }
            catch (Exception exception)
            {
                // Early exit, if we cannot tokenize the line:
                return new CsvMappingResult<TEntity>
                {
                    RowIndex = row.Index,
                    Error = new CsvMappingError
                    {
                        Reason = ErrorReasonEnum.Tokenization,
                        Exception = exception,
                        Value = row.Data,
                        Message = string.Format("Tokenization failed")
                    }
                };
            }

            return mapping.Map(row.Index, tokens);
        }

        public override string ToString()
        {
            return string.Format("CsvParser (Options = {0}, Mapping = {1})", options, mapping);
        }
    }
}
