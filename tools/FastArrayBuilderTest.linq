<Query Kind="Statements">
  <Reference>C:\VCS\LocalOnly\programs\EmnExtensions\bin\Release\EmnExtensions.dll</Reference>
  <Reference>C:\VCS\external\ProgressOnderwijsUtils\src\ProgressOnderwijsUtils\bin\Release\net471\ProgressOnderwijsUtils.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Core.dll</Reference>
  <Reference>C:\VCS\external\ProgressOnderwijsUtils\src\ProgressOnderwijsUtils\bin\Release\net471\System.Memory.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Numerics.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <NuGetReference>CsQuery</NuGetReference>
  <NuGetReference>Dapper</NuGetReference>
  <NuGetReference>ExpressionToCodeLib</NuGetReference>
  <NuGetReference>morelinq</NuGetReference>
  <Namespace>CsQuery</Namespace>
  <Namespace>Dapper</Namespace>
  <Namespace>ExpressionToCodeLib</Namespace>
  <Namespace>MoreLinq</Namespace>
  <Namespace>ProgressOnderwijsUtils</Namespace>
  <Namespace>ProgressOnderwijsUtils.Collections</Namespace>
  <Namespace>System.Globalization</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Numerics</Namespace>
  <Namespace>System.Runtime.CompilerServices</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <AppConfig>
    <Content>
      <configuration>
        <startup>
          <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.7" />
        </startup>
      </configuration>
    </Content>
  </AppConfig>
</Query>

var x = new FastArrayBuilder2c<int>();
var count = 4000;
for(int i=0;i< count;i++)
    x.Add(i);
var arr = x.ToArray();
arr.SequenceEqual(Enumerable.Range(0, count)).Dump();
arr.Dump();