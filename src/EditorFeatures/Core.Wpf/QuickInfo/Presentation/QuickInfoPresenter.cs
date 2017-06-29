﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.Editor.QuickInfo;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense.QuickInfo.Presentation
{
    [Export(typeof(IQuickInfoSourceProvider))]
    [Export(typeof(IIntelliSensePresenter<IQuickInfoPresenterSession>))]
    [Order]
    [Name(PredefinedQuickInfoPresenterNames.RoslynQuickInfoPresenter)]
    [ContentType(ContentTypeNames.RoslynContentType)]
    internal partial class QuickInfoPresenter : ForegroundThreadAffinitizedObject, IIntelliSensePresenter<IQuickInfoPresenterSession, IQuickInfoSession>, IQuickInfoSourceProvider
    {
        private static readonly object s_augmentSessionKey = new object();

        private readonly IQuickInfoBroker _quickInfoBroker;
        private readonly DeferredContentFrameworkElementFactory _elementFactory;

        [ImportingConstructor]
        public QuickInfoPresenter(IQuickInfoBroker quickInfoBroker, DeferredContentFrameworkElementFactory elementFactory)
        {
            _quickInfoBroker = quickInfoBroker;
            _elementFactory = elementFactory;
        }

        IQuickInfoPresenterSession IIntelliSensePresenter<IQuickInfoPresenterSession>.CreateSession(ITextView textView, ITextBuffer subjectBuffer)
        {
            AssertIsForeground();
            return new QuickInfoPresenterSession(_quickInfoBroker, textView, subjectBuffer);
        }

        IQuickInfoPresenterSession IIntelliSensePresenter<IQuickInfoPresenterSession, IQuickInfoSession>.CreateSession(ITextView textView, ITextBuffer subjectBuffer, IQuickInfoSession session)
        {
            AssertIsForeground();
            return new QuickInfoPresenterSession(_quickInfoBroker, _elementFactory, textView, subjectBuffer, session);
        }

        IQuickInfoSource IQuickInfoSourceProvider.TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            AssertIsForeground();
            return new QuickInfoSource();
        }
    }
}
