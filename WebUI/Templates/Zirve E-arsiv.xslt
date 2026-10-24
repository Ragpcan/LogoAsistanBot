<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:cac="urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"
                xmlns:cbc="urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"
                xmlns:ccts="urn:un:unece:uncefact:documentation:2"
                xmlns:clm54217="urn:un:unece:uncefact:codelist:specification:54217:2001"
                xmlns:clm5639="urn:un:unece:uncefact:codelist:specification:5639:1988"
                xmlns:clm66411="urn:un:unece:uncefact:codelist:specification:66411:2001"
                xmlns:clmIANAMIMEMediaType="urn:un:unece:uncefact:codelist:specification:IANAMIMEMediaType:2003"
                xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:link="http://www.xbrl.org/2003/linkbase"
                xmlns:n1="urn:oasis:names:specification:ubl:schema:xsd:Invoice-2"
                xmlns:qdt="urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2"
                xmlns:udt="urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2"
                xmlns:xbrldi="http://xbrl.org/2006/xbrldi" xmlns:xbrli="http://www.xbrl.org/2003/instance"
                xmlns:xdt="http://www.w3.org/2005/xpath-datatypes" xmlns:xlink="http://www.w3.org/1999/xlink"
                xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                xmlns:ext="urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2"
                xmlns:ds="http://www.w3.org/2000/09/xmldsig#"
                xmlns:xades="http://uri.etsi.org/01903/v1.3.2#"
                exclude-result-prefixes="cac cbc ccts clm54217 clm5639 clm66411 clmIANAMIMEMediaType fn link n1 qdt udt xbrldi xbrli xdt xlink xs xsd xsi ext ds xades">
  <xsl:decimal-format name="european" decimal-separator="," grouping-separator="." NaN=""/>
  <xsl:output version="4.0" method="html" indent="no" encoding="UTF-8" doctype-public="-//W3C//DTD HTML 4.01 Transitional//EN" doctype-system="http://www.w3.org/TR/html4/loose.dtd"/>
  <xsl:param name="SV_OutputFormat" select="'HTML'"/>
  <xsl:variable name="XML" select="/"/>
  <xsl:template match="/"><html>
 <head>
  <script type="text/javascript"><![CDATA[var QRCode; !function () {
	            function a(a) {
		            this.mode = c.MODE_8BIT_BYTE, this.data = a, this.parsedData = [];
		            for (var b = [], d = 0, e = this.data.length; e > d; d++) {
			            var f = this.data.charCodeAt(d); f > 65536 ? (b[0] = 240 | (1835008 & f) >>> 18,
				            b[1] = 128 | (258048 & f) >>> 12, b[2] = 128 | (4032 & f) >>> 6, b[3] = 128 | 63 & f) : f > 2048 ? (b[0] = 224 | (61440 & f) >>> 12, b[1] = 128 | (4032 & f) >>> 6,
					            b[2] = 128 | 63 & f) : f > 128 ? (b[0] = 192 | (1984 & f) >>> 6,
						            b[1] = 128 | 63 & f) : b[0] = f, this.parsedData = this.parsedData.concat(b)
		            }
		            this.parsedData.length != this.data.length && (this.parsedData.unshift(191), this.parsedData.unshift(187),
			            this.parsedData.unshift(239))
	            } function b(a, b) {
		            this.typeNumber = a, this.errorCorrectLevel = b, this.modules = null, this.moduleCount = 0, this.dataCache = null,
		            this.dataList = []
	            } function i(a, b) {
		            if (void 0 == a.length) throw new Error(a.length + "/" + b); for (var c = 0; c < a.length && 0 == a[c];)c++;
		            this.num = new Array(a.length - c + b); for (var d = 0; d < a.length - c; d++)this.num[d] = a[d + c]
	            } function j(a, b) { this.totalCount = a, this.dataCount = b }
	            function k() { this.buffer = [], this.length = 0 } function m() { return "undefined" != typeof CanvasRenderingContext2D } function n() { var a = !1, b = navigator.userAgent; return /android/i.test(b) && (a = !0, aMat = b.toString().match(/android ([0-9]\.[0-9])/i), aMat && aMat[1] && (a = parseFloat(aMat[1]))), a } function r(a, b) { for (var c = 1, e = s(a), f = 0, g = l.length; g >= f; f++) { var h = 0; switch (b) { case d.L: h = l[f][0]; break; case d.M: h = l[f][1]; break; case d.Q: h = l[f][2]; break; case d.H: h = l[f][3] }if (h >= e) break; c++ } if (c > l.length) throw new Error("Too long data"); return c } function s(a) { var b = encodeURI(a).toString().replace(/\%[0-9a-fA-F]{2}/g, "a"); return b.length + (b.length != a ? 3 : 0) } a.prototype = { getLength: function () { return this.parsedData.length }, write: function (a) { for (var b = 0, c = this.parsedData.length; c > b; b++)a.put(this.parsedData[b], 8) } }, b.prototype = { addData: function (b) { var c = new a(b); this.dataList.push(c), this.dataCache = null }, isDark: function (a, b) { if (0 > a || this.moduleCount <= a || 0 > b || this.moduleCount <= b) throw new Error(a + "," + b); return this.modules[a][b] }, getModuleCount: function () { return this.moduleCount }, make: function () { this.makeImpl(!1, this.getBestMaskPattern()) }, makeImpl: function (a, c) { this.moduleCount = 4 * this.typeNumber + 17, this.modules = new Array(this.moduleCount); for (var d = 0; d < this.moduleCount; d++) { this.modules[d] = new Array(this.moduleCount); for (var e = 0; e < this.moduleCount; e++)this.modules[d][e] = null } this.setupPositionProbePattern(0, 0), this.setupPositionProbePattern(this.moduleCount - 7, 0), this.setupPositionProbePattern(0, this.moduleCount - 7), this.setupPositionAdjustPattern(), this.setupTimingPattern(), this.setupTypeInfo(a, c), this.typeNumber >= 7 && this.setupTypeNumber(a), null == this.dataCache && (this.dataCache = b.createData(this.typeNumber, this.errorCorrectLevel, this.dataList)), this.mapData(this.dataCache, c) }, setupPositionProbePattern: function (a, b) { for (var c = -1; 7 >= c; c++)if (!(-1 >= a + c || this.moduleCount <= a + c)) for (var d = -1; 7 >= d; d++)-1 >= b + d || this.moduleCount <= b + d || (this.modules[a + c][b + d] = c >= 0 && 6 >= c && (0 == d || 6 == d) || d >= 0 && 6 >= d && (0 == c || 6 == c) || c >= 2 && 4 >= c && d >= 2 && 4 >= d ? !0 : !1) }, getBestMaskPattern: function () { for (var a = 0, b = 0, c = 0; 8 > c; c++) { this.makeImpl(!0, c); var d = f.getLostPoint(this); (0 == c || a > d) && (a = d, b = c) } return b }, createMovieClip: function (a, b, c) { var d = a.createEmptyMovieClip(b, c), e = 1; this.make(); for (var f = 0; f < this.modules.length; f++)for (var g = f * e, h = 0; h < this.modules[f].length; h++) { var i = h * e, j = this.modules[f][h]; j && (d.beginFill(0, 100), d.moveTo(i, g), d.lineTo(i + e, g), d.lineTo(i + e, g + e), d.lineTo(i, g + e), d.endFill()) } return d }, setupTimingPattern: function () { for (var a = 8; a < this.moduleCount - 8; a++)null == this.modules[a][6] && (this.modules[a][6] = 0 == a % 2); for (var b = 8; b < this.moduleCount - 8; b++)null == this.modules[6][b] && (this.modules[6][b] = 0 == b % 2) }, setupPositionAdjustPattern: function () { for (var a = f.getPatternPosition(this.typeNumber), b = 0; b < a.length; b++)for (var c = 0; c < a.length; c++) { var d = a[b], e = a[c]; if (null == this.modules[d][e]) for (var g = -2; 2 >= g; g++)for (var h = -2; 2 >= h; h++)this.modules[d + g][e + h] = -2 == g || 2 == g || -2 == h || 2 == h || 0 == g && 0 == h ? !0 : !1 } }, setupTypeNumber: function (a) { for (var b = f.getBCHTypeNumber(this.typeNumber), c = 0; 18 > c; c++) { var d = !a && 1 == (1 & b >> c); this.modules[Math.floor(c / 3)][c % 3 + this.moduleCount - 8 - 3] = d } for (var c = 0; 18 > c; c++) { var d = !a && 1 == (1 & b >> c); this.modules[c % 3 + this.moduleCount - 8 - 3][Math.floor(c / 3)] = d } }, setupTypeInfo: function (a, b) { for (var c = this.errorCorrectLevel << 3 | b, d = f.getBCHTypeInfo(c), e = 0; 15 > e; e++) { var g = !a && 1 == (1 & d >> e); 6 > e ? this.modules[e][8] = g : 8 > e ? this.modules[e + 1][8] = g : this.modules[this.moduleCount - 15 + e][8] = g } for (var e = 0; 15 > e; e++) { var g = !a && 1 == (1 & d >> e); 8 > e ? this.modules[8][this.moduleCount - e - 1] = g : 9 > e ? this.modules[8][15 - e - 1 + 1] = g : this.modules[8][15 - e - 1] = g } this.modules[this.moduleCount - 8][8] = !a }, mapData: function (a, b) { for (var c = -1, d = this.moduleCount - 1, e = 7, g = 0, h = this.moduleCount - 1; h > 0; h -= 2)for (6 == h && h--; ;) { for (var i = 0; 2 > i; i++)if (null == this.modules[d][h - i]) { var j = !1; g < a.length && (j = 1 == (1 & a[g] >>> e)); var k = f.getMask(b, d, h - i); k && (j = !j), this.modules[d][h - i] = j, e--, -1 == e && (g++, e = 7) } if (d += c, 0 > d || this.moduleCount <= d) { d -= c, c = -c; break } } } }, b.PAD0 = 236, b.PAD1 = 17, b.createData = function (a, c, d) { for (var e = j.getRSBlocks(a, c), g = new k, h = 0; h < d.length; h++) { var i = d[h]; g.put(i.mode, 4), g.put(i.getLength(), f.getLengthInBits(i.mode, a)), i.write(g) } for (var l = 0, h = 0; h < e.length; h++)l += e[h].dataCount; if (g.getLengthInBits() > 8 * l) throw new Error("code length overflow. (" + g.getLengthInBits() + ">" + 8 * l + ")"); for (g.getLengthInBits() + 4 <= 8 * l && g.put(0, 4); 0 != g.getLengthInBits() % 8;)g.putBit(!1); for (; ;) { if (g.getLengthInBits() >= 8 * l) break; if (g.put(b.PAD0, 8), g.getLengthInBits() >= 8 * l) break; g.put(b.PAD1, 8) } return b.createBytes(g, e) }, b.createBytes = function (a, b) { for (var c = 0, d = 0, e = 0, g = new Array(b.length), h = new Array(b.length), j = 0; j < b.length; j++) { var k = b[j].dataCount, l = b[j].totalCount - k; d = Math.max(d, k), e = Math.max(e, l), g[j] = new Array(k); for (var m = 0; m < g[j].length; m++)g[j][m] = 255 & a.buffer[m + c]; c += k; var n = f.getErrorCorrectPolynomial(l), o = new i(g[j], n.getLength() - 1), p = o.mod(n); h[j] = new Array(n.getLength() - 1); for (var m = 0; m < h[j].length; m++) { var q = m + p.getLength() - h[j].length; h[j][m] = q >= 0 ? p.get(q) : 0 } } for (var r = 0, m = 0; m < b.length; m++)r += b[m].totalCount; for (var s = new Array(r), t = 0, m = 0; d > m; m++)for (var j = 0; j < b.length; j++)m < g[j].length && (s[t++] = g[j][m]); for (var m = 0; e > m; m++)for (var j = 0; j < b.length; j++)m < h[j].length && (s[t++] = h[j][m]); return s }; for (var c = { MODE_NUMBER: 1, MODE_ALPHA_NUM: 2, MODE_8BIT_BYTE: 4, MODE_KANJI: 8 }, d = { L: 1, M: 0, Q: 3, H: 2 }, e = { PATTERN000: 0, PATTERN001: 1, PATTERN010: 2, PATTERN011: 3, PATTERN100: 4, PATTERN101: 5, PATTERN110: 6, PATTERN111: 7 }, f = { PATTERN_POSITION_TABLE: [[], [6, 18], [6, 22], [6, 26], [6, 30], [6, 34], [6, 22, 38], [6, 24, 42], [6, 26, 46], [6, 28, 50], [6, 30, 54], [6, 32, 58], [6, 34, 62], [6, 26, 46, 66], [6, 26, 48, 70], [6, 26, 50, 74], [6, 30, 54, 78], [6, 30, 56, 82], [6, 30, 58, 86], [6, 34, 62, 90], [6, 28, 50, 72, 94], [6, 26, 50, 74, 98], [6, 30, 54, 78, 102], [6, 28, 54, 80, 106], [6, 32, 58, 84, 110], [6, 30, 58, 86, 114], [6, 34, 62, 90, 118], [6, 26, 50, 74, 98, 122], [6, 30, 54, 78, 102, 126], [6, 26, 52, 78, 104, 130], [6, 30, 56, 82, 108, 134], [6, 34, 60, 86, 112, 138], [6, 30, 58, 86, 114, 142], [6, 34, 62, 90, 118, 146], [6, 30, 54, 78, 102, 126, 150], [6, 24, 50, 76, 102, 128, 154], [6, 28, 54, 80, 106, 132, 158], [6, 32, 58, 84, 110, 136, 162], [6, 26, 54, 82, 110, 138, 166], [6, 30, 58, 86, 114, 142, 170]], G15: 1335, G18: 7973, G15_MASK: 21522, getBCHTypeInfo: function (a) { for (var b = a << 10; f.getBCHDigit(b) - f.getBCHDigit(f.G15) >= 0;)b ^= f.G15 << f.getBCHDigit(b) - f.getBCHDigit(f.G15); return (a << 10 | b) ^ f.G15_MASK }, getBCHTypeNumber: function (a) { for (var b = a << 12; f.getBCHDigit(b) - f.getBCHDigit(f.G18) >= 0;)b ^= f.G18 << f.getBCHDigit(b) - f.getBCHDigit(f.G18); return a << 12 | b }, getBCHDigit: function (a) { for (var b = 0; 0 != a;)b++, a >>>= 1; return b }, getPatternPosition: function (a) { return f.PATTERN_POSITION_TABLE[a - 1] }, getMask: function (a, b, c) { switch (a) { case e.PATTERN000: return 0 == (b + c) % 2; case e.PATTERN001: return 0 == b % 2; case e.PATTERN010: return 0 == c % 3; case e.PATTERN011: return 0 == (b + c) % 3; case e.PATTERN100: return 0 == (Math.floor(b / 2) + Math.floor(c / 3)) % 2; case e.PATTERN101: return 0 == b * c % 2 + b * c % 3; case e.PATTERN110: return 0 == (b * c % 2 + b * c % 3) % 2; case e.PATTERN111: return 0 == (b * c % 3 + (b + c) % 2) % 2; default: throw new Error("bad maskPattern:" + a) } }, getErrorCorrectPolynomial: function (a) { for (var b = new i([1], 0), c = 0; a > c; c++)b = b.multiply(new i([1, g.gexp(c)], 0)); return b }, getLengthInBits: function (a, b) { if (b >= 1 && 10 > b) switch (a) { case c.MODE_NUMBER: return 10; case c.MODE_ALPHA_NUM: return 9; case c.MODE_8BIT_BYTE: return 8; case c.MODE_KANJI: return 8; default: throw new Error("mode:" + a) } else if (27 > b) switch (a) { case c.MODE_NUMBER: return 12; case c.MODE_ALPHA_NUM: return 11; case c.MODE_8BIT_BYTE: return 16; case c.MODE_KANJI: return 10; default: throw new Error("mode:" + a) } else { if (!(41 > b)) throw new Error("type:" + b); switch (a) { case c.MODE_NUMBER: return 14; case c.MODE_ALPHA_NUM: return 13; case c.MODE_8BIT_BYTE: return 16; case c.MODE_KANJI: return 12; default: throw new Error("mode:" + a) } } }, getLostPoint: function (a) { for (var b = a.getModuleCount(), c = 0, d = 0; b > d; d++)for (var e = 0; b > e; e++) { for (var f = 0, g = a.isDark(d, e), h = -1; 1 >= h; h++)if (!(0 > d + h || d + h >= b)) for (var i = -1; 1 >= i; i++)0 > e + i || e + i >= b || (0 != h || 0 != i) && g == a.isDark(d + h, e + i) && f++; f > 5 && (c += 3 + f - 5) } for (var d = 0; b - 1 > d; d++)for (var e = 0; b - 1 > e; e++) { var j = 0; a.isDark(d, e) && j++, a.isDark(d + 1, e) && j++, a.isDark(d, e + 1) && j++, a.isDark(d + 1, e + 1) && j++, (0 == j || 4 == j) && (c += 3) } for (var d = 0; b > d; d++)for (var e = 0; b - 6 > e; e++)a.isDark(d, e) && !a.isDark(d, e + 1) && a.isDark(d, e + 2) && a.isDark(d, e + 3) && a.isDark(d, e + 4) && !a.isDark(d, e + 5) && a.isDark(d, e + 6) && (c += 40); for (var e = 0; b > e; e++)for (var d = 0; b - 6 > d; d++)a.isDark(d, e) && !a.isDark(d + 1, e) && a.isDark(d + 2, e) && a.isDark(d + 3, e) && a.isDark(d + 4, e) && !a.isDark(d + 5, e) && a.isDark(d + 6, e) && (c += 40); for (var k = 0, e = 0; b > e; e++)for (var d = 0; b > d; d++)a.isDark(d, e) && k++; var l = Math.abs(100 * k / b / b - 50) / 5; return c += 10 * l } }, g = { glog: function (a) { if (1 > a) throw new Error("glog(" + a + ")"); return g.LOG_TABLE[a] }, gexp: function (a) { for (; 0 > a;)a += 255; for (; a >= 256;)a -= 255; return g.EXP_TABLE[a] }, EXP_TABLE: new Array(256), LOG_TABLE: new Array(256) }, h = 0; 8 > h; h++)g.EXP_TABLE[h] = 1 << h; for (var h = 8; 256 > h; h++)g.EXP_TABLE[h] = g.EXP_TABLE[h - 4] ^ g.EXP_TABLE[h - 5] ^ g.EXP_TABLE[h - 6] ^ g.EXP_TABLE[h - 8]; for (var h = 0; 255 > h; h++)g.LOG_TABLE[g.EXP_TABLE[h]] = h; i.prototype = { get: function (a) { return this.num[a] }, getLength: function () { return this.num.length }, multiply: function (a) { for (var b = new Array(this.getLength() + a.getLength() - 1), c = 0; c < this.getLength(); c++)for (var d = 0; d < a.getLength(); d++)b[c + d] ^= g.gexp(g.glog(this.get(c)) + g.glog(a.get(d))); return new i(b, 0) }, mod: function (a) { if (this.getLength() - a.getLength() < 0) return this; for (var b = g.glog(this.get(0)) - g.glog(a.get(0)), c = new Array(this.getLength()), d = 0; d < this.getLength(); d++)c[d] = this.get(d); for (var d = 0; d < a.getLength(); d++)c[d] ^= g.gexp(g.glog(a.get(d)) + b); return new i(c, 0).mod(a) } }, j.RS_BLOCK_TABLE = [[1, 26, 19], [1, 26, 16], [1, 26, 13], [1, 26, 9], [1, 44, 34], [1, 44, 28], [1, 44, 22], [1, 44, 16], [1, 70, 55], [1, 70, 44], [2, 35, 17], [2, 35, 13], [1, 100, 80], [2, 50, 32], [2, 50, 24], [4, 25, 9], [1, 134, 108], [2, 67, 43], [2, 33, 15, 2, 34, 16], [2, 33, 11, 2, 34, 12], [2, 86, 68], [4, 43, 27], [4, 43, 19], [4, 43, 15], [2, 98, 78], [4, 49, 31], [2, 32, 14, 4, 33, 15], [4, 39, 13, 1, 40, 14], [2, 121, 97], [2, 60, 38, 2, 61, 39], [4, 40, 18, 2, 41, 19], [4, 40, 14, 2, 41, 15], [2, 146, 116], [3, 58, 36, 2, 59, 37], [4, 36, 16, 4, 37, 17], [4, 36, 12, 4, 37, 13], [2, 86, 68, 2, 87, 69], [4, 69, 43, 1, 70, 44], [6, 43, 19, 2, 44, 20], [6, 43, 15, 2, 44, 16], [4, 101, 81], [1, 80, 50, 4, 81, 51], [4, 50, 22, 4, 51, 23], [3, 36, 12, 8, 37, 13], [2, 116, 92, 2, 117, 93], [6, 58, 36, 2, 59, 37], [4, 46, 20, 6, 47, 21], [7, 42, 14, 4, 43, 15], [4, 133, 107], [8, 59, 37, 1, 60, 38], [8, 44, 20, 4, 45, 21], [12, 33, 11, 4, 34, 12], [3, 145, 115, 1, 146, 116], [4, 64, 40, 5, 65, 41], [11, 36, 16, 5, 37, 17], [11, 36, 12, 5, 37, 13], [5, 109, 87, 1, 110, 88], [5, 65, 41, 5, 66, 42], [5, 54, 24, 7, 55, 25], [11, 36, 12], [5, 122, 98, 1, 123, 99], [7, 73, 45, 3, 74, 46], [15, 43, 19, 2, 44, 20], [3, 45, 15, 13, 46, 16], [1, 135, 107, 5, 136, 108], [10, 74, 46, 1, 75, 47], [1, 50, 22, 15, 51, 23], [2, 42, 14, 17, 43, 15], [5, 150, 120, 1, 151, 121], [9, 69, 43, 4, 70, 44], [17, 50, 22, 1, 51, 23], [2, 42, 14, 19, 43, 15], [3, 141, 113, 4, 142, 114], [3, 70, 44, 11, 71, 45], [17, 47, 21, 4, 48, 22], [9, 39, 13, 16, 40, 14], [3, 135, 107, 5, 136, 108], [3, 67, 41, 13, 68, 42], [15, 54, 24, 5, 55, 25], [15, 43, 15, 10, 44, 16], [4, 144, 116, 4, 145, 117], [17, 68, 42], [17, 50, 22, 6, 51, 23], [19, 46, 16, 6, 47, 17], [2, 139, 111, 7, 140, 112], [17, 74, 46], [7, 54, 24, 16, 55, 25], [34, 37, 13], [4, 151, 121, 5, 152, 122], [4, 75, 47, 14, 76, 48], [11, 54, 24, 14, 55, 25], [16, 45, 15, 14, 46, 16], [6, 147, 117, 4, 148, 118], [6, 73, 45, 14, 74, 46], [11, 54, 24, 16, 55, 25], [30, 46, 16, 2, 47, 17], [8, 132, 106, 4, 133, 107], [8, 75, 47, 13, 76, 48], [7, 54, 24, 22, 55, 25], [22, 45, 15, 13, 46, 16], [10, 142, 114, 2, 143, 115], [19, 74, 46, 4, 75, 47], [28, 50, 22, 6, 51, 23], [33, 46, 16, 4, 47, 17], [8, 152, 122, 4, 153, 123], [22, 73, 45, 3, 74, 46], [8, 53, 23, 26, 54, 24], [12, 45, 15, 28, 46, 16], [3, 147, 117, 10, 148, 118], [3, 73, 45, 23, 74, 46], [4, 54, 24, 31, 55, 25], [11, 45, 15, 31, 46, 16], [7, 146, 116, 7, 147, 117], [21, 73, 45, 7, 74, 46], [1, 53, 23, 37, 54, 24], [19, 45, 15, 26, 46, 16], [5, 145, 115, 10, 146, 116], [19, 75, 47, 10, 76, 48], [15, 54, 24, 25, 55, 25], [23, 45, 15, 25, 46, 16], [13, 145, 115, 3, 146, 116], [2, 74, 46, 29, 75, 47], [42, 54, 24, 1, 55, 25], [23, 45, 15, 28, 46, 16], [17, 145, 115], [10, 74, 46, 23, 75, 47], [10, 54, 24, 35, 55, 25], [19, 45, 15, 35, 46, 16], [17, 145, 115, 1, 146, 116], [14, 74, 46, 21, 75, 47], [29, 54, 24, 19, 55, 25], [11, 45, 15, 46, 46, 16], [13, 145, 115, 6, 146, 116], [14, 74, 46, 23, 75, 47], [44, 54, 24, 7, 55, 25], [59, 46, 16, 1, 47, 17], [12, 151, 121, 7, 152, 122], [12, 75, 47, 26, 76, 48], [39, 54, 24, 14, 55, 25], [22, 45, 15, 41, 46, 16], [6, 151, 121, 14, 152, 122], [6, 75, 47, 34, 76, 48], [46, 54, 24, 10, 55, 25], [2, 45, 15, 64, 46, 16], [17, 152, 122, 4, 153, 123], [29, 74, 46, 14, 75, 47], [49, 54, 24, 10, 55, 25], [24, 45, 15, 46, 46, 16], [4, 152, 122, 18, 153, 123], [13, 74, 46, 32, 75, 47], [48, 54, 24, 14, 55, 25], [42, 45, 15, 32, 46, 16], [20, 147, 117, 4, 148, 118], [40, 75, 47, 7, 76, 48], [43, 54, 24, 22, 55, 25], [10, 45, 15, 67, 46, 16], [19, 148, 118, 6, 149, 119], [18, 75, 47, 31, 76, 48], [34, 54, 24, 34, 55, 25], [20, 45, 15, 61, 46, 16]], j.getRSBlocks = function (a, b) { var c = j.getRsBlockTable(a, b); if (void 0 == c) throw new Error("bad rs block @ typeNumber:" + a + "/errorCorrectLevel:" + b); for (var d = c.length / 3, e = [], f = 0; d > f; f++)for (var g = c[3 * f + 0], h = c[3 * f + 1], i = c[3 * f + 2], k = 0; g > k; k++)e.push(new j(h, i)); return e }, j.getRsBlockTable = function (a, b) { switch (b) { case d.L: return j.RS_BLOCK_TABLE[4 * (a - 1) + 0]; case d.M: return j.RS_BLOCK_TABLE[4 * (a - 1) + 1]; case d.Q: return j.RS_BLOCK_TABLE[4 * (a - 1) + 2]; case d.H: return j.RS_BLOCK_TABLE[4 * (a - 1) + 3]; default: return void 0 } }, k.prototype = { get: function (a) { var b = Math.floor(a / 8); return 1 == (1 & this.buffer[b] >>> 7 - a % 8) }, put: function (a, b) { for (var c = 0; b > c; c++)this.putBit(1 == (1 & a >>> b - c - 1)) }, getLengthInBits: function () { return this.length }, putBit: function (a) { var b = Math.floor(this.length / 8); this.buffer.length <= b && this.buffer.push(0), a && (this.buffer[b] |= 128 >>> this.length % 8), this.length++ } }; var l = [[17, 14, 11, 7], [32, 26, 20, 14], [53, 42, 32, 24], [78, 62, 46, 34], [106, 84, 60, 44], [134, 106, 74, 58], [154, 122, 86, 64], [192, 152, 108, 84], [230, 180, 130, 98], [271, 213, 151, 119], [321, 251, 177, 137], [367, 287, 203, 155], [425, 331, 241, 177], [458, 362, 258, 194], [520, 412, 292, 220], [586, 450, 322, 250], [644, 504, 364, 280], [718, 560, 394, 310], [792, 624, 442, 338], [858, 666, 482, 382], [929, 711, 509, 403], [1003, 779, 565, 439], [1091, 857, 611, 461], [1171, 911, 661, 511], [1273, 997, 715, 535], [1367, 1059, 751, 593], [1465, 1125, 805, 625], [1528, 1190, 868, 658], [1628, 1264, 908, 698], [1732, 1370, 982, 742], [1840, 1452, 1030, 790], [1952, 1538, 1112, 842], [2068, 1628, 1168, 898], [2188, 1722, 1228, 958], [2303, 1809, 1283, 983], [2431, 1911, 1351, 1051], [2563, 1989, 1423, 1093], [2699, 2099, 1499, 1139], [2809, 2213, 1579, 1219], [2953, 2331, 1663, 1273]], o = function () { var a = function (a, b) { this._el = a, this._htOption = b }; return a.prototype.draw = function (a) { function g(a, b) { var c = document.createElementNS("http://www.w3.org/2000/svg", a); for (var d in b) b.hasOwnProperty(d) && c.setAttribute(d, b[d]); return c } var b = this._htOption, c = this._el, d = a.getModuleCount(); Math.floor(b.width / d), Math.floor(b.height / d), this.clear(); var h = g("svg", { viewBox: "0 0 " + String(d) + " " + String(d), width: "100%", height: "100%", fill: b.colorLight }); h.setAttributeNS("http://www.w3.org/2000/xmlns/", "xmlns:xlink", "http://www.w3.org/1999/xlink"), c.appendChild(h), h.appendChild(g("rect", { fill: b.colorDark, width: "1", height: "1", id: "template" })); for (var i = 0; d > i; i++)for (var j = 0; d > j; j++)if (a.isDark(i, j)) { var k = g("use", { x: String(i), y: String(j) }); k.setAttributeNS("http://www.w3.org/1999/xlink", "href", "#template"), h.appendChild(k) } }, a.prototype.clear = function () { for (; this._el.hasChildNodes();)this._el.removeChild(this._el.lastChild) }, a }(), p = "svg" === document.documentElement.tagName.toLowerCase(), q = p ? o : m() ? function () { function a() { this._elImage.src = this._elCanvas.toDataURL("image/png"), this._elImage.style.display = "block", this._elCanvas.style.display = "none" } function d(a, b) { var c = this; if (c._fFail = b, c._fSuccess = a, null === c._bSupportDataURI) { var d = document.createElement("img"), e = function () { c._bSupportDataURI = !1, c._fFail && _fFail.call(c) }, f = function () { c._bSupportDataURI = !0, c._fSuccess && c._fSuccess.call(c) }; return d.onabort = e, d.onerror = e, d.onload = f, d.src = "data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAUAAAAFCAYAAACNbyblAAAAHElEQVQI12P4//8/w38GIAXDIBKE0DHxgljNBAAO9TXL0Y4OHwAAAABJRU5ErkJggg==", void 0 } c._bSupportDataURI === !0 && c._fSuccess ? c._fSuccess.call(c) : c._bSupportDataURI === !1 && c._fFail && c._fFail.call(c) } if (this._android && this._android <= 2.1) { var b = 1 / window.devicePixelRatio, c = CanvasRenderingContext2D.prototype.drawImage; CanvasRenderingContext2D.prototype.drawImage = function (a, d, e, f, g, h, i, j) { if ("nodeName" in a && /img/i.test(a.nodeName)) for (var l = arguments.length - 1; l >= 1; l--)arguments[l] = arguments[l] * b; else "undefined" == typeof j && (arguments[1] *= b, arguments[2] *= b, arguments[3] *= b, arguments[4] *= b); c.apply(this, arguments) } } var e = function (a, b) { this._bIsPainted = !1, this._android = n(), this._htOption = b, this._elCanvas = document.createElement("canvas"), this._elCanvas.width = b.width, this._elCanvas.height = b.height, a.appendChild(this._elCanvas), this._el = a, this._oContext = this._elCanvas.getContext("2d"), this._bIsPainted = !1, this._elImage = document.createElement("img"), this._elImage.style.display = "none", this._el.appendChild(this._elImage), this._bSupportDataURI = null }; return e.prototype.draw = function (a) { var b = this._elImage, c = this._oContext, d = this._htOption, e = a.getModuleCount(), f = d.width / e, g = d.height / e, h = Math.round(f), i = Math.round(g); b.style.display = "none", this.clear(); for (var j = 0; e > j; j++)for (var k = 0; e > k; k++) { var l = a.isDark(j, k), m = k * f, n = j * g; c.strokeStyle = l ? d.colorDark : d.colorLight, c.lineWidth = 1, c.fillStyle = l ? d.colorDark : d.colorLight, c.fillRect(m, n, f, g), c.strokeRect(Math.floor(m) + .5, Math.floor(n) + .5, h, i), c.strokeRect(Math.ceil(m) - .5, Math.ceil(n) - .5, h, i) } this._bIsPainted = !0 }, e.prototype.makeImage = function () { this._bIsPainted && d.call(this, a) }, e.prototype.isPainted = function () { return this._bIsPainted }, e.prototype.clear = function () { this._oContext.clearRect(0, 0, this._elCanvas.width, this._elCanvas.height), this._bIsPainted = !1 }, e.prototype.round = function (a) { return a ? Math.floor(1e3 * a) / 1e3 : a }, e }() : function () { var a = function (a, b) { this._el = a, this._htOption = b }; return a.prototype.draw = function (a) { for (var b = this._htOption, c = this._el, d = a.getModuleCount(), e = Math.floor(b.width / d), f = Math.floor(b.height / d), g = ['<table style="border:0;border-collapse:collapse;">'], h = 0; d > h; h++) { g.push("<tr>"); for (var i = 0; d > i; i++)g.push('<td style="border:0;border-collapse:collapse;padding:0;margin:0;width:' + e + "px;height:" + f + "px;background-color:" + (a.isDark(h, i) ? b.colorDark : b.colorLight) + ';"></td>'); g.push("</tr>") } g.push("</table>"), c.innerHTML = g.join(""); var j = c.childNodes[0], k = (b.width - j.offsetWidth) / 2, l = (b.height - j.offsetHeight) / 2; k > 0 && l > 0 && (j.style.margin = l + "px " + k + "px") }, a.prototype.clear = function () { this._el.innerHTML = "" }, a }(); QRCode = function (a, b) {
		            if (this._htOption = { width: 256, , typeNumber: 4, colorDark: "#000000", colorLight: "#ffffff", correctLevel: d.H }, "string" == typeof b && (b = { text: b }), b) for (var c in b)
			            this._htOption[c] = b[c]; "string" == typeof a && (a = document.getElementById(a)),
				            this._android = n(), this._el = a,
				            this._oQRCode = null, this._oDrawing = new q(this._el, this._htOption), this._htOption.text && this.makeCode(this._htOption.text)
	            },
		            QRCode.prototype.makeCode = function (a) {
			            this._oQRCode = new b(r(a, this._htOption.correctLevel), this._htOption.correctLevel),
			            this._oQRCode.addData(a), this._oQRCode.make(), this._el.title = a, this._oDrawing.draw(this._oQRCode), this.makeImage()
		            },
		            QRCode.prototype.makeImage = function () { "function" == typeof this._oDrawing.makeImage && (!this._android || this._android >= 3) && this._oDrawing.makeImage() },
		            QRCode.prototype.clear = function () { this._oDrawing.clear() }, QRCode.CorrectLevel = d
            }();]]></script>
 </head>
 <body>
  <table border="0" cellpadding="0" cellspacing="0" style="border-color: blue;" width="800"> 
   <tbody> 
    <tr height="3px;"></tr> 
    <tr valign="top"> 
     <td style="width: 205px;"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><strong>Merkez</strong></span></span></span> 
      <table align="left" border="0" cellpadding="0" cellspacing="0" style="font-size: 9px; border-top-color: #000000; border-bottom-color: #000000; border-top-width: 3px; border-bottom-width: 3px; border-top-style: solid; border-bottom-style: solid;" width="350"> 
       <tbody> 
        <tr align="left"> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:Invoice"> 
              <xsl:for-each select="cac:AccountingSupplierParty"> 
               <xsl:for-each select="cac:Party">  
                <xsl:if test="cac:PartyName"> 
                 <xsl:value-of select="cac:PartyName/cbc:Name" /> 
                 <br /> 
                </xsl:if> 
                <xsl:for-each select="cac:Person"> 
                 <xsl:for-each select="cbc:Title"> 
                  <xsl:apply-templates /> 
                  <span> 
                   <xsl:text>&#160;
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:FirstName"> 
                  <xsl:apply-templates /> 
                  <span> 
                   <xsl:text>&#160;
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:MiddleName"> 
                  <xsl:apply-templates /> 
                  <span> 
                   <xsl:text>&#160;
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:FamilyName"> 
                  <xsl:apply-templates /> 
                  <span> 
                   <xsl:text>&#160;
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:NameSuffix"> 
                  <xsl:apply-templates /> 
                 </xsl:for-each> 
                </xsl:for-each>  
               </xsl:for-each> 
              </xsl:for-each> 
             </xsl:for-each> </span></span></span></td> 
        </tr> 
        <tr align="left"> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:Invoice"> 
              <xsl:for-each select="cac:AccountingSupplierParty"> 
               <xsl:for-each select="cac:Party"> 
                <xsl:for-each select="cac:PostalAddress"> 
                 <xsl:for-each select="cbc:StreetName"> 
                  <xsl:apply-templates /> 
                  <span> 
                   <xsl:text>&#160;
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:BuildingName"> 
                  <xsl:apply-templates /> 
                 </xsl:for-each> 
                 <xsl:if test="cbc:BuildingNumber"> 
                  <span> 
                   <xsl:text>
                     No:
                   </xsl:text> </span> 
                  <xsl:for-each select="cbc:BuildingNumber"> 
                   <xsl:apply-templates /> 
                  </xsl:for-each> 
                  <span> 
                   <xsl:text>&#160;
                   </xsl:text> </span> 
                 </xsl:if> 
                 <br /> 
                 <xsl:for-each select="cbc:PostalZone"> 
                  <xsl:apply-templates /> 
                  <span> 
                   <xsl:text>&#160;
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:CitySubdivisionName"> 
                  <xsl:apply-templates /> 
                 </xsl:for-each> 
                 <span> 
                  <xsl:text>
                   / 
                  </xsl:text> </span> 
                 <xsl:for-each select="cbc:CityName"> 
                  <xsl:apply-templates /> 
                  <span> 
                   <xsl:text>&#160;
                   </xsl:text> </span> 
                 </xsl:for-each> 
                </xsl:for-each> 
               </xsl:for-each> 
              </xsl:for-each> 
             </xsl:for-each> </span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="font-family: verdana,geneva,sans-serif;"><span style="color: #696969;"><span style="font-size: 9px;"><strong>&#350;ube No</strong>: MERS&#304;N &#350;B. KARACALYAS EVREN MAH. 100.YIL CAD. A BLOK NO.2 &#304;&#199; KAPI NO:10 AKDEN&#304;Z/MERS&#304;N</span></span></span></td> 
        </tr> 
        <tr align="left"> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Tel:&#160;
             <xsl:value-of select="n1:Invoice/cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:Telephone" /> Faks: 
             <xsl:value-of select="n1:Invoice/cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:Telefax" /></span></span></span></td> 
        </tr> 
        <xsl:if test="//n1:Invoice/cac:AccountingSupplierParty/cac:Party/cbc:WebsiteURI != ''">
         <tr> 
          <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Web Sitesi:&#160;
              <xsl:value-of select="n1:Invoice/cac:AccountingSupplierParty/cac:Party/cbc:WebsiteURI" /></span></span></span></td> 
         </tr>
        </xsl:if> 
        <tr align="left"> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">E-posta: 
             <xsl:value-of select="n1:Invoice/cac:AccountingSupplierParty/cac:Party/cac:Contact/cbc:ElectronicMail"></xsl:value-of></span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Vergi Dairesi: 
             <xsl:value-of select="n1:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name" />&#160;</span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
             <xsl:for-each select="//n1:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification"> 
              <xsl:variable name="schemeID" select="cbc:ID/@schemeID"></xsl:variable> 
              <xsl:if test="$schemeID ='TCKN' or $schemeID ='VKN'"> 
               <xsl:value-of select="$schemeID"></xsl:value-of> : 
               <xsl:value-of select="cbc:ID"></xsl:value-of> 
              </xsl:if> 
             </xsl:for-each> </span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Mersis No: 
             <xsl:value-of select="n1:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'MERSISNO']"></xsl:value-of></span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Ticaret Sicil No: 
             <xsl:value-of select="n1:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'TICARETSICILNO']" /></span></span></span></td> 
        </tr> 
       </tbody> 
      </table> </td> 
     <td style="width: 197px; text-align: center;"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><img align="middle" alt="GIB Logo" src="data:image/jpeg;base64,/9j/4QAYRXhpZgAASUkqAAgAAAAAAAAAAAAAAP/sABFEdWNreQABAAQAAABkAAD/4QMZaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjYtYzEzMiA3OS4xNTkyODQsIDIwMTYvMDQvMTktMTM6MTM6NDAgICAgICAgICI+IDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+IDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdFJlZj0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlUmVmIyIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOjZDNDJBNEI2QjVCRDExRThCQjM0REIwQkZGMEQxODY0IiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjZDNDJBNEI1QjVCRDExRThCQjM0REIwQkZGMEQxODY0IiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDUzQgV2luZG93cyI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSIzREVENkU1N0FDREVDNEJBNzkxNUM2M0NCN0RENzM0NyIgc3RSZWY6ZG9jdW1lbnRJRD0iM0RFRDZFNTdBQ0RFQzRCQTc5MTVDNjNDQjdERDczNDciLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7/7gAOQWRvYmUAZMAAAAAB/9sAhAABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB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" style="width: 70px;" /><br /><br /><span align="center">e-Ar&#351;iv Fatura</span></span></span></span></td> 
     <td align="right" style="width: 242px;"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><img align="middle" alt="GIB Logo" src="data:image/jpg;base64,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T4TJP3cveqP7K/V9D9D7fUbW7DmC4imCNtYo4bB9DjvVnIPIORX5X6fqN3pF5Hd2N1NZXcWSk9vIY5E4wdrDkHBNfV37L/7QKXlvJ4Y8W6wz3/m7tPvL+UsZlOSY2cjqCMgs2Tv2j7oFelmXDVbA03Vpy50t9NfXqeLk3GuHzKusPXh7NvZ30b7dD6k4NHemowYZByPancV8afpSaauLRRRQMKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACkPSlpDQA3v70tIeATXNeNPiDoPgCxe61rUrey/cySxQyTKss+wZZY0JBduQMDuw9auEJTkoQV2zGrVp0YOpUdoo6RiMHPAr53+PP7UEXgud9D8KSW19rKsUubpv3kVqRkFQARukB7E4GMEE8V4x8Uv2pfE3xCtvsOnIfDOlPGY57e2nMkk5JzzLsUhcAfKuMgtksDgeL1+i5Twy01Wx23SP8An/kfjWf8bqcXhssfrP8Ay/zNfxP4u1nxnqTX+t6lcaldHo875CD0VRgKPYACsiipbW1mvrmK2toZLi4mYRxwwqWd2JwFUAEkk9q/RIxp0YWjZRXyR+QznUxFRym3JvruyKiun/4Vd4z5P/CI66R7aZN+ny1la34Y1jw28S6vpN9pbS5Ma3tu8W/HXbuAzjI/OsY4nD1Hyxmm/VGssHiaS5p05K3WzPQfhn+0V4s+H+owCbUp9X0dpAbizvG807SRu8tidynGcDO3JyQc194eGfE+meMNGttV0i7S+sLgExzJkA4JB4PIIIIP0r8vK6Twb8RvEfgC8+0aDq01g3IMYw0bA4JyjArk4HOM8V8xm/D9PH/vMPaE/wAH6/5n3HD3F1bK37HFXnTfnqvT/I/TWjAzXz/8Gf2qdJ8Ypa6V4lePSNcIEazscW9y2OoY8Ix5+U8dACScV79Gc496/K8Vg6+CqOnXjZn7zgMxwuZ0VXws+ZfivJokooorkPTCiiigAooooAKKKKACiiigAooooAKKKKACiikb7poAWisLxX438O+ANNXUfE+v6X4c05pBELzVryO1hMhyQgeRlBJAOB14Ncta/tIfCW/uYba1+KPgu5uZnEcUMXiC0Z5GJwFUCTJJJwAKAPRqKYpDYIOQemO9PoAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAbyO1Jnmoru5is7aSeeVIYY1LvJIQqqo5JJPQCvnf4i/tj6P4d1GXT/DVgPEDIHSS+aYxQI44Gz5SZBnOSNoxggtnI7cJgcRjZ8mHg2/w+b2PJzDNMHldP2mLqKK/F+i3O2+P/AMah8IfDtsbSKK61u/dltIZlYxqFwXd9uOBuUYyCSwx0OPiPx/8AEPWviVr0mq61OHmI2xwx5EUK/wB1FycD8eTzWRrevah4l1OfUdUupLy9nZpHlkPcsWIA6KMkkAYAyeKo1+v5TktLLYJySlU6v9F5H87Z/wASYjOarUW40ukf1fmFH6UV9Afsq/Biw8e3t94g1uBLzS7CQQRWci5SaYgMS3PIUFflIwd/PTB9XH42ngKEsRU2XTzPDyzLq2aYqOEo7vr5dRvwM/Zdn+IOnnWvEkl1pWjzR5s0tmVZ5TuxvIZWwnBxxk5BHHX6v8F/DDw14Dt4I9I0i0t544Vha7EKefIAAMs4AJJxk++eK6lEAwBwB0HpTwOa/FswzXE4+bc5Wj0S2R/SmU8P4LKacVTgpTW8mtX/AJCFVA+6PyrK8ReFtJ8WabJYavp9vqNnJ1injDAHsRnofcc1r4zSSYCHPTFeTGUoNOLsz6KdOFSLjNJo+Hv2hP2crT4VaLa63ot7dXenPKLeeK8Ks8bNnawZQo28Yxjqffjwfv8ApX1l+1t8YdG1Dw4nhHSbqLUbq5eO4uJ7eRXjhRWyFJBPzEgHHYcnqM/Jvcmv2zIauJq4KM8Ve93a+9ulz+Y+KqGCw+ZSp4H4bK9tr9UgPQ19B+EP2yvFGmT2UGu2VlqVkhVZ54ojHcldw3N94IW25wMKM46V8+Uf56Zr0sZl+Gx0eWvBO23dHj5fmuMyybnhajjffs/U/UvSNXtdc060v7KZLizuolmilU8MrAEEfUGrw61+c3wl+M+tfCrXLe4glub/AEhBJ52jm5KQy7l4IyGCkMFO4DJxjODX2R8Iv2gPD/xakWytEmstaSAzzWMyFgqgqGZZANrLucAdCeu0V+RZnkWIy9uSXNDuv1/qx/QWR8U4PNoxpyfJV/lfX0f9M9Tooor5w+3CiiigAooooAK/PP8A4LFf8k++Gv8A2F7n/wBEV+hlfnn/AMFiv+SffDX/ALC9z/6IoGj8uKKQnAJr9YfDX/BJ34R6z4f0q/m8ReNEmurSKd1jvbQKrMgJAzakgZPGSaBn5PjrX9Gfw9/5ELw3/wBgy2/9FLXx+f8AgkV8HgOfEnjfH/X9Z/8AyJX2lo2mRaJpVjp0DO8FnAlujSEFmVFCgkgAZwB0AoE2XW4Br5i/bG/bW0b9mjwxLaaO9hrvj+aYQQaPNIxW2Bj8zz5wuDsAaPC7lLeYMHAJH09X4gf8FHQB+2d8Q/f+zs/+C62oEeI/Ez4ia18WfH2u+L/ENx9o1fV7p7mbaXKR5Pyxx72ZgiKFRVJOFVRk4Fcz19D7GvbP2PP2fP8AhpX426d4WuLhrTRbaJtS1aaNcv8AZY2RWjTkYZ2dEBz8octhtuD+iXxy/wCCZ/wk1D4Z67N4G8O33h/xTZ2k11YNZajcXH2qZI2KQOk7yAqzAD5QG9DjIIUfFv7LX/BQDx38C73RdA1u/bxH4Ailigls75TJcWFuGAY2zghhtUfLG2UwMALnNfsX4K8ZaN8QfDGm+I/D1/Fqei6jCJ7W7hPyyIe/qDkEEHkEEGvwAPwI+JSsVb4e+KlcZBH9i3Jwf++MGv1Z/wCCZut+JYPgs3grxB4U1Tw2vhnm3m1K1eD7V9pubqZ9odQTtOM9fvDpmgTPsKvzV/4LKf8ANIP+4v8A+2VfpVX5q/8ABZT/AJpB/wBxf/2yoBH5rUUqAFgD0J5r9cR/wSM+D+QP+Ek8cc/9P9n/APItAz8iLv8A49Jv9xv5V/SH4V/5FnSP+vOH/wBAFfGU3/BIf4OyROh8SeOMMCDi+s88/wDbpX2xp1mmnWVvaxlmjhjWJSxySFAAz+VAmy1RRRQIKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigD5K/bL+IGuWWp2HhW3Z7XR7i2W8kmhZka4bc6GMkHBQDaSMdSp7CvlknJz/n/PSv0t+Ivw20T4naG+l61btKmS8U0b7JIHwQHU+oz0IIPcGvjD4gfsveNfBLTz21n/AMJDpqEbbjT1zIRnAzDy2ef4dwA5JwDX6jw7muDhh1hpWhJd/tfPufg/GORZjPFyxkb1Kb7fZXb0PIaKP196K++vdXPyd+lmgPSvvr9lrwsnhj4RabKG3SamzahJn/bAAH/fKr+tfAv+ea++/wBlrxZF4o+EWmwqCJtLJsJc9yoBB/75Zf1r4rizn+pw5dubX8T9N4B9l/aU+b4uV2+9XPXNwA6j86C4x94fnXy78cfhP8QNBuZNX8G+JfEWoaZI/wA2kw6hcvNaqEySrNITICVbjqCVADc4+eV+IfjoX4spPFeuwXQk8lkuNTliCPnGGLOAmD13YA74r4/B5D9ep+0o1o+a1uvU/Qsy4s/sus6OIw0l2eln6H6La5c6nb2RfSbS0vrrOBFd3TW6Y7ncsbnP/Afxrxn4jeF/jR45EltY6toXhrTD1h0+8n85+VPzTeSDwVP3AuQxB3CvFLL4ffHnU7KG7tL7W7q1mRZIpovESMkinkMpE+CCCDU4+GH7QI6T6+Ppr6f/AB+umhllPDSUo4mk2u7v/wAA87F53Wx8HCWErKL/AJU1+S/UP+GMPHOf+QhoWf8Ar5m/+M0v/DF3jrGf7Q0P/wACJv8A41Wbrvhn46eDNNfWNQu/EUFpa/vXkXV/tAQDkllWVvlGOSRgDrXbar+27eSaHFHpvhuKDVmQb5bi4Lwo2TnCgAnjHUjBJHOMn33WzmaTwtWFRPdq2nqfLRw/D1NyWOo1KMktFK936HNn9i7xyB/yEdC/8CZv/jVJ/wAMYeOf+gjoX/gTN/8AGq8+1746ePfEd0bi58UahAx6R2MxtkUemI9ufxyfesv/AIWj4z/6G7Xf/BnP/wDFV6UcPnbV5Vo39P8AgHiSxXDak1DDza/xHqh/Yw8c/wDQR0L/AMCZv/jVepfs8fs9eIvhX42u9Y1i702e3lspLVUs5JHfcZI2BO5FAGEPfrivm3w34t+Ivi7W7bSNI8Sa/e6jclhFAuqyKWwpY8s4A4BOSe1ezaRqfiv4AWn/AAknjjxDda7fXEBt7bw2dUklZZHwwkkJYqAAki5AYZIwTXkZjHMpU3hqtaMpSt7qWr/rufQZPUyeNaONo4ecYw+037qt+b8tz63LYz60odfUfnXwvZP8bfjDLc+JdHl1dbO4k2qtlqJs7YbQFxEjSjIGMErnndkk5q2Phh+0D/z31/8A8KBP/j9fNvJIw0qYmCfVX2PtlxVVqe9RwVSUXs0t0fbnmrk8j86C467gK+Ix8MP2gTx5+vj/ALmBP/j9cP4u1z4l+BdUGna54l1qyvigcwrrTSlQfXZIwB9jz36VtR4eVefJRxEJPydznxHGEsJDnr4OpFd2rL8T9FNw9Rj60qnPTBr4Q+HHh34v/Eye0ksde8Q22kzMN+qXGozLCi5IJXL5kwQRhc84zgc19e/DL4fT+AdMkhu/EOq+I72faZrnU7l5ACAeI0YnYOT3JPGScCvJx+XwwD5XVUpdke/lOc1c19+OHlCH8z6+nc7Wvzz/AOCxX/JPvhr/ANhe5/8ARFfoZX55/wDBYr/kn3w1/wCwvc/+iK8Y+qR+XH+ea9htf2wfjVY2sNtb/EzxDDBCixxxrdkBVAwAB6dMfSvHicDPpzX6M6F/wSA/tvRdO1D/AIWz5P2u2juPL/4Rvds3KG25+1jPXrjnFAzh/wBhL9pn4qfEH9qvwR4f8SePNa1rRbz7d9osbu5LxS7LG4dcjvhlU/UCv1yr4f8A2b/+CaP/AAz98Z/D3j7/AIWP/b39kfaP+Jf/AGH9m83zbaWH/WfaH2483d905xjjOR9wUCEPAr8KP28NTfVf2uPiTM5Yst/HAN/XEcEcY/DCDHTgD8P3XPAr8M/+CgemDSf2wviNApUh7m2uDgY5ktIJMf8Aj5/I0Aj67/4JJfD+y0zRfE/itMNe6lbQ27Z5Kqs84wOOh2KTz1Ar6X/as/az0j9lPTfD17qug3uurrM00MaWcyR+WYwpJO7rncOnpXxd/wAEmfjNDY+Ota+H2p3MVudQsBNpSyPgzyxPJJJGB3bZI74H8KMa9u/4Kg/Bbxj8W/A/ha98K6VHqFp4ea+v9Unlvre1S1g8pD5jGZ0yAEYnGcYoDqcv/wAPhfB//RPtb/8AAyH/AAr3f9lL9tHRf2q9W8Q2OleHL/Qm0aGGaR7yZHEgkLABdo4xtPX1r8Oyu3I4yDjjpxX6I/8ABHX/AJG/4mf9eNl/6MloBrQ/UKvzV/4LKf8ANIP+4v8A+2VfpVX5q/8ABZT/AJpB/wBxf/2yoBH5rAkEEdc9q94/4bs+PI/5qVqmP9yHH/ouvCEXc6rnGSBmv0m/4c2/9Ve/8tnp/wCTdBQn/BNj9pX4m/GH4+6zofjLxfe6/pUPhm5vY7W5WMKsy3Voiv8AKinO2SQenzV+ltfIP7In7AH/AAyx8TtQ8X/8J3/wk/2vR5tJ+x/2P9k275oJfM3+fJnHkY27R97rxg/X1BIUUUUCCiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKbIoZCCMinUhoE9UfJX7Sf7OKWcMXiDwXo08rPO/9oWVrvlYl2yskcfJwGJBVeACCAACa+Xrm1msrma3uIXt7iF2jkhkUq0bA4KkHkEHt9a/VJuhFeGfG/wDZksfiRcDVtFmt9D1pUkM2ICUvGPKbypG05zlwrHB6HAr77JeI3RUcPi37q+1vb1PyLiXg36w5YvLlaT3jok/T87Hw5XpnwK+M9z8HvEU8rwNe6TfBUu7ZCAw2k7ZF/wBpctx0IPJ4BHCa94c1Twtfmy1fT7jTbvaHENzGUYqScEA9RwRkehHbFZx6V+i1qNDH0HCdpRl/Wh+P4bEYjK8Sq1O8akf61P038H+OtB8facL7QtTg1GDo4iJDIT0DqQGU8dGAry/41fs16H4y0TU9S0LT1svFA33MbQNtW6kJLFXBIXLkn5uCCQScAg/HXgj4g698OtUfUNAv2sbiRBHJ8iusi7g20qwI6jsM9hwTXvum/txX8VnGl94ThurkKA8sN+0SscckKY2xz7nGQMmvzupkOYZfiPa5e7r1s/R7Jn6/R4qynOMJ7DN48svS69U1dpnH/Cn4t+JvgR4mh8P+Jkvrfw+hk8/TZ4AzxE5xJETg43r2JQgsQMnNfTtr+0X8P7jw1/bbeIYLe3ztaCUMLgNwCoiA3tjcuSoI75xzXzD8W/2jbD4r+G5tPn8FW9lf/J9n1JrsSyW4DqzBf3QOGAIIBA5rxGvYeRxzRKviYeyn1Ss0/M+chxRUyNvDYGqq1K2jkmnHy17f1Y9j+NP7R+r/ABOlNjprXOi+HzEFksxIN87c5LsozjBxtyQcZPPTxzt6e1FGcc4z7V9bhcJQwVJU6K5Uv61Z8FjsfiMyrutiJuUn/VkgP1x71u+G/A3iHxc8f9j6Ne38bTCEzQws0SOcfK0mNq4DAkseAQTXpvwb/Zj1j4lWaatqMx0jQnBETlSZp+PlZARgL7n04B61QPwU+LXgS6mXTdP1S280lDPpF3gTAHgnYwIB64bB9vXz62aYdynRo1YxnHvt/wAE9WhkeMUIYmvQk6cuyu7fjZPzOt1/x14C+D/hV9N+Hs633jMJHFL4iihEi9VMpVnDKQwTAVcqN+Qcg1gfBn4L6r8VvHBvPFlrrEGlzQNfvfSwOgvWLLhRKwxhg27jOQDjHUer/BT9k9vDOraZ4i8UXUNxdRIZBowhV445GXA3uchiuSeBjcAQTjn6VVQijAAAr4PGZvSwalSwbc5S3m9/Rabfh2P1XLuHa+YShiMxiqcI/DTS0t3eu769e5T0XRLHw9p0Gn6ZaQ2NjAu2OC3QIiDqcAcckk1Yv763020nurueO2toUMss0zhURQMlmJ4AA6mua8bfE7w18PrWWTW9XtrSZYjKlq0q+dKo/uJnLc8cV8PeOfiz4u+Lfim6sLXUb+Sw1G6MNlpKMIlKFyI43VTtLcjJJPPfAGPFy/Ka+Yyc5PlitW3/AFqfS5xxBhcmjGjTjzzeijHp69key/Gn9rVITLpPgW5cXUM5WXVjHHJBIm0giLdnd8x+9jHycbgwNcF8G/2fNe+J2vQ+IPE0E0GgS3BuLmS6Z4573cN+U4yVZiMvkZBO055HsfwV/Zc0fwxY2mseJLcarq1zaL5un38EbwWzsQxAUhvnXAXcD/exwRj39MLgAcV6VbNKGX05YXLY6vRz6v0PEw2RYvOKscbnUtFqqa2Xr/XqVdI0m20LTLTTrGIQWVrEsEMakkIijCjJOeAB1q8OtFIPvV8g227s/SIxUUoxVkh9fnn/AMFiv+SffDX/ALC9z/6Ir9CzXy5+3h+yv4n/AGp/DHhPTPDWp6Tpk2j30t1M+rSSorq0WwBfLjfJz64pFn4oYzxX0bp//BQ/9oLS7K2tLXx/5dvbxrFGn9jae2FUAKMm39O+SeOa9b/4dDfFX/oa/B3/AIEXX/yPR/w6G+Kv/Q1+Dv8AwIuv/kegq55Yf+Cj37RJH/JQs+39iad/8j1+mH7CvxZ8V/GP4Ladr3jDVv7Y1WWMl7n7PFBuPnzr92JVX7qKOnb3NfER/wCCQ/xVAP8AxVfg/wD8CLr/AOR6+9/2QPgTrf7PvwqsfC+u3thf3sCbWl05naI/vZn4Lqp6SDqOxoE7HubfdNfjZ/wUw+FXi3Rv2jPFPjq80K6i8I6xNZW9lq+0GCWVLCBWTIJ2nMb4DYJ2NjIBr9kz0rC8aeCtC+Ifhu+0DxJpVtrWj3sZintLpNysCMZHdWHUMMEEAgggGgR/O/4U8Uap4I8TaV4h0S7aw1jS7qO8s7pVVzFKjBkbawKtggfKwIPQggkV7X4w/b2+Onjvwzqfh/WPHHn6PqlrJZ3lvFpdlF50MgIdNywhhlTt4POffn2b41f8EqfiNoGv6zffDyXS/E/h57wtp+mNeeRqEUDHIV/O2xER5KlvNywUMFBYqvGeBf8AgmJ8cfEnieysdf0K28I6RI4+06tdaja3Qhj3AMVihlZnfaSQp2qcAFl60D0Pkv27Y4xX6Jf8Edf+Rw+Jn/XjY/8Aoyatv4sf8EkZbuz8Nj4eeJrG3urexS31g6150aXVwoG65j2eYU3nJ8s5C8YY17D+wd+xp4u/ZZ13xbe+JdX0XU4tYtreGFdKkmdkaNnJLeZGnGGGMZ70Bc+xq/NX/gsp/wA0g/7i/wD7ZV+lLdDXyJ+33+yL4w/ap/4QT/hE9S0PT/7B+3/af7Znmi3+d9n2bPLikzjyWznHUYz2BH40glTkdRX04P8AgpP+0GD/AMjrD/4JrH/4zXoX/Dor4w/9DJ4H/wDA68/+RaP+HRXxh/6GTwP/AOB15/8AItBR56f+ClP7QhB/4raD/wAE1l/8Zr9cv2efFmp+O/gX4B8Ra1cC81fVdEtLy7uBGqCSV4lZm2qAoySeBxX5n/8ADor4w9/EngfH/X9ef/Itfp18DPA1/wDDP4O+CvCeqS20+paJpFtYXEtm7NC8kcaqxQsqkrkcZUH2FAnY7qiiigQUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAU1uFNOpD0oA53xh4E0Lx3pr2OuadDf27KVG8YdOQTtYYZSSB0PYV8U/HP9nnU/hfeSahpsdxqXhpzuFyEBa2JJwkgBJIAA+fAGTjqRn73psiK6MGUMpGCp717eW5tiMtneDvHqun/AAGfK53w9hM6pWmuWfSS3+fdH5T/AJEUtfZPx3/ZestesY9T8E6Xa6dq0RJms4WEEM8YQ8KgG0PkKB90fM2496+PLyzm068ns7mJoLm3kaGWJxho3U4ZT7gjp2wa/XctzShmUOem7Pqnuj+es5yPFZLW9nW1j0l0ZDRRXq3wL+A138Y7m8nkvTpekWTLHLOIi7SORnZHnAyAASSeNy8HNd2KxVLB0pVqzskeZgcFXzCusPho80n/AFc8wsrC51O5W3tLea6nYHEUKF2PGTgAE9PSvZ/2cviF4F8G67AfEeix21+quqa88skoUllKqYcEIevzgZHToTX194P+F/hbwEpGh6Ja2MhUo06pumZSQSpkbLEZwcE44FeZfFX9lDw94vt5brw5Hb+HNWyGAhjItpAqkBPLUgJk7csoOMHgknPwNXiHCZhzYetGUIPZp6/NLp95+rUeD8wyjlxmGlGpUjq4taW7Jvr9x7hp97bajaQXNpPHc20yCSOWFgyMpGQVI4II5zVnv/Svz407xh4+/Z78YRafdy3cS2JcjS7maQ2U8bFvmRcgbSckMO4PoRX2H4f+PfgPxBpkd3H4l0+138GC+uFt5VPoUcg/iMg9ia+Vx+T1sJyzp+/CWzWv3n3eU8R4fH81KuvZVI7xlp919z0Pt/WvGf2hvjivw38ONBod9YT+I5Z1gMDzK0lsjKzGUx5yfugDPGWB56HyT4w/tVXni6K88NeDrW5tUkuFjj1WCdlnmVWz+6VMEbiFxzkrkFeeLPwA/Z11a88S/wDCSeO9HjnsZoZJI7bUmMkzzsynfLGc54Mh+fnPUdDXdh8qjgqaxeYO1tVDS8jy8Zn9XM6ry/J1zX0dTW0TgfAHwF8Z/Gx4vEd5eKmnXk2JdTvpzJNMqkqxRQSTjaVwxA4AHAyPtzwb4TsvBfhvTdFskXyLOFYg5RVaQgYLtgAbmPJPck1p2Fjb6baxW1pBHbW8ShUiiUKiAdgBwB9KsgZIrzcxzWtmDSdowWyWyPcybIMPlEXNXlUl8Un1/wAh1JS0V4p9SFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFIelLTX5WgBvUH0rh/iD8G/CvxMhA1rTt1zEjpDeQSGKWLcOuQfmweQGBGe3Wu5HNGeDWlOrOjJTpyafkc1fD0cVB068FKL6NXPCNF/Y38CaXeedcvqmrRhcC3u7kKmcjBzEqNxjpnHJyK9h8L+FtK8G6VBpejWUWn2MQO2GIHHuSTyT7nk1qhiaeBXRXxuJxPu1qjkvN6fccmEyzBYF3w9KMW+qWv3i01/u06kIzXGeoc54z8B6H8QNJOm69p8d/algwDEqykEHKspDL05weRkHgmvn7V/2HLGa7DaV4rubW22jKXtotxIW9dytGMe238a+ozxR0NelhcyxeDTjQqNLtuvudzw8dkuX5lLmxVJNrrs/vVmeVfDr9m/wf8ADjULLUrS3uLvWLTeY7+5uGLZZSp+RSE6MwGVyM9a9VFIaUda5K+Iq4mXPWk5PzO/C4PD4Gn7LDwUY+Q6iiisDtCiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooA//Z" height="150" style="width: 250px;" /></span></span></span> 
      <div id="qrcode">&#160;
      </div> 
      <div id="qrcode"></div>
      <div id="qrvalue" style="visibility: hidden; height: 30px; width: 30px; display: none;">
       {"vkntckn": "<xsl:value-of select="n1:Invoice/cac:AccountingSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'TCKN' or @schemeID = 'VKN']"/>","avkntckn": "<xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'TCKN' or @schemeID = 'VKN']"/>","senaryo": "<xsl:value-of select="n1:Invoice/cbc:ProfileID"/>","tip": "<xsl:value-of select="n1:Invoice/cbc:InvoiceTypeCode"/>","tarih": "<xsl:value-of select="n1:Invoice/cbc:IssueDate"/>","no": "<xsl:value-of select="n1:Invoice/cbc:ID"/>","ettn": "<xsl:value-of select="n1:Invoice/cbc:UUID"/>","parabirimi": "<xsl:value-of select="n1:Invoice/cbc:DocumentCurrencyCode"/>","malhizmettoplam": "<xsl:value-of select="n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount"/>",<xsl:for-each select="n1:Invoice/cac:TaxTotal/cac:TaxSubtotal[cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode = '0015']"><xsl:text>"kdvmatrah</xsl:text>(<xsl:value-of select="cbc:Percent"/>)":"<xsl:value-of select="cbc:TaxableAmount"/>",</xsl:for-each><xsl:for-each select="n1:Invoice/cac:TaxTotal/cac:TaxSubtotal[cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode = '0015']"><xsl:text>"hesaplanankdv</xsl:text>(<xsl:value-of select="cbc:Percent"/>)":"<xsl:value-of select="cbc:TaxAmount"/>",</xsl:for-each>"vergidahil": "<xsl:value-of select="n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount"/>","odenecek": "<xsl:value-of select="n1:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount"/>"}
      </div><script type="text/javascript">var qrcode = new QRCode(document.getElementById("qrcode"), { width: 140, height: 140, correctLevel: QRCode.CorrectLevel.L }); qrcode.makeCode(document.getElementById("qrvalue").innerHTML)</script></td> 
    </tr> 
    <tr> 
     <td style="width: 205px;"> 
      <table align="left" border="0" cellpadding="0" cellspacing="0" id="customerPartyTable" style="border-top-color: #000000; border-bottom-color: #000000; border-top-width: 3px; border-bottom-width: 3px; border-top-style: solid; border-bottom-style: solid;" width="350"> 
       <tbody> 
        <tr> 
         <td align="left" style="width: 469px;"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><strong>SAYIN</strong></span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left" style="width: 469px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:for-each select="//n1:Invoice"> 
                <xsl:for-each select="cac:AccountingCustomerParty"> 
                 <xsl:for-each select="cac:Party">  
                  <xsl:if test="cac:PartyName"> 
                   <xsl:value-of select="cac:PartyName/cbc:Name"></xsl:value-of> 
                   <br /> 
                  </xsl:if> 
                  <xsl:for-each select="cac:Person"> 
                   <xsl:for-each select="cbc:Title"> 
                    <xsl:apply-templates></xsl:apply-templates> 
                    <span> 
                     <xsl:text>&#160;
                     </xsl:text> </span> 
                   </xsl:for-each> 
                   <xsl:for-each select="cbc:FirstName"> 
                    <xsl:apply-templates></xsl:apply-templates> 
                    <span> 
                     <xsl:text>&#160;
                     </xsl:text> </span> 
                   </xsl:for-each> 
                   <xsl:for-each select="cbc:MiddleName"> 
                    <xsl:apply-templates></xsl:apply-templates> 
                    <span> 
                     <xsl:text>&#160;
                     </xsl:text> </span> 
                   </xsl:for-each> 
                   <xsl:for-each select="cbc:FamilyName"> 
                    <xsl:apply-templates></xsl:apply-templates> 
                    <span> 
                     <xsl:text>&#160;
                     </xsl:text> </span> 
                   </xsl:for-each> 
                   <xsl:for-each select="cbc:NameSuffix"> 
                    <xsl:apply-templates></xsl:apply-templates> 
                   </xsl:for-each> 
                  </xsl:for-each>  
                 </xsl:for-each> 
                </xsl:for-each> 
               </xsl:for-each> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:for-each select="//n1:Invoice"> 
                <xsl:for-each select="cac:BuyerCustomerParty"> 
                 <xsl:for-each select="cac:Party">  
                  <xsl:if test="cac:PartyName"> 
                   <xsl:value-of select="cac:PartyName/cbc:Name"></xsl:value-of> 
                   <br /> 
                  </xsl:if> 
                  <xsl:for-each select="cac:Person"> 
                   <xsl:for-each select="cbc:Title"> 
                    <xsl:apply-templates></xsl:apply-templates> 
                    <span> 
                     <xsl:text>&#160;
                     </xsl:text> </span> 
                   </xsl:for-each> 
                   <xsl:for-each select="cbc:FirstName"> 
                    <xsl:apply-templates></xsl:apply-templates> 
                    <span> 
                     <xsl:text>&#160;
                     </xsl:text> </span> 
                   </xsl:for-each> 
                   <xsl:for-each select="cbc:MiddleName"> 
                    <xsl:apply-templates></xsl:apply-templates> 
                    <span> 
                     <xsl:text>&#160;
                     </xsl:text> </span> 
                   </xsl:for-each> 
                   <xsl:for-each select="cbc:FamilyName"> 
                    <xsl:apply-templates></xsl:apply-templates> 
                    <span> 
                     <xsl:text>&#160;
                     </xsl:text> </span> 
                   </xsl:for-each> 
                   <xsl:for-each select="cbc:NameSuffix"> 
                    <xsl:apply-templates></xsl:apply-templates> 
                   </xsl:for-each> 
                  </xsl:for-each>  
                 </xsl:for-each> 
                </xsl:for-each> 
               </xsl:for-each> 
              </xsl:otherwise> 
             </xsl:choose></span></span></span></td> 
        </tr> 
        <tr align="left"> 
         <td align="left" style="width: 469px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName"></xsl:value-of> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:value-of select="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:StreetName"></xsl:value-of> 
              </xsl:otherwise> 
             </xsl:choose>&#160;<br />
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingName" /> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:value-of select="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingName" /> 
              </xsl:otherwise> 
             </xsl:choose> &#160;Bina No:
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber" /> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:value-of select="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:BuildingNumber" /> 
              </xsl:otherwise> 
             </xsl:choose>&#160;Kap&#305; No:
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:Room" /> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:value-of select="n1:Invoice/cac:PostalAddress/cac:Party/cac:PostalAddress/cbc:Room" /> 
              </xsl:otherwise> 
             </xsl:choose>&#160; 
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:Region" /> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:value-of select="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:Region" /> 
              </xsl:otherwise> 
             </xsl:choose><br />
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone" /> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:value-of select="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:PostalZone" /> 
              </xsl:otherwise> 
             </xsl:choose>&#160;
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CitySubdivisionName" /> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:value-of select="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:CitySubdivisionName" /> 
              </xsl:otherwise> 
             </xsl:choose> /
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName" /> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:value-of select="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cbc:CityName" /> 
              </xsl:otherwise> 
             </xsl:choose>  </span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left" style="width: 469px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name" /> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:value-of select="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name" /> 
              </xsl:otherwise> 
             </xsl:choose></span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left" style="width: 469px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:if test="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail !=''"> 
                <xsl:text>
                  E-Posta: 
                </xsl:text> 
                <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail"></xsl:value-of> 
               </xsl:if> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:if test="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail != ''"> 
                <xsl:text>
                  E-Posta: 
                </xsl:text> 
                <xsl:value-of select="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail"></xsl:value-of> 
               </xsl:if> 
              </xsl:otherwise> 
             </xsl:choose></span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left" style="width: 469px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:if test="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:Telephone !=''"> 
                <xsl:text>
                  Tel: 
                </xsl:text> 
                <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:Telephone" /> 
               </xsl:if> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:if test="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:Contact/cbc:Telephone != ''"> 
                <xsl:text>
                  Tel: 
                </xsl:text> 
                <xsl:value-of select="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:Contact/cbc:Telephone" /> 
               </xsl:if> 
              </xsl:otherwise> 
             </xsl:choose> 
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:if test="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:Telefax !=''"> 
                <xsl:text>
                  Faks: 
                </xsl:text> 
                <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:Contact/cbc:Telefax" /> 
               </xsl:if> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:if test="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:Contact/cbc:Telefax != ''"> 
                <xsl:text>
                  Faks: 
                </xsl:text> 
                <xsl:value-of select="n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:Contact/cbc:Telefax" /> 
               </xsl:if> 
              </xsl:otherwise> 
             </xsl:choose> </span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left" style="width: 469px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:if test="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name !=''"> 
                <xsl:text>
                  Vergi Dairesi: 
                </xsl:text> 
                <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name" /> 
               </xsl:if> 
              </xsl:when> 
             </xsl:choose></span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left" style="width: 469px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:for-each select="//n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification"> 
                <xsl:variable name="schemeID" select="cbc:ID/@schemeID"></xsl:variable> 
                <xsl:if test="$schemeID ='TCKN' or $schemeID ='VKN'"> 
                 <xsl:value-of select="$schemeID"></xsl:value-of> : 
                 <xsl:value-of select="cbc:ID"></xsl:value-of> 
                </xsl:if> 
               </xsl:for-each> 
              </xsl:when> 
             </xsl:choose></span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Ticaret Sicil No: 
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'MERSISNO']"></xsl:value-of> 
              </xsl:when> 
             </xsl:choose></span></span></span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Mersis No: 
             <xsl:choose> 
              <xsl:when test="not(//n1:Invoice/cbc:ProfileID = 'YOLCUBERABERFATURA' or //n1:Invoice/cbc:ProfileID = 'IHRACAT')"> 
               <xsl:value-of select="n1:Invoice/cac:AccountingCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'TICARETSICILNO']" /> 
              </xsl:when> 
             </xsl:choose></span></span></span></td> 
        </tr> 
       </tbody> 
      </table> </td> 
     <td style="width: 197px;">&#160;</td> 
     <td align="right" style="width: 245px;"> 
      <table style="width: 242px; border-collapse: collapse;"> 
       <tbody> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 116px;"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Fatura Tipi:</span></span></span></span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:value-of select="n1:Invoice/cbc:InvoiceTypeCode" /></span></span></span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 116px;"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Belge </span><span style="font-weight: bold;">No:</span></span></span></span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:value-of select="n1:Invoice/cbc:ID"></xsl:value-of></span></span></span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 116px;"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Fatura Tarihi:</span></span></span></span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:Invoice"> 
              <xsl:for-each select="cbc:IssueDate"> 
               <xsl:value-of select="substring(.,9,2)" />-
               <xsl:value-of select="substring(.,6,2)" />-
               <xsl:value-of select="substring(.,1,4)" /> 
              </xsl:for-each> 
             </xsl:for-each>&#160;
             <xsl:value-of select="n1:Invoice/cbc:IssueTime" /></span></span></span></td> 
        </tr> 
        <xsl:if test="//n1:Invoice/cac:DespatchDocumentReference">
         <tr> 
          <td style="border: 0.05em solid rgb(132, 132, 132); width: 116px;"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><strong>&#304;rsaliye No:</strong></span></span></span></td> 
          <td style="border: 0.05em solid rgb(132, 132, 132); width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:for-each select="//n1:Invoice/cac:DespatchDocumentReference"> 
               <xsl:value-of select="cbc:ID" /> 
               <br /> 
              </xsl:for-each> </span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:Invoice/cac:DespatchDocumentReference">
         <tr> 
          <td style="border: 0.05em solid rgb(132, 132, 132); width: 116px;"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><strong>&#304;rsaliye Tarihi:</strong></span></span></span></td> 
          <td style="border: 0.05em solid rgb(132, 132, 132); width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:for-each select="//n1:Invoice/cac:DespatchDocumentReference"> 
               <xsl:variable name="ddt" select="cbc:IssueDate" /> 
               <xsl:value-of select="concat(substring($ddt, 9, 2), '-', substring($ddt, 6, 2), '-', substring($ddt, 1, 4))" /> 
               <br /> 
              </xsl:for-each> </span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:Invoice/cac:OrderReference">
         <tr> 
          <td style="border: 0.05em solid rgb(132, 132, 132); width: 116px;"><strong style="color: #696969; font-family: verdana, geneva, sans-serif; font-size: 10px;">Sipari&#351; No:</strong></td> 
          <td style="border: 0.05em solid rgb(132, 132, 132); width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:value-of select="n1:Invoice/cac:OrderReference/cbc:ID" /></span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:Invoice/cac:OrderReference">
         <tr> 
          <td style="border: 0.05em solid rgb(132, 132, 132); width: 116px;"><strong style="color: #696969; font-family: verdana, geneva, sans-serif; font-size: 10px;">Sipari&#351; Tarihi:</strong></td> 
          <td style="border: 0.05em solid rgb(132, 132, 132); width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:variable name="spdt" select="n1:Invoice/cac:OrderReference/cbc:IssueDate" /> 
              <xsl:value-of select="concat(substring($spdt, 9, 2), '-', substring($spdt, 6, 2), '-', substring($spdt, 1, 4))" /></span></span></span></td> 
         </tr>
        </xsl:if> 
        <tr loop="loop" loopcond="cbc:ID = 'faturaIptal' and cbc:DocumentType = '1'"> 
         <td align="left" id="lineTableTd" style="border: 0.05em solid rgb(132, 132, 132); width: 116px;"><strong style="color: #696969; font-family: verdana, geneva, sans-serif; font-size: 10px;">&#304;ptal Tarihi:</strong></td> 
         <td id="lineTableTd" style="border: 0.05em solid rgb(132, 132, 132); width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="cbc:IssueDate"> 
              <xsl:value-of select="substring(.,9,2)" />-
              <xsl:value-of select="substring(.,6,2)" />-
              <xsl:value-of select="substring(.,1,4)" /> 
             </xsl:for-each> </span> </span> </span></td> 
        </tr> 
       </tbody> 
      </table> </td> 
    </tr> 
    <tr> 
     <td style="width: 205px;"><span style="font-family: verdana,geneva,sans-serif;"><span style="color: #696969;"><span style="font-size: 10px;"><strong>ETTN: </strong>
         <xsl:value-of select="n1:Invoice/cbc:UUID"></xsl:value-of></span></span></span></td> 
     <td style="width: 197px;">&#160;</td> 
     <td style="width: 245px;">&#160;</td> 
    </tr> 
   </tbody> 
  </table>
 </body>
</html><html>
 <head></head>
 <body>
  <table style="width: 800px; border-collapse: collapse; margin-top: 20px; border: 2px solid #000000;"> 
   <tbody> 
    <tr> 
     <td style="width: 20px; text-align: center; background-color: #000000; border: 0.05em solid #848484;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana, geneva, sans-serif;">S&#305;ra No</span></strong></span></span></td> 
     <td style="width: 20px; text-align: center; background-color: #000000; border: 0.05em solid #848484;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana, geneva, sans-serif;">Malzeme / Hizmet A&#231;&#305;klamas&#305;</span></strong></span></span></td> 
     <td style="width: 20px; text-align: center; background-color: #000000; border: 0.05em solid #848484;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana, geneva, sans-serif;">Miktar</span></strong></span></span></td> 
     <td style="width: 20px; text-align: center; background-color: #000000; border: 0.05em solid #848484;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana, geneva, sans-serif;">Birim Fiyat</span></strong></span></span></td> 
     <td style="width: 20px; text-align: center; background-color: #000000; border: 0.05em solid #848484;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana, geneva, sans-serif;">&#304;skonto Oran&#305;</span></strong></span></span></td> 
     <td style="width: 20px; text-align: center; background-color: #000000; border: 0.05em solid #848484;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana, geneva, sans-serif;">&#304;skonto Tutar&#305;</span></strong></span></span></td> 
     <td style="width: 20px; text-align: center; background-color: #000000; border: 0.05em solid #848484;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana, geneva, sans-serif;">KDV Oran&#305;</span></strong></span></span></td> 
     <td style="width: 20px; text-align: center; background-color: #000000; border: 0.05em solid #848484;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana, geneva, sans-serif;">KDV Tutar&#305;</span></strong></span></span></td> 
     <td style="width: 20px; text-align: center; background-color: #000000; border: 0.05em solid #848484;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana, geneva, sans-serif;">Di&#287;er Vergiler</span></strong></span></span></td> 
     <td style="width: 20px; text-align: center; background-color: #000000; border: 0.05em solid #848484;"><span style="color: #ffffff;"><span style="font-size: 9px;"><strong><span style="font-family: verdana, geneva, sans-serif;">Malzeme / Hizmet Tutar&#305;</span></strong></span></span></td> 
    </tr> 
    <xsl:for-each select="//n1:Invoice/cac:InvoiceLine">
     <tr> 
      <td style="width: 20.2188px; border: 0.05em solid #848484;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">
          <xsl:value-of select="./cbc:ID" /></span></span></span></td> 
      <td style="width: 359px; text-align: center; border: 0.05em solid #848484;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">
          <xsl:value-of select="./cac:Item/cbc:Description" /></span></span></span></td> 
      <td style="width: 59px; text-align: right; border: 0.05em solid #848484;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">
          <xsl:value-of select="format-number(./cbc:InvoicedQuantity, &apos;###.##0,####&apos;, &apos;european&apos;)"></xsl:value-of>&#160;
          <xsl:if test="./cbc:InvoicedQuantity/@unitCode"> 
           <xsl:for-each select="./cbc:InvoicedQuantity"> 
            <xsl:text></xsl:text> 
            <xsl:choose> 
             <xsl:when test="@unitCode  = &apos;AWG&apos;"> 
              <span> 
               <xsl:text>
                amerikan kablo &#246;l&#231;er
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;AYR&apos;"> 
              <span> 
               <xsl:text>
                alt&#305;n ayar&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BAS&apos;"> 
              <span> 
               <xsl:text>
                BAS
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BPM&apos;"> 
              <span> 
               <xsl:text>
                dakika ba&#351;&#305;na vuru&#351;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CPR&apos;"> 
              <span> 
               <xsl:text>
                ADET-&#199;&#304;FT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;DBM&apos;"> 
              <span> 
               <xsl:text>
                desibel-milivat
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;DBW&apos;"> 
              <span> 
               <xsl:text>
                desibel-vat
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;FNU&apos;"> 
              <span> 
               <xsl:text>
                Formazin nefelometrik birim
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;GMS&apos;"> 
              <span> 
               <xsl:text>
                G&#220;M&#220;&#350;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;HAD&apos;"> 
              <span> 
               <xsl:text>
                Adet G&#252;n&#220;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;HMO&apos;"> 
              <span> 
               <xsl:text>
                Adet Ay&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;HWE&apos;"> 
              <span> 
               <xsl:text>
                Adet Haftas&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;IUG&apos;"> 
              <span> 
               <xsl:text>
                uluslararas&#305; birim ba&#351;&#305;na gram
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KFO&apos;"> 
              <span> 
               <xsl:text>
                D&#304;FOSFOR PENTAOKS&#304;T K&#304;LOGRAMI
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KH6&apos;"> 
              <span> 
               <xsl:text>
                K&#304;LOGRAM-BA&#350;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KHO&apos;"> 
              <span> 
               <xsl:text>
                H&#304;DROJEN PEROKS&#304;T K&#304;LOGRAMI
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KNM&apos;"> 
              <span> 
               <xsl:text>
                kilonewton b&#246;l&#252; metrekare
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KOH&apos;"> 
              <span> 
               <xsl:text>
                K&#304;LOGRAM POTASYUM H&#304;DROKS&#304;T
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KPR&apos;"> 
              <span> 
               <xsl:text>
                K&#304;LOGRAM-&#199;&#304;FT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KWN&apos;"> 
              <span> 
               <xsl:text>
                KARAT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KWS&apos;"> 
              <span> 
               <xsl:text>
                OTV Maktu Vergi
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KWY&apos;"> 
              <span> 
               <xsl:text>
                OTV birim fiyat&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MKD&apos;"> 
              <span> 
               <xsl:text>
                B&#304;N K&#304;LOWATT SAAT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MKM&apos;"> 
              <span> 
               <xsl:text>
                Metrekare Ay&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MKW&apos;"> 
              <span> 
               <xsl:text>
                Metrekare Haftas&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MNJ&apos;"> 
              <span> 
               <xsl:text>
                Normalize Metrek&#252;p ba&#351;&#305;na Mega Joule
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MQD&apos;"> 
              <span> 
               <xsl:text>
                Metrek&#252;p G&#252;n
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MQM&apos;"> 
              <span> 
               <xsl:text>
                Metrek&#252;p Ay
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MQW&apos;"> 
              <span> 
               <xsl:text>
                Metrek&#252;p Hafta
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MRD&apos;"> 
              <span> 
               <xsl:text>
                Metre G&#252;n
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MRM&apos;"> 
              <span> 
               <xsl:text>
                Metre Ay
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MRW&apos;"> 
              <span> 
               <xsl:text>
                Metre Hafta
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MTZ&apos;"> 
              <span> 
               <xsl:text>
                millihertz
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NCR&apos;"> 
              <span> 
               <xsl:text>
                KARAT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NTU&apos;"> 
              <span> 
               <xsl:text>
                Nefelometrik bulan&#305;kl&#305;k birimi
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;ODG&apos;"> 
              <span> 
               <xsl:text>
                ODS Grams
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;ODK&apos;"> 
              <span> 
               <xsl:text>
                ODS Kilograms
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;ODM&apos;"> 
              <span> 
               <xsl:text>
                ODS Milligrams
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;OMV&apos;"> 
              <span> 
               <xsl:text>
                OTV Maktu Vergi
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;OPM&apos;"> 
              <span> 
               <xsl:text>
                Dakikadaki sal&#305;n&#305;mlar
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;OTB&apos;"> 
              <span> 
               <xsl:text>
                OTV birim fiyat&#305;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q29&apos;"> 
              <span> 
               <xsl:text>
                hektogram ba&#351;&#305;na mikrogram
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q30&apos;"> 
              <span> 
               <xsl:text>
                kilojoule/gram
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q31&apos;"> 
              <span> 
               <xsl:text>
                femtolitre
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q32&apos;"> 
              <span> 
               <xsl:text>
                pikolitre
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q33&apos;"> 
              <span> 
               <xsl:text>
                nanolitre
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q34&apos;"> 
              <span> 
               <xsl:text>
                megavat/dakika
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q35&apos;"> 
              <span> 
               <xsl:text>
                metrekare/metrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q36&apos;"> 
              <span> 
               <xsl:text>
                G&#252;nl&#252;k standart metrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q38&apos;"> 
              <span> 
               <xsl:text>
                Saatte standart metrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q40&apos;"> 
              <span> 
               <xsl:text>
                Saatte normalle&#351;tirilmi&#351; metrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q41&apos;"> 
              <span> 
               <xsl:text>
                Joule/normalle&#351;tirilmi&#351; metrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Q42&apos;"> 
              <span> 
               <xsl:text>
                Joule/standart metrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;TPI&apos;"> 
              <span> 
               <xsl:text>
                in&#231; ba&#351;&#305;na di&#351;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;TWH&apos;"> 
              <span> 
               <xsl:text>
                B&#304;N K&#304;LOWATT SAAT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;Z9&apos;"> 
              <span> 
               <xsl:text>
                nanomol
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;SA&apos;"> 
              <span> 
               <xsl:text>
                &#199;UVAL
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;26&apos;"> 
              <span> 
               <xsl:text>
                Ton
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BX&apos;"> 
              <span> 
               <xsl:text>
                Kutu
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;LTR&apos;"> 
              <span> 
               <xsl:text>
                LT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;EA&apos;"> 
              <span> 
               <xsl:text>
                Adet
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NIU&apos;"> 
              <span> 
               <xsl:text>
                Adet
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;C62&apos;"> 
              <span> 
               <xsl:text>
                Adet
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KGM&apos;"> 
              <span> 
               <xsl:text>
                KG
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KJO&apos;"> 
              <span> 
               <xsl:text>
                kJ
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;GRM&apos;"> 
              <span> 
               <xsl:text>
                GR
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MGM&apos;"> 
              <span> 
               <xsl:text>
                MG
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NT&apos;"> 
              <span> 
               <xsl:text>
                Net Ton
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;GT&apos;"> 
              <span> 
               <xsl:text>
                Gross Ton
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MTR&apos;"> 
              <span> 
               <xsl:text>
                M
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MMT&apos;"> 
              <span> 
               <xsl:text>
                MM
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KTM&apos;"> 
              <span> 
               <xsl:text>
                KM
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MLT&apos;"> 
              <span> 
               <xsl:text>
                ML
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MMQ&apos;"> 
              <span> 
               <xsl:text>
                MM&#179;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CLT&apos;"> 
              <span> 
               <xsl:text>
                CL
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CMK&apos;"> 
              <span> 
               <xsl:text>
                CM&#178;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CMQ&apos;"> 
              <span> 
               <xsl:text>
                CM&#179;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CMT&apos;"> 
              <span> 
               <xsl:text>
                CM
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MTK&apos;"> 
              <span> 
               <xsl:text>
                M&#178;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MTQ&apos;"> 
              <span> 
               <xsl:text>
                M&#179;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;DAY&apos;"> 
              <span> 
               <xsl:text>
                G&#252;n
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MON&apos;"> 
              <span> 
               <xsl:text>
                Ay
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;PA&apos;"> 
              <span> 
               <xsl:text>
                Paket
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KWH&apos;"> 
              <span> 
               <xsl:text>
                KWH
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;INH&apos;"> 
              <span> 
               <xsl:text>
                IN
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;FOT&apos;"> 
              <span> 
               <xsl:text>
                FT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MMK&apos;"> 
              <span> 
               <xsl:text>
                MM&#178;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;INK&apos;"> 
              <span> 
               <xsl:text>
                IN&#178;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;FTK&apos;"> 
              <span> 
               <xsl:text>
                FT&#178;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;INQ&apos;"> 
              <span> 
               <xsl:text>
                IN&#179;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;FTQ&apos;"> 
              <span> 
               <xsl:text>
                FT&#179;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;ONZ&apos;"> 
              <span> 
               <xsl:text>
                OZ
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;LBR&apos;"> 
              <span> 
               <xsl:text>
                LB
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;PR&apos;"> 
              <span> 
               <xsl:text>
                &#199;ift
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BE&apos;"> 
              <span> 
               <xsl:text>
                Deste
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;PK&apos;"> 
              <span> 
               <xsl:text>
                Koli
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;SW&apos;"> 
              <span> 
               <xsl:text>
                Top
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;QB&apos;"> 
              <span> 
               <xsl:text>
                Cilt
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;SEC&apos;"> 
              <span> 
               <xsl:text>
                Saniye
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;TN&apos;"> 
              <span> 
               <xsl:text>
                Teneke
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MIN&apos;"> 
              <span> 
               <xsl:text>
                Dakika
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;DZN&apos;"> 
              <span> 
               <xsl:text>
                D&#252;zine
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;DPC&apos;"> 
              <span> 
               <xsl:text>
                D&#252;zine
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;5B&apos;"> 
              <span> 
               <xsl:text>
                Tak&#305;m
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;DMQ&apos;"> 
              <span> 
               <xsl:text>
                Desimetrek&#252;p
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;WEE&apos;"> 
              <span> 
               <xsl:text>
                Hafta
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;HUR&apos;"> 
              <span> 
               <xsl:text>
                Saat
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;ANN&apos;"> 
              <span> 
               <xsl:text>
                Y&#305;l
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;RO&apos;"> 
              <span> 
               <xsl:text>
                Rulo
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CTM&apos;"> 
              <span> 
               <xsl:text>
                Karat
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;PTN&apos;"> 
              <span> 
               <xsl:text>
                Par&#231;a,K&#305;s&#305;m
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;B4&apos;"> 
              <span> 
               <xsl:text>
                Varil
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;28&apos;"> 
              <span> 
               <xsl:text>
                KG
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BJ&apos;"> 
              <span> 
               <xsl:text>
                Kova
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BK&apos;"> 
              <span> 
               <xsl:text>
                Sepet
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BL&apos;"> 
              <span> 
               <xsl:text>
                Balya
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BO&apos;"> 
              <span> 
               <xsl:text>
                &#350;i&#351;e
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CO&apos;"> 
              <span> 
               <xsl:text>
                Damacana
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CQ&apos;"> 
              <span> 
               <xsl:text>
                Kartu&#351;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CS&apos;"> 
              <span> 
               <xsl:text>
                Kasa
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CT&apos;"> 
              <span> 
               <xsl:text>
                Karton
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;11&apos;"> 
              <span> 
               <xsl:text>
                Tak&#305;m
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;2W&apos;"> 
              <span> 
               <xsl:text>
                Sepet
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;B32&apos;"> 
              <span> 
               <xsl:text>
                KG-METRE KARE
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CCT&apos;"> 
              <span> 
               <xsl:text>
                TON BA&#350;INA TA&#350;IMA KAPAS&#304;TES&#304;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D30&apos;"> 
              <span> 
               <xsl:text>
                BR&#220;T KALOR&#304; DE&#286;ER&#304;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D40&apos;"> 
              <span> 
               <xsl:text>
                B&#304;N L&#304;TRE
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;GFI&apos;"> 
              <span> 
               <xsl:text>
                FISSILE &#304;ZOTOP GRAMI
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;H62&apos;"> 
              <span> 
               <xsl:text>
                Y&#220;Z ADET
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;K20&apos;"> 
              <span> 
               <xsl:text>
                K&#304;LOGRAM POTASYUM OKS&#304;T
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;K58&apos;"> 
              <span> 
               <xsl:text>
                KURUTULMU&#350; NET A&#286;IRLIKLI K&#304;LOGRAMI
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;K62&apos;"> 
              <span> 
               <xsl:text>
                K&#304;LOGRAM-ADET
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KMA&apos;"> 
              <span> 
               <xsl:text>
                MET&#304;L AM&#304;NLER&#304;N K&#304;LOGRAMI
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KNI&apos;"> 
              <span> 
               <xsl:text>
                AZOTUN K&#304;LOGRAMI
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KPH&apos;"> 
              <span> 
               <xsl:text>
                Kg POTASYUM OKS&#304;D
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KSD&apos;"> 
              <span> 
               <xsl:text>
                %90 KURU &#220;R&#220;N K&#304;LOGRAMI
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KSH&apos;"> 
              <span> 
               <xsl:text>
                SODYUM H&#304;DROKS&#304;T K&#304;LOGRAMI
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KUR&apos;"> 
              <span> 
               <xsl:text>
                URANYUM K&#304;LOGRAMI
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KWT&apos;"> 
              <span> 
               <xsl:text>
                K&#304;LOWATT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KWT&apos;"> 
              <span> 
               <xsl:text>
                K&#304;LOWATT
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;LPA&apos;"> 
              <span> 
               <xsl:text>
                SAF ALKOL L&#304;TRES&#304;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NCL&apos;"> 
              <span> 
               <xsl:text>
                H&#220;CRE ADED&#304;
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;R9&apos;"> 
              <span> 
               <xsl:text>
                B&#304;N METRE K&#220;P
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;SET&apos;"> 
              <span> 
               <xsl:text>
                SET
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;T3&apos;"> 
              <span> 
               <xsl:text>
                B&#304;N ADET
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D61&apos;"> 
              <span> 
               <xsl:text>
                Dakika
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;E07&apos;"> 
              <span> 
               <xsl:text>
                MWh/h
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D62&apos;"> 
              <span> 
               <xsl:text>
                Saniye
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D64&apos;"> 
              <span> 
               <xsl:text>
                BLOK
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;5C&apos;"> 
              <span> 
               <xsl:text>
                Galon
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BG&apos;"> 
              <span> 
               <xsl:text>
                Torba
               </xsl:text> </span> 
             </xsl:when> 
             <xsl:otherwise> 
              <span> 
               <xsl:value-of select="@unitCode" /> </span> 
             </xsl:otherwise> 
            </xsl:choose> 
           </xsl:for-each> 
          </xsl:if></span></span></span></td> 
      <td style="width: 50px; text-align: right; border: 0.05em solid #848484;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">
          <xsl:value-of select="format-number(./cac:Price/cbc:PriceAmount, &apos;###.##0,0000&apos;, &apos;european&apos;)" /> 
          <xsl:if test="./cac:Price/cbc:PriceAmount/@currencyID"> 
           <xsl:text> 
           </xsl:text> 
           <xsl:if test="./cac:Price/cbc:PriceAmount/@currencyID = &apos;TRY&apos; "> 
            <xsl:text>
             TL
            </xsl:text> 
           </xsl:if> 
           <xsl:if test="./cac:Price/cbc:PriceAmount/@currencyID != &apos;TRY&apos;"> 
            <xsl:value-of select="./cac:Price/cbc:PriceAmount/@currencyID" /> 
           </xsl:if> 
          </xsl:if> </span></span></span></td> 
      <td style="width: 38px; text-align: right; border: 0.05em solid #848484;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"> 
          <xsl:for-each select="./cac:AllowanceCharge"> 
           <xsl:text> 
           </xsl:text> 
           <table id="allowanceChargeRateTable" border="0" cellpadding="0" cellspacing="0" align="right" tagid="allowanceChargeRate" style="clear:both" width="100%"> 
            <tbody> 
             <tr id="budgetContainerTr" align="right"> 
              <td align="right"> 
               <xsl:text>
                 % 
               </xsl:text> 
               <xsl:value-of select="format-number(cbc:MultiplierFactorNumeric*100, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> </td> 
             </tr> 
            </tbody> 
           </table> 
          </xsl:for-each></span></span></span></td> 
      <td style="width: 60px; text-align: right; border: 0.05em solid #848484;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"> 
          <xsl:for-each select="./cac:AllowanceCharge"> 
           <xsl:text></xsl:text> 
           <table id="allowanceChargeAmountTable" border="0" cellpadding="0" cellspacing="0" align="right" tagid="allowanceChargeAmount" style="clear:both" width="100%"> 
            <tbody> 
             <tr id="budgetContainerTr" align="right"> 
              <td align="right"> 
               <xsl:value-of select="format-number(cbc:Amount, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
               <xsl:if test="cbc:Amount/@currencyID"> 
                <xsl:text> 
                </xsl:text> 
                <xsl:if test="cbc:Amount/@currencyID = &apos;TRY&apos;"> 
                 <xsl:text>
                   TL 
                 </xsl:text> 
                </xsl:if> 
                <xsl:if test="cbc:Amount/@currencyID != &apos;TRY&apos;"> 
                 <xsl:value-of select="cbc:Amount/@currencyID"></xsl:value-of> 
                </xsl:if> 
               </xsl:if> </td> 
             </tr> 
            </tbody> 
           </table> 
          </xsl:for-each></span></span></span></td> 
      <td style="width: 40px; text-align: right; border: 0.05em solid #848484;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">
          <xsl:for-each select="./cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme"> 
           <xsl:if test="cbc:TaxTypeCode=&apos;0015&apos; "> 
            <xsl:if test="../../cbc:Percent"> 
             <xsl:text>
               %
             </xsl:text> 
             <xsl:value-of select="format-number(../../cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
            </xsl:if> 
           </xsl:if> 
          </xsl:for-each> </span></span></span></td> 
      <td style="width: 31.2344px; text-align: right; border: 0.05em solid #848484;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">
          <xsl:for-each select="./cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme"> 
           <xsl:if test="cbc:TaxTypeCode=&apos;0015&apos; "> 
            <xsl:text> 
            </xsl:text> 
            <xsl:value-of select="format-number(../../cbc:TaxAmount, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
            <xsl:if test="../../cbc:TaxAmount/@currencyID"> 
             <xsl:text> 
             </xsl:text> 
             <xsl:if test="../../cbc:TaxAmount/@currencyID = &apos;TRY&apos;"> 
              <xsl:text>
               TL
              </xsl:text> 
             </xsl:if> 
             <xsl:if test="../../cbc:TaxAmount/@currencyID != &apos;TRY&apos;"> 
              <xsl:value-of select="../../cbc:TaxAmount/@currencyID" /> 
             </xsl:if> 
            </xsl:if> 
           </xsl:if> 
          </xsl:for-each> </span></span></span></td> 
      <td style="width: 40.2188px; text-align: right; border: 0.05em solid #848484;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">
          <xsl:for-each select="./cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme"> 
           <xsl:if test="cbc:TaxTypeCode!=&apos;0015&apos; "> 
            <xsl:text> 
            </xsl:text> 
            <xsl:value-of select="cbc:Name" /> 
            <xsl:if test="../../cbc:Percent"> 
             <xsl:text>
               (%
             </xsl:text> 
             <xsl:value-of select="format-number(../../cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
             <xsl:text>
              )=
             </xsl:text> 
            </xsl:if> 
            <xsl:value-of select="format-number(../../cbc:TaxAmount, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
            <xsl:if test="../../cbc:TaxAmount/@currencyID"> 
             <xsl:text> 
             </xsl:text> 
             <xsl:if test="../../cbc:TaxAmount/@currencyID = &apos;TRY&apos;"> 
              <xsl:text>
               TL
              </xsl:text> 
             </xsl:if> 
             <xsl:if test="../../cbc:TaxAmount/@currencyID != &apos;TRY&apos;"> 
              <xsl:value-of select="../../cbc:TaxAmount/@currencyID" /> 
             </xsl:if> 
            </xsl:if> 
           </xsl:if> 
          </xsl:for-each>
          <xsl:for-each select="./cac:WithholdingTaxTotal/cac:TaxSubtotal">
           <xsl:if test="not(cbc:TaxAmount = 0)">
            <xsl:choose>
             <xsl:when test="cac:TaxCategory/cac:TaxScheme/cbc:Name=&apos;&apos;">
              <xsl:text>
               Di&#287;er Vergiler Toplam&#305; 
              </xsl:text>
             </xsl:when>
             <xsl:otherwise>
              <xsl:text>
               Tevkifat (
              </xsl:text>
              <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode" />
              <xsl:text>
               -
              </xsl:text>
              <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name" />
              <xsl:text>
               )
              </xsl:text>
             </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="cbc:Percent !=&apos;&apos;">
             <xsl:text>
              (%
             </xsl:text>
             <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)" />
             <xsl:text>
              )= 
             </xsl:text>
            </xsl:if>
            <xsl:value-of select="format-number(../cbc:TaxAmount, &apos;###.##0,00&apos;, &apos;european&apos;)" />
            <xsl:if test="../cbc:TaxAmount/@currencyID">
             <xsl:if test="../cbc:TaxAmount/@currencyID = &apos;TRY&apos;">
              <xsl:text>
               TL
              </xsl:text>
             </xsl:if>
             <xsl:if test="../cbc:TaxAmount/@currencyID != &apos;TRY&apos;">
              <xsl:value-of select="../cbc:TaxAmount/@currencyID" />
             </xsl:if>
            </xsl:if>
           </xsl:if>
          </xsl:for-each> </span></span></span></td> 
      <td style="width: 44.5469px; text-align: right; border: 0.05em solid #848484;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">
          <xsl:value-of select="format-number(./cbc:LineExtensionAmount, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
          <xsl:if test="./cbc:LineExtensionAmount/@currencyID"> 
           <xsl:text> 
           </xsl:text> 
           <xsl:if test="./cbc:LineExtensionAmount/@currencyID = &apos;TRY&apos; "> 
            <xsl:text>
             TL
            </xsl:text> 
           </xsl:if> 
           <xsl:if test="./cbc:LineExtensionAmount/@currencyID != &apos;TRY&apos; "> 
            <xsl:value-of select="./cbc:LineExtensionAmount/@currencyID"> 
            </xsl:value-of>
           </xsl:if> 
          </xsl:if> </span></span></span></td> 
     </tr>
    </xsl:for-each> 
   </tbody> 
  </table>
 </body>
</html><html>
 <head></head>
 <body>
  <table border="0" cellpadding="0" cellspacing="0" style="width: 800px;"> 
   <tbody> 
    <tr> 
     <td align="right"> 
      <table border="1" bordercolor="#a4a4a4" cellpadding="0" cellspacing="0" id="budgetContainerTable" style="width: 420px;"> 
       <tbody> 
        <tr align="right" id="budgetContainerTr"> 
         <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Mal / Hizmet Toplam Tutar&#305;</span></span></span></span></td> 
         <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:value-of select="format-number(//n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
             <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID"> 
              <xsl:text> 
              </xsl:text> 
              <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID = &apos;TRY&apos;"> 
               <xsl:text>
                TL
               </xsl:text> 
              </xsl:if> 
              <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID != &apos;TRY&apos;"> 
               <xsl:value-of select="//n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID" /> 
              </xsl:if> 
             </xsl:if> </span></span></span></td> 
        </tr> 
        <xsl:if test="not(//n1:Invoice/cac:LegalMonetaryTotal/cbc:AllowanceTotalAmount = 0)">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><strong><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:text>
                Toplam &#304;skonto
               </xsl:text> 
               <xsl:if test="//n1:Invoice/cac:AllowanceCharge/cbc:MultiplierFactorNumeric"> 
                <xsl:text>
                 (%
                </xsl:text> 
                <xsl:value-of select="format-number((//n1:Invoice/cac:LegalMonetaryTotal/cbc:AllowanceTotalAmount*100) div //n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount, &apos;###.##0,00&apos;, &apos;european&apos;)" />
                <xsl:text>
                 )
                </xsl:text> 
               </xsl:if></span></span></span></strong></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:value-of select="format-number(//n1:Invoice/cac:LegalMonetaryTotal/cbc:AllowanceTotalAmount, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
              <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:AllowanceTotalAmount/@currencyID"> 
               <xsl:text> 
               </xsl:text> 
               <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:AllowanceTotalAmount/@currencyID = &apos;TRY&apos;"> 
                <xsl:text>
                 TL
                </xsl:text> 
               </xsl:if> 
               <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:AllowanceTotalAmount/@currencyID != &apos;TRY&apos;"> 
                <xsl:value-of select="//n1:Invoice/cac:LegalMonetaryTotal/cbc:AllowanceTotalAmount/@currencyID" /> 
               </xsl:if> 
              </xsl:if> </span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="not(//n1:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount = 0)">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><strong><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Toplam Masraf</span></span></span></span></strong></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:value-of select="format-number(//n1:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
              <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount/@currencyID"> 
               <xsl:text></xsl:text> 
               <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount/@currencyID = &apos;TRY&apos;"> 
                <xsl:text>
                 TL
                </xsl:text> 
               </xsl:if> 
               <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount/@currencyID != &apos;TRY&apos;"> 
                <xsl:text>&#160;
                </xsl:text> 
                <xsl:value-of select="//n1:Invoice/cac:LegalMonetaryTotal/cbc:ChargeTotalAmount/@currencyID"></xsl:value-of> 
               </xsl:if> 
              </xsl:if> </span></span></span></td> 
         </tr>
        </xsl:if> 
        <tr align="right" id="budgetContainerTr"> 
         <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><strong><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:for-each select="//n1:Invoice/cac:TaxTotal/cac:TaxSubtotal"> 
               <xsl:choose> 
                <xsl:when test="not(cbc:Percent = 0.0000) or not(cbc:TaxAmount = 0)"> 
                 <table id="taxSubtotalHeaderTable" border="0" cellpadding="0" cellspacing="0" style="width: 100%;"> 
                  <tbody> 
                   <tr align="right"> 
                    <td id="budgetContainerDummyTd"></td> 
                    <td align="right"> <span style="font-weight:bold;"> 
                      <xsl:choose> 
                       <xsl:when test="cac:TaxCategory/cac:TaxScheme/cbc:Name=&apos;&apos;"> 
                        <xsl:text>
                         Di&#287;er Vergiler Toplam&#305; 
                        </xsl:text> 
                       </xsl:when> 
                       <xsl:otherwise> 
                        <xsl:text>
                         Hesaplanan 
                        </xsl:text> 
                        <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:value-of> 
                       </xsl:otherwise> 
                      </xsl:choose> 
                      <xsl:if test="cbc:Percent"> 
                       <xsl:text>
                        (%
                       </xsl:text> 
                       <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                       <xsl:text>
                        )
                       </xsl:text> 
                      </xsl:if> </span> </td> 
                   </tr> 
                  </tbody> 
                 </table> 
                </xsl:when> 
                <xsl:when test="cbc:Percent = 0.0000 or cbc:TaxAmount = 0"> 
                 <table id="taxSubtotalHeaderTable" border="0" cellpadding="0" cellspacing="0" style="width: 100%;"> 
                  <tbody> 
                   <tr align="right"> 
                    <td id="budgetContainerDummyTd"></td> 
                    <td align="right"> <span style="font-weight:bold;"> 
                      <xsl:choose> 
                       <xsl:when test="cac:TaxCategory/cac:TaxScheme/cbc:Name=&apos;&apos;"> 
                        <xsl:text>
                         Di&#287;er Vergiler Toplam&#305; 
                        </xsl:text> 
                       </xsl:when> 
                       <xsl:otherwise> 
                        <xsl:text>
                         Hesaplanan 
                        </xsl:text> 
                        <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:value-of> 
                       </xsl:otherwise> 
                      </xsl:choose> 
                      <xsl:if test="cbc:Percent"> 
                       <xsl:text>
                        (%
                       </xsl:text> 
                       <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                       <xsl:text>
                        )
                       </xsl:text> 
                      </xsl:if> </span> </td> 
                   </tr> 
                  </tbody> 
                 </table> 
                </xsl:when> 
               </xsl:choose> 
              </xsl:for-each></span></span></span></strong></td> 
         <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:Invoice/cac:TaxTotal/cac:TaxSubtotal"> 
              <xsl:choose> 
               <xsl:when test="not(cbc:Percent = 0.0000) or not(cbc:TaxAmount = 0)"> 
                <table id="taxSubtotalsTable" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both;width: 100%;" width="100%"> 
                 <tbody> 
                  <tr id="budgetContainerTr" align="right"> 
                   <td align="right"> 
                    <xsl:for-each select="cac:TaxCategory/cac:TaxScheme"> 
                     <xsl:text /> 
                     <xsl:value-of select="format-number(../../cbc:TaxAmount, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                     <xsl:if test="../../cbc:TaxAmount/@currencyID"> 
                      <xsl:text /> 
                      <xsl:if test="../../cbc:TaxAmount/@currencyID = &apos;TRY&apos;"> 
                       <xsl:text>
                        &#160;TL
                       </xsl:text> 
                      </xsl:if> 
                      <xsl:if test="../../cbc:TaxAmount/@currencyID != &apos;TRY&apos;"> 
                       <xsl:text>&#160;
                       </xsl:text> 
                       <xsl:value-of select="../../cbc:TaxAmount/@currencyID" /> 
                      </xsl:if> 
                     </xsl:if> 
                    </xsl:for-each> </td> 
                  </tr> 
                 </tbody> 
                </table> 
               </xsl:when> 
               <xsl:when test="cbc:Percent = 0.0000 or cbc:TaxAmount = 0"> 
                <table id="taxSubtotalsTable" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both;width: 100%;" width="100%"> 
                 <tbody> 
                  <tr id="budgetContainerTr" align="right"> 
                   <td align="right"> 
                    <xsl:for-each select="cac:TaxCategory/cac:TaxScheme"> 
                     <xsl:text /> 
                     <xsl:value-of select="format-number(../../cbc:TaxAmount, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                     <xsl:if test="../../cbc:TaxAmount/@currencyID"> 
                      <xsl:text /> 
                      <xsl:if test="../../cbc:TaxAmount/@currencyID = &apos;TRY&apos;"> 
                       <xsl:text>
                        &#160;TL
                       </xsl:text> 
                      </xsl:if> 
                      <xsl:if test="../../cbc:TaxAmount/@currencyID != &apos;TRY&apos;"> 
                       <xsl:text>&#160;
                       </xsl:text> 
                       <xsl:value-of select="../../cbc:TaxAmount/@currencyID" /> 
                      </xsl:if> 
                     </xsl:if> 
                    </xsl:for-each> </td> 
                  </tr> 
                 </tbody> 
                </table> 
               </xsl:when> 
              </xsl:choose> 
             </xsl:for-each></span></span></span></td> 
        </tr> 
        <xsl:if test="//n1:Invoice/cac:WithholdingTaxTotal">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><strong><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:for-each select="//n1:Invoice/cac:WithholdingTaxTotal/cac:TaxSubtotal"> 
                <xsl:if test="not(cbc:Percent = 0.0000) or not(cbc:TaxAmount = 0)"> 
                 <table id="taxSubtotalHeaderTable" cellpadding="0" cellspacing="0" width="100%"> 
                  <tbody>
                   <tr align="right" style="width: 319px;"> 
                    <td width="100%" align="right"> <span style="font-weight:bold; "> 
                      <xsl:choose> 
                       <xsl:when test="cac:TaxCategory/cac:TaxScheme/cbc:Name=&apos;&apos;"> 
                        <xsl:text>
                         Di&#287;er Vergiler Toplam&#305; 
                        </xsl:text> 
                       </xsl:when> 
                       <xsl:otherwise> 
                        <xsl:text>
                         Tevkifat 
                        </xsl:text> 
                        <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:value-of> 
                       </xsl:otherwise> 
                      </xsl:choose> 
                      <xsl:text>
                       (%
                      </xsl:text> 
                      <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                      <xsl:text>
                       )
                      </xsl:text> </span> </td> 
                   </tr>   
                  </tbody>
                 </table>
                </xsl:if>
               </xsl:for-each></span></span></span></strong></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:for-each select="//n1:Invoice/cac:WithholdingTaxTotal/cac:TaxSubtotal"> 
               <xsl:if test="not(cbc:Percent = 0.0000) or not(cbc:TaxAmount = 0)"> 
                <table id="taxSubtotalsTable" cellpadding="0" cellspacing="0" align="right" style="clear:both" width="100%"> 
                 <tbody>
                  <tr id="budgetContainerTr" align="right"> 
                   <td align="right"> 
                    <xsl:for-each select="cac:TaxCategory/cac:TaxScheme"> 
                     <xsl:value-of select="format-number(../../cbc:TaxAmount, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                     <xsl:if test="../../cbc:TaxAmount/@currencyID"> 
                      <xsl:text> 
                      </xsl:text> 
                      <xsl:if test="../../cbc:TaxAmount/@currencyID = &apos;TRY&apos;"> 
                       <xsl:text>
                        TL
                       </xsl:text> 
                      </xsl:if> 
                      <xsl:if test="../../cbc:TaxAmount/@currencyID != &apos;TRY&apos;"> 
                       <xsl:value-of select="../../cbc:TaxAmount/@currencyID"></xsl:value-of> 
                      </xsl:if> 
                     </xsl:if> 
                    </xsl:for-each> </td> 
                  </tr>   
                 </tbody>
                </table>
               </xsl:if>
              </xsl:for-each></span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:Invoice/cbc:InvoiceTypeCode = &apos;TEVKIFAT&apos;">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Tevkifata Tabi &#304;&#351;lem Tutar&#305;</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
              <xsl:if test="n1:Invoice/cac:InvoiceLine[cac:WithholdingTaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme]"> 
               <xsl:value-of select="format-number(sum(n1:Invoice/cac:InvoiceLine[cac:WithholdingTaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme]/cbc:LineExtensionAmount), &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
              </xsl:if> 
              <xsl:if test="//n1:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode=&apos;9015&apos;"> 
               <xsl:value-of select="format-number(sum(n1:Invoice/cac:InvoiceLine[cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode=9015]/cbc:LineExtensionAmount), &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
              </xsl:if> 
              <xsl:if test="n1:Invoice/cbc:DocumentCurrencyCode = &apos;TRY&apos; or n1:Invoice/cbc:DocumentCurrencyCode = &apos;TRY&apos;"> 
               <xsl:text>
                &#160;TL
               </xsl:text> 
              </xsl:if> 
              <xsl:if test="n1:Invoice/cbc:DocumentCurrencyCode != &apos;TRY&apos; and n1:Invoice/cbc:DocumentCurrencyCode != &apos;TRY&apos;"> 
               <xsl:text>&#160;
               </xsl:text> 
               <xsl:value-of select="n1:Invoice/cbc:DocumentCurrencyCode" /> 
              </xsl:if></span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:Invoice/cbc:InvoiceTypeCode = &apos;TEVKIFAT&apos;">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Beyan Edilen KDV Tutar&#305;</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
              <xsl:variable name="totalKDV"> 
               <xsl:choose> 
                <xsl:when test="sum(n1:Invoice/cac:TaxTotal/cac:TaxSubtotal[cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode = &apos;0015&apos;]/cbc:TaxAmount) &gt; 0"> 
                 <xsl:value-of select="sum(n1:Invoice/cac:TaxTotal/cac:TaxSubtotal[cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode = &apos;0015&apos;]/cbc:TaxAmount)"></xsl:value-of> 
                </xsl:when> 
                <xsl:otherwise>
                 0
                </xsl:otherwise> 
               </xsl:choose> 
              </xsl:variable> 
              <xsl:variable name="totalTevkifat"> 
               <xsl:choose> 
                <xsl:when test="n1:Invoice/cac:WithholdingTaxTotal/cbc:TaxAmount &gt; 0"> 
                 <xsl:value-of select="n1:Invoice/cac:WithholdingTaxTotal/cbc:TaxAmount"></xsl:value-of> 
                </xsl:when> 
                <xsl:otherwise>
                 0
                </xsl:otherwise> 
               </xsl:choose> 
              </xsl:variable> 
              <xsl:if test="//n1:Invoice/cbc:InvoiceTypeCode = &apos;TEVKIFAT&apos;"> 
               <div class="toplam_satiri"> 
                <span class="alt_toplam_val"> 
                 <xsl:value-of select="format-number($totalKDV - $totalTevkifat, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                 <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID"> 
                  <xsl:text> 
                  </xsl:text> 
                  <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID = &apos;TRY&apos;"> 
                   <xsl:text>
                    TL
                   </xsl:text> 
                  </xsl:if> 
                  <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID != &apos;TRY&apos;"> 
                   <xsl:value-of select="//n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID"> 
                   </xsl:value-of> 
                  </xsl:if> 
                 </xsl:if> </span> 
               </div> 
              </xsl:if></span></span></span></td> 
         </tr>
        </xsl:if> 
        <tr align="right" id="budgetContainerTr"> 
         <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Vergiler Dahil Toplam Tutar</span></span></span></span></td> 
         <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:Invoice"> 
              <xsl:for-each select="cac:LegalMonetaryTotal"> 
               <xsl:for-each select="cbc:TaxInclusiveAmount"> 
                <xsl:value-of select="format-number(., &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID"> 
                 <xsl:text> 
                 </xsl:text> 
                 <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID = &apos;TRY&apos;"> 
                  <xsl:text>
                   TL
                  </xsl:text> 
                 </xsl:if> 
                 <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID != &apos;TRY&apos;"> 
                  <xsl:value-of select="//n1:Invoice/cac:LegalMonetaryTotal/cbc:TaxInclusiveAmount/@currencyID"> 
                  </xsl:value-of>
                 </xsl:if> 
                </xsl:if> 
               </xsl:for-each> 
              </xsl:for-each> 
             </xsl:for-each> </span></span></span></td> 
        </tr> 
        <tr align="right" id="budgetContainerTr"> 
         <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">&#214;denecek Tutar</span></span></span></span></td> 
         <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:Invoice"> 
              <xsl:for-each select="cac:LegalMonetaryTotal"> 
               <xsl:for-each select="cbc:PayableAmount"> 
                <xsl:value-of select="format-number(., &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID"> 
                 <xsl:text> 
                 </xsl:text> 
                 <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID = &apos;TRY&apos;"> 
                  <xsl:text>
                   TL
                  </xsl:text> 
                 </xsl:if> 
                 <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID != &apos;TRY&apos;"> 
                  <xsl:value-of select="//n1:Invoice/cac:LegalMonetaryTotal/cbc:PayableAmount/@currencyID" /> 
                 </xsl:if> 
                </xsl:if> 
               </xsl:for-each> 
              </xsl:for-each> 
             </xsl:for-each> </span></span></span></td> 
        </tr> 
       </tbody> 
      </table> </td> 
    </tr> 
    <tr> 
     <td align="right"> 
      <xsl:if test="//n1:Invoice/cac:LegalMonetaryTotal/cbc:LineExtensionAmount/@currencyID != &apos;TRY&apos;">
       <table border="1" bordercolor="#a4a4a4" cellpadding="0" cellspacing="0" id="budgetContainerTable" style="width: 420px;"> 
        <tbody> 
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">D&#214;viz Kuru (</span><strong>
               <xsl:value-of select="//n1:Invoice/cac:PricingExchangeRate/cbc:TargetCurrencyCode" /></strong><span style="font-weight: bold;">)</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:value-of select="format-number(//n1:Invoice/cac:PricingExchangeRate/cbc:CalculationRate, &apos;###.##0,0000&apos;, &apos;european&apos;)" /> 
              <xsl:text> 
              </xsl:text> 
              <xsl:text>
               TL
              </xsl:text></span></span></span></td> 
         </tr> 
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Mal / Hizmet Toplam Tutar&#305;</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
              <xsl:for-each select="//n1:Invoice/cac:AdditionalDocumentReference"> 
               <xsl:if test="cbc:DocumentType = &apos;LINEEXTENSIONAMOUNT&apos;"> 
                <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
               </xsl:if> 
              </xsl:for-each> 
              <xsl:text> 
              </xsl:text> 
              <xsl:text>
               TL
              </xsl:text></span></span></span></td> 
         </tr> 
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Toplam &#304;skonto</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"><span style="color: #696969;"> 
              <xsl:for-each select="//n1:Invoice/cac:AdditionalDocumentReference"> 
               <xsl:if test="cbc:DocumentType = &apos;ALLOWANCETOTALAMOUNT&apos;"> 
                <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
               </xsl:if> 
              </xsl:for-each> 
              <xsl:text> 
              </xsl:text> 
              <xsl:text>
               TL
              </xsl:text></span></span></span></td> 
         </tr> 
         <xsl:if test="//n1:Invoice/cac:AllowanceCharge/cbc:MultiplierFactorNumeric">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Toplam Masraf</span></span></span></span></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"><span style="color: #696969;">
               <xsl:for-each select="//n1:Invoice/cac:AdditionalDocumentReference"> 
                <xsl:if test="cbc:DocumentType = &apos;CHARGETOTALAMOUNT&apos;"> 
                 <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                </xsl:if> 
               </xsl:for-each> 
               <xsl:text></xsl:text> 
               <xsl:text>
                 TL 
               </xsl:text></span></span></span></td> 
          </tr>
         </xsl:if> 
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="color: #696969;"><strong><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:for-each select="//n1:Invoice/cac:TaxTotal/cac:TaxSubtotal"> 
                <xsl:choose> 
                 <xsl:when test="not(cbc:Percent = 0.0000) or not(cbc:TaxAmount = 0)"> 
                  <table id="taxSubtotalHeaderTable" border="0" cellpadding="0" cellspacing="0" style="width: 100%;"> 
                   <tbody> 
                    <tr align="right"> 
                     <td id="budgetContainerDummyTd"></td> 
                     <td align="right"> <span style="font-weight:bold;"> 
                       <xsl:choose> 
                        <xsl:when test="cac:TaxCategory/cac:TaxScheme/cbc:Name=&apos;&apos;"> 
                         <xsl:text>
                          Di&#287;er Vergiler Toplam&#305; 
                         </xsl:text> 
                        </xsl:when> 
                        <xsl:otherwise> 
                         <xsl:text>
                          Hesaplanan 
                         </xsl:text> 
                         <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:value-of> 
                        </xsl:otherwise> 
                       </xsl:choose> 
                       <xsl:if test="cbc:Percent"> 
                        <xsl:text>
                         (%
                        </xsl:text> 
                        <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                        <xsl:text>
                         )
                        </xsl:text> 
                       </xsl:if> </span> </td> 
                    </tr> 
                   </tbody> 
                  </table> 
                 </xsl:when> 
                 <xsl:when test="cbc:Percent = 0.0000 or cbc:TaxAmount = 0"> 
                  <table id="taxSubtotalHeaderTable" border="0" cellpadding="0" cellspacing="0" style="width: 100%;"> 
                   <tbody> 
                    <tr align="right"> 
                     <td id="budgetContainerDummyTd"></td> 
                     <td align="right"> <span style="font-weight:bold;"> 
                       <xsl:choose> 
                        <xsl:when test="cac:TaxCategory/cac:TaxScheme/cbc:Name=&apos;&apos;"> 
                         <xsl:text>
                          Di&#287;er Vergiler Toplam&#305; 
                         </xsl:text> 
                        </xsl:when> 
                        <xsl:otherwise> 
                         <xsl:text>
                          Hesaplanan 
                         </xsl:text> 
                         <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:value-of> 
                        </xsl:otherwise> 
                       </xsl:choose> 
                       <xsl:if test="cbc:Percent"> 
                        <xsl:text>
                         (%
                        </xsl:text> 
                        <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                        <xsl:text>
                         )
                        </xsl:text> 
                       </xsl:if> </span> </td> 
                    </tr> 
                   </tbody> 
                  </table> 
                 </xsl:when> 
                </xsl:choose> 
               </xsl:for-each></span></span></strong></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"><span style="color: #696969;">
              <xsl:for-each select="//n1:Invoice/cac:TaxTotal/cac:TaxSubtotal"> 
               <xsl:choose> 
                <xsl:when test="not(cbc:Percent = 0.0000) or not(cbc:TaxAmount = 0)"> 
                 <table id="taxSubtotalsTable" border="0" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both;width: 100%;" width="100%"> 
                  <tbody> 
                   <tr id="budgetContainerTr" align="right"> 
                    <td align="right"> 
                     <xsl:text></xsl:text> 
                     <xsl:variable name="var_Percent" select="cbc:Percent"></xsl:variable> 
                     <xsl:variable name="var_TaxTypeCode" select="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode"></xsl:variable> 
                     <xsl:variable name="var_TaxName" select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:variable> 
                     <xsl:for-each select="//n1:Invoice/cac:AdditionalDocumentReference"> 
                      <xsl:if test="(cbc:DocumentTypeCode = $var_TaxTypeCode)  and (not(cbc:ID=0))"> 
                       <xsl:choose> 
                        <xsl:when test="(cbc:DocumentType = $var_Percent)  and (cbc:DocumentDescription = $var_TaxName)"> 
                         <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                        </xsl:when> 
                        <xsl:otherwise> 
                         <xsl:if test="not($var_Percent) and not(cbc:DocumentType)  and (cbc:DocumentDescription = $var_TaxName)"> 
                          <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                         </xsl:if> 
                        </xsl:otherwise> 
                       </xsl:choose> 
                      </xsl:if> 
                     </xsl:for-each> 
                     <xsl:text>
                      &#160;TL
                     </xsl:text> </td> 
                   </tr> 
                  </tbody> 
                 </table> 
                </xsl:when> 
                <xsl:when test="cbc:Percent = 0.0000 or cbc:TaxAmount = 0"> 
                 <table id="taxSubtotalsTable" border="0" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both;width: 100%;" width="100%"> 
                  <tbody> 
                   <tr id="budgetContainerTr" align="right"> 
                    <td align="right"> 
                     <xsl:text>
                      0,00 TL
                     </xsl:text> </td> 
                   </tr> 
                  </tbody> 
                 </table> 
                </xsl:when> 
               </xsl:choose> 
              </xsl:for-each></span></span></span></td> 
         </tr> 
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="color: #696969;"><strong><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:for-each select="//n1:Invoice/cac:WithholdingTaxTotal/cac:TaxSubtotal"> 
                <xsl:if test="not(cbc:Percent = 0.0000) or not(cbc:TaxAmount = 0)"> 
                 <table id="taxSubtotalHeaderTable" cellpadding="0" cellspacing="0" width="100%"> 
                  <tbody>
                   <tr align="right" style="width: 319px;"> 
                    <td width="100%" align="right"> <span style="font-weight:bold; "> 
                      <xsl:choose> 
                       <xsl:when test="cac:TaxCategory/cac:TaxScheme/cbc:Name=&apos;&apos;"> 
                        <xsl:text>
                         Di&#287;er Vergiler Toplam&#305; 
                        </xsl:text> 
                       </xsl:when> 
                       <xsl:otherwise> 
                        <xsl:text>
                         Tevkifat 
                        </xsl:text> 
                        <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:value-of> 
                       </xsl:otherwise> 
                      </xsl:choose> 
                      <xsl:text>
                       (%
                      </xsl:text> 
                      <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                      <xsl:text>
                       )
                      </xsl:text> </span> </td> 
                   </tr>   
                  </tbody>
                 </table>
                </xsl:if>
               </xsl:for-each></span></span></strong></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"><span style="color: #696969;"> 
              <xsl:for-each select="//n1:Invoice/cac:WithholdingTaxTotal/cac:TaxSubtotal"> 
               <xsl:variable name="var_Percent" select="cbc:Percent"></xsl:variable> 
               <xsl:variable name="var_TaxTypeCode" select="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode"></xsl:variable> 
               <xsl:text> 
               </xsl:text> 
               <xsl:for-each select="//n1:Invoice/cac:AdditionalDocumentReference"> 
                <xsl:variable name="var_DocumentType" select="cbc:DocumentType"></xsl:variable> 
                <xsl:variable name="var_DocumentTypeCode" select="cbc:DocumentTypeCode"></xsl:variable> 
                <xsl:choose> 
                 <xsl:when test="string(number($var_Percent)) != &apos;NaN&apos; and string(number($var_DocumentType)) != &apos;NaN&apos;"> 
                  <xsl:if test="(number($var_DocumentType) = number($var_Percent)) and $var_DocumentTypeCode = $var_TaxTypeCode and not(number($var_Percent) = 0) "> 
                   <table id="taxSubtotalsTable" border="0" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both" width="100%"> 
                    <tbody> 
                     <tr id="budgetContainerTr" align="right" height="80"> 
                      <td align="right"> 
                       <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                       <xsl:text> 
                       </xsl:text> 
                       <xsl:text>
                         TL 
                       </xsl:text> </td> 
                     </tr> 
                    </tbody> 
                   </table> 
                  </xsl:if> 
                 </xsl:when> 
                 <xsl:otherwise> 
                  <xsl:if test="$var_DocumentType = $var_Percent and $var_DocumentTypeCode = $var_TaxTypeCode and not($var_Percent = &apos;0.0000&apos;) "> 
                   <table id="taxSubtotalsTable" border="0" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both" width="100%"> 
                    <tbody> 
                     <tr id="budgetContainerTr" align="right" height="80"> 
                      <td align="right"> 
                       <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                       <xsl:text> 
                       </xsl:text> 
                       <xsl:text>
                         TL 
                       </xsl:text> </td> 
                     </tr> 
                    </tbody> 
                   </table> 
                  </xsl:if> 
                 </xsl:otherwise> 
                </xsl:choose> 
               </xsl:for-each> 
              </xsl:for-each></span></span></span></td> 
         </tr> 
         <xsl:if test="//n1:Invoice/cbc:InvoiceTypeCode = &apos;TEVKIFAT&apos;">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Tevkifata Tabi &#304;&#351;lem Tutar&#305;</span></span></span></span></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
               <xsl:if test="n1:Invoice/cac:InvoiceLine[cac:WithholdingTaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme]"> 
                <xsl:value-of select="format-number(sum(n1:Invoice/cac:InvoiceLine[cac:WithholdingTaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme]/cbc:LineExtensionAmount) * number(//n1:Invoice/cac:PricingExchangeRate/cbc:CalculationRate), &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
               </xsl:if> 
               <xsl:if test="//n1:Invoice/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode=&apos;9015&apos;"> 
                <xsl:value-of select="format-number(sum(n1:Invoice/cac:InvoiceLine[cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode=9015]/cbc:LineExtensionAmount) * number(//n1:Invoice/cac:PricingExchangeRate/cbc:CalculationRate), &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
               </xsl:if> 
               <xsl:text>
                &#160;TL
               </xsl:text></span></span></span></td> 
          </tr>
         </xsl:if> 
         <xsl:if test="//n1:Invoice/cbc:InvoiceTypeCode = &apos;TEVKIFAT&apos;">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Beyan Edilen KDV Tutar&#305;</span></span></span></span></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
               <xsl:variable name="totalKDV"> 
                <xsl:choose> 
                 <xsl:when test="sum(n1:Invoice/cac:TaxTotal/cac:TaxSubtotal[cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode = &apos;0015&apos;]/cbc:TaxAmount) &gt; 0"> 
                  <xsl:value-of select="sum(n1:Invoice/cac:TaxTotal/cac:TaxSubtotal[cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode = &apos;0015&apos;]/cbc:TaxAmount)"></xsl:value-of> 
                 </xsl:when> 
                 <xsl:otherwise>
                  0
                 </xsl:otherwise> 
                </xsl:choose> 
               </xsl:variable> 
               <xsl:variable name="totalTevkifat"> 
                <xsl:choose> 
                 <xsl:when test="n1:Invoice/cac:WithholdingTaxTotal/cbc:TaxAmount &gt; 0"> 
                  <xsl:value-of select="n1:Invoice/cac:WithholdingTaxTotal/cbc:TaxAmount"></xsl:value-of> 
                 </xsl:when> 
                 <xsl:otherwise>
                  0
                 </xsl:otherwise> 
                </xsl:choose> 
               </xsl:variable> 
               <xsl:if test="//n1:Invoice/cbc:InvoiceTypeCode = &apos;TEVKIFAT&apos;"> 
                <div class="toplam_satiri"> 
                 <span class="alt_toplam_val"> 
                  <xsl:value-of select="format-number((number($totalKDV - $totalTevkifat) * number(//n1:Invoice/cac:PricingExchangeRate/cbc:CalculationRate)), &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> TL </span> 
                </div> 
               </xsl:if></span></span></span></td> 
          </tr>
         </xsl:if> 
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Vergiler Dahil Toplam Tutar</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"><span style="color: #696969;"> 
              <xsl:for-each select="//n1:Invoice/cac:AdditionalDocumentReference"> 
               <xsl:if test="cbc:DocumentType = &apos;TAXINCLUSIVEAMOUNT&apos;"> 
                <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
               </xsl:if> 
              </xsl:for-each> 
              <xsl:text> 
              </xsl:text> 
              <xsl:text>
               TL
              </xsl:text></span></span></span></td> 
         </tr> 
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">&#214;denecek Tutar</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"><span style="color: #696969;"> 
              <xsl:for-each select="//n1:Invoice/cac:AdditionalDocumentReference"> 
               <xsl:if test="cbc:DocumentType = &apos;PAYABLEAMOUNT&apos;"> 
                <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
               </xsl:if> 
              </xsl:for-each> 
              <xsl:text> 
              </xsl:text> 
              <xsl:text>
               TL
              </xsl:text></span></span></span></td> 
         </tr> 
        </tbody> 
       </table>
      </xsl:if> </td> 
    </tr> 
   </tbody> 
  </table> 
  <xsl:if test="//n1:Invoice/cac:BillingReference/cac:AdditionalDocumentReference/cbc:DocumentTypeCode=&apos;OKCBF&apos;">
   <table style="border: 2px solid rgb(0, 0, 0); width: 800px; border-collapse: collapse;"> 
    <thead> 
     <tr> 
      <th colspan="6">&#214;KC Bilgileri</th> 
     </tr> 
    </thead> 
    <tbody> 
     <tr> 
      <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="font-size: 9px;"><strong><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;">Fi&#351; Numaras&#305;</span></span></strong></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="font-size: 9px;"><strong><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;">Fi&#351; Tarihi</span></span></strong></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="font-size: 9px;"><strong><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;">Fi&#351; Saati</span></span></strong></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="font-size: 9px;"><strong><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;">Fi&#351; Tipi</span></span></strong></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="font-size: 9px;"><strong><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;">Z Rapor No</span></span></strong></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="font-size: 9px;"><strong><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;">&#214;KC Seri No</span></span></strong></span></td> 
     </tr> 
     <tr loop="loop"> 
      <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="../cbc:ID"></xsl:value-of></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="../cbc:IssueDate" /></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="substring(../cac:ValidityPeriod/cbc:StartTime,1,5)" /></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:choose> 
           <xsl:when test="../cbc:DocumentDescription=&apos;AVANS&apos;"> 
            <xsl:text>
             &#214;n Tahsilat(Avans) Bilgi Fi&#351;i
            </xsl:text> 
           </xsl:when> 
           <xsl:when test="../cbc:DocumentDescription=&apos;YEMEK_FIS&apos;"> 
            <xsl:text>
             Yemek Fi&#351;i/Kart&#305; ile Yap&#305;lan Tahsilat Bilgi Fi&#351;i
            </xsl:text> 
           </xsl:when> 
           <xsl:when test="../cbc:DocumentDescription=&apos;E-FATURA&apos;"> 
            <xsl:text>
             E-Fatura Bilgi Fi&#351;i
            </xsl:text> 
           </xsl:when> 
           <xsl:when test="../cbc:DocumentDescription=&apos;E-FATURA_IRSALIYE&apos;"> 
            <xsl:text>
             &#304;rsaliye Yerine Ge&#231;en E-Fatura Bilgi Fi&#351;i
            </xsl:text> 
           </xsl:when> 
           <xsl:when test="../cbc:DocumentDescription=&apos;E-ARSIV&apos;"> 
            <xsl:text>
             E-Ar&#351;iv Bilgi Fi&#351;i
            </xsl:text> 
           </xsl:when> 
           <xsl:when test="../cbc:DocumentDescription=&apos;E-ARSIV_IRSALIYE&apos;"> 
            <xsl:text>
             &#304;rsaliye Yerine Ge&#231;en E-Ar&#351;iv Bilgi Fi&#351;i
            </xsl:text> 
           </xsl:when> 
           <xsl:when test="../cbc:DocumentDescription=&apos;FATURA&apos;"> 
            <xsl:text>
             Fatural&#305; Sat&#305;&#351; Bilgi Fi&#351;i
            </xsl:text> 
           </xsl:when> 
           <xsl:when test="../cbc:DocumentDescription=&apos;OTOPARK&apos;"> 
            <xsl:text>
             Otopark Giri&#351; Bilgi Fi&#351;i
            </xsl:text> 
           </xsl:when> 
           <xsl:when test="../cbc:DocumentDescription=&apos;FATURA_TAHSILAT&apos;"> 
            <xsl:text>
             Fatura Tahsilat Bilgi Fi&#351;i
            </xsl:text> 
           </xsl:when> 
           <xsl:when test="../cbc:DocumentDescription=&apos;FATURA_TAHSILAT_KOMISYONLU&apos;"> 
            <xsl:text>
             Komisyonlu Fatura Tahsilat Bilgi Fi&#351;i
            </xsl:text> 
           </xsl:when> 
           <xsl:otherwise> 
            <xsl:text></xsl:text> 
           </xsl:otherwise> 
          </xsl:choose></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="../cac:Attachment/cac:ExternalReference/cbc:URI" /></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); text-align: center;"><span style="color: #696969;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="../cac:IssuerParty/cbc:EndpointID" /></span></span></span></td> 
     </tr> 
    </tbody> 
   </table>
  </xsl:if> 
  <table border="1" cellpadding="0" cellspacing="0" style="width: 800px; margin-top: 20px;"> 
   <tbody> 
    <tr> 
     <td align="left"> 
      <xsl:if test="//n1:Invoice/cac:PaymentTerms/cbc:PaymentDueDate != &apos;&apos;">
       <div style="margin-left: 40px;">
        <span style="color: #000000;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Vade Tarihi:</span></span></span></span> 
        <span style="color: #000000;"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> <span loop="loop" style="font-size: 10px;"><span style="color: #696969;"><span style="font-family: verdana,geneva,sans-serif;"> 
              <xsl:for-each select="//n1:Invoice/cac:PaymentTerms"> 
               <xsl:variable name="ddt" select="cbc:PaymentDueDate" /> 
               <xsl:value-of select="concat(substring($ddt, 9, 2), &apos;-&apos;, substring($ddt, 6, 2), &apos;-&apos;, substring($ddt, 1, 4))" /> 
               <br /> 
              </xsl:for-each>&#8203;</span></span></span> </span></span></span>
       </div>
      </xsl:if> <span style="font-size: 11px; margin-left: 40px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"> 
         <xsl:if test="//n1:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:ID"> 
          <b>&#304;ban Numaras&#305;:</b> 
          <xsl:value-of select="//n1:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:ID"></xsl:value-of> 
          <br /> 
         </xsl:if> 
         <xsl:if test="//n1:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:CurrencyCode"> 
          <b>D&#246;viz Cinsi:</b> 
          <xsl:value-of select="//n1:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:CurrencyCode"></xsl:value-of> 
          <br /> 
         </xsl:if> 
         <xsl:if test="//n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID and //n1:Invoice/cbc:ProfileID = &apos;KAMU&apos;"> 
          <b>&#214;deme Yapacak Harcama Birimi VKN:</b> 
          <xsl:value-of select="//n1:Invoice/cac:BuyerCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID"></xsl:value-of> 
          <br /> 
         </xsl:if> 
         <xsl:if test="//n1:Invoice/cac:PaymentMeans/cbc:InstructionNote"> 
          <b>&#214;deme Notu:</b> 
          <xsl:value-of select="//n1:Invoice/cac:PaymentMeans/cbc:InstructionNote"></xsl:value-of> 
          <br /> 
         </xsl:if> 
         <xsl:if test="//n1:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:PaymentNote"> 
          <b>Hesap A&#231;&#305;klamas&#305;:</b> 
          <xsl:value-of select="//n1:Invoice/cac:PaymentMeans/cac:PayeeFinancialAccount/cbc:PaymentNote"></xsl:value-of> 
          <br /> 
         </xsl:if> 
         <xsl:if test="//n1:Invoice/cac:PaymentTerms/cbc:Note"> 
          <b>&#214;deme Ko&#351;ulu:</b> 
          <xsl:value-of select="//n1:Invoice/cac:PaymentTerms/cbc:Note"></xsl:value-of> 
          <br /> 
         </xsl:if> 
         <xsl:for-each select="//n1:Invoice/cac:TaxTotal/cac:TaxSubtotal"> 
          <xsl:if test="normalize-space(cac:TaxCategory/cbc:TaxExemptionReason)"> 
           <b>Vergi muafiyet sebebi:</b> 
           <xsl:value-of select="concat(cac:TaxCategory/cbc:TaxExemptionReasonCode,&apos;-&apos;,cac:TaxCategory/cbc:TaxExemptionReason)" /> 
           <br /> 
          </xsl:if> 
         </xsl:for-each> <br />  <br /> <br /> 
         <xsl:if test="//n1:Invoice/cbc:Note"> 
          <b>Genel A&#231;&#305;klamalar</b> 
          <br /> 
          <xsl:for-each select="//n1:Invoice/cbc:Note"> 
           <xsl:value-of select="."></xsl:value-of> 
           <br /> 
          </xsl:for-each> 
         </xsl:if> 
         <xsl:if test="(//n1:Invoice/cbc:ProfileID = &apos;EARSIVFATURA&apos;)"> 
          <br /> 
          <b><span style="color:red;"> 
            <xsl:text>
             e-Ar&#351;iv faturalar&#305;n&#305;z&#305; elektronik ortamda g&#246;r&#252;nt&#252;lemek ve indirilebilmek i&#231;in &quot;
            </xsl:text> <a target="_blank" href="https://ebelgesorgulama.elogo.com.tr/">
             <xsl:text>
              https://ebelgesorgulama.elogo.com.tr/
             </xsl:text></a> 
            <xsl:text>
             &quot; platformunu kullanabilirsiniz.
            </xsl:text> </span></b> 
         </xsl:if></span></span></span></td> 
    </tr> 
   </tbody> 
  </table> 
  <table border="1" cellpadding="1" cellspacing="1" style="width: 800px;"> 
   <tbody> 
    <tr> 
     <td style="background-color: #000000; width: 236px;"><span style="color: #ffffff;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">Banka Ad&#305;</span></span></strong></span></td> 
     <td style="background-color: #000000; width: 139px;"><span style="color: #ffffff;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">Para Birimi</span></span></strong></span></td> 
     <td style="background-color: #000000;"><span style="color: #ffffff;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;">Iban</span></span></strong></span></td> 
    </tr> 
    <tr> 
     <td style="width: 236px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">AKBANK</span></span></span></strong></td> 
     <td style="width: 139px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TL</span></span></span></strong></td> 
     <td><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TR46 0004 6006 2688 8000 2108 03</span></span></span></strong></td> 
    </tr> 
    <tr> 
     <td style="width: 236px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">KUVEYTTURK</span></span></span></strong></td> 
     <td style="width: 139px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TL</span></span></span></strong></td> 
     <td><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TR04 0020 5000 0993 8161 8000 01</span></span></span></strong></td> 
    </tr> 
    <tr> 
     <td style="width: 236px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">Z&#304;RAAT BANKASI</span></span></span></strong></td> 
     <td style="width: 139px;"><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TL</span></span></span></strong></td> 
     <td><strong><span style="font-size: 10px;"><span style="font-family: verdana, geneva, sans-serif;"><span style="color: #000000;">TR40 0001 0023 4090 3610 7950 01</span></span></span></strong></td> 
    </tr> 
   </tbody> 
  </table> 
  <xsl:if test="//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;EINVOICE&apos;]/cbc:DocumentType=&apos;3&apos;">
   <table border="1" cellpadding="0" cellspacing="0" style="width: 800px; margin-top: 4px;"> 
    <tbody> 
     <tr> 
      <td> <p><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><strong>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#304;nternet Sat&#305;&#351; Bilgileri:</strong> </span> </span> </span><br /><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;">&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;Bu sat&#305;&#351; internet &#252;zerinden yap&#305;lm&#305;&#351;t&#305;r.</span> </span> </span><br /><br />
        <xsl:if test="normalize-space(//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/webAdresi&apos;]/cbc:DocumentType)">
         <span style="margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><strong>Sat&#305;&#351; &#304;&#351;leminin Yap&#305;ld&#305;&#287;&#305; Web Adresi: </strong>  
             <xsl:value-of select="//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/webAdresi&apos;]/cbc:DocumentType" /></span></span></span></span>
        </xsl:if><br />
        <xsl:if test="normalize-space(//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/odemeAracisiAdi&apos;]/cbc:DocumentType)">
         <span style="margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><strong>&#214;deme Arac&#305;s&#305;: </strong>  
             <xsl:value-of select="//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/odemeAracisiAdi&apos;]/cbc:DocumentType" /></span></span></span></span>
        </xsl:if><br />
        <xsl:if test="normalize-space(//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/odemeSekli&apos;]/cbc:DocumentType)">
         <span style="line-em; margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><strong>&#214;deme &#351;ekli: </strong>  
             <xsl:value-of select="//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/odemeSekli&apos;]/cbc:DocumentType" /></span></span></span></span>
        </xsl:if><br />
        <xsl:if test="normalize-space(//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/odemeTarihi&apos;]/cbc:DocumentType)">
         <span style="line-em; margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><strong>&#214;deme Tarihi: </strong>  
             <xsl:value-of select="//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/odemeTarihi&apos;]/cbc:DocumentType" /></span></span></span></span>
        </xsl:if><br />
        <xsl:if test="normalize-space(//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/gonderiBilgileri/gonderimTarihi&apos;]/cbc:DocumentType)">
         <span style="line-em; margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><strong>G&#214;nderim Tarihi: </strong>  
             <xsl:value-of select="//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/gonderiBilgileri/gonderimTarihi&apos;]/cbc:DocumentType" /></span></span></span></span>
        </xsl:if><br />
        <xsl:if test="normalize-space(//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/gonderiBilgileri/gonderiTasiyan/gercekKisi/tckn&apos;]/cbc:DocumentType)">
         <span style="line-em; margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><strong>G&#214;nderiyi ta&#351;&#305;yan TCKN: </strong>  
             <xsl:value-of select="//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/gonderiBilgileri/gonderiTasiyan/gercekKisi/tckn&apos;]/cbc:DocumentType" /></span></span></span></span>
        </xsl:if><br />
        <xsl:if test="normalize-space(//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/gonderiBilgileri/gonderiTasiyan/tuzelKisi/vkn&apos;]/cbc:DocumentType)">
         <span style="line-em; margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><strong>G&#214;nderiyi ta&#351;&#305;yan VKN: </strong>  
             <xsl:value-of select="//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/gonderiBilgileri/gonderiTasiyan/tuzelKisi/vkn&apos;]/cbc:DocumentType" /></span></span></span></span>
        </xsl:if><br />
        <xsl:if test="normalize-space(//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/gonderiBilgileri/gonderiTasiyan/gercekKisi/adiSoyadi&apos;]/cbc:DocumentType)">
         <span style="line-em; margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><strong>G&#214;nderiyi ta&#351;&#305;yan ad&#305; soyad&#305;: </strong>  
             <xsl:value-of select="//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/gonderiBilgileri/gonderiTasiyan/gercekKisi/adiSoyadi&apos;]/cbc:DocumentType" /></span></span></span></span>
        </xsl:if><br />
        <xsl:if test="normalize-space(//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/gonderiBilgileri/gonderiTasiyan/tuzelKisi/unvan&apos;]/cbc:DocumentType)">
         <span style="margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><strong>G&#214;nderiyi ta&#351;&#305;yan unvan&#305;: </strong>  
             <xsl:value-of select="//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID = &apos;internetSatisBilgi/gonderiBilgileri/gonderiTasiyan/tuzelKisi/unvan&apos;]/cbc:DocumentType" /></span></span></span></span>
        </xsl:if></p> 
       <xsl:if test="//n1:Invoice/cac:AdditionalDocumentReference/cbc:ID =&apos;EINVOICE&apos; and //n1:Invoice/cac:AdditionalDocumentReference/cbc:DocumentType =&apos;3&apos;">
        <table border="0" cellpadding="0" cellspacing="0" style="margin-top: 30px;"> 
         <tbody> 
          <tr> 
           <td><span style="margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><strong>&#160;&#160;&#160;&#160;&#160;&#304;ade B&#214;l&#252;m&#252;</strong> </span> </span> </span> </span> 
            <table border="1" bordercolor="#a4a4a4" cellpadding="0" cellspacing="0" style="margin-left: 40px;"> 
             <tbody> 
              <tr id="budgetContainerTr"> 
               <td id="lineTableBudgetTd" style="width: 290px;"><span style="margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><span style="font-weight: bold;">Mal&#305;n Cinsi</span> </span> </span> </span></span></td> 
               <td id="lineTableBudgetTd" style="width: 100px;"><span style="margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><span style="font-weight: bold;">Mal&#305;n Miktar&#305;</span> </span> </span> </span></span></td> 
               <td id="lineTableBudgetTd" style="width: 150px;"><span style="margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><span style="font-weight: bold;">Birim Fiyat&#305;</span> </span> </span> </span></span></td> 
               <td id="lineTableBudgetTd" style="width: 150px;"><span style="margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><span style="font-weight: bold;">Tutar</span> </span> </span> </span></span></td> 
              </tr> 
              <tr id="budgetContainerTr"> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
              </tr> 
              <tr id="budgetContainerTr"> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
              </tr> 
              <tr id="budgetContainerTr"> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
              </tr> 
              <tr id="budgetContainerTr"> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
               <td id="lineTableBudgetTd"><span style="margin-left: 40px;">&#160;</span></td> 
              </tr> 
             </tbody> 
            </table> <br /><br /><span style="margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><span style="font-weight: bold;">&#160;&#160;&#160;&#160;&#160;Mal&#305; &#304;ade Edenin</span> </span> </span> </span><br /><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><span style="font-weight: bold;">&#160;&#160;&#160;&#160;&#160;Ad&#305; Soyad&#305;</span> </span> </span> </span><br /><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><span style="font-weight: bold;">&#160;&#160;&#160;&#160;&#160;Adresi</span> </span> </span> </span><br /><br /><br /><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;"><span style="font-weight: bold;">&#160;&#160;&#160;&#160;&#160;&#304;mzas&#305;</span> </span> </span> </span><br /><br /><br />&#160; </span> <span style="margin-left: 40px;"> </span></td> 
          </tr> 
         </tbody> 
        </table>
       </xsl:if> </td> 
     </tr> 
    </tbody> 
   </table>
  </xsl:if> 
  <table border="0" cellpadding="0" cellspacing="0" style="width: 800px;"> 
   <tbody> 
    <tr> 
     <td> 
      <xsl:if test="//n1:Invoice/cac:AdditionalDocumentReference[cbc:ID =&apos;gonderimSekli&apos;]/cbc:DocumentType =&apos;ELEKTRONIK&apos;">
       <div style="text-align: center;">
        <span style="margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;">e-Ar&#351;iv Fatura izni kapsam&#305;nda elektronik ortamda iletilmi&#351;tir.</span> </span> </span></span>
       </div>
      </xsl:if> 
      <xsl:if test="not(//n1:Invoice/cac:DespatchDocumentReference) and //n1:Invoice/cbc:ProfileID !=&apos;IHRACAT&apos;">
       <div style="text-align: center;">
        <span style="margin-left: 40px;"><span style="font-size: 11px;"><span style="color: #696969;"><span style="font-family: tahoma,geneva,sans-serif;">&#304;rsaliye yerine ge&#231;er.</span> </span> </span></span>
       </div>
      </xsl:if> </td> 
    </tr> 
   </tbody> 
  </table>
 </body>
</html>  </xsl:template>
	<xsl:template name="TransportMode">
		<xsl:param name="TransportModeType"/>
		<xsl:choose>
			<xsl:when test="$TransportModeType=1">Denizyolu</xsl:when>
			<xsl:when test="$TransportModeType=2">Demiryolu</xsl:when>
			<xsl:when test="$TransportModeType=3">Karayolu</xsl:when>
			<xsl:when test="$TransportModeType=4">Havayolu</xsl:when>
			<xsl:when test="$TransportModeType=5">Posta</xsl:when>
			<xsl:when test="$TransportModeType=6">Çok araçlı</xsl:when>
			<xsl:when test="$TransportModeType=7">Sabit taşıma tesisleri</xsl:when>
			<xsl:when test="$TransportModeType=8">İç su taşımacılığı</xsl:when>
		<xsl:otherwise>
			<xsl:value-of select="$TransportModeType"/>
		</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
  <xsl:template name="geneliskonto"> 
          <tr id="lineTableTr" >
            <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;"><span><xsl:text>&#160;</xsl:text></span></td>
            <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;" align="right"><span><xsl:text>&#160;</xsl:text></span></td>
            <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;" align="left">
              <span>
                <table border="0">
                  <tbody>
                    <xsl:for-each select="//n1:Invoice/cac:AllowanceCharge">
                      <tr>
                        <td><xsl:text>&#160;&#160;&#160;&#160;&#160;</xsl:text><xsl:text>İndirim</xsl:text></td>
                      </tr>
                    </xsl:for-each>
                  </tbody>
                </table>
              </span>
            </td>
            <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;" align="right"><span><xsl:text>&#160;</xsl:text></span></td>
            <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;" align="right"><span><xsl:text>&#160;</xsl:text></span></td>
            <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;" align="right"><span><xsl:text>&#160;</xsl:text></span></td>
            <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;" align="right">
              <span>
                <table border="0">
                  <tbody>
                    <xsl:for-each select="//n1:Invoice/cac:AllowanceCharge">
                      <tr>
                        <td>
                          <xsl:choose>
                            <xsl:when test="normalize-space(cbc:AllowanceChargeReason)"><xsl:value-of select="cbc:AllowanceChargeReason"/></xsl:when>
                            <xsl:otherwise><xsl:if test ="normalize-space(cbc:MultiplierFactorNumeric)"><xsl:text>%</xsl:text><xsl:value-of select="format-number(cbc:MultiplierFactorNumeric*100, '###.##0,00', 'european')"/></xsl:if></xsl:otherwise>
                          </xsl:choose>
                        </td>
                      </tr>
                    </xsl:for-each>
                  </tbody>
                </table>
              </span>
            </td>
            <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;" align="right">
              <span>
                <table border="0">
                  <tbody>
                    <xsl:for-each select="//n1:Invoice/cac:AllowanceCharge">
                      <tr>
                        <td>
                          <xsl:value-of select="format-number(cbc:Amount, '###.##0,00', 'european')"/>
                          <xsl:if test="cbc:Amount/@currencyID">
                            <xsl:text> </xsl:text>
                            <xsl:if test="cbc:Amount/@currencyID = 'TRY'"><xsl:text>TL</xsl:text></xsl:if>
                            <xsl:if test="cbc:Amount/@currencyID != 'TRY'"><xsl:value-of select="cbc:Amount/@currencyID"/></xsl:if>
                          </xsl:if>                   
                        </td>
                      </tr>
                    </xsl:for-each>
                  </tbody>
                </table>
              </span>
            </td> 
            <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;" align="right"><span><xsl:text>&#160;</xsl:text></span></td>
            <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;" align="right"><span><xsl:text>&#160;</xsl:text></span></td>      
	          <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;" align="right"><span><xsl:text>&#160;</xsl:text></span></td>
            <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;" align="right"><span><xsl:text>&#160;</xsl:text></span></td>
            <td id="lineTableTd" style="border-top:thick double black; border-top-width:3px;" align="right"><span><xsl:text>&#160;</xsl:text></span></td>
          </tr>
  </xsl:template>
  	<xsl:template name="Country">
		<xsl:param name="CountryType" />
		<xsl:choose>
			<xsl:when test="$CountryType='AF'">Afganistan</xsl:when>
			<xsl:when test="$CountryType='DE'">Almanya</xsl:when>
			<xsl:when test="$CountryType='AD'">Andorra</xsl:when>
			<xsl:when test="$CountryType='AO'">Angola</xsl:when>
			<xsl:when test="$CountryType='AG'">Antigua ve Barbuda</xsl:when>
			<xsl:when test="$CountryType='AR'">Arjantin</xsl:when>
			<xsl:when test="$CountryType='AL'">Arnavutluk</xsl:when>
			<xsl:when test="$CountryType='AW'">Aruba</xsl:when>
			<xsl:when test="$CountryType='AU'">Avustralya</xsl:when>
			<xsl:when test="$CountryType='AT'">Avusturya</xsl:when>
			<xsl:when test="$CountryType='AZ'">Azerbaycan</xsl:when>
			<xsl:when test="$CountryType='BS'">Bahamalar</xsl:when>
			<xsl:when test="$CountryType='BH'">Bahreyn</xsl:when>
			<xsl:when test="$CountryType='BD'">Bangladeş</xsl:when>
			<xsl:when test="$CountryType='BB'">Barbados</xsl:when>
			<xsl:when test="$CountryType='EH'">Batı Sahra (MA)</xsl:when>
			<xsl:when test="$CountryType='BE'">Belçika</xsl:when>
			<xsl:when test="$CountryType='BZ'">Belize</xsl:when>
			<xsl:when test="$CountryType='BJ'">Benin</xsl:when>
			<xsl:when test="$CountryType='BM'">Bermuda</xsl:when>
			<xsl:when test="$CountryType='BY'">Beyaz Rusya</xsl:when>
			<xsl:when test="$CountryType='BT'">Bhutan</xsl:when>
			<xsl:when test="$CountryType='AE'">Birleşik Arap Emirlikleri</xsl:when>
			<xsl:when test="$CountryType='US'">Birleşik Devletler</xsl:when>
			<xsl:when test="$CountryType='GB'">Birleşik Krallık</xsl:when>
			<xsl:when test="$CountryType='BO'">Bolivya</xsl:when>
			<xsl:when test="$CountryType='BA'">Bosna-Hersek</xsl:when>
			<xsl:when test="$CountryType='BW'">Botsvana</xsl:when>
			<xsl:when test="$CountryType='BR'">Brezilya</xsl:when>
			<xsl:when test="$CountryType='BN'">Bruney</xsl:when>
			<xsl:when test="$CountryType='BG'">Bulgaristan</xsl:when>
			<xsl:when test="$CountryType='BF'">Burkina Faso</xsl:when>
			<xsl:when test="$CountryType='BI'">Burundi</xsl:when>
			<xsl:when test="$CountryType='TD'">Çad</xsl:when>
			<xsl:when test="$CountryType='KY'">Cayman Adaları</xsl:when>
			<xsl:when test="$CountryType='GI'">Cebelitarık (GB)</xsl:when>
			<xsl:when test="$CountryType='CZ'">Çek Cumhuriyeti</xsl:when>
			<xsl:when test="$CountryType='DZ'">Cezayir</xsl:when>
			<xsl:when test="$CountryType='DJ'">Cibuti</xsl:when>
			<xsl:when test="$CountryType='CN'">Çin</xsl:when>
			<xsl:when test="$CountryType='DK'">Danimarka</xsl:when>
			<xsl:when test="$CountryType='CD'">Demokratik Kongo Cumhuriyeti</xsl:when>
			<xsl:when test="$CountryType='TL'">Doğu Timor</xsl:when>
			<xsl:when test="$CountryType='DO'">Dominik Cumhuriyeti</xsl:when>
			<xsl:when test="$CountryType='DM'">Dominika</xsl:when>
			<xsl:when test="$CountryType='EC'">Ekvador</xsl:when>
			<xsl:when test="$CountryType='GQ'">Ekvator Ginesi</xsl:when>
			<xsl:when test="$CountryType='SV'">El Salvador</xsl:when>
			<xsl:when test="$CountryType='ID'">Endonezya</xsl:when>
			<xsl:when test="$CountryType='ER'">Eritre</xsl:when>
			<xsl:when test="$CountryType='AM'">Ermenistan</xsl:when>
			<xsl:when test="$CountryType='MF'">Ermiş Martin (FR)</xsl:when>
			<xsl:when test="$CountryType='EE'">Estonya</xsl:when>
			<xsl:when test="$CountryType='ET'">Etiyopya</xsl:when>
			<xsl:when test="$CountryType='FK'">Falkland Adaları</xsl:when>
			<xsl:when test="$CountryType='FO'">Faroe Adaları (DK)</xsl:when>
			<xsl:when test="$CountryType='MA'">Fas</xsl:when>
			<xsl:when test="$CountryType='FJ'">Fiji</xsl:when>
			<xsl:when test="$CountryType='CI'">Fildişi Sahili</xsl:when>
			<xsl:when test="$CountryType='PH'">Filipinler</xsl:when>
			<xsl:when test="$CountryType='FI'">Finlandiya</xsl:when>
			<xsl:when test="$CountryType='FR'">Fransa</xsl:when>
			<xsl:when test="$CountryType='GF'">Fransız Guyanası (FR)</xsl:when>
			<xsl:when test="$CountryType='PF'">Fransız Polinezyası (FR)</xsl:when>
			<xsl:when test="$CountryType='GA'">Gabon</xsl:when>
			<xsl:when test="$CountryType='GM'">Gambiya</xsl:when>
			<xsl:when test="$CountryType='GH'">Gana</xsl:when>
			<xsl:when test="$CountryType='GN'">Gine</xsl:when>
			<xsl:when test="$CountryType='GW'">Gine Bissau</xsl:when>
			<xsl:when test="$CountryType='GD'">Grenada</xsl:when>
			<xsl:when test="$CountryType='GL'">Grönland (DK)</xsl:when>
			<xsl:when test="$CountryType='GP'">Guadeloupe (FR)</xsl:when>
			<xsl:when test="$CountryType='GT'">Guatemala</xsl:when>
			<xsl:when test="$CountryType='GG'">Guernsey (GB)</xsl:when>
			<xsl:when test="$CountryType='ZA'">Güney Afrika</xsl:when>
			<xsl:when test="$CountryType='KR'">Güney Kore</xsl:when>
			<xsl:when test="$CountryType='GE'">Gürcistan</xsl:when>
			<xsl:when test="$CountryType='GY'">Guyana</xsl:when>
			<xsl:when test="$CountryType='HT'">Haiti</xsl:when>
			<xsl:when test="$CountryType='IN'">Hindistan</xsl:when>
			<xsl:when test="$CountryType='HR'">Hırvatistan</xsl:when>
			<xsl:when test="$CountryType='NL'">Hollanda</xsl:when>
			<xsl:when test="$CountryType='HN'">Honduras</xsl:when>
			<xsl:when test="$CountryType='HK'">Hong Kong (CN)</xsl:when>
			<xsl:when test="$CountryType='VG'">İngiliz Virjin Adaları</xsl:when>
			<xsl:when test="$CountryType='IQ'">Irak</xsl:when>
			<xsl:when test="$CountryType='IR'">İran</xsl:when>
			<xsl:when test="$CountryType='IE'">İrlanda</xsl:when>
			<xsl:when test="$CountryType='ES'">İspanya</xsl:when>
			<xsl:when test="$CountryType='IL'">İsrail</xsl:when>
			<xsl:when test="$CountryType='SE'">İsveç</xsl:when>
			<xsl:when test="$CountryType='CH'">İsviçre</xsl:when>
			<xsl:when test="$CountryType='IT'">İtalya</xsl:when>
			<xsl:when test="$CountryType='IS'">İzlanda</xsl:when>
			<xsl:when test="$CountryType='JM'">Jamaika</xsl:when>
			<xsl:when test="$CountryType='JP'">Japonya</xsl:when>
			<xsl:when test="$CountryType='JE'">Jersey (GB)</xsl:when>
			<xsl:when test="$CountryType='KH'">Kamboçya</xsl:when>
			<xsl:when test="$CountryType='CM'">Kamerun</xsl:when>
			<xsl:when test="$CountryType='CA'">Kanada</xsl:when>
			<xsl:when test="$CountryType='ME'">Karadağ</xsl:when>
			<xsl:when test="$CountryType='QA'">Katar</xsl:when>
			<xsl:when test="$CountryType='KZ'">Kazakistan</xsl:when>
			<xsl:when test="$CountryType='KE'">Kenya</xsl:when>
			<xsl:when test="$CountryType='CY'">Kıbrıs</xsl:when>
			<xsl:when test="$CountryType='KG'">Kırgızistan</xsl:when>
			<xsl:when test="$CountryType='KI'">Kiribati</xsl:when>
			<xsl:when test="$CountryType='CO'">Kolombiya</xsl:when>
			<xsl:when test="$CountryType='KM'">Komorlar</xsl:when>
			<xsl:when test="$CountryType='CG'">Kongo Cumhuriyeti</xsl:when>
			<xsl:when test="$CountryType='KV'">Kosova (RS)</xsl:when>
			<xsl:when test="$CountryType='CR'">Kosta Rika</xsl:when>
			<xsl:when test="$CountryType='CU'">Küba</xsl:when>
			<xsl:when test="$CountryType='KW'">Kuveyt</xsl:when>
			<xsl:when test="$CountryType='KP'">Kuzey Kore</xsl:when>
			<xsl:when test="$CountryType='LA'">Laos</xsl:when>
			<xsl:when test="$CountryType='LS'">Lesoto</xsl:when>
			<xsl:when test="$CountryType='LV'">Letonya</xsl:when>
			<xsl:when test="$CountryType='LR'">Liberya</xsl:when>
			<xsl:when test="$CountryType='LY'">Libya</xsl:when>
			<xsl:when test="$CountryType='LI'">Lihtenştayn</xsl:when>
			<xsl:when test="$CountryType='LT'">Litvanya</xsl:when>
			<xsl:when test="$CountryType='LB'">Lübnan</xsl:when>
			<xsl:when test="$CountryType='LU'">Lüksemburg</xsl:when>
			<xsl:when test="$CountryType='HU'">Macaristan</xsl:when>
			<xsl:when test="$CountryType='MG'">Madagaskar</xsl:when>
			<xsl:when test="$CountryType='MO'">Makao (CN)</xsl:when>
			<xsl:when test="$CountryType='MK'">Makedonya</xsl:when>
			<xsl:when test="$CountryType='MW'">Malavi</xsl:when>
			<xsl:when test="$CountryType='MV'">Maldivler</xsl:when>
			<xsl:when test="$CountryType='MY'">Malezya</xsl:when>
			<xsl:when test="$CountryType='ML'">Mali</xsl:when>
			<xsl:when test="$CountryType='MT'">Malta</xsl:when>
			<xsl:when test="$CountryType='IM'">Man Adası (GB)</xsl:when>
			<xsl:when test="$CountryType='MH'">Marshall Adaları</xsl:when>
			<xsl:when test="$CountryType='MQ'">Martinique (FR)</xsl:when>
			<xsl:when test="$CountryType='MU'">Mauritius</xsl:when>
			<xsl:when test="$CountryType='YT'">Mayotte (FR)</xsl:when>
			<xsl:when test="$CountryType='MX'">Meksika</xsl:when>
			<xsl:when test="$CountryType='FM'">Mikronezya</xsl:when>
			<xsl:when test="$CountryType='EG'">Mısır</xsl:when>
			<xsl:when test="$CountryType='MN'">Moğolistan</xsl:when>
			<xsl:when test="$CountryType='MD'">Moldova</xsl:when>
			<xsl:when test="$CountryType='MC'">Monako</xsl:when>
			<xsl:when test="$CountryType='MR'">Moritanya</xsl:when>
			<xsl:when test="$CountryType='MZ'">Mozambik</xsl:when>
			<xsl:when test="$CountryType='MM'">Myanmar</xsl:when>
			<xsl:when test="$CountryType='NA'">Namibya</xsl:when>
			<xsl:when test="$CountryType='NR'">Nauru</xsl:when>
			<xsl:when test="$CountryType='NP'">Nepal</xsl:when>
			<xsl:when test="$CountryType='NE'">Nijer</xsl:when>
			<xsl:when test="$CountryType='NG'">Nijerya</xsl:when>
			<xsl:when test="$CountryType='NI'">Nikaragua</xsl:when>
			<xsl:when test="$CountryType='NO'">Norveç</xsl:when>
			<xsl:when test="$CountryType='CF'">Orta Afrika Cumhuriyeti</xsl:when>
			<xsl:when test="$CountryType='UZ'">Özbekistan</xsl:when>
			<xsl:when test="$CountryType='PK'">Pakistan</xsl:when>
			<xsl:when test="$CountryType='PW'">Palau</xsl:when>
			<xsl:when test="$CountryType='PA'">Panama</xsl:when>
			<xsl:when test="$CountryType='PG'">Papua Yeni Gine</xsl:when>
			<xsl:when test="$CountryType='PY'">Paraguay</xsl:when>
			<xsl:when test="$CountryType='PE'">Peru</xsl:when>
			<xsl:when test="$CountryType='PL'">Polonya</xsl:when>
			<xsl:when test="$CountryType='PT'">Portekiz</xsl:when>
			<xsl:when test="$CountryType='PR'">Porto Riko (US)</xsl:when>
			<xsl:when test="$CountryType='RE'">Réunion (FR)</xsl:when>
			<xsl:when test="$CountryType='RO'">Romanya</xsl:when>
			<xsl:when test="$CountryType='RW'">Ruanda</xsl:when>
			<xsl:when test="$CountryType='RU'">Rusya</xsl:when>
			<xsl:when test="$CountryType='BL'">Saint Barthélemy (FR)</xsl:when>
			<xsl:when test="$CountryType='KN'">Saint Kitts ve Nevis</xsl:when>
			<xsl:when test="$CountryType='LC'">Saint Lucia</xsl:when>
			<xsl:when test="$CountryType='PM'">Saint Pierre ve Miquelon (FR)</xsl:when>
			<xsl:when test="$CountryType='VC'">Saint Vincent ve Grenadinler</xsl:when>
			<xsl:when test="$CountryType='WS'">Samoa</xsl:when>
			<xsl:when test="$CountryType='SM'">San Marino</xsl:when>
			<xsl:when test="$CountryType='ST'">São Tomé ve Príncipe</xsl:when>
			<xsl:when test="$CountryType='SN'">Senegal</xsl:when>
			<xsl:when test="$CountryType='SC'">Seyşeller</xsl:when>
			<xsl:when test="$CountryType='SL'">Sierra Leone</xsl:when>
			<xsl:when test="$CountryType='CL'">Şili</xsl:when>
			<xsl:when test="$CountryType='SG'">Singapur</xsl:when>
			<xsl:when test="$CountryType='RS'">Sırbistan</xsl:when>
			<xsl:when test="$CountryType='SK'">Slovakya Cumhuriyeti</xsl:when>
			<xsl:when test="$CountryType='SI'">Slovenya</xsl:when>
			<xsl:when test="$CountryType='SB'">Solomon Adaları</xsl:when>
			<xsl:when test="$CountryType='SO'">Somali</xsl:when>
			<xsl:when test="$CountryType='SS'">South Sudan</xsl:when>
			<xsl:when test="$CountryType='SJ'">Spitsbergen (NO)</xsl:when>
			<xsl:when test="$CountryType='LK'">Sri Lanka</xsl:when>
			<xsl:when test="$CountryType='SD'">Sudan</xsl:when>
			<xsl:when test="$CountryType='SR'">Surinam</xsl:when>
			<xsl:when test="$CountryType='SY'">Suriye</xsl:when>
			<xsl:when test="$CountryType='SA'">Suudi Arabistan</xsl:when>
			<xsl:when test="$CountryType='SZ'">Svaziland</xsl:when>
			<xsl:when test="$CountryType='TJ'">Tacikistan</xsl:when>
			<xsl:when test="$CountryType='TZ'">Tanzanya</xsl:when>
			<xsl:when test="$CountryType='TH'">Tayland</xsl:when>
			<xsl:when test="$CountryType='TW'">Tayvan</xsl:when>
			<xsl:when test="$CountryType='TG'">Togo</xsl:when>
			<xsl:when test="$CountryType='TO'">Tonga</xsl:when>
			<xsl:when test="$CountryType='TT'">Trinidad ve Tobago</xsl:when>
			<xsl:when test="$CountryType='TN'">Tunus</xsl:when>
			<xsl:when test="$CountryType='TR'">Türkiye</xsl:when>
			<xsl:when test="$CountryType='TM'">Türkmenistan</xsl:when>
			<xsl:when test="$CountryType='TC'">Turks ve Caicos</xsl:when>
			<xsl:when test="$CountryType='TV'">Tuvalu</xsl:when>
			<xsl:when test="$CountryType='UG'">Uganda</xsl:when>
			<xsl:when test="$CountryType='UA'">Ukrayna</xsl:when>
			<xsl:when test="$CountryType='OM'">Umman</xsl:when>
			<xsl:when test="$CountryType='JO'">Ürdün</xsl:when>
			<xsl:when test="$CountryType='UY'">Uruguay</xsl:when>
			<xsl:when test="$CountryType='VU'">Vanuatu</xsl:when>
			<xsl:when test="$CountryType='VA'">Vatikan</xsl:when>
			<xsl:when test="$CountryType='VE'">Venezuela</xsl:when>
			<xsl:when test="$CountryType='VN'">Vietnam</xsl:when>
			<xsl:when test="$CountryType='WF'">Wallis ve Futuna (FR)</xsl:when>
			<xsl:when test="$CountryType='YE'">Yemen</xsl:when>
			<xsl:when test="$CountryType='NC'">Yeni Kaledonya (FR)</xsl:when>
			<xsl:when test="$CountryType='NZ'">Yeni Zelanda</xsl:when>
			<xsl:when test="$CountryType='CV'">Yeşil Burun Adaları</xsl:when>
			<xsl:when test="$CountryType='GR'">Yunanistan</xsl:when>
			<xsl:when test="$CountryType='ZM'">Zambiya</xsl:when>
			<xsl:when test="$CountryType='ZW'">Zimbabve</xsl:when>
			<xsl:otherwise><xsl:value-of select="$CountryType"/></xsl:otherwise>
		</xsl:choose>
		
	</xsl:template>

</xsl:stylesheet>
