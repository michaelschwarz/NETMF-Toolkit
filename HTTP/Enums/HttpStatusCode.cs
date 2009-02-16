﻿/* 
 * HttpStatusCode.cs
 * 
 * Copyright (c) 2009, Michael Schwarz (http://www.schwarz-interactive.de)
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR
 * ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * MS   09-02-10    added all http status codes
 * 
 * 
 */
using System;

namespace MSchwarz.Net.Web
{
    /// <summary>
    /// The HttpStatusCode enumeration contains the values of the status codes defined in RFC 2616 for HTTP 1.1.
    /// </summary>
    public enum HttpStatusCode
    {
        /// <summary>
        /// Equivalent to HTTP status 100. Continue indicates that the client can continue with its request. 
        /// </summary>
        Continue = 100,

        /// <summary>
        /// Equivalent to HTTP status 101. SwitchingProtocols indicates that the protocol version or protocol is being changed. 
        /// </summary>
        SwitchingProtocols = 101,

        /// <summary>
        /// Equivalent to HTTP status 200. OK indicates that the request succeeded and that the requested information is in the response. This is the most common status code to receive. 
        /// </summary>
        OK = 200,

        /// <summary>
        /// Equivalent to HTTP status 201. Created indicates that the request resulted in a new resource created before the response was sent. 
        /// </summary>
        Created = 201,

        /// <summary>
        /// Equivalent to HTTP status 202. Accepted indicates that the request has been accepted for further processing. 
        /// </summary>
        Accepted = 202,

        /// <summary>
        /// Equivalent to HTTP status 203. NonAuthoritativeInformation indicates that the returned metainformation is from a cached copy instead of the origin server and therefore may be incorrect. 
        /// </summary>
        NonAuthoritativeInformation = 203,

        /// <summary>
        /// Equivalent to HTTP status 204. NoContent indicates that the request has been successfully processed and that the response is intentionally blank. 
        /// </summary>
        NoContent = 204,

        /// <summary>
        /// Equivalent to HTTP status 205. ResetContent indicates that the client should reset (not reload) the current resource. 
        /// </summary>
        ResetContent = 205,

        /// <summary>
        /// Equivalent to HTTP status 206. PartialContent indicates that the response is a partial response as requested by a GET request that includes a byte range. 
        /// </summary>
        PartialContent = 206,

        /// <summary>
        /// Equivalent to HTTP status 300. MultipleChoices indicates that the requested information has multiple representations. The default action is to treat this status as a redirect and follow the contents of the Location header associated with this response. 
        /// </summary>
        MultipleChoices = 300,

        /// <summary>
        /// Equivalent to HTTP status 301. MovedPermanently indicates that the requested information has been moved to the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. 
        /// </summary>
        MovedPermanently = 301,

        /// <summary>
        /// Equivalent to HTTP status 302. Found indicates that the requested information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will use the GET method. 
        /// </summary>
        Found = 302,

        /// <summary>
        /// Equivalent to HTTP status 303. SeeOther automatically redirects the client to the URI specified in the Location header as the result of a POST. The request to the resource specified by the Location header will be made with a GET. 
        /// </summary>
        SeeOther = 303,

        /// <summary>
        /// Equivalent to HTTP status 304. NotModified indicates that the client's cached copy is up to date. The contents of the resource are not transferred. 
        /// </summary>
        NotModified = 304,

        /// <summary>
        /// Equivalent to HTTP status 305. UseProxy indicates that the request should use the proxy server at the URI specified in the Location header. 
        /// </summary>
        UseProxy = 305,

        /// <summary>
        /// Equivalent to HTTP status 307. TemporaryRedirect indicates that the request information is located at the URI specified in the Location header. The default action when this status is received is to follow the Location header associated with the response. When the original request method was POST, the redirected request will also use the POST method. 
        /// </summary>
        TemporaryRedirect = 307,

        /// <summary>
        /// Equivalent to HTTP status 400. BadRequest indicates that the request could not be understood by the server. BadRequest is sent when no other error is applicable, or if the exact error is unknown or does not have its own error code. 
        /// </summary>
        BadRequest = 400,

        /// <summary>
        /// Equivalent to HTTP status 401. Unauthorized indicates that the requested resource requires authentication. The WWW-Authenticate header contains the details of how to perform the authentication. 
        /// </summary>
        Unauthorized = 401,

        /// <summary>
        /// Equivalent to HTTP status 402. PaymentRequired is reserved for future use. 
        /// </summary>
        PaymentRequired = 402,

        /// <summary>
        /// Equivalent to HTTP status 403. Forbidden indicates that the server refuses to fulfill the request. 
        /// </summary>
        Forbidden = 403,

        /// <summary>
        /// Equivalent to HTTP status 404. NotFound indicates that the requested resource does not exist on the server. 
        /// </summary>
        NotFound = 404,

        /// <summary>
        /// Equivalent to HTTP status 405. MethodNotAllowed indicates that the request method (POST or GET) is not allowed on the requested resource. 
        /// </summary>
        MethodNotAllowed = 405,

        /// <summary>
        /// Equivalent to HTTP status 406. NotAcceptable indicates that the client has indicated with Accept headers that it will not accept any of the available representations of the resource. 
        /// </summary>
        NotAcceptable = 406,

        /// <summary>
        /// Equivalent to HTTP status 407. ProxyAuthenticationRequired indicates that the requested proxy requires authentication. The Proxy-authenticate header contains the details of how to perform the authentication. 
        /// </summary>
        ProxyAuthenticationRequired = 407,

        /// <summary>
        /// Equivalent to HTTP status 408. RequestTimeout indicates that the client did not send a request within the time the server was expecting the request. 
        /// </summary>
        RequestTimeout = 408,

        /// <summary>
        /// Equivalent to HTTP status 409. Conflict indicates that the request could not be carried out because of a conflict on the server. 
        /// </summary>
        Conflict = 409,

        /// <summary>
        /// Equivalent to HTTP status 410. Gone indicates that the requested resource is no longer available. 
        /// </summary>
        Gone = 410,

        /// <summary>
        /// Equivalent to HTTP status 411. LengthRequired indicates that the required Content-length header is missing. 
        /// </summary>
        LengthRequired = 411,

        /// <summary>
        /// Equivalent to HTTP status 412. PreconditionFailed indicates that a condition set for this request failed, and the request cannot be carried out. Conditions are set with conditional request headers like If-Match, If-None-Match, or If-Unmodified-Since. 
        /// </summary>
        PreconditionFailed = 412,

        /// <summary>
        /// Equivalent to HTTP status 413. RequestEntityTooLarge indicates that the request is too large for the server to process. 
        /// </summary>
        RequestEntitiyTooLarge = 413,

        /// <summary>
        /// Equivalent to HTTP status 414. RequestUriTooLong indicates that the URI is too long. 
        /// </summary>
        RequestUriTooLong = 414,

        /// <summary>
        /// Equivalent to HTTP status 415. UnsupportedMediaType indicates that the request is an unsupported type. 
        /// </summary>
        UnsupportedMediaType = 415,

        /// <summary>
        /// Equivalent to HTTP status 416. RequestedRangeNotSatisfiable indicates that the range of data requested from the resource cannot be returned, either because the beginning of the range is before the beginning of the resource, or the end of the range is after the end of the resource. 
        /// </summary>
        RequestedRangeNotSatisfiable = 416,

        /// <summary>
        /// Equivalent to HTTP status 417. ExpectationFailed indicates that an expectation given in an Expect header could not be met by the server. 
        /// </summary>
        ExpectationFailed = 417,

        /// <summary>
        /// Equivalent to HTTP status 500. InternalServerError indicates that a generic error has occurred on the server. 
        /// </summary>
        InternalServerError = 500,

        /// <summary>
        /// Equivalent to HTTP status 501. NotImplemented indicates that the server does not support the requested function. 
        /// </summary>
        NotImplemented = 501,

        /// <summary>
        /// Equivalent to HTTP status 502. BadGateway indicates that an intermediate proxy server received a bad response from another proxy or the origin server. 
        /// </summary>
        BadGateway = 502,

        /// <summary>
        /// Equivalent to HTTP status 503. ServiceUnavailable indicates that the server is temporarily unavailable, usually due to high load or maintenance. 
        /// </summary>
        ServiceUnavailable = 503,

        /// <summary>
        /// Equivalent to HTTP status 504. GatewayTimeout indicates that an intermediate proxy server timed out while waiting for a response from another proxy or the origin server. 
        /// </summary>
        GatewayTimeout = 504,

        /// <summary>
        /// Equivalent to HTTP status 505. HttpVersionNotSupported indicates that the requested HTTP version is not supported by the server. 
        /// </summary>
        HttpVersionNotSupported = 505
    }

    internal class HttpStatusHelper
    {
        public static string GetHttpStatusFromCode(HttpStatusCode status)
        {
            switch (status)
            {
                case HttpStatusCode.Continue:
                    return "Continue";

                case HttpStatusCode.OK:
                    return "OK";

                case HttpStatusCode.MovedPermanently:
                    return "MovedPermanently";

                case HttpStatusCode.NotModified:
                    return "NotModified";
        
                case HttpStatusCode.Forbidden:
                    return "Forbidden";

                case HttpStatusCode.NotFound:
                    return "NotFound";

                case HttpStatusCode.MethodNotAllowed:
                    return "MethodNotAllowed";

                case HttpStatusCode.InternalServerError:
                    return "InternalServerError";

                case HttpStatusCode.NotImplemented:
                    return "NotImplemented";

                case HttpStatusCode.ServiceUnavailable:
                    return "ServiceUnavailable";

                case HttpStatusCode.HttpVersionNotSupported:
                    return "HttpVersionNotSupported";

                default:
                    return status.ToString();
            }
        }
    }
}
