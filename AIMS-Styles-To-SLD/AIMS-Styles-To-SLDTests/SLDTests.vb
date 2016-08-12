Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports AIMS_Styles_To_SLD


Namespace AIMS_Styles_To_SLD.Tests

    <TestClass()> Public Class SLDTests

        <TestMethod()> Public Sub EqualTo()
            Dim test As String = ""
            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>Typ</PropertyName>" & vbCrLf
            test += "<Literal>Vatten</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf
            Assert.AreEqual(SLDFilter.GenerateFilter("Typ  =  'Vatten'"), test)
        End Sub
        <TestMethod()> Public Sub GreaterThan()
            Dim test As String = ""
            test += "<PropertyIsGreaterThan>" & vbCrLf
            test += "<PropertyName>Typ</PropertyName>" & vbCrLf
            test += "<Literal>1</Literal>" & vbCrLf
            test += "</PropertyIsGreaterThan>" & vbCrLf
            Assert.AreEqual(SLDFilter.GenerateFilter("Typ > 1"), test)
        End Sub
        <TestMethod()> Public Sub LessThan()
            Dim test As String = ""
            test += "<PropertyIsLessThan>" & vbCrLf
            test += "<PropertyName>Typ</PropertyName>" & vbCrLf
            test += "<Literal>1</Literal>" & vbCrLf
            test += "</PropertyIsLessThan>" & vbCrLf
            Assert.AreEqual(SLDFilter.GenerateFilter("Typ < 1"), test)
        End Sub
        <TestMethod()> Public Sub GreaterThanOrEqualTo()
            Dim test As String = ""
            test += "<PropertyIsGreaterThanOrEqualTo>" & vbCrLf
            test += "<PropertyName>Typ</PropertyName>" & vbCrLf
            test += "<Literal>1</Literal>" & vbCrLf
            test += "</PropertyIsGreaterThanOrEqualTo>" & vbCrLf
            Assert.AreEqual(SLDFilter.GenerateFilter("Typ >= 1"), test)
        End Sub
        <TestMethod()> Public Sub LessThanOrEqualTo()
            Dim test As String = ""
            test += "<PropertyIsLessThanOrEqualTo>" & vbCrLf
            test += "<PropertyName>Typ</PropertyName>" & vbCrLf
            test += "<Literal>1</Literal>" & vbCrLf
            test += "</PropertyIsLessThanOrEqualTo>" & vbCrLf
            Assert.AreEqual(SLDFilter.GenerateFilter("Typ <= 1"), test)
        End Sub
        <TestMethod()> Public Sub EqualToAND()
            Dim test As String = ""
            test += "<AND>" & vbCrLf
            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>Typ</PropertyName>" & vbCrLf
            test += "<Literal>1</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>col1</PropertyName>" & vbCrLf
            test += "<Literal>2</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf
            test += "</AND>" & vbCrLf
            Assert.AreEqual(SLDFilter.GenerateFilter("Typ = 1 AND col1 = 2"), test)
        End Sub
        <TestMethod()> Public Sub EqualToOR()
            Dim test As String = ""
            test += "<OR>" & vbCrLf
            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>Typ</PropertyName>" & vbCrLf
            test += "<Literal>1</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>col1</PropertyName>" & vbCrLf
            test += "<Literal>2</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf
            test += "</OR>" & vbCrLf
            Assert.AreEqual(SLDFilter.GenerateFilter("Typ = 1 OR col1 = 2"), test)
        End Sub
        <TestMethod()> Public Sub EqualTo2OR()
            Dim test As String = ""
            test += "<OR>" & vbCrLf
            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>Typ</PropertyName>" & vbCrLf
            test += "<Literal>1</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf

            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>col1</PropertyName>" & vbCrLf
            test += "<Literal>2</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf

            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>col2</PropertyName>" & vbCrLf
            test += "<Literal>3</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf
            test += "</OR>" & vbCrLf

            'Trace.WriteLine(SLD_Filter.GenerateFilter("Typ = 1 OR col1 = 2 OR col2 = 3"))
            'Trace.WriteLine(test)

            Assert.AreEqual(SLDFilter.GenerateFilter("Typ = 1 OR col1 = 2 OR col2 = 3"), test)
        End Sub
        <TestMethod()> Public Sub EqualToIN()
            Dim test As String = ""
            test += "<OR>" & vbCrLf
            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>Typ</PropertyName>" & vbCrLf
            test += "<Literal>test1</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf

            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>Typ</PropertyName>" & vbCrLf
            test += "<Literal>test2</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf
            test += "</OR>" & vbCrLf
            Assert.AreEqual(SLDFilter.GenerateFilter("Typ IN ('test1' , 'test2')"), test)
        End Sub
        <TestMethod()> Public Sub EqualToIN2()
            Dim test As String = ""
            test += "<OR>" & vbCrLf
            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>Typ</PropertyName>" & vbCrLf
            test += "<Literal>1</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf

            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>Typ</PropertyName>" & vbCrLf
            test += "<Literal>2</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf
            test += "</OR>" & vbCrLf
            Assert.AreEqual(SLDFilter.GenerateFilter("Typ IN ('1','2')"), test)
        End Sub
        <TestMethod()> Public Sub NOT_IN()
            Dim test As String = ""
            test += "<NOT>" & vbCrLf
            test += "<OR>" & vbCrLf
            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>VALUE</PropertyName>" & vbCrLf
            test += "<Literal>8</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyIsEqualTo>" & vbCrLf
            test += "<PropertyName>VALUE</PropertyName>" & vbCrLf
            test += "<Literal>9</Literal>" & vbCrLf
            test += "</PropertyIsEqualTo>" & vbCrLf
            test += "</OR>" & vbCrLf
            test += "</NOT>" & vbCrLf
            Assert.AreEqual(SLDFilter.GenerateFilter("NOT  VALUE  IN (  '8' ,  '9'  )"), test)
        End Sub
    End Class


End Namespace


