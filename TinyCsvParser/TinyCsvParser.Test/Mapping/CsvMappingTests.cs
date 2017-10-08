﻿// Copyright (c) Philipp Wagner. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using NUnit.Framework;
using TinyCsvParser.Mapping;
using TinyCsvParser.Model;

namespace TinyCsvParser.Test.Mapping
{

    [TestFixture]
    public class CsvMappingTests
    {
        private class SampleEntity
        {
            public int PropertyInt { get; set; }
        }

        private class DuplicateMapping : CsvMapping<SampleEntity>
        {
            public DuplicateMapping()
            {
                MapProperty(0, x => x.PropertyInt);
                MapProperty(0, x => x.PropertyInt);
            }
        }
        
        [Test]
        public void DuplicateMappingTest()
        {
            Assert.Throws<InvalidOperationException>(() => new DuplicateMapping());
        }

        private class WrongColumnMapping : CsvMapping<SampleEntity>
        {
            public WrongColumnMapping()
            {
                MapProperty(1, x => x.PropertyInt);
            }
        }

        [Test]
        public void MapEntity_Invalid_Column_Test()
        {
            var mapping = new WrongColumnMapping();

            var result = mapping.Map(1, new []{"1"});

            Assert.IsFalse(result.IsValid);
        }

        private class CorrectColumnMapping : CsvMapping<SampleEntity>
        {
            public CorrectColumnMapping()
            {
                MapProperty(0, x => x.PropertyInt);
            }
        }
        

        [Test]
        public void MapEntity_ConversionError_Test()
        {
            var mapping = new CorrectColumnMapping();

            var result = mapping.Map(1, new[] { string.Empty });

            Assert.IsFalse(result.IsValid);
            Assert.AreEqual(ErrorReasonEnum.Conversion, result.Error.Reason);
            Assert.AreEqual(string.Empty, result.Error.Value);
            Assert.AreEqual("Column 0 with Value '' cannot be converted", result.Error.Message);
            Assert.AreEqual(0, result.Error.ColumnIndex);

            Assert.DoesNotThrow(() => result.ToString());
        }

        [Test]
        public void MapEntity_ConversionSuccess_Test()
        {
            var mapping = new CorrectColumnMapping();

            var result = mapping.Map(1, new[] { "1" });

            Assert.IsTrue(result.IsValid);

            Assert.IsNull(result.Error);

            Assert.AreEqual(1, result.Result.PropertyInt);

            Assert.DoesNotThrow(() => result.ToString());
        }
    }
}
