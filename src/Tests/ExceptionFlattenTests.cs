using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.UniLogger
{
    public class ExceptionFlattenTests
    {
        private static List<Exception> Flatten(Exception exception)
        {
            var list = new List<Exception>();
            ULogger.FlattenForLog(exception, list);
            return list;
        }

        [Test]
        public void WhenPlainException_ThanReturnedAsIs()
        {
            var exception = new InvalidOperationException("x");
            Flatten(exception).Should().ContainSingle().Which.Should().BeSameAs(exception);
        }

        [Test]
        public void WhenAggregateWithManyInners_ThanEachInnerReturned()
        {
            var a = new Exception("a");
            var b = new Exception("b");
            Flatten(new AggregateException(a, b)).Should().Equal(a, b);
        }

        [Test]
        public void WhenNestedAggregates_ThanFlattenedToLeaves()
        {
            var a = new Exception("a");
            var b = new Exception("b");
            var c = new Exception("c");
            var result = Flatten(new AggregateException(new AggregateException(a, b), c));
            result.Should().HaveCount(3);
            result.Should().Contain(a).And.Contain(b).And.Contain(c);
        }

        [Test]
        public void WhenAggregateWithSingleInner_ThanInnerReturned()
        {
            var a = new Exception("a");
            Flatten(new AggregateException(a)).Should().ContainSingle().Which.Should().BeSameAs(a);
        }

        [Test]
        public void WhenEmptyAggregate_ThanAggregateItselfReturned()
        {
            var aggregate = new AggregateException();
            Flatten(aggregate).Should().ContainSingle().Which.Should().BeSameAs(aggregate);
        }
    }
}
