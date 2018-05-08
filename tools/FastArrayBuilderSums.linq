<Query Kind="Statements">
  <Reference>C:\VCS\LocalOnly\programs\EmnExtensions\bin\Release\EmnExtensions.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Core.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Numerics.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <NuGetReference>CsQuery</NuGetReference>
  <NuGetReference>Dapper</NuGetReference>
  <NuGetReference>ExpressionToCodeLib</NuGetReference>
  <NuGetReference>morelinq</NuGetReference>
  <NuGetReference>ProgressOnderwijsUtils</NuGetReference>
  <Namespace>CsQuery</Namespace>
  <Namespace>Dapper</Namespace>
  <Namespace>ExpressionToCodeLib</Namespace>
  <Namespace>MoreLinq</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Numerics</Namespace>
  <Namespace>System.Runtime.CompilerServices</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
</Query>

var s=16;
var sI=0;
var sum =s;
while(sum>0) {
    sum += s;
    (s,sI, sum).ToString().Dump();
    s = s + (s>>4)+16;
    sI++;
}