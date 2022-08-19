using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
// using System.Windows.Forms;
using System.Drawing;
// using System.Drawing.Imaging;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;
using UnityEngine;

namespace Utils 
{
	namespace Web 
	{
		public static class WebTools 
		{
			public static string ReplaceURLChars(string input) 
			{
				string output = "";
				int i = 0;

				for (i = 0; i < input.Length; i++) 
				{
					if (input[i] == '%' && i + 2 < input.Length) 
					{
						byte b = (byte)Format.HexToDecimal(Convert.ToString(input[i + 1]) + Convert.ToString(input[i + 2]));
						if ((b == 0xd0 || b == 0xd1) && i + 5 < input.Length) { output += Encoding.UTF8.GetString(new byte[] { b, (byte)Format.HexToDecimal(Convert.ToString(input[i + 4]) + Convert.ToString(input[i + 5])) }); i+= 3; }
						else { output += Convert.ToString((char)b); }
						i += 2;
						continue;
					}

					output += input[i];
				}

				return output;
			}

			public static IPAddress ConvertIP(string address) 
			{
				byte[] bytes = new byte[4];
				string[] octets = address.Split('.');
				try 
				{
					bytes[0] = (byte)Convert.ToInt32(octets[0]);
					bytes[1] = (byte)Convert.ToInt32(octets[1]);
					bytes[2] = (byte)Convert.ToInt32(octets[2]);
					bytes[3] = (byte)Convert.ToInt32(octets[3]);
				}
				catch { return WebTools.ConvertIP("127.0.0.1"); }
				return new IPAddress(bytes);
			}

			public static string GetLocalIP() 
			{
				System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
			    for (int i = 0; i < host.AddressList.Length; i++)
			    {
			        if (host.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
			        	&& host.AddressList[i].ToString().StartsWith("192.168.1.")) { return host.AddressList[i].ToString(); }
			    }

			    return "127.0.0.1";
			}
		}

		public class HTTPServer 
		{
			public string Version = "HTTP/1.1";
			public string Name = "HTTP Server";
			public string lastHTML = "";

			public string HostName { set; get; }
			public int Port { set; get; }
			public bool Running { get { return running; } }

			public delegate Response ResponseMethod(HTTPServer server, Request request);
			public ResponseMethod responseMethod;

			private TcpListener listener;
			private Thread thread;
			private bool running = false;

			public static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>() 
			{
				{ ".html", "text/html" },
				{ ".css", "text/css" },
				{ ".js", "text/javascript" },
				{ ".ico", "image/x-icon" },
				{ ".png", "image/png" },
				{ ".jpg", "image/jpeg" },
				{ ".jpeg", "image/jpeg" },
				{ ".gif", "image/gif" },
				{ ".txt", "text/plain" },
				{ ".json", "application/json" },
				{ ".csv", "text/csv" },				
				{ ".mp4", "video/mp4" },
				{ ".mkv", "video/x-matroska" },
				{ ".mp3", "audio/mpeg" },
				{ ".wav", "audio/wav" },
				{ ".ttf", "font/ttf" },
				{ ".htm", "text/html" },
				{ ".ez", "application/andrew-inset" },
				{ ".aw", "application/applixware" },
				{ ".atom", "application/atom+xml" },
				{ ".atomcat", "application/atomcat+xml" },
				{ ".atomsvc", "application/atomsvc+xml" },
				{ ".ccxml", "application/ccxml+xml" },
				{ ".cu", "application/cu-seeme" },
				{ ".davmount", "application/davmount+xml" },
				{ ".ecma", "application/ecmascript" },
				{ ".emma", "application/emma+xml" },
				{ ".epub", "application/epub+zip" },
				{ ".pfr", "application/font-tdpfr" },
				{ ".gz", "application/gzip" },
				{ ".stk", "application/hyperstudio" },
				{ ".jar", "application/java-archive" },
				{ ".ser", "application/java-serialized-object" },
				{ ".class", "application/java-vm" },
				{ ".lostxml", "application/lost+xml" },
				{ ".hqx", "application/mac-binhex40" },
				{ ".cpt", "application/mac-compactpro" },
				{ ".mrc", "application/marc" },
				{ ".ma", "application/mathematica" },
				{ ".mbox", "application/mbox" },
				{ ".mscml", "application/mediaservercontrol+xml" },
				{ ".mp4s", "application/mp4" },
				{ ".doc", "application/msword" },
				{ ".mxf", "application/mxf" },
				{ ".a", "application/octet-stream" },
				{ ".oda", "application/oda" },
				{ ".opf", "application/oebps-package+xml" },
				{ ".ogx", "application/ogg" },
				{ ".onepkg", "application/onenote" },
				{ ".xer", "application/patch-ops-error+xml" },
				{ ".pdf", "application/pdf" },
				{ ".pgp", "application/pgp-encrypted" },
				{ ".asc", "application/pgp-signature" },
				{ ".prf", "application/pics-rules" },
				{ ".p10", "application/pkcs10" },
				{ ".p7c", "application/pkcs7-mime" },
				{ ".p7s", "application/pkcs7-signature" },
				{ ".cer", "application/pkix-cert" },
				{ ".crl", "application/pkix-crl" },
				{ ".pkipath", "application/pkix-pkipath" },
				{ ".pki", "application/pkixcmp" },
				{ ".pls", "application/pls+xml" },
				{ ".ai", "application/postscript" },
				{ ".cww", "application/prs.cww" },
				{ ".rdf", "application/rdf+xml" },
				{ ".rif", "application/reginfo+xml" },
				{ ".rnc", "application/relax-ng-compact-syntax" },
				{ ".rl", "application/resource-lists+xml" },
				{ ".rld", "application/resource-lists-diff+xml" },
				{ ".rs", "application/rls-services+xml" },
				{ ".rsd", "application/rsd+xml" },
				{ ".rss", "application/rss+xml" },
				{ ".rtf", "application/rtf" },
				{ ".sbml", "application/sbml+xml" },
				{ ".scq", "application/scvp-cv-request" },
				{ ".scs", "application/scvp-cv-response" },
				{ ".spq", "application/scvp-vp-request" },
				{ ".spp", "application/scvp-vp-response" },
				{ ".sdp", "application/sdp" },
				{ ".setpay", "application/set-payment-initiation" },
				{ ".setreg", "application/set-registration-initiation" },
				{ ".shf", "application/shf+xml" },
				{ ".smi", "application/smil+xml" },
				{ ".rq", "application/sparql-query" },
				{ ".srx", "application/sparql-results+xml" },
				{ ".gram", "application/srgs" },
				{ ".grxml", "application/srgs+xml" },
				{ ".ssml", "application/ssml+xml" },
				{ ".plb", "application/vnd.3gpp.pic-bw-large" },
				{ ".psb", "application/vnd.3gpp.pic-bw-small" },
				{ ".pvb", "application/vnd.3gpp.pic-bw-var" },
				{ ".tcap", "application/vnd.3gpp2.tcap" },
				{ ".pwn", "application/vnd.3m.post-it-notes" },
				{ ".aso", "application/vnd.accpac.simply.aso" },
				{ ".imp", "application/vnd.accpac.simply.imp" },
				{ ".acu", "application/vnd.acucobol" },
				{ ".acutc", "application/vnd.acucorp" },
				{ ".air", "application/vnd.adobe.air-application-installer-package+zip" },
				{ ".xdp", "application/vnd.adobe.xdp+xml" },
				{ ".xfdf", "application/vnd.adobe.xfdf" },
				{ ".azf", "application/vnd.airzip.filesecure.azf" },
				{ ".azs", "application/vnd.airzip.filesecure.azs" },
				{ ".azw", "application/vnd.amazon.ebook" },
				{ ".acc", "application/vnd.americandynamics.acc" },
				{ ".ami", "application/vnd.amiga.ami" },
				{ ".apk", "application/vnd.android.package-archive" },
				{ ".cii", "application/vnd.anser-web-certificate-issue-initiation" },
				{ ".fti", "application/vnd.anser-web-funds-transfer-initiation" },
				{ ".atx", "application/vnd.antix.game-component" },
				{ ".mpkg", "application/vnd.apple.installer+xml" },
				{ ".swi", "application/vnd.arastra.swi" },
				{ ".aep", "application/vnd.audiograph" },
				{ ".mpm", "application/vnd.blueice.multipass" },
				{ ".bmi", "application/vnd.bmi" },
				{ ".rep", "application/vnd.businessobjects" },
				{ ".cdxml", "application/vnd.chemdraw+xml" },
				{ ".mmd", "application/vnd.chipnuts.karaoke-mmd" },
				{ ".cdy", "application/vnd.cinderella" },
				{ ".cla", "application/vnd.claymore" },
				{ ".c4d", "application/vnd.clonk.c4group" },
				{ ".csp", "application/vnd.commonspace" },
				{ ".cdbcmsg", "application/vnd.contact.cmsg" },
				{ ".cmc", "application/vnd.cosmocaller" },
				{ ".clkx", "application/vnd.crick.clicker" },
				{ ".clkk", "application/vnd.crick.clicker.keyboard" },
				{ ".clkp", "application/vnd.crick.clicker.palette" },
				{ ".clkt", "application/vnd.crick.clicker.template" },
				{ ".clkw", "application/vnd.crick.clicker.wordbank" },
				{ ".wbs", "application/vnd.criticaltools.wbs+xml" },
				{ ".pml", "application/vnd.ctc-posml" },
				{ ".ppd", "application/vnd.cups-ppd" },
				{ ".car", "application/vnd.curl.car" },
				{ ".pcurl", "application/vnd.curl.pcurl" },
				{ ".rdz", "application/vnd.data-vision.rdz" },
				{ ".fe_launch", "application/vnd.denovo.fcselayout-link" },
				{ ".dna", "application/vnd.dna" },
				{ ".mlp", "application/vnd.dolby.mlp" },
				{ ".dpg", "application/vnd.dpgraph" },
				{ ".dfac", "application/vnd.dreamfactory" },
				{ ".geo", "application/vnd.dynageo" },
				{ ".mag", "application/vnd.ecowin.chart" },
				{ ".nml", "application/vnd.enliven" },
				{ ".esf", "application/vnd.epson.esf" },
				{ ".msf", "application/vnd.epson.msf" },
				{ ".qam", "application/vnd.epson.quickanime" },
				{ ".slt", "application/vnd.epson.salt" },
				{ ".ssf", "application/vnd.epson.ssf" },
				{ ".es3", "application/vnd.eszigno3+xml" },
				{ ".ez2", "application/vnd.ezpix-album" },
				{ ".ez3", "application/vnd.ezpix-package" },
				{ ".fdf", "application/vnd.fdf" },
				{ ".mseed", "application/vnd.fdsn.mseed" },
				{ ".dataless", "application/vnd.fdsn.seed" },
				{ ".gph", "application/vnd.flographit" },
				{ ".ftc", "application/vnd.fluxtime.clip" },
				{ ".bin", "application/octet-stream" },
				{ ".book", "application/vnd.framemaker" },
				{ ".fnc", "application/vnd.frogans.fnc" },
				{ ".ltf", "application/vnd.frogans.ltf" },
				{ ".fsc", "application/vnd.fsc.weblaunch" },
				{ ".oas", "application/vnd.fujitsu.oasys" },
				{ ".oa2", "application/vnd.fujitsu.oasys2" },
				{ ".oa3", "application/vnd.fujitsu.oasys3" },
				{ ".fg5", "application/vnd.fujitsu.oasysgp" },
				{ ".bh2", "application/vnd.fujitsu.oasysprs" },
				{ ".ddd", "application/vnd.fujixerox.ddd" },
				{ ".xdw", "application/vnd.fujixerox.docuworks" },
				{ ".xbd", "application/vnd.fujixerox.docuworks.binder" },
				{ ".fzs", "application/vnd.fuzzysheet" },
				{ ".txd", "application/vnd.genomatix.tuxedo" },
				{ ".ggb", "application/vnd.geogebra.file" },
				{ ".ggt", "application/vnd.geogebra.tool" },
				{ ".gex", "application/vnd.geometry-explorer" },
				{ ".gmx", "application/vnd.gmx" },
				{ ".kml", "application/vnd.google-earth.kml+xml" },
				{ ".kmz", "application/vnd.google-earth.kmz" },
				{ ".gqf", "application/vnd.grafeq" },
				{ ".gac", "application/vnd.groove-account" },
				{ ".ghf", "application/vnd.groove-help" },
				{ ".gim", "application/vnd.groove-identity-message" },
				{ ".grv", "application/vnd.groove-injector" },
				{ ".gtm", "application/vnd.groove-tool-message" },
				{ ".tpl", "application/vnd.groove-tool-template" },
				{ ".vcg", "application/vnd.groove-vcard" },
				{ ".zmm", "application/vnd.handheld-entertainment+xml" },
				{ ".hbci", "application/vnd.hbci" },
				{ ".les", "application/vnd.hhe.lesson-player" },
				{ ".hpgl", "application/vnd.hp-hpgl" },
				{ ".hpid", "application/vnd.hp-hpid" },
				{ ".hps", "application/vnd.hp-hps" },
				{ ".jlt", "application/vnd.hp-jlyt" },
				{ ".pcl", "application/vnd.hp-pcl" },
				{ ".pclxl", "application/vnd.hp-pclxl" },
				{ ".php", "application/x-httpd-php" },
				{ ".ppt", "application/vnd.ms-powerpoint" },
				{ ".sfd-hdstx", "application/vnd.hydrostatix.sof-data" },
				{ ".x3d", "application/vnd.hzn-3d-crossword" },
				{ ".mpy", "application/vnd.ibm.minipay" },
				{ ".afp", "application/vnd.ibm.modcap" },
				{ ".irm", "application/vnd.ibm.rights-management" },
				{ ".sc", "application/vnd.ibm.secure-container" },
				{ ".icc", "application/vnd.iccprofile" },
				{ ".igl", "application/vnd.igloader" },
				{ ".ivp", "application/vnd.immervision-ivp" },
				{ ".ivu", "application/vnd.immervision-ivu" },
				{ ".xpw", "application/vnd.intercon.formnet" },
				{ ".qbo", "application/vnd.intu.qbo" },
				{ ".qfx", "application/vnd.intu.qfx" },
				{ ".rar", "application/vnd.rar" },
				{ ".rcprofile", "application/vnd.ipunplugged.rcprofile" },
				{ ".irp", "application/vnd.irepository.package+xml" },
				{ ".xpr", "application/vnd.is-xpr" },
				{ ".jam", "application/vnd.jam" },
				{ ".rms", "application/vnd.jcp.javame.midlet-rms" },
				{ ".jisp", "application/vnd.jisp" },
				{ ".joda", "application/vnd.joost.joda-archive" },
				{ ".ktr", "application/vnd.kahootz" },
				{ ".karbon", "application/vnd.kde.karbon" },
				{ ".chrt", "application/vnd.kde.kchart" },
				{ ".kfo", "application/vnd.kde.kformula" },
				{ ".flw", "application/vnd.kde.kivio" },
				{ ".kon", "application/vnd.kde.kontour" },
				{ ".kpr", "application/vnd.kde.kpresenter" },
				{ ".ksp", "application/vnd.kde.kspread" },
				{ ".kwd", "application/vnd.kde.kword" },
				{ ".htke", "application/vnd.kenameaapp" },
				{ ".kia", "application/vnd.kidspiration" },
				{ ".kne", "application/vnd.kinar" },
				{ ".skd", "application/vnd.koan" },
				{ ".sse", "application/vnd.kodak-descriptor" },
				{ ".lbd", "application/vnd.llamagraphics.life-balance.desktop" },
				{ ".lbe", "application/vnd.llamagraphics.life-balance.exchange+xml" },
				{ ".123", "application/vnd.lotus-1-2-3" },
				{ ".apr", "application/vnd.lotus-approach" },
				{ ".pre", "application/vnd.lotus-freelance" },
				{ ".nsf", "application/vnd.lotus-notes" },
				{ ".org", "application/vnd.lotus-organizer" },
				{ ".scm", "application/vnd.lotus-screencam" },
				{ ".lwp", "application/vnd.lotus-wordpro" },
				{ ".portpkg", "application/vnd.macports.portpkg" },
				{ ".mcd", "application/vnd.mcd" },
				{ ".mc1", "application/vnd.medcalcdata" },
				{ ".cdkey", "application/vnd.mediastation.cdkey" },
				{ ".mwf", "application/vnd.mfer" },
				{ ".mfm", "application/vnd.mfmp" },
				{ ".flo", "application/vnd.micrografx.flo" },
				{ ".igx", "application/vnd.micrografx.igx" },
				{ ".mif", "application/vnd.mif" },
				{ ".daf", "application/vnd.mobius.daf" },
				{ ".dis", "application/vnd.mobius.dis" },
				{ ".mbk", "application/vnd.mobius.mbk" },
				{ ".mqy", "application/vnd.mobius.mqy" },
				{ ".msl", "application/vnd.mobius.msl" },
				{ ".plc", "application/vnd.mobius.plc" },
				{ ".txf", "application/vnd.mobius.txf" },
				{ ".mpn", "application/vnd.mophun.application" },
				{ ".mpc", "application/vnd.mophun.certificate" },
				{ ".xul", "application/vnd.mozilla.xul+xml" },
				{ ".cil", "application/vnd.ms-artgalry" },
				{ ".cab", "application/vnd.ms-cab-compressed" },
				{ ".xla", "application/vnd.ms-excel" },
				{ ".xls", "application/vnd.ms-excel" },
				{ ".xlam", "application/vnd.ms-excel.addin.macroenabled.12" },
				{ ".xlsb", "application/vnd.ms-excel.sheet.binary.macroenabled.12" },
				{ ".xlsm", "application/vnd.ms-excel.sheet.macroenabled.12" },
				{ ".xltm", "application/vnd.ms-excel.template.macroenabled.12" },
				{ ".eot", "application/vnd.ms-fontobject" },
				{ ".chm", "application/vnd.ms-htmlhelp" },
				{ ".ims", "application/vnd.ms-ims" },
				{ ".lrm", "application/vnd.ms-lrm" },
				{ ".cat", "application/vnd.ms-pki.seccat" },
				{ ".stl", "application/vnd.ms-pki.stl" },
				{ ".pot", "application/vnd.ms-powerpoint" },
				{ ".ppam", "application/vnd.ms-powerpoint.addin.macroenabled.12" },
				{ ".pptm", "application/vnd.ms-powerpoint.presentation.macroenabled.12" },
				{ ".sldm", "application/vnd.ms-powerpoint.slide.macroenabled.12" },
				{ ".ppsm", "application/vnd.ms-powerpoint.slideshow.macroenabled.12" },
				{ ".potm", "application/vnd.ms-powerpoint.template.macroenabled.12" },
				{ ".mpp", "application/vnd.ms-project" },
				{ ".docm", "application/vnd.ms-word.document.macroenabled.12" },
				{ ".dotm", "application/vnd.ms-word.template.macroenabled.12" },
				{ ".wcm", "application/vnd.ms-works" },
				{ ".wpl", "application/vnd.ms-wpl" },
				{ ".xps", "application/vnd.ms-xpsdocument" },
				{ ".mseq", "application/vnd.mseq" },
				{ ".mus", "application/vnd.musician" },
				{ ".msty", "application/vnd.muvee.style" },
				{ ".nlu", "application/vnd.neurolanguage.nlu" },
				{ ".nnd", "application/vnd.noblenet-directory" },
				{ ".nns", "application/vnd.noblenet-sealer" },
				{ ".nnw", "application/vnd.noblenet-web" },
				{ ".ngdat", "application/vnd.nokia.n-gage.data" },
				{ ".n-gage", "application/vnd.nokia.n-gage.symbian.install" },
				{ ".rpst", "application/vnd.nokia.radio-preset" },
				{ ".rpss", "application/vnd.nokia.radio-presets" },
				{ ".edm", "application/vnd.novadigm.edm" },
				{ ".edx", "application/vnd.novadigm.edx" },
				{ ".ext", "application/vnd.novadigm.ext" },
				{ ".odc", "application/vnd.oasis.opendocument.chart" },
				{ ".otc", "application/vnd.oasis.opendocument.chart-template" },
				{ ".odb", "application/vnd.oasis.opendocument.database" },
				{ ".odf", "application/vnd.oasis.opendocument.formula" },
				{ ".odft", "application/vnd.oasis.opendocument.formula-template" },
				{ ".odg", "application/vnd.oasis.opendocument.graphics" },
				{ ".otg", "application/vnd.oasis.opendocument.graphics-template" },
				{ ".odi", "application/vnd.oasis.opendocument.image" },
				{ ".oti", "application/vnd.oasis.opendocument.image-template" },
				{ ".odp", "application/vnd.oasis.opendocument.presentation" },
				{ ".otp", "application/vnd.oasis.opendocument.presentation-template" },
				{ ".ods", "application/vnd.oasis.opendocument.spreadsheet" },
				{ ".ots", "application/vnd.oasis.opendocument.spreadsheet-template" },
				{ ".odt", "application/vnd.oasis.opendocument.text" },
				{ ".otm", "application/vnd.oasis.opendocument.text-master" },
				{ ".ott", "application/vnd.oasis.opendocument.text-template" },
				{ ".oth", "application/vnd.oasis.opendocument.text-web" },
				{ ".xo", "application/vnd.olpc-sugar" },
				{ ".dd2", "application/vnd.oma.dd2+xml" },
				{ ".oxt", "application/vnd.openofficeorg.extension" },
				{ ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
				{ ".sldx", "application/vnd.openxmlformats-officedocument.presentationml.slide" },
				{ ".ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
				{ ".potx", "application/vnd.openxmlformats-officedocument.presentationml.template" },
				{ ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
				{ ".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" },
				{ ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
				{ ".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
				{ ".dp", "application/vnd.osgi.dp" },
				{ ".oprc", "application/vnd.palm" },
				{ ".str", "application/vnd.pg.format" },
				{ ".ei6", "application/vnd.pg.osasli" },
				{ ".efif", "application/vnd.picsel" },
				{ ".plf", "application/vnd.pocketlearn" },
				{ ".pbd", "application/vnd.powerbuilder6" },
				{ ".box", "application/vnd.previewsystems.box" },
				{ ".mgz", "application/vnd.proteus.magazine" },
				{ ".qps", "application/vnd.publishare-delta-tree" },
				{ ".ptid", "application/vnd.pvi.ptid1" },
				{ ".qwd", "application/vnd.quark.quarkxpress" },
				{ ".mxl", "application/vnd.recordare.musicxml" },
				{ ".musicxml", "application/vnd.recordare.musicxml+xml" },
				{ ".cod", "application/vnd.rim.cod" },
				{ ".rm", "application/vnd.rn-realmedia" },
				{ ".link66", "application/vnd.route66.link66+xml" },
				{ ".see", "application/vnd.seemail" },
				{ ".sema", "application/vnd.sema" },
				{ ".semd", "application/vnd.semd" },
				{ ".semf", "application/vnd.semf" },
				{ ".ifm", "application/vnd.shana.informed.formdata" },
				{ ".itp", "application/vnd.shana.informed.formtemplate" },
				{ ".iif", "application/vnd.shana.informed.interchange" },
				{ ".ipk", "application/vnd.shana.informed.package" },
				{ ".twd", "application/vnd.simtech-mindmapper" },
				{ ".mmf", "application/vnd.smaf" },
				{ ".teacher", "application/vnd.smart.teacher" },
				{ ".sdkd", "application/vnd.solent.sdkm+xml" },
				{ ".dxp", "application/vnd.spotfire.dxp" },
				{ ".sfs", "application/vnd.spotfire.sfs" },
				{ ".db", "application/vnd.sqlite3" },
				{ ".sdc", "application/vnd.stardivision.calc" },
				{ ".sda", "application/vnd.stardivision.draw" },
				{ ".sdd", "application/vnd.stardivision.impress" },
				{ ".smf", "application/vnd.stardivision.math" },
				{ ".sdw", "application/vnd.stardivision.writer" },
				{ ".sgl", "application/vnd.stardivision.writer-global" },
				{ ".sxc", "application/vnd.sun.xml.calc" },
				{ ".stc", "application/vnd.sun.xml.calc.template" },
				{ ".sxd", "application/vnd.sun.xml.draw" },
				{ ".std", "application/vnd.sun.xml.draw.template" },
				{ ".sxi", "application/vnd.sun.xml.impress" },
				{ ".sti", "application/vnd.sun.xml.impress.template" },
				{ ".sxm", "application/vnd.sun.xml.math" },
				{ ".sxw", "application/vnd.sun.xml.writer" },
				{ ".sxg", "application/vnd.sun.xml.writer.global" },
				{ ".stw", "application/vnd.sun.xml.writer.template" },
				{ ".sus", "application/vnd.sus-calendar" },
				{ ".svd", "application/vnd.svd" },
				{ ".sis", "application/vnd.symbian.install" },
				{ ".xsm", "application/vnd.syncml+xml" },
				{ ".bdm", "application/vnd.syncml.dm+wbxml" },
				{ ".xdm", "application/vnd.syncml.dm+xml" },
				{ ".tao", "application/vnd.tao.intent-module-archive" },
				{ ".tmo", "application/vnd.tmobile-livetv" },
				{ ".tpt", "application/vnd.trid.tpt" },
				{ ".mxs", "application/vnd.triscape.mxs" },
				{ ".tra", "application/vnd.trueapp" },
				{ ".ufd", "application/vnd.ufdl" },
				{ ".utz", "application/vnd.uiq.theme" },
				{ ".umj", "application/vnd.umajin" },
				{ ".unityweb", "application/vnd.unity" },
				{ ".uoml", "application/vnd.uoml+xml" },
				{ ".vcx", "application/vnd.vcx" },
				{ ".vsd", "application/vnd.visio" },
				{ ".vis", "application/vnd.visionary" },
				{ ".vsf", "application/vnd.vsf" },
				{ ".sic", "application/vnd.wap.sic" },
				{ ".slc", "application/vnd.wap.slc" },
				{ ".wbxml", "application/vnd.wap.wbxml" },
				{ ".wmlc", "application/vnd.wap.wmlc" },
				{ ".wmlsc", "application/vnd.wap.wmlscriptc" },
				{ ".wtb", "application/vnd.webturbo" },
				{ ".wpd", "application/vnd.wordperfect" },
				{ ".wqd", "application/vnd.wqd" },
				{ ".stf", "application/vnd.wt.stf" },
				{ ".xar", "application/vnd.xara" },
				{ ".xfdl", "application/vnd.xfdl" },
				{ ".hvd", "application/vnd.yamaha.hv-dic" },
				{ ".hvs", "application/vnd.yamaha.hv-script" },
				{ ".hvp", "application/vnd.yamaha.hv-voice" },
				{ ".osf", "application/vnd.yamaha.openscoreformat" },
				{ ".osfpvg", "application/vnd.yamaha.openscoreformat.osfpvg+xml" },
				{ ".saf", "application/vnd.yamaha.smaf-audio" },
				{ ".spf", "application/vnd.yamaha.smaf-phrase" },
				{ ".cmp", "application/vnd.yellowriver-custom-menu" },
				{ ".zir", "application/vnd.zul" },
				{ ".zaz", "application/vnd.zzazz.deck+xml" },
				{ ".vxml", "application/voicexml+xml" },
				{ ".hlp", "application/winhlp" },
				{ ".wsdl", "application/wsdl+xml" },
				{ ".wspolicy", "application/wspolicy+xml" },
				{ ".7z", "application/x-7z-compressed" },
				{ ".abw", "application/x-abiword" },
				{ ".ace", "application/x-ace-compressed" },
				{ ".aab", "application/x-authorware-bin" },
				{ ".aam", "application/x-authorware-map" },
				{ ".aas", "application/x-authorware-seg" },
				{ ".bcpio", "application/x-bcpio" },
				{ ".torrent", "application/x-bittorrent" },
				{ ".bz", "application/x-bzip" },
				{ ".boz", "application/x-bzip2" },
				{ ".vcd", "application/x-cdlink" },
				{ ".chat", "application/x-chat" },
				{ ".pgn", "application/x-chess-pgn" },
				{ ".cpio", "application/x-cpio" },
				{ ".csh", "application/x-csh" },
				{ ".deb", "application/x-debian-package" },
				{ ".cct", "application/x-director" },
				{ ".wad", "application/x-doom" },
				{ ".ncx", "application/x-dtbncx+xml" },
				{ ".dtb", "application/x-dtbook+xml" },
				{ ".res", "application/x-dtbresource+xml" },
				{ ".dvi", "application/x-dvi" },
				{ ".bdf", "application/x-font-bdf" },
				{ ".gsf", "application/x-font-ghostscript" },
				{ ".psf", "application/x-font-linux-psf" },
				{ ".pcf", "application/x-font-pcf" },
				{ ".snf", "application/x-font-snf" },
				{ ".ttc", "application/x-font-ttf" },
				{ ".afm", "application/x-font-type1" },
				{ ".spl", "application/x-futuresplash" },
				{ ".gnumeric", "application/x-gnumeric" },
				{ ".gtar", "application/x-gtar" },
				{ ".hdf", "application/x-hdf" },
				{ ".jnlp", "application/x-java-jnlp-file" },
				{ ".kil", "application/x-killustrator" },
				{ ".latex", "application/x-latex" },
				{ ".mobi", "application/x-mobipocket-ebook" },
				{ ".application", "application/x-ms-application" },
				{ ".wmd", "application/x-ms-wmd" },
				{ ".wmz", "application/x-ms-wmz" },
				{ ".xbap", "application/x-ms-xbap" },
				{ ".mdb", "application/x-msaccess" },
				{ ".obd", "application/x-msbinder" },
				{ ".crd", "application/x-mscardfile" },
				{ ".clp", "application/x-msclip" },
				{ ".bat", "application/x-msdownload" },
				{ ".m13", "application/x-msmediaview" },
				{ ".wmf", "application/x-msmetafile" },
				{ ".mny", "application/x-msmoney" },
				{ ".pub", "application/x-mspublisher" },
				{ ".scd", "application/x-msschedule" },
				{ ".trm", "application/x-msterminal" },
				{ ".wri", "application/x-mswrite" },
				{ ".cdf", "application/x-netcdf" },
				{ ".pm", "application/x-perl" },
				{ ".p12", "application/x-pkcs12" },
				{ ".p7b", "application/x-pkcs7-certificates" },
				{ ".pyc", "application/x-python-code" },
				{ ".rpa", "application/x-redhat-package-manager" },
				{ ".rpm", "application/x-rpm" },
				{ ".sh", "application/x-sh" },
				{ ".shar", "application/x-shar" },
				{ ".swf", "application/x-shockwave-flash" },
				{ ".xap", "application/x-silverlight-app" },
				{ ".sit", "application/x-stuffit" },
				{ ".sitx", "application/x-stuffitx" },
				{ ".sv4cpio", "application/x-sv4cpio" },
				{ ".sv4crc", "application/x-sv4crc" },
				{ ".tar", "application/x-tar" },
				{ ".tcl", "application/x-tcl" },
				{ ".tex", "application/x-tex" },
				{ ".tfm", "application/x-tex-tfm" },
				{ ".texi", "application/x-texinfo" },
				{ ".ustar", "application/x-ustar" },
				{ ".src", "application/x-wais-source" },
				{ ".crt", "application/x-x509-ca-cert" },
				{ ".fig", "application/x-xfig" },
				{ ".xpi", "application/x-xpinstall" },
				{ ".xenc", "application/xenc+xml" },
				{ ".xht", "application/xhtml+xml" },
				{ ".xml", "application/xml" },
				{ ".dtd", "application/xml-dtd" },
				{ ".xop", "application/xop+xml" },
				{ ".xslt", "application/xslt+xml" },
				{ ".xspf", "application/xspf+xml" },
				{ ".mxml", "application/xv+xml" },
				{ ".zip", "application/zip" },
				{ ".3gp", "audio/3gpp" },
				{ ".3g2", "audio/3gpp2" },
				{ ".adp", "audio/adpcm" },
				{ ".aiff", "audio/aiff" },
				{ ".au", "audio/basic" },
				{ ".kar", "audio/midi" },
				{ ".mp4a", "audio/mp4" },
				{ ".m2a", "audio/mpeg" },
				{ ".oga", "audio/ogg" },
				{ ".opus", "audio/opus" },
				{ ".eol", "audio/vnd.digital-winds" },
				{ ".dts", "audio/vnd.dts" },
				{ ".dtshd", "audio/vnd.dts.hd" },
				{ ".lvp", "audio/vnd.lucent.voice" },
				{ ".pya", "audio/vnd.ms-playready.media.pya" },
				{ ".ecelp4800", "audio/vnd.nuera.ecelp4800" },
				{ ".ecelp7470", "audio/vnd.nuera.ecelp7470" },
				{ ".ecelp9600", "audio/vnd.nuera.ecelp9600" },
				{ ".weba", "audio/webm" },
				{ ".aac", "audio/x-aac" },
				{ ".aif", "audio/x-aiff" },
				{ ".mka", "audio/x-matroska" },
				{ ".m3u", "audio/x-mpegurl" },
				{ ".wax", "audio/x-ms-wax" },
				{ ".wma", "audio/x-ms-wma" },
				{ ".ra", "audio/x-pn-realaudio" },
				{ ".rmp", "audio/x-pn-realaudio-plugin" },
				{ ".cdx", "chemical/x-cdx" },
				{ ".cif", "chemical/x-cif" },
				{ ".cmdf", "chemical/x-cmdf" },
				{ ".cml", "chemical/x-cml" },
				{ ".csml", "chemical/x-csml" },
				{ ".xyz", "chemical/x-xyz" },
				{ ".otf", "font/otf" },
				{ ".woff", "font/woff" },
				{ ".woff2", "font/woff2" },
				{ ".gcode", "gcode" },
				{ ".avif", "image/avif" },
				{ ".bmp", "image/bmp" },
				{ ".cgm", "image/cgm" },
				{ ".g3", "image/g3fax" },
				{ ".heif", "image/heic" },
				{ ".ief", "image/ief" },
				{ ".jpe", "image/jpeg" },
				{ ".btif", "image/prs.btif" },
				{ ".svg", "image/svg+xml" },
				{ ".tif", "image/tiff" },
				{ ".psd", "image/vnd.adobe.photoshop" },
				{ ".djv", "image/vnd.djvu" },
				{ ".dwg", "image/vnd.dwg" },
				{ ".dxf", "image/vnd.dxf" },
				{ ".fbs", "image/vnd.fastbidsheet" },
				{ ".fpx", "image/vnd.fpx" },
				{ ".fst", "image/vnd.fst" },
				{ ".mmr", "image/vnd.fujixerox.edmics-mmr" },
				{ ".rlc", "image/vnd.fujixerox.edmics-rlc" },
				{ ".mdi", "image/vnd.ms-modi" },
				{ ".npx", "image/vnd.net-fpx" },
				{ ".wbmp", "image/vnd.wap.wbmp" },
				{ ".xif", "image/vnd.xiff" },
				{ ".webp", "image/webp" },
				{ ".dng", "image/x-adobe-dng" },
				{ ".cr2", "image/x-canon-cr2" },
				{ ".crw", "image/x-canon-crw" },
				{ ".ras", "image/x-cmu-raster" },
				{ ".cmx", "image/x-cmx" },
				{ ".erf", "image/x-epson-erf" },
				{ ".fh", "image/x-freehand" },
				{ ".raf", "image/x-fuji-raf" },
				{ ".dcr", "image/x-kodak-dcr" },
				{ ".k25", "image/x-kodak-k25" },
				{ ".kdc", "image/x-kodak-kdc" },
				{ ".mrw", "image/x-minolta-mrw" },
				{ ".nef", "image/x-nikon-nef" },
				{ ".orf", "image/x-olympus-orf" },
				{ ".raw", "image/x-panasonic-raw" },
				{ ".pcx", "image/x-pcx" },
				{ ".pef", "image/x-pentax-pef" },
				{ ".pct", "image/x-pict" },
				{ ".pnm", "image/x-portable-anymap" },
				{ ".pbm", "image/x-portable-bitmap" },
				{ ".pgm", "image/x-portable-graymap" },
				{ ".ppm", "image/x-portable-pixmap" },
				{ ".rgb", "image/x-rgb" },
				{ ".x3f", "image/x-sigma-x3f" },
				{ ".arw", "image/x-sony-arw" },
				{ ".sr2", "image/x-sony-sr2" },
				{ ".srf", "image/x-sony-srf" },
				{ ".xbm", "image/x-xbitmap" },
				{ ".xpm", "image/x-xpixmap" },
				{ ".xwd", "image/x-xwindowdump" },
				{ ".eml", "message/rfc822" },
				{ ".iges", "model/iges" },
				{ ".mesh", "model/mesh" },
				{ ".dwf", "model/vnd.dwf" },
				{ ".gdl", "model/vnd.gdl" },
				{ ".gtw", "model/vnd.gtw" },
				{ ".mts", "model/vnd.mts" },
				{ ".vtu", "model/vnd.vtu" },
				{ ".vrml", "model/vrml" },
				{ ".ics", "text/calendar" },
				{ ".md", "text/markdown" },
				{ ".mathml", "text/mathml" },
				{ ".conf", "text/plain" },
				{ ".dsc", "text/prs.lines.tag" },
				{ ".rtx", "text/richtext" },
				{ ".sgm", "text/sgml" },
				{ ".tsv", "text/tab-separated-values" },
				{ ".man", "text/troff" },
				{ ".uri", "text/uri-list" },
				{ ".curl", "text/vnd.curl" },
				{ ".dcurl", "text/vnd.curl.dcurl" },
				{ ".mcurl", "text/vnd.curl.mcurl" },
				{ ".scurl", "text/vnd.curl.scurl" },
				{ ".fly", "text/vnd.fly" },
				{ ".flx", "text/vnd.fmi.flexstor" },
				{ ".gv", "text/vnd.graphviz" },
				{ ".3dml", "text/vnd.in3d.3dml" },
				{ ".spot", "text/vnd.in3d.spot" },
				{ ".jad", "text/vnd.sun.j2me.app-descriptor" },
				{ ".si", "text/vnd.wap.si" },
				{ ".sl", "text/vnd.wap.sl" },
				{ ".wml", "text/vnd.wap.wml" },
				{ ".wmls", "text/vnd.wap.wmlscript" },
				{ ".asm", "text/x-asm" },
				{ ".c", "text/x-c" },
				{ ".f", "text/x-fortran" },
				{ ".java", "text/x-java-source" },
				{ ".p", "text/x-pascal" },
				{ ".py", "text/x-python" },
				{ ".etx", "text/x-setext" },
				{ ".uu", "text/x-uuencode" },
				{ ".vcs", "text/x-vcalendar" },
				{ ".vcf", "text/x-vcard" },
				{ ".h261", "video/h261" },
				{ ".h263", "video/h263" },
				{ ".h264", "video/h264" },
				{ ".jpgv", "video/jpeg" },
				{ ".jpgm", "video/jpm" },
				{ ".mj2", "video/mj2" },
				{ ".m1v", "video/mpeg" },
				{ ".ogv", "video/ogg" },
				{ ".mov", "video/quicktime" },
				{ ".fvt", "video/vnd.fvt" },
				{ ".m4u", "video/vnd.mpegurl" },
				{ ".pyv", "video/vnd.ms-playready.media.pyv" },
				{ ".viv", "video/vnd.vivo" },
				{ ".webm", "video/webm" },
				{ ".f4v", "video/x-f4v" },
				{ ".fli", "video/x-fli" },
				{ ".flv", "video/x-flv" },
				{ ".m4v", "video/x-m4v" },
				{ ".asf", "video/x-ms-asf" },
				{ ".wm", "video/x-ms-wm" },
				{ ".wmv", "video/x-ms-wmv" },
				{ ".wmx", "video/x-ms-wmx" },
				{ ".wvx", "video/x-ms-wvx" },
				{ ".avi", "video/x-msvideo" },
				{ ".movie", "video/x-sgi-movie" },
				{ ".ice", "x-conference/x-cooltalk" }
			};

			private void HandleClient(TcpClient client) 
			{
				if (client == null) { return; }
				StreamReader reader = new StreamReader(client.GetStream());
				string message = "";

				while (reader.Peek() != -1) { message += reader.ReadLine() + "\n"; }

				Request request = Request.GenerateRequest(message);
				Response response = responseMethod(this, request);

				int errorCode = Convert.ToInt32(response.Status.Split(' ')[0]);
				string color = (errorCode >= 400) ? Tools.ConsoleColor.RedF : ((errorCode == 200) ? Tools.ConsoleColor.GreenF : Tools.ConsoleColor.GrayF);
				Tools.ColorWriteLine("Request from: " + request.Host + request.URL + " - Response: " + response.Mime + " " + color + Convert.ToString(errorCode) + Tools.ConsoleColor.GrayF);
				
				response.Post(this, client.GetStream());
			}

			private void Run() 
			{
				running = true;
				listener.Start();

				while (running) 
				{
					if (listener == null) { running = false; return; }
					TcpClient client = listener.AcceptTcpClient();
					HandleClient(client);
					if (client != null) { client.Close(); }
				}

				listener.Stop();
				running = false;
			}

			private void Initialize(string HostName, int Port, ResponseMethod responseMethod) 
			{
				this.HostName = HostName;
				this.Port = Port;
				this.listener = new TcpListener(WebTools.ConvertIP(this.HostName), this.Port);
				this.thread = new Thread(new ThreadStart(Run));
				this.responseMethod = responseMethod;
			}

			public void Start() { thread.Start(); Console.WriteLine("Server started on " + HostName + ":" + Convert.ToString(Port)); }
			public void Stop() { thread.Abort(); Console.WriteLine("Server stopped."); running = false; }

			public HTTPServer() { this.Initialize("127.0.0.1", 8000, Response.GenerateDirectoryResponse); }
			public HTTPServer(string HostName, int Port, ResponseMethod responseMethod) { this.Initialize(HostName, Port, responseMethod); }
			public HTTPServer(string HostName, int Port, ResponseMethod responseMethod, string Name) { this.Initialize(HostName, Port, responseMethod); this.Name = Name; }



			public class Request 
			{
				public string Type { set; get; }
				public string URL { set; get; }
				public string Host { set; get; }

				public static Request GenerateRequest(string request) 
				{
					if (request == "" || request == null) { return null; }

					string[] tokens = request.Split(' ', '\n');
					string type = tokens[0];
					string url = tokens[1];
					string host = tokens[4];

					return new Request(type, url, host);
				}

				public Request() {}
				public Request(string Type, string URL, string Host) 
				{
					this.Type = Type;
					this.URL = URL;
					this.Host = Host;
				}
			}



			public class Response 
			{
				public string Status { set; get; }
				public string Mime { set; get; }
				public string Headers { set; get; }
				public byte[] Data { set; get; }

				public static Response OK() { return new Response("200 OK", "text/html", Encoding.ASCII.GetBytes("<h1>200 OK</h1>")); }
				public static Response BadRequest() { return new Response("400 Bad Request", "text/html", Encoding.ASCII.GetBytes("<h1>400 Bad Request</h1>")); }
				public static Response Forbidden() { return new Response("403 Forbidden", "text/html", Encoding.ASCII.GetBytes("<h1>403 Forbidden</h1>")); }
				public static Response NotFound() { return new Response("404 Not Found", "text/html", Encoding.ASCII.GetBytes("<h1>404 Not Found</h1>")); }
				public static Response MethodNotAllowed() { return new Response("405 Method Not Allowed", "text/html", Encoding.ASCII.GetBytes("<h1>405 Method Not Allowed</h1>")); }
				public static Response NotAcceptable() { return new Response("406 Not Acceptable", "text/html", Encoding.ASCII.GetBytes("<h1>406 Not Acceptable</h1>")); }
				public static Response UnsupportedMediaType() { return new Response("415 Unsupported Media Type", "text/html", Encoding.ASCII.GetBytes("<h1>415 Unsupported Media Type</h1>")); }
				public static Response NotImplemented() { return new Response("501 Not Implemented", "text/html", Encoding.ASCII.GetBytes("<h1>501 Not Implemented</h1>")); }

				public static Response GenerateHTTPResponse(HTTPServer server, Request request) 
				{
					if (request == null) { return Response.BadRequest(); }

					if (request.Type == "GET") 
					{
						Response response = Response.OK();
						int paramsIndex = request.URL.IndexOf("?");
						string path = (paramsIndex >= 0) ? request.URL.Remove(paramsIndex) : request.URL;
						if (path[path.Length - 1] == '/') { path = path.TrimEnd('/'); }
						path = WebTools.ReplaceURLChars(path.Replace("/", "\\"));

						int dotIndex = path.LastIndexOf(".");
						string dir = Directory.GetCurrentDirectory() + path;
						if (File.Exists(dir) || dotIndex >= 0) 
						{
							string extention = (dotIndex >= 0) ? path.Substring(dotIndex, path.Length - dotIndex).ToLower() : "";
							try { response.Mime = HTTPServer.MimeTypes[extention]; }
							catch (KeyNotFoundException) 
							{
								Tools.ColorWriteLine(Tools.ConsoleColor.YellowF + "MIME not avaliable for URL: " + request.URL + Tools.ConsoleColor.GrayF);
								return Response.NotAcceptable();
							}

							string file = dir;
							if (!File.Exists(file)) 
							{
								file = file.Replace(path, server.lastHTML + path);
								if (!File.Exists(file)) { return Response.NotFound(); }
							}

							response.Data = File.ReadAllBytes(file);
							return response;
						}
						else 
						{
							if (!Directory.Exists(dir)) { return Response.NotFound(); }
							string[] files = { "index.html", "index.htm", "default.html", "default.htm" };

							for (int i = 0; i < files.Length; i++) { if (File.Exists(dir + files[i])) { response.Data = File.ReadAllBytes(dir + files[i]); server.lastHTML = path; return response; } }

							files = Directory.GetFiles(dir, "*.html", SearchOption.TopDirectoryOnly);
							if (files.Length > 0) { response.Data = File.ReadAllBytes(files[0]); server.lastHTML = path; return response; }

							files = Directory.GetFiles(dir, "*.htm", SearchOption.TopDirectoryOnly);
							if (files.Length > 0) { response.Data = File.ReadAllBytes(files[0]); server.lastHTML = path; return response; }

							return Response.NotFound();
						}
					}
					
					return Response.MethodNotAllowed();
				}

				public static Response GenerateDirectoryResponse(HTTPServer server, Request request) 
				{
					if (request == null) { return Response.BadRequest(); }

					if (request.Type == "GET") 
					{
						Response response = Response.OK();
						int paramsIndex = request.URL.IndexOf("?");
						string path = (paramsIndex >= 0) ? request.URL.Remove(paramsIndex) : request.URL;
						if (path[path.Length - 1] == '/') { path = path.TrimEnd('/'); }
						path = WebTools.ReplaceURLChars(path.Replace("/", "\\"));

						if (File.Exists(server.Name + path)) 
						{
							int dotIndex = path.LastIndexOf(".");
							string extention = (dotIndex >= 0) ? path.Substring(dotIndex, path.Length - dotIndex).ToLower() : "";
							try { response.Mime = HTTPServer.MimeTypes[extention]; }
							catch (KeyNotFoundException) 
							{
								Tools.ColorWriteLine(Tools.ConsoleColor.YellowF + "MIME not avaliable for URL: " + request.URL + Tools.ConsoleColor.GrayF);
								response.Mime = "text/html";
							}

							string file = server.Name + path;
							if (!File.Exists(file)) 
							{
								file = file.Replace(server.Name, server.lastHTML);
								if (!File.Exists(file)) { return Response.NotFound(); }
							}

							if (response.Mime == "application/pdf") { response.AddHeader("Content-Disposition: inline"); response.Data = File.ReadAllBytes(file); return response; }
							if (response.Mime.StartsWith("image/")) { response.Data = File.ReadAllBytes(file); return response; }
							if (response.Mime.StartsWith("audio/")) { response.Mime = "audio/mpeg"; response.Data = File.ReadAllBytes(file); return response; }
							if (response.Mime.StartsWith("video/")) { response.Mime = "video/mp4"; response.Data = File.ReadAllBytes(file); return response; }
							if (extention == ".exe" || extention == ".dll" || extention == ".bin" ||
								extention == ".doc" || extention == ".ppt" || extention == ".xls" ||
								extention == ".docx" || extention == ".pptx" || extention == ".xlsx" ||
								extention == ".blend" || extention == ".deb" || extention == ".psd" ||
								extention == ".rar" || extention == ".zip" || extention == ".7z" ||
								extention == ".lnk" || extention == ".out")
							{ response.Mime = "text/plain"; response.Data = File.ReadAllBytes(file); return response; }

							response.Mime = "text/html";
							response.Data = Encoding.UTF8.GetBytes
							(
								"<html><head>" +
								"<meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\">" +
								"<meta name=\"color-scheme\" content=\"light dark\">" +
								"</head><body>" +
								"<p style=\"font-family: monospace; word-wrap: break-word; white-space: pre-wrap;\">"+
								File.ReadAllText(file).Replace("<", "&lt;").Replace(">", "&gt;") +
								"</p></body></html>"
							);

							return response;
						}
						else 
						{
							string dir = server.Name + path;
							if (!Directory.Exists(dir)) { return Response.NotFound(); }

							bool isRoot = server.Name == dir;
							int t = (!isRoot) ? 1 : 0;
							string[] files = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly);
							string[] directories = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
							string[] items = new string[files.Length + directories.Length + t];

							if (!isRoot) { items[0] = " "; }
							for (int i = 0; i < files.Length; i++) { items[t++] = files[i].Replace(dir + "\\", ""); }
							for (int i = 0; i < directories.Length; i++) { items[t++] = directories[i].Replace(dir + "\\", ""); }
							Array.Sort(items, (x, y) => String.Compare(x, y));
							if (!isRoot) { items[0] = ".."; }

							string output = 
							"<html><head>" +
							"<meta content=\"text/html; charset=utf-8\" http-equiv=\"Content-Type\">" +
							"<meta name=\"color-scheme\" content=\"light dark\">" +
							"</head><body>" +
							"<p style=\"font-family: monospace; word-wrap: break-word; white-space: pre-wrap;\">";
							for (int i = 0; i < items.Length; i++) 
							{
								bool isURL = items[i].EndsWith(".url");
								output += "<a href=\"" +
								((!isURL) ? (path + "\\" + items[i]) : File.ReadAllText(dir + "\\" + items[i]).Replace("[InternetShortcut]\x000d\x000aURL=", "")) +
								"\"" + ((isURL) ? " target=\"_blank\"" : "") + ">" + items[i] +
								((Directory.Exists(dir + "\\" + items[i])) ? "/" : "") +
								"</a>\n";
							}

							output += "</p></body></html>";
							response.Data = Encoding.UTF8.GetBytes(output);
							return response;
						}
					}
					
					return Response.MethodNotAllowed();
				}

				public void AddHeader(string header) { Headers += header + "\r\n"; }
				public void Post(HTTPServer server, NetworkStream stream) 
				{
					StreamWriter writer = new StreamWriter(stream);
					Headers = server.Version + " " + Status + "\r\n" + Headers;
					AddHeader("Content-Type: " + Mime + "; charset=utf-8");
					AddHeader("Accept-Ranges: bytes");
					AddHeader("Content-Length: " + Convert.ToString(Data.Length));
					writer.WriteLine(Headers);
					writer.Flush();

					try { stream.Write(Data, 0, Data.Length); }
					catch (System.IO.IOException) {}
				}

				public Response() {}
				public Response(string Status, string Mime, byte[] Data) 
				{
					this.Status = Status;
					this.Mime = Mime;
					this.Headers = "";
					this.Data = Data;
				}
			}
		}
	}

    public static class Tools 
    {
        /*
        [System.Runtime.InteropServices.DllImport("user32.dll")] // in class main
		static extern bool ShowWindow(IntPtr hWnd, int nCmdShow); // in clas main

		ShowWindow(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle, 0);
		*/

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

		// public static void SetWallpaper(Bitmap bitmap)
		// {
		// 	int style = 1;
		//     // Tiled = 0
		//     // Centered = 1
		//     // Stretched = 2

		//     Image img = Image.FromHbitmap(bitmap.GetHbitmap());
		//     string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
		//     img.Save(tempPath, ImageFormat.Bmp);

		//     RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
		//     if (style == 2)
		//     {
		//         key.SetValue(@"WallpaperStyle", 2.ToString());
		//         key.SetValue(@"TileWallpaper", 0.ToString());
		//     }

		//     if (style == 1)
		//     {
		//         key.SetValue(@"WallpaperStyle", 1.ToString());
		//         key.SetValue(@"TileWallpaper", 0.ToString());
		//     }

		//     if (style == 0)
		//     {
		//         key.SetValue(@"WallpaperStyle", 1.ToString());
		//         key.SetValue(@"TileWallpaper", 1.ToString());
		//     }

		//     SystemParametersInfo(0x14, 0, tempPath, 0x3);
		// }

		// public static void SetWallpaper(string file)
		// {
		// 	int style = 1;
		//     // Tiled = 0
		//     // Centered = 1
		//     // Stretched = 2

		//     Stream s = new WebClient().OpenRead(file);
		//     Image img = Image.FromStream(s);
		//     string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
		//     img.Save(tempPath, ImageFormat.Bmp);

		//     RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
		//     if (style == 2)
		//     {
		//         key.SetValue(@"WallpaperStyle", 2.ToString());
		//         key.SetValue(@"TileWallpaper", 0.ToString());
		//     }

		//     if (style == 1)
		//     {
		//         key.SetValue(@"WallpaperStyle", 1.ToString());
		//         key.SetValue(@"TileWallpaper", 0.ToString());
		//     }

		//     if (style == 0)
		//     {
		//         key.SetValue(@"WallpaperStyle", 1.ToString());
		//         key.SetValue(@"TileWallpaper", 1.ToString());
		//     }

		//     SystemParametersInfo(0x14, 0, tempPath, 0x3);
		// }

	    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
	    [DllImport("user32.dll")] public static extern IntPtr GetDesktopWindow();
	    [DllImport("user32.dll")] public static extern IntPtr GetShellWindow();
	    [DllImport("user32.dll", SetLastError = true)] public static extern int GetWindowRect(IntPtr hwnd, ref RECT rc);

	  //   public static bool IsFocusedOnDesktop() 
	  //   {
	  //   	IntPtr desktopHandle = GetDesktopWindow();
	  //   	IntPtr targetHandle = GetShellWindow();

			// RECT appBounds = new RECT();
			// Rectangle screenBounds;
			// IntPtr hWnd;

			// hWnd = GetForegroundWindow();
			// if (hWnd != null && !hWnd.Equals(IntPtr.Zero))
			// {
			//     if (!(hWnd.Equals(desktopHandle) || hWnd.Equals(targetHandle)))
			//     {
			//         GetWindowRect(hWnd, ref appBounds);
			//         screenBounds = Screen.FromHandle(hWnd).Bounds;
			//         if ((appBounds.Bottom - appBounds.Top) == screenBounds.Height && (appBounds.Right - appBounds.Left) == screenBounds.Width) 
			//         { return true; }
			//     }
			// }

			// return false;
	  //   }

	  //   public static bool IsSomethingFullscreen() 
	  //   {
	  //   	Screen screen = Screen.PrimaryScreen;
	  //   	RECT rect = new RECT();
	  //       GetWindowRect(new HandleRef(null, GetForegroundWindow()).Handle, ref rect);
	  //       Rectangle box = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
	  //       return box.Contains(screen.Bounds) || screen.Bounds.Contains(box); 
	  //   }

	    [StructLayout(LayoutKind.Sequential)]
	    public struct RECT
	    {
	        public int Left;
	        public int Top;
	        public int Right;
	        public int Bottom;
	    }

		public static string GetCurrentFilePath() { return new StackTrace(true).GetFrame(0).GetFileName(); }

        public static void ClearLastLine() 
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth)); 
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void CMD(string command) 
        {
        	//Process.Start("cmd.exe", "/C " + command);
        	var cmd = new Process();
		    cmd.StartInfo = new ProcessStartInfo("C:\\Windows\\System32\\cmd.exe", command) 
			{
				UseShellExecute = false
			};

		    cmd.Start();
		    cmd.WaitForExit();
        }

        public static void StartEXE(string path, string args) 
        {
        	//Process.Start("cmd.exe", "/C " + path + " " + args);
        	var cmd = new Process();
		    cmd.StartInfo = new ProcessStartInfo(path, args) 
			{
				UseShellExecute = false
			};

		    cmd.Start();
		    cmd.WaitForExit();
        }

        public static void ColorWriteLine(string input) { ColorWrite(input + "\n"); }
		public static void ColorWrite(string input) 
		{
			/*
              0 = foreground color, 1 = backgroun color
			  |the actual color from Console.Color
			  ||indicates a color change
			  ||||
			\x07ff
			*/

			int i = 0;
			int color = 7;
			bool useBackground = false;

			while (i < input.Length) 
			{
				int n = (BitConverter.GetBytes(input[i])[1] << 8) + BitConverter.GetBytes(input[i])[0];
				if ((n & 0xff) << 8 == 0xff00) 
				{
					color = (n >> 8) & 0xf;
					useBackground = (((n >> 8) & 0x10) >> 4) == 1;
					i++;
				}

				if (!useBackground) { Console.ForegroundColor = (System.ConsoleColor)color; }
				else { Console.BackgroundColor = (System.ConsoleColor)color; }
				Console.Write(input[i++]);
			}
		}

		public static string FormatDirectory(string input) 
		{
			input = input.Replace("/", "\\");
			string[] folders = input.Split('\\');
			List<string> newFolders = new List<string>();
			string output = "";

			for (int i = 0; i < folders.Length; i++) 
			{
				if (folders[i] == ".") { continue; }
				if (folders[i] != "..") { newFolders.Add(folders[i]); }
				else 
				{
					if (newFolders.Count == 0) { return null; }
					newFolders.RemoveAt(newFolders.Count - 1);
				}
			}

			for (int i = 0; i < newFolders.Count; i++) { output += newFolders[i] + "\\"; }
			return output.TrimEnd('\\');
		}


        // copy paste in .cs Main() file
        /*
        static List<int> usedRandom = new List<int>();
        public static int Random(int start, int end) 
        {
            System.Random random = new System.Random();
            int _new = start;
            int i = 0;

            usedRandom.Add(start - 1);
            while (i < usedRandom.Count) 
            {
                _new = random.Next(start, end);
                if (usedRandom.IndexOf(_new) <= -1) 
                {
                    usedRandom.Add(_new);
                    break;
                }

                i++;
            }

            usedRandom.Remove(start - 1);
            return _new;
        }
        */

        public class Timer 
        {
        	public int delay { set; get; }
        	public bool done { set; get; }

        	public void Stop() { if (thread != null && !done) { thread.Abort(); } }
        	public void Start() 
        	{
        		if (thread != null && !done) { thread.Abort(); }
				thread = new Thread(Timeout);
				thread.Start();
        	}

        	private Thread thread;
        	private void Timeout() 
        	{
        		done = false;
        		Thread.Sleep(delay);
        		done = true;
        	}

			public Timer(int delay) { this.delay = delay; done = true; }
        }

        public class ConsoleColor 
		{
			public static readonly string BlackF       = "\x00ff";
			public static readonly string BlueF        = "\x09ff";
			public static readonly string CyanF        = "\x0bff";
			public static readonly string DarkBlueF    = "\x01ff";
			public static readonly string DarkCyanF    = "\x03ff";
			public static readonly string DarkGrayF    = "\x08ff";
			public static readonly string DarkGreenF   = "\x02ff";
			public static readonly string DarkMagentaF = "\x05ff";
			public static readonly string DarkRedF     = "\x04ff";
			public static readonly string DarkYellowF  = "\x06ff";
			public static readonly string GrayF        = "\x07ff";
			public static readonly string GreenF       = "\x0aff";
			public static readonly string MagentaF     = "\x0dff";
			public static readonly string RedF         = "\x0cff";
			public static readonly string WhiteF       = "\x0fff";
			public static readonly string YellowF      = "\x0eff";

			public static readonly string BlackB       = "\x10ff";
			public static readonly string BlueB        = "\x19ff";
			public static readonly string CyanB        = "\x1bff";
			public static readonly string DarkBlueB    = "\x11ff";
			public static readonly string DarkCyanB    = "\x13ff";
			public static readonly string DarkGrayB    = "\x18ff";
			public static readonly string DarkGreenB   = "\x12ff";
			public static readonly string DarkMagentaB = "\x15ff";
			public static readonly string DarkRedB     = "\x14ff";
			public static readonly string DarkYellowB  = "\x16ff";
			public static readonly string GrayB        = "\x17ff";
			public static readonly string GreenB       = "\x1aff";
			public static readonly string MagentaB     = "\x1dff";
			public static readonly string RedB         = "\x1cff";
			public static readonly string WhiteB       = "\x1fff";
			public static readonly string YellowB      = "\x1eff";
		}
    }

    public static class Format 
    {
    	public static string To1Decimal(float num) 
		{
			string output = System.Convert.ToString(Math.Round((double)num, 1));
			output = (output.IndexOf(".") <= -1) ? output + ".0" : output;
			return output;
		}

		public static string To2Decimals(float num) 
		{
			string output = System.Convert.ToString(Math.Round((double)num, 2));
			output = (output.IndexOf(".") < 0) ? output + ".00" : output;
			output = (output.IndexOf(".") == output.Length - 2) ? output + "0" : output;
			return output;
		}

		public static string To3Decimals(float num) 
		{
			string output = System.Convert.ToString(Math.Round((double)num, 3));
			output = (output.IndexOf(".") < 0) ? output + ".000" : output;
			output = (output.IndexOf(".") == output.Length - 2) ? output + "00" : output;
			output = (output.IndexOf(".") == output.Length - 3) ? output + "0" : output;
			return output;
		}

		public static int HexToDecimal(string hex) 
		{
			if (hex == "" || hex == " ") { return 0; }
			int sum = 0;
			int pow = 0;
			int i = hex.Length - 1;
			char[] hexBy1 = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
			hex = hex.ToLower();
			while (i > -1) 
			{
				sum += Array.IndexOf(hexBy1, hex[i]) * (int)Math.Pow(16, pow);
				pow++;
				i--;
			}

			return sum;
		}

		public static string DecimalToHex(int dec) 
		{
			string[] hexBy1 = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" };
			int num = (int)Math.Floor((double)dec / 16);
			string hex = hexBy1[dec % 16];

			while (num > 0) 
			{
				hex = hexBy1[num % 16] + hex;
				num = (int)Math.Floor((double)num / 16);
			}

			return hex;
		}

		public static int BinaryToDecimal(string bin) 
		{
			if (bin == "" || bin == " ") { return 0; }
			int sum = 0;
			int pow = 0;
			int i = bin.Length - 1;
			bin = bin.ToLower();
			while (i > -1) 
			{
				sum += ((bin[i] == '1') ? 1 : 0) * (int)Math.Pow(2, pow);
				pow++;
				i--;
			}

			return sum;
		}

		public static string DecimalToBinary(int dec) 
		{
			int num = (int)Math.Floor((double)dec / 2);
			string bin = ((dec % 2 == 0) ? "0" : "1");

			while (num > 0) 
			{
				bin = ((num % 2 == 0) ? 0 : 1) + bin;
				num = (int)Math.Floor((double)num / 2);
			}

			return bin;
		}

		public static float itof(int num) { return float.Parse(System.Convert.ToString(num)); }
		public static int ftoi(float num) { return System.Convert.ToInt32(num); }
		public static float stof(string num) { return float.Parse(num); }
		public static string ftos(float num) { return System.Convert.ToString(num); }
		public static int stoi(string num) { return System.Convert.ToInt32(num); }
		public static string itos(int num) { return System.Convert.ToString(num); }

		public static float ToMinutes(string hoursAndMinutes) 
		{
			if (hoursAndMinutes.IndexOf(".") >= 0 || hoursAndMinutes == "" || hoursAndMinutes == null || hoursAndMinutes.IndexOf(":") <= -1) { return 0f; }
			string[] time = hoursAndMinutes.Replace(":", " ").Split();
			if (time.Length != 2) { return 0f; }
			return (float.Parse(time[0]) * 60f) + float.Parse(time[1]);
		}

		public static float ToSeconds(string minutesAndSeconds) 
		{
			if (minutesAndSeconds.IndexOf(".") >= 0 || minutesAndSeconds == "" || minutesAndSeconds == null || minutesAndSeconds.IndexOf(":") <= -1) { return 0f; }
			string[] time = minutesAndSeconds.Replace(":", " ").Split();
			if (time.Length != 2) { return 0f; }
			return (float.Parse(time[0]) * 60f) + float.Parse(time[1]);
		}

		public static string ToHoursAndMinutes(float minutes) 
		{
			if (minutes < 0) { return "0:00"; }
			float newHours = (float)Math.Floor((double)(minutes / 60f));
			float newMinutes = (float)Math.Abs((double)(minutes - (newHours * 60)));
			string newMinutess = System.Convert.ToString(newMinutes);
			return System.Convert.ToString(newHours) + ":" + ((newMinutess.Length == 1) ? ("0" + newMinutess) : newMinutess);
		}

		public static string ToHoursAndMinutes(string minutes) 
		{
			float minutesf = float.Parse(minutes);
			if (minutes == "" || minutes == " " || minutesf < 0) { return "0:00"; }
			float newHours = (float)Math.Floor((double)(minutesf / 60f));
			float newMinutes = (float)Math.Abs((double)(minutesf - (newHours * 60)));
			string newMinutess = System.Convert.ToString(newMinutes);
			return System.Convert.ToString(newHours) + ":" + ((newMinutess.Length == 1) ? ("0" + newMinutess) : newMinutess);
		}

		public static string ToMinutesAndSeconds(float seconds) 
		{
			if (seconds < 0) { return "0:00"; }
			float newMinutes = (float)Math.Floor((double)(seconds / 60f));
			float newSeconds = (float)Math.Abs((double)(seconds - (newMinutes * 60)));
			string newSecondss = System.Convert.ToString(newSeconds);
			return System.Convert.ToString(newMinutes) + ":" + ((newSecondss.Length == 1) ? ("0" + newSecondss) : newSecondss);
		}

		public static string ToMinutesAndSeconds(string seconds) 
		{
			float secondsf = float.Parse(seconds);
			if (seconds == "" || seconds == " " || secondsf < 0) { return "0:00"; }
			float newMinutes = (float)Math.Floor((double)(secondsf / 60f));
			float newSeconds = (float)Math.Abs((double)(secondsf - (newMinutes * 60)));
			string newSecondss = System.Convert.ToString(newSeconds);
			return System.Convert.ToString(newMinutes) + ":" + ((newSecondss.Length == 1) ? ("0" + newSecondss) : newSecondss);
		}
    }

    public static class Math2 
    {
        public static float PI { get { return 3.1415926535897932384626433832795f; } }
        public static float TAU { get { return 6.283185307179586476925286766559f; } }
        public static float E { get { return 2.7182818284590452353602874713527f; } }
        public static float goldenRatio { get { return 1.6180339887498948482045868343657f; } }
        public static float DEG2RAD { get { return 0.01745329251994329576923690768489f; } }
        public static float RAD2DEG { get { return 57.295779513082320876798154814105f; } }
        public static float EPSILON { get { return 0.0001f; } }

        public static float e(float x) { return (float)Math.Pow((double)E, (double)x); }
	    public static float Sqrt(float x) { return (float)Math.Sqrt((double)x); }
	    public static float Abs(float x) { return (x < 0f) ? x * -1f : x; }
	    public static float Ceiling(float x) { return (float)Math.Ceiling((double)x); }
	    public static float Floor(float x) { return (float)Math.Floor((double)x); }
	    public static float Max(float a, float b) { return (a > b) ? a : b; }
	    public static float Min(float a, float b) { return (a < b) ? a : b; }
	    public static float Sin(float x) { return (float)Math.Sin((double)x); }
	    public static float Cos(float x) { return (float)Math.Cos((double)x); }
	    public static float Tan(float x) { return (float)Math.Tan((double)x); }
	    public static float Asin(float x) { return (float)Math.Asin((double)x); }
	    public static float Acos(float x) { return (float)Math.Acos((double)x); }
	    public static float Atan(float x) { return (float)Math.Atan((double)x); }
	    public static float Atan2(float y, float x) { return (float)Math.Atan2((double)y, (double)x); }
	    public static float Log(float x) { return (float)Math.Log((double)x); }
	    public static float Log(float x, float a) { return (float)Math.Log((double)x, (double)a); }
	    public static float Clamp(float a, float b, float x) { return Max(a, Min(x, b)); }
	    public static Vector3 Clamp(Vector3 a, Vector3 b, Vector3 x) { return (Distance(a, b) < Distance(a, x)) ? b : ((Distance(a, b) < Distance(b, x)) ? a : x); }
	    public static Vector2 Clamp(Vector2 a, Vector2 b, Vector2 x) { return (Distance(a, b) < Distance(a, x)) ? b : ((Distance(a, b) < Distance(b, x)) ? a : x); }
	    public static float InverseLerp(float a, float b, float x) { return (x - a) / (b - a); }
	    public static float Lerp(float a, float b, float x) { return a + (x * (b - a)); }
        public static Vector2 Lerp(Vector2 a, Vector2 b, float x) { return new Vector2(Lerp(a.x, b.x, x), Lerp(a.y, b.y, x)); }
        public static Vector3 Lerp(Vector3 a, Vector3 b, float x) { return new Vector3(Lerp(a.x, b.x, x), Lerp(a.y, b.y, x), Lerp(a.z, b.z, x)); }
        public static float SmoothMax(float a, float b, float x) { return a * Clamp(0f, 1f, (b - a + x) / (2f * x)) + b * (1f - Clamp(0f, 1f, (b - a + x) / (2f * x))) - x * Clamp(0f, 1f, (b - a + x) / (2f * x)) * (1f - Clamp(0f, 1f, (b - a + x) / (2f * x))); }
        public static float SmoothMin(float a, float b, float x) { return SmoothMax(a, b, -1f * x); }
        public static float SmoothStep(float a, float b, float x) { float t = Clamp(0, 1, (x - a) / (b - a)); return t * t * (3f - (2f * t)); }
        public static float GetInt(float x) { return (int)x; }
		public static float GetDecimal(float x) { return x - (float)((int)x); }

        public static float DistanceSquared(Vector3 a, Vector3 b) { return ((b.x - a.x) * (b.x - a.x)) + ((b.y - a.y) * (b.y - a.y)) + ((b.z - a.z) * (b.z - a.z)); }
        public static float DistanceSquared(Vector2 a, Vector2 b) { return ((b.x - a.x) * (b.x - a.x)) + ((b.y - a.y) * (b.y - a.y)); }
        public static float Distance(Vector3 a, Vector3 b) { return Sqrt(((b.x - a.x) * (b.x - a.x)) + ((b.y - a.y) * (b.y - a.y)) + ((b.z - a.z) * (b.z - a.z))); }
        public static float Distance(Vector2 a, Vector2 b) { return Sqrt(((b.x - a.x) * (b.x - a.x)) + ((b.y - a.y) * (b.y - a.y))); }
        public static float Length(Vector3 a) { return Sqrt((a.x * a.x) + (a.y * a.y) + (a.z * a.z)); }
        public static float Length(Vector2 a) { return Sqrt((a.x * a.x) + (a.y * a.y)); }
        public static Vector3 Normalize(Vector3 a) { return a / Length(a); }
        public static Vector2 Normalize(Vector2 a) { return a / Length(a); }
        public static float Dot(Vector3 a, Vector3 b) { return (a.x * b.x) + (a.y * b.y) + (a.z * b.z); }
        public static float Dot(Vector2 a, Vector2 b) { return (a.x * b.x) + (a.y * b.y); }
        public static Vector3 Cross(Vector3 a, Vector3 b) { return new Vector3((a.y*b.z) - (a.z*b.y), (a.z*b.x) - (a.x*b.z), (a.x*b.y) - (a.y*b.x)); }
        public static float Square(float a) { return a * a; }
        public static Vector2 Square(Vector2 a) { return new Vector2(a.x * a.x, a.y * a.y); }
        public static Vector3 Square(Vector3 a) { return new Vector3(a.x * a.x, a.y * a.y, a.z * a.z); }

        public static float AngleBetweenVectors(Vector2 a, Vector2 b) { return Acos(CosBetweenVectors(a, b)); }
        public static float AngleBetweenVectors(Vector3 a, Vector3 b) { return Acos(CosBetweenVectors(a, b)); }
        public static float SinBetweenVectors(Vector2 a, Vector2 b) { return Sqrt(1f - (CosBetweenVectors(a, b)*CosBetweenVectors(a, b))); }
        public static float SinBetweenVectors(Vector3 a, Vector3 b) { return Sqrt(1f - (CosBetweenVectors(a, b)*CosBetweenVectors(a, b))); }
        public static float CosBetweenVectors(Vector2 a, Vector2 b) { return Dot(a, b) / (Length(a) * Length(b)); }
        public static float CosBetweenVectors(Vector3 a, Vector3 b) { return Dot(a, b) / (Length(a) * Length(b)); }

        public static Vector3 PerpendicularToLine(Vector3 a, Vector3 b, Vector3 c) 
        {
        	float da = ((b.x - c.x) * (b.x - c.x)) + ((b.y - c.y) * (b.y - c.y)) + ((b.z - c.z) * (b.z - c.z));
        	float db = ((a.x - c.x) * (a.x - c.x)) + ((a.y - c.y) * (a.y - c.y)) + ((a.z - c.z) * (a.z - c.z));
        	float dc = ((b.x - a.x) * (b.x - a.x)) + ((b.y - a.y) * (b.y - a.y)) + ((b.z - a.z) * (b.z - a.z));
        	float t = -((da - db - dc) / (2f * dc));
        	return a + (t * (b - a));
        }

        public static Vector2 PerpendicularToLine(Vector2 a, Vector2 b, Vector2 c) 
        {
        	float da = ((b.x - c.x) * (b.x - c.x)) + ((b.y - c.y) * (b.y - c.y));
        	float db = ((a.x - c.x) * (a.x - c.x)) + ((a.y - c.y) * (a.y - c.y));
        	float dc = ((b.x - a.x) * (b.x - a.x)) + ((b.y - a.y) * (b.y - a.y));
        	float t = -((da - db - dc) / (2f * dc));
        	return a + (t * (b - a));
        }

        public static float AreaOfTriangle(Vector3 a, Vector3 b, Vector3 c) { return (Distance(a, b) * Distance(c, PerpendicularToLine(a, b, c))) / 2f; }
        public static float AreaOfTriangle(Vector2 a, Vector2 b, Vector2 c) { return (Distance(a, b) * Distance(c, PerpendicularToLine(a, b, c))) / 2f; }

        public static float DistanceToSquare(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 p) 
        {
        	if (b.x < a.x) { Vector2 tmp = a; a = b; b = tmp; }
        	if (c.x < d.x) { Vector2 tmp = d; d = c; c = tmp; }
        	if (a.y > d.y) { Vector2 tmp = a; a = d; d = tmp; }
        	if (b.y > c.y) { Vector2 tmp = b; b = c; c = tmp; }

        	float tX = -((DistanceSquared(b, p) - DistanceSquared(a, p) - DistanceSquared(a, b)) / (2f * DistanceSquared(a, b)));
        	float tY = -((DistanceSquared(c, p) - DistanceSquared(b, p) - DistanceSquared(c, b)) / (2f * DistanceSquared(c, b)));

        	Vector2 hUp = Clamp(d, c, d + (tY * (c - d)));
    		Vector2 hDown = Clamp(a, b, a + (tY * (b - a)));
    		Vector2 hLeft = Clamp(a, d, a + (tX * (d - a)));
    		Vector2 hRight = Clamp(c, b, b + (tX * (c - b)));

    		float output = Min(Min(Distance(p, hUp), Distance(p, hDown)), Min(Distance(p, hLeft), Distance(p, hRight)));
    		return (hUp.y - p.y > 0f && p.y - hDown.y > 0f && p.x - hLeft.x > 0f && p.x - hRight.x < 0f) ? -output : output;
        }

        public static Vector2 RaySphere(Vector3 center, float radius, Vector3 rayOrigin, Vector3 rayDir) // returns Vector2(distance to sphere, distance inside sphere)
		{
		    float a = 1f;
		    Vector3 offset = new Vector3(rayOrigin.x - center.x, rayOrigin.y - center.y, rayOrigin.z - center.z);
		    float b = 2f * Dot(offset, rayDir);
		    float c = Dot(offset, offset) - radius * radius;

		    float disciminant = b * b - 4f * a * c;

		    if (disciminant > 0f) 
		    {
		        float s = Sqrt(disciminant);
		        float dstToSphereNear = Max(0f, (-b - s) / (2f * a));
		        float dstToShpereFar = (-b + s) / (2f * a);

		        if (dstToShpereFar >= 0f) 
		        {
		            return new Vector2(dstToSphereNear, dstToShpereFar - dstToSphereNear);
		        }
		    }

		    return new Vector2(-1f, 0f);
		}

        public static Vector2 Rotate(Vector2 origin, Vector2 point, float angle) 
        {
            float x = origin.x + ((point.x - origin.x) * Cos(angle)) + ((point.y - origin.y) * Sin(angle));
            float y = origin.y + ((point.x - origin.x) * Sin(angle)) + ((point.y - origin.y) * Cos(angle));

            return new Vector2(x, y);
        }

        public static Vector3 MidPoint(Vector3 a, Vector3 b) { return new Vector3((a.x + b.x) / 2, (a.y + b.y) / 2, (a.z + b.z) / 2); }
        public static Vector2 MidPoint(Vector2 a, Vector2 b) { return new Vector2((a.x + b.x) / 2, (a.y + b.y) / 2); }
        public static Vector3 MovePoint(Vector3 a, Vector3 b, float distance) { return a + (distance * Normalize(b - a)); }
        public static Vector2 MovePoint(Vector2 a, Vector2 b, float distance) { return a + (distance * Normalize(b - a)); }
        public static Vector3 MovePoint01(Vector3 a, Vector3 b, float distance) { return Lerp(a, b, distance); }
        public static Vector2 MovePoint01(Vector2 a, Vector2 b, float distance) { return Lerp(a, b, distance); }

        public static float Pow(float a, float b) { return (float)Math.Pow((double)a, (double)b); }
        public static long Pow(int num, int pow) 
        {
            long newNum = num;
            if (pow < 0) { return 0; }
            if (pow == 0) { return 1; }
            while (pow - 1 > 0) { newNum *= num; pow--; }
            return newNum;
        }

        public static long Fact(int x) 
        {
            long newNum = 1;
            while (x > 0) 
            {
                newNum *= x;
                x--;
            }

            return newNum;
        }

        public static bool IsPrime(int x)
		{
		    if (x < 1) { return false; }
		    if (x == 1) { return true; }
		 
		    for (int i = 2; i <= (int)Math.Sqrt((double)x); i++) 
		    {
		        if (x % i == 0) { return false; }
		    }
		 
		    return true;
		}

		public static Vector3 MousePositionToWorldXY(Vector3 mousePosition) 
		{
			Ray mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(mousePosition);
			float distanceToDrawPlane = -mouseRay.origin.z / mouseRay.direction.z;
			return mouseRay.origin + (mouseRay.direction * distanceToDrawPlane);
		}

		public static Vector3 MousePositionToWorldZY(Vector3 mousePosition) 
		{
			Ray mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(mousePosition);
			float distanceToDrawPlane = -mouseRay.origin.x / mouseRay.direction.x;
			return mouseRay.origin + (mouseRay.direction * distanceToDrawPlane);
		}

		public static Vector3 MousePositionToWorldXZ(Vector3 mousePosition) 
		{
			Ray mouseRay = UnityEditor.HandleUtility.GUIPointToWorldRay(mousePosition);
			float distanceToDrawPlane = -mouseRay.origin.y / mouseRay.direction.y;
			return mouseRay.origin + (mouseRay.direction * distanceToDrawPlane);
		}
    }

    public static class Mesh2 
    {
    	/*
        public static Mesh CombineMeshes(MeshFilter[] filters) 
        {
            CombineInstance[] combiner = new CombineInstance[filters.Length];

            for (int i = 0; i < filters.Length; i++)
            {
                combiner[i].subMeshIndex = 0;
                combiner[i].mesh = filters[i].sharedMesh;
                combiner[i].transform = filters[i].transform.localToWorldMatrix;
            }

            Mesh newMesh = new Mesh();
            newMesh.CombineMeshes(combiner);

            return newMesh;
        }
        */

        public static Mesh Icosahedron() 
		{
			Mesh mesh = new Mesh();
			Vector3[] newVertices = new Vector3[12];
			float numA = 3f;
			float numB = 2f;
			int a = 0;
			int b = 4;
			int c = 8;
			
			newVertices[0] = new Vector3(numA / 2f, numB / 2f, 0f);
			newVertices[1] = new Vector3(numA / 2f, -numB / 2f, 0f);
			newVertices[2] = new Vector3(-numA / 2f, -numB / 2f, 0f);
			newVertices[3] = new Vector3(-numA / 2f, numB / 2f, 0f);

			newVertices[4] = new Vector3(numB / 2f, 0f, numA / 2f);
			newVertices[5] = new Vector3(numB / 2f, 0f, -numA / 2f);
			newVertices[6] = new Vector3(-numB / 2f, 0f, -numA / 2f);
			newVertices[7] = new Vector3(-numB / 2f, 0f, numA / 2f);

			newVertices[8] = new Vector3(0f, numA / 2f, numB / 2f);
			newVertices[9] = new Vector3(0f, -numA / 2f, numB / 2f);
			newVertices[10] = new Vector3(0f, -numA / 2f, -numB / 2f);
			newVertices[11] = new Vector3(0f, numA / 2f, -numB / 2f);

			int[] newTriangles = { a+2, a+3, b+2, a+2, b+2, c+2, b+1, c+2, b+2, c+3, b+2, a+3, c+3, b+1, b+2, c+3, a+0, b+1, c+3, c+0, a+0, c+0, b+0, a+0, c+0, b+3, b+0, c+3, a+3, c+0, c+0, a+3, b+3, a+3, a+2, b+3, b+3, a+2, c+1, b+0, b+3, c+1, a+0, b+0, a+1, b+0, c+1, a+1, b+1, a+0, a+1, b+1, a+1, c+2, c+2, a+1, c+1, c+2, c+1, a+2 };
			mesh.vertices = newVertices;
			mesh.triangles = newTriangles;
			return mesh;
		}

		public static Mesh IcoSphere(int resolution) 
		{
			Mesh mesh = Icosahedron();
			Vector3[] borderVertices = new Vector3[2];
			Vector3[] previousBorderVertices = new Vector3[2];
			List<Vector3> newVertices = new List<Vector3>();
			List<int> newTriangles = new List<int>();
			float section = 0f;
			float lineSection = 0f;

			int pointsInside = 0;
			int triangleIndex = 0;
			int line = 0;
			int t = 0;
			int v = 0;

			v = 0; while (v < mesh.vertices.Length) { newVertices.Add(mesh.vertices[v]); v++; }

			while (t < mesh.triangles.Length) 
			{
				borderVertices[0] = mesh.vertices[mesh.triangles[t]];
				borderVertices[1] = mesh.vertices[mesh.triangles[t + 1]];
				previousBorderVertices[0] = borderVertices[0];
				previousBorderVertices[1] = borderVertices[1];

				v = 0;
				lineSection = 1f / ((float)resolution + 1f);
				while (v < resolution) 
				{
					newVertices.Add(Math2.MovePoint01(borderVertices[0], borderVertices[1], lineSection));
					lineSection += 1f / ((float)resolution + 1f);
					v++;
				}
				
				line = 0;
				lineSection = 1f / ((float)resolution + 1f);
				while (line < resolution) 
				{
					newVertices.Add(Math2.MovePoint01(mesh.vertices[mesh.triangles[t]], mesh.vertices[mesh.triangles[t + 2]], lineSection));
					borderVertices[0] = newVertices[newVertices.Count - 1];
					newVertices.Add(Math2.MovePoint01(mesh.vertices[mesh.triangles[t + 1]], mesh.vertices[mesh.triangles[t + 2]], lineSection));
					borderVertices[1] = newVertices[newVertices.Count - 1];
					lineSection += 1f / ((float)resolution + 1f);

					v = 0;
					pointsInside = 0;
					section = 1f / (float)(resolution - line);
					while (v < resolution - 1 - line) 
					{
						newVertices.Add(Math2.MovePoint01(borderVertices[0], borderVertices[1], section));
						section += 1f / (float)(resolution - line);
						pointsInside++;
						v++;
					}

					triangleIndex = 0;
					while (triangleIndex < resolution - line) 
					{
						newTriangles.Add(newVertices.Count - ((2 + pointsInside + (resolution - line)) - triangleIndex));
						newTriangles.Add(newVertices.LastIndexOf(borderVertices[0]) + ((triangleIndex == 0) ? 0 : (2 + triangleIndex - 1)));
						newTriangles.Add((triangleIndex == 0) ? newVertices.LastIndexOf(previousBorderVertices[0]) : newVertices.Count - ((2 + pointsInside + (resolution - line)) - (triangleIndex - 1)));

						newTriangles.Add(newVertices.Count - ((2 + pointsInside + (resolution - line)) - triangleIndex));
						newTriangles.Add(((resolution - line) - 1 - triangleIndex <= 0) ? newVertices.LastIndexOf(borderVertices[1]) : newVertices.Count - ((resolution - line) - 1 - triangleIndex));
						newTriangles.Add(newVertices.LastIndexOf(borderVertices[0]) + ((triangleIndex == 0) ? 0 : (2 + triangleIndex - 1)));

						triangleIndex++;
					}
					
					triangleIndex--;
					newTriangles.Add(newVertices.Count - ((2 + pointsInside + (resolution - line)) - triangleIndex));
					newTriangles.Add(newVertices.LastIndexOf(previousBorderVertices[1]));
					newTriangles.Add(((resolution - line) - 1 - triangleIndex <= 0) ? newVertices.LastIndexOf(borderVertices[1]) : newVertices.Count - ((resolution - line) - 1 - triangleIndex));

					previousBorderVertices[0] = borderVertices[0];
					previousBorderVertices[1] = borderVertices[1];
					line++;
				}
				
				line--;
				triangleIndex--;
				newTriangles.Add(((resolution - line) - 1 - triangleIndex <= 0) ? newVertices.LastIndexOf(borderVertices[1]) : newVertices.Count - ((resolution - line) - 1 - triangleIndex));
				newTriangles.Add(mesh.triangles[t + 2]);
				newTriangles.Add(newVertices.LastIndexOf(borderVertices[0]) + ((triangleIndex == 0) ? 0 : (2 + triangleIndex - 1)));
				t += 3;
			}

			mesh.vertices = newVertices.ToArray();
			mesh.triangles = newTriangles.ToArray();
			return mesh;
		}
    }

    public static class String2 
    {
        public static string en { get { return "abcdefghijklmnopqrstuvwxyz"; } }
        public static string bg { get { return "авбгдежзийклмнопрстуфхцчшщъьюя"; } }
        public static string chars { get { return "!\"#$%&'()*+,-./0123456789:;<=>?@[\\]^_`{|}~ "; } }

        public static bool IsNullOrEmpty(string _main) { return _main == null || _main == ""; }

        public static string JoinArray(string[] array, string separator) 
        {
        	int i = 0;
        	string newString = "";
        	while (i < array.Length) { newString += array[i] + separator; i++; }
        	return newString.Remove(newString.Length - separator.Length, separator.Length);
        }

		public static string Remove(string _main, int startI) 
		{
			if (IsNullOrEmpty(_main) || startI < 0 || startI >= _main.Length) { return _main; }

			int i = 0;
			string output = "";
			while (i < startI) { output += _main[i]; i++; }
			return output;
		}

		public static string Remove(string _main, int startI, int count) 
		{
			if (IsNullOrEmpty(_main) || startI + count > _main.Length || count <= 0 || count > _main.Length || startI < 0 || startI >= _main.Length) { return _main; }

			int i = 0;
			int s = 0;
			string output = "";
			while (i < _main.Length) 
			{
				if (i == startI) { i += count; }
				output += _main[i];
				s++;
				i++;
			}

			return output;
		}

		public static string Reverse(string _main) 
		{
		    if (IsNullOrEmpty(_main)) { return _main; }

		    string output = "";
		    for (int i = _main.Length - 1; i >= 0; i--) { output += _main[i]; }
		    return _main;
		}

		public static string GetStringAt(string _main, int startPos, int endPos) 
		{
		    if (IsNullOrEmpty(_main) || startPos < 0 || endPos <= 0 || endPos < startPos || endPos >= _main.Length) { return ""; }

		    string output = "";
		    int i = startPos;
		    int t = 0;
		    while (t < (endPos - startPos) + 1) 
		    {
		        output += _main[i];
		        i++;
		        t++;
		    }

		    return output;
		}

		public static bool StartsWith(string _main, string _target) 
		{
			if (IsNullOrEmpty(_main) || IsNullOrEmpty(_target) || _target.Length > _main.Length) { return false; }

		    int i = 0;
		    bool pass = true;
		    while (i < _target.Length && pass) 
		    {
		        if (_main[i] != _target[i]) { pass = false; }
		        i++;
		    }

		    return pass;
		}

		public static bool EndsWith(string _main, string _target) 
		{
			if (IsNullOrEmpty(_main) || IsNullOrEmpty(_target) || _target.Length > _main.Length) { return false; }

		    int i = _main.Length - 1;
		    int t = _target.Length - 1;
		    bool pass = true;
		    while (i >= _main.Length - _target.Length && pass) 
		    {
		        if (_main[i] != _target[t]) { pass = false; }
		        i--;
		        t--;
		    }

		    return pass;
		}

		public static int IndexOf(string _main, string _target) 
		{
			if (IsNullOrEmpty(_main) || IsNullOrEmpty(_target) || _target.Length > _main.Length) { return -1; }

			int i = 0;
			int t = 0;
			while (i < _main.Length) 
			{
				t = 0;
				while (t < _target.Length) 
				{
					if (_main[i + t] != _target[t]) { break; }
					t++;
				}

				if (t >= _target.Length) { return i; }
				i++;
			}

			return -1;
		}

		public static int IndexOf(string _main, string _target, int startI) 
		{
			if (IsNullOrEmpty(_main) || IsNullOrEmpty(_target) || _target.Length > _main.Length || startI < 0 || startI >= _main.Length) { return -1; }

			int i = startI;
			int t = 0;
			while (i < _main.Length) 
			{
				t = 0;
				while (t < _target.Length) 
				{
					if (_main[i + t] != _target[t]) { break; }
					t++;
				}

				if (t >= _target.Length) { return i; }
				i++;
			}

			return -1;
		}

		public static int LastIndexOf(string _main, string _target) 
		{
			if (IsNullOrEmpty(_main) || IsNullOrEmpty(_target) || _target.Length > _main.Length) { return -1; }

			int i = _main.Length - _target.Length;
			int t = 0;
			while (i >= 0) 
			{
				t = 0;
				while (t < _target.Length) 
				{
					if (_main[i + t] != _target[t]) { break; }
					t++;
				}

				if (t >= _target.Length) { return i; }
				i--;
			}

			return -1;
		}

		public static bool Contains(string _main, string _target) 
		{
			if (IsNullOrEmpty(_main) || IsNullOrEmpty(_target) || _target.Length > _main.Length) { return false; }

			int i = 0;
			int t = 0;
			while (i < _main.Length) 
			{
				t = 0;
				while (t < _target.Length) 
				{
					if (_main[i + t] != _target[t]) { break; }
					t++;
				}

				if (t >= _target.Length) { return true; }
				i++;
			}

			return false;
		}

		public static int Count(string _main, char _target) 
		{
			if (IsNullOrEmpty(_main) || _target == '\x0') { return 0; }

			int count = 0;
			for (int i = 0; i < _main.Length; i++) { count += (_main[i] == _target) ? 1 : 0; }
			return count;
		}

		public static int Count(string _main, string _target) 
		{
			if (IsNullOrEmpty(_main) || IsNullOrEmpty(_target) || _target.Length > _main.Length) { return 0; }

			int i = IndexOf(_main, _target);
			int count = 0;

			while (i >= 0) 
			{
				i = IndexOf(_main, _target, i + 1);
				count++;
			}

			return count;
		}

		public static string Insert(string _main, string _target, int index) 
		{
			if (IsNullOrEmpty(_main) || IsNullOrEmpty(_target) || index < 0 || index >= _main.Length) { return _main; }

			string output = "";
			for (int i = 0; i < _main.Length; i++) 
			{
				if (i == index) { output += _target; }
				output += _main[i];
			}

			return output;
		}

		public static string Replace(string _main, string _from, string _to) 
		{
			if (IsNullOrEmpty(_main) || IsNullOrEmpty(_from) || IsNullOrEmpty(_to) || _from.Length > _main.Length) { return _main; }

			int i = IndexOf(_main, _from);
			if (i < 0) { return  _main; }

			string removed = Remove(_main, i, _from.Length);
			string inserted = Insert(removed, _to, i);

			return (Contains(inserted, _from)) ? Replace(inserted, _from, _to) : inserted;
		}

		public static string Replace(string _main, string _from, string _to, int count) 
		{
			if (count <= 0 || IsNullOrEmpty(_main) || IsNullOrEmpty(_from) || IsNullOrEmpty(_to) || _from.Length > _main.Length) { return _main; }

			int i = IndexOf(_main, _from);
			if (i < 0) { return  _main; }

			string removed = Remove(_main, i, _from.Length);
			string inserted = Insert(removed, _to, i);

			return (Contains(inserted, _from)) ? Replace(inserted, _from, _to, count - 1) : inserted;
		}

		public static string FillString(string _main, char _target) 
		{
			if (IsNullOrEmpty(_main) || _target == '\x0') { return _main; }

			string output = "";
			for (int i = 0; i < _main.Length; i++) { output += _target; }
			return output;
		}

		public static string FillString(char _target, int count) 
		{
			if (_target == '\x0' || count <= 0) { return ""; }

			string output = "";
			for (int i = 0; i < count; i++) { output += _target; }
			return output;
		}
    }

    public static class Array2<T> 
    {
    	// public static T[] Join(T[] array1, T[] array2) 
    	// {
    	// 	T[] newArray = new T[array1.Length + array2.Length];
    	// 	for (int i = 0; i < array1.Length; i++) { newArray[i] = array1[i]; }
    	// 	for (int i = 0; i < array2.Length; i++) { newArray[i] = array2[i]; }
    	// 	return newArray;
    	// }

    	public static T[] Join(params T[][] array) 
    	{
    		List<T> list = new List<T>();
    		for (int i = 0; i < array.Length; i++) 
    		{
    			for (int t = 0; t < array[i].Length; t++) 
    			{
    				list.Add(array[i][t]);
    			}
    		}

    		return list.ToArray();
    	}

    	// public static T[] Join(T[] array1, T[] array2, T[] array3) 
    	// {
    	// 	T[] newArray = new T[array1.Length + array2.Length + array3.Length];
    	// 	for (int i = 0; i < array1.Length; i++) { newArray[i] = array1[i]; }
    	// 	for (int i = 0; i < array2.Length; i++) { newArray[i] = array2[i]; }
    	// 	for (int i = 0; i < array3.Length; i++) { newArray[i] = array3[i]; }
    	// 	return newArray;
    	// }

    	public static T[,] Replace(T[,] array, T from, T to) 
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            int x = 0;
            int y = 0;

            while (y < height) 
            {
                x = 0;
                while (x < width) 
                {
                    if (EqualityComparer<T>.Default.Equals(array[x, y], from)) { array[x, y] = to; }
                }

                y++;
            }

            return array;
        }

        public static T[,] Replace(T[,] array, T from, T to, int count) 
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            int x = 0;
            int y = 0;

            while (y < height) 
            {
                x = 0;
                while (x < width) 
                {
                    if (EqualityComparer<T>.Default.Equals(array[x, y], from)) { array[x, y] = to; count--; if (count <= 0) { return array; } }
                }

                y++;
            }

            return array;
        }

    	public static T[] Shuffle(T[] array)  
        {  
            int n = array.Length;
            System.Random random = new System.Random();
            while (n > 1) 
            {  
                n--;
                int k = random.Next(n + 1);
                T tmp = array[k];
                array[k] = array[n];
                array[n] = tmp;
            }

            return array;
        }
    }
}