﻿' Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.VisualStudio.Text
Imports Microsoft.VisualStudio.Text.Editor

Namespace Microsoft.CodeAnalysis.Editor.UnitTests.IntelliSense

    Friend Class TestSignatureHelpPresenter
        Implements IIntelliSensePresenter(Of ISignatureHelpPresenterSession)

        Private ReadOnly _testState As IIntelliSenseTestState

        Public Sub New(testState As IIntelliSenseTestState)
            Me._testState = testState
        End Sub

        Public Function CreateSession(textView As ITextView, subjectBuffer As ITextBuffer) As ISignatureHelpPresenterSession _
            Implements IIntelliSensePresenter(Of ISignatureHelpPresenterSession).CreateSession
            Return New TestSignatureHelpPresenterSession(_testState)
        End Function
    End Class
End Namespace
