﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.CodeConverter.Tests.TestRunners;
using Xunit;

namespace ICSharpCode.CodeConverter.Tests.CSharp.ExpressionTests
{
    public class ByRefTests : ConverterTestBase
    {

        [Fact]
        public async Task OptionalRefDateConstsWithOmittedArgListAsync()
        {
            await TestConversionVisualBasicToCSharpAsync(@"Public Class Issue213
    Const x As Date = #1990-1-1#

    Private Sub Y(Optional ByRef opt As Date = x)
    End Sub

    Private Sub CallsY()
        Y
    End Sub
End Class", @"using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public partial class Issue213
{
    private static DateTime x = DateTime.Parse(""1990-01-01"");

    private void Y([Optional, DateTimeConstant(627667488000000000/* Global.Issue213.x */)] ref DateTime opt)
    {
    }

    private void CallsY()
    {
        DateTime argopt = DateTime.Parse(""1990-01-01"");
        Y(opt: ref argopt);
    }
}");
        }

        [Fact]
        public async Task NullInlineRefArgumentAsync()
        {
            await TestConversionVisualBasicToCSharpAsync(@"Public Class VisualBasicClass
  Public Sub UseStuff()
    Stuff(Nothing)
  End Sub

  Public Sub Stuff(ByRef strs As String())
  End Sub
End Class", @"
public partial class VisualBasicClass
{
    public void UseStuff()
    {
        string[] argstrs = null;
        Stuff(ref argstrs);
    }

    public void Stuff(ref string[] strs)
    {
    }
}");
        }

        [Fact]
        public async Task RefArgumentRValueAsync()
        {
            await TestConversionVisualBasicToCSharpAsync(@"Public Class Class1
    Private Property C1 As Class1
    Private _c2 As Class1
    Private _o1 As Object

    Sub Foo()
        Bar(New Class1)
        Bar(C1)
        Bar(Me.C1)
        Bar(_c2)
        Bar(Me._c2)
        Bar(_o1)
        Bar(Me._o1)
    End Sub

    Sub Bar(ByRef class1)
    End Sub
End Class", @"
public partial class Class1
{
    private Class1 C1 { get; set; }

    private Class1 _c2;
    private object _o1;

    public void Foo()
    {
        object argclass1 = new Class1();
        Bar(ref argclass1);
        object argclass11 = C1;
        Bar(ref argclass11);
        object argclass12 = C1;
        Bar(ref argclass12);
        object argclass13 = _c2;
        Bar(ref argclass13);
        object argclass14 = _c2;
        Bar(ref argclass14);
        Bar(ref _o1);
        Bar(ref _o1);
    }

    public void Bar(ref object class1)
    {
    }
}");
        }

        [Fact]
        public async Task RefArgumentRValue2Async()
        {
            await TestConversionVisualBasicToCSharpAsync(@"Public Class Class1
    Sub Foo()
        Dim x = True
        Bar(x = True)
    End Sub

    Function Foo2()
        Return Bar(True = False)
    End Function

    Sub Foo3()
        If Bar(True = False) Then Bar(True = False)
    End Sub

    Sub Foo4()
        If Bar(True = False) Then
            Bar(True = False)
        ElseIf Bar(True = False) Then
            Bar(True = False)
        Else
            Bar(True = False)
        End If
    End Sub

    Sub Foo5()
        Bar(Nothing)
    End Sub

    Function Bar(ByRef b As Boolean) As Boolean
            Return True
    End Function

    Function Bar2(ByRef c1 As Class1) As Integer
        If c1 IsNot Nothing AndAlso Len(Bar3(Me)) <> 0 Then
            Return 1
        End If
        Return 0
    End Function

    Function Bar3(ByRef c1 As Class1) As String
        Return """"
    End Function

End Class", @"using Microsoft.VisualBasic;

public partial class Class1
{
    public void Foo()
    {
        bool x = true;
        bool argb = x == true;
        Bar(ref argb);
    }

    public object Foo2()
    {
        bool argb = true == false;
        return Bar(ref argb);
    }

    public void Foo3()
    {
        bool argb1 = true == false;
        if (Bar(ref argb1))
        {
            bool argb = true == false;
            Bar(ref argb);
        }
    }

    public void Foo4()
    {
        bool argb3 = true == false;
        bool argb4 = true == false;
        if (Bar(ref argb3))
        {
            bool argb = true == false;
            Bar(ref argb);
        }
        else if (Bar(ref argb4))
        {
            bool argb2 = true == false;
            Bar(ref argb2);
        }
        else
        {
            bool argb1 = true == false;
            Bar(ref argb1);
        }
    }

    public void Foo5()
    {
        bool argb = default;
        Bar(ref argb);
    }

    public bool Bar(ref bool b)
    {
        return true;
    }

    public int Bar2(ref Class1 c1)
    {
        var argc1 = this;
        if (c1 is object && Strings.Len(Bar3(ref argc1)) != 0)
        {
            return 1;
        }

        return 0;
    }

    public string Bar3(ref Class1 c1)
    {
        return """";
    }
}");
        }

        [Fact]
        public async Task RefArgumentUsingAsync()
        {
            await TestConversionVisualBasicToCSharpAsync(@"Imports System.Data.SqlClient

Public Class Class1
    Sub Foo()
        Using x = New SqlConnection
            Bar(x)
        End Using
    End Sub
    Sub Bar(ByRef x As SqlConnection)

    End Sub
End Class", @"using System.Data.SqlClient;

public partial class Class1
{
    public void Foo()
    {
        using (var x = new SqlConnection())
        {
            var argx = x;
            Bar(ref argx);
        }
    }

    public void Bar(ref SqlConnection x)
    {
    }
}");
        }

        [Fact]
        public async Task RefOptionalArgumentAsync()
        {
            await TestConversionVisualBasicToCSharpAsync(@"Public Class OptionalRefIssue91
    Public Shared Function TestSub(Optional ByRef IsDefault As Boolean = False) As Boolean
    End Function

    Public Shared Function CallingFunc() As Boolean
        Return TestSub() AndAlso TestSub(True)
    End Function
End Class", @"using System.Runtime.InteropServices;

public partial class OptionalRefIssue91
{
    public static bool TestSub([Optional, DefaultParameterValue(false)] ref bool IsDefault)
    {
        return default;
    }

    public static bool CallingFunc()
    {
        bool argIsDefault = false;
        bool argIsDefault1 = true;
        return TestSub(IsDefault: ref argIsDefault) && TestSub(ref argIsDefault1);
    }
}");
        }

        [Fact]
        public async Task RefArgumentPropertyInitializerAsync()
        {
            await TestConversionVisualBasicToCSharpAsync(@"Public Class Class1
    Private _p1 As Class1 = Foo(New Class1)
    Public Shared Function Foo(ByRef c1 As Class1) As Class1
        Return c1
    End Function
End Class", @"
public partial class Class1
{
    static Class1 Foo__p1()
    {
        var argc1 = new Class1();
        return Foo(ref argc1);
    }

    private Class1 _p1 = Foo__p1();

    public static Class1 Foo(ref Class1 c1)
    {
        return c1;
    }
}");
        }
    }
}