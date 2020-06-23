using System;
using System.Net;
using System.Net.Http;
using MockServerClientNet.Model;
using Xunit;
using static MockServerClientNet.Model.HttpRequest;
using static MockServerClientNet.Model.HttpResponse;

namespace MockServerClientNet.Tests
{
    public class ExpectationsTest : MockServerClientTest
    {
        [Fact]
        public void ShouldRespondAccordingToExpectation()
        {
            // arrange
            SetupPostExpectation();

            // act
            SendRequest(BuildPostRequest(), out var responseBody, out var statusCode);

            // assert
            Assert.NotNull(statusCode);
            Assert.Equal(HttpStatusCode.Created, statusCode.Value);
            Assert.Equal("{ \"id\": \"123\" }", responseBody);
        }

        [Fact]
        public void ShouldRespondAccordingToExpectationOnly2Times()
        {
            // arrange
            SetupPostExpectation(false, 2);

            // act 1
            SendRequest(BuildPostRequest(), out var responseBody, out var statusCode);

            // assert
            Assert.NotNull(statusCode);
            Assert.Equal(HttpStatusCode.Created, statusCode.Value);
            Assert.Equal("{ \"id\": \"123\" }", responseBody);

            // act 2
            SendRequest(BuildPostRequest(), out responseBody, out statusCode);

            // assert
            Assert.NotNull(statusCode);
            Assert.Equal(HttpStatusCode.Created, statusCode.Value);
            Assert.Equal("{ \"id\": \"123\" }", responseBody);

            // act 3
            SendRequest(BuildPostRequest(), out responseBody, out statusCode);

            // assert
            Assert.NotNull(statusCode);
            Assert.Equal(HttpStatusCode.NotFound, statusCode.Value);
        }

        [Fact]
        public void ShouldClearExpectation()
        {
            // arrange
            var request = Request().WithMethod("GET").WithPath("/hello");

            MockServerClient
                .When(request, Times.Unlimited())
                .Respond(Response().WithStatusCode(200).WithBody("hello").WithDelay(TimeSpan.FromSeconds(0)));

            // act 1
            SendRequest(BuildGetRequest("/hello"), out var responseBody, out var statusCode);

            // assert
            Assert.NotNull(statusCode);
            Assert.Equal(HttpStatusCode.OK, statusCode.Value);
            Assert.Equal("hello", responseBody);

            // act 2
            MockServerClient.Clear(request);

            SendRequest(BuildGetRequest("/hello"), out responseBody, out statusCode);

            // assert
            Assert.NotNull(statusCode);
            Assert.Equal(HttpStatusCode.NotFound, statusCode.Value);
        }

        [Fact]
        public void ShouldRetrieveRecordedRequests()
        {
            // arrange
            var request = Request().WithMethod("GET").WithPath("/hello");

            MockServerClient
                .When(request, Times.Unlimited())
                .Respond(Response().WithStatusCode(200).WithBody("hello").WithDelay(TimeSpan.FromSeconds(0)));

            // act
            SendRequest(BuildGetRequest("/hello"), out _, out _);
            SendRequest(BuildGetRequest("/hello"), out _, out _);

            var result = MockServerClient.RetrieveRecordedRequests(request);

            // assert
            Assert.Equal(2, result.Length);
        }

        private void SetupPostExpectation(bool unlimited = true, int times = 0)
        {
            MockServerClient
                .When(Request()
                        .WithMethod("POST")
                        .WithPath("/customers")
                        .WithQueryStringParameters(
                            new Parameter("param", "value"))
                        .WithBody("{\"name\": \"foo\"}"),
            unlimited ? Times.Unlimited() : Times.Exactly(times))
                .Respond(Response()
                    .WithStatusCode(201)
                    .WithHeaders(new Header("Content-Type", "application/json"))
                    .WithBody("{ \"id\": \"123\" }")
                    .WithDelay(TimeSpan.FromSeconds(0)));
        }

        private HttpRequestMessage BuildPostRequest()
        {
            return BuildRequest(HttpMethod.Post,
                "/customers?param=value",
                "{\"name\": \"foo\"}");
        }
    }
}