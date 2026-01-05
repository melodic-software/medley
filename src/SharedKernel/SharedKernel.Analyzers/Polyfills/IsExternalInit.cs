// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// This file is a polyfill for init-only properties in netstandard2.0.
// Required for C# 9+ record types when targeting netstandard2.0.
// Reference: https://github.com/dotnet/runtime/issues/96275

#if NETSTANDARD2_0

using System.ComponentModel;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved to be used by the compiler for tracking metadata.
/// This class should not be used by developers in source code.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IsExternalInit
{
}

#endif
