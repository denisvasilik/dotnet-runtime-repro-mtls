# .NET Runtime mTLS Repro

Using an mTLS setup and getting a list of `acceptableIssuers` from the
`LocalCertificateSelectionCallback` at the client application works great on
Windows, but fails on Linux. This is a sample application that reproduces that
issue for the following system configuration.

* Ubuntu 20.04
* .NET 5.0.201
* OpenSSL 1.1.1f  31 Mar 2020

## Quick Analysis

During debugging I figured out that on Windows the method
[InitializeSecurityContext](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Security/src/System/Net/Security/SslStreamPal.Windows.cs#L83)
 returns
`SecurityStatusPalErrorCode.CredentialsNeeded` (when appropriate). As a consequence,
the `LocalCertificateSelectionCallback` is called a second time with proper content
of acceptable issuers. When looking at the
[InitializeSecurityContext](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Security/src/System/Net/Security/SslStreamPal.Unix.cs#L33)
or [HandshakeInternal](https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Security/src/System/Net/Security/SslStreamPal.Unix.cs#L99)
routine on Linux, it never returns
`SecurityStatusPalErrorCode.CredentialsNeeded`. Instead it returns
`SecurityStatusPalErrorCode.ContinueNeeded` which does not trigger
`LocalCertificateSelectionCallback`. Hence, there's no second invocation of
`LocalCertificateSelectionCallback`.

## Issue

This issue is discussed here:

* [Unable to get acceptableIssuers from LocalCertificateSelectionCallback on Linux](https://www.github.com/)
