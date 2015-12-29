﻿using NUnit.Framework;
using System;
using TinyCsvParser.Exceptions;
using TinyCsvParser.TypeConverter;

namespace TinyCsvParser.Test.TypeConverter
{
    [TestFixture]
    public class TypeConverterProviderTests
    {
        TypeConverterProvider provider;

        [SetUp]
        public void SetUp()
        {
            provider = new TypeConverterProvider();
        }

        private enum SomeEnum
        {
            A = 1
        }

        [Test]
        public void AddTypeRegistrationTest()
        {
            var typeConverter = provider
                .Add(new EnumConverter<SomeEnum>())
                .Resolve<SomeEnum>();

            Assert.AreEqual(typeof(EnumConverter<SomeEnum>), typeConverter.GetType());
        }

        [Test]
        public void PreventDuplicateTypeRegistrationTest()
        {
            Assert.Throws<CsvTypeConverterAlreadyRegisteredException>(() => provider.Add(new Int32Converter()));
        }

        [Test]
        public void OverrideTypeRegistrationTest()
        {
            Assert.DoesNotThrow(() => provider.Override(new Int16Converter()));
        }

        [Test]
        public void ResolveTypeRegistrationTest()
        {
            var typeRegistration = provider.Resolve<Int16>();
            
            Assert.AreEqual(typeof(Int16Converter), typeRegistration.GetType());
        }
    }
}
