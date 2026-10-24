<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:cac="urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"
	xmlns:cbc="urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"
	xmlns:ccts="urn:un:unece:uncefact:documentation:2"
	xmlns:clm54217="urn:un:unece:uncefact:codelist:specification:54217:2001"
	xmlns:clm5639="urn:un:unece:uncefact:codelist:specification:5639:1988"
	xmlns:clm66411="urn:un:unece:uncefact:codelist:specification:66411:2001"
	xmlns:clmIANAMIMEMediaType="urn:un:unece:uncefact:codelist:specification:IANAMIMEMediaType:2003"
	xmlns:fn="http://www.w3.org/2005/xpath-functions"
	xmlns:link="http://www.xbrl.org/2003/linkbase"
	xmlns:n1="urn:oasis:names:specification:ubl:schema:xsd:DespatchAdvice-2"
	xmlns:qdt="urn:oasis:names:specification:ubl:schema:xsd:QualifiedDatatypes-2"
	xmlns:udt="urn:un:unece:uncefact:data:specification:UnqualifiedDataTypesSchemaModule:2"
	xmlns:xbrldi="http://xbrl.org/2006/xbrldi"
	xmlns:xbrli="http://www.xbrl.org/2003/instance"
	xmlns:xdt="http://www.w3.org/2005/xpath-datatypes"
	xmlns:xlink="http://www.w3.org/1999/xlink"
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xmlns:xsd="http://www.w3.org/2001/XMLSchema"
	xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
	exclude-result-prefixes="cac cbc ccts clm54217 clm5639 clm66411 clmIANAMIMEMediaType fn link n1 qdt udt xbrldi xbrli xdt xlink xs xsd xsi">
	<xsl:character-map name="a">
		<xsl:output-character character="&#128;" string=""/>
		<xsl:output-character character="&#129;" string=""/>
		<xsl:output-character character="&#130;" string=""/>
		<xsl:output-character character="&#131;" string=""/>
		<xsl:output-character character="&#132;" string=""/>
		<xsl:output-character character="&#133;" string=""/>
		<xsl:output-character character="&#134;" string=""/>
		<xsl:output-character character="&#135;" string=""/>
		<xsl:output-character character="&#136;" string=""/>
		<xsl:output-character character="&#137;" string=""/>
		<xsl:output-character character="&#138;" string=""/>
		<xsl:output-character character="&#139;" string=""/>
		<xsl:output-character character="&#140;" string=""/>
		<xsl:output-character character="&#141;" string=""/>
		<xsl:output-character character="&#142;" string=""/>
		<xsl:output-character character="&#143;" string=""/>
		<xsl:output-character character="&#144;" string=""/>
		<xsl:output-character character="&#145;" string=""/>
		<xsl:output-character character="&#146;" string=""/>
		<xsl:output-character character="&#147;" string=""/>
		<xsl:output-character character="&#148;" string=""/>
		<xsl:output-character character="&#149;" string=""/>
		<xsl:output-character character="&#150;" string=""/>
		<xsl:output-character character="&#151;" string=""/>
		<xsl:output-character character="&#152;" string=""/>
		<xsl:output-character character="&#153;" string=""/>
		<xsl:output-character character="&#154;" string=""/>
		<xsl:output-character character="&#155;" string=""/>
		<xsl:output-character character="&#156;" string=""/>
		<xsl:output-character character="&#157;" string=""/>
		<xsl:output-character character="&#158;" string=""/>
		<xsl:output-character character="&#159;" string=""/>
	</xsl:character-map>
	<xsl:decimal-format name="european" decimal-separator="," grouping-separator="." NaN=""/>
	<xsl:output version="4.0" method="html" indent="no" encoding="UTF-8"
	doctype-public="-//W3C//DTD HTML 4.01 Transitional//EN"
	doctype-system="http://www.w3.org/TR/html4/loose.dtd" use-character-maps="a"/>
	<xsl:param name="SV_OutputFormat" select="'HTML'"/>
	<xsl:variable name="XML" select="/"/>
    <xsl:key name="groupByTaxes" match="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal" use="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode" />
    <xsl:key name="groupByTevkifat" match="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal" use="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode" />
	<xsl:variable name="CurrValue" select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentDescription=&apos;DOVIZKURU&apos;]/cbc:DocumentTypeCode"></xsl:variable>

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
		            if (this._htOption = { width: 256, height: 256, typeNumber: 4, colorDark: "#000000", colorLight: "#ffffff", correctLevel: d.H }, "string" == typeof b && (b = { text: b }), b) for (var c in b)
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
    <tr height="3px;"> 
    </tr> 
    <tr valign="top"> 
     <td style="width: 205px;"> 
      <table align="left" border="0" cellpadding="0" cellspacing="0" style="font-size: 9px; border-top-color: rgb(0, 0, 0); border-bottom-color: rgb(0, 0, 0); border-top-width: 3px; border-bottom-width: 3px; border-top-style: solid; border-bottom-style: solid;" width="350"> 
       <tbody> 
        <tr align="left"> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:DespatchAdvice"> 
              <xsl:for-each select="cac:DespatchSupplierParty "> 
               <xsl:for-each select="cac:Party">  
                <xsl:if test="cac:PartyName"> 
                 <xsl:value-of select="cac:PartyName/cbc:Name" /> 
                 <br /> 
                </xsl:if> 
                <xsl:for-each select="cac:Person"> 
                 <xsl:for-each select="cbc:Title"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:FirstName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:MiddleName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:FamilyName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:NameSuffix"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                 </xsl:for-each> 
                </xsl:for-each>  
               </xsl:for-each> 
              </xsl:for-each> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr align="left"> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:DespatchAdvice"> 
              <xsl:for-each select="cac:DespatchSupplierParty "> 
               <xsl:for-each select="cac:Party"> 
                <xsl:for-each select="cac:PostalAddress"> 
                 <xsl:for-each select="cbc:StreetName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:BuildingName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                 </xsl:for-each> 
                 <xsl:if test="cbc:BuildingNumber"> 
                  <span> 
                   <xsl:text>
                     No:
                   </xsl:text> </span> 
                  <xsl:for-each select="cbc:BuildingNumber"> 
                   <xsl:apply-templates></xsl:apply-templates> 
                  </xsl:for-each> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:if> 
                 <br /> 
                 <xsl:for-each select="cbc:PostalZone"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:CitySubdivisionName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                 </xsl:for-each> 
                 <span> 
                  <xsl:text>
                   / 
                  </xsl:text> </span> 
                 <xsl:for-each select="cbc:CityName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                </xsl:for-each> 
               </xsl:for-each> 
              </xsl:for-each> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr align="left"> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name"> 
             </xsl:value-of>  </span> </span> </span></td> 
        </tr> 
        <tr align="left"> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Tel:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:Contact/cbc:Telephone"></xsl:value-of> Faks: 
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:Contact/cbc:Telefax"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Web Sitesi:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cbc:WebsiteURI"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr align="left"> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">E-posta: 
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:Contact/cbc:ElectronicMail"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Vergi Dairesi: 
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:PartyIdentification"> 
              <xsl:variable name="schemeID" select="cbc:ID/@schemeID"></xsl:variable> 
              <xsl:if test="$schemeID ='TCKN' or $schemeID ='VKN'"> 
               <xsl:value-of select="$schemeID"></xsl:value-of> : 
               <xsl:value-of select="cbc:ID"></xsl:value-of> 
              </xsl:if> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Mersis No: 
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'MERSISNO']"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Ticaret Sicil No: 
             <xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'TICARETSICILNO']"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
       </tbody> 
      </table> </td> 
     <td style="width: 197px; text-align: center;"> <p><span style="font-size: 10px;"><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;"><img align="middle" alt="GIB Logo" src="data:image/jpeg;base64,/9j/4QAYRXhpZgAASUkqAAgAAAAAAAAAAAAAAP/sABFEdWNreQABAAQAAABkAAD/4QMZaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLwA8P3hwYWNrZXQgYmVnaW49Iu+7vyIgaWQ9Ilc1TTBNcENlaGlIenJlU3pOVGN6a2M5ZCI/PiA8eDp4bXBtZXRhIHhtbG5zOng9ImFkb2JlOm5zOm1ldGEvIiB4OnhtcHRrPSJBZG9iZSBYTVAgQ29yZSA1LjYtYzEzMiA3OS4xNTkyODQsIDIwMTYvMDQvMTktMTM6MTM6NDAgICAgICAgICI+IDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+IDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdFJlZj0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlUmVmIyIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOjZDNDJBNEI2QjVCRDExRThCQjM0REIwQkZGMEQxODY0IiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjZDNDJBNEI1QjVCRDExRThCQjM0REIwQkZGMEQxODY0IiB4bXA6Q3JlYXRvclRvb2w9IkFkb2JlIFBob3Rvc2hvcCBDUzQgV2luZG93cyI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSIzREVENkU1N0FDREVDNEJBNzkxNUM2M0NCN0RENzM0NyIgc3RSZWY6ZG9jdW1lbnRJRD0iM0RFRDZFNTdBQ0RFQzRCQTc5MTVDNjNDQjdERDczNDciLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7/7gAOQWRvYmUAZMAAAAAB/9sAhAABAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEB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" style="width: 70px;" /><br /> <font align="center">e-&#304;RSAL&#304;YE</font> </span> </span> </span></p> </td> 
     <td align="right" style="width: 242px;"><img alt="" src="data:image/png;base64,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" style="color: rgb(105, 105, 105); font-family: verdana, geneva, sans-serif; font-size: 10px; text-align: center; width: 240px; height: 92px;" />
      <div id="qrcode"></div>
      <div id="qrvalue" style="visibility: hidden; height: 30px;width: 30px; display:none">
       {"vkntckn": "<xsl:value-of select="n1:DespatchAdvice/cac:DespatchSupplierParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'TCKN' or @schemeID = 'VKN']"/>","avkntckn": "<xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'TCKN' or @schemeID = 'VKN']"/>","senaryo": "<xsl:value-of select="n1:DespatchAdvice/cbc:ProfileID"/>","tip": "<xsl:value-of select="n1:DespatchAdvice/cbc:DespatchAdviceTypeCode"/>","tarih": "<xsl:value-of select="n1:DespatchAdvice/cbc:IssueDate"/>","no": "<xsl:value-of select="n1:DespatchAdvice/cbc:ID"/>","ettn": "<xsl:value-of select="n1:DespatchAdvice/cbc:UUID"/>","sevktarihi": "<xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:Despatch/cbc:ActualDespatchDate"/>","sevkzamani": "<xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:Despatch/cbc:ActualDespatchTime"/>","tasiyicivkn": "<xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:CarrierParty/cac:PartyIdentification/cbc:ID"/>","plaka": "<xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:ShipmentStage/cac:TransportMeans/cac:RoadTransport/cbc:LicensePlateID"/>"}
      </div><script type="text/javascript">var qrcode = new QRCode(document.getElementById("qrcode"), { width: 140, height: 140, correctLevel: QRCode.CorrectLevel.L }); qrcode.makeCode(document.getElementById("qrvalue").innerHTML)</script></td> 
    </tr> 
    <tr style="height: 20px;"> 
     <td style="width: 205px;"> 
      <table align="left" border="0" cellpadding="0" cellspacing="0" id="customerPartyTable" style="border-top-color: rgb(0, 0, 0); border-bottom-color: rgb(0, 0, 0); border-top-width: 3px; border-bottom-width: 3px; border-top-style: solid; border-bottom-style: solid;" width="350"> 
       <tbody> 
        <tr> 
         <td align="left"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;">SAYIN</span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:DespatchAdvice"> 
              <xsl:for-each select="cac:DeliveryCustomerParty"> 
               <xsl:for-each select="cac:Party">  
                <xsl:if test="cac:PartyName"> 
                 <xsl:value-of select="cac:PartyName/cbc:Name" /> 
                 <br /> 
                </xsl:if> 
                <xsl:for-each select="cac:Person"> 
                 <xsl:for-each select="cbc:Title"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:FirstName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:MiddleName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:FamilyName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:NameSuffix"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                 </xsl:for-each> 
                </xsl:for-each>  
               </xsl:for-each> 
              </xsl:for-each> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:DespatchAdvice"> 
              <xsl:for-each select=" cac:DeliveryCustomerParty"> 
               <xsl:for-each select="cac:Party"> 
                <xsl:for-each select="cac:PostalAddress"> 
                 <xsl:for-each select="cbc:StreetName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:BuildingName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                 </xsl:for-each> 
                 <xsl:if test="cbc:BuildingNumber"> 
                  <span> 
                   <xsl:text>
                     No:
                   </xsl:text> </span> 
                  <xsl:for-each select="cbc:BuildingNumber"> 
                   <xsl:apply-templates></xsl:apply-templates> 
                  </xsl:for-each> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:if> 
                 <br /> 
                 <xsl:for-each select="cbc:PostalZone"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                 <xsl:for-each select="cbc:CitySubdivisionName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                 </xsl:for-each> 
                 <span> 
                  <xsl:text>
                   / 
                  </xsl:text> </span> 
                 <xsl:for-each select="cbc:CityName"> 
                  <xsl:apply-templates></xsl:apply-templates> 
                  <span> 
                   <xsl:text> 
                   </xsl:text> </span> 
                 </xsl:for-each> 
                </xsl:for-each> 
               </xsl:for-each> 
              </xsl:for-each> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:PostalAddress/cac:Country/cbc:Name"> 
             </xsl:value-of>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"><strong>Teslimat Adresi</strong>:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:DeliveryAddress/cbc:StreetName"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
             <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:DeliveryAddress/cbc:CitySubdivisionName"></xsl:value-of> 
             <xsl:text>
              /
             </xsl:text> 
             <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:DeliveryAddress/cbc:CityName"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:DeliveryAddress/cac:Country/cbc:Name"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Tel:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:Contact/cbc:Telephone"></xsl:value-of>&#160;Faks:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:Contact/cbc:Telefax"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Web Sitesi:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cbc:WebsiteURI"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">E-Posta:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:Contact/cbc:ElectronicMail"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Vergi Dairesi:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:PartyTaxScheme/cac:TaxScheme/cbc:Name"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="//n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:PartyIdentification"> 
              <xsl:variable name="schemeID" select="cbc:ID/@schemeID"></xsl:variable> 
              <xsl:if test="$schemeID ='TCKN' or $schemeID ='VKN'"> 
               <xsl:value-of select="$schemeID"></xsl:value-of> : 
               <xsl:value-of select="cbc:ID"></xsl:value-of> 
              </xsl:if> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Mersis No:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'MERSISNO']"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td align="left"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">Ticaret No:&#160;
             <xsl:value-of select="n1:DespatchAdvice/cac:DeliveryCustomerParty/cac:Party/cac:PartyIdentification/cbc:ID[@schemeID = 'TICARETSICILNO']"></xsl:value-of> </span> </span> </span></td> 
        </tr> 
       </tbody> 
      </table> </td> 
     <td style="width: 200px;">&#160;</td> 
     <td align="right" style="width: 242px;"> 
      <table style="width: 242px; border-collapse: collapse;"> 
       <tbody> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none;"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;">&#214;zelle&#351;tirme No:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 129px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cbc:CustomizationID"> 
              <xsl:apply-templates />
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none;"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;">Senaryo:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 129px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cbc:ProfileID"> 
              <xsl:apply-templates></xsl:apply-templates>
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none;"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;">&#304;rsaliye Tipi:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 129px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cbc:DespatchAdviceTypeCode"> 
              <xsl:apply-templates /> 
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none;"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;">&#304;rsaliye No:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 129px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cbc:ID"> 
              <xsl:apply-templates />
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none;"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;">&#304;rsaliye Tarihi:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 129px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cbc:IssueDate"> 
              <xsl:apply-templates select="." />
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none;"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;">&#304;rsaliye Zaman&#305;:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 129px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cbc:IssueTime"> 
              <xsl:apply-templates select="."></xsl:apply-templates>
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none;"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;">SevkTarihi:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 129px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:Despatch/cbc:ActualDespatchDate"> 
              <xsl:apply-templates select="."></xsl:apply-templates>
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none;"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;">Sevk Zaman&#305;:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 129px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cac:Shipment/cac:Delivery/cac:Despatch/cbc:ActualDespatchTime"> 
              <xsl:apply-templates select="." />
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none;"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;">Sipari&#351; No:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 129px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cac:OrderReference/cbc:ID"> 
              <xsl:apply-templates />
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
        <tr> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none;"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;">Sipari&#351; Tarihi:</span> </span> </span></td> 
         <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 129px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:for-each select="n1:DespatchAdvice/cac:OrderReference/cbc:IssueDate"> 
              <xsl:apply-templates select="." />
             </xsl:for-each>  </span> </span> </span></td> 
        </tr> 
       </tbody> 
      </table> </td> 
    </tr> 
    <tr> 
     <td style="width: 205px;"><span style="font-size: 10px;"><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif; font-weight: bold;">ETTN: </span> 
        <xsl:for-each select="n1:DespatchAdvice/cbc:UUID"> 
         <xsl:apply-templates /> 
        </xsl:for-each>  </span> </span></td> 
     <td style="width: 200px;">&#160;</td> 
     <td style="width: 242px;">&#160;</td> 
    </tr> 
   </tbody> 
  </table> 
 </body>
</html><html>
 <head></head>
 <body>
  <p>&#160;</p> 
  <table style="border: 2px solid rgb(0, 0, 0); border-image: none; width: 800px; border-collapse: collapse;"> 
   <tbody> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 3%;text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">S&#305;ra No</span></span></strong></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 10%;text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">Malzeme Kodu</span></span></strong></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 10%;text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">Malzeme A&#231;&#305;klamas&#305;</span></span></strong></span></td> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cbc:Note">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cbc:Note" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 10%;text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">Sat&#305;r A&#231;&#305;klamas&#305;</span></span></strong></span></td>
     </xsl:if> 
     <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 6%; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">Miktar</span></span></strong></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">Birim Fiyat</span></span></strong></span></td> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">&#304;skonto Oran&#305;</span></span></strong></span></td>
     </xsl:if> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">&#304;skonto Tutar&#305;</span></span></strong></span></td>
     </xsl:if> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">KDV Oran&#305;</span></span></strong></span></td>
     </xsl:if> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">KDV Tutar&#305;</span></span></strong></span></td>
     </xsl:if> 
     <xsl:if test="(//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;) or (//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal)">
      <td condition="(//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;) or (//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal)" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">Di&#287;er Vergiler</span></span></strong></span></td>
     </xsl:if> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:TaxExemptionReason">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:TaxExemptionReason" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">Vergi Muafiyet Sebebi</span></span></strong></span></td>
     </xsl:if> 
     <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cbc:OutstandingQuantity">
      <td condition="//n1:DespatchAdvice/cac:DespatchLine/cbc:OutstandingQuantity" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 6%; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">Sonra G&#246;nderilecek Miktar</span></span></strong></span></td>
     </xsl:if> 
     <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 10.6%; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">Tutar</span></span></strong></span></td> 
     <xsl:if test="//n1:DespatchAdvice/cbc:ProfileID =&apos;HKSIRSALIYE&apos;">
      <td condition="//n1:DespatchAdvice/cbc:ProfileID =&apos;HKSIRSALIYE&apos;" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 10.6%; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">K&#252;nye Numaras&#305;</span></span></strong></span></td>
     </xsl:if> 
    </tr> 
    <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine">
     <tr> 
      <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 3%; text-align: left;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="./cbc:ID"></xsl:value-of></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 10%; text-align: center;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="./cac:Item/cbc:Name"></xsl:value-of></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 6%; text-align: center;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="./cac:Item/cbc:Description "></xsl:value-of></span></span></span></td> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cbc:Note">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cbc:Note" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 6%; text-align: center;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
           <xsl:value-of select="./cbc:Note" /></span></span></span></td>
      </xsl:if> 
      <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 6%; text-align: right;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="format-number(./cbc:DeliveredQuantity, &apos;###.##0,##&apos;, &apos;european&apos;)"></xsl:value-of> 
          <xsl:if test="./cbc:DeliveredQuantity/@unitCode"> 
           <xsl:for-each select="./cbc:DeliveredQuantity"> 
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
              <xsl:text>
               &#199;UVAL
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;26&apos;"> 
              <xsl:text>
               ton
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BX&apos;"> 
              <xsl:text>
               Kutu
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;LTR&apos;"> 
              <xsl:text>
               lt
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;C62&apos;"> 
              <xsl:text>
               Adet
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NIU&apos;"> 
              <xsl:text>
               Adet
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KGM&apos;"> 
              <xsl:text>
               kg
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KJO&apos;"> 
              <xsl:text>
               kJ
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;GRM&apos;"> 
              <xsl:text>
               g
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MGM&apos;"> 
              <xsl:text>
               mg
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NT&apos;"> 
              <xsl:text>
               Net Ton
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;GT&apos;"> 
              <xsl:text>
               Gross Ton
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MTR&apos;"> 
              <xsl:text>
               m
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MMT&apos;"> 
              <xsl:text>
               mm
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KTM&apos;"> 
              <xsl:text>
               km
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MLT&apos;"> 
              <xsl:text>
               ml
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MMQ&apos;"> 
              <xsl:text>
               mm3
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CLT&apos;"> 
              <xsl:text>
               cl
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CMK&apos;"> 
              <xsl:text>
               cm2
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CMQ&apos;"> 
              <xsl:text>
               cm3
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CMT&apos;"> 
              <xsl:text>
               cm
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MTK&apos;"> 
              <xsl:text>
               m2
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MTQ&apos;"> 
              <xsl:text>
               m3
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;DAY&apos;"> 
              <xsl:text>
                G&#252;n
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;MON&apos;"> 
              <xsl:text>
                Ay
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;PA&apos;"> 
              <xsl:text>
                Paket
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;KWH&apos;"> 
              <xsl:text>
                KWH
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;ANN&apos;"> 
              <xsl:text>
                Y&#305;l
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;HUR&apos;"> 
              <xsl:text>
                Saat
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D61&apos;"> 
              <xsl:text>
                Dakika
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D62&apos;"> 
              <xsl:text>
                Saniye
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;CCT&apos;"> 
              <xsl:text>
                Ton ba&#351;.ta&#351;&#305;ma kap.
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D30&apos;"> 
              <xsl:text>
                Br&#252;t kalori
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D40&apos;"> 
              <xsl:text>
                1000 lt
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;LPA&apos;"> 
              <xsl:text>
                saf alkol lt
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;B32&apos;"> 
              <xsl:text>
                kg.m2
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;NCL&apos;"> 
              <xsl:text>
                h&#252;cre adet
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;PR&apos;"> 
              <xsl:text>
                &#199;ift
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;R9&apos;"> 
              <xsl:text>
                1000 m3
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;SET&apos;"> 
              <xsl:text>
                Set
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;T3&apos;"> 
              <xsl:text>
                1000 adet
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;E07&apos;"> 
              <xsl:text>
                MWh/h
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;EA&apos;"> 
              <xsl:text>
               Adet
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;D64&apos;"> 
              <xsl:text>
               BLOK
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;TN&apos;"> 
              <xsl:text>
               Teneke
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;5C&apos;"> 
              <xsl:text>
               Galon
              </xsl:text> 
             </xsl:when> 
             <xsl:when test="@unitCode  = &apos;BG&apos;"> 
              <xsl:text>
               Torba
              </xsl:text> 
             </xsl:when> 
            </xsl:choose> 
           </xsl:for-each> 
          </xsl:if></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: right;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
          <xsl:value-of select="format-number(./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount, &apos;###.##0,########&apos;, &apos;european&apos;)" /> 
          <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID"> 
           <xsl:text> 
           </xsl:text> 
           <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID = &quot;TRY&quot; or ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID = &quot;TRY&quot;"> 
            <xsl:text>
             TL
            </xsl:text> 
           </xsl:if> 
           <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID != &quot;TRY&quot; and ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID != &quot;TRY&quot;"> 
            <xsl:value-of select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID" /> 
           </xsl:if> 
          </xsl:if></span></span></span></td> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: right;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
           <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge">  
            <xsl:for-each select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge"> 
             <table border="0" width="100%" cellspacing="0px" cellpadding="0px" style="border-collapse:collapse;"> 
              <tbody>
               <tr> 
                <td align="right"> <span> 
                  <xsl:text>
                   %
                  </xsl:text> 
                  <xsl:value-of select="format-number(./cbc:MultiplierFactorNumeric*100, &apos;###.##0,00&apos;, &apos;european&apos;)" /> </span> </td> 
               </tr> 
              </tbody>
             </table> 
            </xsl:for-each> 
           </xsl:if></span></span></span></td>
      </xsl:if> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: right;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
           <xsl:for-each select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:AllowanceCharge"> 
            <table border="0" width="100%" cellspacing="0px" cellpadding="0px" style="border-collapse:collapse;"> 
             <tbody>
              <tr> 
               <td align="right"> <span> 
                 <xsl:text></xsl:text> 
                 <xsl:value-of select="format-number(./cbc:Amount, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                 <xsl:if test="./cbc:Amount/@currencyID"> 
                  <xsl:text></xsl:text> 
                  <xsl:if test="./cbc:Amount/@currencyID = &apos;TRY&apos;"> 
                   <xsl:text>
                    TL
                   </xsl:text> 
                  </xsl:if> 
                  <xsl:if test="./cbc:Amount/@currencyID != &apos;TRY&apos;"> 
                   <xsl:value-of select="./cbc:Amount/@currencyID"></xsl:value-of> 
                  </xsl:if> 
                 </xsl:if> </span> </td> 
              </tr> 
             </tbody>
            </table> 
           </xsl:for-each>   </span></span></span></td>
      </xsl:if> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: right;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
           <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">  
            <table border="0" width="100%" cellspacing="0px" cellpadding="0px" style="border-collapse:collapse;"> 
             <tbody> 
              <tr> 
               <td align="right"> <span> 
                 <xsl:text>&#160;
                 </xsl:text> 
                 <xsl:for-each select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme"> 
                  <xsl:if test="cbc:TaxTypeCode=&apos;0015&apos; "> 
                   <xsl:text /> 
                   <xsl:if test="../../cbc:Percent"> 
                    <xsl:text>
                      %
                    </xsl:text> 
                    <xsl:value-of select="format-number(../../cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                   </xsl:if> 
                  </xsl:if> 
                 </xsl:for-each> </span> </td> 
              </tr> 
             </tbody> 
            </table> 
           </xsl:if></span></span></span></td>
      </xsl:if> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: right;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
           <table border="0" width="100%" cellspacing="0px" cellpadding="0px" style="border-collapse:collapse;"> 
            <tbody> 
             <tr> 
              <td align="right"> <span> 
                <xsl:text>&#160;
                </xsl:text> 
                <xsl:for-each select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme"> 
                 <xsl:if test="cbc:TaxTypeCode=&apos;0015&apos; "> 
                  <xsl:text /> 
                  <xsl:value-of select="format-number(../../cbc:TaxAmount, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                  <xsl:if test="../../cbc:TaxAmount/@currencyID"> 
                   <xsl:text /> 
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
                </xsl:for-each> </span> </td> 
             </tr> 
            </tbody> 
           </table>  </span></span></span></td>
      </xsl:if> 
      <xsl:if test="(//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;) or (//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal)">
       <td condition="(//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme and //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;) or (//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal)" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: right;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
           <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal  
                    or //n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal">  
            <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme and ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;"> 
            </xsl:if>
            <xsl:for-each select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal"> 
             <xsl:if test="not(cbc:TaxAmount = 0)"> 
              <xsl:choose> 
               <xsl:when test="cac:TaxCategory/cac:TaxScheme/cbc:Name=&apos;&apos;"> 
                <xsl:text>
                 Diger Vergiler Toplami 
                </xsl:text> 
               </xsl:when> 
               <xsl:otherwise> 
                <xsl:text>
                 Tevkifat (
                </xsl:text> 
                <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode"></xsl:value-of> 
                <xsl:text>
                 -
                </xsl:text> 
                <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:value-of> 
                <xsl:text>
                 )
                </xsl:text> 
               </xsl:otherwise> 
              </xsl:choose> 
              <xsl:if test="cbc:Percent !=&apos;&apos;"> 
               <xsl:text>
                (%
               </xsl:text> 
               <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
               <xsl:text>
                )= 
               </xsl:text> 
              </xsl:if> 
              <xsl:value-of select="format-number(../cbc:TaxAmount, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
              <xsl:if test="../cbc:TaxAmount/@currencyID"> 
               <xsl:if test="../cbc:TaxAmount/@currencyID = &apos;TRY&apos;"> 
                <xsl:text>
                 TL
                </xsl:text> 
               </xsl:if> 
               <xsl:if test="../cbc:TaxAmount/@currencyID != &apos;TRY&apos;"> 
                <xsl:value-of select="../cbc:TaxAmount/@currencyID"></xsl:value-of> 
               </xsl:if> 
              </xsl:if> 
             </xsl:if> 
            </xsl:for-each>
            <table border="0" width="100%" cellspacing="0px" cellpadding="0px" style="border-collapse:collapse;"> 
             <tbody> 
              <tr> 
               <td align="left"> <span> 
                 <xsl:for-each select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cac:TaxScheme"> 
                  <xsl:if test="cbc:TaxTypeCode!=&apos;0015&apos; "> 
                   <xsl:text>
                     (
                   </xsl:text> 
                   <xsl:value-of select="cbc:Name"></xsl:value-of> 
                   <xsl:text>
                    )
                   </xsl:text> 
                   <xsl:if test="../../cbc:Percent"> 
                    <xsl:text>
                      (%
                    </xsl:text> 
                    <xsl:value-of select="format-number(../../cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                    <xsl:text>
                     )=
                    </xsl:text> 
                   </xsl:if> 
                   <xsl:value-of select="format-number(../../cbc:TaxAmount, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                   <xsl:if test="../../cbc:TaxAmount/@currencyID"> 
                    <xsl:text></xsl:text> 
                    <xsl:if test="../../cbc:TaxAmount/@currencyID = &apos;TRY&apos;"> 
                     <xsl:text>
                      TL
                     </xsl:text> 
                    </xsl:if> 
                    <xsl:if test="../../cbc:TaxAmount/@currencyID != &apos;TRY&apos;"> 
                     <xsl:value-of select="../../cbc:TaxAmount/@currencyID"></xsl:value-of> 
                    </xsl:if> 
                   </xsl:if> 
                  </xsl:if> 
                 </xsl:for-each> </span> </td> 
              </tr>   
             </tbody> 
            </table> 
           </xsl:if></span></span></span></td>
      </xsl:if> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:TaxExemptionReason">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:TaxExemptionReason" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 13%; text-align: right;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
           <table border="0" width="100%" cellspacing="0px" cellpadding="0px" style="border-collapse:collapse;"> 
            <tbody> 
             <tr> 
              <td> <span> 
                <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:TaxExemptionReason"> 
                 <xsl:value-of select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal/cac:TaxCategory/cbc:TaxExemptionReason" /> 
                </xsl:if> </span> </td> 
             </tr> 
            </tbody> 
           </table>  </span></span></span></td>
      </xsl:if> 
      <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cbc:OutstandingQuantity">
       <td condition="//n1:DespatchAdvice/cac:DespatchLine/cbc:OutstandingQuantity" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 6%; text-align: right;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
           <xsl:value-of select="format-number(./cbc:OutstandingQuantity, &apos;###.##0,####&apos;, &apos;european&apos;)"> 
           </xsl:value-of>  
           <xsl:if test="./cbc:OutstandingQuantity/@unitCode"> 
            <xsl:for-each select="./cbc:DeliveredQuantity"> 
             <xsl:text> 
             </xsl:text> 
             <xsl:choose> 
              <xsl:when test="@unitCode  = &apos;26&apos;"> 
               <xsl:text>
                ton
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;BX&apos;"> 
               <xsl:text>
                Kutu
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;LTR&apos;"> 
               <xsl:text>
                lt
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;C62&apos;"> 
               <xsl:text>
                Adet
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;NIU&apos;"> 
               <xsl:text>
                Adet
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;KGM&apos;"> 
               <xsl:text>
                kg
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;KJO&apos;"> 
               <xsl:text>
                kJ
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;GRM&apos;"> 
               <xsl:text>
                g
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MGM&apos;"> 
               <xsl:text>
                mg
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;NT&apos;"> 
               <xsl:text>
                Net Ton
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;GT&apos;"> 
               <xsl:text>
                Gross Ton
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MTR&apos;"> 
               <xsl:text>
                m
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MMT&apos;"> 
               <xsl:text>
                mm
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;KTM&apos;"> 
               <xsl:text>
                km
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MLT&apos;"> 
               <xsl:text>
                ml
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MMQ&apos;"> 
               <xsl:text>
                mm3
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;CLT&apos;"> 
               <xsl:text>
                cl
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;CMK&apos;"> 
               <xsl:text>
                cm2
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;CMQ&apos;"> 
               <xsl:text>
                cm3
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;CMT&apos;"> 
               <xsl:text>
                cm
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MTK&apos;"> 
               <xsl:text>
                m2
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MTQ&apos;"> 
               <xsl:text>
                m3
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;DAY&apos;"> 
               <xsl:text>
                 G&#252;n
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;MON&apos;"> 
               <xsl:text>
                 Ay
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;PA&apos;"> 
               <xsl:text>
                 Paket
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;KWH&apos;"> 
               <xsl:text>
                 KWH
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;ANN&apos;"> 
               <xsl:text>
                 Y&#305;l
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;HUR&apos;"> 
               <xsl:text>
                 Saat
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;D61&apos;"> 
               <xsl:text>
                 Dakika
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;D62&apos;"> 
               <xsl:text>
                 Saniye
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;CCT&apos;"> 
               <xsl:text>
                 Ton ba&#351;.ta&#351;&#305;ma kap.
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;D30&apos;"> 
               <xsl:text>
                 Br&#252;t kalori
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;D40&apos;"> 
               <xsl:text>
                 1000 lt
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;LPA&apos;"> 
               <xsl:text>
                 saf alkol lt
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;B32&apos;"> 
               <xsl:text>
                 kg.m2
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;NCL&apos;"> 
               <xsl:text>
                 h&#252;cre adet
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;PR&apos;"> 
               <xsl:text>
                 &#199;ift
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;R9&apos;"> 
               <xsl:text>
                 1000 m3
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;SET&apos;"> 
               <xsl:text>
                 Set
               </xsl:text> 
              </xsl:when> 
              <xsl:when test="@unitCode  = &apos;T3&apos;"> 
               <xsl:text>
                 1000 adet
               </xsl:text> 
              </xsl:when> 
             </xsl:choose> 
            </xsl:for-each> 
           </xsl:if> </span></span></span></td>
      </xsl:if> 
      <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 10.6%; text-align: right;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="format-number(./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount, &apos;###.##0,########&apos;, &apos;european&apos;)" /> 
          <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID"> 
           <xsl:text> 
           </xsl:text> 
           <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID = &quot;TRY&quot; or ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID = &quot;TRY&quot;"> 
            <xsl:text>
             TL
            </xsl:text> 
           </xsl:if> 
           <xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID != &quot;TRY&quot; and ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID != &quot;TRY&quot;"> 
            <xsl:value-of select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID" /> 
           </xsl:if> 
          </xsl:if> </span></span></span></td> 
      <xsl:if test="//n1:DespatchAdvice/cbc:ProfileID =&apos;HKSIRSALIYE&apos;">
       <td condition="//n1:DespatchAdvice/cbc:ProfileID =&apos;HKSIRSALIYE&apos;" style="border: 0.05em solid rgb(132, 132, 132); border-image: none; width: 10.6%; text-align: right;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
           <xsl:if test="//n1:DespatchAdvice/cbc:ProfileID =&apos;HKSIRSALIYE&apos;"> 
            <xsl:for-each select="cac:Item/cac:AdditionalItemIdentification"> 
             <xsl:if test="cbc:ID/@schemeID = &apos;KUNYENO&apos;"> 
              <xsl:value-of select="cbc:ID"></xsl:value-of> 
             </xsl:if> 
            </xsl:for-each>  
           </xsl:if></span></span></span></td>
      </xsl:if> 
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
      <table border="1" bordercolor="#a4a4a4" cellpadding="0" cellspacing="0" id="budgetContainerTable" style="width: 371px;" width="500"> 
       <tbody> 
        <tr align="right" id="budgetContainerTr"> 
         <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Toplam Tutar</span></span></span></span></td> 
         <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
             <xsl:variable name="masrafDegeri"> 
              <xsl:choose> 
               <xsl:when test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;CHARGETOTALAMOUNT&apos;]"> 
                <xsl:value-of select="(n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;CHARGETOTALAMOUNT&apos;]/cbc:ID)"></xsl:value-of> 
               </xsl:when> 
               <xsl:otherwise>
                 0 
               </xsl:otherwise> 
              </xsl:choose> 
             </xsl:variable> 
             <xsl:value-of select="format-number(n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount + $masrafDegeri, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
             <xsl:choose> 
              <xsl:when test="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID = &apos;TRY&apos;"> 
               <xsl:text>
                 TL
               </xsl:text> 
              </xsl:when> 
              <xsl:otherwise> 
               <xsl:text> 
               </xsl:text> 
               <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID"></xsl:value-of> 
              </xsl:otherwise> 
             </xsl:choose></span></span></span></td> 
        </tr> 
        <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;CHARGETOTALAMOUNT&apos;]">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Toplam &#304;skonto</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
              <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;CHARGETOTALAMOUNT&apos;]"> 
               <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                <xsl:if test="cbc:DocumentType = &apos;ALLOWANCETOTALAMOUNTLEGAL&apos;"> 
                 <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                </xsl:if> 
               </xsl:for-each> 
               <xsl:choose> 
                <xsl:when test="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID = &apos;TRY&apos;"> 
                 <xsl:text>
                   TL
                 </xsl:text> 
                </xsl:when> 
                <xsl:otherwise> 
                 <xsl:text> 
                 </xsl:text> 
                 <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID" /> 
                </xsl:otherwise> 
               </xsl:choose> 
              </xsl:if></span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><strong><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTaxes&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
                <table id="taxSubtotalHeaderTable" cellpadding="0" cellspacing="0" width="100%" tagid="taxtable"> 
                 <tbody>
                  <tr align="right" style="width: 319px;"> 
                   <td width="100%" align="right"> <span style="font-weight:bold; "> 
                     <xsl:text>
                       Hesaplanan 
                     </xsl:text> 
                     <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:value-of> 
                     <xsl:if test="cbc:Percent and cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;"> 
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
               </xsl:for-each></span></span></span></strong></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
              <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTaxes&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
               <table id="taxSubtotalsTable" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both" width="100%"> 
                <tbody>
                 <tr id="budgetContainerTr" align="right"> 
                  <td align="right"> 
                   <xsl:variable name="sumTaxAmount" select="sum(key(&apos;groupByTaxes&apos;, ./cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)/cbc:TaxAmount)"></xsl:variable> 
                   <xsl:value-of select="format-number($sumTaxAmount,&apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                   <xsl:choose> 
                    <xsl:when test="//n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID = &apos;TRY&apos;"> 
                     <xsl:text>
                       TL
                     </xsl:text> 
                    </xsl:when> 
                    <xsl:otherwise> 
                     <xsl:text> 
                     </xsl:text> 
                     <xsl:value-of select="//n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID"></xsl:value-of> 
                    </xsl:otherwise> 
                   </xsl:choose> </td> 
                 </tr> 
                </tbody>
               </table> 
              </xsl:for-each> </span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><strong><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTevkifat&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
                <table id="taxSubtotalHeaderTable" cellpadding="0" cellspacing="0" width="100%" tagid="taxtable"> 
                 <tbody>
                  <tr align="right" style="width: 319px;"> 
                   <td width="100%" align="right"> <span style="font-weight:bold; "> 
                     <xsl:text>
                      Tevkifat 
                     </xsl:text> 
                     <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name" /> 
                     <xsl:if test="cbc:Percent"> 
                      <xsl:text>
                        (%
                      </xsl:text> 
                      <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                      <xsl:text>
                       )
                      </xsl:text> 
                     </xsl:if> </span> </td> 
                  </tr> 
                 </tbody>
                </table> 
               </xsl:for-each></span></span></span></strong></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTevkifat&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
               <table id="taxSubtotalsTable" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both" width="100%"> 
                <tbody>
                 <tr id="budgetContainerTr" align="right"> 
                  <td align="right"> 
                   <xsl:variable name="sumTaxAmountTevkifat" select="sum(key(&apos;groupByTevkifat&apos;, ./cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)/cbc:TaxAmount)"></xsl:variable> 
                   <xsl:value-of select="format-number($sumTaxAmountTevkifat,&apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                   <xsl:choose> 
                    <xsl:when test="//n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID = &apos;TRY&apos;"> 
                     <xsl:text>
                       TL
                     </xsl:text> 
                    </xsl:when> 
                    <xsl:otherwise> 
                     <xsl:text> 
                     </xsl:text> 
                     <xsl:value-of select="//n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID"></xsl:value-of> 
                    </xsl:otherwise> 
                   </xsl:choose> </td> 
                 </tr> 
                </tbody>
               </table> 
              </xsl:for-each></span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;TAXINCLUSIVEAMOUNTLEGAL&apos;]">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Vergiler Dahil Toplam Tutar</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
              <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;TAXINCLUSIVEAMOUNTLEGAL&apos;]"> 
               <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                <xsl:if test="cbc:DocumentType = &apos;TAXINCLUSIVEAMOUNTLEGAL&apos;"> 
                 <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                </xsl:if> 
               </xsl:for-each> 
               <xsl:text>&#160;
               </xsl:text> 
               <xsl:choose> 
                <xsl:when test="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID = &apos;TRY&apos;"> 
                 <xsl:text>
                   TL
                 </xsl:text> 
                </xsl:when> 
                <xsl:otherwise> 
                 <xsl:text> 
                 </xsl:text> 
                 <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID" /> 
                </xsl:otherwise> 
               </xsl:choose> 
              </xsl:if></span></span></span></td> 
         </tr>
        </xsl:if> 
        <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;PAYABLEAMOUNTLEGAL&apos;]">
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">&#214;denecek Tutar</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
              <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;PAYABLEAMOUNTLEGAL&apos;]"> 
               <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                <xsl:if test="cbc:DocumentType = &apos;PAYABLEAMOUNTLEGAL&apos;"> 
                 <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                </xsl:if> 
               </xsl:for-each> 
               <xsl:text>&#160;
               </xsl:text> 
               <xsl:choose> 
                <xsl:when test="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID = &apos;TRY&apos;"> 
                 <xsl:text>
                   TL
                 </xsl:text> 
                </xsl:when> 
                <xsl:otherwise> 
                 <xsl:text> 
                 </xsl:text> 
                 <xsl:value-of select="n1:DespatchAdvice/cac:Shipment/cac:GoodsItem/cbc:ValueAmount/@currencyID" /> 
                </xsl:otherwise> 
               </xsl:choose> 
              </xsl:if></span></span></span></td> 
         </tr>
        </xsl:if> 
       </tbody> 
      </table> 
      <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]">
       <table border="1" bordercolor="#a4a4a4" cellpadding="0" cellspacing="0" id="budgetContainerTable" style="width: 371px;" width="500"> 
        <tbody> 
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">D&#246;viz Kuru</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]"> 
               <xsl:value-of select="format-number(//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentDescription=&apos;DOVIZKURU&apos;]/cbc:DocumentTypeCode, &apos;###.##0,0000&apos;, &apos;european&apos;)" /> 
               <xsl:text>&#160;
               </xsl:text> 
               <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID" /> 
              </xsl:if></span></span></span></td> 
         </tr> 
         <tr align="right" id="budgetContainerTr"> 
          <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Toplam Tutar</span></span></span></span></td> 
          <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
              <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
               <xsl:if test="cbc:DocumentType = &apos;LINEEXTENSIONAMOUNT&apos;"> 
                <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
               </xsl:if> 
              </xsl:for-each> 
              <xsl:text>&#160;
              </xsl:text> 
              <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID"></xsl:value-of> </span></span></span></td> 
         </tr> 
         <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;CHARGETOTALAMOUNT&apos;]">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Toplam &#304;skonto</span></span></span></span></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]"> 
                <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;CHARGETOTALAMOUNT&apos;]"> 
                 <span> 
                  <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                   <xsl:if test="cbc:DocumentType = &apos;ALLOWANCETOTALAMOUNT&apos;"> 
                    <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                   </xsl:if> 
                  </xsl:for-each> 
                  <xsl:text>&#160;
                  </xsl:text> 
                  <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID" /> </span> 
                </xsl:if> 
               </xsl:if></span></span></span></td> 
          </tr>
         </xsl:if> 
         <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><strong><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
                <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTaxes&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
                 <table id="taxSubtotalHeaderTable" cellpadding="0" cellspacing="0" width="100%" tagid="taxtable"> 
                  <tbody>
                   <tr align="right" style="width: 319px;"> 
                    <td width="100%" align="right"> <span style="font-weight:bold; "> <span style="font-weight:bold;"> 
                       <xsl:text>
                        Hesaplanan 
                       </xsl:text> 
                       <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name" /> 
                       <xsl:if test="cbc:Percent and cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode!=&apos;0015&apos;"> 
                        <xsl:text>
                          (%
                        </xsl:text> 
                        <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                        <xsl:text>
                         )
                        </xsl:text> 
                       </xsl:if> </span> </span></td> 
                   </tr> 
                  </tbody>
                 </table> 
                </xsl:for-each></span></span></span></strong></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]"> 
                <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:TaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTaxes&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
                 <table id="taxSubtotalsTable" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both" width="100%"> 
                  <tbody>
                   <tr id="budgetContainerTr" align="right"> 
                    <td align="right"> 
                     <xsl:variable name="sumTaxAmount" select="sum(key(&apos;groupByTaxes&apos;, ./cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)/cbc:TaxAmount)"></xsl:variable> 
                     <xsl:choose> 
                      <xsl:when test="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode=&apos;0015&apos;"> 
                       <xsl:value-of select="format-number($sumTaxAmount*$CurrValue, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                      </xsl:when> 
                      <xsl:otherwise> 
                       <xsl:variable name="var_Percent" select="cbc:Percent"></xsl:variable> 
                       <xsl:variable name="var_TaxTypeCode" select="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode"></xsl:variable> 
                       <xsl:variable name="var_TaxName" select="cac:TaxCategory/cac:TaxScheme/cbc:Name"></xsl:variable> 
                       <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                        <xsl:if test="(cbc:DocumentTypeCode = $var_TaxTypeCode) and (not(cbc:ID=0))"> 
                         <xsl:choose> 
                          <xsl:when test="(cbc:DocumentType = $var_Percent) and (cbc:DocumentDescription = $var_TaxName)"> 
                           <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                          </xsl:when> 
                          <xsl:otherwise> 
                           <xsl:if test="not($var_Percent) and not(cbc:DocumentType) and (cbc:DocumentDescription = $var_TaxName)"> 
                            <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                           </xsl:if> 
                          </xsl:otherwise> 
                         </xsl:choose> 
                        </xsl:if> 
                       </xsl:for-each> 
                      </xsl:otherwise> 
                     </xsl:choose> 
                     <xsl:text></xsl:text> 
                     <xsl:text>&#160;
                     </xsl:text> 
                     <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID"></xsl:value-of> </td> 
                   </tr> 
                  </tbody>
                 </table> 
                </xsl:for-each> 
               </xsl:if></span></span></span></td> 
          </tr>
         </xsl:if> 
         <xsl:if test="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><strong><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
                <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTevkifat&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
                 <table id="taxSubtotalHeaderTable" cellpadding="0" cellspacing="0" width="100%" tagid="taxtable"> 
                  <tbody>
                   <tr align="right" style="width: 319px;"> 
                    <td width="100%" align="right"> <span style="font-weight:bold;"> 
                      <xsl:text>
                       Tevkifat 
                      </xsl:text> 
                      <xsl:value-of select="cac:TaxCategory/cac:TaxScheme/cbc:Name" /> 
                      <xsl:if test="cbc:Percent"> 
                       <xsl:text>
                         (%
                       </xsl:text> 
                       <xsl:value-of select=" format-number(cbc:Percent, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                       <xsl:text>
                        )
                       </xsl:text> 
                      </xsl:if> </span> </td> 
                   </tr> 
                  </tbody>
                 </table> 
                </xsl:for-each></span></span></span></strong></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;"> 
               <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]"> 
                <xsl:for-each select="//n1:DespatchAdvice/cac:DespatchLine/cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:WithholdingTaxTotal/cac:TaxSubtotal[generate-id(.)=generate-id(key(&apos;groupByTevkifat&apos;, cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)[1])]"> 
                 <table id="taxSubtotalsTable" cellpadding="0" cellspacing="0" align="right" tagid="taxtable" style="clear:both" width="100%"> 
                  <tbody>
                   <tr id="budgetContainerTr" align="right"> 
                    <td align="right"> 
                     <xsl:variable name="sumTaxAmountTevkifat" select="sum(key(&apos;groupByTevkifat&apos;, ./cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode)/cbc:TaxAmount)"></xsl:variable> 
                     <xsl:text></xsl:text> 
                     <xsl:variable name="var_Percent" select="cbc:Percent"></xsl:variable> 
                     <xsl:variable name="var_TaxTypeCode" select="cac:TaxCategory/cac:TaxScheme/cbc:TaxTypeCode"></xsl:variable> 
                     <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                      <xsl:if test="cbc:DocumentTypeCode = $var_TaxTypeCode"> 
                       <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)"></xsl:value-of> 
                      </xsl:if> 
                     </xsl:for-each> 
                     <xsl:text>&#160;
                     </xsl:text> 
                     <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID"></xsl:value-of> </td> 
                   </tr> 
                  </tbody>
                 </table> 
                </xsl:for-each> 
               </xsl:if></span></span></span></td> 
          </tr>
         </xsl:if> 
         <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;TAXINCLUSIVEAMOUNT&apos;]">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">Vergiler Dahil Toplam Tutar</span></span></span></span></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]"> 
                <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;TAXINCLUSIVEAMOUNT&apos;]"> 
                 <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                  <xsl:if test="cbc:DocumentType = &apos;TAXINCLUSIVEAMOUNT&apos;"> 
                   <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                  </xsl:if> 
                 </xsl:for-each> 
                 <xsl:text>&#160;
                 </xsl:text> 
                 <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID" /> 
                </xsl:if> 
               </xsl:if></span></span></span></td> 
          </tr>
         </xsl:if> 
         <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;PAYABLEAMOUNT&apos;]">
          <tr align="right" id="budgetContainerTr"> 
           <td align="right" id="lineTableBudgetTd" style="width: 319px;" width="200"><span style="font-size: 10px;"><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;"><span style="font-weight: bold;">&#214;denecek Tutar</span></span></span></span></td> 
           <td align="right" id="lineTableBudgetTd" style="width: 120px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
               <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]"> 
                <xsl:if test="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentType=&apos;PAYABLEAMOUNT&apos;]"> 
                 <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference"> 
                  <xsl:if test="cbc:DocumentType = &apos;PAYABLEAMOUNT&apos;"> 
                   <xsl:value-of select="format-number(cbc:ID, &apos;###.##0,00&apos;, &apos;european&apos;)" /> 
                  </xsl:if> 
                 </xsl:for-each> 
                 <xsl:text>&#160;
                 </xsl:text> 
                 <xsl:value-of select="//n1:DespatchAdvice/cac:AdditionalDocumentReference[cbc:DocumentTypeCode=&apos;TARGETCURRENCY&apos;]/cbc:ID" /> 
                </xsl:if> 
               </xsl:if></span></span></span></td> 
          </tr>
         </xsl:if> 
        </tbody> 
       </table>
      </xsl:if> </td> 
    </tr> 
   </tbody> 
  </table> 
  <p>&#160;</p> 
  <table style="border: 2px solid rgb(0, 0, 0); border-image: none; width: 800px; border-collapse: collapse;"> 
   <thead> 
    <tr> 
     <th colspan="4">&#304;lgili D&#246;k&#252;manlar</th> 
    </tr> 
   </thead> 
   <tbody> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">Dok&#252;man No</span></span></strong></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">Tarih</span></span></strong></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">Dok&#252;man Tipi</span></span></strong></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; text-align: center;"><span style="font-size: 9px;"><strong><span style="color: rgb(105, 105, 105);"><span style="font-family: verdana,geneva,sans-serif;">A&#231;&#305;klama</span></span></strong></span></td> 
    </tr> 
    <xsl:for-each select="//n1:DespatchAdvice/cac:AdditionalDocumentReference">
     <tr> 
      <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; text-align: center;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="./cbc:ID" /> </span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; text-align: center;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="./cbc:IssueDate"></xsl:value-of></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; text-align: center;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="./cbc:DocumentType" /></span></span></span></td> 
      <td style="border: 0.05em solid rgb(132, 132, 132); border-image: none; text-align: center;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
          <xsl:value-of select="./cbc:DocumentDescription" /></span></span></span></td> 
     </tr>
    </xsl:for-each> 
   </tbody> 
  </table> 
  <p>&#160;</p> 
  <table style="border: 2px solid rgb(0, 0, 0); border-image: none; width: 800px; border-collapse: collapse;"> 
   <thead> 
    <tr> 
     <th colspan="2">Ta&#351;&#305;y&#305;c&#305; Bilgileri</th> 
    </tr> 
   </thead> 
   <tbody> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 372px;"><span style="font-size:10px;"><font color="#696969" face="verdana, geneva, sans-serif"><b>Ta&#351;&#305;y&#305;c&#305; Firma VKN/TCKN, Unvan:</b></font></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 419px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:for-each select="//cac:CarrierParty"> 
          <xsl:choose> 
           <xsl:when test="string-length(./cac:PartyIdentification/cbc:ID) = 11"> 
            <xsl:value-of select="./cac:PartyIdentification/cbc:ID" /> , 
            <xsl:value-of select="./cac:Person/cbc:FirstName" />&#160;
            <xsl:value-of select="./cac:Person/cbc:FamilyName" /> 
           </xsl:when> 
           <xsl:when test="string-length(./cac:PartyIdentification/cbc:ID) = 10"> 
            <xsl:value-of select="./cac:PartyIdentification/cbc:ID" /> , 
            <xsl:value-of select="./cac:PartyName/cbc:Name" /> 
           </xsl:when> 
          </xsl:choose> 
         </xsl:for-each></span></span></span></td> 
    </tr> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 372px;"><span style="font-size:9px;"><b style="color: rgb(105, 105, 105); font-family: verdana, geneva, sans-serif; font-size: x-small;">Ara&#231; Plakas&#305; Numaras&#305;:</b></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 419px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:for-each select="//cac:ShipmentStage/cac:TransportMeans/cac:RoadTransport/cbc:LicensePlateID"> 
          <xsl:value-of select="." /> 
         </xsl:for-each> </span></span></span></td> 
    </tr> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 372px;"><span style="font-size:9px;"><b style="color: rgb(105, 105, 105); font-family: verdana, geneva, sans-serif; font-size: x-small;">Dorse Plakas&#305; Numaras&#305;:</b></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 419px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:for-each select="//cac:TransportHandlingUnit/cac:TransportEquipment/cbc:ID[@schemeID = &apos;DORSEPLAKA&apos;]"> 
          <xsl:value-of select="." /> 
         </xsl:for-each> </span></span></span></td> 
    </tr> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 372px;"><span style="font-size:9px;"><b style="color: rgb(105, 105, 105); font-family: verdana, geneva, sans-serif; font-size: x-small;">&#350;of&#246;r Ad/Soyad, TCKN:</b></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 419px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:for-each select="//cac:ShipmentStage/cac:DriverPerson"> 
          <xsl:if test="cbc:FirstName"> 
           <xsl:for-each select="cbc:Title"> 
            <xsl:apply-templates /> 
            <xsl:text>&#160;
            </xsl:text> 
           </xsl:for-each> 
           <xsl:for-each select="cbc:FirstName"> 
            <xsl:apply-templates /> 
            <xsl:text>&#160;
            </xsl:text> 
           </xsl:for-each> 
           <xsl:for-each select="cbc:MiddleName"> 
            <xsl:apply-templates /> 
            <xsl:text>&#160;
            </xsl:text> 
           </xsl:for-each> 
           <xsl:for-each select="cbc:FamilyName"> 
            <xsl:apply-templates /> 
           </xsl:for-each>, 
           <xsl:for-each select="cbc:NationalityID"> 
            <xsl:apply-templates /> 
           </xsl:for-each> 
          </xsl:if> 
          <xsl:text>&#160;
          </xsl:text> 
         </xsl:for-each></span></span></span></td> 
    </tr> 
   </tbody> 
  </table> 
  <p>&#160;</p> 
  <table style="border: 2px solid rgb(0, 0, 0); border-image: none; width: 800px; border-collapse: collapse;"> 
   <thead> 
    <tr> 
     <th colspan="2">A&#231;&#305;klamalar</th> 
    </tr> 
   </thead> 
   <tbody> 
    <tr> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 373px;"><span style="font-size:9px;"><b style="color: rgb(105, 105, 105); font-family: verdana, geneva, sans-serif; font-size: x-small;">Not:</b></span></td> 
     <td style="border: 0.05em solid rgb(132, 132, 132); width: 419px;"><span style="color: rgb(105, 105, 105);"><span style="font-size: 10px;"><span style="font-family: verdana,geneva,sans-serif;">
         <xsl:for-each select="//n1:DespatchAdvice/cbc:Note"> 
          <xsl:value-of select="." /> 
          <br /> 
         </xsl:for-each> </span></span></span></td> 
    </tr> 
   </tbody> 
  </table> 
 </body>
</html></xsl:template>
<xsl:template match="//n1:DespatchAdvice/cac:DespatchLine">
		<tr class="lineTableTr">
			<td class="lineTableTd">
				<xsl:text>&#160;</xsl:text>
				<xsl:value-of select="./cbc:ID"/>
			</td>
			<td class="lineTableTd">
				<xsl:text>&#160;</xsl:text>
				<xsl:value-of select="./cac:Item/cbc:Name"/>
			</td>
			<td class="lineTableTd">
				<xsl:text>&#160;</xsl:text>
				<xsl:value-of select="./cac:Item/cbc:Description"/>
			</td>
			<td class="lineTableTd" align="right">
				<xsl:text>&#160;</xsl:text>
				<xsl:value-of
	select="format-number(./cbc:DeliveredQuantity, '###.###,####', 'european')"/>
				<xsl:if test="./cbc:DeliveredQuantity/@unitCode">
					<xsl:for-each select="./cbc:DeliveredQuantity">
						<xsl:text> </xsl:text>
						<xsl:choose>
							<xsl:when test="@unitCode  = 'SA'">
								<xsl:text>ÇUVAL</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = '26'">
								<xsl:text>ton</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'BX'">
								<xsl:text>Kutu</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'LTR'">
								<xsl:text>lt</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'C62'">
								<xsl:text>Adet</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'NIU'">
								<xsl:text>Adet</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'KGM'">
								<xsl:text>kg</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'KJO'">
								<xsl:text>kJ</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'GRM'">
								<xsl:text>g</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'MGM'">
								<xsl:text>mg</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'NT'">
								<xsl:text>Net Ton</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'GT'">
								<xsl:text>Gross Ton</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'MTR'">
								<xsl:text>m</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'MMT'">
								<xsl:text>mm</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'KTM'">
								<xsl:text>km</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'MLT'">
								<xsl:text>ml</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'MMQ'">
								<xsl:text>mm3</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'CLT'">
								<xsl:text>cl</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'CMK'">
								<xsl:text>cm2</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'CMQ'">
								<xsl:text>cm3</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'CMT'">
								<xsl:text>cm</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'MTK'">
								<xsl:text>m2</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'MTQ'">
								<xsl:text>m3</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'DAY'">
								<xsl:text> Gün</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'MON'">
								<xsl:text> Ay</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'PA'">
								<xsl:text> Paket</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'KWH'">
								<xsl:text> KWH</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'ANN'">
								<xsl:text> Yıl</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'HUR'">
								<xsl:text> Saat</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'D61'">
								<xsl:text> Dakika</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'D62'">
								<xsl:text> Saniye</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'CCT'">
								<xsl:text> Ton baş.taşıma kap.</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'D30'">
								<xsl:text> Brüt kalori</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'D40'">
								<xsl:text> 1000 lt</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'LPA'">
								<xsl:text> saf alkol lt</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'B32'">
								<xsl:text> kg.m2</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'NCL'">
								<xsl:text> hücre adet</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'PR'">
								<xsl:text> Çift</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'R9'">
								<xsl:text> 1000 m3</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'SET'">
								<xsl:text> Set</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'T3'">
								<xsl:text> 1000 adet</xsl:text>
							</xsl:when>
							<xsl:when test="@unitCode  = 'D64'">
								<xsl:text> BLOK</xsl:text>
							</xsl:when>
						</xsl:choose>
					</xsl:for-each>
				</xsl:if>
			</td>
			<td class="lineTableTd" align="right">
				<xsl:text>&#160;</xsl:text>
				<xsl:value-of
	select="format-number(./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount, '###.##0,########', 'european')"/>
				<xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID">
					<xsl:text> </xsl:text>
					<xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID = &quot;TRL&quot; or ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID = &quot;TRY&quot;">
						<xsl:text>TL</xsl:text>
					</xsl:if>
					<xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID != &quot;TRL&quot; and ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID != &quot;TRY&quot;">
						<xsl:value-of select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cac:Price/cbc:PriceAmount/@currencyID"/>
					</xsl:if>
				</xsl:if>
			</td>
			<td class="lineTableTd" align="right">
				<xsl:text>&#160;</xsl:text>
				<xsl:value-of
	select="format-number(./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount, '###.##0,########', 'european')"/>
				<xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID">
					<xsl:text> </xsl:text>
					<xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID = &quot;TRL&quot; or ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID = &quot;TRY&quot;">
						<xsl:text>TL</xsl:text>
					</xsl:if>
					<xsl:if test="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID != &quot;TRL&quot; and ./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID != &quot;TRY&quot;">
						<xsl:value-of select="./cac:Shipment/cac:GoodsItem/cac:InvoiceLine/cbc:LineExtensionAmount/@currencyID"/>
					</xsl:if>
				</xsl:if>
			</td>
		</tr>
	</xsl:template>
	<xsl:template match="//cbc:IssueDate">
		<xsl:value-of select="substring(.,9,2)"/>-<xsl:value-of select="substring(.,6,2)"/>-<xsl:value-of select="substring(.,1,4)"/>
	</xsl:template>
	<xsl:template match="//cbc:ActualDespatchDate">
		<xsl:value-of select="substring(.,9,2)"/>-<xsl:value-of select="substring(.,6,2)"/>-<xsl:value-of select="substring(.,1,4)"/>
	</xsl:template>
	<xsl:template match="//n1:DespatchAdvice">
		<tr class="lineTableTr">
			<td class="lineTableTd">
				<xsl:text>&#160;</xsl:text>
			</td>
			<td class="lineTableTd">
				<xsl:text>&#160;</xsl:text>
			</td>
			<td class="lineTableTd" align="right">
				<xsl:text>&#160;</xsl:text>
			</td>
			<td class="lineTableTd" align="right">
				<xsl:text>&#160;</xsl:text>
			</td>
			<td class="lineTableTd" align="right">
				<xsl:text>&#160;</xsl:text>
			</td>
			<td class="lineTableTd" align="right">
				<xsl:text>&#160;</xsl:text>
			</td>
		
		</tr>
	</xsl:template>
	<xsl:template name="Party_Title" >
		<xsl:param name="PartyType" />
		<td style="width:469px; " align="left">
			<xsl:if test="cac:PartyName">
				<xsl:value-of select="cac:PartyName/cbc:Name"/>
				<br/>
			</xsl:if>
			<xsl:for-each select="cac:Person">
				<xsl:for-each select="cbc:Title">
					<xsl:apply-templates/>
					<xsl:text>&#160;</xsl:text>
				</xsl:for-each>
				<xsl:for-each select="cbc:FirstName">
					<xsl:apply-templates/>
					<xsl:text>&#160;</xsl:text>
				</xsl:for-each>
				<xsl:for-each select="cbc:MiddleName">
					<xsl:apply-templates/>
					<xsl:text>&#160; </xsl:text>
				</xsl:for-each>
				<xsl:for-each select="cbc:FamilyName">
					<xsl:apply-templates/>
					<xsl:text>&#160;</xsl:text>
				</xsl:for-each>
				<xsl:for-each select="cbc:NameSuffix">
					<xsl:apply-templates/>
				</xsl:for-each>
				<xsl:if test="$PartyType='TAXFREE'">
					<br/>
					<xsl:text>Pasaport No: </xsl:text>
					<xsl:value-of select="cac:IdentityDocumentReference/cbc:ID"/>
					<br/>
					<xsl:text>Ülkesi: </xsl:text>
					<xsl:value-of select="cbc:NationalityID"/>
				</xsl:if>
			</xsl:for-each>
		</td>
	</xsl:template>
	<xsl:template name="Party_Adress" >
		<xsl:param name="PartyType" />
		<td style="width:469px; " align="left">
			<xsl:for-each select="cac:PostalAddress">
				<xsl:for-each select="cbc:StreetName">
					<xsl:apply-templates/>
					<xsl:text>&#160;</xsl:text>
				</xsl:for-each>
				<xsl:for-each select="cbc:BuildingName">
					<xsl:apply-templates/>
				</xsl:for-each>
				<xsl:for-each select="cbc:BuildingNumber">
					<xsl:text> No:</xsl:text>
					<xsl:apply-templates/>
					<xsl:text>&#160;</xsl:text>
				</xsl:for-each>
				<br/>
				<xsl:for-each select="cbc:Room">
					<xsl:text>Kapı No:</xsl:text>
					<xsl:apply-templates/>
					<xsl:text>&#160;</xsl:text>
				</xsl:for-each>
				<br/>
				<xsl:for-each select="cbc:PostalZone">
					<xsl:apply-templates/>
					<xsl:text>&#160;</xsl:text>
				</xsl:for-each>
				<xsl:for-each select="cbc:CitySubdivisionName">
					<xsl:apply-templates/>
					<xsl:text>/ </xsl:text>
				</xsl:for-each>
				<xsl:for-each select="cbc:CityName">
					<xsl:apply-templates/>
					<xsl:text>&#160;</xsl:text>
				</xsl:for-each>
				<xsl:if test="$PartyType='TAXFREE'">
					<br/>
					<xsl:value-of select="cac:Country/cbc:Name"/>
					<br/>
				</xsl:if>
			</xsl:for-each>
		</td>
	</xsl:template>
	<xsl:template name='Party_Other'>
		<xsl:param name="PartyType" />
		<xsl:for-each select="cbc:WebsiteURI">
			<tr align="left">
				<td>
					<xsl:text>Web Sitesi: </xsl:text>
					<xsl:value-of select="."/>
				</td>
			</tr>
		</xsl:for-each>
		<xsl:for-each select="cac:Contact/cbc:ElectronicMail">
			<tr align="left">
				<td>
					<xsl:text>E-Posta: </xsl:text>
					<xsl:value-of select="."/>
				</td>
			</tr>
		</xsl:for-each>
		<xsl:for-each select="cac:Contact">
			<xsl:if test="cbc:Telephone or cbc:Telefax">
				<tr align="left">
					<td style="width:469px; " align="left">
						<xsl:for-each select="cbc:Telephone">
							<xsl:text>Tel: </xsl:text>
							<xsl:apply-templates/>
						</xsl:for-each>
						<xsl:for-each select="cbc:Telefax">
							<xsl:text> Fax: </xsl:text>
							<xsl:apply-templates/>
						</xsl:for-each>
						<xsl:text>&#160;</xsl:text>
					</td>
				</tr>
			</xsl:if>
		</xsl:for-each>
		<xsl:if test="$PartyType!='TAXFREE'">
			<xsl:for-each select="cac:PartyTaxScheme/cac:TaxScheme/cbc:Name">
				<tr align="left">
					<td>
						<xsl:text>Vergi Dairesi: </xsl:text>
						<xsl:apply-templates/>
					</td>
				</tr>
			</xsl:for-each>
			<xsl:for-each select="cac:PartyIdentification">
				<tr align="left">
					<td>
						<xsl:value-of select="cbc:ID/@schemeID"/>
						<xsl:text>: </xsl:text>
						<xsl:value-of select="cbc:ID"/>
					</td>
				</tr>
			</xsl:for-each>
		</xsl:if>
	</xsl:template>
	<xsl:template name="Curr_Type">
		<xsl:value-of select="format-number(., '###.##0,00', 'european')"/>
		<xsl:if	test="@currencyID">
			<xsl:text> </xsl:text>
			<xsl:choose>
				<xsl:when test="@currencyID = 'TRL' or @currencyID = 'TRY'">
					<xsl:text>TL</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@currencyID"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>