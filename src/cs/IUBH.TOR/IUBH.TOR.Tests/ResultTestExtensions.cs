using System;
using IUBH.TOR.Domain;
using Shouldly;

namespace IUBH.TOR.Tests
{
    public static class ResultTestExtensions
    {
        public static void ShouldNotBeSuccessful(this Result result)
        {
            result.IsSuccessful.ShouldBeFalse();
        }

        public static void ShouldBeSuccessful(this Result result)
        {
            if (result.IsSuccessful)
            {
                return;
            }

            if (result.Exception != null)
            {
                throw result.Exception;
            }

            throw new Exception("Operation not successful: " + result.ErrorMessage);
        }
    }
}
