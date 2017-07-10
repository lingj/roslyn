﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BuildBoss
{
    internal static class SharedUtil
    {
        internal static string MSBuildNamespaceUriRaw => "http://schemas.microsoft.com/developer/msbuild/2003";
        internal static Uri MSBuildNamespaceUri { get; } = new Uri(MSBuildNamespaceUriRaw);
        internal static XNamespace MSBuildNamespace { get; } = XNamespace.Get(MSBuildNamespaceUriRaw);
        internal static Encoding Encoding { get; } = Encoding.UTF8;

        internal static bool IsSolutionFile(string path) => Path.GetExtension(path) == ".sln";
        internal static bool IsPropsFile(string path) => Path.GetExtension(path) == ".props";
        internal static bool IsTargetsFile(string path) => Path.GetExtension(path) == ".targets";
        internal static bool IsXslt(string path) => Path.GetExtension(path) == ".xslt";
    }
}
