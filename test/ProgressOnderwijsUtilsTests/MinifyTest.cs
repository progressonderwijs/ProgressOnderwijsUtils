using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExpressionToCodeLib;
using NUnit.Framework;
using System.IO;
using ProgressOnderwijsUtils.WebSupport;

namespace ProgressOnderwijsUtilsTests
{
	[TestFixture]
	public class MinifyTest
	{
		const string inputJS =
@"// is.js

// (c) 2001 Douglas Crockford
// 2001 June 3


// is

// The -is- object is used to identify the browser.  Every browser edition
// identifies itself, but there is no standard way of doing it, and some of
// the identification is deceptive. This is because the authors of web
// browsers are liars. For example, Microsoft's IE browsers claim to be
// Mozilla 4. Netscape 6 claims to be version 5.

var is = {
    ie:      navigator.appName == 'Microsoft Internet Explorer',
    java:    navigator.javaEnabled(),
    ns:      navigator.appName == 'Netscape',
    ua:      navigator.userAgent.toLowerCase(),
    version: parseFloat(navigator.appVersion.substr(21)) ||
             parseFloat(navigator.appVersion),
    win:     navigator.platform == 'Win32'
}
is.mac = is.ua.indexOf('mac') >= 0;
if (is.ua.indexOf('opera') >= 0) {
    is.ie = is.ns = false;
    is.opera = true;
}
if (is.ua.indexOf('gecko') >= 0) {
    is.ie = is.ns = false;
    is.gecko = true;
}
";
		const string outputIdealJS =@"var is={ie:navigator.appName==""Microsoft Internet Explorer"",java:navigator.javaEnabled(),ns:navigator.appName==""Netscape"",ua:navigator.userAgent.toLowerCase(),version:parseFloat(navigator.appVersion.substr(21))||parseFloat(navigator.appVersion),win:navigator.platform==""Win32""};is.mac=is.ua.indexOf(""mac"")>=0;if(is.ua.indexOf(""opera"")>=0){is.ie=is.ns=false;is.opera=true}if(is.ua.indexOf(""gecko"")>=0){is.ie=is.ns=false;is.gecko=true};";
			/*@"var is={ie:navigator.appName=='Microsoft Internet Explorer',java:navigator.javaEnabled(),ns:navigator.appName=='Netscape',ua:navigator.userAgent.toLowerCase(),version:parseFloat(navigator.appVersion.substr(21))||parseFloat(navigator.appVersion),win:navigator.platform=='Win32'}
is.mac=is.ua.indexOf('mac')>=0;if(is.ua.indexOf('opera')>=0){is.ie=is.ns=false;is.opera=true;}
if(is.ua.indexOf('gecko')>=0){is.ie=is.ns=false;is.gecko=true;}";*/

		const string inputCss = @".noxslt {
	background-color: #faf !important;
	font-family: MS Sans Serif, Sans-Serif;
	font-size: smaller;
	color: #000;
}

#reqTime {
	color: black;
	background: #eee;
	padding: 0.2em;
	white-space: pre-wrap;
	font-family: Segoe UI, Verdana, Helvetica, Sans-Serif;
	font-size: 130%;
}

.adisabled {
	color: Gray !important;
}

.KleinVet {
	font-weight: bold;
	font-size: 80%;
	color: Gray;
}
a.KleinVet {
	color: #365a9c;
}

.treebuttons, .taallinks, .statusmodule a {
	font-weight: bold;
	font-size: 80%;
	color: #365a9c;
}
",outputCss = @".noxslt{background-color:#faf!important;font-family:MS Sans Serif,Sans-Serif;font-size:smaller;color:#000;}#reqTime{color:black;background:#eee;padding:.2em;white-space:pre-wrap;font-family:Segoe UI,Verdana,Helvetica,Sans-Serif;font-size:130%;}.adisabled{color:Gray!important;}.KleinVet{font-weight:bold;font-size:80%;color:Gray;}a.KleinVet{color:#365a9c;}.treebuttons,.taallinks,.statusmodule a{font-weight:bold;font-size:80%;color:#365a9c;}";
		[Test]
		public void MinifyJavascript()
		{
			PAssert.That(()=>outputIdealJS == JavascriptMinifyYUI.Minify(inputJS));
		}
		[Test]
		public void MinifyCss()
		{
			PAssert.That(()=>outputCss == CssMinifyYUI.Minify(inputCss));
		}
	}
}
