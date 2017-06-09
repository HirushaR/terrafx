// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;
using NUnit.Framework;

namespace TerraFX.Utilities.UnitTests
{
    /// <summary>Provides a set of tests covering the <see cref="ExceptionUtilities" /> static class.</summary>
    [TestFixture(Author = "Tanner Gooding", TestOf = typeof(ExceptionUtilities))]
    public static class ExceptionUtilitiesTests
    {
        #region Static Method Tests
        /// <summary>Provides validation of the <see cref="ExceptionUtilities.ThrowArgumentExceptionForInvalidType(string, Type)" /> static method.</summary>
        [Test]
        public static void ThrowArgumentExceptionForInvalidType(
            [Values(null, "", "param")] string paramName,
            [Values(typeof(object), typeof(string), typeof(int))] Type paramType
        )
        {
            Assert.That(() => ExceptionUtilities.ThrowArgumentExceptionForInvalidType(paramName, paramType),
                Throws.InstanceOf<ArgumentException>()
                      .With.Property("ParamName").EqualTo(paramName)
            );
        }

        /// <summary>Provides validation of the <see cref="ExceptionUtilities.ThrowArgumentNullException(string)" /> static method.</summary>
        [Test]
        public static void ThrowArgumentNullExceptionStringTest(
            [Values(null, "", "param")] string paramName
        )
        {
            Assert.That(() => ExceptionUtilities.ThrowArgumentNullException(paramName),
                Throws.InstanceOf<ArgumentNullException>()
                      .With.Property("ParamName").EqualTo(paramName)
            );
        }

        /// <summary>Provides validation of the <see cref="ExceptionUtilities.ThrowArgumentOutOfRangeException(string, object)" /> static method.</summary>
        [Test]
        public static void ThrowArgumentOutOfRangeExceptionStringObjectTest(
            [Values(null, "", "param")] string paramName,
            [Values(null, "", "value")] object value
        )
        {
            Assert.That(() => ExceptionUtilities.ThrowArgumentOutOfRangeException(paramName, value),
                Throws.InstanceOf<ArgumentOutOfRangeException>()
                      .With.Property("ParamName").EqualTo(paramName)
                      .And.With.Property("ActualValue").EqualTo(value)
            );
        }
        #endregion
    }
}