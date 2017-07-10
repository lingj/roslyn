﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Elfie.Model;

namespace Microsoft.CodeAnalysis.SymbolSearch
{
    // Wrapper types to ensure we delay load the elfie database.
    internal interface IAddReferenceDatabaseWrapper
    {
        AddReferenceDatabase Database { get; }
    }

    internal class AddReferenceDatabaseWrapper : IAddReferenceDatabaseWrapper
    {
        public AddReferenceDatabase Database { get; }

        public AddReferenceDatabaseWrapper(AddReferenceDatabase database)
        {
            Database = database;
        }
    }
}
