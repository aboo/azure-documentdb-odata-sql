// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// https://github.com/aspnet/HttpAbstractions/blob/master/src/Microsoft.AspNetCore.Http/Internal/DefaultHttpRequest.cs

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Net.Http.Headers;

/// <summary>
/// Class DefaultHttpRequest.
/// Implements the <see cref="Microsoft.AspNetCore.Http.HttpRequest" />
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Http.HttpRequest" />
public class DefaultHttpRequest : HttpRequest
{
	private static readonly Func<IFeatureCollection, IHttpRequestFeature> _nullRequestFeature = f => null;
	private static readonly Func<IFeatureCollection, IQueryFeature> _newQueryFeature = f => new QueryFeature(f);
	private static readonly Func<HttpRequest, IFormFeature> _newFormFeature = r => new FormFeature(r);
	private static readonly Func<IFeatureCollection, IRequestCookiesFeature> _newRequestCookiesFeature = f => new RequestCookiesFeature(f);

	private HttpContext _context;
	private FeatureReferences<FeatureInterfaces> _features;

	/// <summary>
	/// Initializes a new instance of the <see cref="DefaultHttpRequest"/> class.
	/// </summary>
	/// <param name="context">The context.</param>
	public DefaultHttpRequest(HttpContext context)
	{
		Initialize(context);
	}

	/// <inheritdoc />
	public override Stream Body
	{
		get { return HttpRequestFeature.Body; }
		set { HttpRequestFeature.Body = value; }
	}

	/// <inheritdoc />
	public override long? ContentLength
	{
		get { return Headers.ContentLength; }
		set { Headers.ContentLength = value; }
	}

	/// <inheritdoc />
	public override string ContentType
	{
		get { return Headers[HeaderNames.ContentType]; }
		set { Headers[HeaderNames.ContentType] = value; }
	}

	/// <inheritdoc />
	public override IRequestCookieCollection Cookies
	{
		get { return RequestCookiesFeature.Cookies; }
		set { RequestCookiesFeature.Cookies = value; }
	}

	/// <inheritdoc />
	public override IFormCollection Form
	{
		get { return FormFeature.ReadForm(); }
		set { FormFeature.Form = value; }
	}

	/// <inheritdoc />
	public override bool HasFormContentType
	{
		get { return FormFeature.HasFormContentType; }
	}

	/// <inheritdoc />
	public override IHeaderDictionary Headers
	{
		get { return HttpRequestFeature.Headers; }
	}

	/// <inheritdoc />
	public override HostString Host
	{
		get { return HostString.FromUriComponent(Headers["Host"]); }
		set { Headers["Host"] = value.ToUriComponent(); }
	}

	/// <inheritdoc />
	public override HttpContext HttpContext => _context;

	/// <inheritdoc />
	public override bool IsHttps
	{
		get { return string.Equals("https", Scheme, StringComparison.OrdinalIgnoreCase); }
		set { Scheme = value ? "https" : "http"; }
	}

	/// <inheritdoc />
	public override string Method
	{
		get { return HttpRequestFeature.Method; }
		set { HttpRequestFeature.Method = value; }
	}

	/// <inheritdoc />
	public override PathString Path
	{
		get { return new PathString(HttpRequestFeature.Path); }
		set { HttpRequestFeature.Path = value.Value; }
	}

	/// <inheritdoc />
	public override PathString PathBase
	{
		get { return new PathString(HttpRequestFeature.PathBase); }
		set { HttpRequestFeature.PathBase = value.Value; }
	}

	/// <inheritdoc />
	public override string Protocol
	{
		get { return HttpRequestFeature.Protocol; }
		set { HttpRequestFeature.Protocol = value; }
	}

	/// <inheritdoc />
	public override IQueryCollection Query
	{
		get { return QueryFeature.Query; }
		set { QueryFeature.Query = value; }
	}

	/// <inheritdoc />
	public override QueryString QueryString
	{
		get { return new QueryString(HttpRequestFeature.QueryString); }
		set { HttpRequestFeature.QueryString = value.Value; }
	}

	/// <inheritdoc />
	public override string Scheme
	{
		get { return HttpRequestFeature.Scheme; }
		set { HttpRequestFeature.Scheme = value; }
	}

	private IHttpRequestFeature HttpRequestFeature =>
		_features.Fetch(ref _features.Cache.Request, _nullRequestFeature);

	private IQueryFeature QueryFeature =>
		_features.Fetch(ref _features.Cache.Query, _newQueryFeature);

	private IFormFeature FormFeature =>
		_features.Fetch(ref _features.Cache.Form, this, _newFormFeature);

	private IRequestCookiesFeature RequestCookiesFeature =>
		_features.Fetch(ref _features.Cache.Cookies, _newRequestCookiesFeature);

	/// <inheritdoc />
	public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = default)
	{
		return FormFeature.ReadFormAsync(cancellationToken);
	}

	/// <summary>
	/// Initializes the specified context.
	/// </summary>
	/// <param name="context">The context.</param>
	public virtual void Initialize(HttpContext context)
	{
		_context = context;
		_features = new FeatureReferences<FeatureInterfaces>(context.Features);
	}

	/// <summary>
	/// Uninitializes this instance.
	/// </summary>
	public virtual void Uninitialize()
	{
		_context = null;
		_features = default(FeatureReferences<FeatureInterfaces>);
	}

	private struct FeatureInterfaces
	{
		public IHttpRequestFeature Request;
		public IQueryFeature Query;
		public IFormFeature Form;
		public IRequestCookiesFeature Cookies;
	}
}
